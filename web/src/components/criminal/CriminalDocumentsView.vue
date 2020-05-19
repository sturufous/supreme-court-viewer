<template>
<body>
    <b-card bg-variant="light" v-if= "!isMounted && !isDataValid">
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

    <b-card bg-variant="light" v-if= "isMounted && !isDataValid">
        <b-card  style="min-height: 100px;">
            <span>This <b>File-Number '{{this.criminalFileDocument.fileNumber}}'</b> doesn't have participant information. </span>
        </b-card>
        <b-card>         
            <b-button variant="info" @click="navigateToLandingPage">Back to the Landing Page</b-button>
        </b-card>
    </b-card>

   <b-card  v-if= "isMounted && isDataValid">
        <b-card bg-variant="light">
            <b-tabs active-nav-item-class="font-weight-bold text-uppercase text-info bg-light" pills >
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
            <b-dropdown  variant="light text-info" :text="getNameOfParticipant(activeparticipant)" class="m-2">    
                <b-dropdown-item  
                    v-for="(file,index) in SortedParticipants" 
                    :key="index"
                    v-on:click="activeparticipant = index">
                        {{getNameOfParticipant(index)}}
                </b-dropdown-item> 
            </b-dropdown>                 
        </b-card>

        <b-overlay :show="loadingPdf" rounded="sm">  
            <b-card bg-variant="light">           
                <b-table
                :items="FilteredDocuments"
                :fields="fields[fieldsTab]"
                :sort-by.sync="sortBy"
                :sort-desc.sync="sortDesc"
                :no-sort-reset="true"
                @row-hovered="rowHover"
                striped
                responsive="sm"
                >   
                    <template v-for="(field,index) in fields[fieldsTab]" v-slot:[`head(${field.key})`]="data">
                    <b v-bind:key="index" :class="field.headerStyle" > {{ data.label }}</b>
                    </template>
                    <template v-for="(field,index) in fields[fieldsTab]" v-slot:[`cell(${field.key})`]="data" >
                    <span 
                            v-bind:key="index" 
                            v-b-hover="colHover"
                            v-on:click= "cellClick(index, data)"
                            :class= "cellClass(field, index, data)"  
                            style="white-space: pre-line"> {{ data.value }}
                        </span>
                    </template>
                </b-table>
            </b-card>
            <template v-slot:overlay>               
               <div> 
                    <loading-spinner/> 
                    <p id="Downloading-label">Downloading PDF file ...</p>
               </div>                
            </template> 
        </b-overlay>
   </b-card> 
</body>
</template>

<script lang="ts">
import { Component, Vue } from 'vue-property-decorator';
import { namespace } from 'vuex-class';
import '@store/modules/CriminalFileInformation';
const criminalState = namespace('CriminalFileInformation');

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
    public criminalFileInformation!: any

    @criminalState.Action
    public UpdateCriminalFile!: (newCriminalFileInformation: any) => void
    
    public getDocuments(): void {

       
        this.$http.get('api/files/criminal/'+ this.criminalFileInformation.fileNumber)
            .then(Response => Response.json(), err => {console.log(err);this.isMounted = true;}        
            ).then(data => {
                this.participantJson = data.participant 
                this.courtLevel = DecodeCourtLevel[data.courtLevelCd];
                this.courtClass = DecodeCourtClass[data.courtClassCd];

                this.ExtractDocumentInfo()           

                if(this.participantFiles.length)
                {
                    this.isMounted = true;
                    this.isDataValid = true;
                }
                else
                {
                    this.isMounted = true;
                }
            });
    }

    mounted () { 
        this.criminalFileInformation.fileNumber = this.$route.params.fileNumber
        this.UpdateCriminalFile(this.criminalFileInformation);        
        this.getDocuments();  
    }

    participantJson;
    courtLevel;
    courtClass;

    message = 'Loading';

    loadingPdf = false;
    
    activetab = 'ALL'; 
    activeparticipant = 0;           
    sortBy = 'Date Filed/Issued';
    sortDesc = false;
    hoverRow =-1;
    hoverCol = 0;

    isMounted = false
    isDataValid = false

    participantFiles: any[] = [];
    ropDocuments: any[] = [];
    categories: any = []; 

    fieldsTab = fieldTab.Categories;

    fields = [ 
        [
            {key:'Date Filed/Issued',  sortable:true,  headerStyle:'text-danger',   cellStyle:'text'},
            {key:'Document Type',      sortable:true,  headerStyle:'text-primary',  cellStyle:'text-muted'},
            {key:'Category',           sortable:false, headerStyle:'text',          cellStyle:'text'},
            {key:'Pages',              sortable:false, headerStyle:'text',          cellStyle:'text'},
        ],
        [
            {key:'Document Type',    sortable:false, headerStyle:'text-primary',    cellStyle:'text-info'},
            {key:'Category',         sortable:true,  headerStyle:'text',            cellStyle:'text'},
            {key:'Pages',            sortable:false, headerStyle:'text',            cellStyle:'text'},
        ]  
        
    ];

    public getNameOfParticipant(num)
    {        
        return  this.participantFiles[num]["Last Name"]+', '+this.participantFiles[num]["First Name"];           
    }

    public navigateToLandingPage() {
        this.$router.push({name:'Home'})
    }

    public cellClick(index, data)
    {        
        if(data.item.PdfAvail && index==1 && this.activetab!='ROP')
        {
            this.openDocumentsPdf(data.item["Image ID"]);
        }
        else if (index==0 && this.activetab=='ROP')
        {
            this.openRopPdf(data.item["Index"])     
        }         
    }

    public cellClass(field, index, data)
    {
         if(data.item.PdfAvail && index==1 && this.activetab!='ROP')
        {
            if(this.hoverCol==1 && this.hoverRow==data.item.Index) return 'text-white bg-warning'; else return 'text-info';            
        }
        else if(index==0 && this.activetab=='ROP')
        {
            if(this.hoverCol==0 && this.hoverRow==data.item.Index) return 'text-white bg-warning'; else return 'text-info';   
        }
        else 
            return field.cellStyle;
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
            fileInfo["First Name"] = jFile.givenNm? jFile.givenNm: '_noGivenname';
            fileInfo["Last Name"] =  jFile.lastNm? jFile.lastNm: '_noLastname' ;            
            
            fileInfo["Documents"] = [];
            fileInfo["Record of Proceedings"] = [];

            const document: any[] = [];
            const rop: any[] = [];
            for(const doc of jFile.document)
            {
                if(doc.category != 'rop') {
                    const docInfo = {}; 
                    docInfo["Date Filed/Issued"]= doc.issueDate? doc.issueDate.split(" ")[0] : '';
                    docInfo["Document Type"]= doc.docmFormDsc;
                    docInfo["Category"]= doc.docmClassification;
                    docInfo["Pages"]= doc.documentPageCount;
                    docInfo["PdfAvail"]= doc.imageId? true : false
                    docInfo["Image ID"]= doc.imageId
                    
                    if((this.categories.indexOf(docInfo["Category"]) < 0) ) this.categories.push(docInfo["Category"]) 
                    
                    document.push(docInfo);
                }
                else{
                    const docInfo = {};                   
                    docInfo["Document Type"]= 'Record of Proceedings';
                    docInfo["Category"]= "ROP";
                    docInfo["Pages"]= doc.documentPageCount;
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
        return this.participantFiles.sort((a, b): any =>
        {
            const LastName1 = a["Last Name"]? a["Last Name"].toUpperCase() : '';
            const LastName2 = b["Last Name"]? b["Last Name"].toUpperCase() : '';
            if(LastName1 > LastName2) return 1;
            if(LastName1 < LastName2) return -1;
            return 0;
        });        
    }

    get FilteredDocuments() {       
        if(this.activetab == 'ROP')
        {
            this.fieldsTab = fieldTab.Summary;
            return this.participantFiles[this.activeparticipant]["Record of Proceedings"];
        }
        else{  
            return this.participantFiles[this.activeparticipant]["Documents"].filter(doc => {                
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
    
    public colHover(hovered, mouseEvent) {            
        hovered? this.hoverCol = mouseEvent.fromElement.cellIndex: this.hoverCol =-1;
    }

    public rowHover(row) {
        this.hoverRow = row.Index;
    }

    public openDocumentsPdf(imageId): void {
        this.loadingPdf = true;
        const filename = 'doc'+imageId+'.pdf';
        window.open(`api/files/document/${imageId}/${filename}?isCriminal=true`)
        this.loadingPdf = false;
    }
    
    public openRopPdf(index): void {
        this.loadingPdf = true;  
        const partID = this.participantFiles[index]["Part ID"];
        const profSeqNo = this.participantFiles[index]["Prof Seq No"];      
        const filename = 'ROP_'+partID+'.pdf';
        window.open(`api/files/criminal/record-of-proceedings/${partID}/${filename}?profSequenceNumber=${profSeqNo}&courtLevelCode=${this.courtLevel}&courtClassCode=${this.courtClass}`)
        this.loadingPdf = false;
    }
    
}
</script>

<style scoped>
 .card {
        border: white;
    }

</style>