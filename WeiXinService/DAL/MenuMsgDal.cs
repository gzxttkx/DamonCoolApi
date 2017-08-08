using System.Data;

namespace WeiXinWebApi2.DAL
{
    /// <summary>
    /// 菜单事件
    /// </summary>
    public class MenuMsgDal
    {
        #region 根据菜单事件获取消息
        /// <summary>
        /// 根据菜单事件获取消息
        /// </summary>
        public static DataTable GetMenuMsg(string eventKey)
        {
            return new DataTable(); //MSSQLHelper.Query(string.Format("select * from SWX_MenuMsg where MenuKey='{0}' order by Sort", eventKey)).Tables[0];
        }
        #endregion

    }
}