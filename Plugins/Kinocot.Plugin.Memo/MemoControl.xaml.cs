using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace Kinocot.Plugin.Memo
{
    /// <summary>
    /// MemoControl.xaml の相互作用ロジック
    /// </summary>
    public partial class MemoControl : UserControl;
{
        public MemoControl()
        {
            InitializeComponent();
            var memo = Properties.Settings.Default.Memo.Cast<string>().ToList();
            if (memo != null)
            {
                // リストボックスにメモの内容を追加
                foreach (var line in memo)
                {
                    MemoListBox.Items.Add(line);
                }
            }
        }

        // テキストボックスの内容をリストボックスに追加
        private void AddNewItem(object sender, RoutedEventArgs e)
        {
            // 空欄でないなら追加
            if (MemoTextBox.Text != "")
            {
                MemoListBox.Items.Add(MemoTextBox.Text);
                MemoTextBox.Text = "";
            }
            // 保存
            var sc = new StringCollection();
            sc.AddRange(MemoListBox.Items.Cast<string>().ToArray());
            Properties.Settings.Default.Memo = sc;
            Properties.Settings.Default.Save();
        }

        // リストボックスの選択項目を削除
        private void DeleteItem(object sender, RoutedEventArgs e)
        {
            MemoListBox.Items.Remove(MemoListBox.SelectedItem);
            // 保存
            var sc = new StringCollection();
            sc.AddRange(MemoListBox.Items.Cast<string>().ToArray());
            Properties.Settings.Default.Memo = sc;
            Properties.Settings.Default.Save();
        }
    }
}
