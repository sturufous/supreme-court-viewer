<template>
<body>
    <b-navbar type="white" variant="white" v-if="isMounted">
      <b-navbar-nav>

        <b-nav-text class="mt-1 mr-2" style="font-size: 14px;">
            <b-icon icon="file-earmark-text"></b-icon>
            <span style="color: #32CD32; font-weight: bold;">   {{fileNumberText}}</span>
        </b-nav-text>

        <b-nav-text
            class="mt-1 ml-1 mr-2"
            style="font-size: 11px;">
              {{agencyLocation.Name}} 
              <span v-if="agencyLocation.Code"> 
                ({{agencyLocation.Code}}) 
              </span>
        </b-nav-text>

        <b-nav-text class="text-muted mr-3 mt-1" style="font-size: 11px;">
            {{agencyLocation.Region}}
        </b-nav-text>

        <b-nav-text class="mr-2">
            <b-icon icon="person-fill"></b-icon>
            <span   variant="text-info"
                    v-b-tooltip.hover.bottomleft 
                    :title='partyDisplayedTxt'>
                    {{getNameOfPartyTrunc()}}
            </span>
        </b-nav-text>

        <b-dropdown class="mr-4" right variant="white">            
            <b-dropdown-item-button
                disabled
                v-for="(party, index) in leftPartiesInfo"
                :key="index"
                v-on:click="activeparty = index"
            >{{party["Name"]}}</b-dropdown-item-button>
            <b-dropdown-divider></b-dropdown-divider>
            <b-dropdown-item-button 
                disabled
                v-for="(party, index) in rightPartiesInfo"
                :key="index"
                v-on:click="activeparty = index"
            >{{party["Name"]}}</b-dropdown-item-button>
        </b-dropdown>     

        <b-nav-text style="font-size: 14px;" variant="white">
            <b-badge pill variant="danger">{{adjudicatorRestrictionsInfo.length}}</b-badge> Adjudicator Restrictions
        </b-nav-text>

        <b-nav-item-dropdown right  v-if="(adjudicatorRestrictionsInfo.length>0)">            
            <b-dropdown-item-button        
            v-for="(restriction, index) in adjudicatorRestrictionsInfo"
            :key="index">
                <b-button style="font-size: 12px; padding: 0px 2px;" 
                          variant="primary" 
                          v-b-tooltip.hover 
                          :title='restriction["Full Name"]'>
                    {{restriction["Adj Restriction"]}}
                </b-button>
            </b-dropdown-item-button>
        </b-nav-item-dropdown>
      </b-navbar-nav>
    </b-navbar>

    <hr class="mx-3  bg-info" style="height: 2px;"/>  
      
</body>
</template>

<script lang="ts">
import { Component, Vue } from "vue-property-decorator";
import { namespace } from "vuex-class";
import "@store/modules/CivilFileInformation";
const civilState = namespace("CivilFileInformation");

@Component
export default class CriminalHeader extends Vue {

  @civilState.State
  public civilFileInformation!: any;

  mounted() {
    this.getHeaderInfo();
  }

  public getHeaderInfo(): void {      
      const data = this.civilFileInformation.detailsData;
      this.fileNumberText = data.fileNumberTxt;      
      this.agencyLocation.Name = data.homeLocationAgencyName;
      this.agencyLocation.Code = data.homeLocationAgencyCode;
      this.agencyLocation.Region = data.homeLocationRegionName;
      this.partyDisplayedTxt = data.socTxt;
      this.leftPartiesInfo = this.civilFileInformation.leftPartiesInfo;
      this.rightPartiesInfo = this.civilFileInformation.rightPartiesInfo; 
      this.adjudicatorRestrictionsInfo = this.civilFileInformation.adjudicatorRestrictionsInfo;
      this.isMounted = true;          
  } 

  maximumFullNameLength = 47;
  activeParty = 0;
  fileNumberText;
  agencyLocation = {Name:'', Code:0, Region:'' };
  adjudicatorRestrictionsJson;
  isMounted = false;
  partyDisplayedTxt;
  leftPartiesInfo;
  rightPartiesInfo; 
  adjudicatorRestrictionsInfo;

  public getNameOfPartyTrunc() {
    if(this.partyDisplayedTxt.length > this.maximumFullNameLength)   
        return this.partyDisplayedTxt.substr(0, this.maximumFullNameLength) +',...';    
    else 
        return  this.partyDisplayedTxt;
  }
  
}
</script>