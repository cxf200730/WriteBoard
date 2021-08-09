import Vue from 'vue'
import App from './App.vue'
import router from './router';
import './assets/css/global.css';
import './plugin/element';
import './assets/icon/iconfont.css'
import vuescroll from 'vuescroll';
// import 'vuescroll/dist/vuescroll.css';
Vue.use(vuescroll);
import store from 'store'
Vue.config.productionTip = false

new Vue({
	store,
	router,
	render: h => h(App),
}).$mount('#app')