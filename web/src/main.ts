import Vue from 'vue';
import App from './App.vue';
import "./styles/index.scss";

import "bootstrap-vue/dist/bootstrap-vue.css"

import { BootstrapVue } from 'bootstrap-vue'

Vue.config.productionTip = true;

Vue.use(BootstrapVue);

new Vue({
	render: h => h(App)
}).$mount('#app');