using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MaterialDesignColors;
using System.Collections.ObjectModel;
using System.Windows.Media;

namespace MyQrCodeScanner
{
    public class SkinViewModel : ObservableObject
    {
        public SkinViewModel()
        {
            Styles = new ObservableCollection<Color>();
            ChangeHueCommand = new RelayCommand<object>(ThemeHelper.ChangeHue);
            ToggleBaseCommand = new RelayCommand<object>(ThemeHelper.ApplyBase);
            
        }

        public void OnLoaded()
        {
            var swatches = SwatchHelper.Swatches.ToList().SelectMany(t => t.Hues);
            foreach (var item in swatches) Styles.Add(item);
        }

        private ObservableCollection<Color> _styles;

        public ObservableCollection<Color> Styles
        {
            get { return _styles; }
            set { _styles = value; OnPropertyChanged(); }
        }

        //改变颜色
        public RelayCommand<object> ChangeHueCommand { get; }

        //改变主题
        public RelayCommand<object> ToggleBaseCommand { get; }

    }
}
