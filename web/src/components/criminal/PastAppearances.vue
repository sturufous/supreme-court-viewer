<template>
<body>
    <b-card bg-variant="white">
        <div>
            <h3 class="mx-2 font-weight-normal"> Last Three Past Appearances</h3>
            <hr class="mx-1 bg-light" style="height: 5px;"/> 
        </div>

        <b-card bg-variant="white">           
            <b-table
            :items="pastAppearancesList"
            :fields="fields"
            :sort-by.sync="sortBy"
            :sort-desc.sync="sortDesc"
            :no-sort-reset="true"
            borderless
            responsive="sm"
            >   
                <template v-for="(field,index) in fields" v-slot:[`head(${field.key})`]="data">
                    <b v-bind:key="index" :class="field.headerStyle" > {{ data.label }}</b>
                </template>
                <template v-for="(field,index) in fields" v-slot:[`cell(${field.key})`]="data" >
                    <span v-bind:key="index" :class="field.cellStyle" v-if="data.field.key != 'Status' && data.field.key != 'Name'">  {{ data.value }} </span>
                    <span v-bind:key="index" :class="data.item.Charges.length>0?field.cellStyle:''" v-if="data.field.key == 'Name'"> 
                         {{ data.value }}
                       <b-button size="sm" @click="data.toggleDetails" class="mr-2">
                         Details
                        </b-button>                      
                    </span>
                  
                </template>

                <template v-slot:row-details>
                    <b-card> 
                        <criminal-documents-view/>
                    </b-card>
                </template>
            </b-table>
        </b-card>
    </b-card> 

</body>
</template>

<script lang="ts">
import { Component, Vue } from "vue-property-decorator";
import { namespace } from "vuex-class";

import CriminalDocumentsView from '@components/criminal/CriminalDocumentsView.vue';

import "@store/modules/CriminalFileInformation";
const criminalState = namespace("CriminalFileInformation");

@Component({
    components: {
        CriminalDocumentsView
    }
})
export default class PastAppearances extends Vue {

    @criminalState.State
    public criminalFileInformation!: any;

    mounted() {
        this.getPastAppearances();
    }

    public getPastAppearances(): void {      
        const data = this.criminalFileInformation.detailsData;    
        this.pastAppearancesJson = data.participant 
        this.ExtractPastAppearancesInfo();
        this.isMounted = true;          
    } 
  
    isMounted = false;
    pastAppearancesJson;
    
    sortBy = 'Date';
    sortDesc = false;
    pastAppearancesList: any[] = [];

    fields =  
    [
        {key:'Date',       sortable:true,  tdClass: 'border-top', headerStyle:'text-primary', cellStyle:'text-info'},
        {key:'Reason',     sortable:true,  tdClass: 'border-top', headerStyle:'text-primary', cellStyle:'text'},
        {key:'Time',       sortable:false, tdClass: 'border-top', headerStyle:'text',         cellStyle:'text'},
        {key:'Duration',   sortable:false, tdClass: 'border-top', headerStyle:'text',         cellStyle:'text'},
        {key:'Location',   sortable:true,  tdClass: 'border-top', headerStyle:'text-primary', cellStyle:'text'},
        {key:'Room',       sortable:false, tdClass: 'border-top', headerStyle:'text',         cellStyle:'text'},
        {key:'Presider',   sortable:true,  tdClass: 'border-top', headerStyle:'text-primary', cellStyle:'text'},
        {key:'Accused',    sortable:true,  tdClass: 'border-top', headerStyle:'text-primary', cellStyle:'text'},
        {key:'Status',     sortable:true,  tdClass: 'border-top', headerStyle:'text-primary', cellStyle:'text-white bg-secondary'},
    ];

    statusFields = 
    [
        {key:'Warrant Issued',      abbr:'W',   code:'warrantYN'},
        {key:'In Custody',          abbr:'IC',  code:'inCustodyYN'},
        {key:'Detention Order',     abbr:'DO',  code:'detainedYN'} , 
        {key:'Interpreter Required',abbr:'INT', code:'interpreterYN'}
    ];
  
    public ExtractPastAppearancesInfo(): void {
        
        for (const fileIndex in this.pastAppearancesJson) {
            const fileInfo = {};
            const jFile = this.pastAppearancesJson[fileIndex];

            fileInfo["Index"] = fileIndex;
            fileInfo["First Name"] = jFile.givenNm ? jFile.givenNm : "";
            fileInfo["Last Name"] = jFile.lastNm ? jFile.lastNm : jFile.orgNm;
            fileInfo["Name"] = this.getNameOfParticipant(fileInfo["Last Name"], fileInfo["First Name"]);            
            fileInfo["D.O.B."] = jFile.birthDt? (new Date(jFile.birthDt.split(' ')[0])).toUTCString().substr(4,12) : '';

            fileInfo["Charges"] = [];         
            const charges: any[] = [];         
            for(const charge of jFile.charge)
            {              
                    const docInfo = {};                   
                    docInfo["Description"]= charge.sectionDscTxt
                    docInfo["Code"]= charge.sectionTxt
                    charges.push(docInfo);
            }
            fileInfo["Charges"] = charges;

            fileInfo["Status"] = [];
            for (const status of this.statusFields)
            {
                if(jFile[status.code] =='Y')
                    fileInfo["Status"].push(status);
            }
   
            fileInfo['Counsel'] = jFile.counselLastNm? 'JUSTIN: '+ jFile.counselGivenNm +' '+ jFile.counselLastNm : ''
            fileInfo['Counsel Designation Filed'] = jFile.designatedCounselYN           
            this.pastAppearancesList.push(fileInfo); 
        }
    }

    public getNameOfParticipant(lastName, givenName) {
        return ( lastName + ", " + givenName );
    }

}
</script>

<style scoped>
 .card {
        border: white;
    }
</style>