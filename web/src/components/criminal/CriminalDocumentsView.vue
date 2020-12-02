<template>

   <b-card  v-if= "isMounted" no-body>
        <div>         
            <h3 class="mx-4 font-weight-normal"> Documents ({{NumberOfDocuments}}) </h3>
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
                v-on:click="activetab = tabMapping" 
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

                    <template v-slot:[`cell(${fields[0][0].key})`]="data" >
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
                <span v-else class="text-muted ml-4 mb-5"> No document with label <b> {{activetab}} </b> . </span>
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
import {participantFilesInfoType, participantROPInfoType, participantListInfoType, participantDocumentsInfoType, criminalFileInformationType} from '../../types/criminal';
import {inputNamesType} from '../../types/common'
const criminalState = namespace("CriminalFileInformation");
const commonState = namespace("CommonInformation");

enum fieldTab {Categories=0, Summary, Bail}

@Component
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
    documentPlace = [1,0,1]

    fields = [ 
        [
            {key:'Date',               sortable:true,   tdClass: 'border-top',  headerStyle:'text-danger'},
            {key:'Document Type',      sortable:true,   tdClass: 'border-top',  headerStyle:'text-primary'},
            {key:'Category',           sortable:false,  tdClass: 'border-top',  headerStyle:'text'},
            {key:'Pages',              sortable:false,  tdClass: 'border-top',  headerStyle:'text'},
        ],
        [
            {key:'Document Type',    sortable:false,  tdClass: 'border-top', headerStyle:'text-primary'},
            {key:'Category',         sortable:true,   tdClass: 'border-top', headerStyle:'text'},
            {key:'Pages',            sortable:false,  tdClass: 'border-top', headerStyle:'text'},
        ],
        [
            {key:'Date',               sortable:true,   tdClass: 'border-top',  headerStyle:'text-danger'},
            {key:'Document Type',      sortable:true,   tdClass: 'border-top',  headerStyle:'text-primary'},
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

    public cellClick(data)
    {         
        if(data.item.Category !='ROP')        
            this.openDocumentsPdf(data.item["Image ID"]);        
        else        
            this.openRopPdf(data.item["Index"])              
    }

    public openDocumentsPdf(imageId): void {
        this.loadingPdf = true;
        const filename = 'doc'+imageId+'.pdf';
        window.open(`${process.env.BASE_URL}api/files/document/${imageId}/${filename}?isCriminal=true`)
        this.loadingPdf = false;
    }
    
    public openRopPdf(index): void {
        this.loadingPdf = true;         
        const partID = this.participantFiles[index]["Part ID"];
        const profSeqNo = this.participantFiles[index]["Prof Seq No"];      
        const filename = 'ROP_'+partID+'.pdf';
      
        const url =`api/files/criminal/record-of-proceedings/${partID}/${filename}?profSequenceNumber=${profSeqNo}&courtLevelCode=${this.courtLevel}&courtClassCode=${this.courtClass}`;

        this.$http.get(url)
            .then(() => {
                window.open(`${process.env.BASE_URL}${url}`);
                this.loadingPdf = false;},
              err => {
                console.log(err); 
                window.alert("Broken PDF File");
                this.loadingPdf = false;}
            );        
    }
    
}
</script>

<style scoped>
 .card {
        border: white;
    }

</style>