using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WeiXinService.Models
{

    //{"code":100000,"text":"东莞:7月18号 周二,25-31° 25° 大雨 无持续风微风;7月19号 周三,26-33° 阵雨 无持续风微风;7月20号 周四,27-34° 阵雨 无持续风微风;7月21号 周五,27-33° 多云 无持续风微风;"}
public class TuLingMsgModel
    {
        public string text { get; set; }
        public string code { get; set; }
    }
}
