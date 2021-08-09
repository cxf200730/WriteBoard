import Vue from 'vue';
import Router from 'vue-router';
import Login from './components/Login.vue';
import Test from './components/test.vue';
import Watch from './components/Watch.vue';
Vue.use(Router);

const router = new Router({
    routes:[
        { path: '/', redirect: '/Watch'},
        { path: '/Login',component: Login,
        	redirect:'/Login',
        		children:[
        			{ path: '/Login',component: Login },
        ]},
		{ path: '/Test',component: Test,
			redirect:'/Test',
				children:[
					{ path: '/Test',component: Test },
		]},
		{ path: '/Watch',component: Watch,
			redirect:'/Watch',
				children:[
					{ path: '/Watch',component: Watch },
		]},
    ]
})

export default router