using Microsoft.Owin.Hosting;
using System;
using System.Configuration;

namespace WeiXinService
{
    class Program
    {
        static void Main(string[] args)
        {
            //初始化StartOptions参数
            StartOptions options = new StartOptions();

            //服务器Url设置
            options.Urls.Add("http://*:8080");
           //options.Urls.Add("http://112.74.48.118:8080");
            //options.Urls.Add("http://172.18.177.24:9998");

            //Server实现类库设置
            options.ServerFactory = "Microsoft.Owin.Host.HttpListener";
            //ConfigurationManager.AppSettings["url"])
            using (WebApp.Start<Startup>(options))
            {
                Console.WriteLine("Server is running , press Enter to exit.");
                Console.ReadLine();
            }
        }
    }
}
