using System.ComponentModel.Composition;
using System.Windows;

namespace NINA.AstroCircular.SkyWaver.Resources {

    [Export(typeof(ResourceDictionary))]
    public partial class SkwResourceDictionary : ResourceDictionary {
        public SkwResourceDictionary() {
            InitializeComponent();
        }
    }
}
