using MaterialDesignColors;
using MaterialDesignColors.ColorManipulation;
using MaterialDesignThemes.Wpf;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Regions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace MyQrCodeScanner
{
    public class SkinViewModel : BindableBase
    {
        public SkinViewModel()
        {
            Styles = new ObservableCollection<Color>();

            ChangeHueCommand = new DelegateCommand<object>(ThemeHelper.ChangeHue);
            ToggleBaseCommand = new DelegateCommand<object>(ThemeHelper.ApplyBase);
            
        }

        private bool isdark;

        public bool IsDark
        {
            get { return isdark; }
            set
            {
                isdark = value;
                this.RaisePropertyChanged("IsDark");
                ThemeHelper.ApplyBase(isdark);
            }
        }


        public void OnLoaded()
        {
            var swatches = SwatchHelper.Swatches.ToList().SelectMany(t => t.Hues);
            foreach (var item in swatches) Styles.Add(item);
            IsDark = Convert.ToBoolean(IniHelper.GetKeyValue("main", "isdark", "false", IniHelper.inipath));
        }

        private ObservableCollection<Color> _styles;

        public ObservableCollection<Color> Styles
        {
            get { return _styles; }
            set { _styles = value; RaisePropertyChanged(); }
        }

        //改变颜色
        public DelegateCommand<object> ChangeHueCommand { get; }

        //改变主题
        public DelegateCommand<object> ToggleBaseCommand { get; }

    }
}
