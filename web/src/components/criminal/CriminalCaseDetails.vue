<template>
<body> 

    <b-card bg-variant="light" v-if= "!isMounted && !isDataReady">
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

    <b-card bg-variant="light" v-if= "isMounted && !isDataReady">
        <b-card  style="min-height: 100px;">
            <span>This <b>File-Number '{{this.criminalFileInformation.fileNumber}}'</b> doesn't exist in the <b>criminal</b> records. </span>
        </b-card>
        <b-card>         
            <b-button variant="info" @click="navigateToLandingPage">Back to the Landing Page</b-button>
        </b-card>
    </b-card>

    <b-row cols="2" >            
        <b-col md="2" cols="2" style="overflow: auto;">
            <criminal-side-panel v-if="isDataReady"/> 
        </b-col>
        <b-col col md="10" cols="10" style="overflow: auto;">
            <criminal-header-top v-if="isDataReady"/> 
            <criminal-header v-if="isDataReady"/> 

            <h2 style= "white-space: pre" v-if="isDataReady">
                {{selectedSideBar}}
            </h2>

            <criminal-participants v-if="showCaseDetails"/>
            <adjudicator-restrictions v-if="showCaseDetails"/>            
            <criminal-documents-view v-if="showDocuments"/>  
        </b-col>
    </b-row>
</body>
</template>

<script lang="ts">
import { Component, Vue } from 'vue-property-decorator';
import { namespace } from 'vuex-class';
import CriminalDocumentsView from '@components/criminal/CriminalDocumentsView.vue';
import CriminalHeaderTop from './CriminalHeaderTop.vue';
import CriminalHeader from './CriminalHeader.vue';
import CriminalSidePanel from './CriminalSidePanel.vue';
import CriminalParticipants from './CriminalParticipants.vue'
import AdjudicatorRestrictions from './AdjudicatorRestrictions.vue'

import '@store/modules/CriminalFileInformation';
const criminalState = namespace('CriminalFileInformation');

@Component({
    components: {
        CriminalDocumentsView,
        CriminalSidePanel,
        CriminalHeaderTop,
        CriminalHeader,
        CriminalParticipants,
        AdjudicatorRestrictions
    }
})
export default class CriminalCaseDetails extends Vue {

    @criminalState.State
    public criminalFileInformation!: any

    @criminalState.Action
    public UpdateCriminalFile!: (newCriminalFileInformation: any) => void
    
    @criminalState.State
    public showSections    
    
    mounted () { 
        this.criminalFileInformation.fileNumber = this.$route.params.fileNumber
        this.UpdateCriminalFile(this.criminalFileInformation);        
        this.getFileDetails();
    }

    public getFileDetails(): void {
       
        this.$http.get('/api/files/criminal/'+ this.criminalFileInformation.fileNumber)
            .then(Response => Response.json(), err => {console.log(err);}        
            ).then(data => {
                if(data){
                    this.criminalFileInformation.detailsData = data; 
                    this.participantJson = data.participant                
                    this.UpdateCriminalFile(this.criminalFileInformation);               
                    this.ExtractDocumentInfo()
                    if(this.participantFiles.length)
                    {                    
                        this.isDataReady = true;
                    }
                }
                this.isMounted = true;
                       
            });
    }

    isDataReady = false
    isMounted = false
    participantJson;
    participantFiles: any[] = [];
    sidePanelTitles = [ 
       'Case Details', 'Future Appearance', 'Past Appearance', 'Witnesses', 'Criminal Documents', 'Sentence/Order Details'    
    ];
    
    get selectedSideBar()
    {
        for(const title of this.sidePanelTitles)
        {
          if (this.showSections[title] == true ) return '   '+ title
        }
        return ''
    }

    get showCaseDetails()
    {        
        return (this.showSections['Case Details'] && this.isDataReady)
    }
    
    get showDocuments()
    {        
        return ((this.showSections['Case Details'] || this.showSections['Criminal Documents'] ) && this.isDataReady)
    }

    get showFutureAppearance()
    {        
        return ((this.showSections['Case Details'] || this.showSections['Future Appearance'] ) && this.isDataReady)
    }

    get showPastAppearance()
    {        
        return ((this.showSections['Case Details'] || this.showSections['Past Appearance'] ) && this.isDataReady)
    }

    get showWitnesses()
    {        
        return ((this.showSections['Case Details'] || this.showSections['Witnesses'] ) && this.isDataReady)
    }

    get showSentenceOrderDetails()
    {        
        return ((this.showSections['Case Details'] || this.showSections['Sentence/Order Details'] ) && this.isDataReady)
    }

    public ExtractDocumentInfo(): void {
        for(const jFile of this.participantJson)
        {            
            const fileInfo = {};            
            fileInfo["Part ID"] = jFile.partId;
            fileInfo["Prof Seq No"] = jFile.profSeqNo;
            fileInfo["First Name"] = jFile.givenNm? jFile.givenNm: '_noGivenname';
            fileInfo["Last Name"] =  jFile.lastNm? jFile.lastNm: '_noLastname' ; 
            this.participantFiles.push(fileInfo);
        }
    }

    public navigateToLandingPage() {
        this.$router.push({name:'Home'})
    }
    
    
}
</script>