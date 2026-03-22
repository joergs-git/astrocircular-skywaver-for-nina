using NINA.AstroCircular.SkyWaver.Models;
using NINA.Plugin;
using NINA.Plugin.Interfaces;
using NINA.Profile.Interfaces;
using System;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Runtime.CompilerServices;

namespace NINA.AstroCircular.SkyWaver {

    [Export(typeof(IPluginManifest))]
    public class AstroCircularSkyWaverPlugin : PluginBase, INotifyPropertyChanged {
        private readonly IProfileService profileService;

        /// <summary>
        /// Plugin-level settings bound to the Options page.
        /// </summary>
        public SkwSettings Settings { get; }

        [ImportingConstructor]
        public AstroCircularSkyWaverPlugin(IProfileService profileService) {
            this.profileService = profileService;
            Settings = new SkwSettings();

            // Auto-populate equipment defaults from NINA's active profile
            // so users don't have to type anything that NINA already knows
            LoadFromProfile();
        }

        /// <summary>
        /// Read telescope, camera, and location settings from the active NINA profile.
        /// Only overwrites defaults — user can still override in the options page.
        /// </summary>
        private void LoadFromProfile() {
            try {
                var profile = profileService?.ActiveProfile;
                if (profile == null) return;

                // Telescope
                var telescope = profile.TelescopeSettings;
                if (telescope != null) {
                    if (telescope.FocalLength > 0)
                        Settings.FocalLengthMm = telescope.FocalLength;
                    // FocalRatio = FL / Aperture, so Aperture = FL / FR
                    if (telescope.FocalRatio > 0 && telescope.FocalLength > 0)
                        Settings.ApertureMm = telescope.FocalLength / telescope.FocalRatio;
                }

                // Camera
                var camera = profile.CameraSettings;
                if (camera != null) {
                    if (camera.PixelSize > 0)
                        Settings.PixelSizeUm = camera.PixelSize;
                }

                // Location (latitude / longitude)
                var location = profile.AstrometrySettings;
                if (location != null) {
                    if (location.Latitude != 0)
                        Settings.ObserverLatitude = location.Latitude;
                    if (location.Longitude != 0)
                        Settings.ObserverLongitude = location.Longitude;
                }
            } catch {
                // Non-critical — defaults remain if profile read fails
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null) {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
