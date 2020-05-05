<template>
<body>
   <b-card bg-variant="white" border-variant="white">   
       
        <b-card bg-variant="light">
            <b-tabs active-nav-item-class="font-weight-bold text-uppercase text-info bg-light" pills >
                <b-tab 
                v-for="(filterword, index) in filterWords" 
                :key="index"                 
                :title="filterword.Name"                 
                v-on:click="activetab=index" 
                v-bind:class="[ activetab === index ? 'active' : '' ]"
                ></b-tab>
            </b-tabs>
        </b-card>
          
        <b-card border-variant="white"></b-card>
   
        <b-card bg-variant="light">
            <b-table
            :items="filteredItems"
            :fields="fields"
            :sort-by.sync="sortBy"
            :sort-desc.sync="sortDesc"
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

@Component
export default class CivilDocumentsView extends Vue {

    @civilState.State
    public civilFileDocument!: any

    public getDocuments(): void {
        
        this.$http.get('/files'+ this.civilFileDocument.fileNumber)
        .then(Response => {
            return Response.json()
        }).then(data => {
            this.documents = data
        })
    }

    mounted () {
        console.log(this.civilFileDocument.fileNumber)
        // this.getDocuments();
        
    }

    documents = '';   
    activetab= 0;            
    sortBy= 'Seq.';
    sortDesc= false;

    items= [
        {
            'Seq.':'2',
            'Document Type': 'Order',
            'Act': 'FLA',
            'Date Filed': '2018-09-09',
            'Issues': 'Parenting'
        },
        {
            'Seq.':'1',
            'Document Type': 'Affidavit',
            'Act': '',
            'Date Filed': '2019-05-20',
            'Issues': ''
        },
        {
            'Seq.':'3',
            'Document Type': 'Notice of Motion',
            'Act': '',
            'Date Filed': '2018-01-15',
            'Issues': 'prohibited'
        },
        {
            'Seq.':'4',
            'Document Type': 'Affidavit',
            'Act': 'DTM',
            'Date Filed': '2020-04-03',
            'Issues': ''
        }
    ];

    fields= [ 
        {key:'Seq.', sortable:true},
        {key:'Document Type',  sortable:true},
        {key:'Act', sortable:false},
        {key: 'Date Filed', sortable:true},
        {key: 'Issues', sortable:false}
    ];

    filterWords= [
        {Name:'All', AltWords:['all'], AltCdWord:['']}, 
        {Name:'Scheduled', AltWords:['Schedule','date'], AltCdWord:['']}, 
        {Name:'Pleadings', AltWords:['Pleading'], AltCdWord:['AEA','AEO','AFO','APC','APO','ARC','HCL','NFC','NRG','ORO','REC','REP','RES','RFC','RPC','RPL','RTC','SA','SAP','TC','WAG']}, 
        {Name:'Motions', AltWords:['Motion'], AltCdWord:['AAP','ACMW','AFCO','APJ','ATC','AXP','NM','NTRF']}, 
        {Name:'FS/Affidavits', AltWords:['Affidavit','witness'], AltCdWord:['AAS','ACD','AFB','AFBA','AFC','AFF','AFI','AFJ','AFM','AFS','AFSA','AFT','AOS','APS','CSA']}, 
        {Name:'Orders', AltWords:['Order'], AltCdWord:['ABO','AOD','CAO','CDO','CMCO','COR','COS','CPOR','CRT','DJ','DO','DOR','DPO','FCR','MCO','ODT','OFI','ORA','ORD','ORFJ','ORI','ORNA','ORT','ORW','OWN','PCH','PO','POD','POR','PVO','ROR','RSO','SPO']}, 
        {Name:'Concluded', AltWords:['conclude','finish','compelete'], AltCdWord:['']}, 
        {Name:'Court Summary', AltWords:['Reply'], AltCdWord:['']}
        ] ;          

    get filteredItems() {
    return this.items.filter(value => {
        if( this.activetab >0)
        {       
            for(const word of this.filterWords[this.activetab].AltWords) 
            {
                if(value["Document Type"].toUpperCase().includes(word.toUpperCase())){
                    return true
                }                    
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






<style scoped>

</style>

