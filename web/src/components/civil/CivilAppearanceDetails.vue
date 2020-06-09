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

    <b-card bg-variant="light" v-else no-body>
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
                        small               
                        responsive="sm"
                        >   
                            <template v-slot:[`cell(${documentFields[1].key})`]="data" >
                                <b-button 
                                    v-if="data.item.PdfAvail" 
                                    variant="outline-primary text-info" 
                                    style="border:0px;"
                                    @click="documentClick(data)"
                                    size="sm">
                                        {{data.value}}
                                </b-button>
                                <span class="ml-2" v-else>
                                    {{data.value}}
                                </span>
                            </template>

                            <template v-slot:cell(Act)="data" >
                                <b-badge 
                                    variant="secondary"
                                    style="display: block; margin-top: 1px; font-size: 14px; max-width : 50px;"                     
                                    v-for="(act, actIndex) in data.value"  
                                    v-bind:key="actIndex"                               
                                    v-b-tooltip.hover.left
                                    :title="act.Description"> 
                                        {{act.Code}} 
                                </b-badge>
                            </template>                            

                            <template v-slot:[`cell(${documentFields[3].key})`]="data" >
                                {{ data.value | beautify-date}}
                            </template>

                             <template v-slot:cell(Result)="data" >
                                <b-badge
                                    v-if="data.value"  
                                    variant="secondary"
                                    v-b-tooltip.hover.left                            
                                    :title="data.item['Result Description']">
                                        {{data.value}}                                 
                                </b-badge>
                             </template>

                            <template v-slot:cell(Issues)="data" >                               
                                <li 
                                    v-for="(issue, issueIndex) in data.value"  
                                    v-bind:key="issueIndex"
                                    style="line-height: 100%;">
                                        {{ issue }}
                                </li>
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
                            <span v-bind:key="index" :style="field.cellStyle" v-if="data.field.key == 'Current Counsel' && data.value.length>0">
                                <span v-for="(counsel, counselIndex) in data.value"  v-bind:key="counselIndex" style= "white-space: pre-line" >CEIS: {{ counsel }}<br></span>
                            </span>
                            <span v-bind:key="index" :style="field.cellStyle" v-else-if="data.field.key == 'Role' && data.value.length>0">
                                <span v-for="(role, roleIndex) in data.value"  v-bind:key="roleIndex" style= "white-space: pre-line" >{{ role }}<br></span>
                            </span>
                            <span v-bind:key="index" :style="field.cellStyle" v-else-if="data.field.key == 'Name'">  {{ data.value }} </span> 
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
                            <b > {{ data.value }}</b>                                               
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
import "@store/modules/CommonInformation";
import "@store/modules/CivilFileInformation";
const civilState = namespace("CivilFileInformation");
const commonState = namespace("CommonInformation");

@Component
export default class CivilAppearanceDetails extends Vue {

    @commonState.State
    public displayName!: string;

     /* eslint-disable */
    @civilState.State
    public civilFileInformation!: any;

    @civilState.State
    public appearanceInfo!: any;        

    @commonState.Action
    public UpdateDisplayName!: (newInputNames: any) => void

    appearanceAdditionalInfo: any[] = [];
    appearanceDocuments: any[] = [];
    appearanceParties: any[] = [];
    /* eslint-enable */
    
    loadingPdf = false;  
    isMounted = false;
    isDataReady = false;
    appearanceDetailsJson; 
    additionalInfo = {};

    addInfoFields =  
    [
        {key:'key',    sortable:false},
        {key:'value',  sortable:false},
    ];   

    documentFields =  
    [
        {key:'Seq.',           sortable:false,  tdClass: 'border-top', headerStyle:'text', cellClass:'text',                  cellStyle: 'font-weight: normal; font-size: 14px; padding-top:12px'},
        {key:'Document Type',  sortable:false,  tdClass: 'border-top', headerStyle:'text', cellClass:'text',                  cellStyle: 'font-weight: normal; font-size: 14px; padding-top:12px'},
        {key:'Act',            sortable:false,  tdClass: 'border-top', headerStyle:'text', cellClass:'badge badge-dark mt-2', cellStyle: 'display: block; margin-top: 1px; font-size: 14px;'},
        {key:'Date Filed',     sortable:false,  tdClass: 'border-top', headerStyle:'text', cellClass:'text',                  cellStyle: 'font-weight: normal; font-size: 14px; padding-top:12px'},
        {key:'Result',         sortable:false,  tdClass: 'border-top', headerStyle:'text', cellClass:'badge badge-dark mt-2', cellStyle: 'display: block; margin-top: 1px; font-size: 14px;'},
        {key:'Issues',          sortable:false,  tdClass: 'border-top', headerStyle:'text', cellClass:'text',                 cellStyle: 'font-weight: normal; font-size: 14px; padding-top:6px'}
    ];
    
    partyFields =  
    [
        {key:'Name',                  sortable:false, tdClass: 'border-top',  headerStyle:'text',   cellStyle:'font-weight: bold; font-size: 14px;'},
        {key:'Role',                  sortable:false, tdClass: 'border-top',  headerStyle:'text',   cellStyle:'font-size: 14px;'},
        {key:'Current Counsel',       sortable:false, tdClass: 'border-top', headerStyle:'text',    cellStyle:'font-size: 14px;'}
    ];

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
            docInfo["Issues"] = [];
            docInfo["Index"] = documentIndex;
            if (document.issue && document.issue.length > 0) {
                for (const issue of document.issue) {
                    docInfo["Issues"].push(issue.issueDsc)
                }
            }    

            this.appearanceDocuments.push(docInfo);
        }

        for(const party of this.appearanceDetailsJson.party)
        {              
            const partyInfo = {};             
            partyInfo["First Name"] = party.givenNm? party.givenNm: '';
            partyInfo["Last Name"] =  party.lastNm? party.lastNm: party.orgNm ;
            this.UpdateDisplayName({'lastName': partyInfo["Last Name"], 'givenName': partyInfo["First Name"]});
            partyInfo["Name"] = this.displayName
            partyInfo["Current Counsel"] = [];
            if (party.counsel && party.counsel.length > 0) {
                for (const counsel of party.counsel) {
                    partyInfo["Current Counsel"].push(counsel.counselFullName)
                }
            }
            partyInfo["Role"] = [];
            if (party.partyRole && party.partyRole.length > 0) {
                for (const role of party.partyRole) {
                    partyInfo["Role"].push(role.roleTypeDsc)
                }
            }
            this.appearanceParties.push(partyInfo);
        }
    }

    public documentClick(document) 
    {
        if(document.item.DocTypeCd != "CSR") {
            this.openDocumentsPdf(document.item['ID']);
        } else if (document.item.DocTypeCd == "CSR") {
            this.openCourtSummaryPdf(this.additionalInfo["Appearance ID"]);
        }
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


}
</script>
