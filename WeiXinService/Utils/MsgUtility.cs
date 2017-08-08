using System.Collections.Generic;
using System.Data;
using System.Web;
using System.Xml;
using Tencent;
using WeiXinWebApi2.DAL;

namespace WeiXinWebApi2.Utils
{
    public class MsgUtility
    {
        public static string responseMsg( XmlDocument xmlDoc,string sAppID="",string sToken="",string sEncodingAESKey="", string timestamp ="", string nonce="")
        {
            string result = "";
            string msgType = WXMsgUtil.GetFromXML(xmlDoc, "MsgType");
            switch (msgType)
            {
                case "event":
                    switch (WXMsgUtil.GetFromXML(xmlDoc, "Event"))
                    {
                        case "subscribe": //订阅
                            FileLogger.WriteLog( "订阅");
                            break;
                        case "unsubscribe": //取消订阅
                            FileLogger.WriteLog( "取消订阅");
                            break;
                        case "CLICK":
                            DataTable dtMenuMsg = MenuMsgDal.GetMenuMsg(WXMsgUtil.GetFromXML(xmlDoc, "EventKey"));
                            if (dtMenuMsg.Rows.Count > 0)
                            {
                                List<Dictionary<string, string>> dictList = new List<Dictionary<string, string>>();
                                foreach (DataRow dr in dtMenuMsg.Rows)
                                {
                                    Dictionary<string, string> dict = new Dictionary<string, string>();
                                    dict["Title"] = dr["Title"].ToString();
                                    dict["Description"] = dr["Description"].ToString();
                                    dict["PicUrl"] = dr["PicUrl"].ToString();
                                    dict["Url"] = dr["Url"].ToString();
                                    dictList.Add(dict);
                                }
                                result = WXMsgUtil.CreateNewsMsg(xmlDoc, dictList);
                            }
                            else
                            {
                                result = WXMsgUtil.CreateTextMsg(xmlDoc, "无此消息哦");
                            }
                            break;
                        default:
                            break;
                    }
                    break;
                case "text":
                    string text = WXMsgUtil.GetFromXML(xmlDoc, "Content");
                    if (text == "合肥" || text == "合肥天气" || text == "合肥天气预报"
                        || text.ToLower() == "hf" || text.ToLower() == "hefei")
                    {
                        result = WXMsgUtil.CreateNewsMsg(xmlDoc, WeatherUtil.GetWeatherInfo());
                    }
                    else
                    {
                        result = WXMsgUtil.CreateTextMsg(xmlDoc, WXMsgUtil.GetTulingMsg(text));
                    }
                    break;
                default:
                    break;
            }

            if (!string.IsNullOrWhiteSpace(sAppID)) //没有AppID则不加密(订阅号没有AppID)
            {
                //加密
                WXBizMsgCrypt wxcpt = new WXBizMsgCrypt(sToken, sEncodingAESKey, sAppID);
                string sEncryptMsg = ""; //xml格式的密文
                //string timestamp = context.Request["timestamp"];
                //string nonce = context.Request["nonce"];
                int ret = wxcpt.EncryptMsg(result, timestamp, nonce, ref sEncryptMsg);
                if (ret != 0)
                {
                    FileLogger.WriteErrorLog( "加密失败，错误码：" + ret);
                    return string.Empty;
                }

                //context.Response.Write(sEncryptMsg);
                return sEncryptMsg;
               //context.Response.Flush();
            }
            else
            {
                //context.Response.Write(result);
                //context.Response.Flush();
                return result;
            }
        }
    }
}