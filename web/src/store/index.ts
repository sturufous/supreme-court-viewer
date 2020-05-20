import Vue from 'vue'
import Vuex from 'vuex'
import CivilFileInformation from '@/store/modules/CivilFileInformation'
import CriminalFileInformation from '@/store/modules/CriminalFileInformation'

Vue.use(Vuex)

const store = new Vuex.Store({
  modules: {
    CivilFileInformation,
    CriminalFileInformation
  }
})

export default store
