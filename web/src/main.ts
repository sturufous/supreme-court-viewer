import 'core-js/stable'
import 'regenerator-runtime/runtime'
import 'intersection-observer'
import Vue from 'vue';
import VueResource from 'vue-resource'
import VueRouter from 'vue-router'
import BootstrapVue from "bootstrap-vue";
import "./styles/index.scss";
import "bootstrap-vue/dist/bootstrap-vue.css"
import App from './App.vue';
import routes from './router/index'
import store from './store/index'
import LoadingSpinner from "./components/LoadingSpinner.vue";

Vue.use(VueResource);
Vue.use(VueRouter);
Vue.use(BootstrapVue);
Vue.config.productionTip = true;
Vue.component('loading-spinner', LoadingSpinner);

const router = new VueRouter({
	mode: 'hash',
	routes: routes
});

new Vue({
	router,
	store,
	render: h => h(App)	
}).$mount('#app');