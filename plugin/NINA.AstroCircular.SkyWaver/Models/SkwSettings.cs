using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace NINA.AstroCircular.SkyWaver.Models {

    /// <summary>
    /// Plugin-level settings persisted across sessions.
    /// Telescope/sensor params are set once; imaging defaults pre-populate new sequence items.
    /// </summary>
    public class SkwSettings : INotifyPropertyChanged {

        // ── Telescope / Sensor ──

        private double focalLengthMm = 1946;
        public double FocalLengthMm {
            get => focalLengthMm;
            set { focalLengthMm = value; OnPropertyChanged(); }
        }

        private double apertureMm = 304.8;
        public double ApertureMm {
            get => apertureMm;
            set { apertureMm = value; OnPropertyChanged(); }
        }

        private double sensorWidthMm = 36.0;
        public double SensorWidthMm {
            get => sensorWidthMm;
            set { sensorWidthMm = value; OnPropertyChanged(); }
        }

        private double sensorHeightMm = 24.0;
        public double SensorHeightMm {
            get => sensorHeightMm;
            set { sensorHeightMm = value; OnPropertyChanged(); }
        }

        private double pixelSizeUm = 3.76;
        public double PixelSizeUm {
            get => pixelSizeUm;
            set { pixelSizeUm = value; OnPropertyChanged(); }
        }

        // ── Observer Location ──

        private double observerLatitude = 52.17;
        public double ObserverLatitude {
            get => observerLatitude;
            set { observerLatitude = value; OnPropertyChanged(); }
        }

        private double observerLongitude = 7.25;
        public double ObserverLongitude {
            get => observerLongitude;
            set { observerLongitude = value; OnPropertyChanged(); }
        }

        // ── Paths ──

        private string skyWaveOutputDirectory = "";
        public string SkyWaveOutputDirectory {
            get => skyWaveOutputDirectory;
            set { skyWaveOutputDirectory = value; OnPropertyChanged(); }
        }

        // ── Imaging Defaults ──

        private string defaultFilterName = "R";
        public string DefaultFilterName {
            get => defaultFilterName;
            set { defaultFilterName = value; OnPropertyChanged(); }
        }

        private int defaultGain = 100;
        public int DefaultGain {
            get => defaultGain;
            set { defaultGain = value; OnPropertyChanged(); }
        }

        private int defaultOffset = 0;
        public int DefaultOffset {
            get => defaultOffset;
            set { defaultOffset = value; OnPropertyChanged(); }
        }

        private int defaultBinning = 1;
        public int DefaultBinning {
            get => defaultBinning;
            set { defaultBinning = value; OnPropertyChanged(); }
        }

        private double defaultExposureSeconds = 8;
        public double DefaultExposureSeconds {
            get => defaultExposureSeconds;
            set { defaultExposureSeconds = value; OnPropertyChanged(); }
        }

        // ── Defocus Defaults ──

        private int defaultDefocusSteps = 2442;
        public int DefaultDefocusSteps {
            get => defaultDefocusSteps;
            set { defaultDefocusSteps = value; OnPropertyChanged(); }
        }

        private int defaultDefocusDirection = 1; // +1 = extra-focal, -1 = intra-focal
        public int DefaultDefocusDirection {
            get => defaultDefocusDirection;
            set { defaultDefocusDirection = value; OnPropertyChanged(); }
        }

        // ── Pattern Defaults ──

        private int defaultRingPositions = 8;
        public int DefaultRingPositions {
            get => defaultRingPositions;
            set { defaultRingPositions = value; OnPropertyChanged(); }
        }

        private int defaultRadiusPercent = 80;
        public int DefaultRadiusPercent {
            get => defaultRadiusPercent;
            set { defaultRadiusPercent = value; OnPropertyChanged(); }
        }

        private int defaultSettleSeconds = 3;
        public int DefaultSettleSeconds {
            get => defaultSettleSeconds;
            set { defaultSettleSeconds = value; OnPropertyChanged(); }
        }

        private bool includeCenter = true;
        public bool IncludeCenter {
            get => includeCenter;
            set { includeCenter = value; OnPropertyChanged(); }
        }

        private bool useCircularPattern = true;
        public bool UseCircularPattern {
            get => useCircularPattern;
            set { useCircularPattern = value; OnPropertyChanged(); }
        }

        // ── Integration Defaults ──

        private bool autoCleanSubFrames = true;
        public bool AutoCleanSubFrames {
            get => autoCleanSubFrames;
            set { autoCleanSubFrames = value; OnPropertyChanged(); }
        }

        // ── INotifyPropertyChanged ──

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null) {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
