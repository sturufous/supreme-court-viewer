<template>
<div>
    <b-navbar type="white" variant="white" v-if="isMounted" style="height:45px">
      <b-navbar-nav>

        <b-nav-text class="text-primary mr-2" style="margin-top: 6px; font-size: 12px;">
            <b-icon icon="file-earmark-text"></b-icon>
            {{fileNumberText}}
        </b-nav-text>

        <b-nav-text
            class="mt-2 ml-1 mr-2"
            style="font-size: 11px;">
              {{agencyLocation.Name}} 
              <span v-if="agencyLocation.Code"> 
                ({{agencyLocation.Code}}) 
              </span>
        </b-nav-text>

        <b-nav-text class="text-muted mr-3 mt-2" style="font-size: 11px;">
            {{agencyLocation.Region}}
        </b-nav-text>       

        <b-nav-item-dropdown class="mr-3 mt-1" right no-caret size="sm">
            <template v-slot:button-content>
                <b-button
                    variant="outline-primary text-info" 
                    style="transform: translate(0,-4px); border:0px; font-size:16px"
                    size="sm">
                    <b-icon class="mr-2" icon="person-fill"></b-icon>
                    <b>{{getNameOfParticipantTrunc()}}</b> and {{(participantList.length-1)}} other(s)
                    <b-icon class="ml-1" icon="caret-down-fill" font-scale="1"></b-icon>
                </b-button>
            </template>
            <b-dropdown-item-button
                v-for="participant in SortedParticipants"
                :key="participant['Index']"
                v-on:click="setActiveParticipantIndex(participant['Index'])"
            >{{participant['Name']}}</b-dropdown-item-button>
        </b-nav-item-dropdown>

        <b-nav-text style="margin-top: 4px;font-size: 14px;" variant="white">
            <b-badge pill variant="danger">{{adjudicatorRestrictions.length}}</b-badge>
        </b-nav-text>

        <b-nav-item-dropdown right no-caret > 
            <template v-slot:button-content>
                <b-button
                    :variant="(adjudicatorRestrictions.length>0)? 'outline-primary text-info':'white'" 
                    :disabled="adjudicatorRestrictions.length==0"
                    style="transform: translate(-5px,0); border:0px; font-size:14px;text-overflow: ellipsis;"                    
                    size="sm">                    
                    Adjudicator Restrictions
                    <b-icon v-if="(adjudicatorRestrictions.length>0)" class="ml-1" icon="caret-down-fill" font-scale="1"></b-icon>
                </b-button>
            </template>       

            <b-dropdown-item-button                      
              v-for="(restriction, index) in adjudicatorRestrictions"
              :key="index">
                  <b-button style="font-size: 14px; padding: 5px 5px;" 
                            variant="secondary" 
                            v-b-tooltip.hover.left 
                            :title='restriction["Full Name"]'>
                      {{restriction["Adj Restriction"]}}
                  </b-button>
            </b-dropdown-item-button>
        </b-nav-item-dropdown>

        <b-nav-text v-if="bans.length>0" style="margin-top: 4px;font-size: 14px;" variant="white">
            <b-badge pill variant="danger">{{bans.length}}</b-badge>
        </b-nav-text>

        <b-nav-item-dropdown v-if="bans.length>0" right no-caret > 
            <template v-slot:button-content>
                <b-button
                    variant="outline-primary text-info"
                    style="transform: translate(-5px,0); border:0px; font-size:14px;text-overflow: ellipsis;"                    
                    size="sm">                    
                    Ban Details
                    <b-icon class="ml-1" icon="caret-down-fill" font-scale="1"></b-icon>
                </b-button>
            </template>       

            <b-dropdown-item-button>
                <b-card bg-variant="white" no-body border-variant="white">           
                    <b-table        
                    borderless
                    striped
                    :items="bans"
                    :fields="fields"
                    small
                    responsive="sm"
                    >
                    <template v-slot:[`cell(${fields[0].key})`]="data" >
                        <span v-if="data.item.Comment.length == 0">{{ data.value }}</span>
                        <span
                            class="text-success"
                            v-else
                            v-b-tooltip.hover.left
                            v-b-tooltip.hover.html="data.item.Comment"> 
                                {{ data.value }} 
                        </span>
                    </template>
                    </b-table>
                </b-card>                  
            </b-dropdown-item-button>
        </b-nav-item-dropdown>

      </b-navbar-nav>
    </b-navbar>

    <hr class="mx-3 bg-warning" style="border-top: 2px double #FCBA19"/> 
      
</div>
</template>

<script lang="ts">
import { Component, Vue } from "vue-property-decorator";
import * as _ from 'underscore';
import { namespace } from "vuex-class";
import "@store/modules/CriminalFileInformation";
import "@store/modules/CommonInformation";
const criminalState = namespace("CriminalFileInformation");
const commonState = namespace("CommonInformation");

@Component
export default class CriminalHeader extends Vue {  

  @criminalState.State
  public activeCriminalParticipantIndex
  
  @commonState.State
  public displayName!: string;

  /* eslint-disable */
  @criminalState.State
  public criminalFileInformation!: any;

  @criminalState.Action
  public UpdateActiveCriminalParticipantIndex!: (newActiveCriminalParticipantIndex: any) => void

  @commonState.Action
  public UpdateDisplayName!: (newInputNames: any) => void

  participantList: any[] = [];
  adjudicatorRestrictions: any[] = [];
  bans: any[] = [];
  /* eslint-enable */

  maximumFullNameLength = 15;
  numberOfParticipants = 0;
  fileNumberText;
  agencyLocation = {Name:'', Code:0, Region:'' };
  adjudicatorRestrictionsJson;
  isMounted = false;
  participantJson;
  
  fields =  
  [
      {key:'Ban Participant', sortable:false, tdClass: 'border-top',  headerStyle:'table-borderless text-primary'},       
      {key:'Ban Type',        sortable:false, tdClass: 'border-top',  headerStyle:'text-primary'},
      {key:'Order Date',      sortable:false, tdClass: 'border-top',  headerStyle:'text-primary'},
      {key:'Act',             sortable:false, tdClass: 'border-top',  headerStyle:'text-primary'},       
      {key:'Sub',             sortable:false, tdClass: 'border-top',  headerStyle:'text-primary'},
      {key:'Description',     sortable:false, tdClass: 'border-top',  headerStyle:'text-primary'},
           
  ];

  mounted() {
    this.getHeaderInfo();
  }

  public getHeaderInfo(): void {      
      const data = this.criminalFileInformation.detailsData;
      this.fileNumberText = data.fileNumberTxt;      
      this.agencyLocation.Name = data.homeLocationAgencyName;
      this.agencyLocation.Code = data.homeLocationAgencyCode;
      this.agencyLocation.Region = data.homeLocationRegionName;
      this.adjudicatorRestrictions = this.criminalFileInformation.adjudicatorRestrictionsInfo;
      this.participantList = this.criminalFileInformation.participantList
      this.bans = this.criminalFileInformation.bans
      this.numberOfParticipants = this.participantList.length - 1;
      this.isMounted = true; 
      this.setActiveParticipantIndex(this.SortedParticipants[0].Index)
  }

  public setActiveParticipantIndex(index)
  {                   
      this.UpdateActiveCriminalParticipantIndex(index);  
  }	

  public getNameOfParticipant(num) {        
      this.UpdateDisplayName({'lastName': this.participantList[num]["Last Name"], 'givenName': this.participantList[num]["First Name"]});
      return this.displayName;
  }

  public getNameOfParticipantTrunc() {

    const nameOfParticipant = this.getNameOfParticipant(this.activeCriminalParticipantIndex);

    if(nameOfParticipant.length > this.maximumFullNameLength)   
        return nameOfParticipant.substr(0, this.maximumFullNameLength) +' ... ';    
    else 
        return  nameOfParticipant;
     
  }

  get SortedParticipants()
  {         
      return _.sortBy(this.participantList,(participant=>{return (participant["Last Name"]? participant["Last Name"].toUpperCase() : '')}))       
  }

}
</script>