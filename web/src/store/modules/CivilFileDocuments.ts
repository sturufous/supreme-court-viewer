import { VuexModule, Module, Mutation, Action } from 'vuex-module-decorators'

@Module({
  namespaced: true
})
class CivilFileDocuments extends VuexModule {
  public civilFileDocument = {
    proceedingDate: '',
    location: '',
    room: '',
    fileNumber: ''
  }      

  @Mutation
  public setCivilFile(civilFileDocument): void {
    this.civilFileDocument = civilFileDocument
  }

  @Action
  public UpdateCivilFile(newCivilFileDocument): void {
    this.context.commit('setCivilFile', newCivilFileDocument)
  }
}
export default CivilFileDocuments