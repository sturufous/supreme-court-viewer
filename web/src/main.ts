import Vue from 'vue';
import VueResource from 'vue-resource'
import VueRouter from 'vue-router'
import BootstrapVue from "bootstrap-vue";
import "./styles/index.scss";
import "bootstrap-vue/dist/bootstrap-vue.css"

import App from './App.vue';
import routes from './router/index'
import store from './store/index'

Vue.use(VueResource);
Vue.use(VueRouter);
Vue.use(BootstrapVue);
Vue.config.productionTip = true;

(Vue as any).http.options.root = process.env.VUE_APP_API_URL

const router = new VueRouter({
	mode: 'history',
	routes: routes
});

new Vue({
	router,
	store,
	render: h => h(App)	
}).$mount('#app');