using System;
using System.Web.Http;
using WeiXinService.Attributes;
using System.Net.Http;
using log4net;
using System.Reflection;
using System.Linq;
using WeiXinService.Utils;
using System.Collections.Generic;
using WeiXinWebApi2.Utils;
using System.Xml;
using WeiXinService.Models;
using Newtonsoft.Json;

namespace WeiXinService.Controller
{
    /// <summary>
    /// Damon.gu 2017/07/08
    /// </summary>
    public class HomeController : ApiController
    {
        //FormDataCollection form
        string sToken = string.Empty;
        string sAppID = string.Empty;
        string sEncodingAESKey = string.Empty;
        ILog log = log4net.LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        /// <summary>
        /// 获取API版本
        /// </summary>
        /// <returns></returns>
        [Route("home/valid")]
        [AcceptVerbs("Get", "Post")]
        [LoggingFilter]
        public HttpResponseMessage WetChatVerify(HttpRequestMessage content)　　 　　//HttpRequestMessage 和HttpResponseMessage，分别用于封装Requset和Response
        {
            string echostr = (from kvp in content.GetQueryNameValuePairs()
                              where kvp.Key == "echostr"
                              select kvp.Value).FirstOrDefault();

            string signature = (from kvp in content.GetQueryNameValuePairs()
                                where kvp.Key == "signature"
                                select kvp.Value).FirstOrDefault();

            string timestamp = (from kvp in content.GetQueryNameValuePairs()
                                where kvp.Key == "timestamp"
                                select kvp.Value).FirstOrDefault();

            string nonce = (from kvp in content.GetQueryNameValuePairs()
                            where kvp.Key == "nonce"
                            select kvp.Value).FirstOrDefault();
            log.Info("echostr:" + echostr+ " signature:" + signature+ " nonce:"+ nonce);
            string xmlContent = content.Content.ReadAsStringAsync().Result;
            string response = string.Empty;
            log.Info("xml3:" + xmlContent);

            if (!string.IsNullOrEmpty(xmlContent))
            {
                XmlDocument doc = new XmlDocument();
                doc.LoadXml(xmlContent);
                WeChatMessage msg = WeChatHelper.GetWxMessage(xmlContent);

                if (msg.MsgType.Trim() == "text")//用户发送一些文字信息
                {
                    string text = WXMsgUtil.GetFromXML(doc, "Content")+"";

                    response = WXMsgUtil.GetTulingMsg(text!=""?text:"你是谁?")+"";
                    log.Info("text:" + text +" tuling:"+ response);
                }
                if (msg.MsgType.Trim() == "event")//点击菜单或者新增/取消关注
                {
                    switch (msg.EventName.Trim().ToLower())
                    {
                        case "click":      //点击菜单
                            response = "haha";
                            break;
                        case "subscribe":    //用户新增关注（可以返回一些欢迎信息之类）　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　
                            response = "wawa";
                            break;
                        case "unsubscribe":   //用户取消关注（一般不需要去返回什么信息）
                        default:
                            break;
                    }
                }

                HttpResponseMessage xmlResult = new HttpResponseMessage();
                xmlResult.Content = new StringContent(WXMsgUtil.CreateTextMsg(doc, response != "" ? response : "返回不能为空值!"));
                return xmlResult;
            }
                string returnStr = "";
            if (string.IsNullOrEmpty(echostr) | string.IsNullOrEmpty(signature) | string.IsNullOrEmpty(timestamp) | string.IsNullOrEmpty(nonce))
            {
                returnStr = "error";
            }

            //if (CheckSignature(signature, timestamp, nonce))
            //{
            //    log.Info("验证成功，返回：" + echostr);
            returnStr = echostr;
            //}

            HttpResponseMessage result = new HttpResponseMessage();
            result.Content = new StringContent(returnStr!=""?returnStr:"返回不能为空值!");
            return result;
        }

        /// <summary>
        /// 验证微信签名
        /// </summary>
        private bool CheckSignature(string signature, string timestamp, string nonce)
        {
            String[] ArrTmp = { "DamonGu", timestamp, nonce };

            Array.Sort(ArrTmp);
            String tmpStr = String.Join("", ArrTmp);

            tmpStr = System.Web.Security.FormsAuthentication.HashPasswordForStoringInConfigFile(tmpStr, "SHA1").ToLower();

            if (tmpStr == signature)
            {
                return true;
            }
            else
            {
                return false;
            }
        }


        [Route("home/text")]
        [HttpGet]
        public HttpResponseMessage test()
        {
            string content = FinanceUtil.GetHtml();//WXMsgUtil.GetTulingMsg(info);
            List<VolumeRise> list = JsonConvert.DeserializeObject<List<VolumeRise>>(content);
            MySqlHelper.InsertVlumeRise(list);
            return new HttpResponseMessage() { Content=new StringContent(content) };
        }

        [Route("home/GetLxflData")]
        [HttpGet]
        public IHttpActionResult GetLxflData()
        {
            string content = FinanceUtil.GetHtml();//WXMsgUtil.GetTulingMsg(info);
            List<VolumeRise> list = JsonConvert.DeserializeObject<List<VolumeRise>>(content);
            //MySqlHelper.InsertVlumeRise(list);
            return  Ok(new { data= list });
        }


        [Route("home/updateMenu")]
        [HttpGet]
        public HttpRequestMessage UpdateWX()
        {
            string json = "";
            UserInfo user = AdminUtil.GetLoginUser("");
            string result = WXApi.CreateMenu(AdminUtil.GetAccessToken(""), user.OrgID);
            if (Tools.GetJsonValue(result, "errcode") == "0")
            {
                json = "{\"code\":1,\"msg\":\"\"}";
            }
            else
            {
                json = "{\"code\":0,\"msg\":\"errcode:"
                    + Tools.GetJsonValue(result, "errcode") + ", errmsg:"
                    + Tools.GetJsonValue(result, "errmsg") + "\"}";
            }
            return null;
        }


    }
}
