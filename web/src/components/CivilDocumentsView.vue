<template>
<body>
   <b-card bg-variant="white" border-variant="white">   
       
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
        
       
        <b-card border-variant="white"></b-card>
 
        <b-overlay :show="loadingPdf" rounded="sm">  
            <b-card bg-variant="light" :aria-hidden="loadingPdf ? 'true' : null">           
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
                            v-bind:key= "index" 
                            v-b-hover= "colHover"                            
                            v-on:click= "cellClick(index, data)"
                            :class= "cellClass(field, index, data)"    
                            style= "white-space: pre-line"> {{ data.value }}
                        </span>
                    </template>
                </b-table>
            </b-card>
        </b-overlay>
   </b-card> 
</body>
</template>

<script lang="ts">
import { Component, Vue } from 'vue-property-decorator';
import { namespace } from 'vuex-class';
import CivilFileDocuments from '../store/modules/CivilFileDocuments';
const civilState = namespace('CivilFileDocuments');

enum fieldTab {Categories=0, Summary}

@Component
export default class CivilDocumentsView extends Vue {

    @civilState.State
    public civilFileDocument!: any

    public getDocuments(): void {


         this.loadingPdf = true;
         this.$http.get('api/files/civil/'+ this.civilFileDocument.fileNumber)
            .then(Response => Response.json(), err => console.log('error')        
            ).then(data => {
                this.documentsDetailsJson = data.document
                this.ExtractDocumentInfo()
                 this.loadingPdf = false
            });
    }

    mounted () {         
        this.getDocuments();        
    }

    documentsDetailsJson;
    loadingPdf = false;

    activetab = 'ALL';            
    sortBy = 'Seq.';
    sortDesc = false;
    hoverRow =-1;
    hoverCol = 0;

    documents: any[] = [];
    summaryDocuments: any[] = [];
    categories: any=[]; 

    fieldsTab = fieldTab.Categories;

    fields = [ 
        [
            {key:'Seq.',           sortable:true,  headerStyle:'text-primary',  cellStyle:'text'},
            {key:'Document Type',  sortable:true,  headerStyle:'text-primary',  cellStyle:'text-muted'},
            {key:'Act',            sortable:false, headerStyle:'text',          cellStyle:'text-white bg-secondary'},
            {key:'Date Filed',     sortable:true,  headerStyle:'text-danger',   cellStyle:'text'},
            {key:'Issues',         sortable:false, headerStyle:'text',          cellStyle:'text-muted'}
        ],
        [
            {key:'Document Type',    sortable:false, headerStyle:'text-primary',    cellStyle:'text-info'},
            {key:'Appearance Date',  sortable:true, headerStyle:'text-danger',     cellStyle:'text'},
        ]  
        
    ];

    public cellClick(index, data)
    {
        if(data.item.PdfAvail && index==1 && this.activetab!='COURT SUMMARY')
        {
            this.openDocumentsPdf(data.item['Document ID']);
        }
        else if (index==0 && this.activetab=='COURT SUMMARY')
        {
            this.openCourtSummaryPdf(data.item['Appearance ID'])
        }         
    }

    public cellClass(field, index, data)
    {
        if(data.item.PdfAvail && index==1 && this.activetab!='COURT SUMMARY')
        {
            if(this.hoverCol==1 && this.hoverRow==data.item.Index) return 'text-white bg-warning'; else return 'text-info';            
        }
        else if(index==0 && this.activetab=='COURT SUMMARY')
        {
            if(this.hoverCol==0 && this.hoverRow==data.item.Index) return 'text-white bg-warning'; else return 'text-info';   
        }
        else 
            return field.cellStyle;
    }
    
    public ExtractDocumentInfo(): void {
        let courtSummaryExists = false 
        for(const docIndex in this.documentsDetailsJson)
        {
            const docInfo = {}; 
            const jDoc =  this.documentsDetailsJson[docIndex];
            docInfo["Index"] = docIndex;
            if(jDoc.documentTypeCd != 'CSR') {
                docInfo["Seq."] = jDoc.fileSeqNo;
                docInfo["Document Type"] = jDoc.documentTypeDescription;
                docInfo["Concluded"] = jDoc.concludedYn;
                if((this.categories.indexOf("CONCLUDED") < 0) && docInfo["Concluded"].toUpperCase() =="Y") this.categories.push("CONCLUDED")        
                docInfo["Appearance Date"] = jDoc.lastAppearanceDt? jDoc.lastAppearanceDt.split(" ")[0]: ' ';
                if(new Date(docInfo["Appearance Date"]) > new Date() && this.categories.indexOf("SCHEDULED") < 0) this.categories.push("SCHEDULED")   

                docInfo["Category"] = jDoc.category
                if((this.categories.indexOf(docInfo["Category"]) < 0) && docInfo["Category"].length > 0) this.categories.push(docInfo["Category"])
                // ensure all documentSupport elements only have one row
                const docSupport: any = jDoc.documentSupport.length? jDoc.documentSupport[0]:'{}';
                docInfo["Act"] = (docSupport==={})? '': docSupport.actCd;
                docInfo["Document ID"] = jDoc.civilDocumentId;            
                docInfo["PdfAvail"] = jDoc.imageId? true : false 
                docInfo["Date Filed"] = jDoc.filedDt? jDoc.filedDt.split(" ")[0]: ' ';
                docInfo["Issues"] = jDoc.issue.length? this.ExtractIssues(jDoc.issue) : ' ';
                this.documents.push(docInfo);

            } else {                
                docInfo["Document Type"] = 'CourtSummary';
                docInfo["Appearance Date"] = jDoc.lastAppearanceDt.split(" ")[0]
                docInfo["Appearance ID"] = jDoc.imageId;
                this.summaryDocuments.push(docInfo);
                courtSummaryExists = true
            }
        } 
        
        this.categories.sort()
        if(courtSummaryExists) this.categories.push("COURT SUMMARY")
        this.categories.unshift("ALL")        
    }

    public ExtractIssues(issues) {
        let issueString =''; 
        for (const issue of issues)
        {
            issueString += issue.issueDsc + '\n';
        }
        return issueString
    }

    get FilteredDocuments() {

        if(this.activetab == 'COURT SUMMARY')
        {
            this.fieldsTab = fieldTab.Summary;
            return this.summaryDocuments;
        }
        else{       
            return this.documents.filter(doc => {                
                this.fieldsTab = fieldTab.Categories;
                if(this.activetab == 'CONCLUDED') {
                    if(doc["Concluded"] === "Y") return true; else return false;
                
                } else if(this.activetab == 'SCHEDULED') {
                    if(doc["Appearance Date"]){        
                        if(new Date(doc["Appearance Date"]) > new Date()) return true; else return false;
                    
                    } else {
                        return false
                    }  

                } else if ( this.activetab != 'ALL' )
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

    public b64toBlob(b64Data, contentType) {

        const byteCharacters = atob(b64Data);
        const byteArrays: any = [];
        for ( let offset = 0; offset < byteCharacters.length; offset = offset + 512 ) {
            const slice = byteCharacters.slice(offset, offset + 512);
            const byteNumbers = new Array(slice.length);
            for (let i = 0; i < slice.length; i++) {
                byteNumbers[i] = slice.charCodeAt(i);
            }
            const byteArray = new Uint8Array(byteNumbers);
            byteArrays.push(byteArray);
        }
        return new Blob(byteArrays, { type: contentType });
    }

    public openDocumentsPdf(documentId): void {
        this.loadingPdf = true
        const filename = 'doc'+documentId+'.pdf';
       
        this.$http.get('api/files/document/' + documentId + '/filename.pdf?isCriminal=false')
            .then(Response => Response.json(), err => {console.log(err); this.loadingPdf = false }        
            ).then(data => {
               
                if(data.b64Content)
                {
                    if(window.navigator && window.navigator.msSaveOrOpenBlob) {
                        window.navigator.msSaveOrOpenBlob(this.b64toBlob(data.b64Content,'application/pdf'), filename);
                    }
                    else
                    {
                        const url = URL.createObjectURL(this.b64toBlob(data.b64Content,'application/pdf'))
                        window.open(url);
                    }  
                }
                this.loadingPdf = false;
 
            }, err =>  {console.log(err);this.loadingPdf = false});        
    }
    
    public openCourtSummaryPdf(appearanceId): void {

        this.loadingPdf = true
        
        const filename = 'court summary'+appearanceId+'.pdf';

        this.$http.get("api/files/civil/court-summary-report/" + appearanceId + "/"+ filename)
            .then(Response => Response.json(), err =>  this.loadingPdf = false        
            ).then(data => {
               
                if(data.reportContent)
                {
                    if(window.navigator && window.navigator.msSaveOrOpenBlob) {
                        window.navigator.msSaveOrOpenBlob(this.b64toBlob(data.reportContent,'application/pdf'), filename);
                    }
                    else
                    {
                        const url = URL.createObjectURL(this.b64toBlob(data.reportContent,'application/pdf'))
                        window.open(url);
                    }
                }
                this.loadingPdf = false 

            }, err =>  this.loadingPdf = false);
    }
    
    public colHover(hovered, mouseEvent) {            
        hovered? this.hoverCol = mouseEvent.fromElement.cellIndex: this.hoverCol =-1;
    }

    public rowHover(row) {
        this.hoverRow = row.Index;
    }
    
}
</script>