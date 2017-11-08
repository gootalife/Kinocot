using Kinocot.Plugin;
using Microsoft.Expression.Interactivity.Layout;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using WpfAnimatedGif;

namespace Kinocot
{
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : Window
    {
        // プラグインのコントロールを埋め込むグリッド
        private Grid[] grids;
        // ドラッグ移動を担うマン
        private MouseDragElementBehavior[] dragBehaviors;
        // プラグイン読み込み屋さん
        private PluginLorder pl;

        public MainWindow()
        {
            InitializeComponent();
            // マスコット画像の読み込み
            SetMascotImage();
            // プラグインの読み込み
            pl = new PluginLorder();
            GeneratePluginMenu(pl.plugins);
            // プラグインの起動の判定をする配列
            grids = new Grid[pl.plugins.Count];
            dragBehaviors = new MouseDragElementBehavior[pl.plugins.Count];
            // オートブートのチェック
            CheckAutoBootSetting(pl.plugins);
            // ウィンドウの位置を前回と同じ位置に設定
            Left = Properties.Settings.Default.MainWindow_Left;
            Top = Properties.Settings.Default.MainWindow_Top;
        }

        // マスコット画像を設定
        private void SetMascotImage()
        {
            var mascot = new BitmapImage();
            //　画像の読み込み
            mascot.BeginInit();
            mascot.UriSource = new Uri(Directory.GetCurrentDirectory() + @"\Resources\mascot.gif");
            mascot.EndInit();
            // デフォルトの位置に配置
            Canvas.SetLeft(Mascot, Properties.Settings.Default.Mascot_Left);
            Canvas.SetTop(Mascot, Properties.Settings.Default.Mascot_Top);
            ImageBehavior.SetAnimatedSource(Mascot, mascot);
        }

        // プラグインメニューの動的生成
        private void GeneratePluginMenu(List<IPlugin> plugins)
        {
            int count = 0;
            // プラグインの数だけ要素を追加
            foreach (IPlugin plugin in plugins)
            {
                var item = new MenuItem();
                item.Header = plugin.Name;
                // インデックスを振り分ける
                plugin.Index = count++;
                // クリックイベントを追加
                item.Click += (s, e) =>
                {
                    // クリックすると起動
                    BootPlugin(plugin);
                };
                // アイテム追加
                PluginsMenu.Items.Add(item);
            }
        }

        // プラグインのオートブート設定の確認
        void CheckAutoBootSetting(List<IPlugin> plugins)
        {
            foreach (IPlugin plugin in plugins)
            {
                // オートブートが有効なら
                if (plugin.IsAutoBoot == true)
                {
                    BootPlugin(plugin);
                }
            }
        }

        // プラグイン選択時
        private void BootPlugin(IPlugin plugin)
        {
            // プラグインが開いていないとき
            if (plugin.IsOpen == false)
            {
                // 開いた状態になる
                plugin.IsOpen = true;
                // 枠の生成
                grids[plugin.Index] = new Grid();
                // マスコットよりも上側に表示する
                // 下側だとマスコットの下に隠れた時触れなくなる
                Canvas.SetZIndex(grids[plugin.Index], 2);
                // ドラッグで移動可能にする
                dragBehaviors[plugin.Index] = new MouseDragElementBehavior();
                dragBehaviors[plugin.Index].ConstrainToParentBounds = true;
                dragBehaviors[plugin.Index].Attach(grids[plugin.Index]);
                // コンテキストメニューの生成
                var cm = new ContextMenu();
                var item = new MenuItem();
                item.Header = plugin.Name + " を閉じる";
                // クリックイベントを追加
                item.Click += (s, e) =>
                {
                    // クリックすると起動
                    plugin.CloseUserControl();
                    ClosePluginAsync(plugin);
                };
                // オートブート設定用
                var autoBoot = new MenuItem();
                // オートブートがオンのとき
                if (plugin.IsAutoBoot == true)
                {
                    autoBoot.Header = plugin.Name + " のオートブートをオフにする";
                }
                else
                {
                    autoBoot.Header = plugin.Name + " のオートブートをオンにする";
                }
                // クリックイベントを追加
                autoBoot.Click += (s, e) =>
                {
                    // クリックすると起動
                    ChangeAutoBootSetting(plugin, autoBoot);
                };
                // コンテキストメニューを設定
                cm.Items.Add(autoBoot);
                cm.Items.Add(item);
                grids[plugin.Index].ContextMenu = cm;
                // コントロールの設定
                grids[plugin.Index].Children.Add(plugin.Panel);
                // 表示
                Root.Children.Add(grids[plugin.Index]);
                // アニメーション
                PluginOpenAnimation(grids[plugin.Index], plugin, 200);
            }
            else
            {
                // 重複起動時
                MessageBox.Show(string.Format("{0} を閉じてね!", plugin.Name), "重複ダメ、絶対");
            }
        }

        // オートブートのオンオフ
        void ChangeAutoBootSetting(IPlugin plugin, MenuItem autoBoot)
        {
            if (plugin.IsAutoBoot == true)
            {
                plugin.IsAutoBoot = false;
                MessageBox.Show(plugin.Name + " のオートブートをオフにしたよ!", "設定変更");
                autoBoot.Header = plugin.Name + " のオートブートをオンにする";
            }
            else
            {
                plugin.IsAutoBoot = true;
                MessageBox.Show(plugin.Name + " のオートブートをオンにしたよ!", "設定変更");
                autoBoot.Header = plugin.Name + " のオートブートをオフにする";
            }
        }

        // プラグイングリッドのリサイズ
        private void ResizeGrid(IPlugin plugin ,Grid grid)
        {
            grid.Width = plugin.Width;
            grid.Height = plugin.Height;
            Canvas.SetLeft(grid, plugin.Left);
            Canvas.SetTop(grid, plugin.Top);
        }

        // コンテキストメニューからプラグインの退場
        private async Task ClosePluginAsync(IPlugin plugin)
        {
            // プラグインが開いている時
            if (plugin.IsOpen == true)
            {
                double left = dragBehaviors[plugin.Index].X;
                double top = dragBehaviors[plugin.Index].Y;
                // posX,posYがNaNなら修正
                if (double.IsNaN(left) == true)
                {
                    left = Canvas.GetLeft(grids[plugin.Index]);
                }
                if (double.IsNaN(top) == true)
                {
                    top = Canvas.GetTop(grids[plugin.Index]);
                }
                // 現在の位置を保存
                plugin.SavePosition(left, top);
                // 閉じている状態にする
                plugin.IsOpen = false;
                // アニメーション
                PluginCloseAnimation(grids[plugin.Index], plugin.Width, plugin.Height, Canvas.GetLeft(grids[plugin.Index]), Canvas.GetTop(grids[plugin.Index]), 200);
                // アニメーションが終わるまで待機
                await Task.Delay(200);
                // 要素を取り除きnullにする
                grids[plugin.Index].Children.RemoveAt(0);
                dragBehaviors[plugin.Index] = null;
                grids[plugin.Index] = null;
            }
        }

        // 終了処理
        private void QuitClick(object sender, RoutedEventArgs e)
        {
            // 最小化・最大化されていないときのみ設定を保存
            if (WindowState == WindowState.Normal)
            {
                // ウィンドウの値を Settings に格納
                Properties.Settings.Default.MainWindow_Left = Left;
                Properties.Settings.Default.MainWindow_Top = Top;
                // 設定を保存
                Properties.Settings.Default.Save();
            }
            Close();
        }

        // マスコットをドラッグでウィンドウ全体移動
        private void WindowDragMove(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }

        // 登場アニメーション
        private void PluginOpenAnimation(Grid grid, IPlugin plugin, double time)
        {  
            var widthAnimetion = new DoubleAnimation
            {
                From = 0,
                To = plugin.Width,
                Duration = TimeSpan.FromMilliseconds(time)
            };
            var leftAnimation = new DoubleAnimation
            {
                From = plugin.Width / 2.0 + plugin.Left,
                To = plugin.Left, 
                Duration = TimeSpan.FromMilliseconds(time)
            };
            var topAnimation = new DoubleAnimation
            {
                From = plugin.Height / 2.0 + plugin.Top,
                To = plugin.Top,
                Duration = TimeSpan.FromMilliseconds(time)
            };
            var heightAnimation = new DoubleAnimation
            {
                From = 0,
                To = plugin.Height,
                Duration = TimeSpan.FromMilliseconds(time)
            };
            // そぉぃ
            grid.BeginAnimation(WidthProperty, widthAnimetion);
            grid.BeginAnimation(LeftProperty, leftAnimation);
            grid.BeginAnimation(TopProperty, topAnimation);
            grid.BeginAnimation(HeightProperty, heightAnimation);
        }

        // 退場アニメーション
        private void PluginCloseAnimation(Grid grid, double width, double height, double left, double top, double time)
        {
            var widthAnimation = new DoubleAnimation
            {
                From = width,
                To = 0, 
                Duration = TimeSpan.FromMilliseconds(time)
            };
            var leftAnimation = new DoubleAnimation
            {
                From = left,
                To = width / 2.0 + left, 
                Duration = TimeSpan.FromMilliseconds(time)
            };
            var topAnimation = new DoubleAnimation
            {
                From = top,
                To = height / 2.0 + top,
                Duration = TimeSpan.FromMilliseconds(time)
            };
            var heightAnimation = new DoubleAnimation
            {
                From = height,
                To = 0,
                Duration = TimeSpan.FromMilliseconds(time)
            };
            // そぉぃ
            grid.BeginAnimation(WidthProperty, widthAnimation);
            grid.BeginAnimation(LeftProperty, leftAnimation);
            grid.BeginAnimation(TopProperty, topAnimation);
            grid.BeginAnimation(HeightProperty, heightAnimation);
        }
    }
}