using System;
using System.IO;
using System.Text;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Collections.Generic;

using XSNotifications;
using XSNotifications.Enum;

using BuildSoft.VRChat.Osc;


// 指定されたフォルダ内のdatファイル名をすべて取得する
string[] files = System.IO.Directory.GetFiles(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + "\\AppData\\LocalLow\\VRChat\\VRChat", "*.txt", System.IO.SearchOption.TopDirectoryOnly);
var JNTdataLink = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + "\\JoinNotificater.txt";
string newestFileName = string.Empty;
System.DateTime updateTime = System.DateTime.MinValue;
foreach (string file in files)
{
    // それぞれのファイルの更新日付を取得する
    System.IO.FileInfo fi = new System.IO.FileInfo(file);
    // 更新日付が最新なら更新日付とファイル名を保存する
    if (fi.LastWriteTime > updateTime)
    {
        updateTime = fi.LastWriteTime;
        newestFileName = file;
    }
}

int LineCnt = 1;
int TimeNotification = 0;
int NumberOfPlayer = 0;
while (true)
{
    LineCnt--;
    using (var fs = File.Open(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + "\\AppData\\LocalLow\\VRChat\\VRChat\\" + System.IO.Path.GetFileName(newestFileName), FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
    {
        using (TextReader reader = new StreamReader(fs, Encoding.GetEncoding("UTF-8")))
        {
            int LineCntFile = 0;
            int JoinNotification = 0;
            int LeftNotification = 0;
            int UrlNotification = 0;
            string JoinData = "";
            string LeftData = "";
            string UrlData = "";
            string[] LogData = reader.ReadToEnd().Split('\n');
            foreach (string LogDataLine in LogData)
            {
                if (LineCnt == LineCntFile)
                {
                    if (LogDataLine.Contains("[Behaviour] OnPlayerJoined"))
                    {
                        NumberOfPlayer++;
                        var usercomment = "none";
                        Console.WriteLine(LogDataLine.Substring(0, 4) + "-" + LogDataLine.Substring(5, 2) + "-" + LogDataLine.Substring(8, 2) + "_" + LogDataLine.Substring(11, 8) + " [Join] " + string.Format("[{0,2}] ", NumberOfPlayer) + LogDataLine.Substring(61));
                        JoinData = string.Concat(JoinData, "[" + LogDataLine.Substring(61) + "]");
                        JoinNotification = 1;
                        //OscParameter.SendAvatarParameter("nsfw", false);
                        /*
                        if(File.Exists(JNTdataLink)) 
                        {
                            using (var userdata = File.Open(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + "\\JoinNotificater.txt", FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                            {
                                using (TextReader userdatatxt = new StreamReader(userdata, Encoding.GetEncoding("UTF-8")))
                                {
                                    string[] udt = userdatatxt.ReadToEnd().Split('\n');
                                    foreach (string udtl in udt)
                                    {
                                        if (udtl != null)
                                        {
                                            int len = LogDataLine.Substring(61).Length;
                                            if (udtl.Length > len)
                                            {
                                                if (udtl.Substring(0, len) == LogDataLine.Substring(61))
                                                {
                                                    usercomment = udtl.Substring(len + 1);
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                            if(usercomment == "none")
                            {
                                using (var writer = new StreamWriter(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + "\\JoinNotificater.txt", true))
                                {
                                    writer.WriteLine(LogDataLine.Substring(61) + ",");
                                }
                            }
                        }
                        else
                        {
                            using (File.Create(JNTdataLink))
                            {

                            }
                            using (var writer = new StreamWriter(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + "\\JoinNotificater.txt", true))
                            {
                                writer.WriteLine(LogDataLine.Substring(61) + ",");
                            }
                        }
                        if (usercomment == "none")
                        {
                            new XSNotifier().SendNotification(new XSNotification() { Title = "[" + NumberOfPlayer.ToString() + "]Join [" + LogDataLine.Substring(61) + "]", Content = "No data.", Timeout = (float)0.3 });
                        }
                        else
                        {
                            new XSNotifier().SendNotification(new XSNotification() { Title = "[" + NumberOfPlayer.ToString() + "]Join [" + LogDataLine.Substring(61) + "]", Content = usercomment, Timeout = (float)0.3 });
                        }
                        { */
                    }
                    else if (LogDataLine.Contains("[Behaviour] OnPlayerLeftRoom"))
                    {
                    }
                    else if (LogDataLine.Contains("[Behaviour] OnPlayerLeft"))
                    {
                        NumberOfPlayer--;
                        Console.WriteLine(LogDataLine.Substring(0, 4) + "-" + LogDataLine.Substring(5, 2) + "-" + LogDataLine.Substring(8, 2) + "_" + LogDataLine.Substring(11, 8) + " [Left] " + string.Format("[{0,2}] ", NumberOfPlayer) + LogDataLine.Substring(59));
                        LeftData = string.Concat(LeftData, "[" + LogDataLine.Substring(59) + "]");
                        LeftNotification = 1;
/* new XSNotifier().SendNotification(new XSNotification() { Title = "Left [" + NumberOfPlayer.ToString() + "][" + LogDataLine.Substring(59) + "]", Timeout = (float)0.2 });*/
                    }
                    else if (LogDataLine.Contains("Joining or Creating Room"))
                    {
                        NumberOfPlayer = 0;
                    }
                    else if (LogDataLine.Contains("Attempting to resolve URL"))
                    {
                        Console.WriteLine(LogDataLine.Substring(0, 4) + "-" + LogDataLine.Substring(5, 2) + "-" + LogDataLine.Substring(8, 2) + "_" + LogDataLine.Substring(11, 8) + " [URL_] " + LogDataLine.Substring(77));
                        UrlData = string.Concat(UrlData, LogDataLine.Substring(77));
                        UrlNotification = 1;
                    }
                    else if (LogDataLine.Contains("[VRC Camera] Took screenshot to"))
                    {
                        if (LogDataLine.Contains("png")){
                            Console.WriteLine(LogDataLine.Substring(67));
                            using (var httpClient = new HttpClient())
                            {
                                using (var request = new HttpRequestMessage(new HttpMethod("POST"), "http://192.168.0.22/vrc_log.php"))
                                {
                                    File.Copy(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + "\\AppData\\LocalLow\\VRChat\\VRChat\\" + System.IO.Path.GetFileName(newestFileName), Path.GetTempPath() + "log.txt", true);
                                    var multipartContent = new MultipartFormDataContent();
                                    multipartContent.Add(new ByteArrayContent(File.ReadAllBytes(Path.GetTempPath() + "log.txt")), "log", Path.GetFileName(Path.GetTempPath() + "log.txt"));
                                    multipartContent.Add(new ByteArrayContent(File.ReadAllBytes(LogDataLine.Substring(67))), "file", Path.GetFileName(LogDataLine.Substring(67)));
                                    request.Content = multipartContent;

                                    var response = await httpClient.SendAsync(request);
                                }
                            }
                        }
                    }
                    LineCnt++;
                }
                LineCntFile++;
            }
            int NowTime;
            int.TryParse(DateTime.Now.ToString("mm"), out NowTime);
            if (NowTime == 00)
            {
                if (TimeNotification == 0)
                {
                    TimeNotification++;
                    new XSNotifier().SendNotification(new XSNotification() { Title = DateTime.Now.ToString("HHmm"), Content = "現在の時間は" + DateTime.Now.ToString("HH時mm分") + "です" });
                }
            }
            if (JoinNotification == 1)
            {
                new XSNotifier().SendNotification(new XSNotification() { Title = "Join [" + NumberOfPlayer.ToString() + "]", Content = JoinData });

            }
            if (LeftNotification == 1)
            {
                new XSNotifier().SendNotification(new XSNotification() { Title = "Left [" + NumberOfPlayer.ToString() + "]", Content = LeftData });
            }
            if (UrlNotification == 1)
            {
                new XSNotifier().SendNotification(new XSNotification() { Title = "URL", Content = UrlData, });
            }

        }
    }
    Thread.Sleep(100);
}