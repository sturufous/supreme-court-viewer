import { VuexModule, Module, Mutation, Action } from 'vuex-module-decorators'

@Module({
  namespaced: true
})
class CivilFileInformation extends VuexModule {
  public civilFileInformation = { }
  public civilFileInfoLoaded = false

  public showSections = {
    'Case Details': true,
    'Future Appearances': false,
    'Past Appearances': false
  }

  public pastAppearanceInfo = {}

  @Mutation
  public setCivilFile(civilFileInformation): void {
    this.civilFileInformation = civilFileInformation
  }

  @Action
  public UpdateCivilFile(newCivilFileInformation): void {
    this.context.commit('setCivilFile', newCivilFileInformation)
  }

  @Mutation
  public setCivilFileInfoLoaded(civilFileInfoLoaded): void {
    this.civilFileInfoLoaded = civilFileInfoLoaded
  }

  @Action
  public UpdateCivilFileInfoLoaded(newCivilFileInfoLoaded): void {
    this.context.commit('setCivilFileInfoLoaded', newCivilFileInfoLoaded)
  }

  @Mutation
  public setShowSections(showSections): void {
    this.showSections = showSections
  }

  @Action
  public UpdateShowSections(newShowSections): void {
    this.context.commit('setShowSections', newShowSections)
  }

  @Mutation
  public setPastAppearanceInfo(pastAppearanceInfo): void {
    this.pastAppearanceInfo = pastAppearanceInfo
  }

  @Action
  public UpdatePastAppearanceInfo(newPastAppearanceInfo): void {
    this.context.commit('setPastAppearanceInfo', newPastAppearanceInfo)
  }

}
export default CivilFileInformation 