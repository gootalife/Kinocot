using CoreTweet;
using Microsoft.VisualBasic;
using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;

namespace Kinocot.Plugin.Twitter
{
    /// <summary>
    /// TwitterControl.xaml の相互作用ロジック
    /// </summary>
    public partial class TwitterControl : UserControl
    {
        private string consumerKey;
        private string consumerSecret;
        private string accessToken;
        private string accessTokenSecret;

        private Tokens tokens;
        private OAuth.OAuthSession session;

        private bool isAuthenticated;

        public TwitterControl()
        {
            InitializeComponent();
            consumerKey = Properties.Settings.Default.ConsumerKey;
            consumerSecret = Properties.Settings.Default.ConsumerSecret;
            accessToken = Properties.Settings.Default.AccessToken;
            accessTokenSecret = Properties.Settings.Default.AccessTokenSecret;
            // APIKeyが用意されていたらトークンの取得を試みる
            if (!string.IsNullOrEmpty(consumerKey) && !string.IsNullOrEmpty(consumerSecret) && !string.IsNullOrEmpty(accessToken) && !string.IsNullOrEmpty(accessTokenSecret))
            {
                try
                {
                    Task.Run(() =>
                    {
                        // トークン生成
                        tokens = Tokens.Create(consumerKey, consumerSecret, accessToken, accessTokenSecret);
                        ChangeTweetMode();
                    });
                }
                catch
                {
                    MessageBox.Show("自動ログインに失敗しました。再設定が必要です。", "自動ログイン失敗", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else
            {
                ChangeAuthenticationMode();
            }
        }

        // ツイートボタン
        private void TweetButtonClick(object sender, RoutedEventArgs e)
        {
            Tweet();
        }

        // CTRL + ENTER でTweet
        private void TweetTextKeyDown(object sender, KeyEventArgs e)
        {
            if (Keyboard.Modifiers == ModifierKeys.Control && Keyboard.IsKeyDown(Key.Enter))
            {
                Tweet();
            }
        }

        // ツイートの入力確定
        private void Tweet()
        {
            // 認証されているかどうか
            if (isAuthenticated == true)
            {
                // ツイート
                string text = TweetText.Text;
                if (text == "")
                {
                    return;
                }
                else if (text.Length <= 280)
                {
                    tokens.Statuses.UpdateAsync(status => text);
                    TweetText.Text = "";
                }
                else
                {
                    MessageBox.Show("文字数が280文字を超えています。", "文字数オーバー", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else
            {
                // ログイン処理
                Authenticate();
            }
        }

        // 認証手順と保存
        private void Authenticate()
        {
            try
            {
                // APIKeyの設定
                consumerKey = Interaction.InputBox("ConsumerKeyを入力", "APIKey設定", consumerKey, -1, -1);
                consumerSecret = Interaction.InputBox("ConsumerSecretを入力", "APIKey設定", consumerSecret, -1, -1);
                // 認証用のURL
                session = OAuth.Authorize(consumerKey, consumerSecret);
                var url = session.AuthorizeUri;
                // ブラウザを起動
                System.Diagnostics.Process.Start(url.ToString());
                isAuthenticated = false;
                Properties.Settings.Default.ConsumerKey = consumerKey;
                Properties.Settings.Default.ConsumerSecret = consumerSecret;
                Properties.Settings.Default.Save();
            }
            catch
            {
                MessageBox.Show("APIKeyの設定に誤りがあります。", "PINコード発行失敗", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            // 取得
            try
            {
                // PINコードの入力
                var PINCode = "";
                PINCode = Interaction.InputBox("PINコードを入力", "認証設定", "", -1, -1);
                tokens = OAuth.GetTokens(session, PINCode);
                // トークンを保存
                accessToken = tokens.AccessToken;
                accessTokenSecret = tokens.AccessTokenSecret;
                Properties.Settings.Default.AccessToken = accessToken;
                Properties.Settings.Default.AccessTokenSecret = accessTokenSecret;
                Properties.Settings.Default.Save();
                MessageBox.Show("認証設定を保存しました。", "ログイン成功");
                TweetText.Text = "";
                // トークン生成
                tokens = Tokens.Create(consumerKey, consumerSecret,
                                        accessToken, accessTokenSecret);
                ChangeTweetMode();
            }
            catch
            {
                MessageBox.Show("入力エラーです。", "入力値エラー", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // 認証のリセット
        private void Reset_Click(object sender, RoutedEventArgs e)
        {
            if (isAuthenticated == true)
            {
                // トークンをリセット
                Properties.Settings.Default.AccessToken = null;
                Properties.Settings.Default.AccessTokenSecret = null;
                Properties.Settings.Default.Save();
                MessageBox.Show("ログアウトしました。", "ログアウト");
                ChangeAuthenticationMode();
            }
            else
            {
                MessageBox.Show("ログインしていません。", "ログアウト失敗", MessageBoxButton.OK, MessageBoxImage.Exclamation);
            }
        }

        // 認証モード
        private void ChangeAuthenticationMode()
        {
            isAuthenticated = false;
            TweetButton.Content = "Login";
            Navi.Text = "Input PIN code";
            TweetText.Text = "";
        }

        // ツイートモード
        private void ChangeTweetMode()
        {
            Dispatcher.BeginInvoke(new Action(() =>
            {
                isAuthenticated = true;
                TweetText.Text = "";
                var profile = tokens.Account.VerifyCredentials();
                TweetButton.Content = "Tweet";
                Navi.Text = $"{profile.Name}@{profile.ScreenName}";
            }), null);
        }
    }
}
