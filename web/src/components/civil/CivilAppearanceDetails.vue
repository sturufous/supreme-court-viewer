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
                        <b-button-group>
                            <h3 class="mx-2 mt-2 font-weight-normal" style="height: 10px;"> Document Summary</h3> 
                            <b-button                                     
                                variant="outline-primary text-info" 
                                style="transform:translate(0,5px);border:0px;"
                                class="mt-0"
                                v-b-tooltip.hover.right                                
                                title="Download Court Summary"
                                @click="openCourtSummaryPdf(appearanceInfo.appearanceId)"
                                size="sm">                                        
                                <b-icon icon="file-earmark-arrow-down" font-scale="2"></b-icon>
                            </b-button>
                        </b-button-group>                        
                        <hr class="mb-0 bg-light" style="height: 5px;"/> 
                    </div>
                    <b-card v-if="!(appearanceDocuments.length>0)" style="border: white;">
                        <span class="text-muted"> No documents. </span>
                    </b-card>                           
                    <b-table
                        style="max-height: 300px; overflow-y: auto;"
                        v-if="appearanceDocuments.length > 0"
                        :items="appearanceDocuments"
                        :fields="documentFields"               
                        borderless                     
                        small               
                        responsive="sm"
                        >
                            <template v-slot:head(Result)>
                                <b>Result</b><b style="margin-left: 20px">Issues</b>
                            </template>

                            <template v-slot:[`cell(${documentFields[1].key})`]="data" >
                                <b-button 
                                    v-if="data.item.PdfAvail && !data.item.Sealed" 
                                    variant="outline-primary text-info" 
                                    :style="data.field.cellStyle"
                                    @click="documentClick(data)"
                                    size="sm">
                                        {{data.value}}
                                </b-button>
                                <span
                                    class="ml-2"
                                    :style="data.field.cellLabelStyle"
                                    v-else-if="!data.item.PdfAvail && !data.item.Sealed">
                                        {{data.value}}
                                </span>
                                <span class="ml-2 text-muted"
                                      v-else-if="data.item.Sealed"
                                      :style="data.field.cellLabelStyle">
                                    {{data.value}}
                                </span>
                            </template>

                            <template v-slot:cell(Act)="data" >
                                <b-badge 
                                    variant="secondary"
                                    :style="data.field.cellStyle"                     
                                    v-for="(act, actIndex) in data.value"  
                                    v-bind:key="actIndex"                               
                                    v-b-tooltip.hover.left
                                    :title="act.Description"> 
                                        {{act.Code}} 
                                </b-badge>
                            </template>
                            <template v-slot:cell(Seq.)="data">
                                <span v-if="data.item.Sealed" class="ml-2 text-muted" :style="data.field.cellStyle"> 
                                    {{data.value}}
                                </span>
                                <span v-else class="ml-2" :style="data.field.cellStyle"> 
                                    {{data.value}}
                                </span>
                            </template>
                            <template v-slot:[`cell(${documentFields[3].key})`]="data" >
                                <span v-if="data.item.Sealed" 
                                      :style="data.field.cellStyle"
                                      class="text-muted">
                                    {{ data.value | beautify-date}}
                                </span>
                                <span v-else :style="data.field.cellStyle">
                                    {{ data.value | beautify-date}}
                                </span>
                            </template>

                            <template v-slot:cell(Result)="data" >
                                <b-table
                                    :items="data.item.Issues"
                                    :fields="issueFields"
                                    borderless 
                                    thead-class="d-none"
                                    small               
                                    responsive="sm">

                                    <template v-slot:table-colgroup>
                                        <col style=" width: 70px ">
                                    </template>
                                    
                                    <template v-slot:cell(Issue)="dataR" >
                                        <li v-if="data.item.Sealed" 
                                            class="text-muted" 
                                            :style="dataR.field.cellStyle">                                        
                                            {{ dataR.value }}
                                        </li>
                                        <li v-else
                                            :style="dataR.field.cellStyle">                                        
                                            {{ dataR.value }}
                                        </li>
                                    </template>
                                    <template v-slot:cell(Result)="dataR" >
                                        <span :style="dataR.field.cellStyle">
                                            <b-badge
                                                v-if="dataR.value"                                     
                                                variant="secondary"
                                                v-b-tooltip.hover.left
                                                :title="dataR.item['ResultDsc']">
                                                    {{dataR.value}}                               
                                            </b-badge>
                                        </span>
                                    </template>
                                </b-table>     

                            </template> 
                    </b-table>
                    <template v-slot:overlay>               
                        <div style="text-align: center"> 
                                <loading-spinner/> 
                                <p id="Downloading-label">Downloading PDF file ...</p>
                        </div>                
                    </template>
                </b-overlay>                                 
            </b-col>
            <b-col col md="4" cols="4" style="overflow: auto;">
                <div>
                    <b-button-group>
                        <h3 class="mx-2 font-weight-normal" style="margin-top:8px; height:10px"> Additional Info</h3>
                        <b-button 
                            size="sm"
                            style=" font-size:12px; border:0px;" 
                            @click="OpenAdjudicatorComment()"
                            variant="outline-primary text-info"
                            v-if="adjudicatorComment.length> 0" 
                            class="mt-1"
                            v-b-tooltip.hover.right
                            title="Adjudicator Comment">
                            <b-icon icon="chat-square-fill" font-scale="2"></b-icon>                                
                        </b-button>
                    </b-button-group>
                    <hr class="mb-0 bg-light" style="height: 5px;"/> 
                </div>
                <b-card v-if="!(appearanceAdditionalInfo.length>0)" style="border: white;">
                    <span class="text-muted"> No additional information. </span>
                </b-card>                           
                <b-table
                    v-if="appearanceAdditionalInfo.length> 0"
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

                <div v-if="appearanceMethods.length> 0">
                    <h3 class="mx-2 font-weight-normal"> Appearance Methods</h3>
                    <hr class="mb-0 bg-light" style="height: 5px;"/> 
                </div>                           
                <b-table
                    v-if="appearanceMethods.length> 0"
                    :items="appearanceMethods"
                    :fields="appearanceMethodsField"
                    thead-class="d-none"               
                    borderless                              
                    responsive="sm"
                    >
                        <template v-for="(field,index) in appearanceMethodsField" v-slot:[`cell(${field.key})`]="data" >                   
                            <span v-bind:key="index" :class="data.field.cellClass" :style="data.field.cellStyle"><b>{{ data.item.role }}</b> is appearing by {{data.item.method}} </span>
                        </template>
                </b-table>                                       
            </b-col>          
        </b-row>
        <div class="mt-5">
            <h3 class="mx-2 font-weight-normal"> Scheduled Parties</h3>
            <hr class="mb-0 bg-light" style="height: 5px;"/> 
        </div>                           
        <b-table
            style="max-height: 200px; overflow-y: auto;"
            :items="appearanceParties"
            :fields="partyFields"               
            borderless
            striped               
            responsive="sm"
            >   
            <template v-for="(field,index) in partyFields" v-slot:[`head(${field.key})`]="data">
                <b v-bind:key="index"> {{ data.label }}</b>
            </template>                
            <template v-for="(field,index) in partyFields" v-slot:[`cell(${field.key})`]="data" >                                                        
                <span v-bind:key="index" :style="field.cellStyle" v-if="data.field.key == 'Current Counsel' && data.value.length>0">
                    <span v-for="(counsel, counselIndex) in data.value" v-bind:key="counselIndex">
                        <span v-if="counsel.Info.length == 0">CEIS: {{ counsel.Name }}<br></span>
                        <span
                            class="text-success"  
                            v-bind:key="index"
                            v-else-if="counsel.Info.length > 0"
                            v-b-tooltip.hover.right.html="counsel.Info"> 
                                CEIS: {{ counsel.Name }} 
                        <br></span>
                    </span>
                </span>
                <span v-bind:key="index" :style="field.cellStyle" v-else-if="data.field.key == 'Role' && data.value.length>0">
                    <span v-for="(role, roleIndex) in data.value"  v-bind:key="roleIndex">{{ role }}<br></span>
                </span>
                <span v-bind:key="index" :style="field.cellStyle" v-else-if="data.field.key == 'Name' && data.item.Info.length == 0">  {{ data.value }} </span>
                <span
                    class="text-success"  
                    v-bind:key="index"
                    :style="field.cellStyle"
                    v-else-if="data.field.key == 'Name' && data.item.Info.length > 0"                    
                    v-b-tooltip.hover.right.html="data.item.Info"
                    >
                    {{ data.value }} 
                </span>
                <span v-bind:key="index" :style="field.cellStyle" v-else-if="data.field.key == 'Representative' && data.value.length>0">
                    <span v-for="(rep, repIndex) in data.value" v-bind:key="repIndex">
                        <span v-if="rep.Info.length == 0">{{ rep.Name }}<br></span>
                        <span
                            class="text-success"  
                            v-bind:key="index"
                            v-else-if="rep.Info.length > 0"
                            v-b-tooltip.hover.left.html="rep.Info">
                            {{ rep.Name }} 
                        <br></span>
                    </span>
                </span>
                <span v-bind:key="index" :style="field.cellStyle" v-else-if="data.field.key == 'Legal Representative' && data.value.length>0">
                    <span v-for="(legalRep, legalRepIndex) in data.value"  v-bind:key="legalRepIndex"><b>{{ legalRep.Type }}</b>-{{legalRep.Name}}<br></span>
                </span>

            </template>
        </b-table>
      </b-card>  
    </b-card>

    <b-modal v-if= "isMounted" v-model="showAdjudicatorComment" id="bv-modal-comment" hide-footer>
        <template v-slot:modal-title>
                <h2 class="mb-0"> Adjudicator Comment </h2>
        </template>
        <b-card border-variant="white">{{adjudicatorComment}}</b-card>            
        <b-button class="mt-3 bg-info" @click="$bvModal.hide('bv-modal-comment')">Close</b-button>
    </b-modal> 

</div>
</template>

<script lang="ts">
import { Component, Vue, Prop } from "vue-property-decorator";
import { namespace } from "vuex-class";
import base64url from "base64url";
import "@store/modules/CommonInformation";
import "@store/modules/CivilFileInformation";
import {appearanceAdditionalInfoType, civilAppearanceInfoType, appearancePartiesType, appearanceMethodsType, appearanceDocumentsType} from '../../types/civil';
import {inputNamesType } from '../../types/common'
const civilState = namespace("CivilFileInformation");
const commonState = namespace("CommonInformation");

@Component
export default class CivilAppearanceDetails extends Vue {

    @commonState.State
    public displayName!: string;   

    @civilState.State
    public appearanceInfo!: civilAppearanceInfoType;        

    @commonState.Action
    public UpdateDisplayName!: (newInputNames: inputNamesType) => void

    appearanceAdditionalInfo: appearanceAdditionalInfoType[] = [];
    appearanceDocuments: appearanceDocumentsType[] = [];
    appearanceParties: appearancePartiesType[] = [];
    appearanceMethods: appearanceMethodsType[] = [];    
    
    loadingPdf = false; 
    isMounted = false;
    isDataReady = false;
    stripedStyle = false;
    appearanceDetailsJson; 
    additionalInfo = {};
    adjudicatorComment = '';
    showAdjudicatorComment = false;

    addInfoFields =  
    [
        {key:'key',    sortable:false},
        {key:'value',  sortable:false},
    ];   
   
    issueFields =
    [
        {key:'Result',  sortable:false,  cellStyle: 'display: block; font-size: 14px;'},
        {key:'Issue',   sortable:false,  cellStyle: 'font-weight: normal; font-size: 14px; padding-top:4px; line-height: 120%;'}
    ]

    documentFields =  
    [
        {key:'Seq.',           sortable:false,  tdClass: 'border-top', cellClass:'text',                  cellStyle: 'font-weight: normal; font-size: 14px; padding-top:12px'},
        {key:'Document Type',  sortable:false,  tdClass: 'border-top', cellClass:'text',                  cellStyle: 'border:0px; font-size: 14px;', cellLabelStyle: 'font-weight: normal; font-size: 14px; padding-top:12px'},
        {key:'Act',            sortable:false,  tdClass: 'border-top', cellClass:'badge badge-dark mt-2', cellStyle: 'display: block; margin-top: 1px; font-size: 12px; max-width : 50px;'},
        {key:'Date Filed',     sortable:false,  tdClass: 'border-top', cellClass:'text',                  cellStyle: 'font-weight: normal; font-size: 14px; padding-top:12px'},
        {key:'Result',         sortable:false,  tdClass: 'border-top', cellClass:'badge badge-dark mt-2', cellStyle: 'display: block; margin-top: 1px; font-size: 14px;'}
    ];
    
    partyFields =  
    [
        {key:'Name',                  sortable:false, tdClass: 'border-top',  cellStyle:'font-weight: bold; font-size: 14px;'},
        {key:'Role',                  sortable:false, tdClass: 'border-top',  cellStyle:'font-size: 14px; white-space: pre-line;'},
        {key:'Current Counsel',       sortable:false, tdClass: 'border-top',  cellStyle: 'display: block; font-size: 14px; white-space: initial;'},
        {key:'Legal Representative',  sortable:false, tdClass: 'border-top',  cellStyle:'font-size: 14px; white-space: pre-line;'},
        {key:'Representative',        sortable:false, tdClass: 'border-top',  cellStyle: 'display: block; font-size: 14px; white-space: initial;'}
        
    ];

    appearanceMethodsField = 
    [
        {key:'Key', cellClass:'text-danger', cellStyle:'white-space: pre-line'}
    ]


    mounted() {
        this.getAdditionalInfo();
        this.getAppearanceDetails();
    }

    @Prop() tagcasename

    public getAppearanceDetails(): void {      
            
        this.$http.get('api/files/civil/'+ this.appearanceInfo.fileNo +'/appearance-detail/'+this.appearanceInfo.appearanceId)
            .then(Response => Response.json(), err => {console.log(err);window.alert("bad data!");} )        
            .then(data => {
                if(data){ 
                    this.appearanceDetailsJson = data;              
                    this.ExtractAppearanceDetailsInfo(); 
                        const element = document.getElementById(this.tagcasename);                        
                        // console.log(this.tagcasename)
                        // console.log(element)
                        if(element !=null)
                            setTimeout(() => {element.scrollIntoView(); }, 100);                  
                }
                else{
                    window.alert("bad data!");
                }
                this.isMounted = true;                       
            }); 
    }    
    
    public getAdditionalInfo()
    {   
        this.additionalInfo["Supplemental Equipment"] = this.appearanceInfo.supplementalEquipmentTxt? this.appearanceInfo.supplementalEquipmentTxt: '';
        this.additionalInfo["Security Restriction"] = this.appearanceInfo.securityRestrictionTxt? this.appearanceInfo.securityRestrictionTxt: '';
        this.additionalInfo["Out-Of-Town Judge"] =  this.appearanceInfo.outOfTownJudgeTxt? this.appearanceInfo.outOfTownJudgeTxt: '';

        for(const info in this.additionalInfo)
            if(this.additionalInfo[info].length>0)
            this.appearanceAdditionalInfo.push({'key':info,'value':this.additionalInfo[info]});                      
    }

    public ExtractAppearanceDetailsInfo()
    {
        this.adjudicatorComment = this.appearanceDetailsJson.adjudicatorComment? this.appearanceDetailsJson.adjudicatorComment: '';
        for(const documentIndex in this.appearanceDetailsJson.document)
        {              
            const docInfo = {} as appearanceDocumentsType;
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
            
            if (document.sealedYN == "Y") {                
                docInfo["Sealed"] = true;
            } else {
                docInfo["Sealed"] = false;
            }
            
            docInfo["Date Filed"]= document.filedDt? document.filedDt.split(' ')[0] : '';
            docInfo["Result"]= document.appearanceResultCd
            docInfo["Result Description"]= document.appearanceResultDesc
            docInfo["Issues"] = [];
            docInfo["Index"] = documentIndex;
            if (document.issue && document.issue.length > 0) {
                for (const issue of document.issue) {
                    docInfo["Issues"].push({'Issue': issue.issueDsc, 'Result': issue.issueResultCd, 'ResultDsc': issue.issueResultDsc})
                }
            }    

            this.appearanceDocuments.push(docInfo);
        }

        for(const party of this.appearanceDetailsJson.party)
        {              
            const partyInfo = {} as appearancePartiesType;             
            partyInfo["First Name"] = party.givenNm? party.givenNm: '';
            partyInfo["Last Name"] =  party.lastNm? party.lastNm: party.orgNm ;
            this.UpdateDisplayName({'lastName': partyInfo["Last Name"], 'givenName': partyInfo["First Name"]});
            partyInfo["Name"] = this.displayName
            partyInfo["Info"] = '';
            if (party.appearanceMethodDesc) {
                partyInfo["Info"] = 'Appeared by ' + party.appearanceMethodDesc;
            }
            if (party.partyAppearanceMethodDesc) {
                if (partyInfo["Info"].length > 0) {
                    partyInfo["Info"] += '<br>'
                }
                //TODO: remove the pre-text when the longDesc is passed through the api
                partyInfo["Info"]+= 'Appearance: ' + party.partyAppearanceMethodDesc;
            }
            if (party.attendanceMethodDesc) {
                if (partyInfo["Info"].length > 0) {
                    partyInfo["Info"] += '<br>'
                }
                partyInfo["Info"]+= 'Attendance: ' + party.attendanceMethodDesc;
            }
            partyInfo["Current Counsel"] = [];
            if (party.counsel && party.counsel.length > 0) {
                for (const counsel of party.counsel) {
                    let info = '';
                    if (counsel.phoneNumber) {
                        info = "Phone Number: " + counsel.phoneNumber
                    }
                    if (counsel.counselAppearanceMethodDesc) {
                        if (info.length > 0) {
                            info += '\n'
                        }
                        info+= "Appeared by " + counsel.counselAppearanceMethodDesc
                    }
                    partyInfo["Current Counsel"].push({"Name": counsel.counselFullName, "Info": info})
                }
            }
            partyInfo["Representative"] = [];
            if (party.representative && party.representative.length > 0) {
                for (const rep of party.representative) {
                    let info = '';
                    if (rep.phoneNumber) {
                        info = "Phone Number: " + rep.phoneNumber
                    }                    
                    if (rep.attendenceMethodDsc) {
                        if (info.length > 0) {
                            info += '<br>'
                        }
                        info+= "Attended by " + rep.attendanceMethodDesc
                    }
                    if (rep.instruction) {
                        if (info.length > 0) {
                            info += '<br>'
                        }
                        info+= "Instruction: " + rep.instruction
                    }
                    partyInfo["Representative"].push({"Name": rep.repFullName,
                                                      "Info": info})
                }
            }
            partyInfo["Legal Representative"] = [];
            if (party.legalRepresentative && party.legalRepresentative.length > 0) {
                for (const legalRep of party.legalRepresentative) {
                    partyInfo["Legal Representative"].push({"Name": legalRep.legalRepFullName, "Type": legalRep.legalRepTypeDsc})
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
        
        for(const appearanceMethod of this.appearanceDetailsJson.appearanceMethod)
        {              
            const methodInfo = {} as appearanceMethodsType;             
            methodInfo["role"] = appearanceMethod.roleTypeDesc;
            methodInfo["method"] = appearanceMethod.appearanceMethodDesc;
            this.appearanceMethods.push(methodInfo)
        }
    }

    public documentClick(document) 
    {
        if(document.item.DocTypeCd != "CSR") {
            this.openDocumentsPdf(document.item['ID']);
        } else if (document.item.DocTypeCd == "CSR") {
            this.openCourtSummaryPdf(this.appearanceInfo.appearanceId);
        }
    }

    public openDocumentsPdf(documentId): void {
        this.loadingPdf = true;
        const filename = 'doc' + documentId + '.pdf';
        documentId = base64url(documentId);
        window.open(`${process.env.BASE_URL}api/files/document/${documentId}/${filename}?isCriminal=false&fileId=${this.appearanceInfo.fileNo}`)
        this.loadingPdf = false;
    }

    public openCourtSummaryPdf(appearanceId): void {

        this.loadingPdf = true;        
        const filename = 'court summary_'+appearanceId+'.pdf';
        window.open(`${process.env.BASE_URL}api/files/civil/court-summary-report/${appearanceId}/${filename}?vcCivilFileId=${this.appearanceInfo.fileNo}`)
        this.loadingPdf = false;
    }

    public OpenAdjudicatorComment() {        
        this.showAdjudicatorComment=true;           
    }
}
</script>