<template>

   <b-card  v-if= "isMounted" no-body>
        <div>
            <b-row>         
                <h3 class="mx-4 font-weight-normal"> Documents ({{NumberOfDocuments}}) </h3>
                <custom-overlay :show="!downloadCompleted" style="padding: 0 1rem; margin-left:auto; margin-right:2rem;">
                    <b-button @click="downloadDocuments()" size="sm" variant="success" style="padding: 0 1rem; margin-left:auto; margin-right:2rem;"> Download Selected </b-button>
                </custom-overlay>
            </b-row>
            <hr class="mx-3 mb-0 bg-light" style="height: 5px;"/>         
        </div>
        <b-card>
            <b-tabs  nav-wrapper-class = "bg-light text-dark"
                        active-nav-item-class="text-white bg-primary"                     
                        pills >
                <b-tab 
                v-for="(tabMapping, index) in categories"                 
                :key="index"                 
                :title="tabMapping"                 
                v-on:click="switchTab(tabMapping)" 
                v-bind:class="[ activetab === tabMapping ? 'active' : '' ]"
                ></b-tab>
            </b-tabs>
        </b-card>       
      
        <b-card>
            <b-dropdown  variant="light text-info" :text="getNameOfParticipant(activeCriminalParticipantIndex)" class="m-0">    
                <b-dropdown-item-button  
                    v-for="participant in SortedParticipants" 
                    :key="participant['Index']"
                    v-on:click="setActiveParticipantIndex(participant['Index'])">
                        {{participant['Name']}}                        
                </b-dropdown-item-button> 
            </b-dropdown>                 
        </b-card>

        <b-overlay :show="loadingPdf" rounded="sm">  
            <b-card class="mx-3" bg-variant="light">                          
                <b-table
                v-if="FilteredDocuments.length>0"
                :items="FilteredDocuments"
                :fields="fields[fieldsTab]"
                :sort-by.sync="sortBy"
                :sort-desc.sync="sortDesc"
                :no-sort-reset="true"
                small
                striped
                borderless
                sort-icon-left
                responsive="sm"
                >   
                    <template v-for="(field, index) in fields[fieldsTab]" v-slot:[`head(${field.key})`]="data">
                        <b v-bind:key="index" :class="field.headerStyle" > {{ data.label }}</b>
                    </template>

                    <template  v-slot:head(Date) > 
                         <b class="text-danger" >{{getNameOfDateInTabs}}</b>
                    </template>

                    <template v-slot:head(Select) >                                  
                        <b-form-checkbox                            
                            class="m-0"
                            v-model="allDocumentsChecked"
                            @change="checkAllDocuments"                                                                       					
                            size="sm"/>
                    </template>

                    <template v-slot:[`cell(${fields[fieldsTab][0].key})`]="data" >                                  
                        <b-form-checkbox
                            size="sm"
                            class="m-0"
                            :disabled="!data.item.isEnabled"
                            v-model="data.item.isChecked"
                            @change="toggleSelectedDocuments"                                            					
                            />
                    </template> 

                    <template v-slot:[`cell(${fields[0][1].key})`]="data" >
                         {{ data.value | beautify-date}}
                    </template> 

                    <template v-slot:[`cell(${fields[fieldsTab][documentPlace[fieldsTab]].key})`]="data" >
                        <b-button 
                            v-if="data.item.PdfAvail" 
                            variant="outline-primary text-info" 
                            style="border:0px; font-size:16px"
                            @click="cellClick(data)"
                            size="sm">
                                {{data.value}}
                        </b-button>
                        <span class="ml-2" v-else>
                             {{data.value}}
                        </span>
                    </template>

                </b-table>
                <span v-else class="text-muted ml-4 mb-5"> No document with label <b>{{activetab}}</b>.</span>
            </b-card>
            <template v-slot:overlay>               
               <div style="text-align: center"> 
                    <loading-spinner/> 
                    <p id="Downloading-label">Downloading PDF file ...</p>
               </div>                
            </template> 
        </b-overlay>

   </b-card> 

</template>

<script lang="ts">
import { Component, Vue } from 'vue-property-decorator';
import { namespace } from 'vuex-class';
import * as _ from 'underscore';
import '@store/modules/CriminalFileInformation';
import "@store/modules/CommonInformation";
import {participantFilesInfoType, participantROPInfoType, participantListInfoType, participantDocumentsInfoType, criminalFileInformationType, ropRequestsInfoType} from '../../types/criminal';
import {inputNamesType} from '../../types/common'
import base64url from 'base64url';

import shared from '../shared';
const criminalState = namespace("CriminalFileInformation");
const commonState = namespace("CommonInformation");

import CustomOverlay from "../CustomOverlay.vue"
import { archiveInfoType, documentRequestsInfoType } from '../../types/common';
import { CourtDocumentType, DocumentData } from '../../types/shared';


enum fieldTab {Categories=0, Summary, Bail}

@Component({
    components: {
       CustomOverlay
    }
})
export default class CriminalDocumentsView extends Vue {    

    @criminalState.State
    public activeCriminalParticipantIndex
    
    @commonState.State
    public displayName!: string;    
    
    @criminalState.Action
    public UpdateActiveCriminalParticipantIndex!: (newActiveCriminalParticipantIndex: string) => void

    @criminalState.State
    public criminalFileInformation!: criminalFileInformationType

    @criminalState.Action
    public UpdateCriminalFile!: (newCriminalFileInformation: criminalFileInformationType) => void   

    @commonState.Action
    public UpdateDisplayName!: (newInputNames: inputNamesType) => void

    participantFiles: participantFilesInfoType[] = [];
    participantList: participantListInfoType[] = [];
    categories: string[] = [];   

    courtLevel;
    courtClass;

    message = 'Loading';
    loadingPdf = false;    
    activetab = 'ALL';
    tabIndex = 0;              
    sortBy = 'Date';
    sortDesc = true;
    hoverRow =-1;
    hoverCol = 0;
    isMounted = false
    isDataValid = false     

    fieldsTab = fieldTab.Categories;
    documentPlace = [2,1,2]
    selectedDocuments = {} as archiveInfoType; 
    downloadCompleted = true;
    allDocumentsChecked = false; 

    fields = [ 
        [
            {key:'Select',label:'',sortable:false,  headerStyle:'text-primary',  cellStyle:'font-size: 16px;', tdClass: 'border-top', thClass:''},
            {key:'Date',               sortable:true,   tdClass: 'border-top',  headerStyle:'text-danger'},
            {key:'Document Type',      sortable:true,   tdClass: 'border-top',  cellStyle:'text-align:left;', headerStyle:'text-primary'},
            {key:'Category',           sortable:false,  tdClass: 'border-top',  headerStyle:'text'},
            {key:'Pages',              sortable:false,  tdClass: 'border-top',  headerStyle:'text'},
        ],
        [
            {key:'Select',label:'',sortable:false,  headerStyle:'text-primary',  cellStyle:'font-size: 16px;', tdClass: 'border-top', thClass:''},
            {key:'Document Type',    sortable:false,  tdClass: 'border-top', cellStyle:'text-align:left;', headerStyle:'text-primary'},
            {key:'Category',         sortable:true,   tdClass: 'border-top', headerStyle:'text'},
            {key:'Pages',            sortable:false,  tdClass: 'border-top', headerStyle:'text'},
        ],
        [
            {key:'Select',label:'',sortable:false,  headerStyle:'text-primary',  cellStyle:'font-size: 16px;', tdClass: 'border-top', thClass:''},
            {key:'Date',               sortable:true,   tdClass: 'border-top',  headerStyle:'text-danger'},
            {key:'Document Type',      sortable:true,   tdClass: 'border-top',  cellStyle:'text-align:left;', headerStyle:'text-primary'},
            {key:'Status',             sortable:true,   tdClass: 'border-top',  headerStyle:'text-primary'},
            {key:'Status Date',        sortable:true,   tdClass: 'border-top',  headerStyle:'text-primary'},
            {key:'Category',           sortable:false,  tdClass: 'border-top',  headerStyle:'text'},
            {key:'Pages',              sortable:false,  tdClass: 'border-top',  headerStyle:'text'},
        ]  
        
    ];

    public getDocuments(): void {
       
        this.participantList = this.criminalFileInformation.participantList;
        this.courtLevel = this.criminalFileInformation.courtLevel;
        this.courtClass = this.criminalFileInformation.courtClass;

        this.ExtractDocumentInfo()          
        this.isMounted = true;
    }    

    mounted () {       
        this.getDocuments();
        this.downloadCompleted = true
        this.selectedDocuments = {zipName: "", csrRequests: [], documentRequests: [], ropRequests: []}  
    }

    public switchTab(tabMapping) {        
        this.allDocumentsChecked = false;
        this.activetab = tabMapping;
    }

    public setActiveParticipantIndex(index)
    {                   
        this.UpdateActiveCriminalParticipantIndex(index);  
    }

    public navigateToLandingPage() {
        this.$router.push({name:'Home'})
    }

    public getNameOfParticipant(num)
    {
        this.UpdateDisplayName({'lastName': this.participantFiles[num]["Last Name"], 'givenName': this.participantFiles[num]["First Name"]});
        return this.displayName;
    }

    public downloadDocuments() {
        // console.log(this.participantFiles["Documents"])

        const fileName = shared.generateFileName(CourtDocumentType.CriminalZip, {
            location: this.criminalFileInformation.detailsData.homeLocationAgencyName,
            courtClass: this.criminalFileInformation.detailsData.courtClassCd,
            courtLevel: this.criminalFileInformation.detailsData.courtLevelCd,
            fileNumberText:  this.criminalFileInformation.detailsData.fileNumberTxt
        });
        
        this.selectedDocuments = {zipName: fileName, csrRequests: [], documentRequests: [], ropRequests: []};
        for(const doc of this.participantFiles[this.activeCriminalParticipantIndex]["Documents"]){
            if (doc.isChecked && doc.isEnabled) {
                const id = doc["Image ID"];                
                const documentRequest = {} as documentRequestsInfoType;
                documentRequest.isCriminal = true;
                const documentData: DocumentData = { 
                    courtClass: this.criminalFileInformation.detailsData.courtClassCd, 
                    courtLevel: this.criminalFileInformation.detailsData.courtLevelCd, 
                    dateFiled: Vue.filter('beautify-date')(doc["Date"]),
                    documentDescription: doc["Document Type"],
                    documentId: id,
                    fileId: this.criminalFileInformation.fileNumber,
                    fileNumberText: this.criminalFileInformation.detailsData.fileNumberTxt,
                    location: this.criminalFileInformation.detailsData.homeLocationAgencyName,
                };
                documentRequest.pdfFileName = shared.generateFileName(CourtDocumentType.Criminal, documentData);
                documentRequest.base64UrlEncodedDocumentId = base64url(id);
                documentRequest.fileId = this.criminalFileInformation.fileNumber;
                this.selectedDocuments.documentRequests.push(documentRequest);                
            }        
        }

        for(const doc of this.participantFiles[this.activeCriminalParticipantIndex]["Record of Proceedings"]){
            if (doc.isChecked && doc.isEnabled) {                
                const ropRequest = {} as ropRequestsInfoType;
                const partId = doc['Part ID'];
                const documentData: DocumentData = { 
                    courtClass: this.criminalFileInformation.detailsData.courtClassCd, 
                    courtLevel: this.criminalFileInformation.detailsData.courtLevelCd, 
                    documentDescription: doc["Document Type"],
                    fileId: this.criminalFileInformation.fileNumber,
                    fileNumberText: this.criminalFileInformation.detailsData.fileNumberTxt,
                    location: this.criminalFileInformation.detailsData.homeLocationAgencyName,
                    partId: partId,
                    profSeqNo: doc['Prof Seq No']
                };
                ropRequest.pdfFileName = shared.generateFileName(CourtDocumentType.ROP, documentData);
                ropRequest.partId = partId;
                ropRequest.profSequenceNumber = doc['Prof Seq No'];
                ropRequest.courtLevelCode = this.criminalFileInformation.courtLevel;
                ropRequest.courtClassCode = this.criminalFileInformation.courtClass;
                this.selectedDocuments.ropRequests.push(ropRequest); 
            }        
        }

        if(this.selectedDocuments.ropRequests.length>0 ||this.selectedDocuments.documentRequests.length>0){
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
        if(this.activetab == 'ROP')
        {
            for(const docInx in this.participantFiles[this.activeCriminalParticipantIndex]["Record of Proceedings"]){
                if (this.participantFiles[this.activeCriminalParticipantIndex]["Record of Proceedings"][docInx].isEnabled) {
                    this.participantFiles[this.activeCriminalParticipantIndex]["Record of Proceedings"][docInx].isChecked = checked
                }        
            }
        }
        else {
            if(this.activetab != 'ALL')
            { 
                if (this.participantFiles[this.activeCriminalParticipantIndex]["Documents"]["Category"].toUpperCase() == this.activetab.toUpperCase()){
                    for(const docInx in this.participantFiles[this.activeCriminalParticipantIndex]["Documents"]){
                        if (this.participantFiles[this.activeCriminalParticipantIndex]["Documents"][docInx].isEnabled) {
                            this.participantFiles[this.activeCriminalParticipantIndex]["Documents"][docInx].isChecked = checked
                        }        
                    }
                }
            } else {
                for(const docInx in this.participantFiles[this.activeCriminalParticipantIndex]["Documents"]){
                    if (this.participantFiles[this.activeCriminalParticipantIndex]["Documents"][docInx].isEnabled) {
                        this.participantFiles[this.activeCriminalParticipantIndex]["Documents"][docInx].isChecked = checked
                    }        
                }
            }
        }    
    }

    public toggleSelectedDocuments() {  
        Vue.nextTick(()=>{
            if(this.activetab == 'ROP') {
                const checkedDocs = this.participantFiles[this.activeCriminalParticipantIndex]["Record of Proceedings"].filter(doc=>{return doc.isChecked})
                const enabledDocs = this.participantFiles[this.activeCriminalParticipantIndex]["Record of Proceedings"].filter(doc=>{return doc.isEnabled})
                if(checkedDocs.length == enabledDocs.length)
                    this.allDocumentsChecked = true
                else
                    this.allDocumentsChecked = false

            } else {
                const checkedDocs = this.participantFiles[this.activeCriminalParticipantIndex]["Documents"].filter(doc=>{return doc.isChecked})
                const enabledDocs = this.participantFiles[this.activeCriminalParticipantIndex]["Documents"].filter(doc=>{return doc.isEnabled})
                if(checkedDocs.length == enabledDocs.length)
                    this.allDocumentsChecked = true
                else
                    this.allDocumentsChecked = false
            }
            
        })        
	}
   
    public ExtractDocumentInfo(): void {
        let ropExists = false 
        
        for(const partIndex in this.participantList)
        {         
            const partInfo = this.participantList[partIndex];
            partInfo["Documents"] = [];
            partInfo["Record of Proceedings"] = [];            
            const document: participantDocumentsInfoType[] = [];
            const rop: participantROPInfoType[] = [];
            
            for(const doc of partInfo.DocumentsJson)
            {
                if(doc.category != 'rop') {
                    const docInfo = {} as participantDocumentsInfoType; 
                    docInfo["Date"]= doc.issueDate? doc.issueDate.split(' ')[0] : ''; 
                    docInfo["Document Type"]= doc.docmFormDsc;
                    docInfo["Category"]= doc.category? doc.category: doc.docmClassification;
                    docInfo["Pages"]= doc.documentPageCount;
                    docInfo["PdfAvail"]= doc.imageId? true : false
                    docInfo["Image ID"]= doc.imageId;
                    docInfo["Status"] = doc.docmDispositionDsc;
                    docInfo["Status Date"] = doc.docmDispositionDate;
                    docInfo.isEnabled = docInfo["PdfAvail"];
                    docInfo.isChecked = false;
                    if (docInfo["Category"] != "PSR") {
                        docInfo["Category"] = docInfo["Category"].charAt(0).toUpperCase() + docInfo["Category"].slice(1).toLowerCase();                        
                    }                                                     
                    if((this.categories.indexOf(docInfo["Category"]) < 0) ) this.categories.push(docInfo["Category"]) 
                    
                    document.push(docInfo);
                }
                else {
                    const docInfo = {} as participantROPInfoType;                   
                    docInfo["Document Type"]= 'Record of Proceedings';
                    docInfo["Category"]= "ROP";
                    docInfo["Pages"]= doc.documentPageCount;
                    docInfo["PdfAvail"]= true 
                    docInfo["Index"] = partIndex;
                    docInfo["Prof Seq No"] = partInfo["Prof Seq No"];
                    docInfo["Part ID"] = partInfo["Part ID"];
                    docInfo.isEnabled = docInfo["PdfAvail"];
                    docInfo.isChecked = false;
                    rop.push(docInfo);
                    ropExists = true
                }
            }
            partInfo["Documents"] = document;
            partInfo["Record of Proceedings"] = rop;                        
            this.participantFiles.push(partInfo);
        }

         this.categories.sort()
         if(ropExists) this.categories.push("ROP");
         this.categories.unshift("ALL")         
    }

    get SortedParticipants()
    {         
        return _.sortBy(this.participantFiles,(participant=>{return (participant["Last Name"]? participant["Last Name"].toUpperCase() : '')}))       
    }

    get FilteredDocuments() {       
        if(this.activetab == 'ROP')
        {
            this.fieldsTab = fieldTab.Summary;
            return this.participantFiles[this.activeCriminalParticipantIndex]["Record of Proceedings"];
        }
        else{  
            return this.participantFiles[this.activeCriminalParticipantIndex]["Documents"].filter(doc => {                
                this.fieldsTab = fieldTab.Categories;
                if(this.activetab == 'Bail')
                {
                    this.fieldsTab = fieldTab.Bail;
                    
                    if (doc["Category"].toUpperCase() == this.activetab.toUpperCase()){
                        return true;
                    }                                    
                                  
                    return false; 
                } 
                else if ( this.activetab != 'ALL' )
                {
                    if (doc["Category"].toUpperCase() == this.activetab.toUpperCase()) return true;                                   
                                  
                    return false;                     
                }
                else
                {
                    return true;
                }
            }); 
        }    
    }

    get getNameOfDateInTabs()
    {
        switch(this.activetab.toLowerCase())
        {
            case ("all"):
                return "Date Filed/Issued";
            case ("scheduled"):
                return "Date Sworn/Filed";
            case ("bail"):
                return "Date Ordered";
            case ("psr"):
                return "Date Filed";
            default:
                return "Date Sworn/Issued";
        }
    }    

    get NumberOfDocuments() {       
        if(this.activetab == 'ROP')
        {           
            return(this.participantFiles[this.activeCriminalParticipantIndex]["Record of Proceedings"].length)
        }
        else{  
            return(this.participantFiles[this.activeCriminalParticipantIndex]["Documents"].length)            
        }    
    }

    public cellClick(eventData)
    {   
        this.loadingPdf = true;
        const documentType = eventData.item.Category == "ROP" ? CourtDocumentType.ROP : CourtDocumentType.Criminal;
        const index = eventData.item["Index"];
        const documentData: DocumentData = { 
            courtClass: this.criminalFileInformation.detailsData.courtClassCd, 
            courtLevel: this.criminalFileInformation.detailsData.courtLevelCd, 
            dateFiled: Vue.filter('beautify-date')(eventData.item["Date"]),
            documentId: eventData.item["Image ID"], 
            documentDescription: eventData.item["Document Type"],
            fileId: this.criminalFileInformation.fileNumber, 
            fileNumberText: this.criminalFileInformation.detailsData.fileNumberTxt,
            partId: index ? this.participantFiles[index]["Part ID"] : '', 
            profSeqNo: index ? this.participantFiles[index]["Prof Seq No"]: '', 
            location: this.criminalFileInformation.detailsData.homeLocationAgencyName
        };
        shared.openDocumentsPdf(documentType, documentData);
        this.loadingPdf = false;
    }
}
</script>

<style scoped>
 .card {
        border: white;
    }

</style>