<template>
<div>
    <b-card bg-variant="light" v-if= "!isMounted ">
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
    <b-card bg-variant="light" v-else>
      <b-card bg-variant="white">
        <b-row cols="2">            
            <b-col md="8" cols="8" style="overflow: auto;">
                <b-overlay :show="loadingPdf" rounded="sm">
                    <div>
                        <h3 class="mx-2 font-weight-normal"> Document Summary</h3>
                        <hr class="mb-0 bg-light" style="height: 5px;"/> 
                    </div>
                    <b-card v-if="!(appearanceDocuments.length>0)" style="border: white;">
                        <span class="text-muted"> No documents. </span>
                    </b-card>                           
                    <b-table
                    v-if="appearanceDocuments.length > 0"
                    :items="appearanceDocuments"
                    :fields="documentFields"               
                    borderless
                    striped                    
                    @row-hovered="rowHover"               
                    responsive="sm"
                    >   
                        <template v-for="(field,index) in documentFields" v-slot:[`head(${field.key})`]="data">
                            <b v-bind:key="index" :class="field.headerStyle" > {{ data.label }}</b>
                        </template>                
                        <template v-for="(field,index) in documentFields" v-slot:[`cell(${field.key})`]="data" >
                            <span v-bind:key= "index" v-b-hover= "colHover" 
                                v-if="field.key.includes('Document')" 
                                v-on:click="documentClick(data.item)" 
                                :class="documentCellClass(field, data.item)"    
                                :style="field.cellStyle"> {{data.value}}
                            </span>
                            <span v-bind:key="index" :class="field.cellClass" v-else-if="field.key.includes('Date')">{{ data.value|beautify-date }}</span>
                            <span v-bind:key="index" v-else-if="field.key.includes('Act')">
                                <span 
                                    v-for="(act, actIndex) in data.value"  
                                    v-bind:key="actIndex"
                                    variant="outline-primary border-white"
                                    v-b-tooltip.hover                            
                                    :title="act.Description"
                                    :class="field.cellClass" 
                                    :style ="field.cellStyle">{{ act.Code }}<br>
                                </span>
                            </span>
                            <span v-bind:key="index" v-else-if="field.key.includes('Result') && data.value" 
                                variant="outline-primary border-white"
                                v-b-tooltip.hover                            
                                :title="data.item['Result Description']"
                                :class="field.cellClass" 
                                :style ="field.cellStyle">{{ data.value }}                                
                            </span>
                            <ul v-bind:key="index" v-else-if="field.key.includes('Issue')">
                                <li 
                                    v-for="(issue, issueIndex) in data.value"  
                                    v-bind:key="issueIndex"                                    
                                    :class="field.cellClass" 
                                    :style ="field.cellStyle">{{ issue }}
                                </li>
                            </ul>
                            <span v-bind:key="index" :class="field.cellClass" v-else>{{ data.value }}</span>

                    
                        </template>

                    </b-table>
                    <template v-slot:overlay>               
                        <div style="text-align: center"> 
                                <loading-spinner/> 
                                <p id="Downloading-label">Downloading PDF file ...</p>
                        </div>                
                    </template>
                </b-overlay>

                <div class="mt-5">
                    <h3 class="mx-2 font-weight-normal"> Scheduled Parties</h3>
                    <hr class="mb-0 bg-light" style="height: 5px;"/> 
                </div>                           
                <b-table
                :items="appearanceParties"
                :fields="partyFields"               
                borderless
                striped               
                responsive="sm"
                >   
                    <template v-for="(field,index) in partyFields" v-slot:[`head(${field.key})`]="data">
                        <b v-bind:key="index" :class="field.headerStyle" > {{ data.label }}</b>
                    </template>                
                    <template v-for="(field,index) in partyFields" v-slot:[`cell(${field.key})`]="data" >
                        <span v-bind:key="index" :style="field.cellStyle" v-if="data.field.key != 'Current Counsel'">  {{ data.value }} </span>
                        <span v-bind:key="index" :style="field.cellStyle" v-if="data.field.key == 'Current Counsel' && data.value">CEIS: {{ data.value }}</span> 
                    </template>
                </b-table>
                
            </b-col>
            <b-col col md="4" cols="4" style="overflow: auto;">
                 <div>
                    <h3 class="mx-2 font-weight-normal"> Additional Info</h3>
                    <hr class="mb-0 bg-light" style="height: 5px;"/> 
                </div>
                           
                <b-table
                :items="appearanceAdditionalInfo"
                :fields="addInfoFields"
                thead-class="d-none"               
                borderless                                  
                responsive="sm"
                >
                <template  v-slot:cell(key)="data">
                    <span >
                        <b > {{ data.value }}</b>
                    </span>                    
                </template>
                </b-table>                
            </b-col>          
        </b-row>
      </b-card>  
    </b-card> 

</div>
</template>

<script lang="ts">
import { Component, Vue } from "vue-property-decorator";
import { namespace } from "vuex-class";

import "@store/modules/CivilFileInformation";
const civilState = namespace("CivilFileInformation");


@Component
export default class CivilAppearanceDetails extends Vue {

    @civilState.State
    public civilFileInformation!: any;

    @civilState.State
    public appearanceInfo!: any;

    mounted() {
        this.getAdditionalInfo();
        this.getAppearanceDetails();
    }

    public getAppearanceDetails(): void {      
    
        this.$http.get('/api/files/civil/'+ this.additionalInfo["File Number"]+'/appearance-detail/'+this.additionalInfo["Appearance ID"])
            .then(Response => Response.json(), err => {console.log(err);} )        
            .then(data => {
                if(data){  
                    this.appearanceDetailsJson = data;              
                    this.ExtractAppearanceDetailsInfo();
                }
                this.isMounted = true;                       
            }); 
    }
    
    loadingPdf = false;  
    isMounted = false;
    isDataReady = false;
    appearanceDetailsJson;    
    
    appearanceDocuments: any[] = [];
    appearanceParties: any[] = [];
    additionalInfo = {};
    hoverRow =-1;
    hoverCol = 0;

    appearanceAdditionalInfo: any[] = [];

    addInfoFields =  
    [
        {key:'key',    sortable:false},
        {key:'value',  sortable:false},
    ];   

    documentFields =  
    [
        {key:'Seq.',           sortable:false,  tdClass: 'border-top', headerStyle:'text', cellClass:'text',             cellStyle: 'font-weight: normal; font-size: 14px; padding-top:12px'},
        {key:'Document Type',  sortable:false,  tdClass: 'border-top', headerStyle:'text', cellClass:'text',                  cellStyle: 'font-weight: normal; font-size: 14px; padding-top:12px'},
        {key:'Act',            sortable:false,  tdClass: 'border-top', headerStyle:'text', cellClass:'badge badge-dark mt-2', cellStyle: 'display: block; margin-top: 1px; font-size: 14px;'},
        {key:'Date Filed',     sortable:false,  tdClass: 'border-top', headerStyle:'text', cellClass:'text',                  cellStyle: 'font-weight: normal; font-size: 14px; padding-top:12px'},
        {key:'Result',         sortable:false,  tdClass: 'border-top', headerStyle:'text', cellClass:'badge badge-dark mt-2', cellStyle: 'display: block; margin-top: 1px; font-size: 14px;'},
        {key:'Issue',          sortable:false,  tdClass: 'border-top', headerStyle:'text', cellClass:'text',                  cellStyle: 'font-weight: normal; font-size: 14px; padding-top:6px'}
    ];
    
    partyFields =  
    [
        {key:'Name',                  sortable:false, tdClass: 'border-top',  headerStyle:'text',   cellStyle:'font-weight: bold; font-size: 14px;'},
        {key:'Role',                  sortable:false, tdClass: 'border-top',  headerStyle:'text',   cellStyle:'font-size: 14px;'},
        {key:'Current Counsel',       sortable:false, tdClass: 'border-top', headerStyle:'text',    cellStyle:'font-size: 14px;'}
    ];
    
    
    public getAdditionalInfo()
    {
        this.additionalInfo["Supplemental Equipment"] = this.appearanceInfo.supplementalEquipmentTxt;
        this.additionalInfo["Security Restriction"] = this.appearanceInfo.securityRestrictionTxt;
        this.additionalInfo["Out-Of-Town Judge"] =  this.appearanceInfo.outOfTownJudgeTxt;

        for(const info in this.additionalInfo)
            this.appearanceAdditionalInfo.push({'key':info,'value':this.additionalInfo[info]});

        this.additionalInfo["File Number"] = this.appearanceInfo.fileNo; 
        this.additionalInfo["Appearance ID"] = this.appearanceInfo.appearanceId;                  
    }

    public ExtractAppearanceDetailsInfo()
    {               
        for(const documentIndex in this.appearanceDetailsJson.document)
        {              
            const docInfo = {};
            const document = this.appearanceDetailsJson.document[documentIndex]              
            docInfo["Seq."] = document.fileSeqNo;
            docInfo["Document Type"]= document.documentTypeDescription
            docInfo["DocTypeCd"]= document.documentTypeCd
            docInfo["ID"]= document.civilDocumentId
            docInfo["PdfAvail"] = document.imageId? true : false
            docInfo["Act"] = [];            
            if (document.documentSupport && document.documentSupport.length > 0) {
                for (const act of document.documentSupport) {
                    docInfo["Act"].push({'Code': act.actCd, 'Description': act.actDsc})
                }
            }      
            
            docInfo["Date Filed"]= document.filedDt? document.filedDt.split(' ')[0] : '';
            docInfo["Result"]= document.appearanceResultCd
            docInfo["Result Description"]= document.appearanceResultDesc            
            docInfo["Issue"] = [];
            docInfo["Index"] = documentIndex;
            if (document.issue && document.issue.length > 0) {
                for (const issue of document.issue) {
                    docInfo["Issue"].push(issue.issueDsc)
                }
            }    

            this.appearanceDocuments.push(docInfo);
        }

        for(const party of this.appearanceDetailsJson.party)
        {              
            const partyInfo = {};             
            partyInfo["First Name"] = party.givenNm? party.givenNm: '';
            partyInfo["Last Name"] =  party.lastNm? party.lastNm: party.orgNm ;
            partyInfo["Name"] = this.getNameOfParty(partyInfo["Last Name"], partyInfo["First Name"])
            partyInfo["Current Counsel"] = party.counselNm? party.counselNm: '';
            partyInfo["Role"] = party.partyRoleTypeDesc? party.partyRoleTypeDesc: '';  

            this.appearanceParties.push(partyInfo);
        }
    }

    public getNameOfParty(lastName, givenName) {      

        if(lastName.length==0)        
            return givenName;       
        else if(givenName.length==0)       
            return lastName;      
         else if(givenName.length==0 && lastName.length==0)       
            return '';    
        else         
            return ( lastName + ", " + givenName );        
    }

    public documentClick(document) {

        if(document.PdfAvail && document.DocTypeCd != "CSR") {
            this.openDocumentsPdf(document['ID']);
        } else if (document.DocTypeCd == "CSR") {
            this.openCourtSummaryPdf(this.additionalInfo["Appearance ID"]);
        }

    }

    public documentCellClass(field, document) {        
        
        if ((document.PdfAvail && document.DocTypeCd != "CSR") || document.DocTypeCd == "CSR") {            

            if(this.hoverCol==1 && this.hoverRow==document.Index) return 'text-white bg-warning'; else return 'text-info';

        } else 
            return field.cellClass;
    }

    public openDocumentsPdf(documentId): void {
        this.loadingPdf = true;
        const filename = 'doc'+documentId+'.pdf';
        window.open(`/api/files/document/${documentId}/${filename}?isCriminal=false`)
        this.loadingPdf = false;
    }

    public openCourtSummaryPdf(appearanceId): void {

        this.loadingPdf = true;        
        const filename = 'court summary_'+appearanceId+'.pdf';
        window.open(`/api/files/civil/court-summary-report/${appearanceId}/${filename}`)
        this.loadingPdf = false;
    }

    public colHover(hovered, mouseEvent) {            
        hovered && mouseEvent.fromElement != null? this.hoverCol = mouseEvent.fromElement.cellIndex: this.hoverCol =-1;
    }

    public rowHover(row) {
        this.hoverRow = row.Index;
    }


}
</script>
