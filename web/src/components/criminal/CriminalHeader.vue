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
            <b>{{getNameOfParticipantTrunc()}}</b>
            and {{(participantList.length-1)}} other(s)
        </b-nav-text>

        <b-nav-item-dropdown class="mr-3" text right>
            <b-dropdown-item-button
                v-for="(participant, index) in SortedParticipants"
                :key="index"
                v-on:click="activeparticipant = index"
            >{{getNameOfParticipant(index)}}</b-dropdown-item-button>
        </b-nav-item-dropdown>

        <b-nav-text style="font-size: 14px;" variant="white">
            <b-badge pill variant="danger">{{adjudicatorRestrictions.length}}</b-badge> Adjudicator Restrictions
        </b-nav-text>

        <b-nav-item-dropdown right  v-if="(adjudicatorRestrictions.length>0)">            
            <b-dropdown-item-button        
            v-for="(restriction, index) in adjudicatorRestrictions"
            :key="index">
                <b-button style="font-size: 10px; padding: 0px 2px; font-weight: bold;" 
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
import "@store/modules/CriminalFileInformation";
const criminalState = namespace("CriminalFileInformation");

@Component
export default class CriminalHeader extends Vue {

  @criminalState.State
  public criminalFileInformation!: any;

  mounted() {
    this.getHeaderInfo();
  }

  public getHeaderInfo(): void {      
      const data = this.criminalFileInformation.detailsData;
      this.fileNumberText = data.fileNumberTxt;      
      this.agencyLocation.Name = data.homeLocationAgencyName;
      this.agencyLocation.Code = data.homeLocationAgencyCode;
      this.agencyLocation.Region = data.homeLocationRegionName;
      this.adjudicatorRestrictionsJson = data.hearingRestriction;
      this.participantJson = data.participant 
      this.ExtractParticipantInfo();
      this.isMounted = true;          
  } 

  maximumFullNameLength = 17;
  activeparticipant = 0;
  numberOfParticipants = 0;
  fileNumberText;
  agencyLocation = {Name:'', Code:0, Region:'' };
  adjudicatorRestrictionsJson;
  isMounted = false;
  participantJson;

  participantList: any[] = [];
  adjudicatorRestrictions: any[] = [];
 

  public ExtractParticipantInfo(): void {
    for (const fileIndex in this.participantJson) {
      const fileInfo = {};
      const jFile = this.participantJson[fileIndex];
      fileInfo["Index"] = fileIndex;
      fileInfo["First Name"] = jFile.givenNm ? jFile.givenNm : "";
      fileInfo["Last Name"] = jFile.lastNm ? jFile.lastNm : jFile.orgNm;
      this.participantList.push(fileInfo);
    }
    this.numberOfParticipants = this.participantList.length - 1;

    for (const jRestriction of this.adjudicatorRestrictionsJson) {
      const restrictionInfo = {};     
      restrictionInfo["Adj Restriction"] = jRestriction.adjInitialsTxt?jRestriction.hearingRestrictionTypeDsc+ ": " + jRestriction.adjInitialsTxt:jRestriction.hearingRestrictionTypeDsc;
      restrictionInfo["Full Name"] = jRestriction.adjFullNm;      
      this.adjudicatorRestrictions.push(restrictionInfo);      
    }
  }

  public getNameOfParticipant(num) {
    return (
      this.participantList[num]["Last Name"] +
      ", " +
      this.participantList[num]["First Name"]
    );
  }

  public getNameOfParticipantTrunc() {

    const nameOfParticipant = this.getNameOfParticipant(this.activeparticipant);

    if(nameOfParticipant.length > this.maximumFullNameLength)   
        return nameOfParticipant.substr(0, this.maximumFullNameLength) +'. ';    
    else 
        return  nameOfParticipant;
     
  }

  get SortedParticipants() {
    return this.participantList.sort((a, b): any => {
      const LastName1 = a["Last Name"] ? a["Last Name"].toUpperCase() : "";
      const LastName2 = b["Last Name"] ? b["Last Name"].toUpperCase() : "";
      if (LastName1 > LastName2) return 1;
      if (LastName1 < LastName2) return -1;
      return 0;
    });
  }
}
</script>