<template>

    <b-card bg-variant="light" v-if= "!isMounted ">
        <b-overlay :show= "true"> 
            <b-card  style="min-height: 100px;"/>                   
            <template v-slot:overlay>               
            <div> 
                    <loading-spinner/> 
                    <p id="loading-label">Loading ...</p>
            </div>                
            </template> 
        </b-overlay> 
    </b-card>
    <b-card bg-variant="light" v-else>
      <b-card bg-variant="white">
        <b-row cols="2">            
            <b-col md="8" cols="8" style="overflow: auto;">
                <div>
                    <h3 class="mx-2 font-weight-normal"> Charges</h3>
                    <hr class="mb-0 bg-light" style="height: 5px;"/> 
                </div>                           
                <b-table
                :items="appearanceCharges"
                :fields="chargesFields"               
                borderless
                striped               
                responsive="sm"
                >   
                    <template  v-slot:head="data">
                        <b> {{ data.label }}</b>
                    </template>                
                
                    <template v-slot:cell(Finding)="data" >                        
                        <b-badge 
                            variant="secondary" 
                            v-if="data.item['Finding']"                               
                            v-b-tooltip.hover.left 
                            :title='data.item["Finding Description"]'> 
                            {{ data.item["Finding"] }} 
                        </b-badge>                       
                    </template>

                    <template v-slot:cell(LastResult)="data" >
                            <b-badge 
                                variant="secondary"
                                v-if="data.item['Last Result']"
                                v-b-tooltip.hover.left 
                                :title='data.item["Last Result Description"]' > 
                                {{ data.item["Last Result"] }} 
                            </b-badge>
                    </template>

                </b-table>
                
            </b-col>
            <b-col col md="4" cols="4" style="overflow: auto;">
                 <div>
                    <h3 class="mx-2 font-weight-normal"> Additional Info</h3>
                    <hr class="mb-0 bg-light" style="height: 5px;"/> 
                </div>
                           
                <b-table
                :items="appearanceAdditionalInfo"
                :fields="addInfoFields"
                thead-class="d-none"               
                borderless                                  
                responsive="sm"
                > 
                </b-table>
                
            </b-col>          
        </b-row>
      </b-card>  
    </b-card> 

</template>

<script lang="ts">
import { Component, Vue } from "vue-property-decorator";
import { namespace } from "vuex-class";

import "@store/modules/CriminalFileInformation";
const criminalState = namespace("CriminalFileInformation");


@Component
export default class CriminalAppearanceDetails extends Vue {

    @criminalState.State
    public criminalFileInformation!: any;

    @criminalState.State
    public appearanceInfo!: any;

    mounted() {
        this.getAppearanceInfo();
        this.getAppearanceDetails();
    }

    public getAppearanceDetails(): void {      
    
        this.$http.get('/api/files/criminal/'+ this.appearanceDetailsInfo["File Number"]+'/appearance-detail/'+this.appearanceDetailsInfo["Appearance ID"])
            .then(Response => Response.json(), err => {console.log(err);} )        
            .then(data => {
                if(data){  
                    this.appearanceDetailsJson = data;              
                    this.ExtractAppearanceDetailsInfo();
                }
                this.isMounted = true;                       
            }); 
    } 
  
    isMounted = false;
    isDataReady = false;
    appearanceDetailsJson;
    
    sortBy = 'Date';
    sortDesc = true;
    appearanceCharges: any[] = [];
    appearanceDetailsInfo = {};

    appearanceAdditionalInfo: any[] = [];

    addInfoFields =  
    [
        {key:'key',    sortable:false},
        {key:'value',  sortable:false},
    ];   

    chargesFields =  
    [
        {key:'Count',          sortable:false,  tdClass: 'border-top', headerStyle:'text-primary', cellStyle:'text-info'},
        {key:'Criminal Code',  sortable:false,  tdClass: 'border-top', headerStyle:'text-primary', cellStyle:'font-weight-bold'},
        {key:'Description',    sortable:false,  tdClass: 'border-top', headerStyle:'text',         cellStyle:'text'},
        {key:'LastResult',     sortable:false,  tdClass: 'border-top', headerStyle:'text',         cellStyle:'text'},
        {key:'Finding',        sortable:false,  tdClass: 'border-top', headerStyle:'text-primary', cellStyle:'text'},
    ];   
    
    
    public getAppearanceInfo()
    {       
        this.appearanceDetailsInfo["Supplemental Equipment"] = this.appearanceInfo.supplementalEquipmentTxt;
        this.appearanceDetailsInfo["Security Restriction"] = this.appearanceInfo.securityRestrictionTxt;
        this.appearanceDetailsInfo["Out-Of-Town Judge"] =  this.appearanceInfo.outOfTownJudgeTxt;

        for(const info in this.appearanceDetailsInfo)
            this.appearanceAdditionalInfo.push({'key':info,'value':this.appearanceDetailsInfo[info]});

        this.appearanceDetailsInfo["File Number"] = this.appearanceInfo.fileNo; 
        this.appearanceDetailsInfo["Appearance ID"] = this.appearanceInfo.appearanceId;    
    }

    public ExtractAppearanceDetailsInfo()
    {               
        for(const charge of this.appearanceDetailsJson.charges)
        {              
            const chargeInfo = {};             
            chargeInfo["Count"] = charge.printSeqNo;

            chargeInfo["Criminal Code"]= charge.statuteSectionDsc                 
            chargeInfo["Description"]= charge.statuteDsc

            chargeInfo["Last Result"]= charge.appearanceResultCd
            chargeInfo["Last Result Description"]= charge.appearanceResultDesc

            chargeInfo["Finding"]= charge.findingCd
            chargeInfo["Finding Description"]= charge.findingDsc

            this.appearanceCharges.push(chargeInfo);
        }
    }


}
</script>
