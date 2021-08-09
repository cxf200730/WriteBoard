<template>
 <div>
	 <el-button type="primary" @click = "initWebSocket()">连接</el-button>
	 <el-button type="primary" @click = "sendmsg()">发数据</el-button>
	 <el-button type="primary" @click = "websocketclose()">断开</el-button>
 </div>
</template>
<script>

export default {
  data() {
    return {
		teseData:"123456"
    }
  },
  methods: {
		sendmsg(){
			this.websocketsend(this.teseData)
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
			console.log("连上了")
		},
		websocketonerror(){//连接建立失败重连
			console.log("出错了")
		},
		websocketonmessage(e){
			console.log("这是我收到的消息")
		
		},
		websocketsend(Data){//数据发送
			console.log(Data)
			console.log("这是数据长度" + Data.length )
			this.websock.send(Data);
		},
		websocketclose(e){  //关闭
			console.log('断开连接',e);
		},
  },

}
</script>
<style lang="less" scoped>
.all{
	
	
	
}



</style>
