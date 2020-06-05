<template>

    <b-card bg-variant="white">
        <div>            
            <hr class="mb-3 bg-light" style="height: 5px;"/> 
        </div>

        <b-card bg-variant="white" no-body>           
            <b-table
            :items="SortedParticipants"
            :fields="fields"
            borderless
            small
            responsive="sm"
            >   
                <template v-for="(field,index) in fields" v-slot:[`head(${field.key})`]="data">
                    <b v-bind:key="index" :class="field.headerStyle" > {{ data.label }}</b>
                </template>

                <template v-slot:cell()="data" >
                    <span > 

                        <b-button   
                            size="sm" 
                            @click="OpenDetails(data);data.toggleDetails();" 
                            variant="outline-primary border-white"
                            :disabled="data.item.CountsDiable" 
                            class="mr-2">
                                <b-icon-caret-right-fill v-if="!data.item['_showDetails']"></b-icon-caret-right-fill>
                                <b-icon-caret-down-fill v-if="data.item['_showDetails']"></b-icon-caret-down-fill>
                        </b-button>

                        <span :class="data.field.cellStyle" style= "white-space: pre">                     
                            {{data.value}}       
                        </span>

                        <b-button size="sm" variant="outline-primary border-white" class=" mr-2">
                            Order Made Details
                        </b-button>

                        <b-button size="sm" variant="outline-primary border-white" class=" mr-2">
                           Judge's Recommendations
                        </b-button>

                        <span>
                            Counts({{data.item.Counts.length}})
                        </span>
                    </span> 
                </template>
                <template v-slot:row-details>
                    <b-card no-body bg-border="dark"> 
                        <criminal-sentence-details/>
                    </b-card>
                </template>
                
            </b-table>
        </b-card>
      
    </b-card> 

</template>

<script lang="ts">
import { Component, Vue} from 'vue-property-decorator';
import * as _ from 'underscore';
import { namespace } from 'vuex-class';
import CriminalSentenceDetails from '@components/criminal/CriminalSentenceDetails.vue';
import '@store/modules/CriminalFileInformation';
const criminalState = namespace('CriminalFileInformation');

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

    fields = [        
        {key:'Name', tdClass: 'border-top', headerStyle:'text-primary',  cellStyle:'text-info'},
    ];

    public getNameOfParticipant(firstName, lastName)
    {        
        if(!firstName)
            return  lastName;
        else if(!lastName)
            return firstName;
        else
            return  lastName+', '+firstName;           
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
            fileInfo["Name"] = this.getNameOfParticipant(fileInfo["First Name"], fileInfo["Last Name"]);            
            
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
            fileInfo["CountsDiable"] = counts.length>0 ? false: true;


                        
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
            mergedCount.charge = _.chain(countGroup)
            .sortBy(function(stooge){ return stooge.countNumber; })
            .map(function (group) { return group.countNumber+ "|" + group.sectionTxt + "|" + group.sectionDscTxt })
            .uniq()
            .map(function (charge) { return { countNum:charge.split("|")[0], chargeTxt: charge.split("|")[1], chargeDscTxt: charge.split("|")[2] }; })
            .value();                return mergedCount;
        });
    }

    // get NumberOfCounts() { 
    //     return(this.participantFiles[this.activeCriminalParticipantIndex]["Counts"].length) 
    // }

    get SortedParticipants()
    {         
        return _.sortBy(this.participantFiles,(participant=>{return (participant["Last Name"]? participant["Last Name"].toUpperCase() : '')}))       
    }

    public OpenDetails(data)
    {
        console.log(data)
        if(!data.detailsShowing)
        {    
            const participantInfo = this.criminalParticipantSentenceInformation
            participantInfo.selectedParticipant = data.item.Index
            this.UpdateCriminalParticipantSentenceInformation(participantInfo); 
        }       
    }

}
</script>

<style scoped>
 .card {
        border: black;
    }

</style>