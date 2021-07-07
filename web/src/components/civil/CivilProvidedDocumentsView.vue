<template>
<div>
   <b-card  v-if= "isMounted" no-body>
        <div>
            <b-row>         
                <h3 class="ml-5 my-1 p-0 font-weight-normal" v-if="!showSections['Provided Documents']"> Provided Documents ({{NumberOfDocuments}}) </h3>
                <custom-overlay :show="!downloadCompleted" style="padding: 0 1rem; margin-left:auto; margin-right:2rem;">
                    <b-button @click="downloadDocuments()" size="sm" variant="success" style="padding: 0 1rem; margin-left:auto; margin-right:2rem;"> Download Selected </b-button>
                </custom-overlay>
            </b-row>
            <hr class="mx-3 bg-light" style="height: 5px;"/>                   
        </div>

        <b-card v-if="!isDataReady && isMounted">
            <span class="text-muted ml-4 mb-5"> No provided documents. </span>
        </b-card>

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
       
        <b-tabs nav-wrapper-class = "bg-light text-dark"
                active-nav-item-class="text-uppercase font-weight-bold text-white bg-primary"                     
                pills
                no-body
                v-if="isDataReady"
                class="mx-3"
                >
            <b-tab 
            v-for="(tabMapping, index) in categories" 
            :key="index"                 
            :title="tabMapping"                 
            v-on:click="switchTab(tabMapping)" 
            v-bind:class="[ activetab === tabMapping ? 'active mb-3' : 'mb-3' ]"
            ></b-tab>
        </b-tabs> 
        
        <b-overlay :show="loadingPdf" rounded="sm">  
            <b-card bg-variant="light" v-if="isDataReady" style="max-height: 500px; overflow-y: auto;" no-body class="mx-3 mb-5">           
                <b-table
                :items="FilteredDocuments"
                :fields="fields[fieldsTab]"
                sort-by="appearanceDate"
                :sort-desc.sync="sortDesc"
                :no-sort-reset="true"
                sort-icon-left
                small
                striped
                responsive="sm"
                >   
                    <template v-for="(field,index) in fields[fieldsTab]" v-slot:[`head(${field.key})`]="data">
                        <b v-bind:key="index" :class="field.headerStyle" > {{ data.label }}</b>
                    </template>

                    <template v-slot:[`cell(${fields[fieldsTab][appearanceDatePlace[fieldsTab]].key})`]="data" >                        
                        <span :style="data.field.cellStyle">
                            {{ data.value | beautify-date}}
                        </span>
                    </template>

                    <template v-slot:[`cell(${fields[fieldsTab][entryDatePlace[fieldsTab]].key})`]="data" >                        
                        <span :style="data.field.cellStyle">
                            {{ data.value | beautify-date-time}}
                        </span>
                    </template>                     

                    <template v-slot:[`cell(${fields[fieldsTab][documentPlace[fieldsTab]].key})`]="data" >
                        <b-button                             
                            variant="outline-primary text-info" 
                            :style="data.field.cellStyle"
                            @click="cellClick(data)"
                            size="sm">
                                {{data.value}}
                        </b-button>                        
                    </template>

                    <template v-slot:head(Select) >                                  
                        <b-form-checkbox                            
                            class="m-0"
                            v-model="allDocumentsChecked"
                            @change="checkAllDocuments"                                                                       					
                            size="sm"/>
                    </template>

                    <template v-slot:cell(Select)="data" >                                  
                        <b-form-checkbox
                            size="sm"
                            class="m-0"
                            :disabled="!data.item.isEnabled"
                            v-model="data.item.isChecked"
                            @change="toggleSelectedDocuments"                                            					
                            />
                    </template>                    

                    <template v-slot:cell(descriptionText)="data" >                               
                        <div
                            :style="data.field.cellStyle"
                            v-b-tooltip.hover                                
                            :title="data.value.length>45? data.value:''">
                            {{data.value | truncate(45)}}
                        </div>
                    </template>                    
                    
                    <template v-slot:cell()="data">                       
                        <span class="ml-2" :style="data.field.cellStyle"> 
                                {{data.value}}
                        </span>
                    </template>
                </b-table>
            </b-card>
            <template v-slot:overlay>               
               <div style="text-align: center"> 
                    <loading-spinner/> 
                    <p id="Downloading-label">Downloading PDF file ...</p>
               </div>                
            </template> 
        </b-overlay>
   </b-card> 
</div>
</template>

<script lang="ts">
import { Component, Vue} from 'vue-property-decorator';
import { namespace } from 'vuex-class';
import base64url from "base64url";
import '@store/modules/CivilFileInformation';
import {civilFileInformationType, referenceDocumentsInfoType} from '../../types/civil';
import { CourtDocumentType, DocumentData } from '../../types/shared';
const civilState = namespace('CivilFileInformation');

import CustomOverlay from "../CustomOverlay.vue"
import shared from "../shared"
import { archiveInfoType, documentRequestsInfoType } from '../../types/common';
enum fieldTab {Categories=0}

@Component({
    components: {
       CustomOverlay
    }
})
export default class CivilProvidedDocumentsView extends Vue {

    @civilState.State
    public showSections
    
    @civilState.State
    public civilFileInformation!: civilFileInformationType

    @civilState.Action
    public UpdateCivilFile!: (newCivilFileInformation: civilFileInformationType) => void

    documents: referenceDocumentsInfoType[] = [];
    documentsDetailsJson;
    loadingPdf = false;
    isMounted = false;
    isDataReady = false;
    activetab = 'ALL';
    sortDesc = false;
    categories: string[] = []; 
    fieldsTab = fieldTab.Categories;
    documentPlace = [2]
    appearanceDatePlace = [3]
    entryDatePlace = [4]
    selectedDocuments = {} as archiveInfoType; 
    downloadCompleted = true;
    allDocumentsChecked = false;     

    fields = [ 
        [
            {key:'Select',label:'',sortable:false,  headerStyle:'text-primary',  cellStyle:'font-size: 16px;', tdClass: 'border-top', thClass:''},
            {key:'partyName',                label:'Party Name',  sortable:true,  headerStyle:'text-primary',  cellStyle:'font-size: 16px;'},
            {key:'referenceDocumentTypeDsc', label:'Document Type',  sortable:false,  headerStyle:'text-primary',  cellStyle:'border:0px; font-size: 16px;text-align:left;'},
            {key:'appearanceDate',           label:'Appearance Date', sortable:true,  headerStyle:'text',   cellStyle:'font-size: 16px;'},
            {key:'enterDtm',                 label:'Created Date', sortable:true,  headerStyle:'text',   cellStyle:'font-size: 16px;'},
            {key:'descriptionText',          label:'Description', sortable:false, headerStyle:'text',          cellStyle:'font-size: 12px;'}
        ]         
    ];

    public getDocuments(): void {        
        this.documents = this.civilFileInformation.referenceDocumentInfo;
        this.categories = this.civilFileInformation.providedDocumentCategories;
        this.categories.sort()
        if(this.categories.indexOf("ALL") < 0) this.categories.unshift("ALL")
        if (this.documents.length > 0){
            this.isDataReady = true;
        }        
        this.isMounted = true;
    }

    mounted () {    
        this.getDocuments();
        this.downloadCompleted = true
        this.selectedDocuments = {zipName: "", csrRequests: [], documentRequests: [], ropRequests: []}        
    }

    public downloadDocuments(){

        const fileName = shared.generateFileName(CourtDocumentType.CivilZip, {
            location: this.civilFileInformation.detailsData.homeLocationAgencyName,
            courtLevel: this.civilFileInformation.detailsData.courtLevelCd,
            fileNumberText:  this.civilFileInformation.detailsData.fileNumberTxt
        }).replace('documents', 'provided-documents');

        this.selectedDocuments = {zipName: fileName, csrRequests: [], documentRequests: [], ropRequests: []};
        for(const doc of this.documents){
            if (doc.isChecked && doc.isEnabled) {
                const id = doc.objectGuid;                
                const documentRequest = {} as documentRequestsInfoType;
                documentRequest.isCriminal = false;
                const documentData: DocumentData  = {
                    appearanceDate: Vue.filter('beautify-date')(doc.appearanceDate),
                    courtLevel: this.civilFileInformation.detailsData.courtLevelCd,
                    documentDescription: doc.descriptionText,
                    documentId: id,
                    fileNumberText:  this.civilFileInformation.detailsData.fileNumberTxt,
                    location: this.civilFileInformation.detailsData.homeLocationAgencyName,
                    partyName: doc.partyName
                };
                documentRequest.pdfFileName = shared.generateFileName(CourtDocumentType.ProvidedCivil, documentData); 
                documentRequest.base64UrlEncodedDocumentId = base64url(id);
                documentRequest.fileId = this.civilFileInformation.fileNumber;
                this.selectedDocuments.documentRequests.push(documentRequest);                
            }        
        }        

        if(this.selectedDocuments.documentRequests.length>0){
            const options =  {
                responseType: "blob",
                headers: {
                "Content-Type": "application/json",
                }
            }
            this.downloadCompleted = false
            this.$http.post('api/files/archive',this.selectedDocuments, options )
            .then( response =>{
                const blob = response.data;
                const link = document.createElement("a");
                link.href = URL.createObjectURL(blob);
                document.body.appendChild(link);
                link.download = this.selectedDocuments.zipName;
                link.click();
                setTimeout(() => URL.revokeObjectURL(link.href), 1000);
                this.downloadCompleted = true;
            }, err =>{ console.log(err); this.downloadCompleted = true;})
        }
    }

    public checkAllDocuments(checked){

        if ( this.activetab != 'ALL' ) {
            for(const docInx in this.documents){
                if (this.documents[docInx].referenceDocumentTypeDsc.includes(this.activetab) && this.documents[docInx].isEnabled) {
                    this.documents[docInx].isChecked = checked
                }        
            }                  
        } else {
            for(const docInx in this.documents){
                if (this.documents[docInx].isEnabled) {
                    this.documents[docInx].isChecked = checked
                }        
            } 
        }  
    }

    public switchTab(tabMapping) {        
        this.allDocumentsChecked = false;
        this.activetab = tabMapping;
    }

    public toggleSelectedDocuments() {  
        Vue.nextTick(()=>{
            const checkedDocs = this.documents.filter(doc=>{return doc.isChecked})
            const enabledDocs = this.documents.filter(doc=>{return doc.isEnabled})
            if(checkedDocs.length == enabledDocs.length)
                this.allDocumentsChecked = true
            else
                this.allDocumentsChecked = false
        })        
	}

    public cellClick(eventData)
    {  
        this.loadingPdf = true;
        const documentData: DocumentData = {
            appearanceDate: Vue.filter('beautify-date')(eventData.item.appearanceDate),
            courtClass: this.civilFileInformation.detailsData.courtClassCd,
            courtLevel: this.civilFileInformation.detailsData.courtLevelCd, 
            documentId: eventData.item.objectGuid,
            documentDescription: eventData.item.referenceDocumentTypeDsc,
            fileId: this.civilFileInformation.fileNumber,
            fileNumberText: this.civilFileInformation.detailsData.fileNumberTxt,
            partyName: eventData.item.partyName,
            location: this.civilFileInformation.detailsData.homeLocationAgencyName
        }
        shared.openDocumentsPdf(CourtDocumentType.ProvidedCivil, documentData);
        this.loadingPdf = false;
    }

    public navigateToLandingPage() {
        this.$router.push({name:'Home'})
    } 

    get FilteredDocuments() {
         
        return this.documents.filter(doc => {   
            if ( this.activetab != 'ALL' )
            {
                if (doc.referenceDocumentTypeDsc.includes(this.activetab)) {                        
                    return true;
                }                                                                      
                                
                return false;                     
            }
            else
            {
                return true;
            }
        });  
    }
    
    get NumberOfDocuments() {
        return(this.documents.length)
    }
}
</script>

<style scoped>
 .card {
        border: white;
    }

</style>