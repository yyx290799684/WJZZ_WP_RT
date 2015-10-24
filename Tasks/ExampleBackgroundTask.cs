using System;
using System.Diagnostics;
using System.IO;
using Windows.ApplicationModel.Background;
using Windows.Data.Xml.Dom;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.Notifications;

namespace Tasks
{
    public sealed class ExampleBackgroundTask : IBackgroundTask
    {
        private ApplicationDataContainer appSetting;
        private string tileId;
        public async void Run(IBackgroundTaskInstance taskInstance)
        {
            appSetting = ApplicationData.Current.LocalSettings; //本地存储

            BackgroundTaskDeferral deferral = taskInstance.GetDeferral();

            IStorageFolder applicationFolder = ApplicationData.Current.LocalFolder;
            var files = await applicationFolder.GetFilesAsync();
            int[] kk;
            foreach (StorageFile file in files)
            {
                tileId = file.Name.ToString();
                if (appSetting.Values.ContainsKey(tileId) == false)
                    appSetting.Values[tileId] = false;

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


                //当日发送toast
                if (kk[0] == 0 && !(bool)appSetting.Values[tileId])
                {
                    XmlDocument toastXml = ToastNotificationManager.GetTemplateContent(ToastTemplateType.ToastImageAndText02);
                    XmlNodeList elements = toastXml.GetElementsByTagName("text");
                    elements[0].AppendChild(toastXml.CreateTextNode(tileId));
                    elements[1].AppendChild(toastXml.CreateTextNode("今日完成"));
                    ToastNotification toast = new ToastNotification(toastXml);
                    ToastNotificationManager.CreateToastNotifier().Show(toast);
                    appSetting.Values[tileId] = true;
                }


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


                    var TileUpdater = TileUpdateManager.CreateTileUpdaterForSecondaryTile(tileId);
                    ScheduledTileNotification wideSchedule = new ScheduledTileNotification(wideTileXml, DateTimeOffset.Now.AddSeconds(5));
                    ScheduledTileNotification squareSchedule = new ScheduledTileNotification(squareTileXml, DateTimeOffset.Now.AddSeconds(5));
                    TileUpdater.Clear();

                    TileUpdater.EnableNotificationQueue(true);
                    TileUpdater.AddToSchedule(squareSchedule);
                    TileUpdater.AddToSchedule(wideSchedule);
                }
                catch (Exception) { }
            }

            deferral.Complete();
        }
    }
}
