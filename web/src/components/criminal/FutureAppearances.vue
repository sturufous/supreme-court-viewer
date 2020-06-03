<template>

    <b-card bg-variant="white">
        <div>
            <h3 class="mx-2 font-weight-normal" v-if="!showSections['Future Appearances']"> Next Three Future Appearances</h3>
            <hr class="mb-0 bg-light" style="height: 5px;"/> 
        </div>

        <b-card v-if="!isDataReady && isMounted">
            <span class="text-muted"> No future appearances. </span>
        </b-card>

        <b-card bg-variant="light" v-if= "!isMounted && !isDataReady">
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

        <b-card bg-variant="white" v-if="isDataReady">           
            <b-table
            :items="SortedFutureAppearances"
            :fields="fields"
            :sort-by.sync="sortBy"
            :sort-desc.sync="sortDesc"
            :no-sort-reset="true"
            sort-icon-left
            borderless
            small
            responsive="sm"
            >   
                <template v-for="(field,index) in fields" v-slot:[`head(${field.key})`]="data">
                    <b v-bind:key="index" :class="field.headerStyle" > {{ data.label }}</b>
                </template>

                <template  v-slot:cell()="data">
                    <b-badge                        
                        style="font-weight: normal; font-size: 16px; padding-top:12px" 
                        variant="white" > 
                            {{data.value}} 
                    </b-badge>
                </template>

                <template v-slot:cell(Date)="data" >
                    <span :class="data.field.cellStyle"> 
                        <b-button style="transform: translate(0,-7px)" @click="OpenDetails(data);data.toggleDetails();" variant="outline-primary border-white" class="mr-2">
                            <b-icon-caret-right-fill v-if="!data.item['_showDetails']"></b-icon-caret-right-fill>
                            <b-icon-caret-down-fill v-if="data.item['_showDetails']"></b-icon-caret-down-fill>
                        </b-button>
                        {{data.value| beautify-date}}
                    </span> 
                </template>
                <template v-slot:row-details>
                    <b-card> 
                        <appearance-details/>
                    </b-card>
                </template>

                <template  v-slot:cell(Reason)="data">
                    <b-button 
                            :class="data.field.cellStyle"
                            variant="outline-primary border-white"
                            v-b-tooltip.hover                            
                            :title="data.item['Reason Description']"
                            style="margin-top: 1px;"> 
                            {{data.value}}
                    </b-button>
                </template>

                <template  v-slot:cell(Presider)="data">
                    <b-button                              
                            variant="outline-primary border-white"
                            v-if="data.value"
                            v-b-tooltip.hover                           
                            :title="data.item['Judge Full Name']"> 
                            {{data.value}}
                    </b-button>
                </template>

                <template  v-slot:cell(Accused)="data">
                     <b-badge  variant="white" style=" font-size: 16px;" class = "mt-2"> {{data.value}} </b-badge>
                </template>

                <template  v-slot:cell(Status)="data">
                    <b :class = "getStatusStyle(data.value)" style="font-weight: normal; font-size: 16px;"> {{data.value}} </b>
                </template>
                
            </b-table>
        </b-card>
      
    </b-card> 

</template>

<script lang="ts">
import { Component, Vue } from "vue-property-decorator";
import { namespace } from "vuex-class";

import AppearanceDetails from '@components/criminal/AppearanceDetails.vue';

import "@store/modules/CriminalFileInformation";
const criminalState = namespace("CriminalFileInformation");

enum appearanceStatus {UNCF='Unconfirmed', CNCL='Canceled', SCHD='Scheduled' }

@Component({
    components: {
        AppearanceDetails
    }
})
export default class FutureAppearances extends Vue {

    @criminalState.State
    public criminalFileInformation!: any;

    @criminalState.State
    public showSections
    
    @criminalState.State
    public pastAppearanceInfo!: any;

    @criminalState.Action
    public UpdatePastAppearanceInfo!: (newPastAppearanceInfo: any) => void

    mounted() {
        this.getFutureAppearances();
    }

    public getFutureAppearances(): void {      
    
        const data = this.criminalFileInformation.detailsData;
        this.futureAppearancesJson = data.appearances.apprDetail;              
        this.ExtractFutureAppearancesInfo();
        if(this.futureAppearancesList.length)
        {                    
            this.isDataReady = true;
        }
    
    this.isMounted = true;
                       
           
    } 
  
    isMounted = false;
    isDataReady = false;
    futureAppearancesJson;
    
    sortBy = 'Date';
    sortDesc = true;
    futureAppearancesList: any[] = [];

    fields =  
    [
        {key:'Date',       sortable:true,  tdClass: 'border-top', headerStyle:'text-primary', cellStyle:'text-info mt-2 d-inline-flex'},
        {key:'Reason',     sortable:true,  tdClass: 'border-top', headerStyle:'text-primary', cellStyle:'font-weight-bold'},
        {key:'Time',       sortable:false, tdClass: 'border-top', headerStyle:'text',         cellStyle:'text'},
        {key:'Duration',   sortable:false, tdClass: 'border-top', headerStyle:'text',         cellStyle:'text'},
        {key:'Location',   sortable:true,  tdClass: 'border-top', headerStyle:'text-primary', cellStyle:'text'},
        {key:'Room',       sortable:false, tdClass: 'border-top', headerStyle:'text',         cellStyle:'text'},
        {key:'Accused',    sortable:true,  tdClass: 'border-top', headerStyle:'text-primary', cellStyle:'text'},
        {key:'Status',     sortable:true,  tdClass: 'border-top', headerStyle:'text-primary', cellStyle:'badge'},
    ];    
  
    public ExtractFutureAppearancesInfo(): void {
        const currentDate = new Date();

        for (const fileIndex in this.futureAppearancesJson) {
            const fileInfo = {};
            const jFile = this.futureAppearancesJson[fileIndex];

            fileInfo["Index"] = fileIndex;
            fileInfo["Date"] = jFile.appearanceDt.split(' ')[0]
            if(new Date(fileInfo["Date"]) < currentDate) continue;
            fileInfo["Time"] = this.getTime(jFile.appearanceTm.split(' ')[1].substr(0,5));
            fileInfo["Reason"] = jFile.appearanceReasonCd;
            fileInfo["Reason Description"] = jFile.appearanceReasonDsc? jFile.appearanceReasonDsc: console.log(fileInfo["Date"]);
          
            fileInfo["Duration"] = this.getDuration(jFile.estimatedTimeHour, jFile.estimatedTimeMin)           
            fileInfo["Location"] = jFile.courtLocation;
            fileInfo["Room"] =jFile.courtRoomCd

            fileInfo["First Name"] = jFile.givenNm ? jFile.givenNm : "";
            fileInfo["Last Name"] = jFile.lastNm ? jFile.lastNm : jFile.orgNm;
            fileInfo["Accused"] = this.getNameOfParticipant(fileInfo["Last Name"], fileInfo["First Name"]);  
            fileInfo["Status"] = jFile.appearanceStatusCd ? appearanceStatus[jFile.appearanceStatusCd] :''

            fileInfo["Presider"] =  jFile.judgeInitials ? jFile.judgeInitials :''
            fileInfo["Judge Full Name"] =  jFile.judgeInitials ? jFile.judgeFullNm : ''

            fileInfo["Appearance ID"] = jFile.appearanceId
            fileInfo["Supplemental Equipment"] = jFile.supplementalEquipmentTxt
            fileInfo["Security Restriction"] = jFile.securityRestrictionTxt
            fileInfo["OutOfTown Judge"] = jFile.outOfTownJudgeTxt
                       
            this.futureAppearancesList.push(fileInfo); 
        }
    }

    public getStatusStyle(status)
    {
        if(status == appearanceStatus.UNCF) return "badge badge-danger mt-2";
        else if(status == appearanceStatus.CNCL) return "badge badge-warning mt-2";
        else if(status == appearanceStatus.SCHD) return "badge badge-primary mt-2";
    }

    public getNameOfParticipant(lastName, givenName) {
        return ( lastName + ", " + givenName );
    }

    public getTime(time)
    {
        const time12 = (Number(time.substr(0,2)) % 12 || 12 ) + time.substr(2,3)
       
        if(Number(time.substr(0,2))<12) return time12 +' AM'; 
            else  return time12 +' PM';       
    }

    public getDuration(hr, min)
    {        
        let duration = '';
        if(hr)
        {
            if(Number(hr)==1)            
                duration += '1 Hr ';
            else if(Number(hr)>1)
                duration += Number(hr)+' Hrs ';
        }

        if(min)
        {
            if(Number(min)==1)            
                duration += '1 Min ';
            else if(Number(min)>1)
                duration += Number(min)+' Mins ';
        }

        return duration
    }

    public OpenDetails(data)
    {
        if(!data.detailsShowing)
        {
            this.pastAppearanceInfo.fileNo = this.criminalFileInformation.fileNumber; 
            
            this.pastAppearanceInfo.appearanceId = data.item["Appearance ID"]
            this.pastAppearanceInfo.supplementalEquipmentTxt = data.item["Supplemental Equipment"]
            this.pastAppearanceInfo.securityRestrictionTxt = data.item["Security Restriction"]
            this.pastAppearanceInfo.outOfTownJudgeTxt = data.item["OutOfTown Judge"]

            this.UpdatePastAppearanceInfo(this.pastAppearanceInfo);
        }
        
    }

    get SortedFutureAppearances()
    {           
        if(this.showSections['Future Appearances'])
        {
            return this.futureAppearancesList;
        }
        else
        {
            return  this.futureAppearancesList
            .sort((a, b): any =>
            {            
                if(a["Date"] > b["Date"]) return -1;
                else if(a["Date"] < b["Date"]) return 1;
                else return 0;
            })
            .slice(0, 3);
           
        }        
    }
}
</script>

<style scoped>
 .card {
        border: white;
    }
</style>