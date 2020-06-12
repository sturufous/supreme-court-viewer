<template>

    <b-card bg-variant="white" no-body>
        <div>
            <h2 class="mx-4 mt-5 font-weight-normal text-success">Civil</h2>
            <hr class="mx-3 bg-success" style="height: 5px;"/> 
        </div>

        <b-card v-if="!isDataReady && isMounted" no-body>
            <span class="text-muted ml-4 mb-5"> No Civil List has been found. </span>
        </b-card>

        <b-card bg-variant="light" v-if= "!isMounted && !isDataReady" >
            <b-overlay :show= "true"> 
                <b-card  style="min-height: 100px;"/>                   
                <template v-slot:overlay>               
                <div> 
                    <loading-spinner/> 
                    <p id="loading-label">Loading ...</p>
                </div>                
                </template> 
            </b-overlay> 
        </b-card>

        <b-card bg-variant="white" v-if="isDataReady" no-body class="mx-3">           
            <b-table
            :items="civilList"
            :fields="fields"            
            borderless
            small
            responsive="sm"
            >   
                <template v-slot:head()="data">
                    <b> {{ data.label }}</b>
                </template>

                <template  v-slot:cell()="data">
                    <b-badge                        
                        style="font-weight: normal; font-size: 16px; padding-top:12px" 
                        :class="data.field.cellStyle"
                        variant="white" > 
                            {{data.value}} 
                    </b-badge>
                </template>

                <template v-slot:[`cell(${fields[0].key})`]="data" >                     
                    <b-button style="transform: translate(0,-7px); font-size:16px" 
                                size="sm" 
                                @click="OpenDetails(data); data.toggleDetails();" 
                                variant="outline-primary border-white text-info" 
                                class="mr-2 mt-1">
                        <b-icon-caret-right-fill v-if="!data.item['_showDetails']"></b-icon-caret-right-fill>
                        <b-icon-caret-down-fill v-if="data.item['_showDetails']"></b-icon-caret-down-fill>
                        {{data.value}}
                    </b-button>                   
                </template>

                <template v-slot:row-details>
                    <b-card no-body bg-border="dark"> 
                        <civil-appearance-details/>
                    </b-card>
                </template> 
                
                <template  v-slot:cell(Reason)="data">
                    <b-badge
                            variant="secondary"
                            v-b-tooltip.hover.right                            
                            :title="data.item['Reason Description']"
                            style="margin-top: 10px; font-size: 14px;"> 
                            {{data.value}}
                    </b-badge>
                </template>

            </b-table>
        </b-card>
      
    </b-card> 

</template>

<script lang="ts">
import { Component, Vue } from "vue-property-decorator";
import { namespace } from "vuex-class";
import CivilAppearanceDetails from '@components/civil/CivilAppearanceDetails.vue';

import "@store/modules/CommonInformation";
const commonState = namespace("CommonInformation");
import '@store/modules/CourtListInformation';
const courtListState = namespace('CourtListInformation');
import "@store/modules/CivilFileInformation";
const civilState = namespace("CivilFileInformation");

@Component({
    components: {
        CivilAppearanceDetails
    }
})
export default class CivilList extends Vue {

    @courtListState.State
    public courtListInformation!: any

    @civilState.State
    public appearanceInfo!: any;

    @civilState.Action
    public UpdateAppearanceInfo!: (newAppearanceInfo: any) => void    

    @commonState.State
    public displayName!: string;    

    @commonState.State
    public duration

    @commonState.State
    public time

    @commonState.Action
    public UpdateDisplayName!: (newInputNames: any) => void

    @commonState.Action
    public UpdateDuration!: (duration: any) => void

    @commonState.Action
    public UpdateTime!: (time: any) => void

    mounted() {
        this.getCivilList();
    }

    public getCivilList(): void 
    {            
        const data = this.courtListInformation.detailsData;
       
        this.civilCourtListJson = data.civilCourtList

         //console.log(this.civilCourtListJson)
        this.courtRoom = data.courtRoomCode    
        this.ExtractCivilListInfo()
        if(this.civilList.length)
        {                    
            this.isDataReady = true;
        }
    
        this.isMounted = true;
    } 

    civilList: any[] = [];
    
    civilCourtListJson;
    courtRoom;
    isMounted = false;
    isDataReady = false;
    
    fields =  
    [
        {key:'File Number', tdClass: 'border-top', headerStyle:'', cellStyle:''},
        {key:'Parties',     tdClass: 'border-top', headerStyle:'', cellStyle:'text-primary'},
        {key:'Time',        tdClass: 'border-top', headerStyle:'', cellStyle:''},
        {key:'Est.',        tdClass: 'border-top', headerStyle:'', cellStyle:''},
        {key:'Reason',      tdClass: 'border-top', headerStyle:'', cellStyle:''},
        {key:'Room',        tdClass: 'border-top', headerStyle:'', cellStyle:'text-primary'},
        {key:'Counsel',     tdClass: 'border-top', headerStyle:'', cellStyle:''},
        {key:'File Markers',tdClass: 'border-top', headerStyle:'', cellStyle:''},
        {key:'Notes',       tdClass: 'border-top', headerStyle:'', cellStyle:''},
    ];
    
    
  
    public ExtractCivilListInfo(): void {
        
        for (const civilListIndex in this.civilCourtListJson) 
        {
            const civilListInfo = {};
            const jcivilList = this.civilCourtListJson[civilListIndex];

            civilListInfo["Index"] = civilListIndex;
            civilListInfo['File Number']=jcivilList.physicalFile.fileNumber           
            civilListInfo["Time"] = this.getTime(jcivilList.appearanceTime.substr(0,5));

            civilListInfo["Room"] = this.courtRoom
            civilListInfo["Parties"] = jcivilList.sealFileSOCText
            civilListInfo['Reason'] = jcivilList.document[0].appearanceReasonCode
            civilListInfo['Est.'] = jcivilList.scheduledAppearance.length>0 ? this.getDuration(jcivilList.scheduledAppearance[0].estDurationHours, jcivilList.scheduledAppearance.estDurationMins): ''
            // civilListInfo['Counsel'] = jcivilList.counselFullName

            // civilListInfo['File Markers'] = jcivilList.hearingRestriction.length>0? jcivilList.hearingRestriction[0].hearingRestrictiontype :''

            // civilListInfo['PartID'] =  jcivilList.fileInformation.partId
            civilListInfo['FileID'] = jcivilList.physicalFile.physicalFileID
            civilListInfo['AppearanceID'] = jcivilList.appearanceId
            
        //     appInfo["Date"] = jApp.appearanceDt.split(' ')[0]
        //     if(new Date(appInfo["Date"]) < currentDate) continue;            
        //     appInfo["FormattedDate"] = Vue.filter('beautify-date')(appInfo["Date"]);
        //     appInfo["Time"] = this.getTime(jApp.appearanceTm.split(' ')[1].substr(0,5));
        //     appInfo["Reason"] = jApp.appearanceReasonCd;
        //     appInfo["Reason Description"] = jApp.appearanceReasonDsc? jApp.appearanceReasonDsc: '';
          
        //     appInfo["Duration"] = this.getDuration(jApp.estimatedTimeHour, jApp.estimatedTimeMin)           
        //     appInfo["Location"] = jApp.courtLocation;
        //     appInfo["Room"] =jApp.courtRoomCd

        //     appInfo["First Name"] = jApp.givenNm ? jApp.givenNm : "";
        //     appInfo["Last Name"] = jApp.lastNm ? jApp.lastNm : jApp.orgNm;
        //     appInfo["Accused"] = this.getNameOfParticipant(appInfo["Last Name"], appInfo["First Name"]);  
        //     appInfo["Status"] = jApp.appearanceStatusCd ? appearanceStatus[jApp.appearanceStatusCd] :''
        //     appInfo["Status Style"] = this.getStatusStyle(appInfo["Status"])
        //     appInfo["Presider"] =  jApp.judgeInitials ? jApp.judgeInitials :''
        //     appInfo["Judge Full Name"] =  jApp.judgeInitials ? jApp.judgeFullNm : ''

        //     appInfo["Appearance ID"] = jApp.appearanceId            
        //     appInfo["Part ID"] = jApp.partId
        //     appInfo["Supplemental Equipment"] = jApp.supplementalEquipmentTxt
        //     appInfo["Security Restriction"] = jApp.securityRestrictionTxt
        //     appInfo["OutOfTown Judge"] = jApp.outOfTownJudgeTxt
                       
            this.civilList.push(civilListInfo); 
            //console.log(civilListInfo)
        }
    }

    public getNameOfParticipant(lastName, givenName) {
        this.UpdateDisplayName({'lastName': lastName, 'givenName': givenName});
        return this.displayName;        
    }

    public getTime(time)
    {
        this.UpdateTime(time);
        return this.time;      
    }

    public getDuration(hr, min)
    {
        this.UpdateDuration({'hr': hr, 'min': min});
        return this.duration;
    }

    public OpenDetails(data)
    {
        if(!data.detailsShowing)
        {
            this.appearanceInfo.fileNo = data.item['FileID']
            this.appearanceInfo.appearanceId = data.item["AppearanceID"]
            this.appearanceInfo.partId = data.item["PartID"]
            // this.appearanceInfo.supplementalEquipmentTxt = data.item["Supplemental Equipment"]
            // this.appearanceInfo.securityRestrictionTxt = data.item["Security Restriction"]
            // this.appearanceInfo.outOfTownJudgeTxt = data.item["OutOfTown Judge"]
            this.UpdateAppearanceInfo(this.appearanceInfo);
        }        
    }

    get SortedCivilList()
    {           
        return 0       
    }
}
</script>

<style scoped>
 .card {
        border: white;
    }
</style>