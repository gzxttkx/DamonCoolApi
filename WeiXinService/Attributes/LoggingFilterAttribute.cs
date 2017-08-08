using log4net;
using System;
using System.Reflection;
using System.Text;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;

namespace WeiXinService.Attributes
{
    public class LoggingFilterAttribute : ActionFilterAttribute
    {
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public override void OnActionExecuting(HttpActionContext filterContext)
        {
            var message = new StringBuilder();
            message.Append(string.Format("Executing controller {0}, action {1}",
                filterContext.ActionDescriptor.ControllerDescriptor.ControllerName,
                filterContext.ActionDescriptor.ActionName));
            log.Info(message);
        }

        public override void OnActionExecuted(HttpActionExecutedContext filterContext)
        {
            if (filterContext.Exception == null)
            {
                return;
            }
            else
            {
                Exception ex = filterContext.Exception;
                log.Error(ex);
            }
        }
    }
}