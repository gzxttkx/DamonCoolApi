using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WeiXinService.Models;

namespace WeiXinService.Utils
{
    /// <summary>
    /// 所有数据库的操作
    /// </summary>
   public class MySqlHelper
    {
        /// <summary>
        /// Mysql数据库连接字符串
        /// </summary>
        public static string MySqlConnectionString = "server=localhost;user id = root; password=qazsw123456;database=gzx";

        /// <summary>
        /// 新增每日逐渐放量个股
        /// </summary>
        /// <param name="list"></param>
        /// <returns></returns>
        public static int InsertVlumeRise(List<VolumeRise> list)
        {
            MySqlConnection conn = new MySqlConnection();
            conn.ConnectionString = MySqlConnectionString;
            conn.Open();
            MySqlCommand comm = conn.CreateCommand();
            foreach (VolumeRise item in list)
            {
                comm.CommandText = @"INSERT INTO volumerise
                                    (symbol,
                                    name,
                                    close,
                                    volume,
                                    volume_pre,
                                    changes,
                                    changes_con,
                                    turnover,
                                    day,
                                    flag1,
                                    flag_con)
                                    VALUES
                                    ('"+item.symbol+ @"',
                                    '" + item.symbol + @"',
                                    '" + item.symbol + @"',
                                    '" + item.symbol + @"',
                                    '" + item.symbol + @"',
                                   '" + item.symbol + @"',
                                    '" + item.symbol + @"',
                                    '" + item.symbol + @"',
                                    '" + item.symbol + @"',
                                    '" + item.symbol + @"',
                                    '" + item.symbol + @"'); ";
                comm.ExecuteNonQuery();
            }
            comm.Dispose();
            conn.Dispose();
            conn.Close();
            conn = null;
            
            return 0;
        }
    }
}
