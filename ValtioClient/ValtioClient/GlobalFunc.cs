using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Framework.UI.Controls;
using Framework.UI.Input;

namespace ValtioClient
{
    public static class GlobalFunc
    {
        public delegate void CallBack();

        public static async void ShowMessageBox(String header, String content)
        {
            MessageBoxResult result = await MessageDialog.ShowAsync(
                header,
                content,
                MessageBoxButton.OK,
                MessageDialogType.Light);
        }
        public static async void ShowConfirmMessageBox(String header, String content, CallBack func)
        {
            MessageBoxResult result = await MessageDialog.ShowAsync(
                header,
                content,
                MessageBoxButton.YesNo,
                MessageDialogType.Light);
            if (result == MessageBoxResult.Yes)
                func();
        }
    }
}
