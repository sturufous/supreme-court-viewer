import { VuexModule, Module, Mutation, Action } from 'vuex-module-decorators'

@Module({
  namespaced: true
})
class CriminalFileInformation extends VuexModule {
  public criminalFileInformation = { }
  public showCaseDetails = true
  public showFutureAppearances = true
  public showPastAppearances = true
  public showWitnesses = true
  public showDocuments = true
  public showSentenceOrderDetails = true  

  @Mutation
  public setCriminalFile(criminalFileInformation): void {
    this.criminalFileInformation = criminalFileInformation
  }

  @Action
  public UpdateCriminalFile(newCriminalFileInformation): void {
    this.context.commit('setCriminalFile', newCriminalFileInformation)
  }

  @Mutation
  public setShowCaseDetails(showCaseDetails): void {
    this.showCaseDetails = showCaseDetails
  }

  @Action
  public UpdateShowCaseDetails(newShowCaseDetails): void {
    this.context.commit('setShowCaseDetails', newShowCaseDetails)
  }

  @Mutation
  public setShowFutureAppearances(showFutureAppearances): void {
    this.showFutureAppearances = showFutureAppearances
  }

  @Action
  public UpdateShowFutureAppearances(newShowFutureAppearances): void {
    this.context.commit('setShowFutureAppearances', newShowFutureAppearances)
  }

  @Mutation
  public setShowPastAppearances(showPastAppearances): void {
    this.showPastAppearances = showPastAppearances
  }

  @Action
  public UpdateShowPastAppearances(newShowPastAppearances): void {
    this.context.commit('setShowPastAppearances', newShowPastAppearances)
  }

  @Mutation
  public setShowWitnesses(showWitnesses): void {
    this.showWitnesses = showWitnesses
  }

  @Action
  public UpdateShowWitnesses(newShowWitnesses): void {
    this.context.commit('setShowWitnesses', newShowWitnesses)
  }

  @Mutation
  public setShowDocuments(showDocuments): void {
    this.showDocuments = showDocuments
  }

  @Action
  public UpdateShowDocuments(newShowDocuments): void {
    this.context.commit('setShowDocuments', newShowDocuments)
  }

  @Mutation
  public setSentenceOrderDetails(showSentenceOrderDetails): void {
    this.showSentenceOrderDetails = showSentenceOrderDetails
  }

  @Action
  public UpdateShowSentenceOrderDetails(newShowSentenceOrderDetails): void {
    this.context.commit('setShowSentenceOrderDetails', newShowSentenceOrderDetails)
  } 

}
export default CriminalFileInformation