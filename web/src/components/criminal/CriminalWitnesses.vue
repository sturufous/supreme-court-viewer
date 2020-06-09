<template>

    <b-card bg-variant="white">
        <b-card style="height: 50px; background-color:#f0f0f0">
            <b-dropdown variant="light" :text="selectedType" style="position: absolute; top: 6px; bottom: 6px; left: 6px;" >    
                <b-dropdown-item-button  
                    v-for="(witnessType,index) in witnessDropDownFields" 
                    :key="index"
                    v-on:click="selectedType = witnessType">
                        {{witnessDropDownFields[index]}}
                </b-dropdown-item-button> 
            </b-dropdown>                       
        </b-card>
        <b-row cols="2" >
            <b-col class="mt-4" md="8" cols="8" style="overflow: auto;" v-if="!(filteredWitnessList.length>0)">
                <span class="text-muted" v-if="!(witnessList.length>0)"> No witnesses. </span>
                <span class="text-muted" v-if="(witnessList.length>0) && !(filteredWitnessList.length>0)"> No witnesses in this category. </span>
            </b-col>            
            <b-col class="mt-4" md="8" cols="8" style="overflow: auto;" v-if="(filteredWitnessList.length>0)">
                <b-table
                :items="filteredWitnessList"
                :fields="witnessFields"
                :sort-by.sync="sortBy"
                :sort-desc.sync="sortDesc"
                :no-sort-reset="true"
                borderless
                sort-icon-left
                responsive="sm"
                >   
                    <template v-for="(field,index) in witnessFields" v-slot:[`head(${field.key})`]="data">
                        <b v-bind:key="index" :class="field.headerStyle" > {{ data.label }}</b>
                    </template> 
                    <template v-slot:cell(Name)="data" >                        
                        <span :class="data.field.cellStyle" >  {{ data.value }} </span>
                        <span v-if="data.item['Agency']"> <br> ({{data.item.Agency}}: {{data.item['Pin Code']}}) </span>
                    </template>

                    <template v-slot:cell(Required)="data" >                        
                        <b-badge :class="data.field.cellStyle" >  {{ data.value }} </b-badge>                    
                    </template>

                </b-table>
            </b-col>
            <b-col col md="4" cols="4" style="overflow: auto;">
                
                    <h4 class="mt-5 font-weight-bold"> Witness Counts </h4>
                  
                <b-table
                :items="witnessCounts"
                :fields="witnessCountsFields"            
                thead-class="d-none"
                responsive="sm"
                borderless
                :tbody-tr-class="totalBackground"
                >
                    <template  v-slot:cell(WitnessCountValue)="data">
                        <span >
                            <b > {{ data.value }}</b>
                        </span>                    
                    </template>
                    
                </b-table>
            </b-col>
        </b-row>       
    </b-card> 

</template>

<script lang="ts">
import { Component, Vue } from "vue-property-decorator";
import { namespace } from "vuex-class";
import "@store/modules/CriminalFileInformation";
import "@store/modules/CommonInformation";
const criminalState = namespace("CriminalFileInformation");
const commonState = namespace("CommonInformation");

@Component
export default class CriminalWitnesses extends Vue {

    @criminalState.State
    public criminalFileInformation!: any;

    @commonState.State
    public displayName!: string;    

    @commonState.Action
    public UpdateDisplayName!: (newInputNames: any) => void

    mounted() {
        this.getWitnesses();
    }

    public getWitnesses(): void {      
        const data = this.criminalFileInformation.detailsData;    
        this.witnessesJson = data.witness 
        this.ExtractWitnessInfo();
        this.isMounted = true;          
    } 
  
    isMounted = false;
    witnessesJson;
    numberOfTotalWitnesses = 0;
    numberOfPersonnelWitnesses = 0;
    numberOfCivilianWitnesses = 0;
    numberOfExpertWitnesses = 0;
    sortBy = 'Name';
    sortDesc = false;
    witnessList: any[] = [];
    witnessCounts: any[] = [];
    selectedType = 'Required Only';

    witnessFields = [
        {key:'Name',         sortable:true, tdClass: 'border-top',  headerStyle:'text-primary',   cellStyle:'text-danger'},
        {key:'Type',         sortable:true, tdClass: 'border-top',  headerStyle:'text-primary',   cellStyle:'text'},
        {key:'Required',     sortable:true, tdClass: 'border-top',  headerStyle:'text-primary',   cellStyle:'text-white bg-danger font-weight-bold'}
    ];

    witnessCountsFields = [
        {key:"WitnessCountFieldName", tdClass: 'border-top',label: "Witness Count Field Name"},
        {key:"WitnessCountValue", tdClass: 'border-top',label: "Witness Count Value"}
    ];

    witnessDropDownFields = ['All Witnesses', 'Required Only', 'Personnel Only', 'Civilian Only', 'Expert Only']
  
    public ExtractWitnessInfo(): void {
        
        for (const witnessIndex in this.witnessesJson) {
            const witnessInfo = {};
            const jWitness = this.witnessesJson[witnessIndex];
            
            witnessInfo["First Name"] = jWitness.givenNm ? jWitness.givenNm : '';
            witnessInfo["Last Name"] = jWitness.lastNm ? jWitness.lastNm : '';
            this.UpdateDisplayName({'lastName': witnessInfo["Last Name"], 'givenName': witnessInfo["First Name"]});
            witnessInfo["Name"] = this.displayName;            
            witnessInfo["Type"] = jWitness.witnessTypeDsc? jWitness.witnessTypeDsc : '';
            witnessInfo["Required"] = jWitness.requiredYN == "Y"? 'Required': '';
            witnessInfo["Agency"] = jWitness.agencyDsc? jWitness.agencyDsc: '';
            witnessInfo["Pin Code"] = jWitness.pinCodeTxt? jWitness.pinCodeTxt: '';
            if (jWitness.witnessTypeCd) {
                if (jWitness.witnessTypeCd == 'PO' || jWitness.witnessTypeCd == 'PRO') {
                    this.numberOfPersonnelWitnesses += 1;
                    witnessInfo["Type Category"] = 'Personnel'
                } else if (jWitness.witnessTypeCd == 'CIV') {
                    this.numberOfCivilianWitnesses += 1;
                    witnessInfo["Type Category"] = 'Civilian'
                } else if (jWitness.witnessTypeCd == 'EXP') {
                    this.numberOfExpertWitnesses += 1;
                    witnessInfo["Type Category"] = 'Expert'
                }
            }                                   
            this.witnessList.push(witnessInfo);
        }
        this.numberOfTotalWitnesses = this.witnessList.length;
        let countInfo = {};
        countInfo['WitnessCountFieldName'] = "Personnel Witnesses";
        countInfo['WitnessCountValue'] = this.numberOfPersonnelWitnesses;
        this.witnessCounts.push(countInfo);
        countInfo = {};
        countInfo['WitnessCountFieldName'] = "Civilian Witnesses";
        countInfo['WitnessCountValue'] = this.numberOfCivilianWitnesses;
        this.witnessCounts.push(countInfo);
        countInfo = {};
        countInfo['WitnessCountFieldName'] = "Expert Witnesses";
        countInfo['WitnessCountValue'] = this.numberOfExpertWitnesses;
        this.witnessCounts.push(countInfo);
        countInfo = {};
        countInfo['WitnessCountFieldName'] = "Total";
        countInfo['WitnessCountValue'] = this.numberOfTotalWitnesses;
        this.witnessCounts.push(countInfo);
    }

    public totalBackground(item, type){
        if (item.WitnessCountFieldName == 'Total') {
            return 'table-warning'
        }
        return
    }

    get filteredWitnessList() {
        return this.witnessList.filter(witness => {
            if (witness["Required"] == 'Required' && this.selectedType == 'Required Only') {
                return true
            } else if (witness["Type Category"] == 'Personnel' && this.selectedType == 'Personnel Only') {
                return true
            } else if (witness["Type Category"] == 'Civilian' && this.selectedType == 'Civilian Only') {
                return true
            } else if (witness["Type Category"] == 'Expert' && this.selectedType == 'Expert Only') {
                return true
            } else if (this.selectedType == 'All Witnesses') {
                return true;
            }
        })
    }

}
</script>

<style scoped>
 .card {
        border: white;
    }
</style>