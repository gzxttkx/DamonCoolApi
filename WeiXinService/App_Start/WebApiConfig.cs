using Owin;
using System.Web.Http;
using System.Web.Http.Cors;
using System.Web.Http.OData.Extensions;

namespace WeiXinService
{
    public static class WebApiConfig
    {
        public static void Register(IAppBuilder app)
        {
            HttpConfiguration config = new HttpConfiguration();

            // Configure Web API for self-host. 
            config.MapHttpAttributeRoutes();

            //启用【Bearer Token】（无记名口令）认证
            //config.SuppressDefaultHostAuthentication();
            //config.Filters.Add(new HostAuthenticationFilter("Bearer"));

            // 启用OData
            config.AddODataQueryFilter();

            //启用CORS
            var cors = new EnableCorsAttribute("*", "*", "*");
            cors.SupportsCredentials = true;
            config.EnableCors(cors);

            SwaggerConfig.Register(config);

            app.UseWebApi(config);           
        }
    }
}
