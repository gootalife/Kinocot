using Microsoft.VisualBasic;
using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace Kinocot.Plugin.Weather
{
    /// <summary>
    /// WeatherControl.xaml の相互作用ロジック
    /// </summary>
    public partial class WeatherControl : UserControl
    {
        private string baseUrl = "http://api.openweathermap.org/data/2.5/forecast";
        private string cityId = Properties.Settings.Default.CityId;
        private string APIKey = Properties.Settings.Default.APIKey;

        public WeatherControl()
        {
            InitializeComponent();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            APIKey = Interaction.InputBox("APIKeyを入力\nAPIKeyの取得はhttp://openweathermap.org/", "APIKey設定", APIKey, -1, -1);
            var dtToday = DateTime.Today;
            Today.Text = dtToday.ToString("MM/dd");
            Tomorrow.Text = dtToday.AddDays(1).ToString("MM/dd");
            Tdat.Text = dtToday.AddDays(2).ToString("MM/dd");
            CityId.Text = Properties.Settings.Default.CityId;
            // Jsonの取得
            InitInfo();
        }

        // 3日間分の情報を取得
        private async void InitInfo()
        {
            try
            {
                string url = $"{baseUrl}?id={cityId}&units=metric&cnt=3&appid={APIKey}";
                // 情報を表示
                await Task.Run(() =>
                {
                    string json = new HttpClient().GetStringAsync(url).Result;
                    JObject jobj = JObject.Parse(json);
                    SetWeatherInfo(jobj);
                    SaveJson(jobj);
                });
                Properties.Settings.Default.APIKey = APIKey;
                Properties.Settings.Default.Save();
            }
            catch
            {
                MessageBox.Show("APIKeyの設定に誤りがあります。", "APIKeyエラー", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            
        }

        // Jsonの保存
        private void SaveJson(JObject jobj)
        {
            using (var json = new StreamWriter(new FileStream(@"Resources\weather.json", FileMode.Create)))
            {
                json.Write(jobj);
            }
        }

        // 天気の表示
        private void SetWeatherInfo(JObject jobj)
        {
            Dispatcher.Invoke(new Action(() =>
            {
                string url = "http://openweathermap.org/img/w/";
                try
                {
                    InitWetherInfo(jobj, TodayMaxTemp, TodayMinTemp, TodayWeatherImage, url, 0);
                    InitWetherInfo(jobj, TomorrowMaxTemp, TomorrowMinTemp, TomorrowWeatherImage, url, 1);
                    InitWetherInfo(jobj, TdatMaxTemp, TdatMinTemp, TdatWeatherImage, url, 2);
                }
                catch
                {
                    MessageBox.Show("情報が出しく所得できませんでした", "取得情報エラー", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }));
        }

        // 情報の設定
        private void InitWetherInfo(JObject jobj, TextBlock max, TextBlock min, Image image, string url, int index)
        {
            max.Text = $"Max {(int)jobj["list"][index]["main"]["temp_max"]}°C";
            min.Text = $"Min {(int)jobj["list"][index]["main"]["temp_min"]}°C";
            string icon = (string)jobj["list"][index]["weather"][0]["icon"];
            image.Source = new BitmapImage(new Uri($"{url}{icon}.png"));
        }

        private void ResetClick(object sender, RoutedEventArgs e)
        {
            // CityIdの保存
            Properties.Settings.Default.CityId = CityId.Text;
            Properties.Settings.Default.Save();
            // 情報の更新
            InitInfo();
        }
    }
}
