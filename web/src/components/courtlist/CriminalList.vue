<template>

    <b-card bg-variant="white" no-body>
        <div>
            <h2 class="mx-4 mt-5 font-weight-normal text-warning ">Criminal</h2>
            <hr class="mx-3 bg-warning" style="height: 5px;"/> 
        </div>

        <b-card v-if="!isDataReady && isMounted" no-body>
            <span class="text-muted ml-4 mb-5"> No Criminal List has been found. </span>
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
            :items="criminalList"
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
                        <criminal-appearance-details/>
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

                <template v-slot:[`cell(${fields[9].key})`]="data" >                     
                    <span>{{data.value}}</span>
                    <span class="text-muted" style="font-size:14px">d</span>                  
                </template>
             <!--   
                <template  v-slot:cell(Accused)="data">
                     <b-badge  variant="white" style=" font-size: 16px;" class = "mt-2"> {{data.value}} </b-badge>
                </template>

                <template  v-slot:cell(Status)="data">
                    <b :class = "data.item['Status Style']" style="font-weight: normal; font-size: 16px; width:110px"> {{data.value}} </b>
                </template> -->
                
            </b-table>
        </b-card>
      
    </b-card> 

</template>

<script lang="ts">
import { Component, Vue } from "vue-property-decorator";
import { namespace } from "vuex-class";
import CriminalAppearanceDetails from '@components/criminal/CriminalAppearanceDetails.vue';

import "@store/modules/CommonInformation";
const commonState = namespace("CommonInformation");
import '@store/modules/CourtListInformation';
const courtListState = namespace('CourtListInformation');
import "@store/modules/CriminalFileInformation";
const criminalState = namespace("CriminalFileInformation");

@Component({
    components: {
        CriminalAppearanceDetails
    }
})
export default class CriminalList extends Vue {

    @courtListState.State
    public courtListInformation!: any

    @criminalState.State
    public appearanceInfo!: any;

    @criminalState.Action
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
        this.getCriminalList();
    }

    public getCriminalList(): void 
    {            
        const data = this.courtListInformation.detailsData;
       
        this.criminalCourtListJson = data.criminalCourtList

         //console.log(this.criminalCourtListJson)
        this.courtRoom = data.courtRoomCode    
        this.ExtractCriminalListInfo()
        if(this.criminalList.length)
        {                    
            this.isDataReady = true;
        }
    
        this.isMounted = true;
    } 

    criminalList: any[] = [];
    
    criminalCourtListJson;
    courtRoom;
    isMounted = false;
    isDataReady = false;
    
    fields =  
    [
        {key:'File Number', tdClass: 'border-top', headerStyle:'', cellStyle:''},
        {key:'Accused',     tdClass: 'border-top', headerStyle:'', cellStyle:'text-primary'},
        {key:'Time',        tdClass: 'border-top', headerStyle:'', cellStyle:''},
        {key:'Est.',        tdClass: 'border-top', headerStyle:'', cellStyle:''},
        {key:'Reason',      tdClass: 'border-top', headerStyle:'', cellStyle:''},
        {key:'Room',        tdClass: 'border-top', headerStyle:'', cellStyle:'text-primary'},
        {key:'Counsel',     tdClass: 'border-top', headerStyle:'', cellStyle:''},
        {key:'File Markers',tdClass: 'border-top', headerStyle:'', cellStyle:''},
        {key:'Crown',       tdClass: 'border-top', headerStyle:'', cellStyle:''},
        {key:'Case Age',    tdClass: 'border-top', headerStyle:'', cellStyle:''},
        {key:'Notes',       tdClass: 'border-top', headerStyle:'', cellStyle:''},
    ];
    
    
  
    public ExtractCriminalListInfo(): void {
        const currentDate = new Date();

        for (const criminalListIndex in this.criminalCourtListJson) 
        {
            const criminalListInfo = {};
            const jcriminalList = this.criminalCourtListJson[criminalListIndex];

            criminalListInfo["Index"] = criminalListIndex;
            criminalListInfo['File Number']=jcriminalList.fileNumberText
            criminalListInfo['Case Age']= jcriminalList.caseAgeDaysNumber? jcriminalList.caseAgeDaysNumber: ''
            criminalListInfo["Time"] = this.getTime(jcriminalList.appearanceTime.split(' ')[1].substr(0,5));

            criminalListInfo["Room"] = this.courtRoom
            criminalListInfo["Accused"] = jcriminalList.accusedFullName
            criminalListInfo['Reason'] = jcriminalList.appearanceCount[0].appearanceReasonCode

            criminalListInfo['Counsel'] = jcriminalList.counselFullName

            criminalListInfo['File Markers'] = jcriminalList.hearingRestriction.length>0? jcriminalList.hearingRestriction[0].hearingRestrictiontype :''

            criminalListInfo['PartID'] =  jcriminalList.fileInformation.partId
            criminalListInfo['JustinNo'] = jcriminalList.fileInformation.mdocJustinNo
            criminalListInfo['AppearanceID'] = jcriminalList.criminalAppearanceID
            
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
                       
            this.criminalList.push(criminalListInfo); 
            console.log(criminalListInfo)
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
            this.appearanceInfo.fileNo = data.item['JustinNo']
            this.appearanceInfo.appearanceId = data.item["AppearanceID"]
            this.appearanceInfo.partId = data.item["PartID"]
            // this.appearanceInfo.supplementalEquipmentTxt = data.item["Supplemental Equipment"]
            // this.appearanceInfo.securityRestrictionTxt = data.item["Security Restriction"]
            // this.appearanceInfo.outOfTownJudgeTxt = data.item["OutOfTown Judge"]
            this.UpdateAppearanceInfo(this.appearanceInfo);
        }        
    }

    get SortedCriminalList()
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