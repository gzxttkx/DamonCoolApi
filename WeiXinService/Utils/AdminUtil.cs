using System.Web;
using System.Data;
using WeiXinService.Models;

namespace WeiXinService.Utils
{
    /// <summary>
    /// 登录信息帮助类
    /// </summary>
    public class AdminUtil
    {
        #region 获取当前网站根地址
        /// <summary>
        /// 获取当前网站根地址
        /// </summary>
        public static string GetRootUrl()
        {
            return "http://" + HttpContext.Current.Request.Url.Host;
        }
        #endregion

        #region 获取用户登录信息
        /// <summary>
        /// 获取用户登录信息
        /// </summary>
        public static UserInfo GetLoginUser(string WebUser)
        {
            if (WebUser != null)
            {
                string userName = WebUser;
                DataTable dt = MSSQLHelper.Query(string.Format("select * from SWX_Config where UserName='{0}'", userName)).Tables[0];
                if (dt.Rows.Count > 0)
                {
                    UserInfo userInfo = new UserInfo();
                    userInfo.UserName = userName;
                    userInfo.OrgID = dt.Rows[0]["OrgID"].ToString();
                    userInfo.AppID = dt.Rows[0]["AppID"].ToString();
                    userInfo.Token = dt.Rows[0]["Token"].ToString();
                    userInfo.EncodingAESKey = dt.Rows[0]["EncodingAESKey"].ToString();
                    userInfo.access_token = dt.Rows[0]["access_token"].ToString();
                    userInfo.AppSecret = dt.Rows[0]["AppSecret"].ToString();

                    return userInfo;
                }
            }
            return null;
        }
        #endregion

        #region 判断用户是否登录
        /// <summary>
        /// 判断用户是否登录
        /// </summary>
        public static bool IsLogin(string WebUser)
        {
            if (WebUser != null)
            {
                return true;
            }
            return false;
        }
        #endregion

        #region 获取access_token
        /// <summary>
        /// 获取access_token
        /// </summary>
        public static string GetAccessToken(string WebUser)
        {
            string access_token = string.Empty;

            UserInfo user = GetLoginUser(WebUser);
            if (user != null)
            {
                if (string.IsNullOrWhiteSpace(user.access_token)) //尚未保存过access_token
                {
                    access_token = WXApi.GetToken(user.AppID, user.AppSecret);
                }
                else
                {
                    if (WXApi.TokenExpired(user.access_token)) //access_token过期
                    {
                        access_token = WXApi.GetToken(user.AppID, user.AppSecret);
                    }
                    else
                    {
                        return user.access_token;
                    }
                }

                MSSQLHelper.ExecuteSql(string.Format("update SWX_Config set access_token='{0}' where UserName='{1}'", access_token, user.UserName));
            }

            return access_token;
        }
        #endregion

        #region 是否超级管理员
        /// <summary>
        /// 是否超级管理员
        /// </summary>
        public static bool IsAdmin(string WebUser)
        {
            UserInfo user = GetLoginUser(WebUser);
            if (user.UserName == "admin")
            {
                return true;
            }
            return false;
        }
        #endregion

    }
}