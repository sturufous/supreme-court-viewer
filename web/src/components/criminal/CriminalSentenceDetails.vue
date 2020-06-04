<template>
   <b-card  v-if= "isMounted" no-body>
        <div>         
            <h3 class="mx-2 mt-5 font-weight-normal"> Counts ({{NumberOfCounts}}) </h3>
            <hr class="mx-2 bg-light" style="height: 5px;"/>         
        </div>      
        
        <b-card>
            <b-dropdown  variant="light text-info" :text="getNameOfParticipant(activeCriminalParticipantIndex)" class="m-0">    
                <b-dropdown-item-button  
                    v-for="participant in SortedParticipants" 
                    :key="participant['Index']"
                    v-on:click="setActiveParticipantIndex(participant['Index'])">
                        {{getNameOfParticipant(participant['Index'])}}
                </b-dropdown-item-button> 
            </b-dropdown> 
        </b-card>
        <b-card> 
        <b-table-simple small responsive borderless>
            <b-thead>                    
                <b-tr >
                    <b-th v-for="(head,index) in fields" v-bind:key="index" variant="info" :class="head.headerStyle" >                            
                        <b-icon-caret-up-fill v-if="(index<2|| index==3) && dateSortDir=='asc' && sortBy==head.key" @click="sortClick(head)"></b-icon-caret-up-fill>
                        <b-icon-caret-down-fill v-else-if="(index<2|| index==3) && dateSortDir=='desc' && sortBy==head.key" @click="sortClick(head)"></b-icon-caret-down-fill>
                        <b-icon-caret-up v-else-if="(index<2|| index==3) && sortBy!=head.key" @click="sortClick(head)"></b-icon-caret-up>
                        {{head.key}}
                    </b-th>                        
                </b-tr>
            </b-thead>

            <b-tbody v-for="(counts,inx) in SortedParticipantFilesCounts" v-bind:key="inx">
                <b-tr v-for="(sentence,index) in counts['Sentence/Disposition Type']" v-bind:key="index" :style ="getRowStyle(index)">                            
                    
                    <b-td :rowspan="counts.Len" v-if="index==0">{{counts['Date'] | beautify-date}}</b-td>                            
                    <b-td :rowspan="counts.Len" v-if="index==0">{{counts['Count']}}</b-td>
                    
                    <b-td>                                
                        <b> {{counts.ChargeIssueCd[index]}} </b> 
                        <span v-if="counts.ChargeIssueCd[index]">
                            &mdash; 
                            <b-badge
                                variant = "secondary"  
                                v-b-tooltip.hover.right 
                                :title="counts.ChargeIssueDscFull[index]"> 
                                {{counts.ChargeIssueDsc[index]}} 
                            </b-badge>
                        </span>
                    </b-td>
                    
                    <b-td :rowspan="counts.Len" v-if="index==0">
                        <b-badge
                            v-if="counts['Finding']"
                            variant = "secondary"
                            v-b-tooltip.hover.right
                            :title="counts.FindingDsc"> 
                            {{counts['Finding']}} 
                        </b-badge>
                    </b-td>

                    <b-td>
                        <b-badge
                            v-if="sentence"
                            variant = "secondary"  
                            v-b-tooltip.hover.right                             
                            :title="counts.SentenceDsc[index]"> 
                            {{sentence}} 
                        </b-badge>
                    </b-td>

                    <b-td> {{counts['Term'][index]}} </b-td>
                    <b-td> {{counts['Amount'][index]}} </b-td>
                    <b-td> {{counts['Due Date/ Until'][index] | beautify-date }} </b-td>
                    <b-td> {{counts['Effective Date'][index] | beautify-date }} </b-td>
                    
                </b-tr>                       
            </b-tbody>
        </b-table-simple>
      </b-card>       
   </b-card> 
</template>

<script lang="ts">
import { Component, Vue} from 'vue-property-decorator';
import * as _ from 'underscore';
import { namespace } from 'vuex-class';
import '@store/modules/CriminalFileInformation';
const criminalState = namespace('CriminalFileInformation');

@Component
export default class CriminalSentenceDetails extends Vue {

    @criminalState.State
    public criminalFileInformation!: any

    @criminalState.Action
    public UpdateCriminalFile!: (newCriminalFileInformation: any) => void

    @criminalState.State
    public activeCriminalParticipantIndex    

    @criminalState.Action
    public UpdateActiveCriminalParticipantIndex!: (newActiveCriminalParticipantIndex: any) => void

    public getParticipants(): void {       
        const data = this.criminalFileInformation.detailsData;
        this.participantJson = data.participant
        this.ExtractParticipantInfo()          
        this.isMounted = true;
    }

    mounted () {        
        this.getParticipants();  
    }

    participantJson;         
    sortBy = 'Date';
    dateSortDir = 'desc';
   
    isMounted = false

    participantFiles: any[] = [];

    fields = [        
        {key:'Date',                     sortable:true,   tdClass: 'border-top', headerStyle:'text-primary',  cellStyle:'text'},
        {key:'Count',                    sortable:true,   tdClass: 'border-top', headerStyle:'text-primary',  cellStyle:'text-muted'},
        {key:'Charge/Issue',             sortable:false,  tdClass: 'border-top', headerStyle:'text',          cellStyle:'text'},
        {key:'Finding',                  sortable:true,   tdClass: 'border-top', headerStyle:'text-danger',   cellStyle:'text'},        
        {key:'Sentence/ Disposition Type',sortable:false, tdClass: 'border-top', headerStyle:'text',          cellStyle:'text'},   
        {key:'Term',                     sortable:false,  tdClass: 'border-top', headerStyle:'text',          cellStyle:'text'},
        {key:'Amount',                   sortable:false,  tdClass: 'border-top', headerStyle:'text',          cellStyle:'text'},
        {key:'Due Date/ Until',          sortable:false,  tdClass: 'border-top', headerStyle:'text',          cellStyle:'text'},
        {key:'Effective Date',           sortable:false,  tdClass: 'border-top', headerStyle:'text',          cellStyle:'text'},
    ];

    public getNameOfParticipant(num)
    {        
        if(!this.participantFiles[num]["First Name"])
            return  this.participantFiles[num]["Last Name"];
        else if(!this.participantFiles[num]["Last Name"])
            return this.participantFiles[num]["First Name"];
        else
            return  this.participantFiles[num]["Last Name"]+', '+this.participantFiles[num]["First Name"];           
    }

    public setActiveParticipantIndex(index)
    {                   
        this.UpdateActiveCriminalParticipantIndex(index);  
    }	
    
    public ExtractParticipantInfo(): void {        
        
        for(const fileIndex in this.participantJson)
        {            
            const fileInfo = {};
            const jFile =  this.participantJson[fileIndex];
            fileInfo["Index"] = fileIndex; 
            fileInfo["Part ID"] = jFile.partId;
            fileInfo["First Name"] = jFile.givenNm.trim().length>0 ? jFile.givenNm : "";
            fileInfo["Last Name"] = jFile.lastNm ? jFile.lastNm : jFile.orgNm;            
            
            fileInfo["Counts"] = [];
            const counts: any[] = [];           
              
            for(const cnt of this.mergeSentences(jFile.count))           
            {                
                const countInfo = {}; 
               
                countInfo["Date"] = cnt.appearanceDate? cnt.appearanceDate.split(' ')[0] : ''; 
                                
                countInfo["Finding"]= cnt.finding? cnt.finding:'';
                countInfo["FindingDsc"]= cnt.findingDsc? cnt.findingDsc:'';
                                   
                countInfo["Count"]='';
                countInfo["ChargeIssueCd"] = [] ;
                countInfo["ChargeIssueDsc"] = [];
                countInfo["ChargeIssueDscFull"] = [];
                
                for(const charge of cnt.charge)
                { 
                    countInfo["Count"] += charge.countNum + ',';
                    countInfo["ChargeIssueCd"].push(charge.chargeTxt? charge.chargeTxt: '') ;
                    countInfo["ChargeIssueDsc"].push(charge.chargeDscTxt? (charge.chargeDscTxt.length>10 ? charge.chargeDscTxt.substr(0,10)+' ...':charge.chargeDscTxt): '');
                    countInfo["ChargeIssueDscFull"].push(charge.chargeDscTxt? charge.chargeDscTxt:'');
                }
                countInfo["Count"]= countInfo["Count"].slice(0, -1); 

                countInfo["Sentence/Disposition Type"]=[];
                countInfo["SentenceDsc"]=[];
                countInfo["Term"] = [];
                countInfo["Amount"]=[];
                countInfo["Due Date/ Until"]=[];
                countInfo["Effective Date"]=[]; 

                countInfo["LenCharge"] = cnt.charge.length;
                countInfo["Len"]= Math.max(cnt.sentence.length , countInfo["LenCharge"]); 
               
                for(const sentence of cnt.sentence)
                {                    
                   countInfo["Sentence/Disposition Type"].push(sentence.sntpCd); 
                   countInfo["SentenceDsc"].push(sentence.sentenceTypeDesc? sentence.sentenceTypeDesc:'');
                   countInfo["Term"].push(sentence.sentTermPeriodQty? (sentence.sentTermPeriodQty + ' ' + sentence.sentTermCd.replace('-','')):'')
                   countInfo["Amount"].push(sentence.sentMonetaryAmt? sentence.sentMonetaryAmt:'')
                   countInfo["Due Date/ Until"].push(sentence.sentDueTtpDt? sentence.sentDueTtpDt.split(' ')[0]:'')
                   countInfo["Effective Date"].push(sentence.sentEffectiveDt?  sentence.sentEffectiveDt.split(' ')[0]:'')                   
                }                
                
                if(cnt.sentence.length < countInfo["Len"])
                {
                    for(let loop=0; loop < (countInfo["Len"]-cnt.sentence.length)  ;loop++)
                    {                    
                        countInfo["Sentence/Disposition Type"].push(''); 
                        countInfo["SentenceDsc"].push('');
                        countInfo["Term"].push('')
                        countInfo["Amount"].push('')
                        countInfo["Due Date/ Until"].push('')
                        countInfo["Effective Date"].push('')                   
                    } 
                }
                else if(countInfo["LenCharge"] < countInfo["Len"])
                {
                    for(let loop=0; loop < (countInfo["Len"]-countInfo["LenCharge"])  ;loop++)
                    {       
                        countInfo["ChargeIssueCd"].push('');
                        countInfo["ChargeIssueDsc"].push('');
                        countInfo["ChargeIssueDscFull"].push('');
                    } 
                }
                  
                counts.push(countInfo);
            }
            fileInfo["Counts"] = counts;
                        
            this.participantFiles.push(fileInfo);
        } 
    
    }

    public mergeSentences(counts) 
    {
        const groupedCounts = _.groupBy(counts, function (count: any) {
            const orderedSentences = _.sortBy(count.sentence, "sntpCd");                
            return  count.appearanceDate +count.finding + _.pluck(orderedSentences, "sntpCd") + _.pluck(orderedSentences, "sentTermPeriodQty") + _.pluck(orderedSentences, "sentTermCd")
                + _.pluck(orderedSentences, "sentMonetaryAmt") + _.pluck(orderedSentences, "sentDueTtpDt") + _.pluck(orderedSentences, "sentEffectiveDt");
        });

        return _.map(groupedCounts, function (countGroup) {
            const mergedCount = countGroup[0];
            mergedCount.charge = _.chain(countGroup)
            .sortBy(function(stooge){ return stooge.countNumber; })
            .map(function (group) { return group.countNumber+ "|" + group.sectionTxt + "|" + group.sectionDscTxt })
            .uniq()
            .map(function (charge) { return { countNum:charge.split("|")[0], chargeTxt: charge.split("|")[1], chargeDscTxt: charge.split("|")[2] }; })
            .value();                return mergedCount;
        });
    }
    
    get SortedParticipantFilesCounts()
    {
        if(this.dateSortDir =='desc')
            return _.sortBy(this.participantFiles[this.activeCriminalParticipantIndex]['Counts'],this.sortBy).reverse()
        else
            return _.sortBy(this.participantFiles[this.activeCriminalParticipantIndex]['Counts'],this.sortBy)  
    } 

    get NumberOfCounts() { 
        return(this.participantFiles[this.activeCriminalParticipantIndex]["Counts"].length) 
    }

    get SortedParticipants()
    {         
        return _.sortBy(this.participantFiles,(participant=>{return (participant["Last Name"]? participant["Last Name"].toUpperCase() : '')}))       
    }

    public getRowStyle(index) {
        if(index==0) return "border-top : 1px solid #999;"
        return '';
    }
    
    public sortClick(data)
    {
        this.sortBy = data.key
        if(this.dateSortDir =='desc')
            this.dateSortDir ='asc';
        else
            this.dateSortDir ='desc';
    }
}
</script>

<style scoped>
 .card {
        border: white;
    }

</style>