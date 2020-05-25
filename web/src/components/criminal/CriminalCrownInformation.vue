<template>
<body>
   <b-card  v-if= "isMounted">
       <div>         
            <h3 class="mx-2 font-weight-normal"> Crown Information </h3>
            <hr class="mx-1 bg-light" style="height: 5px;"/>         
        </div>       
        <b-card bg-variant="white">
            <b-table
            :items="crownInformation"
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
                assignedCrown.push(this.formatNames(assignee.lastNm) + ", " + this.formatNames(assignee.givenNm))
            }
        }
        let crownInfo = {};
        crownInfo['CrownInfoFieldName'] = "Crown Assigned";
        crownInfo['CrownInfoValue'] = assignedCrown;
        this.crownInformation.push(crownInfo);
        crownInfo = {};
        crownInfo['CrownInfoFieldName'] = "Crown Time Estimate";
        if (data.crownEstimateLenQty) {
            if (data.crownEstimateLenQty == 1) {
                crownInfo['CrownInfoValue'] = data.crownEstimateLenQty + " " + data.crownEstimateLenDsc.replace('s', '')
            } else {
                crownInfo['CrownInfoValue'] = data.crownEstimateLenQty + " " + data.crownEstimateLenDsc
            }

        } else {
            crownInfo['CrownInfoValue'] = ''
        }        
        this.crownInformation.push(crownInfo);
        crownInfo = {};
        crownInfo['CrownInfoFieldName'] = "Case Age";
        crownInfo['CrownInfoValue'] = data.caseAgeDays?data.caseAgeDays + " Days":'';
        this.crownInformation.push(crownInfo);
        crownInfo = {};
        crownInfo['CrownInfoFieldName'] = "Approved By";
        if (data.approvedByAgencyCd) {           
            crownInfo['CrownInfoValue'] = data.approvedByPartNm?data.approvedByAgencyCd + " - " + data.approvedByPartNm: data.approvedByAgencyCd + " -";           
        } else {
            crownInfo['CrownInfoValue'] = '';
        }        
        this.crownInformation.push(crownInfo);
        this.isMounted = true;

    }
    
    public formatNames (name: string): string {
        return name.charAt(0).toUpperCase() + name.slice(1).toLowerCase();
    }

    mounted () {              
        this.getCrownInfo();  
    }
    
    crownInformation: any[] = [];
    isMounted = false  
}
</script>

<style scoped>
 .card {
        border: white;
    }
</style>