using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace QRCodeMagic.ViewModels
{
    public class NotifyIconViewModel
    {
        public ICommand ShowWindowCommand
        {
            get
            {
                return new RelayCommand
                (() =>
                    {
                        Application.Current.MainWindow.WindowState = WindowState.Normal;
                        Application.Current.MainWindow.Show();
                        Application.Current.MainWindow.Activate();
                    }
                );
            }
        }

        public ICommand ExitApplicationCommand
        {
            get
            {
                return new RelayCommand
                (() =>
                    {
                        Application.Current.MainWindow = null;
                        Application.Current.Shutdown();
                    }
                );
            }
        }
    }
}
