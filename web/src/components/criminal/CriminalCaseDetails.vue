<template>
<body>


    <b-row cols="2" >
            
        <b-col md="2" cols="2" style="overflow: auto;">
            <criminal-side-panel/> 
        </b-col>        

        <b-col col md="10" cols="10" style="overflow: auto;"> 
            <criminal-header/>       
            <criminal-documents-view v-if="showDocuments"></criminal-documents-view>   
        </b-col>

    </b-row>
</body>
    
</template>

<script lang="ts">
import { Component, Vue } from 'vue-property-decorator';
import { namespace } from 'vuex-class';
import CriminalDocumentsView from '@components/criminal/CriminalDocumentsView.vue';
import CriminalHeader from './CriminalHeader.vue';
import CriminalSidePanel from './CriminalSidePanel.vue';

import '@store/modules/CriminalFileInformation';
const criminalState = namespace('CriminalFileInformation');

@Component({
    components: {
        CriminalDocumentsView,
        CriminalSidePanel,
        CriminalHeader
    }
})
export default class CriminalCaseDetails extends Vue {

    @criminalState.State
    public criminalFileInformation!: any

    @criminalState.Action
    public UpdateCriminalFile!: (newCriminalFileInformation: any) => void
    
    @criminalState.State
    public showCaseDetails

    @criminalState.State
    public showFutureAppearances

    @criminalState.State
    public showPastAppearances

    @criminalState.State
    public showWitnesses
    
    @criminalState.State
    public showDocuments

    @criminalState.State
    public showSentenceOrderDetails
    
    mounted () { 
        this.criminalFileInformation.fileNumber = this.$route.params.fileNumber
        this.UpdateCriminalFile(this.criminalFileInformation);        
        this.getFileDetails();  
    }

    public getFileDetails(): void {
       
        this.$http.get('api/files/criminal/'+ this.criminalFileInformation.fileNumber)
            .then(Response => Response.json(), err => {console.log(err);}        
            ).then(data => {
                this.criminalFileInformation.detailsData = data;
                this.UpdateCriminalFile(this.criminalFileInformation);
            });
    }
    
}
</script>