<template>
<body>
  <b-card bg-variant="white" border-variant="white" v-if="isMounted">
    <b-navbar type="white" variant="white">
      <b-navbar-nav>
        <b-nav-text class="text-info mt-1 mr-2" style="font-size: 12px;">
            <b-icon icon="file-earmark-text"></b-icon>
            {{fileNumberText}}
        </b-nav-text>

        <b-nav-text
            class="mt-1 ml-1 mr-2"
            style="font-size: 11px;">
            Robson Square Provincial Court (2045)
        </b-nav-text>

        <b-nav-text class="text-muted mr-3 mt-1" style="font-size: 11px;">Vancouver</b-nav-text>

        <b-nav-text class="mr-2">
            <b-icon icon="person-fill"></b-icon>
        </b-nav-text>

        <b-nav-text class="text-info">
            <b>{{getNameOfParticipant(activeparticipant)}}</b>
            and {{(participantList.length-1)}} other(s)
        </b-nav-text>

        <b-nav-item-dropdown class="mr-3" text right>
            <b-dropdown-item
                v-for="(participant, index) in SortedParticipants"
                :key="index"
                v-on:click="activeparticipant = index"
            >{{getNameOfParticipant(index)}}</b-dropdown-item>
        </b-nav-item-dropdown>

        <b-nav-text style="font-size: 14px;" variant="white">
            <b-badge variant="danger">{{adjudicatorRes}}</b-badge> Adjudicator Restrictions
        </b-nav-text>

        <b-nav-item-dropdown right>
            <b-dropdown-item>None</b-dropdown-item>
        </b-nav-item-dropdown>
      </b-navbar-nav>
    </b-navbar>
  </b-card>
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
    this.getDocuments();
  }

  public getDocuments(): void {
    const data = this.criminalFileInformation.detailsData;
    this.fileNumberText = data.fileNumberTxt;
    this.participantJson = data.participant;
    this.ExtractDocumentInfo();
    this.isMounted = true;
  }

  activeparticipant = 0;
  numberOfParticipants = 0;
  fileNumberText;
  isMounted = false;
  participantJson;
  participantList: any[] = [];
  adjudicatorRes = " 0 ";

  public ExtractDocumentInfo(): void {
    for (const fileIndex in this.participantJson) {
      const fileInfo = {};
      const jFile = this.participantJson[fileIndex];
      fileInfo["Index"] = fileIndex;
      fileInfo["First Name"] = jFile.givenNm ? jFile.givenNm : "_noGivenname";
      fileInfo["Last Name"] = jFile.lastNm ? jFile.lastNm : "_noLastname";
      this.participantList.push(fileInfo);
    }
    this.numberOfParticipants = this.participantList.length - 1;
  }

  public getNameOfParticipant(num) {
    return (
      this.participantList[num]["Last Name"] +
      ", " +
      this.participantList[num]["First Name"]
    );
  }

  get getNumberOfParticipants() {
    return (
      this.getNameOfParticipant(this.activeparticipant) +
      "and " +
      (this.participantList.length - 1) +
      " other(s)"
    );
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