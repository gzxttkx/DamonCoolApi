using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace WeiXinService.Utils
{
    //自定义一个微信消息实体类
    public class WeChatMessage
    {
        public string FromUserName { get; set; }
        public string ToUserName { get; set; }
        public string MsgType { get; set; }
        public string EventName { get; set; }
        public string EventKey { get; set; }
        public string Content { get; set; }
    }

    //发送图文消息的列表项
    public class ArticleItem
    {
        public string title { get; set; }
        public string description { get; set; }
        public string picurl { get; set; }
        public string url { get; set; }
    }

    public class WeChatHelper
    {
        /// <summary>
        /// 获取微信信息。
        /// </summary>
        /// <returns></returns>
        public static WeChatMessage GetWxMessage(string xmlStr)
        {
            WeChatMessage wx = new WeChatMessage();
            XmlDocument xml = new XmlDocument();
            xml.LoadXml(xmlStr);
            wx.ToUserName = xml.SelectSingleNode("xml").SelectSingleNode("ToUserName").InnerText;
            wx.FromUserName = xml.SelectSingleNode("xml").SelectSingleNode("FromUserName").InnerText;
            wx.MsgType = xml.SelectSingleNode("xml").SelectSingleNode("MsgType").InnerText;
            if (wx.MsgType.Trim() == "text")
            {
                wx.Content = xml.SelectSingleNode("xml").SelectSingleNode("Content").InnerText;
            }
            if (wx.MsgType.Trim() == "event")
            {
                wx.EventName = xml.SelectSingleNode("xml").SelectSingleNode("Event").InnerText;
                wx.EventKey = xml.SelectSingleNode("xml").SelectSingleNode("EventKey").InnerText;
            }
            return wx;
        }

        /// <summary>
        /// 发送文字消息
        /// </summary>
        public static string SendTextMessage(string fromUserName, string toUserName, string content)
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendFormat("<xml><ToUserName><![CDATA[{0}]]></ToUserName>", fromUserName);
            sb.AppendFormat("<FromUserName><![CDATA[{0}]]></FromUserName>", toUserName);
            sb.AppendFormat("<CreateTime>{0}</CreateTime>", DateTime.Now);
            sb.Append("<MsgType><![CDATA[text]]></MsgType>");
            sb.AppendFormat("<Content><![CDATA[{0}]]></Content>", content);
            sb.Append("<FuncFlag>0</FuncFlag></xml>");

            return sb.ToString();
        }

        /// <summary>
        /// 发送图文列表信息，如果列表为空，会转为发送“没有搜索到内容”的文字信息
        /// </summary>
        public static string SendImageListMessage(string fromUserName, string toUserName, List<ArticleItem> itemList)
        {
            if (itemList == null || itemList.Count == 0)
            {
                return SendTextMessage(fromUserName, toUserName, "没有搜索到相关内容");
            }

            StringBuilder sb = new StringBuilder();

            sb.Append("<xml>");
            sb.AppendFormat("<ToUserName><![CDATA[{0}]]></ToUserName>", fromUserName);
            sb.AppendFormat("<FromUserName><![CDATA[{0}]]></FromUserName>", toUserName);
            sb.AppendFormat("<CreateTime>{0}</CreateTime>", DateTime.Now);
            sb.Append("<MsgType><![CDATA[news]]></MsgType>");
            sb.AppendFormat("<ArticleCount>{0}</ArticleCount>", itemList.Count);
            sb.Append("<Articles>");
            foreach (ArticleItem item in itemList)
            {
                sb.Append("<item>");
                sb.AppendFormat("<Title><![CDATA[{0}]]></Title> ", item.title);
                sb.AppendFormat("<Description><![CDATA[{0}]]></Description>", item.description);
                sb.AppendFormat("<PicUrl><![CDATA[{0}]]></PicUrl>", item.picurl);
                sb.AppendFormat("<Url><![CDATA[{0}]]></Url>", item.url);
                sb.Append("</item>");
            }
            sb.Append("</Articles>");
            sb.Append("</xml>");

            return sb.ToString();
        }

        //http://mp.weixin.qq.com/wiki/index.php?title=%E8%8E%B7%E5%8F%96access_token
        public static string GetAccessToken()
        {
            string accessToken = string.Empty;
            //http请求方式: GET
            //https://api.weixin.qq.com/cgi-bin/token?grant_type=client_credential&appid=APPID&secret=APPSECRET
            string query = string.Format("grant_type=client_credential&appid={0}&secret={1}","" /*Common.AppID*/,"" /*Common.AppSecret*/);
            //string result = WebApiRequest.GetRequest("https://api.weixin.qq.com", "/cgi-bin/token", query);
            string result = "";
             //result返回说明
             //正常情况下，微信会返回下述JSON数据包给公众号：
             //{"access_token":"ACCESS_TOKEN","expires_in":7200}
             //参数    说明
             //access_token    获取到的凭证
             //expires_in    凭证有效时间，单位：秒
             //错误时微信会返回错误码等信息，JSON数据包示例如下（该示例为AppID无效错误）:
             //{"errcode":40013,"errmsg":"invalid appid"}
             JObject jOb = JObject.Parse(result);
            if (jOb["access_token"] != null)
            {
                accessToken = jOb["access_token"].ToString(); ;
            }
            return accessToken;
        }
    }
}
