using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper;
using Models.Server;
using Utils.StaticUtils;
using Utils.VariableUtils;

namespace Utils.Dals
{
    public class ServerDal
    {
        /// <summary>
        /// 更新服务端信息
        /// </summary>
        /// <param name="server"></param>
        /// <returns></returns>
        public static BaseResponseResult UpdateServerInfo(ServerIpEndPoint server)
        {
            try
            {
                string param = $"UPDATE `ServerInfo` SET s_ip = '{server.ServerIp}',s_port = '{server.ServerPort}', s_is_work = '{server.ServerIsWork}',s_online_time = '{server.ServerOnlineTime}' ,s_public_ip = '{server.ServerPublicIp}' WHERE s_id = {server.ServerId}";
                var connection = InitDbContext.GetModelContainer(ConnectionString.WritingBoard);
                int status = connection.Execute(param);
                if (status == 1)
                {
                    return BaseResponseResult.Success("1");
                }
                else
                {
                    return BaseResponseResult.Failed("-1");
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return BaseResponseResult.Wrong("-2");
            }
        }

        /// <summary>
        /// 搜索服务器信息
        /// </summary>
        /// <returns></returns>
        public static BaseResponseResult SearchServerInfo()
        {
            try
            {
                string param = $"select s_id as ServerId,s_ip as ServerIp,s_port as ServerPort,s_is_work as ServerIsWork, s_online_time as ServerOnlineTime,s_public_ip as ServerPublicIp from ServerInfo where s_id = 1";
                var connection = InitDbContext.GetModelContainer(ConnectionString.WritingBoard);
                var status = connection.Query<ServerIpEndPoint>(param).ToList();
                if (status != null && status.Count != 0)
                {
                    return BaseResponseResult.Success(status);
                }
                else
                {
                    return BaseResponseResult.Failed("-1");
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return BaseResponseResult.Wrong("-2");
            }
        }
    }
}