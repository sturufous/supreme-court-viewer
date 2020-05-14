import Vue from 'vue'
import Vuex from 'vuex'
import CivilFileDocuments from '@/store/modules/CivilFileDocuments'
import CriminalFileDocuments from '@/store/modules/CriminalFileDocuments'

Vue.use(Vuex)

const store = new Vuex.Store({
  modules: {
    CivilFileDocuments,
    CriminalFileDocuments
  }
})

export default store
