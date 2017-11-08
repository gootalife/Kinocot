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
        private string apiKey = Properties.Settings.Default.ApiKey;

        public WeatherControl()
        {
            InitializeComponent();
            CityId.Text = Properties.Settings.Default.CityId;
            // Jsonの取得
            InitInfo();
        }

        // 3日間分の情報を取得
        private void InitInfo()
        {
            string url = $"{baseUrl}?id={cityId}&units=metric&cnt=3&appid={apiKey}";
            try
            {
                // 情報を表示
                var task = Task.Run(() =>
                {
                    string json = new HttpClient().GetStringAsync(url).Result;
                    JObject jobj = JObject.Parse(json);
                    SetWeatherInfo(jobj);
                    SaveJson(jobj);
                });
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
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
                InitWetherInfo(jobj, TodayMaxTemp, TodayMinTemp, TodayWeatherImage, url, 0);
                InitWetherInfo(jobj, TomorrowMaxTemp, TomorrowMinTemp, TomorrowWeatherImage, url, 1);
                InitWetherInfo(jobj, TdatMaxTemp, TdatMinTemp, TdatWeatherImage, url, 2);
            }));
        }

        // 情報の設定
        private void InitWetherInfo(JObject jobj, TextBlock max, TextBlock min, Image image, string url, int index)
        {
            max.Text = $"Max {(int)jobj["list"][index]["main"]["temp_max"]}℃";
            min.Text = $"Min {(int)jobj["list"][index]["main"]["temp_min"]}℃";
            string icon = (string)jobj["list"][index]["weather"][0]["icon"];
            image.Source = new BitmapImage(new Uri($"{url}{icon}.png"));
        }

        private void ResetClick(object sender, RoutedEventArgs e)
        {
            // CityIdの保存
            Properties.Settings.Default.CityId = CityId.Text;
            Properties.Settings.Default.Save();
            MessageBox.Show("地点を変更したよ!");
            // 情報の更新
            InitInfo();
        }
    }
}
