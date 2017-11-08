using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows;

namespace Kinocot.Plugin.Memo
{
    public class MemoManager
    {
        private string path = @"Resources\memo.txt";
        public List<string> datas = new List<string>();

        public MemoManager()
        {
            datas = new List<string>();
            LoadMemo();
        }

        // テキストの読み込み
        void LoadMemo()
        {
            // テキストをロード
            using (var sr = new StreamReader(path, Encoding.GetEncoding("UTF-8")))
            {
                while (sr.Peek() > -1)
                {
                    string line = sr.ReadLine();
                    datas.Add(line);
                }
            }
        }

        // テキストの保存
        public void SaveMemo(List<string> datas)
        {
            using (var sw = new StreamWriter(new FileStream(path, FileMode.Create)))
            {
                foreach (string data in datas)
                {
                    sw.WriteLine(data);
                }
            }
        }
    }
}
