import { RouteConfig } from 'vue-router'
import Home from '@components/Home.vue'
import CivilCaseDetails from "@components/civil/CivilCaseDetails.vue";
import CriminalCaseDetails from "@components/criminal/CriminalCaseDetails.vue";
import CivilFutureAppearances from "@components/civil/CivilFutureAppearances.vue";
import CriminalFutureAppearances from "@components/criminal/CriminalFutureAppearances.vue";
import CivilPastAppearances from "@components/civil/CivilPastAppearances.vue";
import CriminalPastAppearances from "@components/criminal/CriminalPastAppearances.vue";
import CriminalWitnesses from "@components/criminal/CriminalWitnesses.vue";
import CriminalSentence from "@components/criminal/CriminalSentence.vue";
import CriminalDocumentsView from "@components/criminal/CriminalDocumentsView.vue";


const routes: Array<RouteConfig> = [
  {
    path: '/',
    name: 'Home',
    component: Home
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
  }
]

export default routes