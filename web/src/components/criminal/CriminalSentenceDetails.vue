<template>
<body>
   <b-card  v-if= "isMounted">
        <div>         
            <h3 class="mx-2 font-weight-normal"> Counts ({{NumberOfCounts}}) </h3>
            <hr class="mx-1 bg-light" style="height: 5px;"/>         
        </div>
      
        <b-card>
            <b-dropdown  variant="light text-info" :text="getNameOfParticipant(activeparticipant)" class="m-2">    
                <b-dropdown-item-button  
                    v-for="(file,index) in SortedParticipants" 
                    :key="index"
                    v-on:click="activeparticipant = index">
                        {{getNameOfParticipant(index)}}
                </b-dropdown-item-button> 
            </b-dropdown>                 
        </b-card>

        
            <b-card bg-variant="white">
                
                 <b-table-simple small responsive borderless>
                    <b-thead head-variant="dark">                    
                        <b-tr>
                            <b-th v-for="(h,index) in fields" v-bind:key="index" >
                                <b-icon-caret-down-fill v-if="index==0" @click="sortClick(h)"></b-icon-caret-down-fill>
                                {{h.key}}
                            </b-th>                        
                        </b-tr>
                    </b-thead>

                    <b-tbody v-for="(counts,inx) in participantFiles[activeparticipant]['Counts']" v-bind:key="inx">
                        <b-tr v-for="(sentence,index) in counts['Sentence/Disposition Type']" v-bind:key="index" :style ="getRowStyle(index)">                            
                            
                            <b-td :rowspan="counts.Len" v-if="index==0">{{counts['Date'] | beautify-date}}</b-td>                            
                            <b-td :rowspan="counts.Len" v-if="index==0">{{counts['Count']}}</b-td>
                            
                            <b-td :rowspan="counts.Len" v-if="index==0">                                
                                <b> {{counts.ChargeIssueCd}} </b> 
                                <span>
                                    &mdash; 
                                    <b-badge
                                        variant = "light"  
                                        v-b-tooltip.hover 
                                        :title="counts.ChargeIssueDscFull"> 
                                        {{counts.ChargeIssueDsc}} 
                                    </b-badge>
                                </span>
                            </b-td>
                            
                            <b-td :rowspan="counts.Len" v-if="index==0">
                                <b-badge
                                    variant = "light"  
                                    v-b-tooltip.hover 
                                    :title="counts.FindingDsc"> 
                                    {{counts['Finding']}} 
                                </b-badge>
                            </b-td>

                            <b-td>
                                <b-badge
                                    variant = "light"  
                                    v-b-tooltip.hover 
                                    title="counts.SentenceDsc"> 
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
</body>
</template>

<script lang="ts">
import { Component, Vue} from 'vue-property-decorator';
import { namespace } from 'vuex-class';
import '@store/modules/CriminalFileInformation';
const criminalState = namespace('CriminalFileInformation');

@Component
export default class CriminalSentenceDetails extends Vue {

    @criminalState.State
    public criminalFileInformation!: any

    @criminalState.Action
    public UpdateCriminalFile!: (newCriminalFileInformation: any) => void


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
   
    activeparticipant = 0;           
    sortBy = 'Count';
    sortDesc = false;
   
    isMounted = false

    participantFiles: any[] = [];

    fields = [        
        {key:'Date',                     sortable:true,   tdClass: 'border-top', headerStyle:'text-primary',  cellStyle:'text'},
        {key:'Count',                    sortable:true,   tdClass: 'border-top', headerStyle:'text-primary',  cellStyle:'text-muted'},
        {key:'Charge/Issue',             sortable:false,  tdClass: 'border-top', headerStyle:'text',          cellStyle:'text'},
        {key:'Finding',                  sortable:true,   tdClass: 'border-top', headerStyle:'text-danger',   cellStyle:'text'},
        
        {key:'Sentence/ Disposition Type',sortable:false,  tdClass: 'border-top', headerStyle:'text',          cellStyle:'text'},
   
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
           
            for(const cnt of jFile.count)           
            {                
                const countInfo = {}; 
                countInfo["Date"] = cnt.appearanceDate? cnt.appearanceDate.split(' ')[0] : ''; 
                countInfo["Count"] = cnt.countNumber;
                countInfo["ChargeIssueCd"] = cnt.sectionTxt ;
                countInfo["ChargeIssueDsc"] = cnt.sectionDscTxt.substr(0,20);
                countInfo["ChargeIssueDscFull"] = cnt.sectionDscTxt;
                countInfo["Finding"]= cnt.finding;
                countInfo["FindingDsc"]= cnt.findingDsc;

                countInfo["Sentence/Disposition Type"]=[];
                countInfo["Term"] = [];
                countInfo["Amount"]=[];
                countInfo["Due Date/ Until"]=[];
                countInfo["Effective Date"]=[]; 
                countInfo["Len"]=cnt.sentence.length; 
                
                for(const sentence of cnt.sentence)
                {                    
                   countInfo["Sentence/Disposition Type"].push(sentence.sntpCd); 
                   countInfo["Term"].push(sentence.sentTermPeriodQty? (sentence.sentTermPeriodQty + ' ' + sentence.sentTermCd):'')
                   countInfo["Amount"].push(sentence.sentMonetaryAmt? sentence.sentMonetaryAmt:'')
                   countInfo["Due Date/ Until"].push(sentence.sentDueTtpDt? sentence.sentDueTtpDt.split(' ')[0]:'')
                   countInfo["Effective Date"].push(sentence.sentEffectiveDt?  sentence.sentEffectiveDt.split(' ')[0]:'')                   
                } 
                  
                counts.push(countInfo);
            }
            fileInfo["Counts"] = counts;
                        
            this.participantFiles.push(fileInfo);
        }

        console.log(this.participantFiles[this.activeparticipant]["Counts"])
        
    }

    get SortedParticipants()
    {
        return this.participantFiles.sort((a, b): any =>
        {
            const LastName1 = a["Last Name"]? a["Last Name"].toUpperCase() : '';
            const LastName2 = b["Last Name"]? b["Last Name"].toUpperCase() : '';
            if(LastName1 > LastName2) return 1;
            if(LastName1 < LastName2) return -1;
            return 0;
        });        
    } 

    get NumberOfCounts() { 
        return(this.participantFiles[this.activeparticipant]["Counts"].length) 
    }

    public getRowStyle(index) {
        if(index==0) return "border-top : 1px solid #999;"
        return '';
    }
    
    public sortClick(data)
    {
        console.log('hi')
    }
}
</script>

<style scoped>
 .card {
        border: white;
    }

</style>