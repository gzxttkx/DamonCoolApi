using System;
using System.Collections.Generic;
using WeiXinService.Utils;

namespace WeiXinWebApi2.Utils
{
    /// <summary>
    /// 天气工具类
    /// </summary>
    public class WeatherUtil
    {
        #region 获取天气信息
        /// <summary>
        /// 获取天气信息
        /// </summary>
        public static List<Dictionary<string, string>> GetWeatherInfo()
        {
            List<Dictionary<string, string>> result = new List<Dictionary<string, string>>();
            string weatherJson = HttpRequestUtil.RequestUrl("http://www.weather.com.cn/data/sk/101220101.html", "GET");

            Dictionary<string, string> dict = new Dictionary<string, string>();
            dict["Title"] = Tools.GetJsonValue(weatherJson, "city") + "天气预报 " + DateTime.Now.ToString("yyyy年M月d日");
            dict["Description"] = "";
            dict["PicUrl"] = "";
            dict["Url"] = "";
            result.Add(dict);

            dict = new Dictionary<string, string>();
            dict["Title"] = string.Format("温度：{0}℃ 湿度：{1} 风速：{2}{3}级", Tools.GetJsonValue(weatherJson, "temp"),
                Tools.GetJsonValue(weatherJson, "SD"),
                Tools.GetJsonValue(weatherJson, "WD"),
                Tools.GetJsonValue(weatherJson, "WSE"));
            dict["Description"] = "";
            dict["PicUrl"] = "";
            dict["Url"] = "";
            result.Add(dict);
            return result;
        }
        #endregion

    }
}