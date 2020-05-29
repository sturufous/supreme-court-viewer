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
            <span>This <b>File-Number '{{this.civilFileInformation.fileNumber}}'</b> doesn't exist in the <b>civil</b> records. </span>
        </b-card>
        <b-card>         
            <b-button variant="info" @click="navigateToLandingPage">Back to the Landing Page</b-button>
        </b-card>
    </b-card>

    <b-row cols="2" >            
        <b-col md="2" cols="2" style="overflow: auto;">
            <civil-side-panel v-if="isDataReady"/> 
        </b-col>
        <b-col col md="10" cols="10" style="overflow: auto;">

            <civil-header-top v-if="isDataReady"/> 
            <civil-header v-if="isDataReady"/> 

            <h2 style= "white-space: pre" v-if="isDataReady">
                {{selectedSideBar}}
            </h2>


            <civil-documents-view v-if="showDocuments"/>
            <b-card><br></b-card>  
        </b-col>
    </b-row>
</body>
</template>

<script lang="ts">
import { Component, Vue } from 'vue-property-decorator';
import { namespace } from 'vuex-class';
import CivilDocumentsView from '@components/civil/CivilDocumentsView.vue';
import CivilHeaderTop from '@components/civil/CivilHeaderTop.vue';
import CivilHeader from '@components/civil/CivilHeader.vue';
import CivilSidePanel from '@components/civil/CivilSidePanel.vue';
import '@store/modules/CivilFileInformation';
const civilState = namespace('CivilFileInformation');

@Component({
    components: {
        CivilDocumentsView,
        CivilSidePanel,
        CivilHeaderTop,
        CivilHeader
    }
})
export default class CivilCaseDetails extends Vue {

    @civilState.State
    public civilFileInformation!: any

    @civilState.Action
    public UpdateCivilFile!: (newCivilFileInformation: any) => void
    
    @civilState.State
    public showSections    
    
    mounted () { 
        this.civilFileInformation.fileNumber = this.$route.params.fileNumber
        this.UpdateCivilFile(this.civilFileInformation);        
        this.getFileDetails();
    }

    public getFileDetails(): void {
       
        this.$http.get('/api/files/civil/'+ this.civilFileInformation.fileNumber)
            .then(Response => Response.json(), err => {console.log(err);}        
            ).then(data => {
                if(data){
                    this.civilFileInformation.detailsData = data;
                    console.log(data) 
                    this.partiesJson = data.party                
                    this.UpdateCivilFile(this.civilFileInformation);               
                    this.ExtractPartiesInfo()
                    if(this.partiesInfo.length)
                    {                    
                        this.isDataReady = true;
                    }
                }
                this.isMounted = true;
                       
            });
    }

    isDataReady = false
    isMounted = false
    partiesJson;
    partiesInfo: any[] = [];
    sidePanelTitles = [ 
       'Case Details', 'Future Appearances', 'Past Appearances'    
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
        return ((this.showSections['Case Details'] || this.showSections['Civil Documents'] ) && this.isDataReady)
    }

    get showFutureAppearances()
    {        
        return ((this.showSections['Case Details'] || this.showSections['Future Appearances'] ) && this.isDataReady)
    }

    get showPastAppearances()
    {        
        return ((this.showSections['Case Details'] || this.showSections['Past Appearances'] ) && this.isDataReady)
    }    

    public ExtractPartiesInfo(): void {
        console.log(this.partiesJson)
        for(const jFile of this.partiesJson)
        {            
            const fileInfo = {};            
            fileInfo["Part ID"] = jFile.partId;
            fileInfo["Prof Seq No"] = jFile.profSeqNo;
            fileInfo["First Name"] = jFile.givenNm? jFile.givenNm: '_noGivenname';
            fileInfo["Last Name"] =  jFile.lastNm? jFile.lastNm: '_noLastname' ; 
            this.partiesInfo.push(fileInfo);
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