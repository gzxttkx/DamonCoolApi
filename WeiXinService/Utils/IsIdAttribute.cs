using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WeiXinService.Utils
{
    /// <summary>
    /// 标识该属性是主健
    /// </summary>
    [Serializable, AttributeUsage(AttributeTargets.Property | AttributeTargets.Class)]
    public class IsIdAttribute : Attribute
    {
    }
}
