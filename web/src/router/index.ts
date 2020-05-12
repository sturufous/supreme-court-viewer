import { RouteConfig } from 'vue-router'
import Home from '../components/Home.vue'
import CivilDocumentsView from "../components/CivilDocumentsView.vue";

const routes: Array<RouteConfig> = [
  {
    path: '/',
    name: 'Home',
    component: Home
  },
  {
    path: '/civil-documents/:fileId',
    name: 'CivilDocumentsView',
    component: CivilDocumentsView,
    props: true
  }
]

export default routes

