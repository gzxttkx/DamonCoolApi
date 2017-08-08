using System;
using System.Data;
using System.Text;
using WeiXinService.Models;

namespace WeiXinService.Utils
{
    /// <summary>
    /// 菜单
    /// </summary>
    public class MenuDal
    {
        #region 读取菜单列表
        /// <summary>
        /// 读取菜单列表
        /// </summary>
        public static DataTable GetMenuList()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat("select * from SWX_WxMenu");
            return MSSQLHelper.Query(sb.ToString()).Tables[0];
        }
        #endregion

        #region 读取一级菜单列表
        /// <summary>
        /// 读取一级菜单列表
        /// </summary>
        public static DataTable GetOneMenuList(UserInfo user)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat(string.Format("select * from SWX_WxMenu where len(Code) = 2 and OrgID='{0}' order by Code", user.OrgID));
            return MSSQLHelper.Query(sb.ToString()).Tables[0];
        }
        #endregion

        #region 读取二级菜单列表
        /// <summary>
        /// 读取二级菜单列表
        /// </summary>
        public static DataTable GetTwoMenuList(string code)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat(@"select * from SWX_WxMenu where Code like '{0}%' and len(Code)>2 order by Code", code);
            return MSSQLHelper.Query(sb.ToString()).Tables[0];
        }
        #endregion

        #region 读取菜单列表
        /// <summary>
        /// 读取菜单列表
        /// </summary>
        public static DataTable GetMenuList(string userName)
        {
            StringBuilder orgsb = new StringBuilder();
            orgsb.AppendFormat(@"select OrgID from SWX_Config where UserName='{0}'", userName);
            DataTable dt = MSSQLHelper.Query(orgsb.ToString()).Tables[0];
            StringBuilder sb = new StringBuilder();
            if (dt != null && dt.Rows.Count > 0)
            {
                sb.AppendFormat("select * from SWX_WxMenu where OrgID='{0}' order by Code", dt.Rows[0]["OrgID"].ToString());
            }
            return MSSQLHelper.Query(sb.ToString()).Tables[0];

        }
        #endregion

        #region 读取菜单列表
        /// <summary>
        /// 读取菜单列表
        /// </summary>
        public static DataTable GetMenuListByOrgID(string orgID)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat("select * from SWX_WxMenu where OrgID='{0}'", orgID);
            return MSSQLHelper.Query(sb.ToString()).Tables[0];
        }
        #endregion

        #region 获取二级菜单下拉列表html
        /// <summary>
        /// 获取二级菜单下拉列表html
        /// </summary>
        public static string GetMenusLevel2(UserInfo user, string selectId)
        {
            StringBuilder sbHtml = new StringBuilder();
            string sql = string.Format("select * from SWX_WxMenu where Code like '____' and Type='click' and OrgID='{0}'", user.OrgID);
            DataTable dt = MSSQLHelper.Query(sql).Tables[0];

            sbHtml.Append(string.Format("<select id='{0}' name='MenuKey' class='SIMPO_Select'>", selectId));
            foreach (DataRow dr in dt.Rows)
            {
                sbHtml.Append(string.Format("<option value='{1}'>{0}</option>", dr["Name"].ToString(), dr["MenuKey"].ToString()));
            }
            sbHtml.Append("</select>");

            return sbHtml.ToString();
        }
        #endregion

        #region 获取二级菜单下拉列表html
        /// <summary>
        /// 获取二级菜单下拉列表html
        /// </summary>
        public static string GetMenusLevel3(UserInfo user)
        {
            StringBuilder sbHtml = new StringBuilder();
            string sql = string.Format("select * from SWX_WxMenu where Code like '____' and Type='click' and OrgID='{0}'", user.OrgID);
            DataTable dt = MSSQLHelper.Query(sql).Tables[0];

           
            foreach (DataRow dr in dt.Rows)
            {
                sbHtml.Append(string.Format(" <li><a href='javascript:void(0);' id='{0}' onclick=\"menulist('{1}')\">{2}</a></li>",  dr["MenuKey"].ToString(), dr["MenuKey"].ToString(),dr["Name"].ToString()));
            }
            

            return sbHtml.ToString();
        }
        #endregion

        #region 根据菜单id获取信息
        /// <summary>
        /// 根据菜单id获取信息
        /// </summary>
        public static DataTable GetMenuByID(int menuId)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat(@"select * from SWX_WxMenu where id = {0}", menuId);
            return MSSQLHelper.Query(sb.ToString()).Tables[0];
        }
        #endregion

        #region 根据菜单id删除
        /// <summary>
        /// 根据菜单id删除
        /// </summary>
        public static int DeleteMenu(string menuId)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat(@"delete from SWX_WxMenu where id = {0}", menuId);
            int row = MSSQLHelper.ExecuteSql(sb.ToString());
            return row;
        }
        #endregion

        #region 根据菜单code删除
        /// <summary>
        /// 根据菜单code删除
        /// </summary>
        public static int DeleteOneMenu(string code, UserInfo user)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat(@"delete from SWX_WxMenu where Code like '{0}%' and OrgID='{1}'", code, user.OrgID);
            int row = MSSQLHelper.ExecuteSql(sb.ToString());
            return row;
        }
        #endregion

        #region 根据菜单id修改保存
        /// <summary>
        /// 根据菜单id修改保存
        /// </summary>
        public static int UpdateMenu(string menuId, string name, string type, string key, string url)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat(@"update SWX_WxMenu set Name='{0}',Type='{1}',MenuKey = '{2}',Url ='{3}' where id = {4}",
                               name, type, key, url, menuId);
            int row = MSSQLHelper.ExecuteSql(sb.ToString());
            return row;
        }
        #endregion

        #region 新增菜单保存
        /// <summary>
        /// 新增菜单保存
        /// </summary>
        public static int AddMenu(string code, string name, string type, string key, string url, string username)
        {
            int row = 0;
            StringBuilder orgsb = new StringBuilder();
            orgsb.AppendFormat(@"select OrgID from SWX_Config where UserName='{0}'", username);
            DataTable dt = MSSQLHelper.Query(orgsb.ToString()).Tables[0];
            if (dt != null && dt.Rows.Count > 0)
            {
                string winxin = dt.Rows[0]["OrgID"].ToString();
                StringBuilder sb = new StringBuilder();
                sb.AppendFormat(@"insert into SWX_WxMenu values( '{0}','{1}','{2}','{3}','{4}','{5}')",
                                  code, name, type, key, url, winxin);
                row = MSSQLHelper.ExecuteSql(sb.ToString());

            }
            return row;

        }
        #endregion

        #region 生成菜单Json
        /// <summary>
        /// 生成菜单Json
        /// </summary>
        public static string GetMenuJsonStr(string orgID)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("{ \"button\":[");
            DataTable dt = GetMenuListByOrgID(orgID);
            foreach (DataRow dr in dt.Rows)
            {
                string sCode = dr["Code"].ToString();
                if (sCode.Length == 2)
                {
                    sb.AppendFormat("{{\"type\":\"{0}\"", dr["Type"].ToString());
                    sb.AppendFormat(",\"name\":\"{0}\"", dr["Name"].ToString());
                    if (dr["Type"].ToString() == "click")
                    {
                        sb.AppendFormat(",\"key\":\"{0}\"", dr["MenuKey"].ToString());
                    }
                    if (dr["Type"].ToString() == "view")
                    {
                        sb.AppendFormat(",\"url\":\"{0}\"", dr["Url"].ToString());
                    }
                    bool subMenuExists = false;
                    bool first = true;
                    foreach (DataRow drSub in dt.Rows)
                    {
                        string sSubCode = drSub["Code"].ToString();
                        if (sSubCode.Length == 4 && sSubCode.IndexOf(sCode) == 0)
                        {
                            subMenuExists = true;
                            if (subMenuExists && first)
                            {
                                sb.Append(",\"sub_button\":[");
                            }
                            if (!first) sb.Append(",");
                            if (first) first = false;

                            sb.AppendFormat("{{\"type\":\"{0}\"", drSub["Type"].ToString());
                            sb.AppendFormat(",\"name\":\"{0}\"", drSub["Name"].ToString());
                            if (drSub["Type"].ToString() == "click")
                            {
                                sb.AppendFormat(",\"key\":\"{0}\"", drSub["MenuKey"].ToString());
                            }
                            if (drSub["Type"].ToString() == "view")
                            {
                                sb.AppendFormat(",\"url\":\"{0}\"", drSub["Url"].ToString());
                            }
                            sb.Append("}");
                        }
                    }
                    if (subMenuExists) sb.Append("]");
                    sb.Append("},");
                }
            }
            if (sb.Length > 1 && sb.ToString(sb.Length - 2, 2) == "},")
            {
                sb.Remove(sb.Length - 1, 1);
            }
            sb.Append("]}");
            return sb.ToString();
        }
        #endregion

        #region 获取一级菜单code
        /// <summary>
        /// 获取一级菜单code
        /// </summary>
        public static String GetOneCode()
        {
            string mcode = "";
            DataTable oneCode = MSSQLHelper.Query("select max(Code) Code from SWX_WxMenu where len(Code) = 2").Tables[0];
            if (oneCode != null && oneCode.Rows.Count > 0)
            {
                int a = int.Parse(oneCode.Rows[0]["Code"].ToString()) + 1;
                if (a < 10)
                {
                    mcode = "0" + a.ToString();
                }
                else
                {
                    mcode = a.ToString();
                }
            }
            else {
                mcode = "01";
            }
            return mcode;
        }
        #endregion

        #region 获取二级菜单code
        /// <summary>
        /// 获取二级菜单code
        /// </summary>
        public static String GetTwoCode(string code)
        {
            string mcode = "";
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat(@"select max(Code) Code from SWX_WxMenu where   Code like '{0}%' ", code);
            DataTable oneCode = MSSQLHelper.Query(sb.ToString()).Tables[0];
            if (oneCode != null && oneCode.Rows.Count > 0)
            {
                string twocode = oneCode.Rows[0]["Code"].ToString();
                if (twocode.Length == 2)
                {
                    mcode = code + "01";
                }
                else
                {
                    int a = int.Parse(twocode) + 1;
                    if (a < 1000)
                    {
                        mcode = "0" + a.ToString();
                    }
                    else
                    {
                        mcode = a.ToString();
                    }
                }

            }
            return mcode;
        }
        #endregion

    }
}