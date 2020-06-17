import { VuexModule, Module, Mutation, Action } from 'vuex-module-decorators'

@Module({
  namespaced: true
})
class CourtListInformation extends VuexModule {
  public courtListInformation = { }

  @Mutation
  public setCourtList(courtListInformation): void {
    this.courtListInformation = courtListInformation
  }

  @Action
  public UpdateCourtList(newCourtListInformation): void {
    this.context.commit('setCourtList', newCourtListInformation)
  }

}
export default CourtListInformation