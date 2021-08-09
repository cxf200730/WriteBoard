using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using Models;
using Models.Server;
using Newtonsoft.Json;
using Utils.Dals;
using Utils.VariableUtils;

namespace 手写板工具服务端
{
    public partial class Server : Form
    {
        public Server()
        {
            InitializeComponent();
            GetLocalIpAndPort();
        }

        /// <summary>
        /// 获得本机IP和端口
        /// </summary>
        private async void GetLocalIpAndPort()
        {
            await Task.Run(() =>
            {
                string name = Dns.GetHostName();
                IPAddress[] ipAddressList = Dns.GetHostAddresses(name);
                var availablePort = GetUsefulPort.GetFirstAvailablePort();

                if (this.InvokeRequired)
                {
                    this.Invoke(new MethodInvoker(delegate
                    {
                        foreach (IPAddress ipa in ipAddressList)
                        {
                            if (ipa.AddressFamily == AddressFamily.InterNetwork)
                            {
                                textBox1.Text = ipa.ToString();

                                if (availablePort == -1)
                                {
                                    textBox2.Text = "9999";
                                }
                                else
                                {
                                    textBox2.Text = availablePort.ToString();
                                }

                                var server = new ServerIpEndPoint
                                {
                                    ServerId = 1,
                                    ServerIp = ipa.ToString(),
                                    ServerPort = availablePort.ToString(),
                                    ServerIsWork = 1,
                                    ServerOnlineTime =
                                        DateTime.Now.ToLongDateString() + " " + DateTime.Now.ToLongTimeString(),
                                    //ServerPublicIp = "101.34.101.58"
                                    ServerPublicIp = ipa.ToString()
                                };
                                MysqlHelper.UpdateServerInfo(server);
                                Start2Listen();
                                return;
                            }
                        }
                    }));
                }
            });
        }

        private Dictionary<string, Socket> _classSocketsDict = new Dictionary<string, Socket>();
        private Dictionary<string, List<JsonProtocol>> _studentSocketsDict = new Dictionary<string, List<JsonProtocol>>();

        /// <summary>
        /// 开始监听
        /// </summary>
        private async void Start2Listen()
        {
            Socket socketServer = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            IPAddress ip = IPAddress.Parse(textBox1.Text);
            IPEndPoint ipEndPoint = new IPEndPoint(ip, int.Parse(textBox2.Text));
            socketServer.Bind(ipEndPoint);
            socketServer.Listen(100);
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
                                byte[] data = new byte[1024 * 1024 * 10];
                                //int readDataLength = newSocket.Receive(data, 0, data.Length, SocketFlags.None);

                                int readDataLength = newSocket.Receive(data, 0, data.Length, SocketFlags.None);
                                if (readDataLength > data.Length)
                                {
                                    while (readDataLength != data.Length)
                                    {
                                        Console.WriteLine("111");
                                        readDataLength += newSocket.Receive(data, readDataLength, data.Length - readDataLength, SocketFlags.None);
                                    }
                                }

                                var jsonString = Encoding.UTF8.GetString(data, 0, readDataLength);
                                SwitchMessage(data, readDataLength, jsonString, newSocket);
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
                    });
                }
            });
        }

        private void SwitchMessage(byte[] data, int readDataLength, string jsonString, Socket newSocket)
        {
            if (jsonString.Contains("GET"))
            {
                if (this.InvokeRequired)
                {
                    this.Invoke(new MethodInvoker(delegate
                    {
                        richTextBox1.AppendText(jsonString);
                        newSocket.Send(PackageHandShakeData(data, readDataLength));
                    }));
                }
            }
            else
            {
                JsonProtocol protocolJson = new JsonProtocol();
                JsonProtocol protocolJson2 = new JsonProtocol();

                try
                {
                    //protocolJson = JsonConvert.DeserializeObject<JsonProtocol>(AnalyzeClientData(data, readDataLength));
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

        private void WebMessage(JsonProtocol protocolJson, Socket socket)
        {
            try
            {
                if (protocolJson.JsonType == JsonProtocol.JsonTypeEnum.StudentPrepared2Login)
                {
                    
                    if (StudentDal.StudentLogin(protocolJson.UserCode, protocolJson.CLassCode) == true)
                    {
                        if (protocolJson.JsonData == "close")
                        {
                            if (this.InvokeRequired)
                            {
                                this.Invoke(new MethodInvoker(delegate
                                {
                                    richTextBox1.AppendText("学生登陆成功，跳转页面" + "\n");
                                    socket.Close();
                                }));
                            }
                        }
                        else {
                            protocolJson.ContentSocket = socket;
                            _studentSocketsDict[protocolJson.CLassCode].Add(protocolJson);

                            JsonProtocol jsonProtocol = new JsonProtocol
                            {
                                ConnectionDetail = JsonProtocol.ConnectionDetailEnum.Server2Student,
                                AimIp = protocolJson.LocalIp,
                                LocalIp = protocolJson.AimIp,
                                UserType = JsonProtocol.UserTypeEnum.Server,
                                JsonType = JsonProtocol.JsonTypeEnum.WebStudentLogin,
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
                    //richTextBox1.AppendText(protocolJson.UserCode + $"学生发送了一条消息给老师！" + "\n");
                    //richTextBox1.AppendText(protocolJson.UserCode + $"学生发送了一条消息给老师！" + "\n");
                    //protocolJson.ContentSocket = socket;
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
            //var uuu = new byte[payload_data.Length * 3 / 4];
            //for (int i = 0; i < uuu.Length; i++)
            //{
            //    uuu[i] = payload_data[i];
            //}
            //Console.WriteLine("UUUUUU：" + Encoding.UTF8.GetString(uuu));
            var con = Encoding.UTF8.GetString(payload_data);
            return Encoding.UTF8.GetString(payload_data);
        }

        #region 处理Web的数据

        /// <summary>
        /// 处理接收的数据
        /// 参考 https://www.cnblogs.com/smark/archive/2012/11/26/2789812.html
        /// </summary>
        /// <param name="recBytes"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        private string AnalyzeClientData(byte[] recBytes, int length)
        {
            int start = 0;
            // 如果有数据则至少包括3位
            if (length < 2) return "";
            // 判断是否为结束针
            bool IsEof = (recBytes[start] >> 7) > 0;
            // 暂不处理超过一帧的数据
            if (!IsEof) return "";
            start++;
            // 是否包含掩码
            bool hasMask = (recBytes[start] >> 7) > 0;
            // 不包含掩码的暂不处理
            if (!hasMask) return "";
            // 获取数据长度
            UInt64 mPackageLength = (UInt64)recBytes[start] & 0x7F;
            start++;
            // 存储4位掩码值
            byte[] Masking_key = new byte[4];
            // 存储数据
            byte[] mDataPackage;
            if (mPackageLength == 126)
            {
                // 等于126 随后的两个字节16位表示数据长度
                mPackageLength = (UInt64)(recBytes[start] << 8 | recBytes[start + 1]);
                start += 2;
            }
            if (mPackageLength == 127)
            {
                // 等于127 随后的八个字节64位表示数据长度
                mPackageLength = (UInt64)(recBytes[start] << (8 * 7) | recBytes[start] << (8 * 6) | recBytes[start] << (8 * 5) | recBytes[start] << (8 * 4) | recBytes[start] << (8 * 3) | recBytes[start] << (8 * 2) | recBytes[start] << 8 | recBytes[start + 1]);
                start += 8;
            }
            mDataPackage = new byte[mPackageLength];
            for (UInt64 i = 0; i < mPackageLength; i++)
            {
                mDataPackage[i] = recBytes[i + (UInt64)start + 4];
            }
            Buffer.BlockCopy(recBytes, start, Masking_key, 0, 4);
            for (UInt64 i = 0; i < mPackageLength; i++)
            {
                mDataPackage[i] = (byte)(mDataPackage[i] ^ Masking_key[i % 4]);
            }
            var count = Encoding.UTF8.GetString(mDataPackage);
            return Encoding.UTF8.GetString(mDataPackage);
        }

        /// <summary>
        /// 把发送给客户端消息打包处理（拼接上谁什么时候发的什么消息）
        /// </summary>
        /// <returns>The data.</returns>
        /// <param name="message">Message.</param>
        /// <summary>
        /// 把发送给客户端消息打包处理（拼接上谁什么时候发的什么消息）
        /// </summary>
        /// <returns>The data.</returns>
        /// <param name="message">Message.</param>
        private byte[] PackageServerData(string msg)
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

        /// <summary>
        /// 打包请求连接数据
        /// </summary>
        /// <param name="handShakeBytes"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        private byte[] PackageHandShakeData(byte[] handShakeBytes, int length)
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

        #endregion 处理Web的数据

        /// <summary>
        /// 操作数据
        /// </summary>
        /// <param name="protocolJson"></param>
        private void OperateMessages(string jsonString, JsonProtocol protocolJson, Socket socket)
        {
            
            if (protocolJson.JsonType == JsonProtocol.JsonTypeEnum.TeacherPrepared2Login)
            {
                if (this.InvokeRequired)
                {
                    this.Invoke(new MethodInvoker(delegate
                    {
                        if (TeacherDal.TeacherLogin(protocolJson.UserName, protocolJson.CLassCode) == false)
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
                            
                            //序列化
                            string count = JsonConvert.SerializeObject(jsonProtocol);
                            var castBytes = Encoding.UTF8.GetBytes(count);
                            socket.Send(castBytes);
                        }
                        else {
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

                            richTextBox1.AppendText(protocolJson.CLassCode + "班级的老师已经登录！" + "\n");
                            _classSocketsDict.Add(protocolJson.CLassCode, socket);
                            _studentSocketsDict.Add(protocolJson.CLassCode, new List<JsonProtocol>());
                        }
                       
                    }));
                }
            }
            else if (protocolJson.JsonType == JsonProtocol.JsonTypeEnum.TeacherSendNormalMessage)
            {
                if (this.InvokeRequired)
                {
                    this.Invoke(new MethodInvoker(delegate
                    {
                        richTextBox1.AppendText(protocolJson.CLassCode + "班级的老师发送了一条消息！" + "\n");

                        //var response = ClassDal.SearchServerInfo(protocolJson.CLassCode);
                        //var infoList = response.Object as List<string>;

                        TeacherSendMessages(protocolJson);
                    }));
                }
            }

            else if (protocolJson.JsonType == JsonProtocol.JsonTypeEnum.TeacherSendImg)
            {
                if (this.InvokeRequired)
                {
                    this.Invoke(new MethodInvoker(delegate
                    {
                        richTextBox1.AppendText(protocolJson.CLassCode + "班级的老师发送了一张课件图片！" + "\n");

                        //var response = ClassDal.SearchServerInfo(protocolJson.CLassCode);
                        //var infoList = response.Object as List<string>;

                        TeacherSendMessages(protocolJson);
                    }));
                }
            }

            else if (protocolJson.JsonType == JsonProtocol.JsonTypeEnum.TeacherPrepared2Exit)
            {
                if (this.InvokeRequired)
                {
                    this.Invoke(new MethodInvoker(delegate
                    {
                        if (TeacherDal.TeacherExit(protocolJson.UserName, protocolJson.CLassCode) == true) {
                            MessageBox.Show("成功退出");
                            richTextBox1.AppendText(protocolJson.CLassCode + "班级的老师退出了登录，班级已经关闭！" + "\n");
                            _classSocketsDict.Remove(protocolJson.CLassCode);
                            _studentSocketsDict.Remove(protocolJson.CLassCode);
                        }
                    }));
                }
            }
            else if (protocolJson.JsonType == JsonProtocol.JsonTypeEnum.TeacherSendPenPoint)
            {
                if (this.InvokeRequired)
                {
                    this.Invoke(new MethodInvoker(delegate
                    {
                        richTextBox1.AppendText(protocolJson.CLassCode + "班级的老师书写了一个笔迹！" + "\n");
                        //richTextBox1.AppendText(protocolJson.JsonData + "\n");
                        TeacherSendMessages(protocolJson);
                    }));
                }
            }
            else if (protocolJson.JsonType == JsonProtocol.JsonTypeEnum.TeacherSendRevocation)
            {
                if (this.InvokeRequired)
                {
                    this.Invoke(new MethodInvoker(delegate
                    {
                        richTextBox1.AppendText(protocolJson.CLassCode + "班级的老师撤销了一个笔迹！" + "\n");
                        //richTextBox1.AppendText(protocolJson.JsonData + "\n");
                        TeacherSendMessages(protocolJson);
                    }));
                }
            }
            else if (protocolJson.JsonType == JsonProtocol.JsonTypeEnum.TeacherSendClear)
            {
                if (this.InvokeRequired)
                {
                    this.Invoke(new MethodInvoker(delegate
                    {
                        richTextBox1.AppendText(protocolJson.CLassCode + "班级的老师清空了画板" + "\n");
                        TeacherSendMessages(protocolJson);
                    }));
                }
            }
            else if (protocolJson.JsonType == JsonProtocol.JsonTypeEnum.StudentPrepared2Login)
            {
                if (this.InvokeRequired)
                {
                    this.Invoke(new MethodInvoker(delegate
                    {
                        richTextBox1.AppendText(protocolJson.UserCode + $"学生登录到服务器，连接到班级{protocolJson.CLassCode}" + "\n");
                        protocolJson.ContentSocket = socket;
                        _studentSocketsDict[protocolJson.CLassCode].Add(protocolJson);
                        //_studentSocketsDict.Add(protocolJson.CLassCode, );
                    }));
                }
            }
            else if (protocolJson.JsonType == JsonProtocol.JsonTypeEnum.StudentSendNormalMessage)
            {
                if (this.InvokeRequired)
                {
                    this.Invoke(new MethodInvoker(delegate
                    {
                        richTextBox1.AppendText(protocolJson.UserCode + $"学生发送了一条消息给老师！" + "\n");
                        StudentSendMessages(protocolJson);
                    }));
                }
            }
            else if (protocolJson.JsonType == JsonProtocol.JsonTypeEnum.StudentPrepared2Exit)
            {
                if (this.InvokeRequired)
                {
                    this.Invoke(new MethodInvoker(delegate
                    {
                        richTextBox1.AppendText(protocolJson.UserCode + $"学生退出房间，退出服务器！" + "\n");
                        _studentSocketsDict[protocolJson.CLassCode].Remove(protocolJson);
                    }));
                }
            }
            else if (protocolJson.JsonType == JsonProtocol.JsonTypeEnum.StudentSendPenPoint)
            {
                if (this.InvokeRequired)
                {
                    this.Invoke(new MethodInvoker(delegate
                    {
                        //richTextBox1.AppendText(protocolJson.JsonData + "\n");
                    }));
                }
            }
        }

        /// <summary>
        /// 老师给学生发送信息
        /// </summary>
        /// <param name="jsonProtocol"></param>
        private void TeacherSendMessages(JsonProtocol jsonProtocol)
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
        private void StudentSendMessages(JsonProtocol jsonProtocol)
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
        /// 服务端初次加载的时候
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Server_Load(object sender, EventArgs e)
        {
        }
    }
}