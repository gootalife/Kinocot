using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Windows;
using System.Windows.Controls;

namespace Kinocot.Plugin.Memo
{
    /// <summary>
    /// MemoControl.xaml の相互作用ロジック
    /// </summary>
    public partial class MemoControl : UserControl
    {
        private MemoManager mm;

        public MemoControl()
        {
            InitializeComponent();
            mm = new MemoManager();
            AddToList();
        }

        // リストボックスにメモの内容を追加
        private void AddToList()
        {
            foreach (string data in mm.datas)
            {
                Memo_ListBox.Items.Add(data);
            }
        }

        // テキストボックスの内容をリストボックスに追加
        private void AddItem(object sender, RoutedEventArgs e)
        {
            string text = Memo_TextBox.Text;
            // 空欄でないなら追加
            if (text != "")
            {
                Memo_ListBox.Items.Add(text);
                Memo_TextBox.Text = "";
            }
            List<string> datas = ConvertFromListBoxToList();
            // 保存
            mm.SaveMemo(datas);
        }

        // リストボックスの選択項目を削除
        private void DeleteItem(object sender, RoutedEventArgs e)
        {
            Memo_ListBox.Items.Remove(Memo_ListBox.SelectedItem);
            List<string> datas = ConvertFromListBoxToList();
            // 保存
            mm.SaveMemo(datas);
        }

        // リストボックスの内容を取得
        private List<string> ConvertFromListBoxToList()
        {
            var datas = new List<string>();
            foreach (string data in Memo_ListBox.Items)
            {
                datas.Add(data);
            }
            return datas;
        }
    }
}
