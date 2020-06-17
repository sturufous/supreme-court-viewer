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
                    <b-button-group><h3 class="mx-2 mt-1 font-weight-normal" style="height: 10px;">Charges</h3>
                        <b-button                                     
                            variant="outline-primary text-info" 
                            style="border:0px;"
                            class="mt-1"
                            v-b-tooltip.hover.right
                            title="Download Record Of Proceeding"
                            @click="openRopPdf()"
                            size="sm">
                            <b-icon icon="file-earmark-arrow-down"></b-icon>
                        </b-button>
                    </b-button-group>
                    <hr class="mb-0 bg-light" style="height: 5px;"/> 
                </div>
                <b-card v-if="!(appearanceCharges.length>0)" style="border: white;">
                    <span class="text-muted"> No charges. </span>
                </b-card>                           
                <b-table
                style="max-height: 300px; overflow-y: auto;"
                 v-if="appearanceCharges.length>0"
                :items="appearanceCharges"
                :fields="chargesFields"               
                borderless
                striped               
                responsive="sm"
                >   
                    <template  v-slot:head="data">
                        <b> {{ data.label }}</b>
                    </template>                
                
                    <template v-slot:cell(Finding)="data" >                        
                        <b-badge 
                            variant="secondary" 
                            v-if="data.item['Finding']"                               
                            v-b-tooltip.hover.left 
                            :title='data.item["Finding Description"]'> 
                            {{ data.item["Finding"] }} 
                        </b-badge>                       
                    </template>

                    <template v-slot:cell(LastResult)="data" >
                        <b-badge 
                            variant="secondary"
                            v-if="data.item['Last Result']"
                            v-b-tooltip.hover.left 
                            :title='data.item["Last Result Description"]' > 
                            {{ data.item["Last Result"] }} 
                        </b-badge>
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
                     <b-button-group><h3 class="mx-2 mt-1 font-weight-normal" style="height: 10px;">Additional Info</h3>
                        <b-button 
                            size="sm"
                            style=" font-size:12px; border:0px;" 
                            @click="OpenNotes()"
                            variant="outline-primary text-info" 
                            v-if="notes.judgeRec.length> 0 || notes.appNote.length> 0" 
                            class="mt-1"
                            v-b-tooltip.hover
                            title="Notes">
                            <b-icon icon="chat-square-fill"></b-icon>                                
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
                        <template v-slot:cell(key)="data">
                            <b>{{data.value}}</b>
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
                            <span v-bind:key="index">
                                <span 
                                :class="data.field.cellClass"
                                :style="data.field.cellStyle"><b>{{ data.item.role }}</b> is appearing by {{data.item.method}}.<br>
                                </span>
                                <span 
                                v-if="data.item.phoneNumber.length>0" 
                                :class="data.field.cellClass"
                                :style="data.field.cellStyle"><b>Phone number: </b>{{data.item.phoneNumber}}.<br>
                                </span>
                                <span 
                                v-if="data.item.instruction.length>0" 
                                class="text"
                                :style="data.field.cellStyle">{{data.item.instruction}}
                                </span>
                                <span 
                                v-else-if="data.item.instruction.length==0" 
                                class="text-muted"
                                :style="data.field.cellStyle">No instructions.
                                </span>  
                            </span>

                        </template>
                </b-table>   
                
            </b-col>          
        </b-row>
        <b-overlay :show="loadingPdf" rounded="sm">
        <div class="mt-5">
            <b-button-group><h3 class="mx-2 mt-1 font-weight-normal" style="height: 10px;">Appearance Information</h3>
                <b-button
                    v-if="informationsFileExists"                                     
                    variant="outline-primary text-info" 
                    style="border:0px;"
                    class="mt-1"
                    v-b-tooltip.hover.right
                    title="Download Information File"
                    @click="openDocumentsPdf()"
                    size="sm">
                    <b-icon icon="file-earmark-arrow-down"></b-icon>
                </b-button>
            </b-button-group>
            <!-- <h3 class="mx-2 font-weight-normal" v-if="!informationsFileExists">Appearance Information</h3> -->
            <hr class="mb-0 bg-light" style="height: 5px;"/> 
        </div>
        <b-card v-if="!(appearanceMethodDetails.length> 0)" style="border: white;">
            <span class="text-muted"> No appearance information. </span>
        </b-card>                           
        <b-table
            v-if="appearanceMethodDetails.length> 0"
            style="max-height: 200px; overflow-y: auto;"
            :items="appearanceMethodDetails"
            :fields="appearanceFields"               
            borderless
            striped               
            responsive="sm"
            >
            <template  v-slot:head="data">
                <b> {{ data.label }}</b>
            </template>             
        
            <template v-slot:cell(Appearance)="data" :style="data.field.cellStyle">                        
                <span>{{ data.value }}</span>
                <span><br v-if="data.value.length>0">{{data.item.PartyAppearance}}</span>                      
            </template>           
        </b-table>
        <template v-slot:overlay>               
            <div style="text-align: center"> 
                    <loading-spinner/> 
                    <p id="Downloading-label">Downloading PDF file ...</p>
            </div>                
        </template>
        </b-overlay>
        


      </b-card>       
    </b-card>
    <b-modal v-if= "isMounted" v-model="showNotes" id="bv-modal-comment" hide-footer>
        <template v-slot:modal-title>
                <h2 class="mb-0">Notes</h2>
        </template>
        <b-card 
            v-if="notes.judgeRec.length>0" title="Judge Recommendation" border-variant="white">{{notes.judgeRec}}
        </b-card>
        <b-card 
            v-if="notes.appNote.length>0" title="Appearance Note" border-variant="white">{{notes.appNote}}
        </b-card>             
        <b-button class="mt-3 bg-info" @click="$bvModal.hide('bv-modal-comment')">Close</b-button>
    </b-modal>  
     
</div>
</template>

<script lang="ts">
import { Component, Vue } from "vue-property-decorator";
import { namespace } from "vuex-class";

import "@store/modules/CriminalFileInformation";
const criminalState = namespace("CriminalFileInformation");


@Component
export default class CriminalAppearanceDetails extends Vue {

    /* eslint-disable */
    @criminalState.State
    public criminalFileInformation!: any;

    @criminalState.State
    public appearanceInfo!: any;
    
    appearanceAdditionalInfo: any[] = [];
    appearanceCharges: any[] = [];    
    appearanceMethods: any[] = [];
    appearanceMethodDetails: any[] = [];
    /* eslint-enable */ 
  
    loadingPdf = false;
    isMounted = false;
    isDataReady = false;
    appearanceDetailsJson;    
    sortBy = 'Date';
    sortDesc = true;
    showNotes = false;
    informationsFileExists = false;
    notes = {};       
    appearanceDetailsInfo = {};
    initiatingDocuments: string[] = [];    

    addInfoFields =  
    [
        {key:'key',    sortable:false},
        {key:'value',  sortable:false},
    ];   

    chargesFields =  
    [
        {key:'Count',          sortable:false,  tdClass: 'border-top'},
        {key:'Criminal Code',  sortable:false,  tdClass: 'border-top'},
        {key:'Description',    sortable:false,  tdClass: 'border-top'},
        {key:'LastResult',     sortable:false,  tdClass: 'border-top'},
        {key:'Finding',        sortable:false,  tdClass: 'border-top'},
    ];

    appearanceMethodsField = 
    [
        {key:'Key', cellClass:'text-danger', cellStyle:'white-space: pre'}
    ]

    appearanceFields =  
    [
        {key:'Name',             sortable:false, tdClass: 'border-top',  cellStyle: 'font-size: 14px;'},
        {key:'Role',             sortable:false, tdClass: 'border-top',  cellStyle: 'font-size: 14px; white-space: pre-line;'},
        {key:'Appearance',       sortable:false, tdClass: 'border-top',  cellStyle: 'display: block; font-size: 14px; white-space: initial;'},
        {key:'Attendance',       sortable:false, tdClass: 'border-top',  cellStyle: 'display: block; font-size: 14px; white-space: initial;'}
        
    ];
    
    mounted() {
        this.getAppearanceInfo();
        this.getAppearanceDetails();
    }

    public getAppearanceDetails(): void {      
    
        this.$http.get('/api/files/criminal/'+ this.appearanceInfo.fileNo+'/appearance-detail/'+this.appearanceInfo.appearanceId+ '/'+this.appearanceInfo.partId)
            .then(Response => Response.json(), err => {console.log(err);} )        
            .then(data => {
                if(data){  
                    this.appearanceDetailsJson = data;              
                    this.ExtractAppearanceDetailsInfo();
                }
                this.isMounted = true;                       
            }); 
    }    
    
    public getAppearanceInfo()
    {       
        this.appearanceDetailsInfo["Supplemental Equipment"] = this.appearanceInfo.supplementalEquipmentTxt? this.appearanceInfo.supplementalEquipmentTxt: '';
        this.appearanceDetailsInfo["Security Restriction"] = this.appearanceInfo.securityRestrictionTxt? this.appearanceInfo.securityRestrictionTxt: '';
        this.appearanceDetailsInfo["Out-Of-Town Judge"] =  this.appearanceInfo.outOfTownJudgeTxt? this.appearanceInfo.outOfTownJudgeTxt: '';

        for(const info in this.appearanceDetailsInfo) {
            if(this.appearanceDetailsInfo[info].length>0)
            this.appearanceAdditionalInfo.push({'key':info,'value':this.appearanceDetailsInfo[info]});
        }
    }

    public ExtractAppearanceDetailsInfo()
    {
        const judgeRec = this.appearanceDetailsJson.judgesRecommendation? this.appearanceDetailsJson.judgesRecommendation: '';
        const appNote = this.appearanceDetailsJson.appearanceNote? this.appearanceDetailsJson.appearanceNote: '';
        this.notes =  {'judgeRec': judgeRec, 'appNote': appNote}        
        if (this.appearanceDetailsJson.initiatingDocuments && this.appearanceDetailsJson.initiatingDocuments.length>0) {
            this.initiatingDocuments.push(this.appearanceDetailsJson.initiatingDocuments[0])
            this.informationsFileExists = true;
        }
             
        for(const charge of this.appearanceDetailsJson.charges)
        {              
            const chargeInfo = {};             
            chargeInfo["Count"] = charge.printSeqNo;

            chargeInfo["Criminal Code"]= charge.statuteSectionDsc                 
            chargeInfo["Description"]= charge.statuteDsc

            chargeInfo["Last Result"]= charge.appearanceResultCd
            chargeInfo["Last Result Description"]= charge.appearanceResultDesc

            chargeInfo["Finding"]= charge.findingCd
            chargeInfo["Finding Description"]= charge.findingDsc

            this.appearanceCharges.push(chargeInfo);
        }

        for(const appearanceMethod of this.appearanceDetailsJson.appearanceMethods)
        {                         
            const methodInfo = {};             
            methodInfo["role"] = appearanceMethod.roleTypeDsc;
            methodInfo["method"] = appearanceMethod.appearanceMethodDesc;
            methodInfo["instruction"] = appearanceMethod.instructionTxt? appearanceMethod.instructionTxt: '';
            methodInfo["phoneNumber"] = appearanceMethod.phoneNumberTxt? appearanceMethod.phoneNumberTxt: '';
            this.appearanceMethods.push(methodInfo)
        }        

        if (this.appearanceDetailsJson.accused) {
            const accusedJson = this.appearanceDetailsJson.accused;
            const accused = {'Name': accusedJson.fullName,
                            'Role': 'Accused',
                            'Attendance': accusedJson.attendanceMethodDesc? accusedJson.attendanceMethodDesc: '',
                            'Appearance': accusedJson.appearanceMethodDesc? accusedJson.appearanceMethodDesc: '',
                            'PartyAppearance': accusedJson.partyAppearanceMethodDesc? accusedJson.partyAppearanceMethodDesc: ''
                            }
            this.appearanceMethodDetails.push(accused)
        }

        if (this.appearanceDetailsJson.prosecutor) {
            const prosecutorJson = this.appearanceDetailsJson.prosecutor;
            const prosecutor = {'Name': prosecutorJson.fullName,
                                'Role': 'Prosecutor',
                                'Attendance': prosecutorJson.attendanceMethodDesc? prosecutorJson.attendanceMethodDesc: '',
                                'Appearance': prosecutorJson.appearanceMethodDesc? prosecutorJson.appearanceMethodDesc: '',
                                'PartyAppearance': prosecutorJson.partyAppearanceMethodDesc? prosecutorJson.partyAppearanceMethodDesc: ''
                            }
            this.appearanceMethodDetails.push(prosecutor)
        }

        if (this.appearanceDetailsJson.adjudicator) {
            const adjudicatorJson = this.appearanceDetailsJson.adjudicator;
            const adjudicator = {'Name': adjudicatorJson.fullName,
                                'Role': 'Adjudicator',
                                'Attendance': adjudicatorJson.attendanceMethodDesc? adjudicatorJson.attendanceMethodDesc: '',
                                'Appearance': adjudicatorJson.appearanceMethodDesc? adjudicatorJson.appearanceMethodDesc: '',
                                'PartyAppearance': adjudicatorJson.partyAppearanceMethodDesc? adjudicatorJson.partyAppearanceMethodDesc: ''
                            }
            this.appearanceMethodDetails.push(adjudicator)

        }

        if (this.appearanceDetailsJson.justinCounsel) {
            const counselJson = this.appearanceDetailsJson.justinCounsel;
            const counsel = {'Name': counselJson.fullName,
                            'Role': 'Counsel',
                            'Attendance': counselJson.attendanceMethodDesc? counselJson.attendanceMethodDesc: '',
                            'Appearance': counselJson.appearanceMethodDesc? counselJson.appearanceMethodDesc: '',
                            'PartyAppearance': counselJson.partyAppearanceMethodDesc? counselJson.partyAppearanceMethodDesc: ''
                            }
            this.appearanceMethodDetails.push(counsel)
        }                
    }

    public OpenNotes() {        
        this.showNotes=true;           
    }

    public openDocumentsPdf(): void {
        this.loadingPdf = true;
        const imageId = this.initiatingDocuments[0]
        console.log(imageId)
        const filename = 'doc'+imageId+'.pdf';
        window.open(`/api/files/document/${imageId}/${filename}?isCriminal=true`)
        this.loadingPdf = false;
    }

    public openRopPdf(): void {
        this.loadingPdf = true;         
        const partID = this.appearanceInfo.partId;
        const profSeqNo = this.appearanceInfo.profSeqNo;      
        const filename = 'ROP_'+partID+'.pdf';
        const courtLevel = this.appearanceInfo.courtLevel;
        const courtClass = this.appearanceInfo.courtClass;
      
        const url =`/api/files/criminal/record-of-proceedings/${partID}/${filename}?profSequenceNumber=${profSeqNo}&courtLevelCode=${courtLevel}&courtClassCode=${courtClass}`;

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
