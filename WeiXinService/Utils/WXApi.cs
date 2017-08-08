using System.Collections.Generic;
using System.Linq;

namespace WeiXinService.Utils
{
    /// <summary>
    /// 微信API，用于给用户发送消息
    /// </summary>
    public class WXApi
    {
        #region 获取Token
        /// <summary>
        /// 获取Token
        /// </summary>
        public static string GetToken(string appid, string secret)
        {
            string strJson = HttpRequestUtil.RequestUrl(string.Format("https://api.weixin.qq.com/cgi-bin/token?grant_type=client_credential&appid={0}&secret={1}", appid, secret));
            return Tools.GetJsonValue(strJson, "access_token");
        }
        #endregion

        #region 验证Token是否过期
        /// <summary>
        /// 验证Token是否过期
        /// </summary>
        public static bool TokenExpired(string access_token)
        {
            string jsonStr = HttpRequestUtil.RequestUrl(string.Format("https://api.weixin.qq.com/cgi-bin/menu/get?access_token={0}", access_token));
            if (Tools.GetJsonValue(jsonStr, "errcode") == "42001")
            {
                return true;
            }
            return false;
        }
        #endregion

        #region 创建菜单
        /// <summary>
        /// 创建菜单
        /// </summary>
        public static string CreateMenu(string access_token, string orgID)
        {
            string menuJsonStr = MenuDal.GetMenuJsonStr(orgID);
            return CreateMenu2(access_token, menuJsonStr);
        }
        /// <summary>
        /// 创建菜单
        /// </summary>
        public static string CreateMenu2(string access_token, string menuJsonStr)
        {
            return HttpRequestUtil.PostUrl(string.Format("https://api.weixin.qq.com/cgi-bin/menu/create?access_token={0}", access_token), menuJsonStr);
        }
        #endregion

        #region 根据OpenID列表群发
        /// <summary>
        /// 根据OpenID列表群发
        /// </summary>
        public static string Send(string access_token, string postData)
        {
            return HttpRequestUtil.PostUrl(string.Format("https://api.weixin.qq.com/cgi-bin/message/mass/send?access_token={0}", access_token), postData);
        }
        #endregion

        #region 上传图文消息素材返回media_id
        /// <summary>
        /// 上传图文消息素材返回media_id
        /// </summary>
        public static string UploadNews(string access_token, string postData)
        {
            return HttpRequestUtil.PostUrl(string.Format("https://api.weixin.qq.com/cgi-bin/media/uploadnews?access_token={0}", access_token), postData);
        }
        #endregion

        #region 上传媒体返回媒体ID
        /// <summary>
        /// 上传媒体返回媒体ID
        /// </summary>
        public static string UploadMedia(string access_token, string type, string path)
        {
            // 设置参数
            string url = string.Format("http://file.api.weixin.qq.com/cgi-bin/media/upload?access_token={0}&type={1}", access_token, type);
            return HttpRequestUtil.HttpUploadFile(url, path);
        }
        #endregion

        #region 获取关注者OpenID集合
        /// <summary>
        /// 获取关注者OpenID集合
        /// </summary>
        public static List<string> GetOpenIDs(string access_token)
        {
            List<string> result = new List<string>();

            List<string> openidList = GetOpenIDs(access_token, null);
            result.AddRange(openidList);

            while (openidList.Count > 0)
            {
                openidList = GetOpenIDs(access_token, openidList[openidList.Count - 1]);
                result.AddRange(openidList);
            }

            return result;
        }

        /// <summary>
        /// 获取关注者OpenID集合
        /// </summary>
        public static List<string> GetOpenIDs(string access_token, string next_openid)
        {
            // 设置参数
            string url = string.Format("https://api.weixin.qq.com/cgi-bin/user/get?access_token={0}&next_openid={1}", access_token, string.IsNullOrWhiteSpace(next_openid) ? "" : next_openid);
            string returnStr = HttpRequestUtil.RequestUrl(url);
            int count = int.Parse(Tools.GetJsonValue(returnStr, "count"));
            if (count > 0)
            {
                string startFlg = "\"openid\":[";
                int start = returnStr.IndexOf(startFlg) + startFlg.Length;
                int end = returnStr.IndexOf("]", start);
                string openids = returnStr.Substring(start, end - start).Replace("\"", "");
                return openids.Split(',').ToList<string>();
            }
            else
            {
                return new List<string>();
            }
        }
        #endregion

    }
}