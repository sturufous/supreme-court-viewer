import { RouteConfig } from 'vue-router'
import Home from '../components/Home.vue'
import CivilDocumentsView from "../components/CivilDocumentsView.vue";
import CriminalDocumentsView from "../components/CriminalDocumentsView.vue";

const routes: Array<RouteConfig> = [
  {
    path: '/',
    name: 'Home',
    component: Home
  },
  {
    path: '/civil-file/:fileNumber',
    name: 'CivilDocumentsView',
    component: CivilDocumentsView, 
    props: true
  },
  {
    path: '/criminal-file/:fileNumber',
    name: 'CriminalDocumentsView',
    component: CriminalDocumentsView, 
    props: true
  }
]

export default routes

