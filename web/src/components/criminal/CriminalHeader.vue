<template>
<body>

  <b-card bg-variant="white" border-variant="white">
    <b-navbar type="white" variant="white">
      <b-navbar-nav>
        <b-nav-text class="text-info mr-2" >555-1</b-nav-text>
        <b-nav-text class="mt-1 ml-1 mr-2" style="font-size: 11px;">Robson Square Provincial Court (2045)</b-nav-text>
        <b-nav-text class="text-muted mr-5 mt-1" style="font-size: 11px;">Vancouver</b-nav-text>

        <b-nav-text>
           <b-icon icon="person-fill"></b-icon>
        </b-nav-text>  
        
        <!-- Navbar dropdowns -->
        <b-nav-item-dropdown text="Participants" right>
          <b-dropdown-item href="#">EN</b-dropdown-item>        
        </b-nav-item-dropdown>

        <b-nav-item-dropdown text="Adjudicator Restrictions" right>
          <b-dropdown-item href="#">Account</b-dropdown-item>       
        </b-nav-item-dropdown>
      </b-navbar-nav>
    </b-navbar>
  </b-card>   


</body>
    
</template>

<script lang="ts">
import { Component, Vue } from 'vue-property-decorator';
import { namespace } from 'vuex-class';
import '@store/modules/CriminalFileInformation';
const criminalState = namespace('CriminalFileInformation');

@Component
export default class CriminalHeader extends Vue {
  

  @criminalState.State
  public criminalFileInformation!: any

  @criminalState.Action
  public UpdateCriminalFile!: (newCriminalFileInformation: any) => void
    


    

    
}
</script>