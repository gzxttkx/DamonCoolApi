using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WeiXinService.Models
{
    /// <summary>
    /// 菜单信息
    /// </summary>
    public class MenuModel
    {
        #region jQGrid树形表格所属字段
        /// <summary>
        /// id
        /// </summary>
        public string id { get; set; }

        /// <summary>
        /// level
        /// </summary>
        public int level { get; set; }

        /// <summary>
        /// parent
        /// </summary>
        public string parent { get; set; }

        /// <summary>
        /// isLeaf
        /// </summary>
        public bool isLeaf { get; set; }

        /// <summary>
        /// expanded
        /// </summary>
        public bool expanded { get; set; }

        /// <summary>
        /// loaded
        /// </summary>
        public bool loaded
        {
            get
            {
                return true;
            }
        }
        #endregion

        #region 菜单
        /// <summary>
        /// id
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// 编号
        /// </summary>
        public string Code { get; set; }

        /// <summary>
        /// 菜单名称
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 菜单Type
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// 菜单key
        /// </summary>
        public string MenuKey { get; set; }

        /// <summary>
        /// 菜单Url
        /// </summary>
        public string Url { get; set; }

        /// <summary>
        /// 微信公众号原始ID
        /// </summary>
        public string OrgID { get; set; }
        #endregion
    }
}