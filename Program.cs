using System;
using System.IO;
using System.Text;
using System.Collections.Generic;

public class Hello
{
    public static void Main()
    {
        // 初期化
        int line_cnt = 0;
        string line;
        List<string> mylist = new List<string>();

        // ファイルを開く
        while (true)
        {
            int file_line_cnt = 0;
            using (StreamReader sr = new StreamReader("C:\\Users\\zyano\\vrclog.txt"))
            {
                // ファイルの内容を1行ずつ読み込み
                while ((line = sr.ReadLine()) != null)
                {
                    if (line_cnt <= file_line_cnt)
                    {
                        line_cnt++;
                        if (line.Contains("[Behaviour] OnPlayerJoined"))
                        {
                            Console.WriteLine(line);
                        }
                        else if (line.Contains("[Behaviour] OnPlayerLeftRoom"))
                        {
                        }
                        else if (line.Contains("[Behaviour] OnPlayerLeft"))
                        {
                            Console.WriteLine(line);
                        }
                    }
                    file_line_cnt++;
                }
            }
            Thread.Sleep(100);
        }
    }
}