using NINA.AstroCircular.SkyWaver.Models;
using NINA.Plugin;
using NINA.Plugin.Interfaces;
using NINA.Profile.Interfaces;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Runtime.CompilerServices;

namespace NINA.AstroCircular.SkyWaver {

    [Export(typeof(IPluginManifest))]
    public class AstroCircularSkyWaverPlugin : PluginBase, INotifyPropertyChanged {
        private readonly IProfileService profileService;

        /// <summary>
        /// Plugin-level settings bound to the Options page.
        /// Persisted in NINA's plugin settings store.
        /// </summary>
        public SkwSettings Settings { get; }

        [ImportingConstructor]
        public AstroCircularSkyWaverPlugin(IProfileService profileService) {
            this.profileService = profileService;
            Settings = new SkwSettings();

            // Load saved settings from NINA profile if available
            // Settings are auto-persisted via NINA's plugin settings mechanism
            // when using {Binding Settings.PropertyName} in the options XAML
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null) {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
