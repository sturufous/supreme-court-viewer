<template>
   <b-card  v-if= "isMounted" no-body> 
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

            <b-tbody v-for="(counts,inx) in SortedParticipantSentencesCounts" v-bind:key="inx">
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
</template>

<script lang="ts">
import { Component, Vue} from 'vue-property-decorator';
import * as _ from 'underscore';
import { namespace } from 'vuex-class';
import '@store/modules/CriminalFileInformation';
import {participantSentencesInfoType} from '../../types/criminal';
const criminalState = namespace("CriminalFileInformation");

@Component
export default class CriminalSentenceDetails extends Vue {   

    @criminalState.State
    public criminalParticipantSentenceInformation
    
    public getParticipants(): void {       
         
        this.participantSentences = this.criminalParticipantSentenceInformation.participantSentences
        this.selectedParticipant = this.criminalParticipantSentenceInformation.selectedParticipant
         
        this.isMounted = true;
    }

    mounted () {        
        this.getParticipants();  
    }

    participantJson;         
    sortBy = 'Date';
    dateSortDir = 'desc';   
    isMounted = false
    selectedParticipant = 0;
    participantSentences: participantSentencesInfoType[] = [];

    fields = [        
        {key:'Date',                     sortable:true,   tdClass: 'border-top', headerStyle:'text-primary'},
        {key:'Count',                    sortable:true,   tdClass: 'border-top', headerStyle:'text-primary'},
        {key:'Charge/Issue',             sortable:false,  tdClass: 'border-top', headerStyle:'text'},
        {key:'Finding',                  sortable:true,   tdClass: 'border-top', headerStyle:'text-danger'},        
        {key:'Sentence/ Disposition Type',sortable:false, tdClass: 'border-top', headerStyle:'text'},   
        {key:'Term',                     sortable:false,  tdClass: 'border-top', headerStyle:'text'},
        {key:'Amount',                   sortable:false,  tdClass: 'border-top', headerStyle:'text'},
        {key:'Due Date/ Until',          sortable:false,  tdClass: 'border-top', headerStyle:'text'},
        {key:'Effective Date',           sortable:false,  tdClass: 'border-top', headerStyle:'text'},
    ];   
    
    get SortedParticipantSentencesCounts()
    {
        if(this.dateSortDir =='desc')
            return _.sortBy(this.participantSentences[this.selectedParticipant]['Counts'],this.sortBy).reverse()
        else
            return _.sortBy(this.participantSentences[this.selectedParticipant]['Counts'],this.sortBy)  
    } 

    get NumberOfCounts() { 
        return(this.participantSentences[this.selectedParticipant]["Counts"].length) 
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