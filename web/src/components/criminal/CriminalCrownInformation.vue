<template>
<body>
   <b-card  v-if= "isMounted">
       <div>         
            <h3 class="mx-2 font-weight-normal"> Crown Information </h3>
            <hr class="mx-1 bg-light" style="height: 5px;"/>         
        </div>       
        <b-card bg-variant="white">
            <b-table
            :items="CrownInformation"
            :fields="fields"            
            thead-class="d-none"
            responsive="sm"
            borderless               
            striped
            >
            <template  v-slot:cell(CrownInfoValue)="data">
                <span v-if="data.item['CrownInfoFieldName'] == 'Crown Assigned' ">
                    <b v-for="(assignee, index) in data.item['CrownInfoValue']"  v-bind:key="index" style= "white-space: pre-line">{{ assignee }} <br></b>
                </span>
                <span v-if="data.item['CrownInfoFieldName'] != 'Crown Assigned' ">
                    <b > {{ data.value }}</b>
                </span>                    
            </template>  
            <!-- <template v-for="(field,index) in fields" v-slot:[`cell(${field.key})`]="data">
                <span v-bind:key="index" v-if="data.items.value != 'Crown Assigned'">
                    <b > {{ data.value }}</b>
                </span>
            <span v-bind:key="index" v-if="data.items.value == 'Crown Assigned'">  
                <p  
                    v-for="(assigned,index) in assignedCrown"
                    :key="index" 
                    class="mr-1"> 
                    {{ assigned }} 
                </p>
            </span>
            </template> -->
            </b-table>            
        </b-card>       
   </b-card> 
</body>
</template>

<script lang="ts">
import { Component, Vue, Watch } from 'vue-property-decorator';
import { namespace } from 'vuex-class';
import '@store/modules/CriminalFileInformation';
const criminalState = namespace('CriminalFileInformation');

@Component
export default class CriminalCrownInformation extends Vue {

    @criminalState.State
    public criminalFileInformation!: any

    fields =[
        {key:"CrownInfoFieldName", label: "Crown Info Field Name"},
        {key:"CrownInfoValue", label: "Crown Info Value"}
    ];

    public getCrownInfo(): void {
       
        const data = this.criminalFileInformation.detailsData;
        const assignedCrown: string[] = [];
        if (data.crown.length > 0) {
            for (const assignee of data.crown) {
                assignedCrown.push(assignee.lastNm + ", " + assignee.givenNm)
            }
            console.log(assignedCrown)
        }
        let crownInfo = {};    

        crownInfo['CrownInfoFieldName'] = "Crown Assigned";
        crownInfo['CrownInfoValue'] = assignedCrown;
        this.CrownInformation.push(crownInfo);
        crownInfo = {};
        crownInfo['CrownInfoFieldName'] = "Crown Time Estimate";
        crownInfo['CrownInfoValue'] = " Days"
        this.CrownInformation.push(crownInfo);
        crownInfo = {};
        crownInfo['CrownInfoFieldName'] = "Case Age";
        crownInfo['CrownInfoValue'] = data.caseAgeDays + " Days";
        this.CrownInformation.push(crownInfo);
        crownInfo = {};
        crownInfo['CrownInfoFieldName'] = "Approved By";
        crownInfo['CrownInfoValue'] = data.approvedByAgencyCd + "-"
        this.CrownInformation.push(crownInfo);  

        this.isMounted = true;

    }    

    mounted () {              
        this.getCrownInfo();  
    }

    participantJson;
    crownAssigned;
    crownTimeEstimate;
    CaseAge;
    ApprovedBy;
    CrownInformation: any[] = [];
    message = 'Loading';
    isMounted = false  
}
</script>

<style>
    .card {
        border: white;
    }

</style>