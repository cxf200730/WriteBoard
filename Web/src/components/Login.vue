<template>
	<el-container class = "all">
		
	  <el-header height = 10%>
		  
		  <div style="margin-left: 10px;font-size: 20px;letter-spacing:2px">金奥手写板直播平台</div>
			  <div style="margin-right: 30px;">帮助</div>
	  </el-header>
	  
	  <el-main>
		  <el-card class="box-card">
		    <div slot="header" class="clearfix" style="height: 50px;line-height: 50px;">
		      <span style="font-size: 25px;font-weight: bold;" >学生登录</span>
		    </div>
		    <div style="text-align: center;">
				<el-input style="width: 85%;margin-top: 10%;" prefix-icon = "iconfont icon-yonghu" v-model="form.UserCode"  placeholder="请输入学号"></el-input>
				<el-input style="width: 85%;margin-top: 5%;" prefix-icon = "iconfont icon-banjiguanli" v-model="form.CLassCode"  placeholder="请输入班级号"></el-input>
				<!-- <el-input style="width: 85%;margin-top: 5%;" prefix-icon = "iconfont icon-mima" v-model="form.code"  placeholder="请输入课堂码"></el-input> -->
				<el-button style="width: 85%;margin-top: 15%;height: 50px;" type="primary" @click = "login()">登录</el-button>
			</div> 
		  </el-card>
	  </el-main>
	  
	  <el-footer height = 20%>
			<div style="font-size: 13px;">
				隐私政策
			<el-divider direction="vertical"></el-divider>
				儿童隐私政策
			<el-divider direction="vertical"></el-divider>
				家长须知
		  </div>
	  </el-footer>
	</el-container>
</template>

<script>
	import $ from '../../node_modules/jquery/dist/jquery.js'
	import store from '../store.js'
export default {
	store,
	created(){
	 
	},
	data(){
		return{
			form:{
				"ConnectionDetail": 4,
				"LocalIp": "192.168.51.198",
				"AimIp": "101.34.101.58",
				"UserType": 0,
				// "UserName": store.state.userName,
				// "UserCode": store.state.userName,
				// "CLassCode": store.state.classCode,
				"UserName": "",
				"UserCode": "",
				"CLassCode": "",
				"JsonType": 4,
				"JsonData": "登录到服务器",
				"ContentSocket": null
			},
		}
	},
	methods:{
		login(){
			if(this.form.UserCode == null || this.form.UserCode == "" || this.form.CLassCode == null || this.form.CLassCode == ""){
				this.$message({
				          message: '输入不能为空哦',
				          type: 'warning'
				        });
			}else{
				this.initWebSocket();
			}
		},
		initWebSocket(){ //初始化weosocket
		
		        const wsuri = "ws://192.168.51.198:9999";    
				// const wsuri = "ws://101.34.101.58:9999"; 
				this.websock = new WebSocket(wsuri);
				this.websock.onopen = this.websocketonopen;
				this.websock.onmessage = this.websocketonmessage;
				this.websock.onerror = this.websocketonerror;
				this.websock.onclose = this.websocketclose;
		},
		websocketonopen(){ //连接建立之后执行send方法发送数据
			var msg = JSON.stringify(this.form);//转化为json字符串
			console.log(this.form);
			console.log(msg); 
			this.websocketsend(msg);
		},
		websocketonerror(){//连接建立失败重连
		
		},
		websocketonmessage(e){ //数据接收
			var jsonObj = eval('(' + e.data + ')');
			console.log(jsonObj)
			console.log(jsonObj.JsonData)
			if(jsonObj.JsonData == "false"){
				this.$message.error('登陆失败');
			}else{
				const that = this
				this.$store.commit('getuserCode', this.form.UserCode)
				this.$store.commit('getclassCode', this.form.CLassCode)
				// this.$message({
				// 				  message: '登陆成功',
				// 				  type: 'success'
				// 				});
				// this.form.JsonData = "close"
				// var msg = JSON.stringify(this.form);//转化为json字符串
				// this.websock.close();
				// this.websocketsend(msg);
				that.$router.push('Watch')
				this.websock.onclose = function(e){
					console.log("关闭")
					// that.$router.push('Watch')
				}
				
				
			}
		},
		websocketsend(Data){//数据发送
		this.websock.send(Data);
		},
		websocketclose(e){  //关闭
			console.log('断开连接',e);
		},
	},
	mounted() {
		
	},
}
</script>

<!-- Add "scoped" attribute to limit CSS to this component only -->
<style lang="less" scoped>
.all{
	height: 100%;
	width: 100%;
	text-align: center;
	  .el-header, {
	     background-color: #FFFFFF;
	     color: #333;
	     line-height: 80px;
		 display: flex;
		 justify-content: space-between;
	   }
	   .el-main {
		background-color: #48a2f7;
		background-image: url(../assets/bg.jpg);
		background-size: contain;
		background-repeat: no-repeat;
		background-position: 50%;
		text-align: center;
		.text {
		    font-size: 14px;
		  }
		
		  .item {
		    margin-bottom: 18px;
		  }
		
		  .box-card {
		    width: 22%;
			height: 85%;
			margin-left: 60%;
			margin-top: 2%;
		  }
	   }
	   .el-footer {
		   background-color: #FFFFFF;
		   color: #333;
		   text-align: center;
		   line-height: 60px;
	   }
	   
	   
}



</style>
