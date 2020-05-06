<template>
<body>
   <b-card bg-variant="white" border-variant="white">   
       
        <b-card bg-variant="light">
            <b-tabs active-nav-item-class="font-weight-bold text-uppercase text-info bg-light" pills >
                <b-tab 
                v-for="(tabMapping, index) in tabMappings" 
                :key="index"                 
                :title="tabMapping.Name"                 
                v-on:click="activetab=index" 
                v-bind:class="[ activetab === index ? 'active' : '' ]"
                ></b-tab>
            </b-tabs>
        </b-card>
          
        <b-card border-variant="white"></b-card>
   
        <b-card bg-variant="light">
            <b-table
            :items="FilteredDocuments"
            :fields="fields"
            :sort-by.sync="sortBy"
            :sort-desc.sync="sortDesc"
            :no-sort-reset="true"
            striped
            responsive="sm"
            >   
                <template v-slot:[`head(${fields[0].key})`]="data">
                    <b class="text-primary">{{ data.label }}</b>
                </template>

                <template v-slot:[`head(${fields[1].key})`]="data">
                    <b class="text-primary">{{ data.label }}</b>
                </template>

                <template v-slot:[`head(${fields[3].key})`]="data">
                    <b class="text-danger">{{ data.label }}</b>
                </template>

                <template v-slot:cell(Seq.)="data">
                    <span class="text">{{ data.value }}</span>
                </template>

                <template v-slot:[`cell(${fields[1].key})`]="data">
                    <b class="text-info">{{ data.value }}</b>
                </template>

                <template v-slot:[`cell(${fields[2].key})`]="data">
                    <b class="text-white bg-secondary">{{ data.value.toUpperCase() }}</b>
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

import json from '../assets/data.json';
import documentTypeCodesJson from '../assets/lookup/documentTypeCodes.json';

@Component
export default class CivilDocumentsView extends Vue {

    @civilState.State
    public civilFileDocument!: any

    public getDocuments(): void {
        
        this.$http.get('/files'+ this.civilFileDocument.fileNumber)
        .then(Response => {
            return Response.json()
        }).then(data => {
            this.documentsJson = data.document
        })
    }

    mounted () {
        console.log(this.civilFileDocument.fileNumber)
        // this.getDocuments();
        
        this.ExtractDocumentInfo();
        
        
    }

    documentTypeCodes = documentTypeCodesJson;
    documentsJson = json.document;
    activetab= 0;            
    sortBy= 'Seq.';
    sortDesc= false;

    documents: any[] = [];

    fields= [ 
        {key:'Seq.', sortable:true},
        {key:'Document Type',  sortable:true},
        {key:'Act', sortable:false},
        {key:'Date Filed', sortable:true},
        {key:'Issues', sortable:false}
    ];

    tabMappings= [
        {Name:'All', AltCdWords:['']}, 
        {Name:'Scheduled', AltCdWords:['']}, 
        {Name:'Pleadings', AltCdWords:['AEA','AEO','AFO','APC','APO','ARC','HCL','NFC','NRG','ORO','REC','REP','RES','RFC','RPC','RPL','RTC','SA','SAP','TC','WAG']}, 
        {Name:'Motions', AltCdWords:['AAP','ACMW','AFCO','APJ','ATC','AXP','NM','NTRF']}, 
        {Name:'FS/Affidavits', AltCdWords:['AAS','ACD','AFB','AFBA','AFC','AFF','AFI','AFJ','AFM','AFS','AFSA','AFT','AOS','APS','CSA']}, 
        {Name:'Orders', AltCdWords:['ABO','AOD','CAO','CDO','CMCO','COR','COS','CPOR','CRT','DJ','DO','DOR','DPO','FCR','MCO','ODT','OFI','ORA','ORD','ORFJ','ORI','ORNA','ORT','ORW','OWN','PCH','PO','POD','POR','PVO','ROR','RSO','SPO']}, 
        {Name:'Concluded', AltCdWords:['']}, 
        {Name:'Court Summary', AltCdWords:['']}
        ] ;          

    public ExtractDocumentInfo(): void {
               
        for(const doc of this.documentsJson)
        {
            const docInfo = {};           
            docInfo[this.fields[0].key] = doc.fileSeqNo;
            docInfo[this.fields[1].key] = this.ConvertCode2Description(doc.documentTypeCd);
            docInfo["Cd"] = doc.documentTypeCd;
            docInfo["Concluded"]= doc.concludedYn;

            const appearance: any = doc.appearance.slice(-1)[0];
            docInfo["Scheduled"]= appearance.appearanceDate;

            // ensure all documentSupport elements only have one row
            docInfo[this.fields[2].key] = doc.documentSupport[0].actCd;
            docInfo[this.fields[3].key] = doc.filedDt.split(" ")[0];
            // Once we have sample data with multiple issues we should rewrite it
            docInfo[this.fields[4].key] = doc.issue.length? doc.issue : ' ';

            this.documents.push(docInfo);
        }

    }

    public ConvertCode2Description(code: string): string{
       return this.documentTypeCodes.filter(item =>{ return item.code==code })[0].shortDesc;
    }

    get FilteredDocuments() {
        return this.documents.filter(doc => {
            if(this.activetab == 6) {
                if(doc["Concluded"] === "Y") return true; else return false;
            } else if(this.activetab == 1) {
                if(doc["Scheduled"]){        
                    if(new Date(doc["Scheduled"]) > new Date()) return true; else return false;
                } else {
                    return false
                }  

            } else if ( this.activetab >1 )
            {                  
                for(const word of this.tabMappings[this.activetab].AltCdWords) 
                {
                    if(doc["Cd"]==word) return true;                                    
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
</script>