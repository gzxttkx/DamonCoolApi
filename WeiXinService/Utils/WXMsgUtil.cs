using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Xml;
using System.Text;
using Newtonsoft.Json;
using WeiXinService.Models;
using WeiXinService.Utils;

namespace WeiXinWebApi2.Utils
{
    /// <summary>
    /// 微信消息封装，用于给用户发送被动响应消息
    /// </summary>
    public class WXMsgUtil
    {
        #region 生成文本消息
        /// <summary>
        /// 生成文本消息
        /// </summary>
        public static string CreateTextMsg(XmlDocument xmlDoc, string content)
        {
            string strTpl = string.Format(@"<xml>
                <ToUserName><![CDATA[{0}]]></ToUserName>
                <FromUserName><![CDATA[{1}]]></FromUserName>
                <CreateTime>{2}</CreateTime>
                <MsgType><![CDATA[text]]></MsgType>
                <Content><![CDATA[{3}]]></Content>
                </xml>", GetFromXML(xmlDoc, "FromUserName"), GetFromXML(xmlDoc, "ToUserName"),
                       DateTime2Int(DateTime.Now), content);

            return strTpl;
        }
        #endregion

        #region 生成图文消息
        /// <summary>
        /// 生成图文消息
        /// </summary>
        public static string CreateNewsMsg(XmlDocument xmlDoc, List<Dictionary<string, string>> dictList)
        {
            StringBuilder sbItems = new StringBuilder();
            foreach (Dictionary<string, string> dict in dictList)
            {
                sbItems.Append(string.Format(@"
                    <item>
                        <Title><![CDATA[{0}]]></Title> 
                        <Description><![CDATA[{1}]]></Description>
                        <PicUrl><![CDATA[{2}]]></PicUrl>
                        <Url><![CDATA[{3}]]></Url>
                    </item>", dict["Title"], dict["Description"], dict["PicUrl"], dict["Url"]));
            }

            string strTpl = string.Format(@"
                <xml>
                    <ToUserName><![CDATA[{0}]]></ToUserName>
                    <FromUserName><![CDATA[{1}]]></FromUserName>
                    <CreateTime>{2}</CreateTime>
                    <MsgType><![CDATA[news]]></MsgType>
                    <ArticleCount>{3}</ArticleCount>
                    <Articles>
                        {4}
                    </Articles>
                </xml> ", GetFromXML(xmlDoc, "FromUserName"), GetFromXML(xmlDoc, "ToUserName"),
                        DateTime2Int(DateTime.Now), dictList.Count, sbItems.ToString());

            return strTpl;
        }
        #endregion

        #region 时间转换成int
        /// <summary>
        /// 时间转换成int
        /// </summary>
        public static int DateTime2Int(DateTime dt)
        {
            DateTime startTime = TimeZone.CurrentTimeZone.ToLocalTime(new System.DateTime(1970, 1, 1));
            return (int)(dt - startTime).TotalSeconds;
        }
        #endregion

        #region 解析消息XML
        /// <summary>
        /// 解析消息XML
        /// </summary>
        public static string GetFromXML(XmlDocument xmlDoc, string name)
        {
            XmlNode node = xmlDoc.SelectSingleNode("xml/" + name);
            if (node != null && node.ChildNodes.Count > 0)
            {
                return node.ChildNodes[0].Value;
            }
            return "";
        }
        #endregion

        #region 解析图灵消息
        /// <summary>
        /// 解析图灵消息
        /// </summary>
        public static string GetTulingMsg(string info)
        {
            string jsonStr = HttpRequestUtil.RequestTuling(info);
            TuLingMsgModel tm = JsonConvert.DeserializeObject<TuLingMsgModel>(jsonStr);
            if (tm.code == "100000")
            {
                return tm.text;
            }
            if (tm.code == "200000")
            {
                return tm.text;
            }
            return "不知道怎么回复你哎";
        }
        #endregion

        #region 高级群发消息
        #region 文本json
        /// <summary>
        /// 文本json
        /// </summary>
        public static string CreateTextJson(string text, List<string> openidList)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("{\"touser\":[");
            sb.Append(string.Join(",", openidList.ConvertAll<string>(a => "\"" + a + "\"").ToArray()));
            sb.Append("],");
            sb.Append("\"msgtype\":\"text\",");
            sb.Append("\"text\":{\"content\":\"" + text.Trim() + "\"}");
            sb.Append("}");
            return sb.ToString();
        }
        #endregion

        #region 图片json
        /// <summary>
        /// 图片json
        /// </summary>
        public static string CreateImageJson(string media_id, List<string> openidList)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("{\"touser\":[");
            sb.Append(string.Join(",", openidList.ConvertAll<string>(a => "\"" + a + "\"").ToArray()));
            sb.Append("],");
            sb.Append("\"msgtype\":\"image\",");
            sb.Append("\"image\":{\"media_id\":\"" + media_id + "\"}");
            sb.Append("}");
            return sb.ToString();
        }
        #endregion

        #region 图文消息json
        /// <summary>
        /// 图文消息json
        /// </summary>
        public static string CreateNewsJson(string media_id, List<string> openidList)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("{\"touser\":[");
            sb.Append(string.Join(",", openidList.ConvertAll<string>(a => "\"" + a + "\"").ToArray()));
            sb.Append("],");
            sb.Append("\"msgtype\":\"mpnews\",");
            sb.Append("\"mpnews\":{\"media_id\":\"" + media_id + "\"}");
            sb.Append("}");
            return sb.ToString();
        }
        #endregion
        #endregion

    }
}