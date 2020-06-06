<template>
<div>
    <b-card bg-variant="white">
        <div>
            <h3 class="mx-2 font-weight-normal"> Adjudicator Restrictions </h3>
            <hr class="mb-2 bg-light" style="height: 5px;"/> 
        </div>

        <b-card v-if="!(adjudicatorRestrictionsInfo.length>0)">
            <span class="text-muted"> No adjudicator restrictions. </span>
        </b-card>

        <b-card bg-variant="white" v-if="isMounted && (adjudicatorRestrictionsInfo.length>0)" no-body>           
            <b-table        
            borderless
            :items="adjudicatorRestrictionsInfo"
            :fields="fields"
            :sort-by.sync="sortBy"
            :sort-desc.sync="sortDesc"
            sort-icon-left
            small
            responsive="sm"
            >   
                <template v-for="(field,index) in fields" v-slot:[`head(${field.key})`]="data">
                    <b v-bind:key="index" :class="field.headerStyle" > {{ data.label }}</b>
                </template>
                <template v-slot:cell(Status)="data" >                   
                    <b-badge 
                        variant="primary" 
                        style="font-weight: normal; font-size: 14px;"> 
                        {{ data.value }}
                    </b-badge>
                </template>
            </b-table>
        </b-card>
    </b-card>
    
</div>
</template>

<script lang="ts">
import { Component, Vue } from "vue-property-decorator";
import { namespace } from "vuex-class";
import "@store/modules/CivilFileInformation";
const civilState = namespace("CivilFileInformation");

@Component
export default class  CivilAdjudicatorRestrictions extends Vue {

  @civilState.State
  public civilFileInformation!: any;

  mounted() {
    this.getAdjudicatorRestrictions();
  }

  public getAdjudicatorRestrictions(): void {      
      this.adjudicatorRestrictionsInfo = this.civilFileInformation.adjudicatorRestrictionsInfo;     
      this.isMounted = true;          
  } 

  sortBy = 'Adjudicator';
  sortDesc = false;  
  adjudicatorRestrictionsInfo: any[] = [];
  isMounted = false;

  fields = [
      {key:'Adjudicator', sortable:true, tdClass: 'border-top',  headerStyle:'text-primary'},       
      {key:'Status',      sortable:true, tdClass: 'border-top',  headerStyle:'text-primary'},
      {key:'Applies to',  sortable:true, tdClass: 'border-top',  headerStyle:'text-primary'}           
  ];  
  
}
</script>

<style scoped>
 .card {
        border: white;
    }
</style>