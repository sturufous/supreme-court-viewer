import Vue from 'vue';
import App from './App.vue';
import BootstrapVue from "bootstrap-vue";
import "./styles/index.scss";

import "bootstrap-vue/dist/bootstrap-vue.css"

import { BootstrapVue } from 'bootstrap-vue'

Vue.use(BootstrapVue);
Vue.config.productionTip = true;

Vue.use(BootstrapVue);

new Vue({
	render: h => h(App)
}).$mount('#app');