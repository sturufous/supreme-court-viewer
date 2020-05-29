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

        
            <b-card bg-variant="light">           
                <b-table
                :items="participantFiles['Counts']"
                :fields="fields"
                :sort-by.sync="sortBy"
                :sort-desc.sync="sortDesc"
                :no-sort-reset="true"                
                striped
                borderless
                sort-icon-left
                responsive="sm"
                >   
                    <template v-for="(field,index) in fields" v-slot:[`head(${field.key})`]="data">
                    <b v-bind:key="index" :class="field.headerStyle" > {{ data.label }}</b>
                    </template>
                    <template v-for="(field,index) in fields" v-slot:[`cell(${field.key})`]="data" >
                        <span 
                            v-bind:key="index"                                                    
                            v-if="field.key.includes('Date')"                           
                            :class= "cellClass(field, index, data)"  
                            style="white-space: pre-line"> {{ data.value | beautify-date}}
                        </span>
                        <span 
                            v-bind:key="index"                                                       
                            v-else
                            v-on:click= "cellClick(index, data)"
                            :class= "cellClass(field, index, data)"  
                            style="white-space: pre-line"> {{ data.value }}
                        </span>
                    </template>
                </b-table>
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
        this.countJson = data.count
        this.ExtractParticipantInfo()          
        this.isMounted = true;
    }

    mounted () {        
        this.getParticipants();  
    }

    participantJson;
    countJson;

    activeparticipant = 0;           
    sortBy = 'Date';
    sortDesc = true;
   
    isMounted = false
    
    NumberOfCounts = 0;

    participantFiles: any[] = [];


    fields = [        
            {key:'Date',                     sortable:true,   tdClass: 'border-top', headerStyle:'text-primary',  cellStyle:'text'},
            {key:'Count',                    sortable:true,   tdClass: 'border-top', headerStyle:'text-primary',  cellStyle:'text-muted'},
            {key:'Charge/Issue',             sortable:false,  tdClass: 'border-top', headerStyle:'text',          cellStyle:'text'},
            {key:'Finding',                  sortable:true,   tdClass: 'border-top', headerStyle:'text-danger',   cellStyle:'text'},
            {key:'Sentence/Disposition Type',sortable:false,  tdClass: 'border-top', headerStyle:'text',          cellStyle:'text'},
            {key:'Term',                     sortable:false,  tdClass: 'border-top', headerStyle:'text',          cellStyle:'text'},
            {key:'Amount',                   sortable:false,  tdClass: 'border-top', headerStyle:'text',          cellStyle:'text'},
            {key:'Due Date/Until',           sortable:false,  tdClass: 'border-top', headerStyle:'text',          cellStyle:'text'},
            {key:'Effective Date',           sortable:false,  tdClass: 'border-top', headerStyle:'text',          cellStyle:'text'},
        ];

    public getNameOfParticipant(num)
    {        
        return  this.participantFiles[num]["Last Name"]+', '+this.participantFiles[num]["First Name"];           
    }
    
    public ExtractParticipantInfo(): void {        
        
        for(const fileIndex in this.participantJson)
        {            
            const fileInfo = {};
            const jFile =  this.participantJson[fileIndex];
            fileInfo["Index"] = fileIndex; 
            fileInfo["Part ID"] = jFile.partId;            
            fileInfo["First Name"] = jFile.givenNm ? jFile.givenNm : "";
            fileInfo["Last Name"] = jFile.lastNm ? jFile.lastNm : jFile.orgNm;            
            
            fileInfo["Counts"] = [];
            const counts: any[] = [];
           
            //for(const cnt of jFile.count)
            for(const cnt of  this.countJson)
            {                
                const countInfo = {}; 
                countInfo["Date"]= cnt.appearanceDate? cnt.appearanceDate.split(' ')[0] : ''; 
                countInfo["Count"]= cnt.countNumber;
                countInfo["Charge/Issue"]= cnt.sectionTxt +'--' +cnt.sectionDscTxt;
                countInfo["Finding"]= cnt.finding;
                // countInfo["Sentence/Disposition Type"]=
                // countInfo["Term"]=  
                // countInfo["Amount"]=   
                // countInfo["Due Date/Until"]=  
                // countInfo["Effective Date"]=             
                    
                counts.push(countInfo);
            }
            fileInfo["Counts"] = counts;
                        
            this.participantFiles.push(fileInfo);
        }
        
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

    get NumberOfDocuments() {       
        
        return(this.participantFiles[this.activeparticipant]["Documents"].length)            
         
    }
    
}
</script>

<style scoped>
 .card {
        border: white;
    }

</style>