using System.ComponentModel.Composition;
using System.Windows;

namespace NINA.AstroCircular.SkyWaver.Resources {

    [Export(typeof(ResourceDictionary))]
    public class SkwResourceDictionary : ResourceDictionary {
        public SkwResourceDictionary() {
            // Load merged dictionaries programmatically instead of XAML InitializeComponent
            MergedDictionaries.Add(new ResourceDictionary {
                Source = new System.Uri("pack://application:,,,/NINA.AstroCircular.SkyWaver;component/Resources/OptionsTemplate.xaml")
            });
            MergedDictionaries.Add(new ResourceDictionary {
                Source = new System.Uri("pack://application:,,,/NINA.AstroCircular.SkyWaver;component/Resources/SkwDefocusTemplate.xaml")
            });
            MergedDictionaries.Add(new ResourceDictionary {
                Source = new System.Uri("pack://application:,,,/NINA.AstroCircular.SkyWaver;component/Resources/SkwCircularCaptureTemplate.xaml")
            });
            MergedDictionaries.Add(new ResourceDictionary {
                Source = new System.Uri("pack://application:,,,/NINA.AstroCircular.SkyWaver;component/Resources/SkwIntegrateFramesTemplate.xaml")
            });
            MergedDictionaries.Add(new ResourceDictionary {
                Source = new System.Uri("pack://application:,,,/NINA.AstroCircular.SkyWaver;component/Resources/SkwCollimationRunTemplate.xaml")
            });
        }
    }
}
