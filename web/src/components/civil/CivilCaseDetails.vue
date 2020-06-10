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
            <span v-if="errorCode==404">This <b>File-Number '{{this.civilFileInformation.fileNumber}}'</b> doesn't exist in the <b>civil</b> records.</span>
            <span v-if="errorCode>405"> Server doesn't respond. <b>({{errorText}})</b> </span>
            <span v-if="errorCode==200"> Bad Data.</span>
        </b-card>
        <b-card>         
            <b-button variant="info" @click="navigateToLandingPage">Back to the Landing Page</b-button>
        </b-card>
    </b-card>

    <b-card no-body>
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

                <civil-parties v-if="showCaseDetails"/>
                <civil-adjudicator-restrictions v-if="showCaseDetails"/>
                <civil-documents-view v-if="showCaseDetails"/>            
                <civil-past-appearances v-if="showPastAppearances" />
                <civil-future-appearances v-if="showFutureAppearances" />
                <b-card><br></b-card>  
            </b-col>
        </b-row>
    </b-card>
</body>
</template>

<script lang="ts">
import { Component, Vue } from 'vue-property-decorator';
import { namespace } from 'vuex-class';
import CivilDocumentsView from '@components/civil/CivilDocumentsView.vue';
import CivilPastAppearances from '@components/civil/CivilPastAppearances.vue';
import CivilFutureAppearances from '@components/civil/CivilFutureAppearances.vue';
import CivilAdjudicatorRestrictions from '@components/civil/CivilAdjudicatorRestrictions.vue';
import CivilParties from '@components/civil/CivilParties.vue';
import CivilHeaderTop from '@components/civil/CivilHeaderTop.vue';
import CivilHeader from '@components/civil/CivilHeader.vue';
import CivilSidePanel from '@components/civil/CivilSidePanel.vue';
import "@store/modules/CommonInformation";
import "@store/modules/CivilFileInformation";
const civilState = namespace("CivilFileInformation");
const commonState = namespace("CommonInformation");

@Component({
    components: {
        CivilAdjudicatorRestrictions,
        CivilDocumentsView,
        CivilPastAppearances,
        CivilFutureAppearances,
        CivilParties,
        CivilSidePanel,
        CivilHeaderTop,
        CivilHeader
    }
})
export default class CivilCaseDetails extends Vue {

    @civilState.State
    public showSections
    
    @commonState.State
    public displayName!: string;

    /* eslint-disable */
    @civilState.State
    public civilFileInformation!: any

    @civilState.Action
    public UpdateCivilFile!: (newCivilFileInformation: any) => void 

    @commonState.Action
    public UpdateDisplayName!: (newInputNames: any) => void

    leftPartiesInfo: any[] = [];
    rightPartiesInfo: any[] = [];
    adjudicatorRestrictionsInfo: any[] = [];
    /* eslint-enable */
    
    isDataReady = false
    isMounted = false
    errorCode =0 ;
    errorText='';
    partiesJson;    
    adjudicatorRestrictionsJson;
    sidePanelTitles = [ 
       'Case Details', 'Future Appearances', 'Past Appearances'    
    ];
    
    mounted () { 
        this.civilFileInformation.fileNumber = this.$route.params.fileNumber
        this.UpdateCivilFile(this.civilFileInformation);        
        this.getFileDetails();
    }

    public getFileDetails(): void {
       
        this.$http.get('/api/files/civil/'+ this.civilFileInformation.fileNumber)
            .then(Response => Response.json(), err => {this.errorCode= err.status;this.errorText= err.statusText;console.log(err);}        
            ).then(data => {
                if(data){
                    this.civilFileInformation.detailsData = data;
                    this.partiesJson = data.party
                    this.adjudicatorRestrictionsJson = data.hearingRestriction; 
                    this.ExtractCaseInfo()
                    if((this.leftPartiesInfo.length> 0)  || (this.rightPartiesInfo.length > 0))
                    {
                        this.civilFileInformation.leftPartiesInfo = this.leftPartiesInfo
                        this.civilFileInformation.rightPartiesInfo = this.rightPartiesInfo
                        this.civilFileInformation.adjudicatorRestrictionsInfo = this.adjudicatorRestrictionsInfo;
                        this.UpdateCivilFile(this.civilFileInformation);                    
                        this.isDataReady = true;
                    }
                }
                this.isMounted = true;
                       
            });
    }    
    
    get selectedSideBar()
    {
        for(const title of this.sidePanelTitles)
        {
          if (this.showSections[title] == true ) return  title
        }
        return ''
    }

    get showCaseDetails()
    {        
        return (this.showSections['Case Details'] && this.isDataReady)
    }

    get showFutureAppearances()
    {        
        return ((this.showSections['Case Details'] || this.showSections['Future Appearances'] ) && this.isDataReady)
    }

    get showPastAppearances()
    {        
        return ((this.showSections['Case Details'] || this.showSections['Past Appearances'] ) && this.isDataReady)
    }    

    public ExtractCaseInfo(): void {        
        for(const jParty of this.partiesJson) {            
            const partyInfo = {};            
            partyInfo["Party ID"] = jParty.partyId;
            partyInfo["Role"] = jParty.roleTypeDescription;
            if (jParty.counsel.length > 0) {
                partyInfo["Counsel"] = []
                for (const couns of jParty.counsel) {                    
                    partyInfo["Counsel"].push(couns.fullNm);
                }
            } else {
                partyInfo["Counsel"] = []
            }             
            partyInfo["Left/Right"] = jParty.leftRightCd;
            partyInfo["First Name"] = jParty.givenNm? jParty.givenNm: '';
            partyInfo["Last Name"] =  jParty.lastNm? jParty.lastNm: jParty.orgNm ;
            this.UpdateDisplayName({'lastName': partyInfo["Last Name"], 'givenName': partyInfo["First Name"]});
            partyInfo["Name"] = this.displayName            
            partyInfo["ID"] = jParty.partyId            
            if (partyInfo["Left/Right"] == "R") {
                this.rightPartiesInfo.push(partyInfo);
            } else {
                this.leftPartiesInfo.push(partyInfo);
            }            
        }
        this.leftPartiesInfo = this.SortParties(this.leftPartiesInfo);
        this.rightPartiesInfo = this.SortParties(this.rightPartiesInfo);

        for (const jRestriction of this.adjudicatorRestrictionsJson) {
            const restrictionInfo = {};     
            restrictionInfo["Adj Restriction"] = jRestriction.adjInitialsTxt?jRestriction.hearingRestrictionTypeDsc+ ": " + jRestriction.adjInitialsTxt:jRestriction.hearingRestrictionTypeDsc;     
            restrictionInfo["Adjudicator"] =   jRestriction.adjInitialsTxt?jRestriction.adjInitialsTxt +" - " + jRestriction.adjFullNm: jRestriction.adjFullNm;
            restrictionInfo["Full Name"] = jRestriction.adjFullNm;
            restrictionInfo["Status"] = jRestriction.hearingRestrictionTypeDsc + ' ';
            restrictionInfo["Applies to"] = jRestriction.applyToNm ? jRestriction.applyToNm: 'All Documents' 
                    
            this.adjudicatorRestrictionsInfo.push(restrictionInfo);      
        }
    }

    public SortParties(partiesList) {
        return partiesList.sort((a, b): any => {
        const LastName1 = a["Last Name"] ? a["Last Name"].toUpperCase() : "";
        const LastName2 = b["Last Name"] ? b["Last Name"].toUpperCase() : "";
        if (LastName1 > LastName2) return 1;
        if (LastName1 < LastName2) return -1;
        return 0;
        });
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