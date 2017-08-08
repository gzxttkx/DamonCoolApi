using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WeiXinService.Models
{
    public class VolumeRise
    {
        public string symbol { get; set; }
        public string name { get; set; }
        public string close { get; set; }
        public string volume { get; set; }
        public string volume_pre { get; set; }
        public string changes { get; set; }
        public string changes_con { get; set; }
        public string turnover { get; set; }
        public string day_con { get; set; }
        public string day { get; set; }
        public string flag1 { get; set; }
        public string flag_con { get; set; }
    }
}
