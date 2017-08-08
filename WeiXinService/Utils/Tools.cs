using System;
using System.Collections.Generic;
using System.Web;

namespace WeiXinService.Utils
{
    /// <summary>
    /// 工具类
    /// </summary>
    public class Tools
    {
        #region 获取Json字符串某节点的值
        /// <summary>
        /// 获取Json字符串某节点的值
        /// </summary>
        public static string GetJsonValue(string jsonStr, string key)
        {
            string result = string.Empty;
            if (!string.IsNullOrEmpty(jsonStr))
            {
                key = "\"" + key.Trim('"') + "\"";
                int index = jsonStr.IndexOf(key) + key.Length + 1;
                if (index > key.Length + 1)
                {
                    //先截逗号，若是最后一个，截“｝”号，取最小值
                    int end = jsonStr.IndexOf(',', index);
                    if (end == -1)
                    {
                        end = jsonStr.IndexOf('}', index);
                    }

                    result = jsonStr.Substring(index, end - index);
                    result = result.Trim(new char[] { '"', ' ', '\'' }); //过滤引号或空格
                }
            }
            return result;
        }
        
        //public static string GetData()
        //{
        //   // http://money.finance.sina.com.cn/quotes_service/api/jsonp_v2.php/IO.XSRV2.CallbackList['M5a$19K_cODdxgk9']/StatisticsService.getVolumeRiseConList?page=1&num=50&sort=day_con&asc=0&node=adr_hk
        //}
        
        #endregion

    }
}