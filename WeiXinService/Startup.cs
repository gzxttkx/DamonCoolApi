using log4net.Config;
using Owin;
using System.Net;
using System.Web.Http;
using System.Web.Http.Cors;

namespace WeiXinService
{
    public class Startup
    {        
        public void Configuration(IAppBuilder app)
        {

            WebApiConfig.Register(app);
            XmlConfigurator.Configure();

            // For Setting Homepage
            app.Run(context =>
            {                
                context.Response.ContentType = "text/html";
                return context.Response.WriteAsync("<meta http-equiv='refresh' content='0; url =/swagger' />");
            });

        }
    }
}
