<template>

    <b-card bg-variant="white">
        <div>
            <h3 class="mx-2 font-weight-normal" v-if="!showSections['Future Appearances']"> Next Three Future Appearances</h3>
            <hr class="mb-3 bg-light" style="height: 5px;"/> 
        </div>

        <b-card v-if="!isDataReady && isMounted">
            <span class="text-muted"> No future appearances. </span>
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

        <b-card bg-variant="white" v-if="isDataReady" no-body>           
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
                        <b-button style="transform: translate(0,-7px)" size="sm" @click="OpenDetails(data);data.toggleDetails();" variant="outline-primary border-white" class="mr-2 mt-1">
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
                    <b-badge
                            variant="secondary"
                            v-b-tooltip.hover.right                            
                            :title="data.item['Reason Description']"
                            style="margin-top: 10px; font-size: 14px;"> 
                            {{data.value}}
                    </b-badge>
                </template>

                <template  v-slot:cell(Accused)="data">
                     <b-badge  variant="white" style=" font-size: 16px;" class = "mt-2"> {{data.value}} </b-badge>
                </template>

                <template  v-slot:cell(Status)="data">
                    <b :class = "data.item['Status Style']" style="font-weight: normal; font-size: 16px;"> {{data.value}} </b>
                </template>
                
            </b-table>
        </b-card>
      
    </b-card> 

</template>

<script lang="ts">
import { Component, Vue } from "vue-property-decorator";
import { namespace } from "vuex-class";
import AppearanceDetails from '@components/criminal/AppearanceDetails.vue';
import "@store/modules/CommonInformation";
const criminalState = namespace("CriminalFileInformation");
const commonState = namespace("CommonInformation");

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
    public appearanceInfo!: any;

    @criminalState.Action
    public UpdateAppearanceInfo!: (newAppearanceInfo: any) => void

    @commonState.State
    public displayName!: string;    

    @commonState.Action
    public UpdateDisplayName!: (newInputNames: any) => void

    @commonState.State
    public duration

    @commonState.Action
    public UpdateDuration!: (duration: any) => void

    @commonState.State
    public time

    @commonState.Action
    public UpdateTime!: (time: any) => void
    
    @commonState.State
    public statusStyle
    
    @commonState.Action
    public UpdateStatusStyle!: (statusStyle: any) => void

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
            fileInfo["Status Style"] = this.getStatusStyle(fileInfo["Status"])
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
        this.UpdateStatusStyle(status);
        return this.statusStyle;
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
            this.appearanceInfo.fileNo = this.criminalFileInformation.fileNumber;
            this.appearanceInfo.appearanceId = data.item["Appearance ID"]
            this.appearanceInfo.supplementalEquipmentTxt = data.item["Supplemental Equipment"]
            this.appearanceInfo.securityRestrictionTxt = data.item["Security Restriction"]
            this.appearanceInfo.outOfTownJudgeTxt = data.item["OutOfTown Judge"]
            this.UpdateAppearanceInfo(this.appearanceInfo);
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