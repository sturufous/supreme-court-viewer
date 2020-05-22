<template>
<body>
    <b-card bg-variant="white">
        <div>
            <b> Participants ({{numberOfParticipants}}) </b>
        </div>

        <b-card bg-variant="white">           
            <b-table
            :items="participantList"
            :fields="fields"
            :sort-by.sync="sortBy"
            :sort-desc.sync="sortDesc"
            :no-sort-reset="true"
            responsive="sm"
            >   
                <template v-for="(field,index) in fields" v-slot:[`head(${field.key})`]="data">
                    <b v-bind:key="index" :class="field.headerStyle" > {{ data.label }}</b>
                </template>
                <template v-for="(field,index) in fields" v-slot:[`cell(${field.key})`]="data" >
                    <span v-bind:key="index" :class="field.cellStyle" v-if="data.field.key != 'Status'">  {{ data.value }} </span>
                    <span v-bind:key="index" v-if="data.field.key == 'Status'">  
                        <b-badge 
                            v-for="(field,index) in data.value"
                            :key="index" 
                            class="mr-1"
                            v-b-tooltip.hover 
                            :title='field.key' > 
                            {{ field.abbr }} 
                        </b-badge>
                    </span>
                </template>
            </b-table>
        </b-card>
    </b-card>
    <hr class="mx-3" style="height: 2px;"/>  

</body>
</template>

<script lang="ts">
import { Component, Vue } from "vue-property-decorator";
import { namespace } from "vuex-class";
import "@store/modules/CriminalFileInformation";
const criminalState = namespace("CriminalFileInformation");

@Component
export default class CriminalParticipants extends Vue {

    @criminalState.State
    public criminalFileInformation!: any;

    mounted() {
        this.getDocuments();
    }

    public getDocuments(): void {      
        const data = this.criminalFileInformation.detailsData;    
        this.participantJson = data.participant 
        this.ExtractDocumentInfo();
        this.isMounted = true;          
    } 

  
    isMounted = false;
    participantJson;
    numberOfParticipants = 0;

    sortBy = 'Name';
    sortDesc = false;
    participantList: any[] = [];

    fields =  
    [
        {key:'Name',                    sortable:true,  headerStyle:'text-info',   cellStyle:'text-info'},
        {key:'D.O.B.',                  sortable:false,  headerStyle:'text',         cellStyle:'text'},
        {key:'Status',                  sortable:false, headerStyle:'text',         cellStyle:'text-white bg-secondary'},
        {key:'Counsel',                 sortable:false, headerStyle:'text',         cellStyle:'text'},
        {key:'Counsel Designation Filed',sortable:false, headerStyle:'text',        cellStyle:'text'},
    ];

    statusFields = 
    [
        {key:'Warrant Issued',      abbr:'W',   code:'warrantYN'},
        {key:'In Custody',          abbr:'IC',  code:'inCustodyYN'},
        {key:'Detention Order',     abbr:'DO',  code:'detainedYN'} , 
        {key:'Interpreter Required',abbr:'INT', code:'interpreterYN'}
    ];
  
    public ExtractDocumentInfo(): void {
        
        for (const fileIndex in this.participantJson) {
            const fileInfo = {};
            const jFile = this.participantJson[fileIndex];
            fileInfo["Index"] = fileIndex;
            fileInfo["First Name"] = jFile.givenNm ? jFile.givenNm : "";
            fileInfo["Last Name"] = jFile.lastNm ? jFile.lastNm : jFile.orgNm;
            fileInfo["Name"] = this.getNameOfParticipant(fileInfo["Last Name"], fileInfo["First Name"]);            
            fileInfo["D.O.B."] = jFile.birthDt? (new Date(jFile.birthDt.split(' ')[0])).toUTCString().substr(4,12) : '';
            fileInfo["Status"] = [];

            for (const status of this.statusFields)
            {
                if(jFile[status.code] =='Y')
                    fileInfo["Status"].push(status);
            }
   
            fileInfo['Counsel'] = ''
            fileInfo['Counsel Designation Filed'] = jFile.designatedCounselYN

            this.participantList.push(fileInfo);
        }
        this.numberOfParticipants = this.participantList.length;
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