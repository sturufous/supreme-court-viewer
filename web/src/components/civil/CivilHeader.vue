<template>
<body>
    <b-navbar type="white" variant="white" v-if="isMounted">
      <b-navbar-nav>

        <b-nav-text class="text-info mt-1 mr-2" style="font-size: 12px;">
            <b-icon icon="file-earmark-text"></b-icon>
            {{fileNumberText}}
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
        </b-nav-text>

        <b-nav-text class="text-info">
            <b>{{getNameOfPartyTrunc()}}</b>
            and {{(partiesList.length-1)}} other(s)
        </b-nav-text>

        <b-nav-item-dropdown class="mr-3" text right>
            <b-dropdown-item-button
                v-for="(party, index) in SortedParties"
                :key="index"
                v-on:click="activeparty = index"
            >{{getNameOfParty(index)}}</b-dropdown-item-button>
        </b-nav-item-dropdown>

        <b-nav-text style="font-size: 14px;" variant="white">
            <b-badge pill variant="danger">{{adjudicatorRestrictions.length}}</b-badge> Adjudicator Restrictions
        </b-nav-text>

        <b-nav-item-dropdown right  v-if="(adjudicatorRestrictions.length>0)">            
            <b-dropdown-item-button        
            v-for="(restriction, index) in adjudicatorRestrictions"
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
      this.adjudicatorRestrictionsJson = data.hearingRestriction;
      this.partiesJson = data.party 
      this.ExtractPartiesInfo();
      this.isMounted = true;          
  } 

  maximumFullNameLength = 17;
  activeParty = 0;
  numberOfParties = 0;
  fileNumberText;
  agencyLocation = {Name:'', Code:0, Region:'' };
  adjudicatorRestrictionsJson;
  isMounted = false;
  partiesJson;

  partiesList: any[] = [];
  adjudicatorRestrictions: any[] = [];
 

  public ExtractPartiesInfo(): void {
    for (const fileIndex in this.partiesJson) {
      const fileInfo = {};
      const jFile = this.partiesJson[fileIndex];
      fileInfo["Index"] = fileIndex;
      fileInfo["First Name"] = jFile.givenNm ? jFile.givenNm : "";
      fileInfo["Last Name"] = jFile.lastNm ? jFile.lastNm : jFile.orgNm;
      this.partiesList.push(fileInfo);
    }
    this.numberOfParties = this.partiesList.length - 1;

    for (const jRestriction of this.adjudicatorRestrictionsJson) {
      const restrictionInfo = {};     
      restrictionInfo["Adj Restriction"] = jRestriction.adjInitialsTxt?jRestriction.hearingRestrictionTypeDsc+ ": " + jRestriction.adjInitialsTxt:jRestriction.hearingRestrictionTypeDsc;
      restrictionInfo["Full Name"] = jRestriction.adjFullNm;      
      this.adjudicatorRestrictions.push(restrictionInfo);      
    }
  }

  public getNameOfParty(num) {
    return (
      this.partiesList[num]["Last Name"] +
      ", " +
      this.partiesList[num]["First Name"]
    );
  }

  public getNameOfPartyTrunc() {

    const nameOfParty = this.getNameOfParty(this.activeParty);

    if(nameOfParty.length > this.maximumFullNameLength)   
        return nameOfParty.substr(0, this.maximumFullNameLength) +'. ';    
    else 
        return  nameOfParty;
     
  }

  get SortedParties() {
    return this.partiesList.sort((a, b): any => {
      const LastName1 = a["Last Name"] ? a["Last Name"].toUpperCase() : "";
      const LastName2 = b["Last Name"] ? b["Last Name"].toUpperCase() : "";
      if (LastName1 > LastName2) return 1;
      if (LastName1 < LastName2) return -1;
      return 0;
    });
  }
}
</script>