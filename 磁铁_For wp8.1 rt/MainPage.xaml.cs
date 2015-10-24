using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.UI.StartScreen;
using Windows.UI.Notifications;
using Windows.UI.Popups;
using Windows.Data.Xml.Dom;
using Windows.ApplicationModel.Background;
using Windows.Storage;
using Windows.Storage.Streams;
using System.Diagnostics;
using Windows.UI.Core;
using Windows.Phone.UI.Input;
using Windows.ApplicationModel.Store;

// “空白页”项模板在 http://go.microsoft.com/fwlink/?LinkId=391641 上有介绍

namespace 磁铁_For_wp8._1_rt
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class MainPage : Page
    {
        IStorageFolder applicationFolder = ApplicationData.Current.LocalFolder;
        private string tileId = "001";
        int[] kk;
        public MainPage()
        {
            this.InitializeComponent();

            this.NavigationCacheMode = NavigationCacheMode.Required;
        }

        /// <summary>
        /// 在此页将要在 Frame 中显示时进行调用。
        /// </summary>
        /// <param name="e">描述如何访问此页的事件数据。
        /// 此参数通常用于配置页。</param>
        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            HardwareButtons.BackPressed += HardwareButtons_BackPressed;//注册重写后退按钮事件
            addBackgroundTask();
            refreshList();
            refreshTile();
        }

        private async void refreshTile()
        {
            IStorageFolder applicationFolder = ApplicationData.Current.LocalFolder;
            var files = await applicationFolder.GetFilesAsync();
            int[] kk;
            foreach (StorageFile file in files)
            {
                tileId = file.Name.ToString();
                string text = "";
                IStorageFile storageFileRE = await applicationFolder.GetFileAsync(file.Name.ToString());
                IRandomAccessStream accessStream = await storageFileRE.OpenReadAsync();
                using (StreamReader streamReader = new StreamReader(accessStream.AsStreamForRead((int)accessStream.Size)))
                {
                    text = streamReader.ReadToEnd();
                }
                kk = DateTimeDiff.dateTimeDiff.toResult(text.Substring(text.IndexOf("!@#$%^&*") + 8), DateTime.Now.Date.ToString(), DateTimeDiff.diffResultFormat.dd);
                Debug.WriteLine(kk[0].ToString());
                string EventFileRemarks = text.Substring(0, text.IndexOf("!@#$%^&*"));
                string EventFileTitle = file.Name.ToString();
                string EventFileDateRemain = kk[0].ToString();
                string EventFileDate = text.Substring(text.IndexOf("!@#$%^&*") + 8);

                //更新磁贴
                try
                {
                    XmlDocument wideTileXml = TileUpdateManager.GetTemplateContent(TileTemplateType.TileWide310x150BlockAndText01);
                    XmlNodeList wideTileTextAttributes = wideTileXml.GetElementsByTagName("text");
                    wideTileTextAttributes[0].AppendChild(wideTileXml.CreateTextNode(EventFileDateRemain));
                    wideTileTextAttributes[2].AppendChild(wideTileXml.CreateTextNode(tileId));
                    if (DateTime.Now.Date < DateTime.Parse(EventFileDate))
                        wideTileTextAttributes[1].AppendChild(wideTileXml.CreateTextNode("剩余"));
                    else
                        wideTileTextAttributes[1].AppendChild(wideTileXml.CreateTextNode("已过"));
                    if (EventFileRemarks != "")
                        wideTileTextAttributes[3].AppendChild(wideTileXml.CreateTextNode(EventFileRemarks));
                    else
                        wideTileTextAttributes[3].AppendChild(wideTileXml.CreateTextNode("无备注"));

                    XmlDocument squareTileXml = TileUpdateManager.GetTemplateContent(TileTemplateType.TileSquare150x150Text01);
                    XmlNodeList squareTileXmlAttributes = squareTileXml.GetElementsByTagName("text");
                    squareTileXmlAttributes[0].AppendChild(squareTileXml.CreateTextNode(tileId));
                    squareTileXmlAttributes[1].AppendChild(squareTileXml.CreateTextNode(EventFileRemarks));
                    if (DateTime.Now.Date < DateTime.Parse(EventFileDate))
                        squareTileXmlAttributes[2].AppendChild(squareTileXml.CreateTextNode("剩余 " + EventFileDateRemain + " 天"));
                    else
                        squareTileXmlAttributes[2].AppendChild(squareTileXml.CreateTextNode("已过 " + EventFileDateRemain + " 天"));


                    var TileUpdater = TileUpdateManager.CreateTileUpdaterForApplication(tileId);
                    ScheduledTileNotification wideSchedule = new ScheduledTileNotification(wideTileXml, DateTimeOffset.Now.AddSeconds(5));
                    ScheduledTileNotification squareSchedule = new ScheduledTileNotification(squareTileXml, DateTimeOffset.Now.AddSeconds(5));
                    TileUpdater.Clear();

                    TileUpdater.EnableNotificationQueue(true);
                    TileUpdater.AddToSchedule(squareSchedule);
                    TileUpdater.AddToSchedule(wideSchedule);
                }
                catch (Exception) { }
            }
        }

        public class EventManage
        {
            public string EventFileTitle { get; set; }
            public string EventFileRemarks { get; set; }
            public string EventFileDate { get; set; }
            public string EventFileDateRemain { get; set; }
        }

        private async void refreshList()
        {
            var files = await applicationFolder.GetFilesAsync();
            List<EventManage> Eventdata = new List<EventManage>();
            foreach (StorageFile file in files)
            {
                string text = "";
                IStorageFile storageFileRE = await applicationFolder.GetFileAsync(file.Name.ToString());
                IRandomAccessStream accessStream = await storageFileRE.OpenReadAsync();
                using (StreamReader streamReader = new StreamReader(accessStream.AsStreamForRead((int)accessStream.Size)))
                {
                    text = streamReader.ReadToEnd();
                }
                Debug.WriteLine("File:" + text);
                Debug.WriteLine(text.Substring(0, text.IndexOf("!@#$%^&*")));
                Debug.WriteLine(text.Substring(text.IndexOf("!@#$%^&*") + 8));

                kk = 磁铁_For_wp8._1_rt.DateTimeDiff.dateTimeDiff.toResult(text.Substring(text.IndexOf("!@#$%^&*") + 8), DateTime.Now.Date.ToString(), 磁铁_For_wp8._1_rt.DateTimeDiff.diffResultFormat.dd);

                if (DateTime.Now.Date > DateTime.Parse(text.Substring(text.IndexOf("!@#$%^&*") + 8)))
                    kk[0] = -kk[0];
                string EventFileRemarks = text.Substring(0, text.IndexOf("!@#$%^&*"));
                if (EventFileRemarks == "")
                    EventFileRemarks = "无备注";
                Eventdata.Add(new EventManage { EventFileTitle = file.Name.ToString(), EventFileRemarks = EventFileRemarks, EventFileDate = text.Substring(text.IndexOf("!@#$%^&*") + 8), EventFileDateRemain = kk[0].ToString() });
            }
            EventListView.ItemsSource = Eventdata;
        }

        private async void addBackgroundTask()
        {
            bool taskRegistered = false;
            string exampleTaskName = "citieBackgroundTask";
            taskRegistered = BackgroundTaskRegistration.AllTasks.Any(x => x.Value.Name == exampleTaskName);

            if(taskRegistered)
            {
                foreach (var task in BackgroundTaskRegistration.AllTasks)
                {
                    if (task.Value.Name == "SimpleBackTask")
                    {
                        task.Value.Unregister(true);//删除后台任务
                    }
                }
                taskRegistered = false;
            }

            if (!taskRegistered)
            {
                BackgroundAccessStatus status = await BackgroundExecutionManager.RequestAccessAsync();
                var builder = new BackgroundTaskBuilder();
                builder.Name = exampleTaskName;
                builder.TaskEntryPoint = "Tasks.ExampleBackgroundTask";
                //后台触发器
                //builder.SetTrigger(new SystemTrigger(SystemTriggerType.UserPresent, false));
                builder.SetTrigger(new SystemTrigger(SystemTriggerType.NetworkStateChange, false));
                builder.SetTrigger(new SystemTrigger(SystemTriggerType.InternetAvailable, false));
                //builder.SetTrigger(new SystemTrigger(SystemTriggerType.SmsReceived, false));

                //builder.SetTrigger(new MaintenanceTrigger(15, false));

                BackgroundTaskRegistration task = builder.Register();
                task.Completed += task_Completed;
            }
            else
            {
                var cur = BackgroundTaskRegistration.AllTasks.FirstOrDefault(x => x.Value.Name == exampleTaskName);
                BackgroundTaskRegistration task = (BackgroundTaskRegistration)(cur.Value);
                task.Completed += task_Completed;
            }
        }

        //离开页面时，取消事件
        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            HardwareButtons.BackPressed -= HardwareButtons_BackPressed;//注册重写后退按钮事件
        }

        private void HardwareButtons_BackPressed(object sender, BackPressedEventArgs e)//重写后退按钮，如果要对所有页面使用，可以放在App.Xaml.cs的APP初始化函数中重写。
        {
            Application.Current.Exit();
        }


        async void task_Completed(BackgroundTaskRegistration sender, BackgroundTaskCompletedEventArgs args)
        {
            Debug.WriteLine(sender.Name);
        }

        private void bar_add_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(addpage));
        }

        private async void bar_good_Click(object sender, RoutedEventArgs e)
        {
            await Windows.System.Launcher.LaunchUriAsync(new Uri("zune:reviewapp?appid=" + CurrentApp.AppId)); //用于商店app，自动获取ID
        }


        private void EventListViewGrid_Holding(object sender, HoldingRoutedEventArgs e)
        {
            FrameworkElement senderElement = sender as FrameworkElement;
            FlyoutBase flyoutBase = FlyoutBase.GetAttachedFlyout(senderElement);

            flyoutBase.ShowAt(senderElement);
        }

        private async void PinMenuFlyout_Click(object sender, RoutedEventArgs e)
        {
            EventManage selectedEventManage = ((MenuFlyoutItem)sender).DataContext as EventManage;
            string EventFileTitle = selectedEventManage.EventFileTitle;
            string EventFileRemarks = selectedEventManage.EventFileRemarks;
            string EventFileDateRemain = selectedEventManage.EventFileDateRemain;
            if (Int32.Parse(EventFileDateRemain) < 0)
                EventFileDateRemain = (-Int32.Parse(EventFileDateRemain)).ToString(); ;
            string EventFileDate = selectedEventManage.EventFileDate;
            Debug.WriteLine(EventFileTitle);

            string tileId = EventFileTitle;

            if (SecondaryTile.Exists(tileId))
            {
                await new MessageDialog("已存在").ShowAsync();
            }
            else
            {
                try
                {
                    Uri square71x71Logo = new Uri("ms-appx:///Assets/Square71x71Logo.scale-240.png");
                    Uri square150x150Logo = new Uri("ms-appx:///Assets/Square71x71Logo.scale-240.png");
                    Uri wide310x150Logo = new Uri("ms-appx:///Assets/WideLogo.scale-240.png");
                    SecondaryTile msecondaryTile = new SecondaryTile(tileId,
                                                                    "倒数计事",
                                                                    tileId,
                                                                    square150x150Logo,
                                                                    TileSize.Wide310x150);
                    msecondaryTile.VisualElements.Wide310x150Logo = wide310x150Logo;
                    msecondaryTile.VisualElements.Square150x150Logo = square150x150Logo;
                    msecondaryTile.VisualElements.Square71x71Logo = square71x71Logo;
                    msecondaryTile.VisualElements.ShowNameOnSquare150x150Logo = false;

                    bool isPinned = await msecondaryTile.RequestCreateAsync();

                    XmlDocument wideTileXml = TileUpdateManager.GetTemplateContent(TileTemplateType.TileWide310x150BlockAndText01);
                    XmlNodeList wideTileTextAttributes = wideTileXml.GetElementsByTagName("text");
                    wideTileTextAttributes[0].AppendChild(wideTileXml.CreateTextNode(EventFileDateRemain));
                    wideTileTextAttributes[2].AppendChild(wideTileXml.CreateTextNode(tileId));
                    if (DateTime.Now.Date < DateTime.Parse(EventFileDate))
                        wideTileTextAttributes[1].AppendChild(wideTileXml.CreateTextNode("剩余"));
                    else
                        wideTileTextAttributes[1].AppendChild(wideTileXml.CreateTextNode("已过"));
                    if (EventFileRemarks != "")
                        wideTileTextAttributes[3].AppendChild(wideTileXml.CreateTextNode(EventFileRemarks));
                    else
                        wideTileTextAttributes[3].AppendChild(wideTileXml.CreateTextNode("无备注"));



                    XmlDocument squareTileXml = TileUpdateManager.GetTemplateContent(TileTemplateType.TileSquare150x150Text01);
                    XmlNodeList squareTileXmlAttributes = squareTileXml.GetElementsByTagName("text");
                    squareTileXmlAttributes[0].AppendChild(squareTileXml.CreateTextNode(tileId));
                    squareTileXmlAttributes[1].AppendChild(squareTileXml.CreateTextNode(EventFileRemarks));
                    if (DateTime.Now.Date < DateTime.Parse(EventFileDate))
                        squareTileXmlAttributes[2].AppendChild(squareTileXml.CreateTextNode("剩余 " + EventFileDateRemain + " 天"));
                    else
                        squareTileXmlAttributes[2].AppendChild(squareTileXml.CreateTextNode("已过 " + EventFileDateRemain + " 天"));

                    var TileUpdater = TileUpdateManager.CreateTileUpdaterForApplication(tileId);
                    ScheduledTileNotification wideSchedule = new ScheduledTileNotification(wideTileXml, DateTimeOffset.Now.AddSeconds(5));
                    ScheduledTileNotification squareSchedule = new ScheduledTileNotification(squareTileXml, DateTimeOffset.Now.AddSeconds(5));

                    TileUpdater.Clear();

                    TileUpdater.EnableNotificationQueue(true);
                    TileUpdater.AddToSchedule(squareSchedule);
                    TileUpdater.AddToSchedule(wideSchedule);
                }
                catch (ArgumentException) { }
            }
        }

        private async void DeleteMenuFlyout_Click(object sender, RoutedEventArgs e)
        {
            EventManage selectedEventManage = ((MenuFlyoutItem)sender).DataContext as EventManage;
            string EventFileTitle = selectedEventManage.EventFileTitle;
            StorageFile file = await applicationFolder.GetFileAsync(EventFileTitle);
            await file.DeleteAsync();
            refreshList();

            if (SecondaryTile.Exists(EventFileTitle))
            {
                SecondaryTile secondaryTile = new SecondaryTile(EventFileTitle);
                await secondaryTile.RequestDeleteAsync();
            }
        }

        private void EditMenuFlyout_Click(object sender, RoutedEventArgs e)
        {
            EventManage selectedEventManage = ((MenuFlyoutItem)sender).DataContext as EventManage;
            Frame.Navigate(typeof(addpage), selectedEventManage);
        }
    }
}
