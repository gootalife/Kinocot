using System;
using System.Windows.Controls;
using System.Windows.Threading;

namespace Kinocot.Plugin.Clock
{
    /// <summary>
    /// ClockControl.xaml の相互作用ロジック
    /// </summary>
    public partial class ClockControl : UserControl
    {
        private DispatcherTimer timer;

        public ClockControl()
        {
            InitializeComponent();
            Date.Text = "";
            // タイマーイベントの発生間隔を設定
            timer = CreateTimer(100);
            timer.Start();
        }

        // 時計用タイマー生成
        private DispatcherTimer CreateTimer(double time)
        {
            // タイマー生成（優先度はアイドル時に設定）
            var t = new DispatcherTimer(DispatcherPriority.SystemIdle)
            {
                // タイマーイベントの発生間隔を設定
                Interval = TimeSpan.FromMilliseconds(time),
            };
            // タイマーイベントの定義
            t.Tick += (s, e) => {
                // 現在の時分秒をテキストに設定
                Date.Text = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss");
            };
            // 生成したタイマーを返す
            return t;
        }
    }
}
