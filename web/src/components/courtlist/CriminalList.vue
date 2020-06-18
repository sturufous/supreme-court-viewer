<template>

    <b-card bg-variant="white" no-body>
        <div>
            <h2 class="mx-4 mt-5 font-weight-normal text-criminal ">Criminal</h2>
            <hr class="mx-3 bg-criminal" style="height: 5px;"/> 
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

        <b-card bg-variant="white" v-if="isDataReady" no-body class="mx-3" style="overflow:auto">           
            <b-table
            :items="SortedCriminalList"
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
                        style="font-weight: normal; font-size: 16px; padding-top:8px" 
                        :class="data.field.cellStyle"
                        variant="white" > 
                            {{data.value}} 
                    </b-badge>
                </template>

                <template v-slot:[`cell(${fields[1].key})`]="data" >                     
                    <b-button  
                        size="sm" 
                        style=" font-size:16px"
                        @click="OpenDetails(data); data.toggleDetails();" 
                        variant="outline-primary border-white text-criminal" 
                        class="mr-2">
                            <b-icon-caret-right-fill variant="criminal" v-if="!data.item['_showDetails']"></b-icon-caret-right-fill>
                            <b-icon-caret-down-fill variant="criminal" v-if="data.item['_showDetails']"></b-icon-caret-down-fill>
                            {{data.value}}
                    </b-button>                   
                </template>

                <template v-slot:row-details>
                    <b-card no-body bg-border="dark"> 
                        <criminal-appearance-details/>
                    </b-card>
                </template> 
                
                <template  v-slot:cell(Accused)="data">
                    <b-button
                        style=" font-size:16px" 
                        size="sm" 
                        @click="OpenCriminalFilePage(data)"                         
                        variant="outline-primary border-white text-criminal" 
                        class="mr-2">                            
                            {{data.value}}
                    </b-button>
                </template>

                <template  v-slot:cell(Reason)="data">
                    <b-badge
                        variant="secondary"
                        v-b-tooltip.hover.right                            
                        :title="data.item['ReasonDesc']"
                        style="margin-top: 6px; font-size: 14px;"> 
                            {{data.value}}
                    </b-badge>
                </template>
                
                <template  v-slot:cell(Crown)="data">
                    <b-badge
                        v-if="data.item['CrownDesc']"
                        variant="white text-success"                        
                        v-b-tooltip.hover.left                           
                        :title="data.item['CrownDesc']"
                        style="margin-top: 4px; font-size: 16px; font-weight:normal"> 
                            {{data.value}}
                    </b-badge>
                    <b-badge v-else
                        variant="white"
                        style="margin-top: 4px; font-size: 16px; font-weight:normal">
                            {{data.value}}
                    </b-badge>
                </template>
                

                <template v-slot:[`cell(${fields[8].key})`]="data" >
                        <b-badge  
                            v-for="(field,index) in data.value"
                            :key="index" 
                            class="mr-1"
                            style="font-weight: normal;margin-top: 6px; font-size: 14px;"
                            v-b-tooltip.hover 
                            :title='field.key' > 
                            {{ field.abbr }} 
                        </b-badge>
                </template>

                <template v-slot:[`cell(${fields[10].key})`]="data" >                     
                    <b-badge variant="white" style="margin-top: 5px; font-weight: normal;font-size:16px">{{data.value}}
                    <span class="text-muted" style="font-weight: normal; font-size:14px">d</span>  </b-badge>                
                </template>
            
                
            </b-table>
        </b-card>
      
    </b-card> 

</template>

<script lang="ts">
import { Component, Vue } from "vue-property-decorator";
import { namespace } from "vuex-class";
import CriminalAppearanceDetails from '@components/criminal/CriminalAppearanceDetails.vue';
import * as _ from 'underscore';

import "@store/modules/CommonInformation";
const commonState = namespace("CommonInformation");
import '@store/modules/CourtListInformation';
const courtListState = namespace('CourtListInformation');
import "@store/modules/CriminalFileInformation";
const criminalState = namespace("CriminalFileInformation");

enum HearingType {'A'= '+','G' = '@','D'='-', 'S' = '*'  }

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

    @criminalState.Action
    public UpdateCriminalFile!: (newCriminalFileInformation: any) => void

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

        console.log(this.criminalCourtListJson)
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
        {key:'Seq.',        tdClass: 'border-top', headerStyle:'', cellStyle:''},
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

            criminalListInfo['Seq.']=jcriminalList.appearanceSequenceNumber
            criminalListInfo['File Number']=jcriminalList.fileNumberText
            criminalListInfo['Case Age']= jcriminalList.caseAgeDaysNumber? jcriminalList.caseAgeDaysNumber: ''
            criminalListInfo["Time"] = this.getTime(jcriminalList.appearanceTime.split(' ')[1].substr(0,5));

            criminalListInfo["Room"] = this.courtRoom
            criminalListInfo["Accused"] = jcriminalList.accusedFullName
            criminalListInfo['Reason'] = jcriminalList.appearanceReasonCd
            criminalListInfo['ReasonDesc'] = jcriminalList.appearanceReasonDesc

            criminalListInfo['Counsel'] = jcriminalList.counselFullName

            criminalListInfo['Crown']= ''
            criminalListInfo['CrownDesc']= ''            
            if(jcriminalList.crown.length>0)
            {
                //console.log(jcriminalList.crown)
                let firstCrownSet=false
                for(const crown of jcriminalList.crown)
                {
                    if(crown.assigned)  
                    { 
                        if(!firstCrownSet)
                        {
                            criminalListInfo['Crown'] = crown.fullName;
                            firstCrownSet = true;
                        }
                        else
                        {
                            criminalListInfo['CrownDesc'] += crown.fullName +', ';
                        }
                    }
                
                    //console.log(crown.fullName)
                    //console.log(crown.assigned);
                }

                if(criminalListInfo['CrownDesc']) criminalListInfo['CrownDesc'] += criminalListInfo['Crown'];
            }
            criminalListInfo['Est.'] = this.getDuration(jcriminalList.estimatedTimeHour, jcriminalList.estimatedTimeMin)

            criminalListInfo['PartID'] =  jcriminalList.fileInformation.partId
            criminalListInfo['JustinNo'] = jcriminalList.fileInformation.mdocJustinNo
            criminalListInfo['AppearanceID'] = jcriminalList.criminalAppearanceID


            criminalListInfo['File Markers'] = [];
            for (const hearingRestriction of jcriminalList.hearingRestriction)
            {
                const marker =  hearingRestriction.adjInitialsText +  HearingType[hearingRestriction.hearingRestrictiontype]  
                const markerDesc =  hearingRestriction.judgeName + ' ('+ hearingRestriction.hearingRestrictionTypeDesc+')'          
                criminalListInfo['File Markers'].push({abbr:marker, key:markerDesc});
                //console.log(markerDesc)
                //console.log(hearingRestriction.hearingRestrictiontype)
            }
            //console.log(criminalListInfo['File Markers'])
            criminalListInfo["Supplemental Equipment"] = jcriminalList.supplementalEquipment
            criminalListInfo["Security Restriction"] = jcriminalList.securityRestriction
            criminalListInfo["OutOfTown Judge"] = jcriminalList.outOfTownJudge

            criminalListInfo["Court Level"] = jcriminalList.fileInformation.courtLevelCd
            criminalListInfo["Court Class"] = jcriminalList.fileInformation.courtClassCd
            criminalListInfo["Prof SeqNo"] = jcriminalList.fileInformation.profSeqNo
                       
            this.criminalList.push(criminalListInfo); 
            //console.log(criminalListInfo)
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
            this.appearanceInfo.supplementalEquipmentTxt = data.item["Supplemental Equipment"]
            this.appearanceInfo.securityRestrictionTxt = data.item["Security Restriction"]
            this.appearanceInfo.outOfTownJudgeTxt = data.item["OutOfTown Judge"]

            this.appearanceInfo.courtLevel = data.item['Court Level']
            this.appearanceInfo.courtClass = data.item['Court Class']
            this.appearanceInfo.profSeqNo = data.item['Prof SeqNo']
            this.UpdateAppearanceInfo(this.appearanceInfo);
        }        
    }

    public OpenCriminalFilePage(data)
    {
        const fileInformation = { }
        fileInformation['fileNumber'] = data.item['JustinNo']
        this.UpdateCriminalFile(fileInformation)
        const routeData = this.$router.resolve({name:'CriminalCaseDetails', params: {fileNumber: fileInformation['fileNumber']}})
        window.open(routeData.href, '_blank');
    }

    get SortedCriminalList()
    {                
        return  _.sortBy(this.criminalList, 'Seq.')
    }
}
</script>

<style scoped>
 .card {
        border: white;
    }
</style>