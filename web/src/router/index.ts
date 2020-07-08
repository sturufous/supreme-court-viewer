import { RouteConfig } from 'vue-router'
import Home from '@components/Home.vue'
import CivilCaseDetails from "@components/civil/CivilCaseDetails.vue";
import CriminalCaseDetails from "@components/criminal/CriminalCaseDetails.vue";
import CivilFileSearchResultList from "@components/civil/CivilFileSearchResultList.vue";
import CriminalFileSearchResultList from "@components/criminal/CriminalFileSearchResultList.vue";
import CourtList from "@components/courtlist/CourtList.vue";

const routes: Array<RouteConfig> = [
  {
    path: '/',
    name: 'Home',
    component: Home
  },
  {
    path: '/court-list',
    name: 'CourtList',
    component: CourtList, 
    props: true,
    children: [
      {
        path: 'location/:location/room/:room/date/:date',
        name: 'CourtListResult',
        component: CourtList, 
        props: true
      }
    ]
  },
  {
    path: '/civil-file/:fileNumber',
    name: 'CivilCaseDetails',
    component: CivilCaseDetails, 
    props: true
  },
  {
    path: '/criminal-file/:fileNumber',
    name: 'CriminalCaseDetails',
    component: CriminalCaseDetails,
    props: true
  },
  {
    path: '/civil-files/fileNumber/:fileNumber/location/:location',
    name: 'CivilFileSearchResultList',
    component: CivilFileSearchResultList, 
    props: true
  },
  {
    path: '/criminal-file/fileNumber/:fileNumber/location/:location',
    name: 'CriminalFileSearchResultList',
    component: CriminalFileSearchResultList,
    props: true
  }
]

export default routes