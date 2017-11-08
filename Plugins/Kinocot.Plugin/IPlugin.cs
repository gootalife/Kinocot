using System.Windows.Controls;

namespace Kinocot.Plugin
{
    public interface IPlugin
    {
        // プラグインの名前を返す
        string Name { get; }
        // コントロールを返す
        UserControl Panel { get; }
        // 自動で起動するかどうか
        bool IsAutoBoot { get; set; }
        // 起動しているかどうか
        bool IsOpen { get; set; }
        // プラグイン毎のコントロールのHeightを返す
        double Height { get; }
        // プラグイン毎のコントロールのWidthを返す
        double Width { get; }
        // プラグイン毎のコントロールのLeftを返す
        double Left { get; }
        // プラグイン毎のコントロールのTopを返す
        double Top { get; }
        // プラグイン毎に振られる番号
        int Index { get; set; }
        // プラグインの位置を設定に保存
        void SavePosition(double left, double top);
        // 閉じるときの処理
        void CloseUserControl();
    }
}
