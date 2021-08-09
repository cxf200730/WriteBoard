using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Models.Server;
using Utils.Dals;
using Utils.StaticUtils;

namespace Utils.VariableUtils
{
    public class MysqlHelper
    {
        #region 服务器

        /// <summary>
        /// 更新服务器ip端口状态
        /// </summary>
        /// <param name="server"></param>
        /// <returns></returns>
        public static BaseResponseResult UpdateServerInfo(ServerIpEndPoint server)
        {
            return ServerDal.UpdateServerInfo(server);
        }

        /// <summary>
        /// 搜索服务器信息
        /// </summary>
        /// <returns></returns>
        public static BaseResponseResult SearchServerInfo()
        {
            return ServerDal.SearchServerInfo();
        }

        #endregion 服务器
    }
}