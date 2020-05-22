<template>
<body>
  
    <b-navbar type="white" variant="white" v-if="isMounted">
      <b-navbar-nav>
            <b-nav-text style="font-size: 14px;" variant="white">
                <b-badge class="mr-1" variant="primary">{{courtClassDescription}}</b-badge>
                <b-badge class="mr-1" variant="danger">{{indictable}}</b-badge>                
                <b-badge variant="secondary">{{courtLevelDescription}}</b-badge>
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
export default class CriminalHeader extends Vue {
  
  @criminalState.State
  public criminalFileInformation!: any;

  mounted() {
    this.getDocuments();
  }

  public getDocuments(): void {      
      const data = this.criminalFileInformation.detailsData;
      this.courtClassDescription = data.courtClassDescription;      
      this.courtLevelDescription = data.courtLevelDescription;
      this.indictable = data.indictableYN =='Y'? 'Indictable' : 'Summarily';
     
      this.isMounted = true;          
  } 

  courtLevelDescription;
  courtClassDescription;
  indictable;
  isMounted = false;
 
}
</script>