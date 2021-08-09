using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;

namespace Utils.VariableUtils
{
    public class InitDbContext
    {
        /// <summary>
        /// 初始化连接对象
        /// </summary>
        /// <returns></returns>
        public static MySqlConnection GetModelContainer(string conStr)
        {
            MySqlConnection dbContext = (MySqlConnection)CallContext.GetData("WritingBoard");
            if (dbContext == null)
            {
                dbContext = new MySqlConnection(conStr);

                CallContext.SetData("WritingBoard", dbContext);
            }
            return dbContext;
        }
    }
}