<template>
<body>

    <b-card bg-variant="white">
        <div>
            <h3 class="mx-2"> Adjudicator Restrictions </h3>
            <hr class="mx-1 bg-light" style="height: 5px;"/> 
        </div>

        <b-card bg-variant="white">           
            <b-table        
            borderless
            :items="adjudicatorRestrictions"
            :fields="fields"
            :sort-by.sync="sortBy"
            :sort-desc.sync="sortDesc"
            responsive="sm"
            >   
                <template v-for="(field,index) in fields" v-slot:[`head(${field.key})`]="data">
                    <b v-bind:key="index" :class="field.headerStyle" > {{ data.label }}</b>
                </template>
                <template v-for="(field,index) in fields" v-slot:[`cell(${field.key})`]="data" >                   
                    <span v-bind:key="index" :class="field.cellStyle" style= "white-space: pre" > {{ data.value }}</span>
                </template>
            </b-table>
        </b-card>
    </b-card>
    
</body>
</template>

<script lang="ts">
import { Component, Vue } from "vue-property-decorator";
import { namespace } from "vuex-class";
import "@store/modules/CriminalFileInformation";
const criminalState = namespace("CriminalFileInformation");

@Component
export default class  AdjudicatorRestrictions extends Vue {

  @criminalState.State
  public criminalFileInformation!: any;

  mounted() {
    this.getAdjudicatorRestrictions();
  }

  public getAdjudicatorRestrictions(): void {      
      const data = this.criminalFileInformation.detailsData;         
      this.adjudicatorRestrictionsJson = data.hearingRestriction;
      this.ExtractAdjudicatorRestrictionsInfo();
      this.isMounted = true;          
  } 

  sortBy = 'Adjudicator';
  sortDesc = false;  
  adjudicatorRestrictionsJson;
  isMounted = false;

  adjudicatorRestrictions: any[] = [];

  fields =  
  [
      {key:'Adjudicator', sortable:true, tdClass: 'border-top',  headerStyle:'table-borderless text-primary',   cellStyle:'text'},       
      {key:'Status',      sortable:true, tdClass: 'border-top',  headerStyle:'text-primary',                    cellStyle:'text-white bg-primary'},
      {key:'Applies to',  sortable:true, tdClass: 'border-top',  headerStyle:'text-primary',                    cellStyle:'text'},
           
  ];

  public ExtractAdjudicatorRestrictionsInfo(): void {

    for (const jRestriction of this.adjudicatorRestrictionsJson) {
      const restrictionInfo = {};     
      restrictionInfo["Adjudicator"] =   jRestriction.adjInitialsTxt +" - " + jRestriction.adjFullNm;
      restrictionInfo["Status"] = ' ' +jRestriction.hearingRestrictionTypeDsc + ' ';
      restrictionInfo["Applies to"] = jRestriction.partNm ? jRestriction.partNm: 'All participants on file' 
      this.adjudicatorRestrictions.push(restrictionInfo);      
    }
  }

}
</script>

<style scoped>
 .card {
        border: white;
    }
</style>