using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace Kinocot.Plugin.Timer
{
    /// <summary>
    /// TimerControl.xaml の相互作用ロジック
    /// </summary>
    public partial class TimerControl : UserControl
    {
        DispatcherTimer timer;
        private int count = 0;

        public TimerControl()
        {
            InitializeComponent();
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
            t.Tick += (s, e) =>
            {
                if (count > 0)
                {
                    // カウントの更新
                    count--;
                    CountMinute.Text = (count / 60).ToString().PadLeft(2, '0');
                    CountSecond.Text = (count % 60).ToString().PadLeft(2, '0');
                }
                else
                {
                    // 更新のストップ
                    t.Stop();
                    // 通知の設定
                    var notifyIcon = new System.Windows.Forms.NotifyIcon
                    {
                        Icon = Properties.Resources.Icon,
                        Visible = true,
                        BalloonTipTitle = "タイマー",
                        BalloonTipText = "ピピピピピピピピピピ!",
                        BalloonTipIcon = System.Windows.Forms.ToolTipIcon.Info
                    };
                    notifyIcon.ShowBalloonTip(5000);
                    var delay = new DispatcherTimer(DispatcherPriority.SystemIdle)
                    {
                        Interval = TimeSpan.FromMilliseconds(7500),
                    };
                    delay.Tick += (se, ev) =>
                    {
                        notifyIcon.Visible = false;
                        delay.Stop();
                    };
                    delay.Start();
                }
            };
            // 生成したタイマーを返す
            return t;
        }

        // 数字の増減
        private void MinuteUpButtonClick(object sender, RoutedEventArgs e)
        {
            var num = int.Parse(SettingMinute.Text);
            if (num < 99)
            {
                num++;
                SettingMinute.Text = num.ToString().PadLeft(2, '0');
            }
        }

        private void MinuteDownBottonClick(object sender, RoutedEventArgs e)
        {
            var num = int.Parse(SettingMinute.Text);
            if (num > 0)
            {
                num--;
                SettingMinute.Text = num.ToString().PadLeft(2, '0');
            }
        }

        private void SecondUpBottonClick(object sender, RoutedEventArgs e)
        {
            var num = int.Parse(SettingSecond.Text);
            if (num < 58)
            {
                num++;
                SettingSecond.Text = num.ToString().PadLeft(2, '0');
            }
        }

        private void SecondDownBottonClick(object sender, RoutedEventArgs e)
        {
            var num = int.Parse(SettingSecond.Text);
            if (num > 0)
            {
                num--;
                SettingSecond.Text = num.ToString().PadLeft(2, '0');
            }
        }

        // タイマーのセット
        private void SetButtonClick(object sender, RoutedEventArgs e)
        {
            // カウントを設定
            CountMinute.Text = SettingMinute.Text.PadLeft(2, '0');
            CountSecond.Text = SettingSecond.Text.PadLeft(2, '0');
            count = int.Parse(SettingMinute.Text) * 60 + int.Parse(SettingSecond.Text);
            SettingMinute.Text = "00";
            SettingSecond.Text = "00";
            // 処理による遅延を考慮
            timer = CreateTimer(985);
        }
        
        // タイマーのスタート
        private void StartButtonClick(object sender, RoutedEventArgs e)
        {
            if (count > 0)
            {
                timer.Start();
            }
        }

        // タイマーのリセット
        private void ResetButtonClick(object sender, RoutedEventArgs e)
        {
            SettingMinute.Text = "00";
            SettingSecond.Text = "00";
            CountMinute.Text = "00";
            CountSecond.Text = "00";
            count = 0;
            if (timer != null)
            {
                timer.Stop();
            }
        }

        // 閉じるときにリセットする
        public void Close()
        {
            SettingMinute.Text = "00";
            SettingSecond.Text = "00";
            CountMinute.Text = "00";
            CountSecond.Text = "00";
            count = 0;
            if (timer != null)
            {
                timer.Stop();
            }
        }
    }
}
