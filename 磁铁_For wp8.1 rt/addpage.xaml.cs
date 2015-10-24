using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Data.Xml.Dom;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Phone.UI.Input;
using Windows.Storage;
using Windows.UI.Notifications;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// “空白页”项模板在 http://go.microsoft.com/fwlink/?LinkID=390556 上有介绍

namespace 磁铁_For_wp8._1_rt
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class addpage : Page
    {
        public addpage()
        {
            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            HardwareButtons.BackPressed += HardwareButtons_BackPressed;//注册重写后退按钮事件
            var selectedEventManage = (磁铁_For_wp8._1_rt.MainPage.EventManage)e.Parameter;
            if (selectedEventManage != null)
            {
                eventnameTextBox.Text = selectedEventManage.EventFileTitle;
                remarksTextBox.Text = selectedEventManage.EventFileRemarks;
                eventDatePicker.Date = DateTime.Parse(selectedEventManage.EventFileDate);
                bar_Accept.IsEnabled = true;
                remindTextBlock.Visibility = Visibility.Visible;
            }
        }

        //离开页面时，取消事件
        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            HardwareButtons.BackPressed -= HardwareButtons_BackPressed;//注册重写后退按钮事件
        }

        private void HardwareButtons_BackPressed(object sender, BackPressedEventArgs e)//重写后退按钮，如果要对所有页面使用，可以放在App.Xaml.cs的APP初始化函数中重写。
        {
            Frame rootFrame = Window.Current.Content as Frame;
            if (rootFrame != null && rootFrame.CanGoBack)
            {
                rootFrame.GoBack();
                e.Handled = true;
            }
        }



        private async void bar_Accept_Click(object sender, RoutedEventArgs e)
        {
            if (eventnameTextBox.Text != "")
            {
                IStorageFolder applicationFolder = ApplicationData.Current.LocalFolder;
                IStorageFile storageFileWR = await applicationFolder.CreateFileAsync(eventnameTextBox.Text, CreationCollisionOption.OpenIfExists);
                await FileIO.WriteTextAsync(storageFileWR, remarksTextBox.Text + "!@#$%^&*" +eventDatePicker.Date.ToString());
                Debug.WriteLine(remarksTextBox.Text + "!@#$%^&*" + eventDatePicker.Date.ToString());

                Frame rootFrame = Window.Current.Content as Frame;
                if (rootFrame != null && rootFrame.CanGoBack)
                {
                    rootFrame.GoBack();
                }
            }
            else
            {
                XmlDocument toastXml = ToastNotificationManager.GetTemplateContent(ToastTemplateType.ToastText01);
                XmlNodeList elements = toastXml.GetElementsByTagName("text");
                elements[0].AppendChild(toastXml.CreateTextNode("事件名称不能为空"));
                ToastNotification toast = new ToastNotification(toastXml);
                //toast.Activated += toast_Activated;//点击
                //toast.Dismissed += toast_Dismissed;//消失
                //toast.Failed += toast_Failed;//消除
                ToastNotificationManager.CreateToastNotifier().Show(toast);

                //从通知中心删除
                ToastNotificationManager.History.Clear();
            }
        }

        private void eventnameTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (eventnameTextBox.Text != "")
                bar_Accept.IsEnabled = true;
            else
                bar_Accept.IsEnabled = false;
        }
    }
}
