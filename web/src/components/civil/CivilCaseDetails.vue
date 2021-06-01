<template>
<div style="overflow:hidden"> 
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
            <span v-else-if="errorCode==200 || errorCode==204"> Bad Data in <b>File-Number '{{this.civilFileInformation.fileNumber}}'</b>.</span>
            <span v-else-if="errorCode==403"> You are not authorized to access this file. </span>
            <span v-else> Server is not responding. <b>({{errorText}})</b> </span>
        </b-card>
        <b-card>         
            <b-button id="backToLandingPage" variant="info" @click="navigateToLandingPage">Back to the Landing Page</b-button>
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

                <b-row v-if="showAllDocuments">
                    <h2 style= "white-space: pre" v-if="isDataReady">
                        {{selectedSideBar}}
                    </h2>         
                    <custom-overlay v-if="isDataReady" :show="!downloadCompleted" style="padding: 0 1rem; margin-left:auto; margin-right:2rem;">
                        <b-button @click="downloadAllDocuments()" size="md" variant="info" style="padding: 0 1rem; margin-left:auto; margin-right:2rem;"> Download All Documents </b-button>
                    </custom-overlay>
                </b-row> 

                <h2 v-if="!showAllDocuments && isDataReady" style= "white-space: pre">
                    {{selectedSideBar}}
                </h2>

                <civil-parties v-if="showCaseDetails"/>
                <!-- Disable for now, as per request by Lorne, although they may be used for Provincial files.
                <civil-adjudicator-restrictions v-if="showCaseDetails"/>
                -->
                <civil-comment-notes v-if="showCaseDetails"/>
                <civil-documents-view v-if="showDocuments || showAllDocuments"/>
                <civil-provided-documents-view v-if="showProvidedDocuments || showAllDocuments"/>            
                <civil-past-appearances v-if="showPastAppearances" />
                <civil-future-appearances v-if="showFutureAppearances" />
                <b-card><br></b-card>  
            </b-col>
        </b-row>
    </b-card>
    <b-modal v-if= "isMounted" v-model="showSealedWarning" id="bv-modal-ban" hide-header hide-footer>        
        <b-card v-if="isSealed"> 
            This file has been sealed. Only authorized users are permitted access to sealed files.
        </b-card>
        <b-card v-else-if="docIsSealed"> 
            This File contains one or more Sealed Documents.
        </b-card>                     
        <b-button class="mt-3 bg-primary" @click="$bvModal.hide('bv-modal-ban')">Continue</b-button>
    </b-modal>
</div>
</template>

<script lang="ts">
import { Component, Vue } from 'vue-property-decorator';
import { namespace } from 'vuex-class';
import * as _ from 'underscore';
import CivilDocumentsView from '@components/civil/CivilDocumentsView.vue';
import CivilProvidedDocumentsView from '@components/civil/CivilProvidedDocumentsView.vue';
import CivilPastAppearances from '@components/civil/CivilPastAppearances.vue';
import CivilFutureAppearances from '@components/civil/CivilFutureAppearances.vue';
import CivilAdjudicatorRestrictions from '@components/civil/CivilAdjudicatorRestrictions.vue';
import CivilCommentNotes from '@components/civil/CivilCommentNotes.vue'; 
import CivilParties from '@components/civil/CivilParties.vue';
import CivilHeaderTop from '@components/civil/CivilHeaderTop.vue';
import CivilHeader from '@components/civil/CivilHeader.vue';
import CivilSidePanel from '@components/civil/CivilSidePanel.vue';
import {civilFileInformationType, partiesInfoType, documentsInfoType, summaryDocumentsInfoType, referenceDocumentsInfoType, csrRequestsInfoType} from '../../types/civil';
import {inputNamesType, adjudicatorRestrictionsInfoType, archiveInfoType, documentRequestsInfoType } from '../../types/common'
import "@store/modules/CommonInformation";
import "@store/modules/CivilFileInformation";
import CustomOverlay from "../CustomOverlay.vue"
import { civilReferenceDocumentJsonType } from '../../types/civil/jsonTypes';
import base64url from 'base64url';
import shared from '../shared';
import { CourtDocumentType, DocumentData } from '../../types/shared';
const civilState = namespace("CivilFileInformation");
const commonState = namespace("CommonInformation");

@Component({
    components: {
        CivilAdjudicatorRestrictions,
        CivilCommentNotes,
        CivilDocumentsView,
        CivilProvidedDocumentsView,
        CivilPastAppearances,
        CivilFutureAppearances,
        CivilParties,
        CivilSidePanel,
        CivilHeaderTop,
        CivilHeader,
        CustomOverlay
    }
})
export default class CivilCaseDetails extends Vue {

    @civilState.State
    public showSections
    
    @commonState.State
    public displayName!: string;
    
    @civilState.State
    public civilFileInformation!: civilFileInformationType

    @civilState.Action
    public UpdateCivilFile!: (newCivilFileInformation: civilFileInformationType) => void 

    @commonState.Action
    public UpdateDisplayName!: (newInputNames: inputNamesType) => void

    leftPartiesInfo: partiesInfoType[] = [];
    rightPartiesInfo: partiesInfoType[] = [];
    adjudicatorRestrictionsInfo: adjudicatorRestrictionsInfoType[] = [];
    documentsInfo: documentsInfoType[] = [];
    providedDocumentsInfo: referenceDocumentsInfoType[] = [];
    summaryDocumentsInfo: summaryDocumentsInfoType[] = [];
    
    isDataReady = false;
    isMounted = false;
    downloadCompleted = true;
    isSealed = false;
    docIsSealed = false;
    showSealedWarning = false;
    errorCode =0 ;
    errorText='';
    partiesJson;    
    adjudicatorRestrictionsJson;
    documentsDetailsJson;
    providedDocumentsDetailsJson: civilReferenceDocumentJsonType[] = [];
    categories: string[] = [];
    providedDocumentCategories: string[] = [];
    sidePanelTitles = [ 
       'Case Details', 'Future Appearances', 'Past Appearances', 'All Documents', 'Documents', 'Provided Documents'   
    ];
    
    mounted () {
        this.civilFileInformation.fileNumber = this.$route.params.fileNumber
        this.UpdateCivilFile(this.civilFileInformation);        
        this.getFileDetails();
    }

    public getFileDetails(): void {
       
        this.errorCode=0
        this.$http.get('api/files/civil/'+ this.civilFileInformation.fileNumber)
            .then(Response => Response.json(), err => {this.errorCode= err.status;this.errorText= err.statusText;console.log(err);}        
            ).then(data => {
                if(data){
                    this.civilFileInformation.detailsData = data;
                    this.partiesJson = data.party
                    this.adjudicatorRestrictionsJson = data.hearingRestriction;
                    this.documentsDetailsJson = data.document;
                    this.providedDocumentsDetailsJson = data.referenceDocument;
                    if (data.sealedYN == "Y") {
                        this.isSealed = true;
                    } 
                    this.ExtractCaseInfo()
                    if ((this.adjudicatorRestrictionsInfo.length>0) || 
                        (this.leftPartiesInfo.length> 0)  || (this.rightPartiesInfo.length > 0)
                        || (this.documentsInfo.length>0) || (this.summaryDocumentsInfo.length>0))
                    {
                        this.civilFileInformation.leftPartiesInfo = this.leftPartiesInfo;
                        this.civilFileInformation.rightPartiesInfo = this.rightPartiesInfo;
                        this.civilFileInformation.isSealed = this.isSealed;                        
                        this.civilFileInformation.adjudicatorRestrictionsInfo = this.adjudicatorRestrictionsInfo;
                        this.civilFileInformation.documentsInfo = this.documentsInfo;
                        this.civilFileInformation.summaryDocumentsInfo = this.summaryDocumentsInfo;
                        this.civilFileInformation.referenceDocumentInfo = this.providedDocumentsInfo;
                        this.civilFileInformation.categories = this.categories;
                        this.civilFileInformation.providedDocumentCategories = this.providedDocumentCategories;
                        this.UpdateCivilFile(this.civilFileInformation);
                        if (this.isSealed || this.docIsSealed) {
                            this.showSealedWarning = true;
                        }                    
                        this.isDataReady = true;
                    }
                    else
                        this.errorCode=200;                    
                }
                else
                    if(this.errorCode==0) this.errorCode=200;
                this.isMounted = true;                       
            });
    }
    
    public downloadAllDocuments(){

        const fileName = shared.generateFileName(CourtDocumentType.CivilZip, {
            location: this.civilFileInformation.detailsData.homeLocationAgencyName,
            courtLevel: this.civilFileInformation.detailsData.courtLevelCd,
            fileNumberText:  this.civilFileInformation.detailsData.fileNumberTxt
        });
        
        const documentsToDownload = { zipName: fileName, csrRequests: [], documentRequests: [], ropRequests: [], vcCivilFileId: this.civilFileInformation.fileNumber } as archiveInfoType;
        for(const doc of this.providedDocumentsInfo){
            if (doc.isEnabled) {
                const id = doc.objectGuid;                
                const documentRequest = {} as documentRequestsInfoType;
                documentRequest.isCriminal = false;
                const documentData: DocumentData  = {
                    appearanceDate: Vue.filter('beautify-date')(doc.appearanceDate), 
                    courtLevel: this.civilFileInformation.detailsData.courtLevelCd,
                    documentDescription: doc.descriptionText,
                    documentId: id,
                    fileId: this.civilFileInformation.fileNumber,
                    fileNumberText:  this.civilFileInformation.detailsData.fileNumberTxt,
                    location: this.civilFileInformation.detailsData.homeLocationAgencyName,
                    partyName: doc.partyName
                };
                documentRequest.pdfFileName = shared.generateFileName(CourtDocumentType.ProvidedCivil, documentData);
                documentRequest.base64UrlEncodedDocumentId = base64url(id);
                documentRequest.fileId = this.civilFileInformation.fileNumber;
                documentsToDownload.documentRequests.push(documentRequest);                
            }        
        }
        
        for(const doc of this.documentsInfo){
            if (doc.isEnabled) {
                const id = doc["Document ID"]                
                const documentRequest = {} as documentRequestsInfoType;
                documentRequest.isCriminal = false;
                const documentData: DocumentData  = { 
                    courtLevel: this.civilFileInformation.detailsData.courtLevelCd,
                    dateFiled: Vue.filter('beautify-date')(doc["Date Filed"]),
                    documentDescription: doc["Document Type"],
                    documentId: id,
                    fileId:this.civilFileInformation.fileNumber,
                    fileNumberText:  this.civilFileInformation.detailsData.fileNumberTxt,
                    location: this.civilFileInformation.detailsData.homeLocationAgencyName
                };
                documentRequest.pdfFileName = shared.generateFileName(CourtDocumentType.Civil, documentData);
                documentRequest.base64UrlEncodedDocumentId = base64url(id);
                documentRequest.fileId = this.civilFileInformation.fileNumber;
                documentsToDownload.documentRequests.push(documentRequest);                
            }        
        }

        for(const doc of this.summaryDocumentsInfo){
            if (doc.isEnabled) {                
                const id = doc["Appearance ID"];                      
                const csrRequest = {} as csrRequestsInfoType;
                csrRequest.appearanceId = id;
                const documentData: DocumentData  = { 
                    appearanceDate: Vue.filter('beautify-date')(doc["Appearance Date"]),
                    appearanceId: id,
                    courtLevel: this.civilFileInformation.detailsData.courtLevelCd,
                    documentDescription: doc["Document Type"],
                    fileNumberText:  this.civilFileInformation.detailsData.fileNumberTxt,
                    fileId: this.civilFileInformation.fileNumber,
                    location: this.civilFileInformation.detailsData.homeLocationAgencyName
                };
                csrRequest.pdfFileName = shared.generateFileName(CourtDocumentType.CSR, documentData);
                documentsToDownload.csrRequests.push(csrRequest);
            }        
        }

        if(documentsToDownload.csrRequests.length>0 || documentsToDownload.documentRequests.length>0){
            const options =  {
                responseType: "blob",
                headers: {
                "Content-Type": "application/json",
                }
            }
            this.downloadCompleted = false
            this.$http.post('api/files/archive',documentsToDownload, options )
            .then( response =>{
                const blob = response.data;
                const link = document.createElement("a");
                link.href = URL.createObjectURL(blob);
                document.body.appendChild(link);
                link.download = documentsToDownload.zipName;
                link.click();
                setTimeout(() => URL.revokeObjectURL(link.href), 1000);
                this.downloadCompleted = true;
            }, err =>{this.downloadCompleted = true;})
        }
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
    
    get showProvidedDocuments()
    {        
        return ((this.showSections['Case Details'] || this.showSections['Provided Documents'] ) && this.isDataReady)
    }

    get showDocuments()
    {        
        return ((this.showSections['Case Details'] || this.showSections['Documents'] ) && this.isDataReady)
    }

    get showAllDocuments()
    {        
        return ((this.showSections['Case Details'] || this.showSections['All Documents'] ) && this.isDataReady)
    }


    public ExtractCaseInfo(): void {
        let partyIndex = 0       
        for(const jParty of this.partiesJson) {                        
            const partyInfo = {} as partiesInfoType;            
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
            partyInfo["Index"] = partyIndex
            partyIndex = partyIndex + 1            
            if (partyInfo["Left/Right"] == "R") {
                this.rightPartiesInfo.push(partyInfo);
            } else {
                this.leftPartiesInfo.push(partyInfo);
            }            
        }
        this.leftPartiesInfo = this.SortParties(this.leftPartiesInfo);
        this.rightPartiesInfo = this.SortParties(this.rightPartiesInfo);

        for (const jRestriction of this.adjudicatorRestrictionsJson) {
            const restrictionInfo = {} as adjudicatorRestrictionsInfoType;     
            restrictionInfo["Adj Restriction"] = jRestriction.adjInitialsTxt?jRestriction.hearingRestrictionTypeDsc+ ": " + jRestriction.adjInitialsTxt:jRestriction.hearingRestrictionTypeDsc;     
            restrictionInfo["Adjudicator"] =   jRestriction.adjInitialsTxt?jRestriction.adjInitialsTxt +" - " + jRestriction.adjFullNm: jRestriction.adjFullNm;
            restrictionInfo["Full Name"] = jRestriction.adjFullNm;
            restrictionInfo["Status"] = jRestriction.hearingRestrictionTypeDsc + ' ';
            restrictionInfo["Applies to"] = jRestriction.applyToNm ? jRestriction.applyToNm: 'All Documents' 
                    
            this.adjudicatorRestrictionsInfo.push(restrictionInfo);      
        }   

        for(const docIndex in this.documentsDetailsJson)
        {             
            const jDoc =  this.documentsDetailsJson[docIndex];            
            if(jDoc.documentTypeCd != 'CSR') {
                const docInfo = {} as documentsInfoType;
                docInfo["Index"] = docIndex;
                docInfo["Seq."] = jDoc.fileSeqNo;
                docInfo["Document Type"] = jDoc.documentTypeDescription;
                docInfo["Concluded"] = jDoc.concludedYn;
                if((this.categories.indexOf("CONCLUDED") < 0) && docInfo["Concluded"].toUpperCase() =="Y") this.categories.push("CONCLUDED")        
                docInfo["Next Appearance Date"] = jDoc.nextAppearanceDt? Vue.filter('beautify-date')(jDoc.nextAppearanceDt) : ''; 
                if(docInfo["Next Appearance Date"].length > 0 && this.categories.indexOf("SCHEDULED") < 0) this.categories.push("SCHEDULED")   

                docInfo["Category"] = jDoc.category? jDoc.category : '';
                if((this.categories.indexOf(docInfo["Category"]) < 0) && docInfo["Category"].length > 0) this.categories.push(docInfo["Category"])
                docInfo["Act"] = [];            
                if (jDoc.documentSupport && jDoc.documentSupport.length > 0) {
                    for (const act of jDoc.documentSupport) {
                        docInfo["Act"].push({'Code': act.actCd, 'Description': act.actDsc})
                    }
                }                
                if (jDoc.sealedYN == "Y") {
                    this.docIsSealed = true;
                    docInfo["Sealed"] = true;
                } else {
                    docInfo["Sealed"] = false;
                }
                docInfo["Document ID"] = jDoc.civilDocumentId;            
                docInfo["PdfAvail"] = jDoc.imageId? true : false 
                docInfo["Date Filed"] = jDoc.filedDt? jDoc.filedDt.split(' ')[0] : '';
                docInfo["Issues"] = [];
                if (jDoc.issue && jDoc.issue.length > 0) {
                    for (const issue of jDoc.issue) {
                        docInfo["Issues"].push(issue.issueDsc)
                    }
                }
                docInfo["Comment"] = jDoc.commentTxt? jDoc.commentTxt : '';
                docInfo["Filed By Name"] = [];
                if (jDoc.filedBy && jDoc.filedBy[0] && jDoc.filedBy.length > 0) {
                    for (const filed of jDoc.filedBy) {
                        if (filed.roleTypeCode){
                            docInfo["Filed By Name"].push(filed.filedByName + ' (' + filed.roleTypeCode + ')');
                        } else {
                            docInfo["Filed By Name"].push(filed.filedByName);
                        }                        
                    }
                } 
                docInfo["Order Made Date"] = jDoc.DateGranted? Vue.filter('beautify-date')(jDoc.DateGranted) : '';                
                docInfo.isChecked = false;
                docInfo.isEnabled = docInfo["PdfAvail"] && !docInfo["Sealed"];
                
                this.documentsInfo.push(docInfo);                

            } else {
                const docInfo = {} as summaryDocumentsInfoType;
                docInfo["Index"] = docIndex;                
                docInfo["Document Type"] = 'CourtSummary';
                docInfo["Appearance Date"] = jDoc.lastAppearanceDt.split(' ')[0];
                docInfo["Appearance ID"] = jDoc.imageId;
                docInfo["PdfAvail"] = jDoc.imageId? true : false
                docInfo.isChecked = false;
                docInfo.isEnabled = docInfo["PdfAvail"];
                this.summaryDocumentsInfo.push(docInfo);
            }
        } 

        for(const providedDocIndex in this.providedDocumentsDetailsJson)
        {             
            const jDoc =  this.providedDocumentsDetailsJson[providedDocIndex];            
            const providedDocInfo = {} as referenceDocumentsInfoType;
            providedDocInfo.partyId = jDoc.PartyId;
            providedDocInfo.appearanceId = jDoc.AppearanceId;
            providedDocInfo.partyName = jDoc.PartyName;
            providedDocInfo.appearanceDate = jDoc.AppearanceDate;
            providedDocInfo.descriptionText = jDoc.DescriptionText;
            providedDocInfo.enterDtm = jDoc.EnterDtm;
            providedDocInfo.referenceDocumentTypeDsc = jDoc.ReferenceDocumentTypeDsc;
            providedDocInfo.objectGuid = jDoc.ObjectGuid;
            providedDocInfo.isChecked = false;
            providedDocInfo.isEnabled = jDoc.ObjectGuid? true:false;
            if((this.providedDocumentCategories.indexOf(providedDocInfo.referenceDocumentTypeDsc) < 0) && providedDocInfo.referenceDocumentTypeDsc.length > 0) {
                this.providedDocumentCategories.push(providedDocInfo.referenceDocumentTypeDsc);
            }
            this.providedDocumentsInfo.push(providedDocInfo);          
        }
    }

    public SortParties(partiesList) {
        return _.sortBy(partiesList,((party: partiesInfoType) =>{return (party["Last Name"]? party["Last Name"].toUpperCase() : '')}))        
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
    body {
        overflow-x: hidden;
    }
</style>