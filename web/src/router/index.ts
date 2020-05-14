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
    path: '/civil-documents',
    name: 'CivilDocumentsView',
    component: CivilDocumentsView
  },
  {
    path: '/criminal-documents',
    name: 'CriminalDocumentsView',
    component: CriminalDocumentsView
  }
]

export default routes

