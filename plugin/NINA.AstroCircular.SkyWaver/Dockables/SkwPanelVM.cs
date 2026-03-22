using NINA.Astrometry;
using NINA.AstroCircular.SkyWaver.Imaging;
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
using NINA.WPF.Base.ViewModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace NINA.AstroCircular.SkyWaver.Dockables {

    [Export(typeof(NINA.Equipment.Interfaces.ViewModel.IDockableVM))]
    public class SkwPanelVM : DockableVM {
        private readonly ITelescopeMediator telescopeMediator;
        private readonly ICameraMediator cameraMediator;
        private readonly IFocuserMediator focuserMediator;
        private readonly IFilterWheelMediator filterWheelMediator;
        private readonly IImagingMediator imagingMediator;
        private readonly IImageDataFactory imageDataFactory;

        private CancellationTokenSource runCts;

        [ImportingConstructor]
        public SkwPanelVM(
            IProfileService profileService,
            ITelescopeMediator telescopeMediator,
            ICameraMediator cameraMediator,
            IFocuserMediator focuserMediator,
            IFilterWheelMediator filterWheelMediator,
            IImagingMediator imagingMediator,
            IImageDataFactory imageDataFactory
        ) : base(profileService) {
            this.telescopeMediator = telescopeMediator;
            this.cameraMediator = cameraMediator;
            this.focuserMediator = focuserMediator;
            this.filterWheelMediator = filterWheelMediator;
            this.imagingMediator = imagingMediator;
            this.imageDataFactory = imageDataFactory;

            Title = "SKW Collimation";

            // Icon
            try {
                var dict = new ResourceDictionary();
                dict.Source = new Uri("NINA.AstroCircular.SkyWaver;component/Resources/Icons.xaml", UriKind.RelativeOrAbsolute);
                ImageGeometry = (GeometryGroup)dict["SkwPanelIconSVG"];
                ImageGeometry.Freeze();
            } catch {
                // Default puzzle piece icon if icon load fails
            }

            // Commands
            RunCommand = new AsyncCommand<bool>(RunCollimation, (o) => !IsRunning);
            CancelCommand = new RelayCommand((o) => Cancel());
            BrowseFolderCommand = new RelayCommand((o) => BrowseFolder());
            FindBestStarCommand = new RelayCommand((o) => FindBestStar());

            // Load settings
            LoadSettings();

            // Try to populate from NINA profile
            LoadFromProfile();

            // Build initial map
            try { BuildMapPositions(); } catch { }
        }

        public override bool IsTool => true;

        // ── Commands ──

        public ICommand RunCommand { get; }
        public ICommand CancelCommand { get; }
        public ICommand BrowseFolderCommand { get; }
        public ICommand FindBestStarCommand { get; }

        // ── State ──

        private bool isRunning;
        public bool IsRunning {
            get => isRunning;
            set { isRunning = value; RaisePropertyChanged(); }
        }

        private string statusText = "Ready";
        public string StatusText {
            get => statusText;
            set { statusText = value; RaisePropertyChanged(); }
        }

        private int progress;
        public int Progress {
            get => progress;
            set { progress = value; RaisePropertyChanged(); }
        }

        // ── Star Selection ──

        private string starName = "theta Boo";
        public string StarName {
            get => starName;
            set { starName = value; RaisePropertyChanged(); SaveSettings(); }
        }

        private string targetRA = "14:25:11.8";
        public string TargetRA {
            get => targetRA;
            set { targetRA = value; RaisePropertyChanged(); SaveSettings(); RebuildMap(); }
        }

        private string targetDec = "51:51:02.7";
        public string TargetDec {
            get => targetDec;
            set { targetDec = value; RaisePropertyChanged(); SaveSettings(); RebuildMap(); }
        }

        // ── Defocus ──

        private int defocusSteps = 2442;
        public int DefocusSteps {
            get => defocusSteps;
            set { defocusSteps = value; RaisePropertyChanged(); SaveSettings(); }
        }

        private int defocusDirection = 1;
        public int DefocusDirection {
            get => defocusDirection;
            set { defocusDirection = value; RaisePropertyChanged(); SaveSettings(); }
        }

        // ── Imaging ──

        private double exposureTime = 8;
        public double ExposureTime {
            get => exposureTime;
            set { exposureTime = value; RaisePropertyChanged(); SaveSettings(); }
        }

        private string filterName = "R";
        public string FilterName {
            get => filterName;
            set { filterName = value; RaisePropertyChanged(); SaveSettings(); }
        }

        private int gain = 100;
        public int Gain {
            get => gain;
            set { gain = value; RaisePropertyChanged(); SaveSettings(); }
        }

        private int offset = 0;
        public int Offset {
            get => offset;
            set { offset = value; RaisePropertyChanged(); SaveSettings(); }
        }

        private int binning = 1;
        public int Binning {
            get => binning;
            set { binning = value; RaisePropertyChanged(); SaveSettings(); }
        }

        // ── Pattern ──

        private int ringPositions = 8;
        public int RingPositions {
            get => ringPositions;
            set { ringPositions = value; RaisePropertyChanged(); SaveSettings(); RebuildMap(); }
        }

        private int radiusPercent = 80;
        public int RadiusPercent {
            get => radiusPercent;
            set { radiusPercent = value; RaisePropertyChanged(); SaveSettings(); RebuildMap(); }
        }

        private int settleSeconds = 3;
        public int SettleSeconds {
            get => settleSeconds;
            set { settleSeconds = value; RaisePropertyChanged(); SaveSettings(); }
        }

        private bool includeCenter = true;
        public bool IncludeCenter {
            get => includeCenter;
            set { includeCenter = value; RaisePropertyChanged(); SaveSettings(); RebuildMap(); }
        }

        // ── Integration ──

        private bool cropToCircle = true;
        public bool CropToCircle {
            get => cropToCircle;
            set { cropToCircle = value; RaisePropertyChanged(); SaveSettings(); }
        }

        private bool binToHalf = true;
        public bool BinToHalf {
            get => binToHalf;
            set { binToHalf = value; RaisePropertyChanged(); SaveSettings(); }
        }

        private bool autoCleanSubFrames = true;
        public bool AutoCleanSubFrames {
            get => autoCleanSubFrames;
            set { autoCleanSubFrames = value; RaisePropertyChanged(); SaveSettings(); }
        }

        // ── Output ──

        private string skyWaveOutputDirectory = "";
        public string SkyWaveOutputDirectory {
            get => skyWaveOutputDirectory;
            set { skyWaveOutputDirectory = value; RaisePropertyChanged(); SaveSettings(); }
        }

        // ── Sensor Map ──

        public ObservableCollection<MapPosition> MapPositions { get; } = new ObservableCollection<MapPosition>();

        private string progressText = "";
        public string ProgressText {
            get => progressText;
            set { progressText = value; RaisePropertyChanged(); }
        }

        // Map canvas dimensions
        public double MapWidth => 280;
        public double MapHeight => 200;

        private double ringCanvasRadius;
        public double RingCanvasRadius {
            get => ringCanvasRadius;
            set { ringCanvasRadius = value; RaisePropertyChanged(); }
        }
        public double RingCanvasCenterX => MapWidth / 2;
        public double RingCanvasCenterY => MapHeight / 2;

        /// <summary>
        /// Build the map positions for visualization based on current settings.
        /// Called when settings change or before a run starts.
        /// </summary>
        private void BuildMapPositions() {
            MapPositions.Clear();

            double sensorW = 36.0, sensorH = 24.0;
            try {
                sensorW = profileService?.ActiveProfile?.CameraSettings?.PixelSize > 0
                    ? cameraMediator.GetInfo().XSize * profileService.ActiveProfile.CameraSettings.PixelSize / 1000.0 : 36.0;
                sensorH = profileService?.ActiveProfile?.CameraSettings?.PixelSize > 0
                    ? cameraMediator.GetInfo().YSize * profileService.ActiveProfile.CameraSettings.PixelSize / 1000.0 : 24.0;
            } catch { }

            double fl = 1946;
            try { fl = profileService?.ActiveProfile?.TelescopeSettings?.FocalLength ?? 1946; if (fl <= 0) fl = 1946; } catch { }

            var (fovW, fovH) = CircularPatternCalculator.ComputeFOV(sensorW, sensorH, fl);
            double centerRA = CoordinateUtils.ParseHMS(TargetRA);
            double centerDec = CoordinateUtils.ParseDMS(TargetDec);

            var positions = CircularPatternCalculator.Calculate(
                centerRA, centerDec, fovW, fovH,
                RingPositions, RadiusPercent, true, IncludeCenter);

            // Map sky positions to canvas coordinates
            double pad = 14;
            double drawW = MapWidth - 2 * pad;
            double drawH = MapHeight - 2 * pad;
            double cx = MapWidth / 2;
            double cy = MapHeight / 2;
            double cosDec = Math.Cos(centerDec * Math.PI / 180.0);

            // Ring radius on canvas
            double minFov = Math.Min(fovW, fovH);
            double ringDeg = (RadiusPercent / 100.0) * (minFov / 2.0);
            RingCanvasRadius = (ringDeg / fovW) * drawW;

            foreach (var pos in positions) {
                double dRaDeg = (pos.RAHours - centerRA) * 15.0 * cosDec;
                double dDecDeg = pos.DecDegrees - centerDec;
                double canvasX = cx + (dRaDeg / fovW) * drawW;
                double canvasY = cy - (dDecDeg / fovH) * drawH;

                MapPositions.Add(new MapPosition {
                    Label = pos.Label,
                    CanvasX = canvasX,
                    CanvasY = canvasY,
                    IsCenter = pos.Label == "Center",
                    State = PositionState.Pending
                });
            }

            ProgressText = $"0 / {positions.Count} positions";
        }

        private void RebuildMap() {
            if (!IsRunning) {
                try { BuildMapPositions(); } catch { }
            }
        }

        // ── Last Captured Image Preview ──

        private System.Windows.Media.Imaging.BitmapSource lastCapturedImage;
        public System.Windows.Media.Imaging.BitmapSource LastCapturedImage {
            get => lastCapturedImage;
            set { lastCapturedImage = value; RaisePropertyChanged(); }
        }

        /// <summary>
        /// Create a quick thumbnail from image data for the preview panel.
        /// </summary>
        private void UpdatePreviewImage(NINA.Image.Interfaces.IImageData imageData) {
            try {
                if (imageData == null) return;
                var pixels = imageData.Data.FlatArray;
                int w = imageData.Properties.Width;
                int h = imageData.Properties.Height;

                // Downsample to thumbnail (max 200px wide)
                int scale = Math.Max(1, w / 200);
                int tw = w / scale;
                int th = h / scale;

                // Find min/max for auto-stretch
                ushort min = ushort.MaxValue, max = 0;
                for (int i = 0; i < pixels.Length; i += scale * 10) {
                    if (pixels[i] < min) min = pixels[i];
                    if (pixels[i] > max) max = pixels[i];
                }
                double range = Math.Max(1, max - min);

                // Create 8-bit grayscale bitmap
                byte[] bmpData = new byte[tw * th];
                for (int y = 0; y < th; y++) {
                    for (int x = 0; x < tw; x++) {
                        int srcIdx = (y * scale) * w + (x * scale);
                        if (srcIdx < pixels.Length) {
                            bmpData[y * tw + x] = (byte)(255.0 * (pixels[srcIdx] - min) / range);
                        }
                    }
                }

                var bmp = System.Windows.Media.Imaging.BitmapSource.Create(
                    tw, th, 96, 96,
                    System.Windows.Media.PixelFormats.Gray8, null,
                    bmpData, tw);
                bmp.Freeze();

                System.Windows.Application.Current?.Dispatcher?.Invoke(() => {
                    LastCapturedImage = bmp;
                });
            } catch { }
        }

        // ── Star Presets (for ComboBox) ──

        public List<StarPreset> StarPresets => StarCatalog.Presets;

        private StarPreset selectedPreset;
        public StarPreset SelectedPreset {
            get => selectedPreset;
            set {
                selectedPreset = value;
                if (value != null) {
                    StarName = value.Name;
                    TargetRA = value.RA;
                    TargetDec = value.Dec;
                }
                RaisePropertyChanged();
            }
        }

        // ── Folder Browser ──

        private void BrowseFolder() {
            using (var dialog = new System.Windows.Forms.FolderBrowserDialog()) {
                dialog.Description = "Select SkyWave watch folder";
                if (!string.IsNullOrEmpty(SkyWaveOutputDirectory)) {
                    dialog.SelectedPath = SkyWaveOutputDirectory;
                }
                if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK) {
                    SkyWaveOutputDirectory = dialog.SelectedPath;
                }
            }
        }

        // ── Find Best Star ──

        private void FindBestStar() {
            try {
                double lat = profileService?.ActiveProfile?.AstrometrySettings?.Latitude ?? 52.17;
                double lon = profileService?.ActiveProfile?.AstrometrySettings?.Longitude ?? 7.25;
                var result = StarCatalog.FindBestStar(DateTime.UtcNow, lat, lon);
                if (result.HasValue) {
                    SelectedPreset = result.Value.Star;
                    StatusText = $"Best star: {result.Value.Star.Name} (alt {result.Value.AltitudeDeg:F0} deg)";
                } else {
                    StatusText = "No suitable star within 3h of meridian";
                }
            } catch (Exception ex) {
                StatusText = $"Star finder error: {ex.Message}";
            }
        }

        // ── Cancel ──

        private void Cancel() {
            runCts?.Cancel();
            StatusText = "Cancelling...";
        }

        // ══════════════════════════════════════════════════════════
        // MAIN WORKFLOW — Run Collimation
        // ══════════════════════════════════════════════════════════

        private async Task<bool> RunCollimation() {
            if (IsRunning) return false;

            // Pre-flight checks
            if (!telescopeMediator.GetInfo().Connected) {
                StatusText = "Error: Mount not connected";
                return false;
            }
            if (!cameraMediator.GetInfo().Connected) {
                StatusText = "Error: Camera not connected";
                return false;
            }
            if (!focuserMediator.GetInfo().Connected) {
                StatusText = "Error: Focuser not connected";
                return false;
            }

            IsRunning = true;
            Progress = 0;
            runCts = new CancellationTokenSource();
            var ct = runCts.Token;
            var progressReporter = new Progress<ApplicationStatus>(s => StatusText = s.Status);

            int originalFocuserPos = focuserMediator.GetInfo().Position;
            int relativeDefocus = DefocusSteps * DefocusDirection;
            string tempDir = Path.Combine(Path.GetTempPath(), "NINA", "AstroCircular_SKW",
                DateTime.Now.ToString("yyyyMMdd_HHmmss"));

            try {
                // Get equipment params for FOV calculation
                double fl = profileService?.ActiveProfile?.TelescopeSettings?.FocalLength ?? 1946;
                double sensorW = profileService?.ActiveProfile?.CameraSettings?.PixelSize > 0
                    ? cameraMediator.GetInfo().XSize * profileService.ActiveProfile.CameraSettings.PixelSize / 1000.0
                    : 36.0;
                double sensorH = profileService?.ActiveProfile?.CameraSettings?.PixelSize > 0
                    ? cameraMediator.GetInfo().YSize * profileService.ActiveProfile.CameraSettings.PixelSize / 1000.0
                    : 24.0;

                // Step 1: Switch filter
                StatusText = $"Switching to filter {FilterName}...";
                Progress = 5;
                await filterWheelMediator.ChangeFilter(new FilterInfo(FilterName, 0, (short)0), ct);

                // Step 2: Slew & Center on target star (plate-solve, in focus)
                StatusText = $"Slewing and centering on {StarName} (plate-solve)...";
                Progress = 10;
                var coords = new Coordinates(
                    Angle.ByHours(CoordinateUtils.ParseHMS(TargetRA)),
                    Angle.ByDegree(CoordinateUtils.ParseDMS(TargetDec)),
                    Epoch.J2000);
                // Slew first, then plate-solve will be added when IPlateSolverFactory is available
                // For now: slew to coordinates (plate-solve centering requires additional mediator)
                await telescopeMediator.SlewToCoordinatesAsync(coords, ct);

                // Step 3: Defocus
                StatusText = $"Defocusing {(relativeDefocus > 0 ? "+" : "")}{relativeDefocus} steps...";
                Progress = 15;
                await focuserMediator.MoveFocuserRelative(relativeDefocus, ct);

                // Step 4: Compute positions and capture
                var (fovW, fovH) = CircularPatternCalculator.ComputeFOV(sensorW, sensorH, fl);
                double centerRA = CoordinateUtils.ParseHMS(TargetRA);
                double centerDec = CoordinateUtils.ParseDMS(TargetDec);
                var positions = CircularPatternCalculator.Calculate(
                    centerRA, centerDec, fovW, fovH,
                    RingPositions, RadiusPercent, true, IncludeCenter);

                Directory.CreateDirectory(tempDir);
                var capturedFiles = new List<string>();
                int total = positions.Count;

                // Build the visual map
                BuildMapPositions();

                for (int i = 0; i < total; i++) {
                    ct.ThrowIfCancellationRequested();
                    var pos = positions[i];
                    int pctBase = 20;
                    int pctRange = 60;
                    Progress = pctBase + (i * pctRange / total);

                    // Update map: mark current position active
                    if (i < MapPositions.Count) {
                        MapPositions[i].State = PositionState.Active;
                    }
                    ProgressText = $"{i} / {total} positions";

                    // Blind slew (no plate-solve — telescope is defocused)
                    StatusText = $"Slewing to {pos.Label} ({i + 1}/{total})...";
                    try {
                        var posCoords = new Coordinates(
                            Angle.ByHours(pos.RAHours),
                            Angle.ByDegree(pos.DecDegrees),
                            Epoch.J2000);
                        await telescopeMediator.SlewToCoordinatesAsync(posCoords, ct);
                    } catch {
                        StatusText = $"Slew to {pos.Label} failed, skipping...";
                        if (i < MapPositions.Count) MapPositions[i].State = PositionState.Failed;
                        continue;
                    }

                    // Settle
                    if (SettleSeconds > 0) {
                        await Task.Delay(TimeSpan.FromSeconds(SettleSeconds), ct);
                    }

                    // Capture
                    StatusText = $"Exposing {ExposureTime}s at {pos.Label} ({i + 1}/{total})...";
                    try {
                        var captureSeq = new CaptureSequence(
                            ExposureTime,
                            CaptureSequence.ImageTypes.LIGHT,
                            new FilterInfo(FilterName, 0, (short)0),
                            new BinningMode((short)Binning, (short)Binning),
                            1) {
                            Gain = Gain,
                            Offset = Offset
                        };

                        var exposureData = await imagingMediator.CaptureImage(captureSeq, ct, progressReporter);
                        if (exposureData != null) {
                            var imageData = await exposureData.ToImageData(progressReporter, ct);
                            if (imageData != null) {
                                // Update live preview
                                UpdatePreviewImage(imageData);
                                string posLabel = pos.Label.Replace(" ", "");
                                var fileSaveInfo = new FileSaveInfo(profileService) {
                                    FilePath = tempDir,
                                    FilePattern = $"SKW_{posLabel}_{i:D2}",
                                    FileType = NINA.Core.Enum.FileTypeEnum.FITS
                                };
                                string savedPath = await imageData.SaveToDisk(fileSaveInfo, ct);
                                if (!string.IsNullOrEmpty(savedPath)) {
                                    capturedFiles.Add(savedPath);
                                    if (i < MapPositions.Count) MapPositions[i].State = PositionState.Done;
                                }
                            }
                        }
                    } catch (Exception ex) {
                        Logger.Warning($"SKW: Capture at {pos.Label} failed: {ex.Message}");
                        if (i < MapPositions.Count) MapPositions[i].State = PositionState.Failed;
                    }
                }

                if (capturedFiles.Count < 2) {
                    StatusText = $"Error: Only {capturedFiles.Count} frames captured. Need at least 2.";
                    return false;
                }

                // Step 5: Integrate
                Progress = 85;
                StatusText = $"Integrating {capturedFiles.Count} frames...";

                var (averaged, width, height, frameCount) = await FitsAverager.Average(
                    capturedFiles, imageDataFactory, ct);

                if (CropToCircle) {
                    (averaged, width, height) = FitsAverager.CropToCircle(averaged, width, height);
                }
                if (BinToHalf) {
                    (averaged, width, height) = FitsAverager.Bin2x2(averaged, width, height);
                }

                ushort[] pixelData = FitsAverager.ToUShort16(averaged);

                double pixelSize = profileService?.ActiveProfile?.CameraSettings?.PixelSize ?? 3.76;
                var headers = FitsHeaderWriter.BuildHeaders(
                    fl, pixelSize, Binning, BinToHalf, ExposureTime, -999, FilterName);

                string outputDir = !string.IsNullOrEmpty(SkyWaveOutputDirectory) ? SkyWaveOutputDirectory : tempDir;
                Directory.CreateDirectory(outputDir);
                string outputFile = $"SKW_Collimation_{DateTime.Now:yyyyMMdd_HHmmss}.fits";
                string outputPath = Path.Combine(outputDir, outputFile);

                RawFitsWriter.Write(outputPath, pixelData, width, height, headers);

                Progress = 95;
                StatusText = $"Saved: {outputPath}";

                // Cleanup sub-frames
                if (AutoCleanSubFrames) {
                    try {
                        foreach (var f in capturedFiles) {
                            if (File.Exists(f)) File.Delete(f);
                        }
                        if (Directory.Exists(tempDir) && !Directory.EnumerateFileSystemEntries(tempDir).Any()) {
                            Directory.Delete(tempDir);
                        }
                    } catch { }
                }

                Progress = 100;
                ProgressText = $"{capturedFiles.Count} / {total} positions done";
                StatusText = $"Done! {capturedFiles.Count} frames integrated. Output: {outputFile}";
                return true;

            } catch (OperationCanceledException) {
                StatusText = "Cancelled by user";
                return false;
            } catch (Exception ex) {
                StatusText = $"Error: {ex.Message}";
                Logger.Error($"SKW Collimation failed: {ex}");
                return false;
            } finally {
                // ALWAYS refocus
                try {
                    StatusText = IsRunning ? $"Refocusing to position {originalFocuserPos}..." : StatusText;
                    await focuserMediator.MoveFocuserRelative(-relativeDefocus, CancellationToken.None);
                } catch (Exception ex) {
                    Logger.Error($"SKW: Refocus failed! Original position was {originalFocuserPos}: {ex.Message}");
                }
                IsRunning = false;
                runCts?.Dispose();
                runCts = null;
            }
        }

        // ── Settings Persistence ──

        private const string SETTINGS_PREFIX = "SKW_";

        private void SaveSettings() {
            try {
                var guid = new Guid("b7e3f1a2-9c4d-4e8b-a6f5-1d2c3b4a5e6f");
                var accessor = new NINA.Profile.PluginOptionsAccessor(profileService, guid);
                accessor.SetValueString(SETTINGS_PREFIX + "StarName", StarName);
                accessor.SetValueString(SETTINGS_PREFIX + "TargetRA", TargetRA);
                accessor.SetValueString(SETTINGS_PREFIX + "TargetDec", TargetDec);
                accessor.SetValueInt32(SETTINGS_PREFIX + "DefocusSteps", DefocusSteps);
                accessor.SetValueInt32(SETTINGS_PREFIX + "DefocusDirection", DefocusDirection);
                accessor.SetValueDouble(SETTINGS_PREFIX + "ExposureTime", ExposureTime);
                accessor.SetValueString(SETTINGS_PREFIX + "FilterName", FilterName);
                accessor.SetValueInt32(SETTINGS_PREFIX + "Gain", Gain);
                accessor.SetValueInt32(SETTINGS_PREFIX + "Offset", Offset);
                accessor.SetValueInt32(SETTINGS_PREFIX + "Binning", Binning);
                accessor.SetValueInt32(SETTINGS_PREFIX + "RingPositions", RingPositions);
                accessor.SetValueInt32(SETTINGS_PREFIX + "RadiusPercent", RadiusPercent);
                accessor.SetValueInt32(SETTINGS_PREFIX + "SettleSeconds", SettleSeconds);
                accessor.SetValueBoolean(SETTINGS_PREFIX + "IncludeCenter", IncludeCenter);
                accessor.SetValueBoolean(SETTINGS_PREFIX + "CropToCircle", CropToCircle);
                accessor.SetValueBoolean(SETTINGS_PREFIX + "BinToHalf", BinToHalf);
                accessor.SetValueBoolean(SETTINGS_PREFIX + "AutoCleanSubFrames", AutoCleanSubFrames);
                accessor.SetValueString(SETTINGS_PREFIX + "SkyWaveOutputDirectory", SkyWaveOutputDirectory);
            } catch { }
        }

        private void LoadSettings() {
            try {
                var guid = new Guid("b7e3f1a2-9c4d-4e8b-a6f5-1d2c3b4a5e6f");
                var accessor = new NINA.Profile.PluginOptionsAccessor(profileService, guid);
                starName = accessor.GetValueString(SETTINGS_PREFIX + "StarName", starName);
                targetRA = accessor.GetValueString(SETTINGS_PREFIX + "TargetRA", targetRA);
                targetDec = accessor.GetValueString(SETTINGS_PREFIX + "TargetDec", targetDec);
                defocusSteps = accessor.GetValueInt32(SETTINGS_PREFIX + "DefocusSteps", defocusSteps);
                defocusDirection = accessor.GetValueInt32(SETTINGS_PREFIX + "DefocusDirection", defocusDirection);
                exposureTime = accessor.GetValueDouble(SETTINGS_PREFIX + "ExposureTime", exposureTime);
                filterName = accessor.GetValueString(SETTINGS_PREFIX + "FilterName", filterName);
                gain = accessor.GetValueInt32(SETTINGS_PREFIX + "Gain", gain);
                offset = accessor.GetValueInt32(SETTINGS_PREFIX + "Offset", offset);
                binning = accessor.GetValueInt32(SETTINGS_PREFIX + "Binning", binning);
                ringPositions = accessor.GetValueInt32(SETTINGS_PREFIX + "RingPositions", ringPositions);
                radiusPercent = accessor.GetValueInt32(SETTINGS_PREFIX + "RadiusPercent", radiusPercent);
                settleSeconds = accessor.GetValueInt32(SETTINGS_PREFIX + "SettleSeconds", settleSeconds);
                includeCenter = accessor.GetValueBoolean(SETTINGS_PREFIX + "IncludeCenter", includeCenter);
                cropToCircle = accessor.GetValueBoolean(SETTINGS_PREFIX + "CropToCircle", cropToCircle);
                binToHalf = accessor.GetValueBoolean(SETTINGS_PREFIX + "BinToHalf", binToHalf);
                autoCleanSubFrames = accessor.GetValueBoolean(SETTINGS_PREFIX + "AutoCleanSubFrames", autoCleanSubFrames);
                skyWaveOutputDirectory = accessor.GetValueString(SETTINGS_PREFIX + "SkyWaveOutputDirectory", skyWaveOutputDirectory);
            } catch { }
        }

        private void LoadFromProfile() {
            try {
                var p = profileService?.ActiveProfile;
                if (p == null) return;
                try { if (p.AstrometrySettings?.Latitude != 0) { /* lat/lon used in FindBestStar */ } } catch { }
            } catch { }
        }
    }
}
