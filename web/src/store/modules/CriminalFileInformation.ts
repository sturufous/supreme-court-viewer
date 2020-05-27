import { VuexModule, Module, Mutation, Action } from 'vuex-module-decorators'

@Module({
  namespaced: true
})
class CriminalFileInformation extends VuexModule {
  public criminalFileInformation = { }
  public criminalFileInfoLoaded = false
  public activeCriminalParticipantIndex = 0

  public showSections = {
    'Case Details': true,
    'Future Appearances': false,
    'Past Appearances': false, 
    'Witnesses': false,
    'Criminal Documents': false,
    'Sentence/Order Details': false
  }

  @Mutation
  public setCriminalFile(criminalFileInformation): void {
    this.criminalFileInformation = criminalFileInformation
  }

  @Action
  public UpdateCriminalFile(newCriminalFileInformation): void {
    this.context.commit('setCriminalFile', newCriminalFileInformation)
  }

  @Mutation
  public setCriminalFileInfoLoaded(criminalFileInfoLoaded): void {
    this.criminalFileInfoLoaded = criminalFileInfoLoaded
  }

  @Action
  public UpdateCriminalFileInfoLoaded(newCriminalFileInfoLoaded): void {
    this.context.commit('setCriminalFileInfoLoaded', newCriminalFileInfoLoaded)
  }

  @Mutation
  public setActiveCriminalParticipantIndex(activeCriminalParticipantIndex): void {
    this.activeCriminalParticipantIndex = activeCriminalParticipantIndex
  }

  @Action
  public UpdateActiveCriminalParticipantIndex(newActiveCriminalParticipantIndex): void {
    this.context.commit('setActiveCriminalParticipantIndex', newActiveCriminalParticipantIndex)
  }

  @Mutation
  public setShowSections(showSections): void {
    this.showSections = showSections
  }

  @Action
  public UpdateShowSections(newShowSections): void {
    this.context.commit('setShowSections', newShowSections)
  }


}
export default CriminalFileInformation