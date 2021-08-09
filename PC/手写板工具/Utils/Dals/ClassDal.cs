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
    public class ClassDal
    {
        public static BaseResponseResult SearchServerInfo(string classCode)
        {
            try
            {
                string param = $"select cStucode as ServerIp from ClassStudents where cClassCode = '{classCode}'";
                var connection = InitDbContext.GetModelContainer(ConnectionString.WritingBoard);
                var status = connection.Query<string>(param).ToList();
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