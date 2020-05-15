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
            <span>This <b>File-Number '{{this.criminalFileDocument.fileNumber}}'</b> doesn't exist in the <b>criminal</b> records. </span>
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
                    v-for="(file,index) in accusedFiles" 
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
import CriminalFileDocuments from '../store/modules/CriminalFileDocuments';
const criminalState = namespace('CriminalFileDocuments');

enum fieldTab {Categories=0, Summary}

@Component
export default class CriminalDocumentsView extends Vue {

    @criminalState.State
    public criminalFileDocument!: any

    public getDocuments(): void {

        Promise.all([            
            this.$http.get('api/files/criminal/'+ this.criminalFileDocument.fileNumber),
            this.$http.get('api/files/criminal/file-content?justinNumber='+ this.criminalFileDocument.fileNumber) 
        ]).then(responses =>                
            Promise.all(responses.map(res => res.json())), err => {console.log('error');this.isMounted = true;}            
        ).then(data => {           
            this.participantJson = data[0].participant                
            this.accusedFileJson = data[1].accusedFile               
            this.ExtractDocumentInfo()
            if(this.accusedFiles.length)
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
        this.getDocuments();        
    }

    participantJson;
    accusedFileJson;

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

    accusedFiles: any[] = [];
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

    public getNameOfParticipant(value)
    {        
        return  this.accusedFiles[value]["Last Name"]+', '+this.accusedFiles[value]["First Name"];           
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
             //TODO replace with ROP file open 
             console.log("open ROP pdf")     
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
       
        for(const fileIndex in this.accusedFileJson)
        {
            const fileInfo = {}; 
            const jFile =  this.accusedFileJson[fileIndex];
            
            fileInfo["Index"] = fileIndex;
            fileInfo["Part ID"] = jFile.partId; 
            
            
            fileInfo["Documents"] = [];

            const document: any[] = [];
            for(const doc of jFile.document)
            {
                const docInfo = {}; 
                docInfo["Date Filed/Issued"]= doc.issueDate.split(" ")[0];
                docInfo["Document Type"]= doc.docmFormDsc;
                docInfo["Category"]= doc.docmClassification;
                docInfo["Pages"]= doc.documentPageCount;
                docInfo["PdfAvail"]= doc.imageId? true : false
                docInfo["Image ID"]= doc.imageId

                if((this.categories.indexOf(docInfo["Category"]) < 0) ) this.categories.push(docInfo["Category"]) 
                
                document.push(docInfo);
            }
            fileInfo["Documents"] = document;
            
            this.accusedFiles.push(fileInfo);
        }

        for(const file of this.accusedFiles)
        {
            const index = file["Index"];
            file["First Name"] = this.participantJson[index]? this.participantJson[index].givenNm :'';
            file["Last Name"] =  this.participantJson[index]? this.participantJson[index].lastNm  :'';
        }

         this.categories.sort()
         this.categories.push("ROP")
         this.categories.unshift("ALL") 
    }

    get FilteredDocuments() {       
        if(this.activetab == 'ROP')
        {
            this.fieldsTab = fieldTab.Summary;
            return this.ropDocuments;
        }
        else{  
            return this.accusedFiles[this.activeparticipant]["Documents"].filter(doc => {                
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
}
</script>

<style scoped>
 .card {
        border: white;
    }

</style>