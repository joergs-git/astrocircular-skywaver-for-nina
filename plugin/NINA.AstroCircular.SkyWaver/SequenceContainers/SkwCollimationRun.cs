using Newtonsoft.Json;
using NINA.Astrometry;
using NINA.AstroCircular.SkyWaver.Models;
using NINA.AstroCircular.SkyWaver.SequenceItems;
using NINA.AstroCircular.SkyWaver.Utility;
using NINA.Core.Model;
using NINA.Core.Model.Equipment;
using NINA.Core.Utility;
using NINA.Equipment.Interfaces.Mediator;
using NINA.Image.Interfaces;
using NINA.Profile.Interfaces;
using NINA.Sequencer.SequenceItem;
using System;
using System.ComponentModel.Composition;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace NINA.AstroCircular.SkyWaver.SequenceContainers {

    /// <summary>
    /// Orchestrates the full SKW collimation workflow:
    /// 1. Switch filter
    /// 2. Plate-solve and center on target star (IN FOCUS)
    /// 3. Defocus
    /// 4. Circular capture (blind SlewToRaDec — no plate-solve)
    /// 5. Integrate sub-frames
    /// 6. Refocus (ALWAYS runs, even on failure)
    /// </summary>
    [ExportMetadata("Name", "SKW Collimation Run")]
    [ExportMetadata("Description", "Complete SkyWave collimation: center, defocus, circular capture, integrate, refocus")]
    [ExportMetadata("Icon", "TelescopeSVG")]
    [ExportMetadata("Category", "SkyWave Collimation")]
    [Export(typeof(ISequenceItem))]
    [JsonObject(MemberSerialization.OptIn)]
    public class SkwCollimationRun : SequenceItem {
        private readonly IFocuserMediator focuserMediator;
        private readonly ITelescopeMediator telescopeMediator;
        private readonly ICameraMediator cameraMediator;
        private readonly IImagingMediator imagingMediator;
        private readonly IFilterWheelMediator filterWheelMediator;
        private readonly IProfileService profileService;
        private readonly IImageDataFactory imageDataFactory;

        // ── Target ──

        [JsonProperty]
        public string StarName { get; set; } = "θ Boo";

        [JsonProperty]
        public string TargetRA { get; set; } = "14:25:11.8";

        [JsonProperty]
        public string TargetDec { get; set; } = "51:51:02.7";

        // ── Defocus ──

        [JsonProperty]
        public int DefocusSteps { get; set; } = 2442;

        [JsonProperty]
        public int DefocusDirection { get; set; } = 1; // +1 extra-focal, -1 intra-focal

        // ── Pattern ──

        [JsonProperty]
        public int RingPositions { get; set; } = 8;

        [JsonProperty]
        public int RadiusPercent { get; set; } = 80;

        [JsonProperty]
        public bool IncludeCenter { get; set; } = true;

        [JsonProperty]
        public bool UseCircle { get; set; } = true;

        // ── Imaging ──

        [JsonProperty]
        public double ExposureTime { get; set; } = 8;

        [JsonProperty]
        public string FilterName { get; set; } = "R";

        [JsonProperty]
        public int Gain { get; set; } = 100;

        [JsonProperty]
        public int Offset { get; set; } = 0;

        [JsonProperty]
        public int Binning { get; set; } = 1;

        [JsonProperty]
        public int SettleSeconds { get; set; } = 3;

        // ── Integration ──

        [JsonProperty]
        public bool CropToCircle { get; set; } = true;

        [JsonProperty]
        public bool BinToHalf { get; set; } = true;

        [JsonProperty]
        public bool AutoCleanSubFrames { get; set; } = true;

        // ── Telescope/Sensor (from settings) ──

        [JsonProperty]
        public double FocalLengthMm { get; set; } = 1946;

        [JsonProperty]
        public double ApertureMm { get; set; } = 304.8;

        [JsonProperty]
        public double SensorWidthMm { get; set; } = 36.0;

        [JsonProperty]
        public double SensorHeightMm { get; set; } = 24.0;

        [JsonProperty]
        public double PixelSizeUm { get; set; } = 3.76;

        // ── Output ──

        [JsonProperty]
        public string SkyWaveOutputDirectory { get; set; } = "";

        [ImportingConstructor]
        public SkwCollimationRun(
            IFocuserMediator focuserMediator,
            ITelescopeMediator telescopeMediator,
            ICameraMediator cameraMediator,
            IImagingMediator imagingMediator,
            IFilterWheelMediator filterWheelMediator,
            IProfileService profileService,
            IImageDataFactory imageDataFactory) {
            this.focuserMediator = focuserMediator;
            this.telescopeMediator = telescopeMediator;
            this.cameraMediator = cameraMediator;
            this.imagingMediator = imagingMediator;
            this.filterWheelMediator = filterWheelMediator;
            this.profileService = profileService;
            this.imageDataFactory = imageDataFactory;
        }

        private SkwCollimationRun(SkwCollimationRun cloneMe) : this(
            cloneMe.focuserMediator,
            cloneMe.telescopeMediator,
            cloneMe.cameraMediator,
            cloneMe.imagingMediator,
            cloneMe.filterWheelMediator,
            cloneMe.profileService,
            cloneMe.imageDataFactory) {
            CopyMetaData(cloneMe);
        }

        public override object Clone() {
            return new SkwCollimationRun(this) {
                StarName = StarName,
                TargetRA = TargetRA,
                TargetDec = TargetDec,
                DefocusSteps = DefocusSteps,
                DefocusDirection = DefocusDirection,
                RingPositions = RingPositions,
                RadiusPercent = RadiusPercent,
                IncludeCenter = IncludeCenter,
                UseCircle = UseCircle,
                ExposureTime = ExposureTime,
                FilterName = FilterName,
                Gain = Gain,
                Offset = Offset,
                Binning = Binning,
                SettleSeconds = SettleSeconds,
                CropToCircle = CropToCircle,
                BinToHalf = BinToHalf,
                AutoCleanSubFrames = AutoCleanSubFrames,
                FocalLengthMm = FocalLengthMm,
                ApertureMm = ApertureMm,
                SensorWidthMm = SensorWidthMm,
                SensorHeightMm = SensorHeightMm,
                PixelSizeUm = PixelSizeUm,
                SkyWaveOutputDirectory = SkyWaveOutputDirectory
            };
        }

        public override async Task Execute(IProgress<ApplicationStatus> progress, CancellationToken ct) {
            // ── Pre-flight checks ──
            if (!telescopeMediator.GetInfo().Connected)
                throw new SequenceEntityFailedException("Mount is not connected.");
            if (!cameraMediator.GetInfo().Connected)
                throw new SequenceEntityFailedException("Camera is not connected.");
            if (!focuserMediator.GetInfo().Connected)
                throw new SequenceEntityFailedException("Focuser is not connected.");

            int originalFocuserPosition = focuserMediator.GetInfo().Position;
            int relativeDefocus = DefocusSteps * DefocusDirection;
            string tempDir = Path.Combine(Path.GetTempPath(), "NINA", "AstroCircular_SKW",
                DateTime.Now.ToString("yyyyMMdd_HHmmss"));

            try {
                // ── Step 1: Switch filter ──
                progress?.Report(new ApplicationStatus { Status = $"SKW: Switching to filter {FilterName}" });
                await filterWheelMediator.ChangeFilter(new FilterInfo(FilterName, 0, (short)0), ct);

                // ── Step 2: Plate-solve and center on target star (IN FOCUS) ──
                progress?.Report(new ApplicationStatus {
                    Status = $"SKW: Slewing to {StarName} ({TargetRA} {TargetDec}) and centering..."
                });
                var centerCoords = new Coordinates(
                    Angle.ByHours(CoordinateUtils.ParseHMS(TargetRA)),
                    Angle.ByDegree(CoordinateUtils.ParseDMS(TargetDec)),
                    Epoch.J2000);
                await telescopeMediator.SlewToCoordinatesAsync(centerCoords, ct);

                // Note: Full plate-solve centering would use NINA's built-in Center instruction.
                // For now we do a blind slew. The orchestrator can be enhanced to use
                // IPlateSolverFactory for precise centering in a future version.

                // ── Step 3: Defocus ──
                progress?.Report(new ApplicationStatus {
                    Status = $"SKW: Defocusing {(relativeDefocus > 0 ? "+" : "")}{relativeDefocus} steps"
                });
                await focuserMediator.MoveFocuserRelative(relativeDefocus, ct);

                // ── Step 4: Circular capture ──
                var capture = new SkwCircularCapture(
                    telescopeMediator, cameraMediator, imagingMediator,
                    profileService) {
                    TargetRA = TargetRA,
                    TargetDec = TargetDec,
                    RingPositions = RingPositions,
                    RadiusPercent = RadiusPercent,
                    IncludeCenter = IncludeCenter,
                    UseCircle = UseCircle,
                    ExposureTime = ExposureTime,
                    FilterName = FilterName,
                    Gain = Gain,
                    Offset = Offset,
                    Binning = Binning,
                    SettleSeconds = SettleSeconds,
                    SensorWidthMm = SensorWidthMm,
                    SensorHeightMm = SensorHeightMm,
                    FocalLengthMm = FocalLengthMm,
                    OutputDirectory = tempDir
                };
                await capture.Execute(progress, ct);

                // ── Step 5: Integrate sub-frames ──
                string outputDir = !string.IsNullOrEmpty(SkyWaveOutputDirectory)
                    ? SkyWaveOutputDirectory : tempDir;
                string outputFileName = $"SKW_Collimation_{DateTime.Now:yyyyMMdd_HHmmss}.fits";

                var integrate = new SkwIntegrateFrames(imageDataFactory) {
                    InputDirectory = tempDir,
                    InputFiles = capture.CapturedFiles,
                    OutputDirectory = outputDir,
                    OutputFileName = outputFileName,
                    CropToCircle = CropToCircle,
                    BinToHalf = BinToHalf,
                    FocalLengthMm = FocalLengthMm,
                    PixelSizeUm = PixelSizeUm,
                    CaptureBinning = Binning,
                    ExposureSeconds = ExposureTime,
                    FilterName = FilterName,
                    AutoCleanSubFrames = AutoCleanSubFrames
                };
                await integrate.Execute(progress, ct);

                progress?.Report(new ApplicationStatus {
                    Status = $"SKW: Collimation run complete — output in {outputDir}"
                });

            } finally {
                // ── Step 6: ALWAYS refocus, even on failure ──
                try {
                    progress?.Report(new ApplicationStatus {
                        Status = $"SKW: Returning focuser to position {originalFocuserPosition}"
                    });
                    await focuserMediator.MoveFocuserRelative(-relativeDefocus, ct);
                } catch (Exception refocusEx) {
                    Logger.Error($"SKW: Failed to refocus! Original position was {originalFocuserPosition}. Error: {refocusEx.Message}");
                }
            }
        }

        public override string ToString() {
            return $"Category: SkyWave Collimation, Item: SkwCollimationRun, Star: {StarName}, Positions: {RingPositions}";
        }
    }
}
