using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Models;
using Models.Server;
using Newtonsoft.Json;
using RobotpenGateway;
using Utils.StaticUtils;
using Utils.VariableUtils;

namespace 手写板工具学生端
{
    public partial class StudentServer : Form
    {
        private static Socket socketClient;
        private static IPEndPoint ipEndPoint;
        private string publicServerIp = "";
        private byte[] buffer = new byte[1024 * 1024];

        public StudentServer()
        {
            InitializeComponent();
            button3.Enabled = true;
            button4.Enabled = true;
            InitPen();
            Wait2Connect();
        }

        #region 罗博笔相关

        private RobotpenGateway.robotpenController.returnPointData _pointData = null;
        private int _deviceConnectStatus = -1;
        private WritingDeviceInfo deviveInfo = new WritingDeviceInfo();
        private eDeviceType deviceType;
        private eDeviceMode eDMode;
        private PointInfo _prePoint = new PointInfo(-1, -1);

        private List<PointInfo[]> pointList = new List<PointInfo[]>();
        private List<PointInfo[]> tempList = new List<PointInfo[]>();

        private float widthHeightRate = 0.0000f;

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
            if (button3.Enabled == true)
            {
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
                    ConnectionDetail = JsonProtocol.ConnectionDetailEnum.Student2Server,
                    AimIp = publicServerIp,
                    LocalIp = GetLocalIp(),
                    UserType = JsonProtocol.UserTypeEnum.Student,
                    UserName = textBox2.Text,
                    UserCode = textBox2.Text,
                    CLassCode = textBox1.Text,
                    JsonType = JsonProtocol.JsonTypeEnum.StudentSendNormalMessage,
                    JsonData = result
                };

                SendMessage2Server(JsonConvert.SerializeObject(jsonProtocol));
                tempList.Clear();
            }
        }

        /// <summary>
        /// 切换为鼠标或者手写模式
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button1_Click(object sender, EventArgs e)
        {
            if (deviceType == eDeviceType.T8B_D2 || deviceType == eDeviceType.T8Y || deviceType == eDeviceType.T8S_LQ || deviceType == eDeviceType.J7M)
            {
                if (eDMode == eDeviceMode.DEVICE_HAND)
                {
                    robotpenController.GetInstance().setDeviceMode(eDeviceMode.DEVICE_MOUSE);
                    eDMode = eDeviceMode.DEVICE_MOUSE;
                    button1.Text = "切换为手写模式";
                }
                else
                {
                    robotpenController.GetInstance().setDeviceMode(eDeviceMode.DEVICE_HAND);
                    eDMode = eDeviceMode.DEVICE_HAND;
                    button1.Text = "切换为鼠标模式";
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
            PointInfo point = new PointInfo(bx / 10, by / 10);
            if (_prePoint.X == -1)
            {
                _prePoint = point;
            }

            Console.WriteLine($"x:{point.X} - y:{point.Y} - bPress:{bPress} - bIndex:{bIndex} - bPenStatus:{bPenStatus}");

            Graphics g = pictureBox1.CreateGraphics();
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;//消除锯齿
            g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
            if (bPress > 0 && !point.X.Equals(-1))
            {
                g.DrawLine(new Pen(Color.Black, 1), new PointF(pictureBox1.Width - _prePoint.X, pictureBox1.Height - _prePoint.Y), new PointF(pictureBox1.Width - point.X, pictureBox1.Height - point.Y));

                tempList.Add(new PointInfo[]
                {
                    _prePoint,
                    point
                });

                pointList.AddRange(tempList);
                _prePoint = point;
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
                                        widthHeightRate = (robotpenController.GetInstance().getWidth() * 1.0000f) / (robotpenController.GetInstance().getHeight() * 1.0000f);

                                        InitPictureBox();
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
                                    this.Text = $"手写板工具-学生端  -手写板{deviceType}连接成功";
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
            socketClient?.Send(Encoding.UTF8.GetBytes(result));
        }

        /// <summary>
        /// 初始化套接字内容
        /// </summary>
        private void InitSocketContent()
        {
            socketClient = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            var response = MysqlHelper.SearchServerInfo();
            if (response.State == BaseResponseState.Success)
            {
                var infoList = response.Object as List<ServerIpEndPoint>;
                publicServerIp = infoList[0].ServerPublicIp;
                IPAddress ip = IPAddress.Parse(infoList[0].ServerPublicIp);
                ipEndPoint = new IPEndPoint(ip, int.Parse(infoList[0].ServerPort));
                socketClient.Connect(ipEndPoint);
                JsonProtocol jsonProtocol = new JsonProtocol
                {
                    ConnectionDetail = JsonProtocol.ConnectionDetailEnum.Student2Server,
                    AimIp = infoList[0].ServerPublicIp,
                    LocalIp = GetLocalIp(),
                    UserType = JsonProtocol.UserTypeEnum.Student,
                    UserName = textBox2.Text,
                    UserCode = textBox2.Text,
                    CLassCode = textBox1.Text,
                    JsonType = JsonProtocol.JsonTypeEnum.StudentPrepared2Login,
                    JsonData = "登录到服务器"
                };

                SendMessage2Server(JsonConvert.SerializeObject(jsonProtocol));

                //socketClient.Send(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(jsonProtocol)));
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
            if (this.InvokeRequired)
            {
                this.Invoke(new MethodInvoker(delegate
                {
                    try
                    {
                        var socket = ar.AsyncState as Socket;
                        var length = socket.EndReceive(ar);
                        //var str = Encoding.UTF8.GetString(buffer, 0, length);

                        JsonProtocol protocolJson = JsonConvert.DeserializeObject<JsonProtocol>(Encoding.UTF8.GetString(buffer, 0, length));

                        OperateMessages(protocolJson);

                        socket.BeginReceive(buffer, 0, buffer.Length, SocketFlags.None, new AsyncCallback(ReceiveMessage), socket);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                    }
                }));
            }
        }

        /// <summary>
        /// 信息操作方法
        /// </summary>
        /// <param name="protocolJson"></param>
        private void OperateMessages(JsonProtocol protocolJson)
        {
            if (protocolJson.JsonType == JsonProtocol.JsonTypeEnum.TeacherSendNormalMessage)
            {
                var jsonPoint = protocolJson.JsonData;
                try
                {
                    var pointListDes = JsonConvert.DeserializeObject<List<PointInfo[]>>(jsonPoint);
                    Graphics g = pictureBox1.CreateGraphics();
                    g.SmoothingMode = SmoothingMode.AntiAlias;//消除锯齿
                    g.InterpolationMode = InterpolationMode.HighQualityBicubic;

                    foreach (var pointInfos in pointListDes)
                    {
                        if (!pointInfos[1].X.Equals(-1) || !pointInfos[1].Y.Equals(-1))
                        {
                            g.DrawLine(new Pen(Color.Black, 2), new PointF(pictureBox1.Width - pointInfos[0].X, pictureBox1.Height - pointInfos[0].Y), new PointF(pictureBox1.Width - pointInfos[1].X, pictureBox1.Height - pointInfos[1].Y));
                        }
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
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

        private void PreLoadData()
        {
            this.Text = $"手写板工具-学生端";
        }

        /// <summary>
        /// 窗体关闭的时候
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void StudentServer_FormClosing(object sender, FormClosingEventArgs e)
        {
            JsonProtocol jsonProtocol = new JsonProtocol
            {
                ConnectionDetail = JsonProtocol.ConnectionDetailEnum.Student2Server,
                AimIp = publicServerIp,
                LocalIp = GetLocalIp(),
                UserType = JsonProtocol.UserTypeEnum.Student,
                UserName = textBox2.Text,
                UserCode = textBox2.Text,
                CLassCode = textBox1.Text,
                JsonType = JsonProtocol.JsonTypeEnum.StudentPrepared2Exit,
                JsonData = $"{textBox2.Text}学生退出服务器"
            };

            socketClient?.Send(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(jsonProtocol)));
            System.Environment.Exit(0);
        }

        /// <summary>
        /// 初始化图片盒
        /// </summary>
        private void InitPictureBox()
        {
            button3.Enabled = true;
            button4.Enabled = true;

            var originPictureBoxHeight = pictureBox1.Height;
            var pictureBoxWidth = pictureBox1.Width;
            var newPictureBoxHeight = pictureBoxWidth * 1.0000f / widthHeightRate;

            if (this.InvokeRequired)
            {
                this.Invoke(new MethodInvoker(delegate
                {
                    pictureBox1.Height = Convert.ToInt32(newPictureBoxHeight);
                    var need2MoveHeight = originPictureBoxHeight - pictureBox1.Height;
                    pictureBox1.Location = new Point(pictureBox1.Location.X, pictureBox1.Location.Y + need2MoveHeight / 2);
                }));
            }
        }

        /// <summary>
        /// 连接到房间
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button3_Click(object sender, EventArgs e)
        {
            InitSocketContent();
            button3.Enabled = false;
            button3.Text = "已经连接...";
        }

        #endregion 界面相关

        /// <summary>
        /// 发送
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button2_Click(object sender, EventArgs e)
        {
            JsonProtocol jsonProtocol = new JsonProtocol
            {
                ConnectionDetail = JsonProtocol.ConnectionDetailEnum.Student2Server,
                AimIp = publicServerIp,
                LocalIp = GetLocalIp(),
                UserType = JsonProtocol.UserTypeEnum.Student,
                UserName = textBox2.Text,
                UserCode = textBox2.Text,
                CLassCode = textBox1.Text,
                JsonType = JsonProtocol.JsonTypeEnum.StudentSendNormalMessage,
                JsonData = $"{richTextBox1.Text}"
            };

            socketClient.Send(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(jsonProtocol)));
        }

        /// <summary>
        /// 退出房间
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button4_Click(object sender, EventArgs e)
        {
            button4.Enabled = false;
            button3.Enabled = true;
            button3.Text = "连接到房间";
        }
    }
}