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

int LineCnt = 0;
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
                        Console.WriteLine(LogDataLine.Substring(0,4) + "-" + LogDataLine.Substring(5,2) + "-" + LogDataLine.Substring(8,2) + "_" + LogDataLine.Substring(11,8) + " [Join] " + string.Format("[{0,2}] ", NumberOfPlayer) + LogDataLine.Substring(61));
                        JoinData = string.Concat(JoinData, "[" + LogDataLine.Substring(61) + "]");
                        JoinNotification = 1;
                        OscParameter.SendAvatarParameter("nsfw", false);
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
                    LineCnt++;
                }
                LineCntFile++;
            }
            int NowTime;
            int.TryParse(DateTime.Now.ToString("mm"), out NowTime);
            if (NowTime == 00)
            {
                if(TimeNotification == 0)
                {
                    TimeNotification++;
                    new XSNotifier().SendNotification(new XSNotification() { Title = DateTime.Now.ToString("HHmm"), Content = "現在の時間は" + DateTime.Now.ToString("HH時mm分") + "です" });
                }
            }
            if (JoinNotification == 1)
            {
                new XSNotifier().SendNotification(new XSNotification() { Title = "Join [" + NumberOfPlayer.ToString() + "]", Content = JoinData });
                if (NumberOfPlayer > 20)
                {
                    new XSNotifier().SendNotification(new XSNotification() { Title = "There are over 20 people in this instance.", Content = NumberOfPlayer.ToString() });
                }
            }
            if(LeftNotification == 1)
            {
                new XSNotifier().SendNotification(new XSNotification() { Title = "Left ["+NumberOfPlayer.ToString()+"]", Content = LeftData });
                if (NumberOfPlayer > 20)
                {
                    new XSNotifier().SendNotification(new XSNotification() { Title = "There are over 20 people in this instance.", Content = NumberOfPlayer.ToString() });
                }
            }
            if (UrlNotification == 1)
            {
                new XSNotifier().SendNotification(new XSNotification() { Title = "URL", Content = UrlData, }) ;
            }

        }
    }
    Thread.Sleep(100);
}
