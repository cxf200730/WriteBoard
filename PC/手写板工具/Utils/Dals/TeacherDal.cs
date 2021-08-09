using Dapper;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Utils.Dals
{
    public class TeacherDal
    {

        public static string constr = "Database = 'WritingBoard'; Data Source = 'sh-cdb-am03pru4.sql.tencentcdb.com';Port = '59791'; User Id = 'root'; Password = 'jinao123!@#'; charset = 'utf8'; pooling = true;Allow User Variables=True;";

        /// <summary>
        /// 老师登录
        /// </summary>
        /// <returns></returns>
        public static bool TeacherLogin(string username, string classcode)
        {
            try

            {
                string param = $"select * from teacherList where name = '{username}' and classcode = '{classcode}'";

                var connection = new MySqlConnection(constr);

                var page = connection.Query(param).ToList().Count;

                if (page == 0)
                {
                    return false;
                }
                else
                {
                    string param2 = $"update classroom set classroom.classstu = 1 where classroom.classcode = '{classcode}'";
                    var connection2 = new MySqlConnection(constr);
                    connection.Execute(param2);
                    return true;
                }
            }
            catch (Exception ex)

            {
                return false;
            }
        }

        /// <summary>
        /// 老师退出
        /// </summary>
        /// <returns></returns>
        public static bool TeacherExit(string username, string classcode)
        {
            try

            {
                string param = $"select * from teacherList where name = '{username}' and classcode = '{classcode}'";

                var connection = new MySqlConnection(constr);

                var page = connection.Query(param).ToList().Count;

                if (page == 0)
                {
                    return false;
                }
                else
                {
                    string param2 = $"update classroom set classroom.classstu = 0 where classroom.classcode = '{classcode}'";
                    var connection2 = new MySqlConnection(constr);
                    connection.Execute(param2);
                    return true;
                }
            }
            catch (Exception ex)

            {
                return false;
            }
        }
    }
}
