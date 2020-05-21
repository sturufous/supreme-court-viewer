import { VuexModule, Module, Mutation, Action } from 'vuex-module-decorators'

@Module({
  namespaced: true
})
class CivilFileInformation extends VuexModule {
  public civilFileInformation = { }

  @Mutation
  public setCivilFile(civilFileInformation): void {
    this.civilFileInformation = civilFileInformation
  }

  @Action
  public UpdateCivilFile(newCivilFileInformation): void {
    this.context.commit('setCivilFile', newCivilFileInformation)
  }

}
export default CivilFileInformation 