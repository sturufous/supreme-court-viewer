import { RouteConfig } from 'vue-router'
import Home from '../components/Home.vue'
import CivilDocumentsView from "../components/CivilDocumentsView.vue";

// Vue.use(VueRouter)

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
  }
]
//   {
//     path: '/sample-api',
//     name: 'SampleAPI',
//     // route level code-splitting
//     // this generates a separate chunk (about.[hash].js) for this route
//     // which is lazy-loaded when the route is visited.
//     component: () => import(/* webpackChunkName: "about" */ '../views/SampleAPI.vue')
//   }
// ]

// export default const routes = [
//   {path: "/", component: Home}

// ]

export default routes

