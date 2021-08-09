<template>
	<el-container style="height: 100%;width: 100%;">
		
		<!-- 头部 -->
		<el-header height="7%">
			<div><span>金奥绘画教学平台</span></div>
			<el-button type="infor" @click="dialogFormVisible = true">登录</el-button>
			<el-button type="infor" @click = "test">滚动条</el-button>
			<el-button type="infor" @click = "test2">撤销</el-button>
		</el-header>

		<!-- 除头部区域 -->
		<el-container style="height: 93%;">
			<!-- 左边绘画区 -->
			<div style="position: absolute;left:40%">
			   <el-button @click="show3 = !show3" icon = "iconfont icon-xiala" v-if="!show3" ></el-button>
				<el-button @click="show3 = !show3" icon = "iconfont icon-shang1" v-if="show3"></el-button>
			   <div v-show="show3" style="margin-top: 20px; height: 50px;position: absolute;z-index: 100;transform: translateX(-44%);">
			     <el-collapse-transition>
			       <div v-show="show3">
			        <div class="transition-box" style="height: 50px;width: 700px;background-color: #bab4ab;">
						<el-button type="primary" icon="iconfont icon-jushou" style="background-color: #409eff;transform: translateY(-20px);" @click = "handsup" ></el-button>
						<el-button @click = "test2" type="primary" icon="iconfont icon-chexiao" style="background-color: #409eff;transform: translateY(-20px);margin-left: 30px;"></el-button>
						<el-button type="primary" icon="iconfont icon-qingkonghuancun" style="background-color: #409eff;transform: translateY(-20px);margin-left: 30px;"></el-button>
						<el-button type="primary" icon="iconfont icon-huabi1" style="background-color: #409eff;transform: translateY(-20px);margin-left: 30px;"  ></el-button>
						<el-button  type="primary" icon="iconfont icon-huabigongju-tuya" style="background-color: #409eff;transform: translateY(-20px);margin-left: 30px;"></el-button>
					</div>
			       </div>
			     </el-collapse-transition>
			   </div>
			 </div>
			<el-aside width="80%" style = "background-image: linear-gradient(#bbb5ac, #847c74);">
				
				<div style="width: 95%;height: 90%;margin:auto;text-align: center; transform: translateY(5%);" id="canv">
					<canvas id="mycanvas" width="400" height="400" style="background-color: white;"></canvas>
				</div>

				<!-- <el-button  type="warning" icon="el-icon-star-off" circle size="mini" @click = "candraw"  id="unmuteButton"></el-button> -->
			</el-aside>

			<!-- 右侧区域 -->
			<el-container>
				<!-- 右上角视频区域 -->
				<el-main style="height: 20%">
					<div style="height: 100%;width: 100%;text-align: center;padding: 0;"><video id="myvideo" style="width: 200px;height: 95%;"></video></div>
				</el-main>

				<!-- 右侧聊天区 -->
				<el-footer height="70%" style=" background-image: linear-gradient(#e66465, #9198e5); padding: 5%;" class="messagefooter">
					<div class="messagediv" style="background-color: white;border-radius: 2%;">
						<el-card class="box-card" style="background-color: white;" >
							 
							 <div slot="header" class="clearfix">
							    <el-menu :default-active="activeIndex" class="el-menu-demo" mode="horizontal" @select="handleSelect">
							      <el-menu-item index="1" @click = "showmessage = true">消息</el-menu-item>
							      <el-menu-item index="2" @click = "showmessage = false">学生列表</el-menu-item>
							    </el-menu>
							  </div>
							<!-- <div class="text item" v-for="message in ChatArray" :key="message">
								{{ message.TeacherName }}:{{ message.TeacherSay }}
								<el-divider></el-divider>
							</div> -->
								<vue-scroll :ops="ops" class="text item" style = ""  id="chatContent" ref="vs"> 
								<div class="text item" v-for=" i in 15" :key="i" v-if="showmessage" style="width: 95%;">
									第{{ i }}条消息
									<el-divider></el-divider>
								</div>
								<div class="text item" v-for=" j in 15" :key="j" v-if="!showmessage" style="width: 95%;">
									第{{ j }}个学生
									<el-divider></el-divider>
								</div>
								</vue-scroll>
							
							
						</el-card>
					</div>
				</el-footer>

				<!-- 右侧发消息区 -->
				<el-footer height = "auto" style="background-image: linear-gradient(45deg,#126bae 25%,#2177b8 0,#2177b8 50%,#126bae 0,#126bae 75%,#2177b8 0);
background-size: 80px 80px;padding: 5%;">
					<div style="height: 100%;">
						<el-input type="text" size="medium" clearable v-model="textarea" style="  display=inline-block;  width: 70%;" @keyup.enter.native="sendMessage"></el-input>
						<el-button
							type="primary"
							icon="el-icon-edit"
							circle
							style="  display=inline-block;  
							margin-left: 15px;"
							@click="sendMessage"
						></el-button>
					</div>
				</el-footer>
			</el-container>
		</el-container>

		<!-- 登录DiaLog -->
		<el-dialog title="学生登陆" :visible.sync="dialogFormVisible">
			<div style="text-align: center;">
				<el-input style="width: 85%;margin-top: 10%;" prefix-icon="iconfont icon-yonghu" v-model="form.UserCode" placeholder="请输入学号"></el-input>
				<el-input
					style="width: 85%;margin-top: 5%;"
					prefix-icon="iconfont icon-banjiguanli"
					v-model="form.CLassCode"
					placeholder="请输入班级号"
					@keyup.enter.native="login"
				></el-input>
				<el-button style="width: 85%;margin-top: 15%;height: 50px;" type="primary" @click="login()">登录</el-button>
			</div>
		</el-dialog>
	</el-container>
</template>

<script>
import $ from '../../node_modules/jquery/dist/jquery.js';
import store from '../store.js';
import flv from 'flv.js';
export default {
	store,
	created() {
		// if(flv.isSupported()){
		// 	this.player = flv.createPlayer({
		// 	type: 'flv',
		// 	url: 'http://101.34.101.58/live?port=9988&&app=live&&stream=test',
		// 	isLive: true,
		// 	});
		// }
	},
	data() {
		
		return {
			ops: {
			                    vuescroll: {
									detectResize: true,
								},
			                    scrollPanel: {},
			                    rail: {
			                        keepShow:true
			                    },
			                    bar: {
			                        hoverStyle: true,
			                        onlyShowBarOnScroll: false, //是否只有滚动的时候才显示滚动条
			                        background: "#2282ff",
			                    }
			                },
			show3: false,
			showmessage:true,
			activeIndex: '1',
			dialogFormVisible: false,
			player: null,
			playing: false,
			screenWidth: 0, //屏幕宽度
			screenHeight: 0, //屏幕高度
			websock: null,
			drawx: 100,
			drawy: 500,
			//页面启动自动接连
			form: {
				ConnectionDetail: 4,
				LocalIp: '192.168.51.198',
				AimIp: '101.34.101.58',
				UserType: 0,
				// "UserName": store.state.userName,
				// "UserCode": store.state.userCode,
				// "CLassCode": store.state.classCode,
				UserName: '',
				UserCode: '17003',
				CLassCode: '5327',
				JsonType: 4,
				JsonData: '登录到服务器',
				ContentSocket: null
			},
			countnum: 0,
			//坐标点对象
			pointObj: null,
			textarea: '',
			iscandraw: false,
			flag: false,
			x: 0,
			y: 0,
			pointArray: [],
			testjson: {
				ConnectionDetail: 4,
				LocalIp: '192.168.51.198',
				AimIp: '192.168.51.198',
				UserType: 0,
				UserName: 123,
				UserCode: 123,
				CLassCode: 5327,
				JsonType: 6,
				JsonData: '',
				ContentSocket: null
			},
			ChatArray: [],
			bgimg: 'http://qwf43oqfq.hn-bkt.clouddn.com/%E6%A1%8C%E9%9D%A2%E5%A3%81%E7%BA%B8.jpg',
			imgObj: null,
			imgSrcRoot: 'http://qwf43oqfq.hn-bkt.clouddn.com/',
			imgData: null,
			temp: [],
			tempcount:0,
			pdfheight: 0,
			pdfwidth: 0,
			upPicture: null,
			nowPicture: null,
			imageSrc:""
		};
	},

	methods: {
		test(){
			
			// var Data = "123456789"
			// var Data1
			// var Data2
			// var Data3
			// Data1 = Data.slice(0,2)
			// Data2 = Data.slice(2,4)
			// Data3 = Data.slice(4,Data.length)
			// console.log("这是第一段" + Data1)
			// setTimeout(function () {
			//     // 这里就是处理的事件
			// 	console.log("这是第二段" + Data2)
			// }, 500);
			// setTimeout(function () {
			//     // 这里就是处理的事件
			// 	console.log("这是第三段" + Data3)
			// }, 1000);
			// $("#chatContent").scrollTop(0);
			
			const that = this
			console.log(123)
			const { v, h } = this.$refs["vs"].getScrollProcess();
			console.log(v, h);
			if(v != 1){
				that.$refs["vs"].scrollTo(
				  {
				    y:"100%"
				  },
				  500
				);
			}
			
			// $("#chatContent").scrollTop($("#chatContent")[0].scrollHeight);
		
			
		},
		test2(){
			var context = document.getElementById('mycanvas').getContext('2d');
			context.canvas.width = context.canvas.width;
			context.canvas.height = context.canvas.height;
			
			this.canvaspaste()
		},
		 handleSelect(key, keyPath) {
		        console.log(key, keyPath);
		      },
		login() {
			this.dialogFormVisible = false;
			this.initWebSocket();
		},

		clearcanvas() {
			var context = document.getElementById('mycanvas').getContext('2d');
			context.canvas.width = context.canvas.width;
		},

		canvascopy() {
			var context = document.getElementById('mycanvas').getContext('2d');
			var width = context.canvas.width;
			var height = context.canvas.height;
			
			this.imgData = context.getImageData(0, 0, width, height);
			for(var i = 0; i < this.imgData.data.length; i += 4) {
			// 当该像素是透明的,则设置成白色
			if(this.imgData.data[i + 3] == 0) {
			this.imgData.data[i] = 255;
			this.imgData.data[i + 1] = 255;
			this.imgData.data[i + 2] = 255;
			this.imgData.data[i + 3] = 255;
			}
			}
			var newCanvas = $('<canvas>')
				.attr('width', this.imgData.width)
				.attr('height', this.imgData.height)[0];
				
			newCanvas.getContext('2d').putImageData(this.imgData, 0, 0);
			this.temp[this.tempcount] = newCanvas;
			console.log("复制给了" + this.tempcount)
			this.tempcount++
		},

		canvaspaste() {
			this.tempcount--
			var context = document.getElementById('mycanvas').getContext('2d');
			var width = context.canvas.width;
			var height = context.canvas.height;
			context.scale(width / this.temp[this.tempcount - 1].width, height / this.temp[this.tempcount - 1].height);
			context.drawImage(this.temp[this.tempcount - 1], 0, 0);
			console.log("粘贴了" + this.tempcount)
			this.imgData = context.getImageData(0, 0, width, height);
			var newCanvas = $('<canvas>')
				.attr('width', this.imgData.width)
				.attr('height', this.imgData.height)[0];
			newCanvas.getContext('2d').putImageData(this.imgData, 0, 0);

			// context = document.getElementById("mycanvas").getContext('2d');
			context.canvas.width = width;
			context.canvas.height = height;
			context.drawImage(newCanvas, 0, 0);
			
			console.log(width + ' + ' + height);
		},

		revocation() {
			// this.canvaspaste();
			var context = document.getElementById('mycanvas').getContext('2d');
			context.canvas.width = context.canvas.width;
			this.canvaspaste();
		},

		loadImg() {
			const that = this;
			var beauty = new Image();
			beauty.crossOrigin = 'Anonymous'; //这是是需要加的
			beauty.src = this.imgSrcRoot + this.imgObj;
			console.log(444 + beauty.src);
			beauty.onload = function() {
				// 等比例缩放图片
				var scale = 1;
				var mycv = document.getElementById('mycanvas');
				var myctx = mycv.getContext('2d');
				this.width = mycv.width / 2;
				this.height = mycv.height;
				console.log(this.width + '+' + this.height);
				myctx.drawImage(this, 0, 0, this.width, this.height); // 加载图片
				//myctx.drawImage(this, 0, 0)
				that.canvascopy();
			};
		},

		initCanvas() {
			console.log('initcanvas');
			var canvas = document.getElementById('mycanvas');
			var context = canvas.getContext('2d');
			canvas.width = window.innerWidth * 0.75;
			canvas.height = window.innerHeight * 0.85;
			canvas.style.width = window.innerWidth * 0.75;
			canvas.style.height = window.innerHeight * 0.85;
		},

		drawline() {
			const that = this;
			console.log('可以画了');
			console.log(window.innerWidth);
			var canvas = document.getElementById('mycanvas');
			var context = canvas.getContext('2d');
			// canvas.width = window.innerWidth * 0.75;
			// canvas.height =  window.innerHeight * 0.85;
			// canvas.style.width = window.innerWidth * 0.75;
			// canvas.style.height =  window.innerHeight * 0.85;
			var canvasWidth = canvas.width;
			var canvasHeight = canvas.height;
			canvas.onmousedown = function(e) {
				that.pointArray = [];
				var point = {};
				var count = [];
				var mid = [];
				var e = e || window.event;
				var ox = e.clientX - window.innerWidth * 0.025;
				var oy = e.clientY - window.innerHeight * 0.11;
				context.moveTo(ox, oy);

				document.onmousemove = function(e) {
					// setTimeout("showLogin()",1000);
					var e = e || window.event;
					var ox2 = e.clientX - window.innerWidth * 0.025;
					var oy2 = e.clientY - window.innerHeight * 0.11;

					point.X = parseFloat(that.BoradWidth - (that.BoradWidth * (e.clientX - window.innerWidth * 0.025)) / canvasWidth).toFixed(4);
					point.Y = parseFloat(that.BoradHeight - (that.BoradHeight * (e.clientY - window.innerHeight * 0.11)) / canvasHeight).toFixed(4);
					
					context.lineTo(ox2, oy2);
					context.stroke();
					count.push(point);
					point = {};
					if(count.length % 12 == 0 ){
						for (var i = 0; i < count.length; i++) {
							mid.push(count[i]);
							mid.push(count[i + 2]);
							that.pointArray.push(mid);
							mid = [];
						}
						
					}
				};

				document.onmouseup = function(e) {
					document.onmousemove = null;
					document.onmouseup = null;
					
					for (var i = 0; i < count.length; i++) {
						mid.push(count[i]);
						mid.push(count[i + 1]);
						that.pointArray.push(mid);
						mid = [];
					}
					that.form.JsonType = 7;
					var msg = JSON.stringify(that.pointArray);
					that.form.JsonData = msg;
					that.countnum++;
					 that.canvascopy();
				};
			};

			console.log(1111111111111111111111111111111111111111111111);
			console.log(canvasWidth);
			console.log(canvasHeight);
			for (var i = 0; i < this.pointObj.length; i++) {
				context.moveTo(canvasWidth - (this.pointObj[i][0].X / this.BoradWidth) * canvasWidth, canvasHeight - (this.pointObj[i][0].Y / this.BoradHeight) * canvasHeight);
				context.lineTo(canvasWidth - (this.pointObj[i][1].X / this.BoradWidth) * canvasWidth, canvasHeight - (this.pointObj[i][1].Y / this.BoradHeight) * canvasHeight);
				context.strokeStyle = 'black';
				context.lineWidth = 1.5;
				context.stroke();
			}
		},

		sendMessage() {
			const that = this;
			if (this.textarea === null || this.textarea === '' || this.textarea === ' ') {
				alert('别nm发空消息');
			} else {
				that.form.JsonType = 6;
				that.form.JsonData = that.textarea;
				that.textarea = null;
				var msg = JSON.stringify(that.form);
				that.websocketsend(msg);
			}
		},

		logout() {
			this.$router.push('/Login');
			this.websock.close(); //离开路由之后断开websocket连接
		},

		initWebSocket() {
			//初始化weosocket
			//192.168.51.198
			//101.34.101.58
			// const wsuri = "ws://192.168.51.132:9999";
			// const wsuri = "ws://192.168.51.198:9999";
			const wsuri = 'ws://101.34.101.58:9999';
			// const wsuri = 'ws://127.0.0.1:9999';
			// const wsuri = 'ws://192.168.51.170:9999';
			
			this.websock = new WebSocket(wsuri);
			this.websock.onopen = this.websocketonopen;
			this.websock.onmessage = this.websocketonmessage;
			this.websock.onerror = this.websocketonerror;
			this.websock.onclose = this.websocketclose;
		},

		websocketonopen() {
			//连接建立之后执行send方法发送数据
			var msg = JSON.stringify(this.form); //转化为json字符串
			console.log(this.form);
			console.log(msg);
			this.websocketsend(msg);
		},

		websocketonerror() {
			//连接建立失败重连
			alert('正在重新连接');
		},

		websocketonmessage(e) {
			//数据接收
			var jsonObj = eval('(' + e.data + ')');
			console.log(jsonObj);
			console.log(jsonObj.JsonType);
			if (jsonObj.JsonData == 'false') {
				alert('登陆失败');
			} else {
				if (jsonObj.JsonData == 'true') {
					alert('登陆成功');
					this.BoradWidth = jsonObj.BoradWidth;
					this.BoradHeight = jsonObj.BoradHeight;
				} else {
					if (jsonObj.JsonType == 3) {
						console.log('这是画线！！');
						this.canvascopy();
						var pointObj = eval('(' + jsonObj.JsonData + ')');
						this.pointObj = pointObj;
						this.BoradWidth = jsonObj.BoradWidth;
						this.BoradHeight = jsonObj.BoradHeight;
						console.log('这是手写版的宽高：' + jsonObj.BoradWidth + jsonObj.BoradHeight);
					} else if (jsonObj.JsonType == 8) {
						console.log('这是pdf！！');
						// console.log("这是清空画板")
						//  var c=document.getElementById("mycanvas");
						//     var cxt=c.getContext("2d");
						//     c.height=c.height;
						var imgObj = jsonObj.JsonData;
						this.imgObj = imgObj;
						console.log(2333 + this.imgObj);
					} else if (jsonObj.JsonType == 2) {
						console.log('这是消息！！');
						var count = {};
						(count.TeacherName = jsonObj.UserName), (count.TeacherSay = jsonObj.JsonData), console.log(count);
						this.ChatArray.push(count);
					} else if (jsonObj.JsonType == 10) {
						console.log('这是撤销！！');

						this.revocation();
					} else if (jsonObj.JsonType == 9) {
						console.log('这是清空花板！！');
						var c = document.getElementById('mycanvas');
						c.height = c.height;
					}
				}
			}
			// if(jsonObj.ConnectionDetail = 8){
			// 	if(jsonObj.JsonData == "false"){
			// 	this.$message.error('登陆失败');
			// 	}else{
			// 		if(jsonObj.JsonType == 4){
			// 			const that = this
			// 		 this.$message({
			// 				  message: '登陆成功',
			// 				  type: 'success'
			// 				});
			// 	this.openLogin = false;
			// 		}

			// 	else if(jsonObj.JsonType == 8){
			// 			var imgObj = jsonObj.JsonData;
			// 			this.imgObj = imgObj
			// 			console.log(2333+this.imgObj)
			// 		}else if(jsonObj.JsonType == 3){
			// 			this.canvascopy();
			// 			var pointObj = eval('(' + jsonObj.JsonData + ')');
			// 			this.pointObj = pointObj
			// 			this.BoradWidth = jsonObj.BoradWidth
			// 			this.BoradHeight = jsonObj.BoradHeight
			// 			console.log("这是手写版的宽高：" + jsonObj.BoradWidth + jsonObj.BoradHeight)
			// 		}else{
			// 			alert("这是其他的")
			// 		}
			// 	// this.loginbtn = false
			// 	}
			// }
		},
		//数据发送
		websocketsend(Data) {
			// var that= this
			// //每个包发1000的数据量
			// var count = Data.length / 1000;
			// console.log("发了" + Math.ceil(count) + "次")
			// for(var i = 0; i < Math.ceil(count); i++){
			// 	// console.log(Data.slice(1000 * i,1000 * (i + 1)))
			// 	var Data1 = Data.slice(1000 * i,1000 * (i + 1))
				
			// 	setTimeout(function () {
			// 	    // 这里就是处理的事件
			// 		that.websock.send(Data1)
			// 	}, 100 * i);
			// }
			if(Data.length > 325000){
				console.log("太多了")
			}
			else if(Data.length > 260000){
				const that = this
				var Data1;
				var Data2;
				var Data3;
				var Data4;
				var Data5;
				console.log('大于260000' + Data.length);
				Data1 = Data.slice(0,65000)
				Data2 = Data.slice(65000,130000)
				Data3 = Data.slice(130000,195000)
				Data4 = Data.slice(195000,260000)
				Data5 = Data.slice(260000,Data.length)
			
				that.websock.send(Data1 + "Wait")
				setTimeout(function () {
				    // 这里就是处理的事件
					that.websock.send(Data2 + "Wait")
				}, 1000);
				setTimeout(function () {
				    // 这里就是处理的事件
					that.websock.send(Data3 + "Wait")
				}, 2000);setTimeout(function () {
				    // 这里就是处理的事件
					that.websock.send(Data4 + "Wait")
				}, 3000);
				setTimeout(function () {
				    // 这里就是处理的事件
					that.websock.send(Data5)
				}, 4000);
			}
			else if(Data.length > 195000 && Data.length < 260000){
				const that = this
				var Data1;
				var Data2;
				var Data3;
				var Data4;
				console.log('大于195000' + Data.length);
				Data1 = Data.slice(0,65000)
				Data2 = Data.slice(65000,130000)
				Data3 = Data.slice(130000,195000)
				Data4 = Data.slice(195000,Data.length)
				that.websock.send(Data1 + "Wait")
				
				setTimeout(function () {
				    // 这里就是处理的事件
					that.websock.send(Data2 + "Wait")
				}, 1000);
				setTimeout(function () {
				    // 这里就是处理的事件
					that.websock.send(Data3 + "Wait")
				}, 2000);
				setTimeout(function () {
				    // 这里就是处理的事件
					that.websock.send(Data4)
				}, 3000);
			}
			else if(Data.length > 130000 && Data.length < 195000){
				var Data1;
				var Data2;
				var Data3;
				console.log('大于130000' + Data.length);
				const that = this
				Data1 = Data.slice(0,65000)
				Data2 = Data.slice(65000,130000)
				Data3 = Data.slice(130000,Data.length)
				console.log('第一次发' + Data1.length);
				console.log('第二次发' + Data2.length);
				console.log('第三次发' + Data3.length);
				this.websock.send(Data1 + "Wait");
				setTimeout(function () {
				    // 这里就是处理的事件
					that.websock.send(Data2 + "Wait")
				}, 1000);
				setTimeout(function () {
				    // 这里就是处理的事件
					that.websock.send(Data3)
				}, 2000);
				
			}
			else if(Data.length > 65000 && Data.length < 130000){
				const that = this
				var Data1;
				var Data2;
				console.log('大于65000' + Data.length);
				Data1 = Data.slice(0,65000)
				Data2 = Data.slice(65000,Data.length)
				this.websock.send(Data1 + "Wait");
				setTimeout(function () {
				    // 这里就是处理的事件
					that.websock.send(Data2);
				}, 1000);
				
			}
			else{
				console.log('直接发' + Data.length);
				this.websock.send(Data);
			}
			
		},

		websocketclose(e) {
			//关闭
			console.log('断开连接', e);
		}
	},

	mounted() {
		// if (flv.isSupported()) {
		// 	  var videoElement = document.getElementById('myvideo')
		// 	  var flvPlayer = flv.createPlayer({
		// 	   type: 'flv',
		// 	   url: 'http://101.34.101.58/live?port=9988&&app=live&&stream=test',
		// 	     isLive: true,
		// 		 fluid:true,
		// 		// 首帧等待时长
		// 		// stashInitialSize: 2
		// 	    // hasAudio: false,//是否有音频轨道
		// 	  },
		// 	  {
		// 		   // lazyLoadMaxDuration: 3 * 60,
		// 		   // enableWorker: true,
		// 		// 启用IO隐藏缓冲区。如果您需要实时（最小延迟）来进行实时流播放，则设置为false，但是如果网络抖动，则可能会停顿。
		//         enableStashBuffer: false,//是否开启播放器端缓存
		// 		fixAudioTimestampGap:false,//音视频同步
		// 		isLive:true,//是否为直播流
		// 		// autoCleanupMaxBackwardDuration:1,
		// 		// withCredentials: false,
		// 		 // cors: true,
		//                      },
		// 						)
		// 	  flvPlayer.attachMediaElement(videoElement)
		// 	  flvPlayer.load()
		// 	  flvPlayer.play()
		// }

		var _this = this;
		window.onresize = function() {
			// 定义窗口大小变更通知事件
			_this.screenWidth = document.documentElement.clientWidth; //窗口宽度
			_this.screenHeight = document.documentElement.clientHeight; //窗口高度
			console.log(document.documentElement.clientWidth);
			console.log(document.documentElement.clientWidth);
		};

		this.initCanvas();
		// this.initWebSocket()
		this.drawline();
	},

	computed: {
		getuserCode() {
			return this.$store.state.userCode;
		},
		getclassCode() {
			return this.$store.state.classCode;
		}
	},

	watch: {
		ChatArray: {
			deep: true,
			handler: function(newVal, oldVal) {
				$("#chatContent").scrollTop($("#chatContent")[0].scrollHeight);
			}
		},
		pointObj: {
			deep: true,
			handler: function(newVal, oldVal) {
				this.drawline();
			}
		},

		countnum: {
			deep: true,
			handler: function(newVal, oldVal) {
				
				var msg = JSON.stringify(this.form);
				this.websocketsend(msg);
				
				this.drawline();
			}
		},
		imgObj: {
			deep: true,
			handler: function(newVal, oldVal) {
				this.loadImg();
			}
		},
		screenWidth: function(val) {
			//监听屏幕宽度变化
			console.log(123456);
			console.log(this.imgData);
			var canvas = document.getElementById('mycanvas');
			var context = canvas.getContext('2d');
			canvas.width = window.innerWidth * 0.75;
			canvas.height = window.innerHeight * 0.85;
			canvas.style.width = window.innerWidth * 0.75;
			canvas.style.height = window.innerHeight * 0.85;
			console.log(window.innerWidth);

			this.canvaspaste();
		},
		screenHeight: function() {
			//监听屏幕高度变化
			console.log(123456);
			var canvas = document.getElementById('mycanvas');
			var context = canvas.getContext('2d');
			canvas.width = window.innerWidth * 0.75;
			canvas.height = window.innerHeight * 0.85;
			canvas.style.width = window.innerWidth * 0.75;
			canvas.style.height = window.innerHeight * 0.85;
			this.canvaspaste();
		}
	}
};
</script>

<style scoped lang="less">
.el-header {
	background-color: #373d41;
	display: flex;
	justify-content: space-between;
	padding-left: 20px;
	align-items: center;
	color: #ffffff;
	font-size: 20px;
	> div {
		display: flex;
		align-items: center;
		> span {
			margin-left: 20px;
		}
	}
}

.messagediv {
	width: 100%;
	height: 100%;
	.el-card /deep/ .el-card__body {
		
		height: 90%;
		
	}
.el-card /deep/ .el-card__header {
		    padding: 0px;
			text-align: center;
		}
		
	.box-card {
		width: 100%;

		background-color: white;
		text-align: left;
		overflow-y: hidden;
		overflow-x: hidden;

		height: 100%;
		
		
		
		.el-menu--horizontal > .el-menu-item{
			height: 40px;
			line-height: 40px;
		}
		
		.text {
			font-size: 15px;
			width: 100%;
		}
		.item {
			line-height: 20px;
		}
		.el-divider {
			margin: 10px 0;
			background: 0 0;
			border-top: 1px solid #c5c5d5;
		}
	}
}

.el-footer {
	background-color: #b3c0d1;
	// background-color: #f9f4dc;
	color: #333;
	text-align: center;
	line-height: 100%;
}

.el-aside {
	background-color: rgb(#ced6e0);
	text-align: center;
}

.el-main {
	background-color: black;
	color: #333;
	text-align: center;
	padding: 0;
}

.transition-box {
	    margin-bottom: 10px;
	    border-radius: 4px;
	    background-color: #409EFF;
	    text-align: center;
	    color: #fff;
	    padding: 40px 20px;
	    box-sizing: border-box;
	    margin-right: 20px;
	  }
</style>
