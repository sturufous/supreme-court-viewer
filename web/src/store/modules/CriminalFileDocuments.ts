import { VuexModule, Module, Mutation, Action } from 'vuex-module-decorators'

@Module({
  namespaced: true
})
class CriminalFileDocuments extends VuexModule {
  public criminalFileDocument = { }  

  @Mutation
  public setCriminalFile(criminalFileDocument): void {
    this.criminalFileDocument = criminalFileDocument
  }

  @Action
  public UpdateCriminalFile(newCriminalFileDocument): void {
    this.context.commit('setCriminalFile', newCriminalFileDocument)
  }
  
}
export default CriminalFileDocuments