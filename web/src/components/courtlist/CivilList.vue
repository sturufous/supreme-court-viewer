<template>

    <b-card bg-variant="white" no-body>
        <div>
            <h2 :class="'mx-4 mt-5 font-weight-normal text-'+civilClass">{{getClassName}}</h2>
            <hr :class="'bg-'+civilClass+' mx-3'" style="height: 5px;"/> 
        </div>

        <b-card v-if="!isDataReady && isMounted" no-body>
            <span class="text-muted ml-4 mb-5"> No {{getClassName}} List has been found. </span>
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
                        style="font-weight: normal; font-size: 16px; padding-top:8px" 
                        :class="data.field.cellStyle"
                        variant="white" > 
                            {{data.value}} 
                    </b-badge>
                </template>

                <template v-slot:[`cell(${fields[0].key})`]="data" >                     
                    <b-button 
                        style=" font-size:16px" 
                        size="sm" 
                        @click="OpenDetails(data); data.toggleDetails();" 
                        variant="outline-primary border-white text-info" 
                        class="mr-2">
                            <b-icon-caret-right-fill :variant="civilClass" v-if="!data.item['_showDetails']"></b-icon-caret-right-fill>
                            <b-icon-caret-down-fill :variant="civilClass" v-if="data.item['_showDetails']"></b-icon-caret-down-fill>
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
                        :title="data.item['ReasonDesc']"
                        style="margin-top: 6px; font-size: 14px;"> 
                            {{data.value}}
                    </b-badge>
                </template>

                <template  v-slot:cell(Parties)="data">
                    <b-badge
                        v-if="data.item['PartiesTruncApplied']"
                        variant="white text-success"                        
                        v-b-tooltip.hover.right                            
                        :title="data.item['PartiesDesc']"
                        style="margin-top: 5px; font-size: 16px; font-weight:normal"> 
                            {{data.value}}
                    </b-badge>
                     <b-badge
                        variant="white"
                        style="margin-top: 5px; font-size: 16px; font-weight:normal" v-else>
                            {{data.value}}
                    </b-badge>
                </template>
                <template  v-slot:cell(Counsel)="data">
                    <b-badge
                        v-if="data.item['CounselDesc']"
                        variant="white text-success"                        
                        v-b-tooltip.hover.left                           
                        :title="data.item['CounselDesc']"
                        style="margin-top: 4px; font-size: 16px; font-weight:normal"> 
                            {{data.value}}
                    </b-badge>
                    <b-badge v-else
                        variant="white"
                        style="margin-top: 4px; font-size: 16px; font-weight:normal" >
                            {{data.value}}
                    </b-badge>
                </template>
                 
            

                <template v-slot:[`cell(${fields[7].key})`]="data" >
                        <b-badge  
                            v-for="(field,index) in data.value"
                            :key="index" 
                            class="mr-1"
                            style="margin-top: 6px; font-weight: normal; font-size: 14px;"
                            v-b-tooltip.hover 
                            :title='field.key' > 
                            {{ field.abbr }} 
                        </b-badge>
                </template>

            </b-table>
        </b-card>
      
    </b-card> 

</template>

<script lang="ts">
import { Component, Vue, Prop } from "vue-property-decorator";
import { namespace } from "vuex-class";
import CivilAppearanceDetails from '@components/civil/CivilAppearanceDetails.vue';

import "@store/modules/CommonInformation";
const commonState = namespace("CommonInformation");
import '@store/modules/CourtListInformation';
const courtListState = namespace('CourtListInformation');
import "@store/modules/CivilFileInformation";
const civilState = namespace("CivilFileInformation");

enum HearingType {'A'= '+','G' = '@','D'='-', 'S' = '*'  }

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

    @Prop() civilClass

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
        const listClass = this.civilClass=='family'? 'F': 'I';
        for (const civilListIndex in this.civilCourtListJson) 
        {
            const civilListInfo = {};
            const jcivilList = this.civilCourtListJson[civilListIndex];

            civilListInfo["Index"] = civilListIndex;
            if(jcivilList.activityClassCd != listClass) continue;

            civilListInfo['File Number']=jcivilList.physicalFile.fileNumber           
            civilListInfo["Time"] = this.getTime(jcivilList.appearanceTime.substr(0,5));

            civilListInfo["Room"] = this.courtRoom
            const partyNames = this.getNameOfPartyTrunc(jcivilList.sealFileSOCText)
            civilListInfo["Parties"] = partyNames.name
            civilListInfo["PartiesTruncApplied"] = partyNames.trunc
            civilListInfo["PartiesDesc"] = jcivilList.sealFileSOCText


            civilListInfo['Reason'] = jcivilList.appearanceReasonCd             
            civilListInfo['ReasonDesc'] = jcivilList.appearanceReasonDesc
            civilListInfo['Est.'] = this.getDuration(jcivilList.estimatedTimeHour, jcivilList.estimatedTimeMin)
            
            civilListInfo['Counsel'] = ''          
            civilListInfo['CounselDesc'] =''

            let firstCounselSet=false
            for (const party of jcivilList.parties)
            {
                //console.log(party)
                for(const counsel of party.counsel)
                {                    
                    if(!firstCounselSet)
                    {
                        civilListInfo['Counsel'] = counsel.counselFullName;
                        firstCounselSet = true;
                    }
                    else
                    {
                        civilListInfo['CounselDesc'] += counsel.counselFullName +', ';
                    }                    
                }
            }
            if(civilListInfo['CounselDesc']) civilListInfo['CounselDesc'] += civilListInfo['Counsel'];

            civilListInfo['FileID'] = jcivilList.physicalFile.physicalFileID
            civilListInfo['AppearanceID'] = jcivilList.appearanceId

            civilListInfo['File Markers'] = [];
            for (const hearingRestriction of jcivilList.hearingRestriction)
            {
                const marker =  hearingRestriction.adjInitialsText +  HearingType[hearingRestriction.hearingRestrictiontype]  
                const markerDesc =  hearingRestriction.judgeName + ' ('+ hearingRestriction.hearingRestrictionTypeDesc+')' 
                
                //console.log(markerDesc)
                civilListInfo['File Markers'].push({abbr:marker, key:markerDesc});
            }
            //console.log(civilListInfo['File Markers'])
                       
            this.civilList.push(civilListInfo);
            //console.log(civilListInfo)
        }
    }

    public getNameOfPartyTrunc(partyNames) 
    {
        const maximumFullNameLength=15
        let truncApplied = false 
        if (partyNames) {
            let firstParty = partyNames.split('/')[0].trim()
            let secondParty = partyNames.split('/')[1].trim()

            if(firstParty.length > maximumFullNameLength)
            { 
                firstParty = firstParty.substr(0, maximumFullNameLength) +' ...';
                truncApplied = true;
            }

            if(secondParty.length > maximumFullNameLength)
            { 
                secondParty = secondParty.substr(0, maximumFullNameLength) +' ...';
                truncApplied = true;
            } 
            
                return  {name: firstParty+' / '+secondParty, trunc:truncApplied};
        } else {
            return {name: '', trunc:truncApplied};
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

    get getClassName()
    {
        if (this.civilClass=='family') 
            return 'Family';
        else
            return 'Civil';
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