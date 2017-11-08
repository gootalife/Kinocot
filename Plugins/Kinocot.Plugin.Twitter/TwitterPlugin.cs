using System.Windows.Controls;

namespace Kinocot.Plugin.Twitter
{
    public class TwitterPlugin : IPlugin
    {
        TwitterControl control;
        // プラグインの名前を返す
        public string Name
        {
            get
            {
                return "Twitter";
            }
        }
        // コントロールを返す
        public UserControl Panel
        {
            get
            {
                control = new TwitterControl();
                return control;
            }
        }
        // 自動で起動するかどうか
        public bool IsAutoBoot
        {
            get
            {
                return Properties.Settings.Default.AutoBoot;
            }
            set
            {
                Properties.Settings.Default.AutoBoot = value;
                Properties.Settings.Default.Save();
            }
        }
        // 起動しているかどうか
        public bool IsOpen { get; set; }
        // プラグイン毎のコントロールのWidthを返す
        public double Width
        {
            get
            {
                return Properties.Settings.Default.Width;
            }
        }
        // プラグイン毎のコントロールのHeightを返す
        public double Height
        {
            get
            {
                return Properties.Settings.Default.Height;
            }
        }
        // プラグイン毎のコントロールのLeftを返す
        public double Left
        {
            get
            {
                return Properties.Settings.Default.Left;
            }
        }
        // プラグイン毎のコントロールのTopを返す
        public double Top
        {
            get
            {
                return Properties.Settings.Default.Top;
            }
        }
        // プラグイン毎に振られる番号
        public int Index { get; set; }
        // プラグインの位置を設定に保存
        public void SavePosition(double left, double top)
        {
            Properties.Settings.Default.Left = left;
            Properties.Settings.Default.Top = top;
            Properties.Settings.Default.Save();
        }
        // 閉じるときの処理
        public void CloseUserControl() { }
    }
}
