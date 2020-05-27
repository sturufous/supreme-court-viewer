<template>
<body>

    <b-card bg-variant="white">
        <div>
            <h3 class="mx-2 font-weight-normal"> Adjudicator Restrictions </h3>
            <hr class="mx-1 bg-light" style="height: 5px;"/> 
        </div>

        <b-card v-if="!(adjudicatorRestrictions.length>0)">
            <span class="text-muted"> No adjudicator restrictions. </span>
        </b-card>

        <b-card bg-variant="white" v-if="isMounted && (adjudicatorRestrictions.length>0)">           
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
                <template v-slot:cell(Status)="data" >                   
                    <b-badge 
                        variant="primary" 
                        style="font-weight: normal; font-size: 16px;"> 
                        {{ data.value }}
                    </b-badge>
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
      {key:'Adjudicator', sortable:true, tdClass: 'border-top',  headerStyle:'table-borderless text-primary'},       
      {key:'Status',      sortable:true, tdClass: 'border-top',  headerStyle:'text-primary'},
      {key:'Applies to',  sortable:true, tdClass: 'border-top',  headerStyle:'text-primary'},
           
  ];

  public ExtractAdjudicatorRestrictionsInfo(): void {

    for (const jRestriction of this.adjudicatorRestrictionsJson) {
      const restrictionInfo = {};     
      restrictionInfo["Adjudicator"] =   jRestriction.adjInitialsTxt?jRestriction.adjInitialsTxt +" - " + jRestriction.adjFullNm: jRestriction.adjFullNm;
      restrictionInfo["Status"] = jRestriction.hearingRestrictionTypeDsc + ' ';
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