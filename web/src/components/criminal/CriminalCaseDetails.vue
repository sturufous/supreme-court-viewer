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
            <criminal-adjudicator-restrictions v-if="showCaseDetails"/>
            <criminal-crown-information v-if="showCaseDetails"/>
            <criminal-crown-notes v-if="showCaseDetails"/>
            <criminal-past-appearances v-if="showPastAppearances" />
            <criminal-future-appearances v-if="showFutureAppearances" />
            <criminal-documents-view v-if="showDocuments"/>
            <criminal-witnesses v-if="showWitnesses" />
            <criminal-sentence-details v-if="showSentenceOrderDetails"/>
            <b-card><br></b-card>  
        </b-col>
    </b-row>
</body>
</template>

<script lang="ts">
import { Component, Vue } from 'vue-property-decorator';
import { namespace } from 'vuex-class';
import CriminalDocumentsView from '@components/criminal/CriminalDocumentsView.vue';
import CriminalHeaderTop from '@components/criminal/CriminalHeaderTop.vue';
import CriminalHeader from '@components/criminal/CriminalHeader.vue';
import CriminalSidePanel from '@components/criminal/CriminalSidePanel.vue';
import CriminalParticipants from '@components/criminal/CriminalParticipants.vue';
import CriminalAdjudicatorRestrictions from '@components/criminal/CriminalAdjudicatorRestrictions.vue'
import CriminalCrownInformation from '@components/criminal/CriminalCrownInformation.vue';
import CriminalPastAppearances from '@components/criminal/CriminalPastAppearances.vue'
import CriminalFutureAppearances from '@components/criminal/CriminalFutureAppearances.vue'
import CriminalCrownNotes from '@components/criminal/CriminalCrownNotes.vue';
import CriminalWitnesses from '@components/criminal/CriminalWitnesses.vue';
import CriminalSentenceDetails from '@components/criminal/CriminalSentenceDetails.vue';
import '@store/modules/CriminalFileInformation';
const criminalState = namespace('CriminalFileInformation');

@Component({
    components: {
        CriminalDocumentsView,
        CriminalSidePanel,
        CriminalHeaderTop,
        CriminalHeader,
        CriminalParticipants,
        CriminalAdjudicatorRestrictions,
        CriminalCrownInformation,
        CriminalPastAppearances,
        CriminalFutureAppearances,
        CriminalCrownNotes,
        CriminalWitnesses,
        CriminalSentenceDetails
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
                    this.ExtractFileInfo()
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
       'Case Details', 'Future Appearances', 'Past Appearances', 'Witnesses', 'Documents', 'Sentence/Order Details'    
    ];
    topTitles = [ 
       'Case Details', 'Future Appearances', 'Past Appearances', 'Witnesses', 'Criminal Documents', 'Criminal Sentences'    
    ];
    
    get selectedSideBar()
    {
        for(const titleInx in this.sidePanelTitles)
        {
          if (this.showSections[this.sidePanelTitles[titleInx]] == true ) return '  '+ this.topTitles[titleInx]
        }
        return ''
    }

    get showCaseDetails()
    {        
        return (this.showSections['Case Details'] && this.isDataReady)
    }
    
    get showDocuments()
    {        
        return (this.showSections['Documents'] && this.isDataReady)
    }

    get showFutureAppearances()
    {        
        return ((this.showSections['Case Details'] || this.showSections['Future Appearances'] ) && this.isDataReady)
    }

    get showPastAppearances()
    {        
        return ((this.showSections['Case Details'] || this.showSections['Past Appearances'] ) && this.isDataReady)
    }

    get showWitnesses()
    {        
        return (this.showSections['Witnesses'] && this.isDataReady)
    }

    get showSentenceOrderDetails()
    {        
        return (this.showSections['Sentence/Order Details'] && this.isDataReady)
    }

    public ExtractFileInfo(): void {
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

<style scoped>
 .card {
        border: white;
    }
</style>