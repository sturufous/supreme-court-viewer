<template>
<b-card bg-variant="white" no-body>
    <div>
        <h3 class="mx-4 font-weight-normal" v-if="!showSections['Past Appearances']"> Last Three Past Appearances</h3>
        <hr class="mx-3 bg-light" style="height: 5px;"/> 
    </div>

    <b-card v-if="!isDataReady && isMounted">
        <span class="text-muted ml-4 mb-5"> No past appearances. </span>
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

    <b-card bg-variant="white" v-if="isDataReady" style="overflow: auto;" no-body class="mx-3 mb-5">           
        <b-table
        :items="SortedPastAppearances"
        :fields="fields"
        :sort-by.sync="sortBy"
        :sort-desc.sync="sortDesc"
        :no-sort-reset="true"
        sort-icon-left
        borderless
        @sort-changed="sortChanged"
        small
        responsive="sm"
        >   
            <template v-for="(field,index) in fields" v-slot:[`head(${field.key})`]="data">
                <b v-bind:key="index" :class="field.headerStyle" > {{ data.label }}</b>
            </template>

            <template  v-slot:cell()="data">
                <b-badge                        
                    :style="data.field.cellStyle" 
                    variant="white" > 
                        {{data.value}} 
                </b-badge>
            </template>

            <template v-slot:cell(Date)="data" >
                <span :class="data.field.cellClass" :style="data.field.cellStyle"> 
                    <b-button style="transform: translate(-2px,-7px); font-size:14px;" size="sm" @click="OpenDetails(data);data.toggleDetails();" variant="outline-primary border-white  text-info" class="mr-2">
                        <b-icon-caret-right-fill v-if="!data.item['_showDetails']"></b-icon-caret-right-fill>
                        <b-icon-caret-down-fill v-if="data.item['_showDetails']"></b-icon-caret-down-fill>
                        {{data.item.FormattedDate}}
                    </b-button>
                </span> 
            </template>
            <template v-slot:row-details>
                <civil-appearance-details/>
            </template>

            <template  v-slot:cell(Reason)="data">
                <b-badge
                        :class="data.field.cellClass"
                        variant="secondary"
                        v-b-tooltip.hover.right                            
                        :title="data.item['Reason Description']"
                        :style="data.field.cellStyle">  
                        {{data.value}}
                </b-badge>
            </template>

            <template v-slot:cell(Result)="data" >
                <span
                        v-if="data.value"
                        :class="data.field.cellClass"
                        variant="outline-primary border-white"
                        v-b-tooltip.hover.right                            
                        :title="data.item['Result Description']"
                        :style="data.field.cellStyle"> 
                        {{data.value}}
                </span>
            </template>

            <template v-slot:cell(Presider)="data">
                <b-badge                              
                        variant="secondary"
                        v-if="data.value"
                        :class="data.field.cellClass"
                        :style="data.field.cellStyle"
                        v-b-tooltip.hover.left                           
                        :title="data.item['Judge Full Name']"> 
                        {{data.value}}
                </b-badge>
            </template>                

            <template v-slot:cell(Status)="data">
                <b :class = "data.item['Status Style']" :style="data.field.cellStyle"> {{data.value}} </b>
            </template>
            
        </b-table>
    </b-card>
</b-card>
</template>

<script lang="ts">
import { Component, Vue } from "vue-property-decorator";
import { namespace } from "vuex-class";
import * as _ from 'underscore';
import CivilAppearanceDetails from '@components/civil/CivilAppearanceDetails.vue';
import "@store/modules/CommonInformation";
import "@store/modules/CivilFileInformation";
import {civilFileInformationType, civilAppearanceInfoType, civilAppearancesListType} from '../../types/civil';
import {inputNamesType, durationType } from '../../types/common'
const civilState = namespace("CivilFileInformation");
const commonState = namespace("CommonInformation");

enum appearanceStatus {UNCF='Unconfirmed', CNCL='Canceled', SCHD='Scheduled' }

@Component({
    components: {
        CivilAppearanceDetails
    }
})
export default class CivilPastAppearances extends Vue {    

    @civilState.State
    public showSections

     @commonState.State
    public statusStyle 

    @commonState.State
    public displayName!: string;    

    @commonState.State
    public duration    

    @commonState.State
    public time

    @civilState.State
    public appearanceInfo!: civilAppearanceInfoType;

    @civilState.State
    public civilFileInformation!: civilFileInformationType;

    @civilState.Action
    public UpdateAppearanceInfo!: (newAppearanceInfo: civilAppearanceInfoType) => void    

    @commonState.Action
    public UpdateTime!: (time: string) => void

     @commonState.Action
    public UpdateDisplayName!: (newInputNames: inputNamesType) => void
    
    @commonState.Action
    public UpdateDuration!: (duration: durationType) => void   
    
    @commonState.Action
    public UpdateStatusStyle!: (statusStyle: string) => void

    pastAppearancesList: civilAppearancesListType[] = [];

    isMounted = false;
    isDataReady = false;
    pastAppearancesJson;    
    sortBy = 'Date';
    sortDesc = true;    

    fields =  
    [
        {key:'Date',            sortable:true,  tdClass: 'border-top', headerStyle:'text-primary',  cellClass:'text-info mt-2 d-inline-flex', cellStyle: 'display: inline-flex; font-size: 14px;'},
        {key:'Reason',          sortable:true,  tdClass: 'border-top', headerStyle:'text-primary',  cellClass:'badge badge-secondary mt-2',   cellStyle: 'font-size: 14px;'},
        {key:'Document Type',   sortable:false, tdClass: 'border-top', headerStyle:'text',          cellClass:'text',                         cellStyle: 'font-weight: normal;font-size: 14px; padding-top:12px'},
        {key:'Result',          sortable:true,  tdClass: 'border-top', headerStyle:'text-primary',  cellClass:'badge badge-secondary mt-2',   cellStyle: 'font-size: 14px;'},
        {key:'Time',            sortable:false, tdClass: 'border-top', headerStyle:'text',          cellClass:'text',                         cellStyle: 'font-weight: normal; font-size: 14px; padding-top:12px'},
        {key:'Duration',        sortable:false, tdClass: 'border-top', headerStyle:'text',          cellClass:'text',                         cellStyle: 'font-weight: normal; font-size: 14px; padding-top:12px'},
        {key:'Location',        sortable:true,  tdClass: 'border-top', headerStyle:'text-primary',  cellClass:'text',                         cellStyle: 'font-weight: normal; font-size: 14px; padding-top:12px'},
        {key:'Room',            sortable:false, tdClass: 'border-top', headerStyle:'text',          cellClass:'text',                         cellStyle: 'font-weight: normal; font-size: 14px; padding-top:12px'},
        {key:'Presider',        sortable:true,  tdClass: 'border-top', headerStyle:'text-primary',  cellClass:'badge badge-secondary mt-2',   cellStyle: 'font-size: 14px;'},        
        {key:'Status',          sortable:true,  tdClass: 'border-top', headerStyle:'text-primary',  cellClass:'badge',                        cellStyle: 'font-size: 14px; width:110px;'}
    ];
    
    mounted() {
        this.getPastAppearances();
    }

    public getPastAppearances(): void {      
    
        const data = this.civilFileInformation.detailsData;
        this.pastAppearancesJson = data.appearances.apprDetail;              
        this.ExtractPastAppearancesInfo();
        if(this.pastAppearancesList.length) {                    
            this.isDataReady = true;
        }
    
        this.isMounted = true;           
    }
  
    public ExtractPastAppearancesInfo(): void {
        const currentDate = new Date();
        for (const appIndex in this.pastAppearancesJson) {
            const appInfo = {} as civilAppearancesListType;
            const jApp = this.pastAppearancesJson[appIndex];

            appInfo["Index"] = appIndex;
            appInfo["Date"] = jApp.appearanceDt.split(' ')[0]
            if(new Date(appInfo["Date"]) >= currentDate) continue;            
            appInfo["FormattedDate"] = Vue.filter('beautify-date')(appInfo["Date"]);
            appInfo["Document Type"] = jApp.documentTypeDsc;
            appInfo["Result"] = jApp.appearanceResultCd;
            appInfo["Result Description"] = jApp.appearanceResultDsc? jApp.appearanceResultDsc: '';
            appInfo["Time"] = this.getTime(jApp.appearanceTm.split(' ')[1].substr(0,5));
            appInfo["Reason"] = jApp.appearanceReasonCd;
            appInfo["Reason Description"] = jApp.appearanceReasonDsc? jApp.appearanceReasonDsc: '';
            appInfo["Duration"] = this.getDuration(jApp.estimatedTimeHour, jApp.estimatedTimeMin)           
            appInfo["Location"] = jApp.courtLocation;
            appInfo["Room"] =jApp.courtRoomCd              
            appInfo["Status"] = jApp.appearanceStatusCd ? appearanceStatus[jApp.appearanceStatusCd] :''
            appInfo["Status Style"] = this.getStatusStyle(appInfo["Status"])
            appInfo["Presider"] =  jApp.judgeInitials ? jApp.judgeInitials :''
            appInfo["Judge Full Name"] =  jApp.judgeInitials ? jApp.judgeFullNm : ''

            appInfo["Appearance ID"] = jApp.appearanceId
            appInfo["Supplemental Equipment"] = jApp.supplementalEquipmentTxt
            appInfo["Security Restriction"] = jApp.securityRestrictionTxt
            appInfo["OutOfTown Judge"] = jApp.outOfTownJudgeTxt
                       
            this.pastAppearancesList.push(appInfo); 
        }
    }

    public getStatusStyle(status) {
        this.UpdateStatusStyle(status);
        return this.statusStyle;
    }

    public getTime(time) {
        this.UpdateTime(time);
        return this.time;     
    }

    public getDuration(hr, min) {
        this.UpdateDuration({'hr': hr, 'min': min});
        return this.duration;
    }

    public OpenDetails(data)
    {
        if(!data.detailsShowing)
        {
            this.appearanceInfo.fileNo = this.civilFileInformation.fileNumber; 
            
            this.appearanceInfo.appearanceId = data.item["Appearance ID"]
            this.appearanceInfo.supplementalEquipmentTxt = data.item["Supplemental Equipment"]
            this.appearanceInfo.securityRestrictionTxt = data.item["Security Restriction"]
            this.appearanceInfo.outOfTownJudgeTxt = data.item["OutOfTown Judge"]

            this.UpdateAppearanceInfo(this.appearanceInfo);
        }
        
    }

    public sortChanged() 
    {
        this.SortedPastAppearances.forEach((item) => {
            this.$set(item, '_showDetails', false)
        })
    }

    get SortedPastAppearances()
    {           
        if(this.showSections['Past Appearances'])
        {
            return this.pastAppearancesList;
        }
        else
        {
            return _.sortBy(this.pastAppearancesList,"Date").reverse().slice(0, 3);           
        }     
    }
}
</script>

<style scoped>
 .card {
        border: white;
    }
</style>