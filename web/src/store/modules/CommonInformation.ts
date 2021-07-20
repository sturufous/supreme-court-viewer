import { VuexModule, Module, Mutation, Action } from 'vuex-module-decorators';
import {iconStyleType} from '@/types/common';

enum appearanceStatus {UNCF='Unconfirmed', CNCL='Canceled', SCHD='Scheduled' }

@Module({
  namespaced: true
})
class CommonInformation extends VuexModule {  

  public displayName = ''
  public time = ''
  public duration = ''
  public statusStyle = ''
  public iconStyles: iconStyleType[] = [];

  @Mutation
  public setDisplayName(displayName): void {   
    this.displayName = displayName
  }

  @Action({rawError: true})
  public UpdateDisplayName(inputNames): void {
    let newDisplayName = ''
    if(inputNames.lastName.length==0) {
      newDisplayName = inputNames.givenName;
    } else if(inputNames.givenName.length==0) {
      newDisplayName = inputNames.lastName;
    } else if(inputNames.givenName.length==0 && inputNames.lastName.length==0) {
      newDisplayName = '';
    } else {
      newDisplayName = inputNames.lastName.charAt(0).toUpperCase() + inputNames.lastName.slice(1).toLowerCase() + ", " 
                      + inputNames.givenName.charAt(0).toUpperCase() + inputNames.givenName.slice(1).toLowerCase();
    }
    this.context.commit('setDisplayName', newDisplayName)
  }

  @Action
  public FormatDisplayName(newDisplayName): void {
    this.context.commit('setDisplayName', newDisplayName.charAt(0).toUpperCase() + newDisplayName.slice(1).toLowerCase())
  }

  @Mutation
  public setTime(time): void {   
    this.time = time
  }
  
  @Action
  public UpdateTime(time) {
    const time12 = (Number(time.substr(0,2)) % 12 || 12 ) + time.substr(2,3)
    
    if (Number(time.substr(0,2))<12) {
      this.context.commit('setTime', time12 +' AM')
    } else {
      this.context.commit('setTime', time12 +' PM')
    }      
  }

  @Mutation
  public setDuration(duration): void {   
    this.duration = duration
  }

  @Action
  public UpdateDuration(newDuration) {        
    let duration = '';
    if(newDuration.hr) {
      if(Number(newDuration.hr)==1)            
          duration += '1 Hr ';
      else if(Number(newDuration.hr)>1)
          duration += Number(newDuration.hr)+' Hrs ';
    }

    if(newDuration.min) {
      if(Number(newDuration.min)==1)            
          duration += '1 Min ';
      else if(Number(newDuration.min)>1)
          duration += Number(newDuration.min)+' Mins ';
    }    

    this.context.commit('setDuration', duration)
  }

  @Mutation
  public setStatusStyle(statusStyle): void {   
    this.statusStyle = statusStyle
  }

  @Action
  public UpdateStatusStyle(status) {

    let style = '';
    if(status == appearanceStatus.UNCF) {
      style = "badge badge-danger mt-2";
    } else if(status == appearanceStatus.CNCL) {
      style = "badge badge-warning mt-2";
    } else if(status == appearanceStatus.SCHD) {
      style = "badge badge-primary mt-2";
    }
    
    this.context.commit('setStatusStyle', style)
  }

  @Mutation
  public setIconStyle(iconStyles): void {   
    this.iconStyles = iconStyles
  }

  @Action
  public UpdateIconStyle(newIconsInfo) {
    const iconStyles: iconStyleType[] = [];
    for (const iconInfo of newIconsInfo) {
      if(iconInfo["info"] == "UNCF") {
        iconStyles.push({"icon":'circle-half', "desc":appearanceStatus.UNCF});
      } else if(iconInfo["info"] == "CNCL") {
        iconStyles.push({"icon":'trash', "desc":appearanceStatus.CNCL});
      } else if(iconInfo["info"] == "SCHD") {
        iconStyles.push({"icon":'calendar', "desc":appearanceStatus.SCHD})
      } else if(iconInfo["info"] == "Video") {
        iconStyles.push({"icon":'camera-video-fill', "desc": "video"})
      } else if(iconInfo["info"] == "Home") {
        iconStyles.push({"icon":'house-door-fill', "desc": iconInfo["desc"]})
      } 
    }   
    this.context.commit('setIconStyle', iconStyles)
  }  
}

export default CommonInformation 