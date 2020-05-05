<template>
<body>
   <div class="civil-documents-view">          
        <div>
            <b-card >
                <b-tabs v-model="tabIndex" small scard >
                    <b-tab 
                    v-for="(filterword, index) in filterWords" 
                    :key="index" 
                    :title="filterword.Name" 
                    v-on:click="activetab=index" 
                    ></b-tab>
                </b-tabs>
            </b-card>
        </div>   
        <div id="tabs" class="container">
            <div class="tabs">
                <a v-for="(filterword, index) in filterWords" :key="index" v-on:click="activetab=index" v-bind:class="[ activetab === index ? 'active' : '' ]"> {{filterword.Name}}</a> 
            </div>
        </div>
   
        <div>
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
        </div>
   </div> 
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
    /* .btn-group {
        background-color: #F0F0F0; 
        border: 1px solid rgba(0, 128, 0, 0.034);
        color: white;
        padding: 15px 32px;
        text-align: center;
        text-decoration: none;
        display: inline-block;
        font-size: 16px;
        cursor: pointer;
        float: left;
    } */

    .civil-documents-view {
        margin-left: 25px;
        margin-right: 25px;
    }
   

    .container {  
        max-width: 1620px; 
        min-width: 420px;
        margin: 40px auto;
        font-family: Arial, Helvetica, sans-serif;
        font-size: 0.9em;
        color: #888;
        background-color: #F0F0F0; /* Green */
    }

    /* Style the tabs */
    .tabs {
        overflow: hidden;
        margin-left: 20px;
        margin-bottom: -2px; 
    }

    .tabs ul {
        list-style-type: none;
        margin-left: 20px;
    }

    .tabs a{
        float: left;
        cursor: pointer;
        padding: 12px 24px;
        transition: background-color 0.2s;
        border: 1px solid #ccc;
        border-right: none;
        background-color: #f1f1f1;
        border-radius: 5px 5px 5px 5px;
        font-weight: bold;
    }
    .tabs a:last-child { 
        border-right: 1px solid #ccc;
    }

    /* Change background color of tabs on hover */
    .tabs a:hover {
        background-color: #aaa;
        color: #fff;
    }

    /* Styling for active tab */
    .tabs a.active {
        background-color: #fff;
        color: #484848;
        border-bottom: 2px solid #fff;
        cursor: default;
    }

    /* Style the tab content */
    .tabcontent {
        padding: 30px;
        border: 1px solid #ccc;
        border-radius: 10px;
    box-shadow: 3px 3px 6px #e1e1e1
    }

</style>

