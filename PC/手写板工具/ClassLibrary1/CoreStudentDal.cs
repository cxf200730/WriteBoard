﻿using Dapper;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ClassLibrary1
{
    public class CoreStudentDal
    {
        public static string constr = "Database = 'WritingBoard'; Data Source = 'sh-cdb-am03pru4.sql.tencentcdb.com';Port = '59791'; User Id = 'root'; Password = 'jinao123!@#'; charset = 'utf8'; pooling = true;Allow User Variables=True;";

        /// <summary>
        /// 学生登录
        /// </summary>
        /// <returns></returns>
        public static bool StudentLogin(string usercode, string classcode)
        {
            try

            {
                string param = $"select * from studentList,classroom where studentList.usercode = '{usercode}' and studentList.classcode = '{classcode}' and studentList.classcode = classroom.classcode and classroom.classstu = 1";

                var connection = new MySqlConnection(constr);

                var page = connection.Query(param).ToList().Count;

                if (page == 0)
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }
            catch (Exception ex)

            {
                return false;
            }
        }
        /// <summary>
        /// 获取学生姓名
        /// </summary>
        /// <returns></returns>
        public static string GetStudentName(string usercode)
        {
            try
            {
                string param = $"select username from studentList where usercode = '{usercode}'";

                var connection = new MySqlConnection(constr);

                var page = connection.Query(param).ToList();
                var con = page[0].username;


                var con2 = con.ToString();
                //String join = String.Join(",", page);


                return con2;

            }
            catch (Exception ex)

            {
                return null;
            }
        }
    }
}
