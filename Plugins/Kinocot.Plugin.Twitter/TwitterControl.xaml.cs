using CoreTweet;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

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

        private bool isAvailable;

        public TwitterControl()
        {
            InitializeComponent();
            consumerKey = Properties.Settings.Default.ConsumerKey;
            consumerSecret = Properties.Settings.Default.ConsumerSecret;
            accessToken = Properties.Settings.Default.AccessToken;
            accessTokenSecret = Properties.Settings.Default.AccessTokenSecret;
            var task = Task.Run(() =>
            {
                // 認証されていない時
                if (string.IsNullOrEmpty(accessToken) || string.IsNullOrEmpty(accessTokenSecret))
                {
                    ChangeAuthenticationMode();
                }
                else
                {
                    // トークン生成
                    tokens = Tokens.Create(Properties.Settings.Default.ConsumerKey, Properties.Settings.Default.ConsumerSecret,
                                            Properties.Settings.Default.AccessToken, Properties.Settings.Default.AccessTokenSecret);
                    ChangeTweetMode();
                }
            });
        }

        private void TweetButtonClick(object sender, RoutedEventArgs e)
        {
            // 認証されているかどうか
            if (isAvailable == true)
            {
                // ツイート
                string text = TweetText.Text;
                if (text == "")
                {
                    return;
                }
                else if (text.Length <= 140)
                {
                    tokens.Statuses.UpdateAsync(new Dictionary<string, object>() { { "status", text } });
                    TweetText.Text = "";
                } else
                {
                    MessageBox.Show("文字数が多すぎだよ!");
                }
            }
            else
            {
                // PINコードの入力確定
                Authenticate();
            }
        }

        // 認証手順と保存
        private void Authenticate()
        {
            // 取得
            string pinCode = TweetText.Text;
            try
            {
                tokens = OAuth.GetTokens(session, pinCode);
                // トークンを保存
                Properties.Settings.Default.AccessToken = tokens.AccessToken.ToString();
                Properties.Settings.Default.AccessTokenSecret = tokens.AccessTokenSecret.ToString();
                Properties.Settings.Default.Save();
                MessageBox.Show("認証設定を保存したよ!");
                TweetText.Text = "";
                // トークン生成
                tokens = Tokens.Create(Properties.Settings.Default.ConsumerKey, Properties.Settings.Default.ConsumerSecret,
                                        Properties.Settings.Default.AccessToken, Properties.Settings.Default.AccessTokenSecret);
                ChangeTweetMode();
            }
            catch
            {
                MessageBox.Show("入力エラーだよ!");
                TweetText.Text = "";
            }
        }

        private void ResetClick(object sender, RoutedEventArgs e)
        {
            isAvailable = false;
            // トークンをリセット
            Properties.Settings.Default.AccessToken = null;
            Properties.Settings.Default.AccessTokenSecret = null;
            Properties.Settings.Default.Save();
            MessageBox.Show("認証設定をリセットしたよ!");
            ChangeAuthenticationMode();
        }

        private void ChangeAuthenticationMode()
        {
            Dispatcher.Invoke(new Action(() =>
            {
                isAvailable = false;
                TweetButton.Content = "認証";
                Navi.Text = "PINコードを入力してね!";
                // 認証用のURL
                session = OAuth.Authorize(consumerKey, consumerSecret);
                Uri url = session.AuthorizeUri;
                // ブラウザを起動
                System.Diagnostics.Process.Start(url.ToString());
            }));
            
        }

        private void ChangeTweetMode()
        {
            var task = Task.Run(() =>
            {
                isAvailable = true;
                UserResponse profile = tokens.Account.VerifyCredentials();
                Dispatcher.Invoke(new Action(() =>
                {
                    TweetButton.Content = "ツイート";
                    Navi.Text = $"{profile.Name}@{profile.ScreenName}";
                }));
            });

        }
    }
}
