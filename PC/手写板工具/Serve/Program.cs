using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using ClassLibrary1;

namespace Serve
{
    public class Program
    {

        private static Socket socketServer;
        private static float BoradHeight;
        private static float BoradWidth;
        private static void Main(string[] args)
        {
            //Start2Listen();

            Socket socketServer = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            //IPAddress ip = IPAddress.Parse("172.17.0.8");
            //IPAddress ip = IPAddress.Parse("192.168.51.198");
            IPAddress ip = IPAddress.Parse("192.168.51.170");
            //IPAddress ip = IPAddress.Parse("127.0.0.1");
            IPEndPoint ipEndPoint = new IPEndPoint(ip, 9999);
            socketServer.Bind(ipEndPoint);
            socketServer.Listen(100);
            //利用线程后台执行监听,否则程序会假死
            Thread thread = new Thread(Start2Listen);
            thread.IsBackground = true;
            thread.Start(socketServer);
            Console.ReadKey();

            /*Console.WriteLine("Hello World123!");
            Socket serverSocket = new Socket(SocketType.Stream, ProtocolType.Tcp);
            //IPAddress ip = IPAddress.Parse("172.17.0.8");
            IPAddress ip = IPAddress.Parse("192.168.198.1");
            //IPAddress ip = IPAddress.Parse("192.168.51.198");
            IPEndPoint point = new IPEndPoint(ip, 9999);
            //socket绑定监听地址
            serverSocket.Bind(point);
            Console.WriteLine("Listen Success");
            //设置同时连接个数
            serverSocket.Listen(10);

            //利用线程后台执行监听,否则程序会假死
            Thread thread = new Thread(Listen);
            thread.IsBackground = true;
            thread.Start(serverSocket);
            Console.ReadKey();*/
        }

        /// <summary>
        /// 监听连接
        /// </summary>
        /// <param name="o"></param>
        static void Listen(object o)
        {
            var serverSocket = o as Socket;
            while (true)
            {
                try
                {
                    //等待连接并且创建一个负责通讯的socket
                    var send = serverSocket.Accept();
                    //获取链接的IP地址
                    var sendIpoint = send.RemoteEndPoint.ToString();
                    Console.WriteLine($"{sendIpoint}Connection");
                    //开启一个新线程不停接收消息
                    Thread thread = new Thread(Recive);
                    thread.IsBackground = true;
                    thread.Start(send);
                }
                 catch (Exception)
                {

                    throw;
                }
               
            }
        }
        
        /// <summary>
        /// 接收消息
        /// </summary>
        /// <param name="o"></param>
        static void Recive(object o)
        {
            var newSocket = o as Socket;
            while (true)
            {
                try
                {
                    //获取发送过来的消息容器
                    byte[] buffer = new byte[1024 * 1024 ];
                    byte[] buf;
                    string result = "";
                    int count = newSocket.Available;
                    if (count > 0) {
                        buf = new byte[count];
                        int readDataLength = newSocket.Receive(buf);
                        var jsonString = Encoding.UTF8.GetString(buf, 0, readDataLength);
                        SwitchMessage(buf, readDataLength, jsonString, newSocket);
                    }
                    
                    

                    
                    //int readDataLength = newSocket.Receive(data, 0, data.Length, SocketFlags.None);
                    //if (readDataLength == 0)
                    //{
                    //    break;
                    //}
                    //else {

                    //byte[] data = new byte[1024 * 1024];
                    //int readDataLength = newSocket.Receive(data, 0, data.Length, SocketFlags.None);
                    //newSocket.BeginReceive(buffer, 0, buffer.Length, SocketFlags.None, new AsyncCallback(ReceiveMessage), newSocket);
                    //int readDataLength = newSocket.Receive(buffer, 0, buffer.Length, SocketFlags.None);
                    //Thread.Sleep(500);

                    //int index = 0;
                    //while (newSocket.Available > 0)
                    //{//参数 数据缓存区  起始位置  数据长度  值的按位组合
                    //    index = newSocket.Receive(buffer, 0, buffer.Length, SocketFlags.None);
                    //    readDataLength += index;
                    //    Thread.Sleep(500);
                    //}
                    //var jsonString = Encoding.UTF8.GetString(buffer, 0, readDataLength);
                    //if (!string.IsNullOrEmpty(jsonString))
                    //{
                    //SwitchMessage(buffer, readDataLength, jsonString, newSocket);
                    //}




                    //var jsonString = Encoding.UTF8.GetString(data, 0, readDataLength);
                    //SwitchMessage(data, readDataLength, jsonString, newSocket);
                    //}

                }
                catch (Exception)
                {

                    throw;
                }
                
            }
        }


        private static Dictionary<string, Socket> _classSocketsDict = new Dictionary<string, Socket>();
        private static Dictionary<string, List<JsonProtocol>> _studentSocketsDict = new Dictionary<string, List<JsonProtocol>>();
        private static int AllLength = 0;
        private static string AllString = "";
        private static string AllWebString = "";
        private static async void Start2Listen(Object o)
        {
            Socket socketServer = o as Socket;
            await Task.Run(() =>
            {
                while (true)
                {
                    Socket newSocket = socketServer.Accept();
                    Task.Run(() =>
                    {
                        
                        while (true)
                        {
                            try
                            {
                                //获取发送过来的消息容器
                                byte[] buf;
                                int count = newSocket.Available;

                                

                                if (count > 0)
                                {
                                        
                                    buf = new byte[count];
                                    int readDataLength = newSocket.Receive(buf);
                                    //Thread.Sleep(1000);
                                    string jsonString = Encoding.UTF8.GetString(buf, 0, readDataLength);

                                    if (jsonString.Contains("GET"))
                                    {
                                        newSocket.Send(PackageHandShakeData(buf, readDataLength));

                                    }
                                    else {

                                        if (jsonString.Contains("End"))
                                        {
                                            AllLength += readDataLength;
                                            AllString += jsonString.Remove(jsonString.Length - 3);
                                            Console.WriteLine("接收完了");
                                            Console.WriteLine("数据长度：{0}", readDataLength);
                                            Console.WriteLine("这是最后得到的总长度和数据：{0}：{1}", AllLength, AllString);

                                            SwitchMessage(buf, AllLength, AllString, newSocket);
                                            AllLength = 0;
                                            AllString = "";
                                            //Console.WriteLine("有用的数据为：{0}", jsonString.Remove(jsonString.Length - 3));
                                        }
                                        else if (jsonString.Contains("��"))
                                        {
                                            AllLength += readDataLength;
                                            var a = DecodeClientData(buf, readDataLength);
                                            if (a.Contains("Wait"))
                                            {
                                                var b = a.Remove(a.Length - 4);
                                                AllWebString += b;
                                            }
                                            else {
                                                AllWebString += a;
                                                Console.WriteLine("接收完了");
                                                Console.WriteLine("数据长度：{0}", AllLength);
                                                //Console.WriteLine("数据内容：{0}", AllWebString);
                                                JsonProtocol protocolJson = JsonConvert.DeserializeObject<JsonProtocol>(AllWebString);
                                                WebMessage(protocolJson, newSocket);
                                                AllLength = 0;
                                                AllWebString = "";
                                            }


                                            //if (count > 65000)
                                            //{
                                            //    AllLength += readDataLength;
                                            //    byte[] newbyte = buf;
                                            //    test = new byte[AllLength];
                                            //    byte[] oldbyte = test;

                                            //    int index = 0;
                                            //    if (oldbyte[0] == 0)
                                            //    {

                                            //        for (int j = 0; j < newbyte.Length; j++)
                                            //        {
                                            //            test[index] = newbyte[j];
                                            //            index++;
                                            //        }
                                            //    }
                                            //    else
                                            //    {

                                            //        for (int j = 0; j < newbyte.Length; j++)
                                            //        {
                                            //            test[index] = newbyte[j];
                                            //            index++;
                                            //        }
                                            //        for (int i = 0; i < oldbyte.Length; i++)
                                            //        {
                                            //            test[index] = oldbyte[i];
                                            //            index++;
                                            //        }
                                            //    }

                                            //    Console.WriteLine("这是web端的数据大于65000的数据");










                                            //    //string a = DecodeClientData(test, AllLength);

                                            //    Console.WriteLine("没接收完");
                                            //    Console.WriteLine("数据长度：{0}", readDataLength);
                                            //    //Console.WriteLine("数据内容：{0}", a);
                                            //}
                                            //else {
                                            //    Console.WriteLine("这是web端小于65000的数据的数据");
                                            //    AllLength += readDataLength;
                                            //    byte[] newbyte = buf;
                                            //    byte[] oldbyte = test;
                                            //    byte[] allbyte = new byte[AllLength];


                                            //    int index = 0;
                                            //    if (oldbyte[0] == 0)
                                            //    {

                                            //        for (int j = 0; j < newbyte.Length; j++)
                                            //        {
                                            //            allbyte[index] = newbyte[j];
                                            //            index++;
                                            //        }
                                            //    }
                                            //    else {

                                            //        for (int j = 0; j < newbyte.Length; j++)
                                            //        {
                                            //            allbyte[index] = newbyte[j];
                                            //            index++;
                                            //        }
                                            //        for (int i = 0; i < oldbyte.Length; i++)
                                            //        {
                                            //            allbyte[index] = oldbyte[i];
                                            //            index++;
                                            //        }

                                            //    }






                                            //    string a = DecodeClientData(allbyte, AllLength);
                                            //    string b = a.Remove(DecodeClientData(allbyte, AllLength).Length - 3);
                                            //    JsonProtocol protocolJson = JsonConvert.DeserializeObject<JsonProtocol>(b);
                                            //    Console.WriteLine("接收完了");
                                            //    Console.WriteLine("数据长度：{0}", AllLength);
                                            //    Console.WriteLine("数据内容：{0}", b);
                                            //    WebMessage(protocolJson, newSocket);
                                            //    AllLength = 0;
                                            //    AllString = "";
                                            //    test = new byte[10];
                                            //}


                                            //if (DecodeClientData(buf, readDataLength).Contains("End"))
                                            //{
                                            //    AllLength += readDataLength;
                                            //    string a = DecodeClientData(buf, readDataLength);
                                            //    string b = a.Remove(DecodeClientData(buf, readDataLength).Length - 3);
                                            //    AllString += DecodeClientData(buf, readDataLength).Remove(DecodeClientData(buf, readDataLength).Length - 3);
                                            //    Console.WriteLine("接收完了");
                                            //    Console.WriteLine("数据长度：{0}", readDataLength);
                                            //    Console.WriteLine("这是最后得到的总长度和数据：{0}：{1}", AllLength, AllString);
                                            //    JsonProtocol protocolJson = JsonConvert.DeserializeObject<JsonProtocol>(AllString);
                                            //    var aaa = 1;
                                            //    WebMessage(protocolJson, newSocket);
                                            //    AllLength = 0;
                                            //    AllString = "";

                                            //}
                                            //else
                                            //{
                                            //    AllLength += readDataLength;
                                            //    AllString += DecodeClientData(buf, readDataLength);
                                            //    Console.WriteLine("没接收完");
                                            //    Console.WriteLine("数据长度：{0}", readDataLength);
                                            //}

                                        }
                                        else
                                        {
                                            AllLength += readDataLength;
                                            AllString += jsonString;
                                            Console.WriteLine("没接收完");
                                            Console.WriteLine("数据长度：{0}", readDataLength);
                                        }
                                    }


                                    
                                   



                                    //Console.WriteLine("数据长度：{0}，数据内容：{1}", readDataLength, jsonString);




                                    //SwitchMessage(buf, readDataLength, jsonString, newSocket);


                                }
                               
                                //byte[] data = new byte[1024 * 1024 * 10];
                                ////int readDataLength = newSocket.Receive(data, 0, data.Length, SocketFlags.None);

                                //int readDataLength = newSocket.Receive(data, 0, data.Length, SocketFlags.None);


                                //var jsonString = Encoding.UTF8.GetString(data, 0, readDataLength);
                                //SwitchMessage(data, readDataLength, jsonString, newSocket);
                                //else
                                //{
                                //    JsonProtocol protocolJson = JsonConvert.DeserializeObject<JsonProtocol>(jsonString);

                                //    if (protocolJson != null)
                                //    {
                                //        OperateMessages(jsonString, protocolJson, newSocket);
                                //    }
                                //}
                            }
                            catch (Exception e)
                            {
                                return;
                            }
                           
                        }
                        
                        //SwitchMessage(buf, readDataLength, jsonString, newSocket);
                    });
                }
            });
        }

        private static void SwitchMessage(byte[] data, int readDataLength, string jsonString, Socket newSocket)
        {
            if (jsonString.Contains("GET"))
            {
              newSocket.Send(PackageHandShakeData(data, readDataLength));
            }
            else
            {
                JsonProtocol protocolJson = new JsonProtocol();
                JsonProtocol protocolJson2 = new JsonProtocol();

                try
                {
                    protocolJson = JsonConvert.DeserializeObject<JsonProtocol>(DecodeClientData(data, readDataLength));
                }
                catch (Exception)
                {
                }
                try
                {
                    protocolJson2 = JsonConvert.DeserializeObject<JsonProtocol>(jsonString);
                }
                catch (Exception)
                {
                }
                if (protocolJson != null && (int)protocolJson.ConnectionDetail > 3)
                {
                    WebMessage(protocolJson, newSocket);
                }
                else
                {
                    OperateMessages(jsonString, protocolJson2, newSocket);
                }
            }
        }
        private static void WebMessage(JsonProtocol protocolJson, Socket socket)
        {
            try
            {
                if (protocolJson.JsonType == JsonProtocol.JsonTypeEnum.StudentPrepared2Login)
                {

                    if (CoreStudentDal.StudentLogin(protocolJson.UserCode, protocolJson.CLassCode) == true)
                    {
                        if (protocolJson.JsonData == "close")
                        {
                            
                                    //socket.Close();
                        }
                        else
                        {
                           
                            protocolJson.ContentSocket = socket;
                            _studentSocketsDict[protocolJson.CLassCode].Add(protocolJson);

                            JsonProtocol jsonProtocol = new JsonProtocol
                            {
                                ConnectionDetail = JsonProtocol.ConnectionDetailEnum.Server2Student,
                                AimIp = protocolJson.LocalIp,
                                LocalIp = protocolJson.AimIp,
                                UserType = JsonProtocol.UserTypeEnum.Server,
                                JsonType = JsonProtocol.JsonTypeEnum.WebStudentLogin,
                                BoradHeight = (int)Program.BoradHeight,
                                BoradWidth = (int)Program.BoradWidth,
                                JsonData = "true",
                            };
                            string count = JsonConvert.SerializeObject(jsonProtocol);
                            socket.Send(PackageServerData(count));
                        }
                    }
                    else
                    {
                        JsonProtocol jsonProtocol = new JsonProtocol
                        {
                            ConnectionDetail = JsonProtocol.ConnectionDetailEnum.Server2Student,
                            AimIp = protocolJson.LocalIp,
                            LocalIp = protocolJson.AimIp,
                            UserType = JsonProtocol.UserTypeEnum.Server,
                            JsonType = JsonProtocol.JsonTypeEnum.WebStudentLogin,
                            JsonData = "false",
                        };
                        string count = JsonConvert.SerializeObject(jsonProtocol);
                        socket.Send(PackageServerData(count));
                    }
                }




                if (protocolJson.JsonType == JsonProtocol.JsonTypeEnum.StudentSendNormalMessage)
                {
                    protocolJson.UserName = CoreStudentDal.GetStudentName(protocolJson.UserCode);
                    StudentSendMessages(protocolJson);
                }
                if (protocolJson.JsonType == JsonProtocol.JsonTypeEnum.StudentSendPenPoint)
                {
                    //richTextBox1.AppendText(protocolJson.UserCode + $"学生发送了一条消息给老师！" + "\n");
                    //richTextBox1.AppendText(protocolJson.UserCode + $"学生发送了一条消息给老师！" + "\n");
                    //protocolJson.ContentSocket = socket;
                    StudentSendMessages(protocolJson);
                }
            }
            catch (Exception)
            {
            }
        }
        /// <summary>
        /// 操作数据
        /// </summary>
        /// <param name="protocolJson"></param>
        private static void OperateMessages(string jsonString, JsonProtocol protocolJson, Socket socket)
        {
            if (protocolJson.JsonType == JsonProtocol.JsonTypeEnum.TeacherPrepared2Login)
            {
                if (CoreTeacherDal.TeacherLogin(protocolJson.UserName, protocolJson.CLassCode) == false)
                {
                    JsonProtocol jsonProtocol = new JsonProtocol
                    {
                        ConnectionDetail = JsonProtocol.ConnectionDetailEnum.Server2Teacher,
                        AimIp = protocolJson.LocalIp,
                        LocalIp = protocolJson.AimIp,
                        UserType = JsonProtocol.UserTypeEnum.Server,
                        JsonType = JsonProtocol.JsonTypeEnum.TeacherPrepared2Login,
                        JsonData = "false",
                    };
                    //if (protocolJson.JsonData == null) { socket.Close(); }
                    //序列化
                    string count = JsonConvert.SerializeObject(jsonProtocol);
                    var castBytes = Encoding.UTF8.GetBytes(count);
                    socket.Send(castBytes);
                }
                else
                {
                    JsonProtocol jsonProtocol = new JsonProtocol
                    {
                        ConnectionDetail = JsonProtocol.ConnectionDetailEnum.Server2Teacher,
                        AimIp = protocolJson.LocalIp,
                        LocalIp = protocolJson.AimIp,
                        UserType = JsonProtocol.UserTypeEnum.Server,
                        JsonType = JsonProtocol.JsonTypeEnum.TeacherPrepared2Login,
                        JsonData = "true",
                    };
                    string count = JsonConvert.SerializeObject(jsonProtocol);
                    var castBytes = Encoding.UTF8.GetBytes(count);
                    socket.Send(castBytes);

                           
                    _classSocketsDict.Add(protocolJson.CLassCode, socket);
                    _studentSocketsDict.Add(protocolJson.CLassCode, new List<JsonProtocol>());
                    BoradHeight = protocolJson.BoradHeight;
                    BoradWidth = protocolJson.BoradWidth;
                }
            }
            else if (protocolJson.JsonType == JsonProtocol.JsonTypeEnum.TeacherSendNormalMessage)
            {
                        TeacherSendMessages(protocolJson);
            }

            else if (protocolJson.JsonType == JsonProtocol.JsonTypeEnum.TeacherSendImg)
            {

                        TeacherSendMessages(protocolJson);

            }

            else if (protocolJson.JsonType == JsonProtocol.JsonTypeEnum.TeacherPrepared2Exit)
            {

                        if (CoreTeacherDal.TeacherExit(protocolJson.UserName, protocolJson.CLassCode) == true)
                        {
                            _classSocketsDict.Remove(protocolJson.CLassCode);
                            _studentSocketsDict.Remove(protocolJson.CLassCode);
                        }

            }
            else if (protocolJson.JsonType == JsonProtocol.JsonTypeEnum.TeacherSendPenPoint)
            {
                        Console.WriteLine("这是画线");
                        TeacherSendMessages(protocolJson);

            }
            else if (protocolJson.JsonType == JsonProtocol.JsonTypeEnum.TeacherSendRevocation)
            {
                        TeacherSendMessages(protocolJson);

            }
            else if (protocolJson.JsonType == JsonProtocol.JsonTypeEnum.TeacherSendClear)
            {
                        TeacherSendMessages(protocolJson);

            }
            else if (protocolJson.JsonType == JsonProtocol.JsonTypeEnum.StudentPrepared2Login)
            {
                        protocolJson.ContentSocket = socket;
                        _studentSocketsDict[protocolJson.CLassCode].Add(protocolJson);

            }
            else if (protocolJson.JsonType == JsonProtocol.JsonTypeEnum.StudentSendNormalMessage)
            {
                        StudentSendMessages(protocolJson);
            }
            else if (protocolJson.JsonType == JsonProtocol.JsonTypeEnum.StudentPrepared2Exit)
            {
                        _studentSocketsDict[protocolJson.CLassCode].Remove(protocolJson);
            }
            else if (protocolJson.JsonType == JsonProtocol.JsonTypeEnum.StudentSendPenPoint)
            {
                
            }

        }
        /// <summary>
        /// 老师给学生发送信息
        /// </summary>
        /// <param name="jsonProtocol"></param>
        private static void TeacherSendMessages(JsonProtocol jsonProtocol)
        {
            Task.Run(() =>
            {
                foreach (var dict in _studentSocketsDict)
                {
                    if (jsonProtocol.CLassCode.Equals(dict.Key))
                    {
                        foreach (var json in dict.Value)
                        {
                            var newSocket2 = json.ContentSocket;
                            if ((int)json.ConnectionDetail > 3)
                            {
                                try
                                {
                                    string count = JsonConvert.SerializeObject(jsonProtocol);
                                    var a = 1;
                                    Console.WriteLine(count.Length);
                                     a = 2;
                                    newSocket2.Send(PackageServerData(count));
                                }
                                catch (Exception ex)
                                {
                                    Console.WriteLine(ex);
                                }
                            }
                            else
                            {
                                var castBytes = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(jsonProtocol));
                                try
                                {
                                    newSocket2.Send(castBytes);
                                }
                                catch (Exception ex)
                                {
                                    Console.WriteLine(ex);
                                }
                            }
                        }
                    }
                }
            });
        }

        /// <summary>
        /// 学生发送信息
        /// </summary>
        /// <param name="jsonProtocol"></param>
        private static void StudentSendMessages(JsonProtocol jsonProtocol)
        {
            Task.Run(() =>
            {
                foreach (var dict in _classSocketsDict)
                {
                    if (jsonProtocol.CLassCode.Equals(dict.Key))
                    {
                        try
                        {
                            var newSocket2 = dict.Value;
                            //序列化

                            string count = JsonConvert.SerializeObject(jsonProtocol);
                            //count.MaxJsonLength = int.MaxValue;

                            var castBytes = Encoding.UTF8.GetBytes(count);
                            newSocket2.Send(castBytes);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex);
                        }
                    }
                }
            });
        }
        /// <summary>
        /// 打包请求连接数据
        /// </summary>
        /// <param name="handShakeBytes"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        private static byte[] PackageHandShakeData(byte[] handShakeBytes, int length)
        {
            string handShakeText = Encoding.UTF8.GetString(handShakeBytes, 0, length);
            string key = string.Empty;
            Regex reg = new Regex(@"Sec\-WebSocket\-Key:(.*?)\r\n");
            Match m = reg.Match(handShakeText);
            if (m.Value != "")
            {
                key = Regex.Replace(m.Value, @"Sec\-WebSocket\-Key:(.*?)\r\n", "$1").Trim();
            }
            byte[] secKeyBytes = SHA1.Create().ComputeHash(Encoding.ASCII.GetBytes(key + "258EAFA5-E914-47DA-95CA-C5AB0DC85B11"));
            string secKey = Convert.ToBase64String(secKeyBytes);
            var responseBuilder = new StringBuilder();
            responseBuilder.Append("HTTP/1.1 101 Switching Protocols" + "\r\n");
            responseBuilder.Append("Upgrade: websocket" + "\r\n");
            responseBuilder.Append("Connection: Upgrade" + "\r\n");
            responseBuilder.Append("Sec-WebSocket-Accept: " + secKey + "\r\n\r\n");
            return Encoding.UTF8.GetBytes(responseBuilder.ToString());
        }

        /// <summary>
        /// 解析客户端发送来的数据
        /// </summary>
        public static string DecodeClientData(byte[] recBytes, int length)
        {





            if (length < 2)
            {
                return string.Empty;
            }

            bool fin = (recBytes[0] & 0x80) == 0x80; //0x80 = 1000,0000  第1bit = 1表示最后一帧
            if (!fin)
            {
                if (recBytes[1] == 0xff)
                {
                }
                else
                    return string.Empty;
            }

            bool mask_flag = (recBytes[1] & 0x80) == 0x80; // 是否包含掩码
            if (!mask_flag)
            {
                return string.Empty;// 不包含掩码的暂不处理
            }
            int payload_len = recBytes[1] & 0x7F; // 数据长度
            // 获取数据长度
            byte[] masks = new byte[4];
            byte[] payload_data;

            if (payload_len == 126)
            {
                Array.Copy(recBytes, 4, masks, 0, 4);
                payload_len = (UInt16)(recBytes[2] << 8 | recBytes[3]);
                payload_data = new byte[payload_len];
                Array.Copy(recBytes, 8, payload_data, 0, payload_len);
            }
            else if (payload_len == 127)
            {
                Array.Copy(recBytes, 10, masks, 0, 4);
                byte[] uInt64Bytes = new byte[8];
                for (int i = 0; i < 8; i++)
                {
                    uInt64Bytes[i] = recBytes[9 - i];
                }
                UInt64 len = BitConverter.ToUInt64(uInt64Bytes, 0);

                payload_data = new byte[len];
                for (UInt64 i = 0; i < len; i++)
                {
                    payload_data[i] = recBytes[i + 14];
                }
            }
            else
            {
                Array.Copy(recBytes, 2, masks, 0, 4);
                payload_data = new byte[payload_len];
                Array.Copy(recBytes, 6, payload_data, 0, payload_len);
            }

            for (var i = 0; i < payload_len; i++)
            {
                payload_data[i] = (byte)(payload_data[i] ^ masks[i % 4]);
            }
            var con = Encoding.UTF8.GetString(payload_data);
           
            return Encoding.UTF8.GetString(payload_data);
        }
        /// <summary>
        /// 把发送给客户端消息打包处理（拼接上谁什么时候发的什么消息）
        /// </summary>
        /// <returns>The data.</returns>
        /// <param name="message">Message.</param>
        private static byte[] PackageServerData(string msg)
        {
            byte[] content = null;
            byte[] temp = Encoding.UTF8.GetBytes(msg);
            if (temp.Length < 126)
            {
                content = new byte[temp.Length + 2];
                content[0] = 0x81;
                content[1] = (byte)temp.Length;
                Array.Copy(temp, 0, content, 2, temp.Length);
            }
            else if (temp.Length < 65535)
            {
                content = new byte[temp.Length + 4];
                content[0] = 0x81;
                content[1] = 126;
                content[2] = (byte)(temp.Length >> 8 & 0xFF);
                content[3] = (byte)(temp.Length & 0xFF);
                Array.Copy(temp, 0, content, 4, temp.Length);
            }
            else if (temp.Length >= 65535)
            {
                content = new byte[temp.Length + 10];
                content[0] = 0x81;
                content[1] = (byte)127;
                content[2] = 0;
                content[3] = 0;
                content[4] = 0;
                content[5] = 0;
                content[6] = (byte)(temp.Length >> 24);
                content[7] = (byte)(temp.Length >> 16);
                content[8] = (byte)(temp.Length >> 8);
                content[9] = (byte)(temp.Length & 0xFF);
                Array.Copy(temp, 0, content, 10, temp.Length);
            }
            return content;
        }
    }
}
