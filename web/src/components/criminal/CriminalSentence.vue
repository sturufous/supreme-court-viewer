<template>

    <b-card bg-variant="white" v-if="isMounted">
        <div>            
            <hr class="mb-3 bg-light" style="height: 5px;"/> 
        </div>

        <b-card bg-variant="white" no-body>           
            <b-table
            :items="SortedParticipants"
            :fields="fields"
            thead-class="d-none"
            borderless
            small
            responsive="sm"
            >   
                <template v-for="(field,index) in fields" v-slot:[`head(${field.key})`]="data">
                    <b v-bind:key="index" :class="field.headerStyle" > {{ data.label }}</b>
                </template>

                <template v-slot:cell(Name)="data" >
                    <b-button   
                        size="sm" 
                        @click="OpenDetails(data);data.toggleDetails();" 
                        :variant="!data.item.CountsDisable ? 'outline-primary border-white text-info' :'text-muted'"
                        :disabled="data.item.CountsDisable">
                            <b-icon-caret-right-fill v-if="!data.item['_showDetails']"></b-icon-caret-right-fill>
                            <b-icon-caret-down-fill v-if="data.item['_showDetails']"></b-icon-caret-down-fill>
                            {{data.value}}
                    </b-button>             
                </template>

                <template v-slot:cell(Judge)="data" >
                    <b-button 
                        size="sm" 
                        @click="OpenOrderMadeDetails(data)"
                        :variant="!data.item.OrderMadeDisable ? 'outline-primary border-white text-info' :'text-muted'"
                        :disabled="data.item.OrderMadeDisable" 
                        class=" mr-2">
                            Order Made Details
                    </b-button>
                    <b-button 
                        size="sm" 
                        @click="OpenJudgeRecommendation(data)"
                        :variant="!data.item.RecommendationDisable ? 'outline-primary border-white text-info' :'text-muted'"
                        :disabled="data.item.RecommendationDisable" 
                        class=" mr-2">
                            Judge's Recommendations
                    </b-button>
                </template>

                <template v-slot:cell(Count)="data" >
                    <b-button size="sm" disabled variant="white"> 
                        <b>
                            Counts ({{data.item.Counts.length}})
                        </b>    
                    </b-button>                
                </template>

                <template v-slot:row-details>
                    <b-card no-body bg-border="dark"> 
                        <criminal-sentence-details/>
                    </b-card>
                </template>
                
            </b-table>
        </b-card>

        <b-modal v-model="showRecommendation" id="bv-modal-recommendation" hide-footer>
            <template v-slot:modal-title>
                 <h2 class="mb-0"> Recommendations </h2>
            </template>
            <b-table                
                :items="SortedJudgesRecommendation"
                :fields="orderMadeFields"                
                borderless
                small                
                > 
                <template v-slot:cell(Date)="data" >
                    <b-button                        
                        @click="data.toggleDetails();" 
                        variant="outline-primary border-white text-info"
                        size="sm">
                            <b-icon-caret-right-fill v-if="!data.item['_showDetails']"></b-icon-caret-right-fill>
                            <b-icon-caret-down-fill v-if="data.item['_showDetails']"></b-icon-caret-down-fill>
                            {{data.item.FormattedDate}}
                    </b-button>             
                </template> 
                <template v-slot:row-details="row">                   
                    <li                
                        v-for="(recommendation,inx) in row.item.JudgeRecommendation"
                        :key="inx"
                        class="mx-3">
                            {{recommendation}}                           
                    </li>                  
                </template> 
            </b-table>
            <b-button class="mt-3" @click="$bvModal.hide('bv-modal-recommendation')">Close</b-button>
        </b-modal>

        <b-modal v-model="showOrderMade" id="bv-modal-ordermade" hide-footer>
            <template v-slot:modal-title>
                <h2 class="mb-0"> Order Made Details </h2>
            </template>           
            <b-table                
                :items="SortedOrderMade"
                :fields="orderMadeFields"                
                borderless
                small                
                > 
                <template v-slot:cell(Date)="data" >
                    <b-button                        
                        @click="data.toggleDetails();" 
                        variant="outline-primary border-white text-info"
                        size="sm">
                            <b-icon-caret-right-fill v-if="!data.item['_showDetails']"></b-icon-caret-right-fill>
                            <b-icon-caret-down-fill v-if="data.item['_showDetails']"></b-icon-caret-down-fill>
                            {{data.item.FormattedDate}}
                    </b-button>             
                </template> 
                <template v-slot:row-details="row">
                        <li                
                            v-for="(order,inx) in row.item.OrderMade"
                            :key="inx"
                            class="mx-3">
                                {{order}}                           
                        </li>                  
                </template> 
            </b-table>           
            <b-button class="mt-3" @click="$bvModal.hide('bv-modal-ordermade')">Close</b-button>
        </b-modal>
      
    </b-card> 

</template>

<script lang="ts">
import { Component, Vue} from 'vue-property-decorator';
import * as _ from 'underscore';
import { namespace } from 'vuex-class';
import CriminalSentenceDetails from '@components/criminal/CriminalSentenceDetails.vue';
import '@store/modules/CriminalFileInformation';
import "@store/modules/CommonInformation";
const criminalState = namespace("CriminalFileInformation");
const commonState = namespace("CommonInformation");

@Component({
    components: {
        CriminalSentenceDetails
    }
})
export default class CriminalSentence extends Vue {

    @criminalState.State
    public criminalFileInformation!: any

    @criminalState.Action
    public UpdateCriminalFile!: (newCriminalFileInformation: any) => void

    @criminalState.State
    public criminalParticipantSentenceInformation    

    @criminalState.Action
    public UpdateCriminalParticipantSentenceInformation!: (newCriminalParticipantSentenceInformation: any) => void

    @commonState.State
    public displayName!: string;    

    @commonState.Action
    public UpdateDisplayName!: (newInputNames: any) => void

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
   
    isMounted = false

    participantFiles: any[] = [];
    orderMadeClickedParticipant=0;
    judgeRecomClickedParticipant =0;

    

    fields = [        
        {key:'Name',  tdClass: 'border-bottom', headerStyle:'text-primary',  cellStyle:'text-info'},
        {key:'Count', tdClass: 'border-bottom', headerStyle:'text-primary',  cellStyle:'text-info'},
        {key:'Judge', tdClass: 'border-bottom', headerStyle:'text-primary',  cellStyle:'text-info'},
    ]; 
    
    orderMadeFields = [
        {key:'Date',  tdClass: 'border-top', headerStyle:'text-primary',  cellStyle:'text'},
        {key:'Count', tdClass: 'border-top', headerStyle:'text',          cellStyle:'text'},
    ]
    
    public ExtractParticipantInfo(): void {        
        
        for(const fileIndex in this.participantJson)
        {            
            const fileInfo = {};
            const jFile =  this.participantJson[fileIndex];
            fileInfo["Index"] = fileIndex; 
            fileInfo["Part ID"] = jFile.partId;
            fileInfo["First Name"] = jFile.givenNm.trim().length>0 ? jFile.givenNm : "";
            fileInfo["Last Name"] = jFile.lastNm ? jFile.lastNm : jFile.orgNm;
            this.UpdateDisplayName({'lastName': fileInfo["Last Name"], 'givenName': fileInfo["First Name"]});
            fileInfo["Name"] = this.displayName;

            fileInfo["Counts"] = [];
            const counts: any[] = []; 
            
            fileInfo["OrderMade"] = [];
            fileInfo["OrderMadeDisable"] =  true;
            fileInfo["JudgesRecommendation"] = [];
            fileInfo["RecommendationDisable"] =  true;
              
            for(const cnt of this.mergeSentences(jFile.count))           
            {                
                const countInfo = {}; 
               
                countInfo["Date"] = cnt.appearanceDate? cnt.appearanceDate.split(' ')[0] : ''; 
                countInfo["FormattedDate"] = Vue.filter('beautify-date')(countInfo["Date"]);
                                
                countInfo["Finding"]= cnt.finding? cnt.finding:'';
                countInfo["FindingDsc"]= cnt.findingDsc? cnt.findingDsc:'';                                   
               
                countInfo["ChargeIssueCd"] = [] ;
                countInfo["ChargeIssueDsc"] = [];
                countInfo["ChargeIssueDscFull"] = [];
                countInfo["Count"] =cnt.countNumber;

                for(const charge of cnt.charge)
                {                     
                    countInfo["ChargeIssueCd"].push(charge.chargeTxt? charge.chargeTxt: '') ;
                    countInfo["ChargeIssueDsc"].push(charge.chargeDscTxt? (charge.chargeDscTxt.length>10 ? charge.chargeDscTxt.substr(0,10)+' ...':charge.chargeDscTxt): '');
                    countInfo["ChargeIssueDscFull"].push(charge.chargeDscTxt? charge.chargeDscTxt:'');
                }                

                countInfo["Sentence/Disposition Type"]=[];
                countInfo["SentenceDsc"]=[];
                countInfo["Term"] = [];
                countInfo["Amount"]=[];
                countInfo["Due Date/ Until"]=[];
                countInfo["Effective Date"]=[];
                countInfo["OrderMade"]=[];
                countInfo["JudgeRecommendation"]=[]; 

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
                    if(sentence.judgesRecommendation)
                        countInfo["JudgeRecommendation"].push(sentence.judgesRecommendation);
                    
                    if(sentence.sentDetailTxt)
                        countInfo["OrderMade"].push(sentence.sentDetailTxt);                                   
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
                if(countInfo["OrderMade"].length>0)
                {
                    fileInfo["OrderMade"].push(countInfo)
                    fileInfo["OrderMadeDisable"] =  false;
                }

                 if(countInfo["JudgeRecommendation"].length>0)
                {
                    fileInfo["JudgesRecommendation"].push(countInfo)
                    fileInfo["RecommendationDisable"] =  false;
                }
            }
            fileInfo["Counts"] = counts;
            fileInfo["CountsDisable"] = counts.length>0 ? false: true;
            
            
            this.participantFiles.push(fileInfo);
        } 
        const participantInfo = {participantFiles:{}, selectedParticipant:0}
        participantInfo.participantFiles = this.participantFiles;
        this.UpdateCriminalParticipantSentenceInformation(participantInfo);
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
            mergedCount.countNumber = _.uniq(_.sortBy(_.pluck(countGroup, "countNumber"))).join(", ");
            mergedCount.charge = _.chain(countGroup)
            .sortBy(function(sort){ return sort.countNumber; })            
            .map(function (group) { return group.countNumber+ "|" + group.sectionTxt + "|" + group.sectionDscTxt })
            .uniq()
            .map(function (charge) {return { countNum:charge.split("|")[0], chargeTxt: charge.split("|")[1], chargeDscTxt: charge.split("|")[2] }; })
            .value();                return mergedCount;
        });
    }

    get SortedParticipants()
    {         
        return _.sortBy(this.participantFiles,(participant=>{return (participant["Last Name"]? participant["Last Name"].toUpperCase() : '')}))       
    }

    get SortedOrderMade()
    {
        return _.sortBy(this.participantFiles[this.orderMadeClickedParticipant].OrderMade, 'Date').reverse()
    }

     get SortedJudgesRecommendation()
    {
        return _.sortBy(this.participantFiles[this.judgeRecomClickedParticipant].JudgesRecommendation, 'Date').reverse()
    }

    public OpenDetails(data)
    {        
        if(!data.detailsShowing)
        {    
            const participantInfo = this.criminalParticipantSentenceInformation
            participantInfo.selectedParticipant = data.item.Index
            this.UpdateCriminalParticipantSentenceInformation(participantInfo); 
        }       
    }

    showRecommendation=false
    showOrderMade=false

    public OpenOrderMadeDetails(data)
    {
        console.log(data)
        this.orderMadeClickedParticipant = data.item.Index
        this.showOrderMade=true
    }

    public OpenJudgeRecommendation(data)
    { 
        console.log(data)
        this.judgeRecomClickedParticipant = data.item.Index
        this.showRecommendation=true           
    }

}
</script>

<style scoped>
 .card {
        border: black;
    }

</style>