using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Models
{
    public class JsonProtocol
    {
        /// <summary>
        /// 连接详情 长度位2  00 01 10 11 四个状态
        /// </summary>
        public ConnectionDetailEnum ConnectionDetail { get; set; }

        /// <summary>
        /// 本机Ip地址  255.255.255.255  15个长度
        /// </summary>
        public string LocalIp { get; set; }

        /// <summary>
        /// 目的Ip 255.255.255.255  15个长度
        /// </summary>
        public string AimIp { get; set; }

        /// <summary>
        /// 用户类型 00 01 10 11 四个状态
        /// </summary>
        public UserTypeEnum UserType { get; set; }

        /// <summary>
        /// 用户名 最长为 6
        /// </summary>
        public string UserName { get; set; }

        /// <summary>
        /// 用户编号
        /// </summary>
        public string UserCode { get; set; }

        /// <summary>
        /// 班级编号
        /// </summary>
        public string CLassCode { get; set; }

        /// <summary>
        /// 手写板宽度
        /// </summary>
        public int BoradWidth { get; set; }

        /// <summary>
        /// 手写板高度
        /// </summary>
        public int BoradHeight { get; set; }

        /// <summary>
        /// json 类型  长度位3  8种状态   000 - 111
        /// </summary>
        public JsonTypeEnum JsonType { get; set; }

        /// <summary>
        /// json 数据  长度可变
        /// </summary>
        public string JsonData { get; set; }

        /// <summary>
        /// 相关套接字
        /// </summary>
        public Socket ContentSocket { get; set; }

        public enum ConnectionDetailEnum
        {
            Server2Student,
            Server2Teacher,
            Student2Server,
            Teacher2Server,
            WebStudent2Server,
            WebTeacher2Server,
            WebServer2Student,
            WebServer2Teacher,
        }

        public enum UserTypeEnum
        {
            Student,
            Teacher,
            Server
        }

        public enum JsonTypeEnum
        {
            TeacherPrepared2Login,
            TeacherPrepared2Exit,
            TeacherSendNormalMessage,
            TeacherSendPenPoint,
            StudentPrepared2Login,
            StudentPrepared2Exit,
            StudentSendNormalMessage,
            StudentSendPenPoint,
            TeacherSendImg,
            TeacherSendClear,
            TeacherSendRevocation,
            WebStudentLogin,
        }
    }
}