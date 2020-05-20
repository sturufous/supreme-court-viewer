import { RouteConfig } from 'vue-router'
import Home from '@components/Home.vue'
import CivilDocumentsView from "@components/civil/CivilDocumentsView.vue";
import CriminalCaseDetails from "@components/criminal/CriminalCaseDetails.vue";

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
    name: 'CriminalCaseDetails',
    component: CriminalCaseDetails,
    props: true,
    children: [

    ]
  }
]

export default routes

