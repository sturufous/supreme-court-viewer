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
import { Component, Vue, Watch } from 'vue-property-decorator';
import { namespace } from 'vuex-class';
import * as _ from 'underscore';
import '@store/modules/CriminalFileInformation';
import "@store/modules/CommonInformation";
const criminalState = namespace("CriminalFileInformation");
const commonState = namespace("CommonInformation");

enum fieldTab {Categories=0, Summary}
enum DecodeCourtLevel {'P'= 0, 'S' = 1, 'A' = 2 }
enum DecodeCourtClass {
    'A' = 0, 'Y' = 1, 'T' = 2, 'F' = 3, 'C' = 4, 'M' = 5,        
    'L' = 6, 'R' = 7, 'B' = 8, 'D' = 9, 'E' = 10, 'G' = 11,        
    'H' = 12, 'N' = 13, 'O' = 14, 'P' = 15, 'S' = 16, 'V' = 17,
}

@Component
export default class CriminalDocumentsView extends Vue {    

    @criminalState.State
    public activeCriminalParticipantIndex
    
    @commonState.State
    public displayName!: string;    
    
     /* eslint-disable */
    @criminalState.Action
    public UpdateActiveCriminalParticipantIndex!: (newActiveCriminalParticipantIndex: any) => void

    @criminalState.State
    public criminalFileInformation!: any

    @criminalState.Action
    public UpdateCriminalFile!: (newCriminalFileInformation: any) => void   

    @commonState.Action
    public UpdateDisplayName!: (newInputNames: any) => void

    participantFiles: any[] = [];
    ropDocuments: any[] = [];
    categories: any = [];
    /* eslint-enable */    

    participantJson;
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
    documentPlace = [1,0]

    fields = [ 
        [
            {key:'Date',  sortable:true,  tdClass: 'border-top',  headerStyle:'text-danger',   cellStyle:'text'},
            {key:'Document Type',      sortable:true,  tdClass: 'border-top',  headerStyle:'text-primary',  cellStyle:'text-muted'},
            {key:'Category',           sortable:false,  tdClass: 'border-top', headerStyle:'text',          cellStyle:'text'},
            {key:'Pages',              sortable:false,  tdClass: 'border-top', headerStyle:'text',          cellStyle:'text'},
        ],
        [
            {key:'Document Type',    sortable:false,  tdClass: 'border-top', headerStyle:'text-primary',    cellStyle:'text-info'},
            {key:'Category',         sortable:true,  tdClass: 'border-top',  headerStyle:'text',            cellStyle:'text'},
            {key:'Pages',            sortable:false,  tdClass: 'border-top', headerStyle:'text',            cellStyle:'text'},
        ]  
        
    ];

    public getDocuments(): void {
       
        const data = this.criminalFileInformation.detailsData;

        this.participantJson = data.participant 

        this.courtLevel = DecodeCourtLevel[data.courtLevelCd];
        this.courtClass = DecodeCourtClass[data.courtClassCd];

        this.ExtractDocumentInfo()          
        this.isMounted = true;
    }

    @Watch('$route', { immediate: false, deep: true })
    onUrlChange() {
        this.criminalFileInformation.fileNumber = this.$route.params.fileNumber
        this.UpdateCriminalFile(this.criminalFileInformation) 
        location.reload();
    }

    mounted () { 
        this.criminalFileInformation.fileNumber = this.$route.params.fileNumber
        this.UpdateCriminalFile(this.criminalFileInformation);        
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
        
        for(const fileIndex in this.participantJson)
        {            
            const fileInfo = {};
            const jFile =  this.participantJson[fileIndex];
            fileInfo["Index"] = fileIndex; 
            fileInfo["Part ID"] = jFile.partId;
            fileInfo["Prof Seq No"] = jFile.profSeqNo;
            fileInfo["First Name"] = jFile.givenNm.trim().length>0 ? jFile.givenNm : "";
            fileInfo["Last Name"] = jFile.lastNm ? jFile.lastNm : jFile.orgNm;            
            this.UpdateDisplayName({'lastName': fileInfo["Last Name"], 'givenName': fileInfo["First Name"]});
            fileInfo["Name"] = this.displayName;
            fileInfo["Documents"] = [];
            fileInfo["Record of Proceedings"] = [];

            /* eslint-disable */
            const document: any[] = [];
            const rop: any[] = [];
            /* eslint-enable */
            for(const doc of jFile.document)
            {
                if(doc.category != 'rop') {
                    const docInfo = {}; 
                    docInfo["Date"]= doc.issueDate? doc.issueDate.split(' ')[0] : ''; 
                    docInfo["Document Type"]= doc.docmFormDsc;
                    docInfo["Category"]= doc.docmClassification;
                    docInfo["Pages"]= doc.documentPageCount;
                    docInfo["PdfAvail"]= doc.imageId? true : false
                    docInfo["Image ID"]= doc.imageId
                    
                    if((this.categories.indexOf(docInfo["Category"]) < 0) ) this.categories.push(docInfo["Category"]) 
                    
                    document.push(docInfo);
                }
                else {
                    const docInfo = {};                   
                    docInfo["Document Type"]= 'Record of Proceedings';
                    docInfo["Category"]= "ROP";
                    docInfo["Pages"]= doc.documentPageCount;
                    docInfo["PdfAvail"]= true 
                    docInfo["Index"] = fileIndex;
                    rop.push(docInfo);
                    ropExists = true
                }
            }
            fileInfo["Documents"] = document;
            fileInfo["Record of Proceedings"] = rop;
                        
            this.participantFiles.push(fileInfo);
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
                if ( this.activetab != 'ALL' )
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
        window.open(`/api/files/document/${imageId}/${filename}?isCriminal=true`)
        this.loadingPdf = false;
    }
    
    public openRopPdf(index): void {
        this.loadingPdf = true;         
        const partID = this.participantFiles[index]["Part ID"];
        const profSeqNo = this.participantFiles[index]["Prof Seq No"];      
        const filename = 'ROP_'+partID+'.pdf';
      
        const url =`/api/files/criminal/record-of-proceedings/${partID}/${filename}?profSequenceNumber=${profSeqNo}&courtLevelCode=${this.courtLevel}&courtClassCode=${this.courtClass}`;

        this.$http.get(url)
            .then(Response => {
                window.open(url);
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