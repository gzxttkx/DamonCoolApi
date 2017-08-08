using System.Data;

namespace WeiXinWebApi2.DAL
{
    /// <summary>
    /// 配置
    /// </summary>
    public class ConfigDal
    {
        #region 读取配置
        /// <summary>
        /// 读取配置
        /// </summary>
        public static DataTable GetConfig(string orgID)
        {
            return null; //MSSQLHelper.Query(string.Format("select * from SWX_Config where OrgID='{0}'", orgID)).Tables[0];
        }
        #endregion

    }
}