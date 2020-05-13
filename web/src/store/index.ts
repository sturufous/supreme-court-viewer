import Vue from 'vue'
import Vuex from 'vuex'
import CivilFileDocuments from '@/store/modules/CivilFileDocuments'
import CriminalFileDocuments from '@/store/modules/CivilFileDocuments'

Vue.use(Vuex)

const store = new Vuex.Store({
  modules: {
    CivilFileDocuments,
    CriminalFileDocuments
  }
})

export default store
