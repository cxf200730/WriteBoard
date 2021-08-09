import Vue from 'vue'
import Vuex from 'vuex'
Vue.use(Vuex)

export default  new Vuex.Store({

	state: {
		userCode:null,
		classCode:null
	},
	getters:{
	
	},
	mutations: {
	     getuserCode(state, value){
	     	state.userCode = value
	    },
		getclassCode(state, value){
		 	state.classCode = value
		}
	},
	actions: {
	      
	}
})