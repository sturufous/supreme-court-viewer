<template>
<body>
   <b-card bg-variant="white" border-variant="white">   
       
        <b-card bg-variant="light">
            <b-tabs active-nav-item-class="font-weight-bold text-uppercase text-info bg-light" pills >
                <b-tab 
                v-for="(tabMapping, index) in tabMappings" 
                :key="index"                 
                :title="tabMapping.key"                 
                v-on:click="activetab = tabMapping.key" 
                v-bind:class="[ activetab === tabMapping.key ? 'active' : '' ]"
                ></b-tab>
            </b-tabs>
        </b-card>
          
        <b-card border-variant="white"></b-card>
   
        <b-card bg-variant="light">           
            <b-table
            :items="FilteredDocuments"
            :fields="fields[fieldsTab]"
            :sort-by.sync="sortBy"
            :sort-desc.sync="sortDesc"
            :no-sort-reset="true"
            striped
            responsive="sm"
            >   
                <template v-for="(f,index) in fields[fieldsTab]" v-slot:[`head(${f.key})`]="data">
                   <b v-bind:key="index" :class="f.headerStyle" > {{ data.label }}</b>
                </template>
                 <template v-for="(f,index) in fields[fieldsTab]" v-slot:[`cell(${f.key})`]="data" >
                   <span v-bind:key="index"
                                v-on:click="(data.item.PdfAvail)&&(index==1)&&(activetab!='Court Summary')? 
                                openDocumentsPdf(data.item['Document ID']): 
                                ((index==0)&&(activetab=='Court Summary'))? openCourtSummaryPdf(data.item['Appearance ID']) : ''"
                               :class="(data.item.PdfAvail)&&(index==1)&&(activetab!='Court Summary')? 'text-info': f.cellStyle" 
                                style="white-space: pre-line"> {{ data.value }}
                    </span>
                </template>
            </b-table>
        </b-card>

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

        Promise.all([
            this.$http.get('api/files/civil/'+ this.civilFileDocument.fileNumber),
            this.$http.get('api/files/civil/file-content?physicalFileId='+ this.civilFileDocument.fileNumber),
            this.$http.get('api/files/civil/'+ this.civilFileDocument.fileNumber + '/appearances?future=1&history=0')
        ]).then(responses =>
                Promise.all(responses.map(res => res.json()))
            ).then(data => {

                this.documentsDetailsJson = data[0].document
                this.documentsJson = data[1].civilFile[0].document
                this.documentsAppearanceJson = data[2].apprDetail
                this.ExtractDocumentInfo()
 
            });
    }

    mounted () {        
        this.getDocuments();        
    }

    documentsJson;
    documentsDetailsJson;
    documentsAppearanceJson;

    activetab = 'All';            
    sortBy = 'Seq.';
    sortDesc = false;

    documents: any[] = [];
    summaryDocuments: any[] = [];

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

    tabMappings= [
        {key:'All', AltCdWords:['']}, 
        {key:'Scheduled', AltCdWords:['']}, 
        {key:'Pleadings', AltCdWords:['AEA','AEO','AFO','APC','APO','ARC','HCL','NFC','NRG','ORO','REC','REP','RES','RFC','RPC','RPL','RTC','SA','SAP','TC','WAG']}, 
        {key:'Motions', AltCdWords:['AAP','ACMW','AFCO','APJ','ATC','AXP','NM','NTRF']}, 
        {key:'FS/Affidavits', AltCdWords:['AAS','ACD','AFB','AFBA','AFC','AFF','AFI','AFJ','AFM','AFS','AFSA','AFT','AOS','APS','CSA']}, 
        {key:'Orders', AltCdWords:['ABO','AOD','CAO','CDO','CMCO','COR','COS','CPOR','CRT','DJ','DO','DOR','DPO','FCR','MCO','ODT','OFI','ORA','ORD','ORFJ','ORI','ORNA','ORT','ORW','OWN','PCH','PO','POD','POR','PVO','ROR','RSO','SPO']}, 
        {key:'Concluded', AltCdWords:['']}, 
        {key:'Court Summary', AltCdWords:['']}
    ] ;          

    public ExtractDocumentInfo(): void {
               
        for(const docIndex in this.documentsJson)
        {
            const docInfo = {}; 
            const jDoc =  this.documentsJson[docIndex]; 
            docInfo["Index"] = docIndex;       
            docInfo["Seq."] = jDoc.fileSeqNumber;
            docInfo["Document Type"] = jDoc.documentTypeDescription;
            docInfo["DocumentTypeCd"] = jDoc.documentTypeCd;
            this.documents.push(docInfo);
        }
        
        for(const inx in this.documents)
        {
            const docInfo  = this.documents[inx];
            const jDoc =  this.documentsDetailsJson[this.documents[inx].Index];

            docInfo["Concluded"] = jDoc.concludedYn;        
            docInfo["Appearance Date"] = jDoc.lastAppearanceDt? jDoc.lastAppearanceDt.split(" ")[0]: ' ';

            // ensure all documentSupport elements only have one row
            const docSupport: any = jDoc.documentSupport.length? jDoc.documentSupport[0]:'{}';
            docInfo["Act"] = (docSupport==={})? '': docSupport.actCd;
            docInfo["Document ID"] = jDoc.civilDocumentId;            
            docInfo["PdfAvail"] = jDoc.civilDocumentId? true : false 
            docInfo["Date Filed"] = jDoc.filedDt? jDoc.filedDt.split(" ")[0]: ' ';
            docInfo["Issues"] = jDoc.issue.length? this.ExtractIssues(jDoc.issue) : ' ';
        }

        for(const doc of this.documentsAppearanceJson)
        {
            const docInfo = {}; 
            docInfo["Document Type"] = 'CourtSummary';
            docInfo["Appearance Date"] = doc.appearanceDt.split(" ")[0]
            docInfo["Appearance ID"] = doc.appearanceId;
            this.summaryDocuments.push(docInfo);
        }
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

        if(this.activetab == 'Court Summary')
        {
            this.fieldsTab = fieldTab.Summary;
            return this.summaryDocuments;
        }
        else{       
            return this.documents.filter(doc => {                
                this.fieldsTab = fieldTab.Categories;
                if(this.activetab == 'Concluded') {
                    if(doc["Concluded"] === "Y") return true; else return false;
                
                } else if(this.activetab == 'Scheduled') {
                    if(doc["Appearance Date"]){        
                        if(new Date(doc["Appearance Date"]) > new Date()) return true; else return false;
                    
                    } else {
                        return false
                    }  

                } else if ( this.activetab != 'All' )
                {          
                    const altCdWords = this.tabMappings.filter( tabMap => {return this.activetab == tabMap.key})[0].AltCdWords;
                    for(const word of altCdWords) 
                    {
                        if(doc["DocumentTypeCd"]==word) return true;                                    
                    }                    
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
        // TODO: remove the hardcoded documentId once sample data has pdf
        documentId = 70
        const filename = 'doc'+documentId+'.pdf';
        // TODO: change to civil when a civil documentID is available 

        this.$http.get('api/files/document/' + documentId + '/filename.pdf?isCriminal=true')
            .then(Response => Response.json(), err => console.log('error')        
            ).then(data => {
                
                if(window.navigator && window.navigator.msSaveOrOpenBlob) {
                    window.navigator.msSaveOrOpenBlob(this.b64toBlob(data.b64Content,'application/pdf'), filename);
                    return;
                }
                else
                {
                    const url = URL.createObjectURL(this.b64toBlob(data.b64Content,'application/pdf'))
                    window.open(url);
                }             
 
            });        
    }
    
    public openCourtSummaryPdf(appearanceId): void {

        // TODO: remove the hardcoded appearanceId once sample data has pdf
        appearanceId = 10098
        const filename = 'court summary'+appearanceId+'.pdf';

        this.$http.get("api/files/civil/court-summary-report/" + appearanceId + "/filename.pdf")
            .then(Response => Response.json()        
            ).then(data => {
                
                if(window.navigator && window.navigator.msSaveOrOpenBlob) {
                    window.navigator.msSaveOrOpenBlob(this.b64toBlob(data.reportContent,'application/pdf'), filename);
                    return;
                }
                else
                {
                    const url = URL.createObjectURL(this.b64toBlob(data.reportContent,'application/pdf'))
                    window.open(url);
                }  
            });
    }      
    
}
</script>