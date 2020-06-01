<template>
<body>
  
    <b-navbar type="white" variant="white" v-if="isMounted">
      <b-navbar-nav>
            <b-nav-text style="font-size: 14px;" variant="white">
                <b-badge class="mr-1" variant="primary">{{courtClassDescription}}</b-badge>
                <b-badge class="mr-1" variant="danger">{{indictable}}</b-badge>                
                <b-badge class="mr-1" variant="secondary">{{courtLevelDescription}}</b-badge>
                <b-badge v-if="hasAssignmentTypeDsc" class="mr-1" variant="success">{{assignmentTypeDescription}}</b-badge>
                <b-badge v-if="hasCrownNotesToJCM" variant="danger">Crown Notes to JCM</b-badge>                
            </b-nav-text>
      </b-navbar-nav>
    </b-navbar>

</body>
</template>

<script lang="ts">
import { Component, Vue } from "vue-property-decorator";
import { namespace } from "vuex-class";
import "@store/modules/CriminalFileInformation";
const criminalState = namespace("CriminalFileInformation");

@Component
export default class CriminalHeaderTop extends Vue {
  
  @criminalState.State
  public criminalFileInformation!: any;

  mounted() {
    this.getFileDescription();
  }

  public getFileDescription(): void {      
      const data = this.criminalFileInformation.detailsData;
      this.courtClassDescription = data.courtClassDescription;      
      this.courtLevelDescription = data.courtLevelDescription;
      if (data.assignmentTypeDsc && data.assignmentTypeDsc.length > 0) {
        this.assignmentTypeDescription = data.assignmentTypeDsc;
        this.hasAssignmentTypeDsc = true;
      }
      this.hasCrownNotesToJCM = data.trialRemark.length > 0;
      this.indictable = data.indictableYN =='Y'? 'Indictable' : 'Summarily';
     
      this.isMounted = true;          
  } 

  courtLevelDescription;
  courtClassDescription;
  indictable;
  assignmentTypeDescription;
  hasCrownNotesToJCM = false;
  hasAssignmentTypeDsc = false;
  isMounted = false;
 
}
</script>