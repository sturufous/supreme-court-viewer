<template>
<body>
   <b-card  v-if= "isMounted">       
            <b-card bg-variant="light">
                <b-table
                :items="CrownInformation"
                :fields="fields"
                thead-class="d-none"
                responsive="sm"               
                striped
                > 
              
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

    fields =[{key:"key", label: "Key"},
    {key:"value", label: "Key", tdClass: 'crownInformation'}];
    public getCrownInfo(): void {
       
        const data = this.criminalFileInformation.detailsData;
        let crownInfo = {};
        // crownInfo["Crown Assigned"] = data.assignedPartNm;
        // crownInfo["Crown Time Estimate"] = " Days"
        // crownInfo["Case Age"] = data.caseAgeDays + " Days";
        // crownInfo["Approved By"] = data.approvedByAgencyCd + "-"
        // this.CrownInformation.push(crownInfo);                  
        

        crownInfo['key'] = "Crown Assigned";
        crownInfo['value'] = data.assignedPartNm;
        this.CrownInformation.push(crownInfo);
        crownInfo = {};
        crownInfo['key'] = "Crown Time Estimate";
        crownInfo['value'] = " Days"
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

    .crownInformation {
        font-weight: bold;
    }

    .card {
        border: white;
    }

</style>