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
                        :id="'criminalcase-'+data.item.Tag"
                        :href="'#criminalcase-'+data.item.Tag"
                        @click="OpenDetails(data); data.toggleDetails();" 
                        variant="outline-primary border-white text-criminal" 
                        class="mr-2">
                            <b-icon-caret-right-fill variant="criminal" v-if="!data.item['_showDetails']"></b-icon-caret-right-fill>
                            <b-icon-caret-down-fill variant="criminal" v-if="data.item['_showDetails']"></b-icon-caret-down-fill>
                            {{data.value}}
                    </b-button>                   
                </template>

                <template v-slot:row-details="data">
                    <b-card no-body bg-border="dark"> 
                        <criminal-appearance-details :tagcasename="'criminalcase-'+data.item.Tag"/>
                    </b-card>
                </template>

                <template v-slot:cell(Icons)="data" >
                    <b-badge
                        variant="white border-white outline-white"
                        class="mr-1 mt-1" 
                        v-for="(field,index) in data.value"
                        :key="index"
                        v-b-tooltip.hover.top 
                        :title='field.desc'>
                        <b-icon                            
                            :icon="field.icon"
                            font-scale="1.25">                            
                        </b-icon>
                    </b-badge>    
                </template> 
                
                <template  v-slot:cell(Accused)="data">
                    <b-button
                        style="font-size:16px; font-weight: bold;" 
                        size="sm" 
                        @click="OpenCriminalFilePage(data)" 
                        v-b-tooltip.hover.right
                        :title="data.item['AccusedTruncApplied']?data.item['AccusedDesc']:null"
                        variant="outline-primary border-white text-criminal" 
                        class="mr-2">                            
                            {{data.value}}
                    </b-button>
                </template>

                <template  v-slot:cell(Reason)="data">
                    <b-badge
                        v-if="data.item['Reason']"
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
                

                <template v-slot:[`cell(${fields[9].key})`]="data" >
                        <b-badge  
                            v-for="(field,index) in data.value"
                            :key="index" 
                            class="mr-1"
                            style="font-weight: normal;margin-top: 6px; font-size: 14px;"
                            v-b-tooltip.hover.right 
                            :title='field.key' > 
                            {{ field.abbr }} 
                        </b-badge>
                </template>

                <template v-slot:[`cell(${fields[10].key})`]="data" >
                        <b-badge  
                            v-for="(field,index) in data.value"
                            :key="index" 
                            class="mr-1"
                            style="font-weight: normal;margin-top: 6px; font-size: 14px;"
                            v-b-tooltip.hover.right 
                            :title='field.key' > 
                            {{ field.abbr }} 
                        </b-badge>
                </template>

                <template v-slot:[`cell(${fields[12].key})`]="data" >                     
                    <b-badge variant="white" style="margin-top: 5px; font-weight: normal;font-size:16px">{{data.value}}
                    <span class="text-muted" style="font-weight: normal; font-size:14px">d</span>  </b-badge>                
                </template>

                <template  v-slot:cell(Notes)="data">
                    <b-button
                        v-if="data.item['NoteExist']"
                        size="sm"
                        style=" font-size:12px; border:0px;"
                        @click="OpenNotes(data.value)"                        
                        variant="outline-primary border-white text-info" 
                        class="mt-1"
                        v-b-tooltip.hover.left
                        title="Notes">                            
                            <b-icon icon="chat-square-fill" font-scale="1.5"></b-icon>
                    </b-button>
                </template>            
                
            </b-table>
        </b-card>
        <b-modal v-if= "isMounted" v-model="showNotes" id="bv-modal-notes" hide-footer>
            <template v-slot:modal-title>
                    <h2 class="mb-0">Notes</h2>
            </template>
            <b-card 
                v-if="notes.text" 
                title="Trial Notes" 
                border-variant="white">
                    {{notes.text}}
            </b-card>
            <b-card 
                v-if="notes.remarks.length>0" 
                title="Crown Notes to JCM" 
                border-variant="white">
                    <b-table        
                        borderless
                        :items="notes.remarks"                                    
                        thead-class="d-none"
                        responsive="sm"          
                        striped
                        >
                    </b-table>
               
            </b-card>             
            <b-button class="mt-3 bg-info" @click="$bvModal.hide('bv-modal-notes')">Close</b-button>
        </b-modal> 
      
    </b-card> 

</template>

<script lang="ts">
import { Component, Vue } from "vue-property-decorator";
import { namespace } from "vuex-class";
import * as _ from 'underscore';
import CriminalAppearanceDetails from '@components/criminal/CriminalAppearanceDetails.vue';
import {courtListInformationInfoType, criminalListInfoType} from '../../types/courtlist';
import {criminalFileInformationType, criminalAppearanceInfoType} from '../../types/criminal';
import {inputNamesType, durationType, iconInfoType, iconStyleType } from '../../types/common'
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
    public courtListInformation!: courtListInformationInfoType

    @criminalState.State
    public appearanceInfo!: criminalAppearanceInfoType;

    @criminalState.Action
    public UpdateAppearanceInfo!: (newAppearanceInfo: criminalAppearanceInfoType) => void       

    @criminalState.Action
    public UpdateCriminalFile!: (newCriminalFileInformation: criminalFileInformationType) => void

    @commonState.State
    public iconStyles!: iconStyleType[];
    
    @commonState.State
    public displayName!: string;    

    @commonState.State
    public duration

    @commonState.State
    public time

    @commonState.Action
    public UpdateIconStyle!: (newIconsInfo: iconInfoType[]) => void

    @commonState.Action
    public UpdateDisplayName!: (newInputNames: inputNamesType) => void

    @commonState.Action
    public UpdateDuration!: (duration: durationType) => void

    @commonState.Action
    public UpdateTime!: (time: string) => void
   
    criminalList: criminalListInfoType[] = [];    
    criminalCourtListJson;
    courtRoom;
    isMounted = false;
    isDataReady = false;
    showNotes = false;
    notes = {remarks:[], text:''};
    
    fields =  
    [
        {key:'Seq.',                tdClass: 'border-top', headerStyle:'', cellStyle:''},
        {key:'File Number',         tdClass: 'border-top', headerStyle:'', cellStyle:''},
        {key:'Icons',               tdClass: 'border-top', thClass:'text-white', cellStyle:''},
        {key:'Accused',             tdClass: 'border-top', headerStyle:'', cellStyle:'text-primary'},
        {key:'Time',                tdClass: 'border-top', headerStyle:'', cellStyle:''},
        {key:'Est.',                tdClass: 'border-top', headerStyle:'', cellStyle:''},
        {key:'Reason',              tdClass: 'border-top', headerStyle:'', cellStyle:''},
        {key:'Room',                tdClass: 'border-top', headerStyle:'', cellStyle:'text-primary'},
        {key:'Counsel',             tdClass: 'border-top', headerStyle:'', cellStyle:''},
        {key:'File Markers',        tdClass: 'border-top', headerStyle:'', cellStyle:''},
        {key:'Hearing Restrictions',tdClass: 'border-top', headerStyle:'', cellStyle:''},
        {key:'Crown',               tdClass: 'border-top', headerStyle:'', cellStyle:''},
        {key:'Case Age',            tdClass: 'border-top', headerStyle:'', cellStyle:''},
        {key:'Notes',               tdClass: 'border-top', headerStyle:'', cellStyle:''},
    ];

     mounted() {
        this.getCriminalList();       
    }

    public getCriminalList(): void 
    {            
        const data = this.courtListInformation.detailsData;       
        this.criminalCourtListJson = data.criminalCourtList
        this.courtRoom = data.courtRoomCode    
        this.ExtractCriminalListInfo()
        if(this.criminalList.length)
        {                    
            this.isDataReady = true;
        }
    
        this.isMounted = true;
    }
  
    public ExtractCriminalListInfo(): void {

        for (const criminalListIndex in this.criminalCourtListJson) 
        {
            const criminalListInfo = {} as criminalListInfoType;
            const jcriminalList = this.criminalCourtListJson[criminalListIndex];

            criminalListInfo["Index"] = criminalListIndex;

            criminalListInfo['Seq.'] = jcriminalList.appearanceSequenceNumber?parseInt(jcriminalList.appearanceSequenceNumber):0
            criminalListInfo['File Number'] = jcriminalList.fileNumberText
            criminalListInfo['Tag'] = criminalListInfo['File Number']+'-'+criminalListInfo['Seq.'];  

            criminalListInfo['Icons'] = [];
            const iconInfo: iconInfoType[] = []
            let iconExists = false;
            if (jcriminalList.appearanceStatusCd){
                iconInfo.push({"info": jcriminalList.appearanceStatusCd, "desc": ''})
                iconExists = true;
            }              
            if (jcriminalList.video){
                iconInfo.push({"info": "Video", "desc": ''})
                iconExists = true;
            }
            if (jcriminalList.fileHomeLocationName){
                iconInfo.push({"info": "Home", "desc": jcriminalList.fileHomeLocationName})
                iconExists = true;
            }            
            if (iconExists){
                this.UpdateIconStyle(iconInfo);
                criminalListInfo["Icons"] = this.iconStyles
            }
            criminalListInfo['Case Age']= jcriminalList.caseAgeDaysNumber? jcriminalList.caseAgeDaysNumber: ''
            criminalListInfo["Time"] = this.getTime(jcriminalList.appearanceTime.split(' ')[1].substr(0,5));

            criminalListInfo["Room"] = this.courtRoom

            const accusedName = this.getNameOfAccusedTrunc(jcriminalList.accusedFullName)
            criminalListInfo["Accused"] = accusedName.name;            
            criminalListInfo["AccusedTruncApplied"] = accusedName.trunc
            criminalListInfo["AccusedDesc"] = jcriminalList.accusedFullName

            criminalListInfo['Reason'] = jcriminalList.appearanceReasonCd
            criminalListInfo['ReasonDesc'] = jcriminalList.appearanceReasonDesc

            criminalListInfo['Counsel'] = jcriminalList.counselFullName

            criminalListInfo['Crown']= ''
            criminalListInfo['CrownDesc']= ''            
            if(jcriminalList.crown && jcriminalList.crown.length>0)
            {               
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
                }

                if(criminalListInfo['CrownDesc']) criminalListInfo['CrownDesc'] += criminalListInfo['Crown'];
            }
            criminalListInfo['Est.'] = this.getDuration(jcriminalList.estimatedTimeHour, jcriminalList.estimatedTimeMin)
            criminalListInfo['PartID'] =  jcriminalList.fileInformation.partId
            criminalListInfo['JustinNo'] = jcriminalList.fileInformation.mdocJustinNo
            criminalListInfo['AppearanceID'] = jcriminalList.criminalAppearanceID

            criminalListInfo['File Markers'] = [];
            if (jcriminalList.inCustody){
                criminalListInfo['File Markers'].push({abbr: 'IC', key: 'In Custody'})
            }            
            if (jcriminalList.otherFileInformationText) {
                criminalListInfo['File Markers'].push({abbr: 'OTH', key: jcriminalList.otherFileInformationText})
            }
            if (jcriminalList.detained) {
                criminalListInfo['File Markers'].push({abbr: 'DO', key: 'Detention Order'})
            }

            criminalListInfo['Hearing Restrictions'] = [];
            for (const hearingRestriction of jcriminalList.hearingRestriction)
            {
                const marker =  hearingRestriction.adjInitialsText +  HearingType[hearingRestriction.hearingRestrictiontype]  
                const markerDesc =  hearingRestriction.judgeName + ' ('+ hearingRestriction.hearingRestrictionTypeDesc+')'          
                criminalListInfo['Hearing Restrictions'].push({abbr:marker, key:markerDesc});
            }
            criminalListInfo['TrialNotes'] = jcriminalList.trialRemarkTxt;

            criminalListInfo['TrialRemarks'] = [];
            for (const trialRemark of jcriminalList.trialRemark)
            {
                criminalListInfo['TrialRemarks'].push({txt:trialRemark.commentTxt})
            }
            criminalListInfo["Notes"]={remarks:criminalListInfo['TrialRemarks'], text:criminalListInfo['TrialNotes']}            
            criminalListInfo["Supplemental Equipment"] = jcriminalList.supplementalEquipment
            criminalListInfo["Security Restriction"] = jcriminalList.securityRestriction
            criminalListInfo["OutOfTown Judge"] = jcriminalList.outOfTownJudge

            criminalListInfo["Court Level"] = jcriminalList.fileInformation.courtLevelCd
            criminalListInfo["Court Class"] = jcriminalList.fileInformation.courtClassCd
            criminalListInfo["Prof SeqNo"] = jcriminalList.fileInformation.profSeqNo
                       
            criminalListInfo["NoteExist"] = this.isNoteAvailable(criminalListInfo);
            this.criminalList.push(criminalListInfo); 
        }
    }

    public isNoteAvailable(criminalListInfo)
    {
        if(criminalListInfo['TrialRemarks'].length>0) return true;
        if(criminalListInfo['TrialNotes']) return true;
        return false;
    }
    public OpenNotes(notesData) {
        this.notes = notesData;
        this.showNotes=true;           
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
        const fileInformation = { } as criminalFileInformationType
        fileInformation['fileNumber'] = data.item['JustinNo']
        this.UpdateCriminalFile(fileInformation)
        const routeData = this.$router.resolve({name:'CriminalCaseDetails', params: {fileNumber: fileInformation['fileNumber']}})
        window.open(routeData.href, '_blank');
    }

    public getNameOfAccusedTrunc(nameOfAccused) 
    {
        const maximumFullNameLength = 20
        if(nameOfAccused.length > maximumFullNameLength)   
            return {name: nameOfAccused.substr(0, maximumFullNameLength) +' ... ', trunc:true};   
        else 
            return  {name: nameOfAccused, trunc:false};        
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