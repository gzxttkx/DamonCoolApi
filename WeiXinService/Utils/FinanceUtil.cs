using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace WeiXinService.Utils
{
    public class FinanceUtil
    {
        public static string GetHtml()
        {

            WebClient MyWebClient = new WebClient();


            MyWebClient.Credentials = CredentialCache.DefaultCredentials;//获取或设置用于向Internet资源的请求进行身份验证的网络凭据

            Byte[] pageData = MyWebClient.DownloadData("http://money.finance.sina.com.cn/quotes_service/api/jsonp_v2.php/IO.XSRV2.CallbackList['M5a$19K_cODdxgk9']/StatisticsService.getVolumeRiseConList?page=1&num=2000&sort=day_con&asc=0&node=adr_hk"); //从指定网站下载数据

            string pageHtml = Encoding.Default.GetString(pageData);  //如果获取网站页面采用的是GB2312，则使用这句  
            pageHtml = pageHtml.Substring(pageHtml.IndexOf("[{"),pageHtml.Length- pageHtml.IndexOf("[{")-1);

            return pageHtml;
        }
    }
}
