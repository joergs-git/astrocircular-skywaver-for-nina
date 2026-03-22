using Newtonsoft.Json;
using NINA.Astrometry;
using NINA.AstroCircular.SkyWaver.Models;
using NINA.AstroCircular.SkyWaver.Utility;
using NINA.Core.Model;
using NINA.Core.Model.Equipment;
using NINA.Core.Utility;
using NINA.Equipment.Interfaces.Mediator;
using NINA.Equipment.Model;
using NINA.Image.FileFormat;
using NINA.Image.Interfaces;
using NINA.Profile.Interfaces;
using NINA.Sequencer.SequenceItem;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace NINA.AstroCircular.SkyWaver.SequenceItems {

    /// <summary>
    /// Captures exposures at N positions around a circular pattern.
    /// Uses blind SlewToRaDec (no plate-solving — telescope is defocused).
    /// </summary>
    [ExportMetadata("Name", "SKW Circular Capture")]
    [ExportMetadata("Description", "Capture defocused star images at circular ring positions")]
    [ExportMetadata("Icon", "CameraSVG")]
    [ExportMetadata("Category", "AstroCircular SKW")]
    [Export(typeof(ISequenceItem))]
    [JsonObject(MemberSerialization.OptIn)]
    public class SkwCircularCapture : SequenceItem {
        private readonly ITelescopeMediator telescopeMediator;
        private readonly ICameraMediator cameraMediator;
        private readonly IImagingMediator imagingMediator;
        private readonly IProfileService profileService;

        // ── Target Star ──

        [JsonProperty]
        public string TargetRA { get; set; } = "14:25:11.8"; // H:M:S

        [JsonProperty]
        public string TargetDec { get; set; } = "51:51:02.7"; // D:M:S

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

        // ── Sensor params for FOV calculation ──

        [JsonProperty]
        public double SensorWidthMm { get; set; } = 36.0;

        [JsonProperty]
        public double SensorHeightMm { get; set; } = 24.0;

        [JsonProperty]
        public double FocalLengthMm { get; set; } = 1946;

        /// <summary>Directory where sub-frames are saved. Set by the orchestrator.</summary>
        public string OutputDirectory { get; set; } = "";

        /// <summary>Paths of captured sub-frames, populated after execution.</summary>
        public List<string> CapturedFiles { get; } = new List<string>();

        [ImportingConstructor]
        public SkwCircularCapture(
            ITelescopeMediator telescopeMediator,
            ICameraMediator cameraMediator,
            IImagingMediator imagingMediator,
            IProfileService profileService) {
            this.telescopeMediator = telescopeMediator;
            this.cameraMediator = cameraMediator;
            this.imagingMediator = imagingMediator;
            this.profileService = profileService;
        }

        private SkwCircularCapture(SkwCircularCapture cloneMe) : this(
            cloneMe.telescopeMediator,
            cloneMe.cameraMediator,
            cloneMe.imagingMediator,
            cloneMe.profileService) {
            CopyMetaData(cloneMe);
        }

        public override object Clone() {
            return new SkwCircularCapture(this) {
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
                FocalLengthMm = FocalLengthMm
            };
        }

        public override async Task Execute(IProgress<ApplicationStatus> progress, CancellationToken ct) {
            // Validate equipment connections
            var teleInfo = telescopeMediator.GetInfo();
            if (!teleInfo.Connected) {
                throw new SequenceEntityFailedException("Mount is not connected. Connect the mount before running SKW collimation.");
            }
            var camInfo = cameraMediator.GetInfo();
            if (!camInfo.Connected) {
                throw new SequenceEntityFailedException("Camera is not connected. Connect the camera before running SKW collimation.");
            }

            // Ensure output directory exists
            if (string.IsNullOrEmpty(OutputDirectory)) {
                OutputDirectory = Path.Combine(Path.GetTempPath(), "NINA", "AstroCircular_SKW",
                    DateTime.Now.ToString("yyyyMMdd_HHmmss"));
            }
            Directory.CreateDirectory(OutputDirectory);

            // Compute FOV and ring positions
            double centerRA = CoordinateUtils.ParseHMS(TargetRA);
            double centerDec = CoordinateUtils.ParseDMS(TargetDec);
            var (fovW, fovH) = CircularPatternCalculator.ComputeFOV(SensorWidthMm, SensorHeightMm, FocalLengthMm);

            var positions = CircularPatternCalculator.Calculate(
                centerRA, centerDec, fovW, fovH,
                RingPositions, RadiusPercent, UseCircle, IncludeCenter);

            CapturedFiles.Clear();
            int captured = 0;
            int total = positions.Count;

            for (int i = 0; i < positions.Count; i++) {
                ct.ThrowIfCancellationRequested();

                var pos = positions[i];
                string posLabel = pos.Label.Replace(" ", "");
                progress?.Report(new ApplicationStatus {
                    Status = $"SKW: Slewing to {pos.Label} ({i + 1}/{total}) — RA {CoordinateUtils.ToHMS(pos.RAHours)} Dec {CoordinateUtils.ToDMS(pos.DecDegrees)}"
                });

                // Blind slew — no plate-solving (telescope is defocused)
                try {
                    var coords = new Coordinates(
                        Angle.ByHours(pos.RAHours),
                        Angle.ByDegree(pos.DecDegrees),
                        Epoch.J2000);
                    await telescopeMediator.SlewToCoordinatesAsync(coords, ct);
                } catch (Exception ex) {
                    // Skip this position on slew failure, continue with remaining
                    Logger.Warning($"SKW: Slew to {pos.Label} failed, skipping: {ex.Message}");
                    continue;
                }

                // Settle time — let the mount stabilize
                if (SettleSeconds > 0) {
                    progress?.Report(new ApplicationStatus {
                        Status = $"SKW: Settling {SettleSeconds}s at {pos.Label}"
                    });
                    await Task.Delay(TimeSpan.FromSeconds(SettleSeconds), ct);
                }

                // Capture exposure
                progress?.Report(new ApplicationStatus {
                    Status = $"SKW: Exposing {ExposureTime}s at {pos.Label} ({i + 1}/{total})"
                });

                try {
                    string savedPath = await CaptureAndSaveFrame(posLabel, i, progress, ct);
                    if (savedPath != null) {
                        CapturedFiles.Add(savedPath);
                        captured++;
                        progress?.Report(new ApplicationStatus {
                            Status = $"SKW: Saved {pos.Label} ({captured}/{total})"
                        });
                    }
                } catch (Exception ex) {
                    // Retry once on capture failure
                    Logger.Warning($"SKW: Capture at {pos.Label} failed, retrying: {ex.Message}");
                    try {
                        string savedPath = await CaptureAndSaveFrame(posLabel, i, progress, ct);
                        if (savedPath != null) {
                            CapturedFiles.Add(savedPath);
                            captured++;
                        }
                    } catch (Exception retryEx) {
                        Logger.Warning($"SKW: Retry at {pos.Label} also failed, skipping: {retryEx.Message}");
                    }
                }
            }

            if (captured < 3) {
                throw new SequenceEntityFailedException(
                    $"SKW: Only {captured} frames captured out of {total}. Need at least 3 for integration.");
            }

            progress?.Report(new ApplicationStatus {
                Status = $"SKW: Circular capture complete — {captured}/{total} frames in {OutputDirectory}"
            });
        }

        /// <summary>
        /// Capture a single exposure and save it as FITS to the output directory.
        /// Uses NINA's standard capture flow: CaptureImage → ToImageData → SaveToDisk.
        /// </summary>
        private async Task<string> CaptureAndSaveFrame(string posLabel, int index,
            IProgress<ApplicationStatus> progress, CancellationToken ct) {

            var captureSequence = new CaptureSequence(
                ExposureTime,
                CaptureSequence.ImageTypes.LIGHT,
                new FilterInfo(FilterName, 0, (short)0),
                new BinningMode((short)Binning, (short)Binning),
                1) {
                Gain = Gain,
                Offset = Offset
            };

            var exposureData = await imagingMediator.CaptureImage(captureSequence, ct, progress);
            if (exposureData == null) return null;

            // Convert exposure data to image data for saving
            var imageData = await exposureData.ToImageData(progress, ct);
            if (imageData == null) return null;

            // Save as FITS to our output directory
            var fileSaveInfo = new FileSaveInfo(profileService) {
                FilePath = OutputDirectory,
                FilePattern = $"SKW_{posLabel}_{index:D2}",
                FileType = NINA.Core.Enum.FileTypeEnum.FITS
            };

            string savedPath = await imageData.SaveToDisk(fileSaveInfo, ct);
            return savedPath;
        }

        public override string ToString() {
            return $"Category: AstroCircular SKW, Item: SkwCircularCapture, Positions: {RingPositions}, Radius: {RadiusPercent}%";
        }
    }
}
