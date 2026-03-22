using NINA.AstroCircular.SkyWaver.Models;
using NINA.Core.Utility;
using NINA.Plugin;
using NINA.Plugin.Interfaces;
using NINA.Profile.Interfaces;
using System;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace NINA.AstroCircular.SkyWaver {

    [Export(typeof(IPluginManifest))]
    public class AstroCircularSkyWaverPlugin : PluginBase, INotifyPropertyChanged {
        private readonly IProfileService profileService;

        public SkwSettings Settings { get; }
        public ICommand BrowseFolderCommand { get; }

        [ImportingConstructor]
        public AstroCircularSkyWaverPlugin(IProfileService profileService) {
            this.profileService = profileService;
            Settings = new SkwSettings();
            BrowseFolderCommand = new RelayCommand((o) => BrowseFolder());

            try { LoadFromProfile(); } catch { }
        }

        private void BrowseFolder() {
            using (var dialog = new System.Windows.Forms.FolderBrowserDialog()) {
                dialog.Description = "Select SkyWave watch folder";
                if (!string.IsNullOrEmpty(Settings.SkyWaveOutputDirectory)) {
                    dialog.SelectedPath = Settings.SkyWaveOutputDirectory;
                }
                if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK) {
                    Settings.SkyWaveOutputDirectory = dialog.SelectedPath;
                }
            }
        }

        private void LoadFromProfile() {
            var p = profileService?.ActiveProfile;
            if (p == null) return;
            try { if (p.TelescopeSettings.FocalLength > 0) Settings.FocalLengthMm = p.TelescopeSettings.FocalLength; } catch { }
            try { if (p.TelescopeSettings.FocalRatio > 0) Settings.ApertureMm = p.TelescopeSettings.FocalLength / p.TelescopeSettings.FocalRatio; } catch { }
            try { if (p.CameraSettings.PixelSize > 0) Settings.PixelSizeUm = p.CameraSettings.PixelSize; } catch { }
            try { if (p.AstrometrySettings.Latitude != 0) Settings.ObserverLatitude = p.AstrometrySettings.Latitude; } catch { }
            try { if (p.AstrometrySettings.Longitude != 0) Settings.ObserverLongitude = p.AstrometrySettings.Longitude; } catch { }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null) {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
