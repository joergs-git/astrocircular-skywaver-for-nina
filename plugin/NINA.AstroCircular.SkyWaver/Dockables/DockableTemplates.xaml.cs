using System.ComponentModel.Composition;
using System.Windows;

namespace NINA.AstroCircular.SkyWaver.Dockables {

    [Export(typeof(ResourceDictionary))]
    public partial class DockableTemplates : ResourceDictionary {
        public DockableTemplates() {
            InitializeComponent();
        }
    }
}
