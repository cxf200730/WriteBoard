using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Models;
using Models.Server;
using Newtonsoft.Json;
using Qiniu.Http;
using Qiniu.Storage;
using Qiniu.Util;
using RobotpenGateway;
using Spire.Pdf;
using Utils.StaticUtils;
using Utils.VariableUtils;

namespace 手写板工具
{
    public partial class TeacherClient : Form
    {
        private static Socket socketClient;
        private static IPEndPoint ipEndPoint;
        private string publicServerIp = "";
        private int _deviceConnectStatus = -1;
        private byte[] buffer = new byte[1024 * 1024];
        private WritingDeviceInfo deviveInfo = new WritingDeviceInfo();

        [System.Runtime.InteropServices.DllImport("kernel32.dll", CharSet = System.Runtime.InteropServices.CharSet.Auto)]
        public static extern void OutputDebugString(string message);

        private RobotpenGateway.robotpenController.returnPointData _pointData = null;

        private float _picAndDevicewidthRate = 0.0000f;
        private float _picAndDeviceHeightRate = 0.0000f;

        private int currentPage = 0;
        private int totalPageCount = 0;
        private string imgPath = "";
        public TeacherClient()
        {
            InitializeComponent();
            PreLoad();
            InitPen();
            Wait2Connect();
        }

        #region 罗博笔相关

        private eDeviceType deviceType;
        private eDeviceMode eDMode;
        private PointInfo _prePoint = new PointInfo(-1, -1);
        private PointInfo _pointObj = new PointInfo(-1, -1);
        private PointInfo pointObj = new PointInfo(-1, -1);
        private int _deviceWidth;
        private int _deviceHeight;
        private List<PointInfo[]> pointList = new List<PointInfo[]>();
        private List<PointInfo[]> tempList = new List<PointInfo[]>();
        private List<PointInfo[]> saveList = new List<PointInfo[]>();
        /// <summary>
        /// 初始化笔
        /// </summary>
        private void InitPen()
        {
            this.Text = "手写板工具-教师端  -未连接到手写板";
            _pointData = new RobotpenGateway.robotpenController.returnPointData(GetPointData);
            robotpenController.GetInstance().initDeletgate(ref _pointData);
        }

        /// <summary>
        /// 获得点的值
        /// </summary>
        /// <param name="bIndex"></param>
        /// <param name="bPenStatus"></param>
        /// <param name="bx"></param>
        /// <param name="by"></param>
        /// <param name="bPress"></param>
        private void GetPointData(byte bIndex, byte bPenStatus, short bx, short by, short bPress)
        {
            if (button2.Enabled == true)
            {
                if ((bPenStatus == 16 || bPenStatus == 0) && !_prePoint.X.Equals(-1))
                {
                    _prePoint = new PointInfo(-1, -1);
                }


                ReadPointDataNotSend(bIndex, bPenStatus, bx, by, bPress);

                return;
            }

            if ((bPenStatus == 16 || bPenStatus == 0) && !_prePoint.X.Equals(-1))
            {
                EndWriteLine();
            }
            ReadPointData(bIndex, bPenStatus, bx, by, bPress);
        }

        /// <summary>
        /// 结束画线
        /// </summary>
        private void EndWriteLine()
        {
            _prePoint = new PointInfo(-1, -1);

            if (tempList.Count != 0)
            {

                string result = JsonConvert.SerializeObject(tempList);

                JsonProtocol jsonProtocol = new JsonProtocol
                {
                    ConnectionDetail = JsonProtocol.ConnectionDetailEnum.Teacher2Server,
                    AimIp = publicServerIp,
                    LocalIp = GetLocalIp(),
                    UserType = JsonProtocol.UserTypeEnum.Teacher,
                    UserName = textBox2.Text,
                    UserCode = textBox2.Text,
                    CLassCode = textBox1.Text,
                    BoradWidth = _deviceWidth,
                    BoradHeight = _deviceHeight,
                    JsonType = JsonProtocol.JsonTypeEnum.TeacherSendPenPoint,
                    JsonData = result
                };
                File.Copy(@"D:\test2.png", @"D:\test1.png", true);
                this.Invoke(new MethodInvoker(delegate
                { savePicture(2,false); }));

                SendMessage2Server(JsonConvert.SerializeObject(jsonProtocol));
                tempList.Clear();



            }
        }

        /// <summary>
        /// 切换为手写模式
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button3_Click(object sender, EventArgs e)
        {
            if (deviceType == eDeviceType.T8B_D2 || deviceType == eDeviceType.T8Y || deviceType == eDeviceType.T8S_LQ || deviceType == eDeviceType.J7M)
            {
                if (eDMode == eDeviceMode.DEVICE_HAND)
                {
                    robotpenController.GetInstance().setDeviceMode(eDeviceMode.DEVICE_MOUSE);
                    eDMode = eDeviceMode.DEVICE_MOUSE;
                    button3.Text = "切换为手写模式";
                }
                else
                {
                    robotpenController.GetInstance().setDeviceMode(eDeviceMode.DEVICE_HAND);
                    eDMode = eDeviceMode.DEVICE_HAND;
                    button3.Text = "切换为鼠标模式";
                }
            }
            robotpenController.GetInstance()._Send(cmdId.SearchMode);
        }

        /// <summary>
        /// 读取数据并画线
        /// </summary>
        /// <param name="bIndex"></param>
        /// <param name="bPenStatus"></param> // 17 按压  16 悬浮  0 离开手写板
        /// <param name="bx"></param>
        /// <param name="by"></param>
        /// <param name="bPress"></param>
        private void ReadPointData(int bIndex, int bPenStatus, float bx, float by, float bPress)
        {

            PointInfo point = new PointInfo(bx, by);
            PointInfo pointObj = new PointInfo(bx, by);
            if (_prePoint.X.Equals(-1))
            {
                _prePoint = point;
            }
            if (_pointObj.X.Equals(-1))
            {
                _pointObj = pointObj;
            }
            //Console.WriteLine($"x:{point.X} - y:{point.Y} - bPress:{bPress} - bIndex:{bIndex} - bPenStatus:{bPenStatus}");

            Graphics g = pictureBox1.CreateGraphics();
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;//消除锯齿
            g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;

            if (bPress > 0 && !point.X.Equals(-1))
            {

                var _x = Convert.ToInt32(pictureBox1.Width - _prePoint.X * 1.0000f / _picAndDevicewidthRate);
                var _y = Convert.ToInt32(pictureBox1.Height - _prePoint.Y * 1.0000f / _picAndDeviceHeightRate);
                var x = Convert.ToInt32(pictureBox1.Width - point.X * 1.0000f / _picAndDevicewidthRate);
                var y = Convert.ToInt32(pictureBox1.Height - point.Y * 1.0000f / _picAndDeviceHeightRate);

                _pointObj.X = _x;
                _pointObj.Y = _y;
                pointObj.X = x;
                pointObj.Y = y;
                g.DrawLine(new Pen(Color.Red, 2), new PointF(_x, _y), new PointF(x, y));

                tempList.Add(new PointInfo[]
                {
                    _prePoint,
                    point
                });
                pointList.AddRange(tempList);
                saveList.Add(new PointInfo[]
                       {
                             _pointObj,
                              pointObj
                       });



                _pointObj = pointObj;

                pointObj = null;

                _prePoint = point;
            }
        }

        /// <summary>
        /// 画点但不发送
        /// </summary>
        /// <param name="bIndex"></param>
        /// <param name="bPenStatus"></param>
        /// <param name="bx"></param>
        /// <param name="by"></param>
        /// <param name="bPress"></param>
        private void ReadPointDataNotSend(int bIndex, int bPenStatus, float bx, float by, float bPress)
        {

            PointInfo point = new PointInfo(bx, by);
            PointInfo pointObj = new PointInfo(bx, by);
            if (_prePoint.X.Equals(-1))
            {
                _prePoint = point;
            }
            if (_pointObj.X.Equals(-1))
            {
                _pointObj = pointObj;
            }
            //Console.WriteLine($"x:{point.X} - y:{point.Y} - bPress:{bPress} - bIndex:{bIndex} - bPenStatus:{bPenStatus}");


            Graphics g = pictureBox1.CreateGraphics();
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;//消除锯齿
            g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
            if (bPress > 0 && !point.X.Equals(-1))
            {
                if (this.InvokeRequired)
                {
                    this.Invoke(new MethodInvoker(delegate
                    {

                        var _x = Convert.ToInt32(pictureBox1.Width - _prePoint.X * 1.0000f / _picAndDevicewidthRate);
                        var _y = Convert.ToInt32(pictureBox1.Height - _prePoint.Y * 1.0000f / _picAndDeviceHeightRate);
                        var x = Convert.ToInt32(pictureBox1.Width - point.X * 1.0000f / _picAndDevicewidthRate);
                        var y = Convert.ToInt32(pictureBox1.Height - point.Y * 1.0000f / _picAndDeviceHeightRate);

                        //Console.WriteLine(1111111111111111);
                        //Console.WriteLine(_x + " + " + _y);
                        //Console.WriteLine(x + " + " + y);
                        _pointObj.X = _x;
                        _pointObj.Y = _y;
                        pointObj.X = x;
                        pointObj.Y = y;

                        g.DrawLine(new Pen(Color.Red, 2), new PointF(_x, _y), new PointF(x, y));

                        saveList.Add(new PointInfo[]
                       {
                             _pointObj,
                              pointObj
                       });



                        _pointObj = pointObj;
                        _prePoint = point;
                    }));
                }
            }
        }

        /// <summary>
        /// 等待手写板自动连接
        /// </summary>
        private void Wait2Connect()
        {
            Task.Run(() =>
            {
                while (true)
                {
                    Thread.Sleep(1000);
                    if (_deviceConnectStatus == 1)
                    {
                        continue;
                    }
                    else
                    {
                        _deviceConnectStatus = -1;
                        int nDeviceCount = robotpenController.GetInstance()._GetDeviceCount();
                        if (nDeviceCount > 0)
                        {
                            for (int i = 0; i < nDeviceCount; ++i)
                            {
                                ushort npid = 0;
                                ushort nvid = 0;
                                string strDeviceName = string.Empty;
                                eDeviceType dtype = eDeviceType.Unknow;
                                if (robotpenController.GetInstance()._GetAvailableDevice(i, ref npid, ref nvid, ref strDeviceName, ref dtype))
                                {
                                    if (_deviceConnectStatus != 1)
                                    {
                                        _deviceConnectStatus = 1;
                                        robotpenController.GetInstance()._ConnectInitialize(dtype, IntPtr.Zero);
                                        int nRes = robotpenController.GetInstance()._ConnectOpen();
                                        if (nRes != 0)
                                        {
                                            MessageBox.Show("设备自动连接失败，请重新插拔设备或尝试手动连接!");
                                            _deviceConnectStatus = -1;
                                            break;
                                        }
                                        //robotpenController.GetInstance()._Send(cmdId.SwitchMode);

                                        _deviceWidth = robotpenController.GetInstance().getWidth();
                                        _deviceHeight = robotpenController.GetInstance().getHeight();
                                        _picAndDevicewidthRate = (robotpenController.GetInstance().getWidth() * 1.0000f) / (pictureBox1.Width * 1.0000f);
                                        _picAndDeviceHeightRate = (robotpenController.GetInstance().getHeight() * 1.0000f) / (pictureBox1.Height * 1.0000f);

                                        //InitPictureBox();
                                        robotpenController.GetInstance().setDeviceMode(eDeviceMode.DEVICE_HAND);
                                        eDMode = eDeviceMode.DEVICE_HAND;
                                        deviceType = robotpenController.GetInstance().Device_Type;
                                        robotpenController.GetInstance()._Send(cmdId.GetConfig);
                                    }
                                    deviveInfo.DeviceName = strDeviceName;
                                    deviveInfo.NVId = nvid;
                                    deviveInfo.NPId = npid;
                                    deviveInfo.DType = (int)dtype;
                                }
                            }

                            if (this.InvokeRequired)
                            {
                                this.Invoke(new MethodInvoker(delegate
                                {
                                    this.Text = $"手写板工具-教师端  -手写板{deviceType}连接成功";
                                    button2.Enabled = true;
                                    button4.Enabled = true;
                                }));
                            }
                        }
                    }
                }
                //    }));
                //}
            });
        }

        #endregion 罗博笔相关

        #region Socket相关

        /// <summary>
        /// 给服务端发送消息
        /// </summary>
        /// <param name="result"></param>
        private void SendMessage2Server(string result)
        {
            Console.WriteLine("这是老师端发送的数据，长度为：");
            Console.WriteLine(Encoding.UTF8.GetBytes(result).Length);
            socketClient.Send(Encoding.UTF8.GetBytes(result + "End"));
            //stream?.WriteAsync(Encoding.UTF8.GetBytes(result), 0, Encoding.UTF8.GetBytes(result).Length);
        }
        //新建TCP客户端
        private TcpClient tcpclient = new TcpClient();
        //提供网络访问的基础流的数据
        private NetworkStream stream = null;
        /// <summary>
        /// 初始化套接字内容
        /// </summary>
        private void InitSocketContent(string userName, string classCode, out int status)
        {
            socketClient = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            status = 0;
            var response = MysqlHelper.SearchServerInfo();
            if (response.State == BaseResponseState.Success)
            {
                var infoList = response.Object as List<ServerIpEndPoint>;
                /*publicServerIp = infoList[0].ServerPublicIp;*/

                IPAddress ip = IPAddress.Parse("101.34.101.58");
                //IPAddress ip = IPAddress.Parse("192.168.51.198");
                //IPAddress ip = IPAddress.Parse("192.168.51.170");
                ipEndPoint = new IPEndPoint(ip, 9999);

                socketClient.Connect(ipEndPoint);

                //建立连接
                //tcpclient.Connect(ip, 9999);
                //返回用于发送和接收数据的 System.Net.Sockets.NetworkStream。
                //stream = tcpclient.GetStream();


                JsonProtocol jsonProtocol = new JsonProtocol
                {
                    ConnectionDetail = JsonProtocol.ConnectionDetailEnum.Teacher2Server,
                    //AimIp = "192.168.51.198",
                    AimIp = "192.168.51.198",
                    LocalIp = GetLocalIp(),
                    UserType = JsonProtocol.UserTypeEnum.Teacher,
                    UserName = userName,
                    CLassCode = classCode,
                    BoradWidth = _deviceWidth,
                    BoradHeight = _deviceHeight,
                    JsonType = JsonProtocol.JsonTypeEnum.TeacherPrepared2Login,
                    JsonData = "这是登录这是登录这是登录这是登录",
                    ContentSocket = null,
                };
                var vv = JsonConvert.SerializeObject(jsonProtocol);
                Console.WriteLine(vv.Length);

                socketClient.Send(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(jsonProtocol) + "End"));
                //stream.WriteAsync(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(jsonProtocol)), 0, vv.Length + 4);


                //if (byteLength > 0)
                //{
                //    status = 1;
                //}
                //else
                //{
                //    status = -1;
                //    return;
                //}
                Start2Listen();
            }
        }

        /// <summary>
        /// 开始监听服务器
        /// </summary>
        private void Start2Listen()
        {
            socketClient.BeginReceive(buffer, 0, buffer.Length, SocketFlags.None, new AsyncCallback(ReceiveMessage), socketClient);
        }

        /// <summary>
        /// 接收消息的委托方法
        /// </summary>
        /// <param name="ar"></param>
        public void ReceiveMessage(IAsyncResult ar)
        {
            try
            {
                var socket = ar.AsyncState as Socket;
                var length = socket.EndReceive(ar);
                JsonProtocol protocolJson = JsonConvert.DeserializeObject<JsonProtocol>(Encoding.UTF8.GetString(buffer, 0, length));

                //Console.WriteLine(protocolJson.JsonData);
                OperateMessages(protocolJson);
                socket.BeginReceive(buffer, 0, buffer.Length, SocketFlags.None, new AsyncCallback(ReceiveMessage), socket);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        /// <summary>
        /// 信息操作方法
        /// </summary>
        /// <param name="protocolJson"></param>
        private void OperateMessages(JsonProtocol protocolJson)
        {
            if (protocolJson.JsonType == JsonProtocol.JsonTypeEnum.TeacherPrepared2Login)
            {
                this.Invoke(new MethodInvoker(delegate
                {
                    if (protocolJson.JsonData == "false")
                    {
                        MessageBox.Show("信息输入有误,请重新登录");
                    }
                    else
                    {
                        button2.Text = "正在直播...";
                        textBox1.ReadOnly = true;
                        textBox2.ReadOnly = true;
                        button2.Enabled = false;
                        MessageBox.Show("登陆成功");
                    }

                }));

            }
            else if (protocolJson.JsonType == JsonProtocol.JsonTypeEnum.StudentSendPenPoint)
            {
                savePicture(1, false);
                
                var jsonPoint = protocolJson.JsonData;
                try
                {
                    var pointListDes = JsonConvert.DeserializeObject<List<PointInfo[]>>(jsonPoint);

                    foreach (var pointInfos in pointListDes)
                    {
                        Graphics g = pictureBox1.CreateGraphics();
                        g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;//消除锯齿
                        g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                        if (pointInfos[1] == null) {
                            
                        }
                        else 
                            //(!pointInfos[1].X.Equals(-1) || !pointInfos[1].Y.Equals(-1))
                        {
                            g.DrawLine(new Pen(Color.Black, 3), new PointF(
                           (float)(pictureBox1.Width - pointInfos[0].X * 1.0000f / _picAndDevicewidthRate), (float)(pictureBox1.Height - pointInfos[0].Y * 1.0000f / _picAndDeviceHeightRate)),
                           new PointF(
                           (float)(pictureBox1.Width - pointInfos[1].X * 1.0000f / _picAndDevicewidthRate), (float)(pictureBox1.Height - pointInfos[1].Y * 1.0000f / _picAndDeviceHeightRate))
                            );
                        }
                    }
                    savePicture(2, false);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
                savePicture(2, false);
            }
            else if (protocolJson.JsonType == JsonProtocol.JsonTypeEnum.StudentSendNormalMessage)
            {
                //var jsonPoint = protocolJson.JsonData;
                this.Invoke(new MethodInvoker(delegate
                {
                    richTextBox1.AppendText(protocolJson.UserName + $":" + protocolJson.JsonData + "\n");

                    JsonProtocol jsonProtocol = new JsonProtocol
                    {
                        ConnectionDetail = JsonProtocol.ConnectionDetailEnum.Teacher2Server,
                        AimIp = publicServerIp,
                        LocalIp = GetLocalIp(),
                        UserType = JsonProtocol.UserTypeEnum.Student,
                        UserName = protocolJson.UserName,
                        UserCode = protocolJson.UserCode,
                        CLassCode = protocolJson.CLassCode,
                        BoradWidth = _deviceWidth,
                        BoradHeight = _deviceHeight,
                        JsonType = JsonProtocol.JsonTypeEnum.TeacherSendNormalMessage,
                        JsonData = protocolJson.JsonData,
                    };

                    socketClient.Send(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(jsonProtocol) + "End"));
                }));


            }
        }

        /// <summary>
        /// 获得本机IP和端口
        /// </summary>
        private string GetLocalIp()
        {
            string name = Dns.GetHostName();
            IPAddress[] ipAddressList = Dns.GetHostAddresses(name);

            foreach (IPAddress ipa in ipAddressList)
            {
                if (ipa.AddressFamily == AddressFamily.InterNetwork)
                {
                    return ipa.ToString();
                }
            }

            return "";
        }

        #endregion Socket相关

        #region 界面相关

        private void PreLoad()
        {
            button2.Enabled = false;
            button4.Enabled = false;
        }

        /// <summary>
        /// 初始化图片盒
        /// </summary>

        /// <summary>
        /// 窗体关闭的时候
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TeacherClient_FormClosing(object sender, FormClosingEventArgs e)
        {
            JsonProtocol jsonProtocol = new JsonProtocol
            {
                ConnectionDetail = JsonProtocol.ConnectionDetailEnum.Teacher2Server,
                AimIp = publicServerIp,
                LocalIp = GetLocalIp(),
                UserType = JsonProtocol.UserTypeEnum.Teacher,
                UserName = Guid.NewGuid().ToString().Substring(0, 5),
                UserCode = "0000",
                CLassCode = textBox1.Text,
                BoradWidth = _deviceWidth,
                BoradHeight = _deviceHeight,
                JsonType = JsonProtocol.JsonTypeEnum.TeacherPrepared2Exit,
                JsonData = "退出登录"
            };

            //use null propagation
            var result = socketClient?.Send(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(jsonProtocol) + "End"));

            Thread.Sleep(1000);
            System.Environment.Exit(0);
        }

        /// <summary>
        /// 发送消息
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button1_Click(object sender, EventArgs e)
        {

            JsonProtocol jsonProtocol = new JsonProtocol
            {
                ConnectionDetail = JsonProtocol.ConnectionDetailEnum.Teacher2Server,
                AimIp = publicServerIp,
                LocalIp = GetLocalIp(),
                UserType = JsonProtocol.UserTypeEnum.Student,
                UserName = textBox2.Text,
                CLassCode = textBox1.Text,
                BoradWidth = _deviceWidth,
                BoradHeight = _deviceHeight,
                JsonType = JsonProtocol.JsonTypeEnum.TeacherSendNormalMessage,
                JsonData = textBox5.Text,
            };
            richTextBox1.AppendText(textBox2.Text + $":" + textBox5.Text + "\n");
            textBox5.Text = null;
            socketClient.Send(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(jsonProtocol) + "End"));

        }

        #endregion 界面相关

        /// <summary>
        /// 开启直播  登录 ，将text box 中的用户信息发送到服务器
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button2_Click(object sender, EventArgs e)
        {
            //this.MaximizeBox = false;

            var classCode = textBox1.Text;
            var userName = textBox2.Text;

            if (string.IsNullOrEmpty(classCode) || string.IsNullOrEmpty(userName))
            {
                AlertHelp.AlertInformation("请输入用户名或者房间号以后再开启直播！");
                return;
            }

            InitSocketContent(userName, classCode, out int status);
            if (status == 1)
            {
                //button2.Text = "正在直播...";
                //textBox1.ReadOnly = true;
                //textBox2.ReadOnly = true;
                //button2.Enabled = false;
            }
        }

        /// <summary>
        /// 结束直播
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button4_Click(object sender, EventArgs e)
        {
            JsonProtocol jsonProtocol = new JsonProtocol
            {
                ConnectionDetail = JsonProtocol.ConnectionDetailEnum.Teacher2Server,
                AimIp = publicServerIp,
                LocalIp = GetLocalIp(),
                UserType = JsonProtocol.UserTypeEnum.Teacher,
                UserName = textBox2.Text,
                UserCode = textBox2.Text,
                CLassCode = textBox1.Text,
                BoradWidth = _deviceWidth,
                BoradHeight = _deviceHeight,
                JsonType = JsonProtocol.JsonTypeEnum.TeacherPrepared2Exit,
                JsonData = "退出登录"
            };

            //use null propagation
            socketClient?.Send(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(jsonProtocol) + "End"));

            button2.Text = "开启直播";
            textBox1.ReadOnly = false;
            textBox2.ReadOnly = false;
            button2.Enabled = true;
            // this.MaximizeBox = true;
        }

        /// <summary>
        /// 清空笔迹
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button5_Click(object sender, EventArgs e)
        {
            this.pictureBox1.Image = null;
            File.Copy(@"D:\test2.png", @"D:\test1.png", true);
            File.Copy(@"D:\test3.png", @"D:\test2.png", true);
            JsonProtocol jsonProtocol = new JsonProtocol
            {
                ConnectionDetail = JsonProtocol.ConnectionDetailEnum.Teacher2Server,
                AimIp = publicServerIp,
                LocalIp = GetLocalIp(),
                UserType = JsonProtocol.UserTypeEnum.Teacher,
                UserName = textBox2.Text,
                UserCode = textBox2.Text,
                CLassCode = textBox1.Text,
                BoradWidth = _deviceWidth,
                BoradHeight = _deviceHeight,
                JsonType = JsonProtocol.JsonTypeEnum.TeacherSendClear,
                JsonData = "清空画板"
            };

            //use null propagation
            socketClient?.Send(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(jsonProtocol) + "End"));
        }

        /// <summary>
        /// 窗体大小改变的时候
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TeacherClient_SizeChanged(object sender, EventArgs e)
        {

        }

        private void button6_Click(object sender, EventArgs e)
        {
            Console.WriteLine("wo");
            //Graphics g = pictureBox1.CreateGraphics();

            //g.DrawLine(new Pen(Color.Black, 5), new PointF((float)0, (float)0), new PointF((float)1000, (float)1000));
            Graphics g = pictureBox1.CreateGraphics();
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;//消除锯齿
            g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
            Pen MyPen = new Pen(Color.Red, 2f);
            g.DrawLine(MyPen, new PointF((float)50.00, (float)20.00), new PointF((float)300.0, (float)200.00));
            Console.WriteLine("13");
        }




        private void button8_Click(object sender, EventArgs e) //打开pdf文件
        {
            OpenFileDialog openFile = new OpenFileDialog();
            openFile.Filter = "PDF文档(*.pdf)|*.pdf";
            if (openFile.ShowDialog() == DialogResult.OK)
            {
                textBox3.Text = openFile.FileName;
            }
        }

        private void button9_Click(object sender, EventArgs e) //打开pdf文件
        {
            pictureBox1.Image = null;
            if (!string.IsNullOrEmpty(textBox3.Text))
            {
                string file = textBox3.Text;
                //初始化一个PdfDocument类实例,并加载PDF文档

                PdfDocument doc = new PdfDocument();

                doc.LoadFromFile(file);

                //遍历PDF每一页

                for (int i = 0; i < doc.Pages.Count; i++)

                {
                    //将PDF页转换成Bitmap图形

                    System.Drawing.Image bmp = doc.SaveAsImage(i);

                    //将Bitmap图形保存为Png格式的图片
                    int len = file.Split('\\').Length;
                    string filename = file.Split('\\')[len - 1].Split('.')[0];
                    string path = "D:/课件/" + filename;
                    string fileName = string.Format("-{0}.png", i + 1);

                    bmp.Save(path + fileName, System.Drawing.Imaging.ImageFormat.Png);
                    if (pictureBox1.Image == null)
                    {
                        try
                        {
                            pictureBox1.Image = (Image)GetThumbnail(new Bitmap(path + fileName),
                                pictureBox1.Height, pictureBox1.Width);
                            imgPath = path + fileName;
                        }
                        catch (FileNotFoundException)
                        {
                            Console.Write("找不到文件！");
                        }
                    }
                    uploadImg(filename + fileName);
                }
                currentPage = 1;
                textBox4.Text = currentPage.ToString();
                totalPageCount = doc.Pages.Count;
                label8.Text = "共" + totalPageCount + "页";

                sendImgToServer();
            }
        }

        private void button10_Click(object sender, EventArgs e) //点击上一页
        {
            if (currentPage > 1)
            {
                currentPage--;
                textBox4.Text = currentPage.ToString();
                string file = textBox3.Text;
                int len = file.Split('\\').Length;
                string filename = file.Split('\\')[len - 1].Split('.')[0];
                string path = "D:/课件/" + filename + "-" + currentPage + ".png";
                try
                {
                    pictureBox1.Image = GetThumbnail(new Bitmap(path),
                        pictureBox1.Height, pictureBox1.Width);
                    imgPath = path;
                }
                catch (FileNotFoundException)
                {
                    Console.Write("找不到文件！");
                }
            }
            sendImgToServer();
        }

        private void button11_Click(object sender, EventArgs e)//点击下一页
        {
            if (currentPage < totalPageCount)
            {
                currentPage++;
                textBox4.Text = currentPage.ToString();
                string file = textBox3.Text;
                int len = file.Split('\\').Length;
                string filename = file.Split('\\')[len - 1].Split('.')[0];
                string path = "D:/课件/" + filename + "-" + currentPage + ".png";
                try
                {
                    pictureBox1.Image = GetThumbnail(new Bitmap(path),
                        pictureBox1.Height, pictureBox1.Width);
                    imgPath = path;
                }
                catch (FileNotFoundException)
                {
                    Console.Write("找不到文件！");
                }
            }
            sendImgToServer();
        }

        public Bitmap GetThumbnail(Bitmap b, int destHeight, int destWidth) //图片自适应容器大小
        {
            System.Drawing.Image imgSource = b;
            System.Drawing.Imaging.ImageFormat thisFormat = imgSource.RawFormat;
            int sW = 0, sH = 0;
            // 按比例缩放
            int sWidth = imgSource.Width;
            int sHeight = imgSource.Height;
            if (sHeight > destHeight || sWidth > destWidth)
            {
                if ((sWidth * destHeight) > (sHeight * destWidth))
                {
                    sW = destWidth;
                    sH = (destWidth * sHeight) / sWidth;
                }
                else
                {
                    sH = destHeight;
                    sW = (sWidth * destHeight) / sHeight;
                }
            }
            else
            {
                sW = sWidth;
                sH = sHeight;
            }
            Bitmap outBmp = new Bitmap(destWidth, destHeight);
            Graphics g = Graphics.FromImage(outBmp);
            g.Clear(Color.Transparent);
            // 设置画布的描绘质量
            g.CompositingQuality = CompositingQuality.HighQuality;
            g.SmoothingMode = SmoothingMode.HighQuality;
            g.InterpolationMode = InterpolationMode.HighQualityBicubic;
            g.DrawImage(imgSource, new Rectangle((destWidth - sW) / 2, (destHeight - sH) / 2, sW, sH), 0, 0, imgSource.Width, imgSource.Height, GraphicsUnit.Pixel);
            g.Dispose();
            // 以下代码为保存图片时，设置压缩质量
            EncoderParameters encoderParams = new EncoderParameters();
            long[] quality = new long[1];
            quality[0] = 100;
            EncoderParameter encoderParam = new EncoderParameter(System.Drawing.Imaging.Encoder.Quality, quality);
            encoderParams.Param[0] = encoderParam;
            imgSource.Dispose();
            return outBmp;
        }

        private void button12_Click(object sender, EventArgs e) //当前页面
        {
            if (string.IsNullOrEmpty(textBox4.Text))
            {
                MessageBox.Show("页码不能为空！");
            }

            try
            {
                currentPage = Int32.Parse(textBox4.Text);
            }
            catch (FormatException exception)
            {
                MessageBox.Show("页码应该为数字！");
                throw;
            }
            string file = textBox3.Text;
            int len = file.Split('\\').Length;
            string filename = file.Split('\\')[len - 1].Split('.')[0];
            string path = "D:/课件/" + filename + "-" + currentPage + ".png";
            try
            {
                pictureBox1.Image = GetThumbnail(new Bitmap(path),
                    pictureBox1.Height, pictureBox1.Width);
            }
            catch (FileNotFoundException)
            {
                Console.Write("找不到文件！");
            }
        }

        private void sendImgToServer()
        {
            //this.MaximizeBox = false;

            var classCode = "5327";
            var userName = "大山";
            int len = imgPath.Split('/').Length;
            var fileName = imgPath.Split('/')[len - 1];
            socketClient = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            var response = MysqlHelper.SearchServerInfo();
            if (response.State == BaseResponseState.Success)
            {
                var infoList = response.Object as List<ServerIpEndPoint>;
                /*publicServerIp = infoList[0].ServerPublicIp;*/
                IPAddress ip = IPAddress.Parse("101.34.101.58 ");
                ipEndPoint = new IPEndPoint(ip, 9999);
                socketClient.Connect(ipEndPoint);
                JsonProtocol jsonProtocol = new JsonProtocol
                {
                    ConnectionDetail = JsonProtocol.ConnectionDetailEnum.Teacher2Server,
                    AimIp = "192.168.51.198",
                    LocalIp = GetLocalIp(),
                    UserType = JsonProtocol.UserTypeEnum.Teacher,
                    UserName = userName,
                    UserCode = "123",
                    CLassCode = classCode,
                    BoradWidth = _deviceWidth,
                    BoradHeight = _deviceHeight,
                    JsonType = JsonProtocol.JsonTypeEnum.TeacherSendImg,
                    JsonData = fileName
                };
                var vv = JsonConvert.SerializeObject(jsonProtocol);
                var byteLength = socketClient.Send(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(jsonProtocol) + "End"));
            }
        }

        private byte[] getBytesFromImg(Image img, System.Drawing.Imaging.ImageFormat imgFormat)

        {
            Bitmap bmp = new Bitmap(img);
            MemoryStream memStream = new MemoryStream();
            bmp.Save(memStream, imgFormat);
            memStream.Seek(0, SeekOrigin.Begin); //及时定位流的开始位置
            byte[] btImage = new byte[memStream.Length];
            memStream.Read(btImage, 0, btImage.Length);
            memStream.Close();
            return btImage;
        }

        //图片上传到七牛云
        private void uploadImg(string fileName)
        {
            string AccessKey = "t7OQeDB_Iu3ZihWosC5rr3t_qsiJznXlTx0eMOLb";
            string SecretKey = "scS_2dyn_95suAtlPNvAWDfkYrz3Uph0bYqYfpvY";
            // 上传文件到七牛云储存

            Mac mac = new Mac(AccessKey, SecretKey);
            // 上传文件名
            string key = fileName;
            // 本地文件路径
            string filePath = "D:/课件/" + fileName;
            // 存储空间名
            string Bucket = "my1cloud";

            // 设置上传策略
            PutPolicy putPolicy = new PutPolicy();
            // 设置要上传的目标空间
            putPolicy.Scope = Bucket;
            // 上传策略的过期时间(单位:秒)
            putPolicy.SetExpires(3600);
            // 文件上传完毕后，在多少天后自动被删除
            putPolicy.DeleteAfterDays = 1;
            // 生成上传token
            string token = Auth.CreateUploadToken(mac, putPolicy.ToJsonString());

            Config config = new Config();
            // 设置上传区域
            config.Zone = Zone.ZONE_CN_South;
            // 设置 http 或者 https 上传
            config.UseHttps = false;
            config.UseCdnDomains = true;
            config.ChunkSize = ChunkUnit.U512K;
            // 表单上传
            FormUploader target = new FormUploader(config);
            HttpResult result = target.UploadFile(filePath, key, token, null);
            Console.WriteLine("form upload result: " + result.ToString());
        }



        private void TeacherClient_Load(object sender, EventArgs e)
        {
            Rectangle ScreenArea = System.Windows.Forms.Screen.GetBounds(this);
            this.Width = Convert.ToInt32(ScreenArea.Width * 0.9);
            this.Height = Convert.ToInt32(ScreenArea.Height * 0.9);
            this.Location = new System.Drawing.Point(Convert.ToInt32(ScreenArea.Width * 0.05), Convert.ToInt32(ScreenArea.Height * 0.05));
            savePicture(1, true);
            savePicture(2, true);
            savePicture(3, true);
        }


        private void groupBox4_Enter(object sender, EventArgs e)
        {

        }

        private void button6_Click_1(object sender, EventArgs e)
        {

            this.pictureBox1.Image = null;
            pictureBox1.ImageLocation = @"D:\test" + 1 + ".png";
            File.Copy(@"D:\test1.png", @"D:\test2.png", true);


            JsonProtocol jsonProtocol = new JsonProtocol
            {
                ConnectionDetail = JsonProtocol.ConnectionDetailEnum.Teacher2Server,
                AimIp = publicServerIp,
                LocalIp = GetLocalIp(),
                UserType = JsonProtocol.UserTypeEnum.Student,
                UserName = textBox2.Text,
                CLassCode = textBox1.Text,
                BoradWidth = _deviceWidth,
                BoradHeight = _deviceHeight,
                JsonType = JsonProtocol.JsonTypeEnum.TeacherSendRevocation,
                JsonData = "123",
            };
            var count = JsonConvert.SerializeObject(jsonProtocol);
            socketClient.Send(Encoding.UTF8.GetBytes(count + "End"));
        }
        private void savePicture(int i,bool iswhite)
        {
            //获得当前屏幕的大小
            Rectangle rect = new Rectangle();
            this.Invoke(new MethodInvoker(delegate
            {
                rect = Screen.GetWorkingArea(this);
        }));
            //创建一个以当前屏幕为模板的图象
            Graphics g1 = this.CreateGraphics();
            //创建以屏幕大小为标准的位图 
            Image myImage = new Bitmap(pictureBox1.Width, pictureBox1.Height, g1);
            Graphics g2 = Graphics.FromImage(myImage);
            
            //得到屏幕的DC
            IntPtr dc1 = g1.GetHdc();
            //得到Bitmap的DC 
            IntPtr dc2 = g2.GetHdc();
            //调用此API函数，实现屏幕捕获
            BitBlt(dc2, 0, 0, rect.Width, rect.Height, dc1, pictureBox1.Location.X, pictureBox1.Location.Y, 13369376);
            //释放掉屏幕的DC
            g1.ReleaseHdc(dc1);
            //释放掉Bitmap的DC 
            g2.ReleaseHdc(dc2);
            //以JPG文件格式来保存
            if (iswhite) {
                g2.Clear(Color.White);
            }
            myImage.Save(@"D:\test" + i + ".png", ImageFormat.Png);
        }


        [DllImportAttribute("gdi32.dll")]

        private static extern bool BitBlt(
            IntPtr hdcDest, // 目标 DC的句柄
            int nXDest,
            int nYDest,
            int nWidth,
            int nHeight,
            IntPtr hdcSrc,  // 源DC的句柄
            int nXSrc,
            int nYSrc,
            System.Int32 dwRop  // 光栅的处理数值
            );
    }
}