
<template>
<body>
   <div>

   </div>    
    <div id="tabs" class="container">
        <div class="tabs">
            <a v-for="(filterword, index) in filterWords" :key="index" v-on:click="activetab=index" v-bind:class="[ activetab === index ? 'active' : '' ]"> {{filterword.Name}}</a> 
        </div>
    </div>

    <table class="table table-condensed table-striped">
      <thead>
          <tr>
              <th v-for="(column, index) in columnsColor" :key="index" v-on:click="sortCol(index)" v-bind:style="{ color:`${column.Color}` }"> 
                  {{column.Name}}                  
                  <div v-bind:style="arrow[column.ArrowDir]"></div>
              </th>              
          </tr>
      </thead>
      <tbody>
          <tr v-for="(item, index) in sortedFilteredItems" :key="index">                   
               <td v-for="(column, indexColumn) in columnsColor" :key="indexColumn" v-bind:style="{ 'color':`${column.bodyColor}`}">
                   <span v-bind:style="{ 'background-color':`${column.bodyBgColor}`}">
                        {{item[column.Name]}}
                   </span>
               </td> 
          </tr>
      </tbody>
    </table>
</body>
</template>

<script lang="ts">
import { Component, Vue } from 'vue-property-decorator';

@Component
export default class CivilDocumentsView extends Vue{
    
    
            activetab= 0;
            msg= 'red'; 
            currentSortIndex= 0;
            //sortDirection= 'desc';      

            arrow = [
                {
                'width': 0, 
                'height': 0,                 
                'border-left': '5px solid transparent',
                'border-right': '5px solid transparent',
                'display':'inline-block',
                'vertical-align':'top',
                'border-top':'5px solid black',
                'border-bottom': '0'
                },
                {
                'width': 0, 
                'height': 0,                 
                'border-left': '5px solid transparent',
                'border-right': '5px solid transparent',
                'display':'inline-block',
                'vertical-align':'top',
                'border-top':'0',
                'border-bottom': '5px solid black'
                },
                {
                'width': 0, 
                'height': 0,                 
                'border-left': '5px solid transparent',
                'border-right': '5px solid transparent',
                'display':'inline-block',
                'vertical-align':'top',
                'border-top':'5px solid black',
                'border-bottom': '5px solid black'
                }
            ];

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

            columns= [ 'Seq.', 'Document Type', 'Act', 'Date Filed', 'Issues'];

            columnsColor= [ 
                {Name:'Seq.',         Color:'#0000FF', bodyColor:'#000001',bodyBgColor:'',        ArrowDir:0}, 
                {Name:'Document Type',Color:'#0000FF', bodyColor:'#04818F',bodyBgColor:'',        ArrowDir:2}, 
                {Name:'Act',          Color:'#000000', bodyColor:'#FFFFFF',bodyBgColor:'#6B706C', ArrowDir:-1}, 
                {Name:'Date Filed',   Color:'#FF0000', bodyColor:'#000001',bodyBgColor:'',        ArrowDir:2}, 
                {Name:'Issues',       Color:'#000000', bodyColor:'#000001',bodyBgColor:'',        ArrowDir:-1}
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


        public sortCol(index: number): void {
            console.log( index);

            
            
            if(this.columnsColor[index].ArrowDir==0)
            {
                this.columnsColor[index].ArrowDir=1;
                this.currentSortIndex = index;
            }
            else if(this.columnsColor[index].ArrowDir==1)
            {
                this.columnsColor[index].ArrowDir=0;
                this.currentSortIndex = index;
            }
            else if(this.columnsColor[index].ArrowDir==2)
            {
                this.columnsColor[index].ArrowDir=0;
                this.currentSortIndex = index;
            }
            else
            {
                return;
            }

            for(const inx in this.columnsColor)
            {
               
                if(this.columnsColor[inx].ArrowDir >=0 )
                {
                    if(inx==index.toString())continue;
                    this.columnsColor[inx].ArrowDir = 2;
                }
            }


        }
        
        get sortedFilteredItems() {
            return this.items
            .sort((a, b) => {
                let dirSign = 1;
                const sortDirection = this.columnsColor[this.currentSortIndex].ArrowDir;
                const currentSortName = this.columnsColor[this.currentSortIndex].Name;
                if(sortDirection == 0) {dirSign = -1;}
                if(a[currentSortName] < b[currentSortName]) return (-1*dirSign );
                else if(a[currentSortName] > b[currentSortName]) return (1*dirSign);
                else return 0;
            })
            .filter(value => {
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

        //  get sortedItems(): any {
        //     return this.items.sort((a: any, b: any) => {
        //         let dirSign = 1;
        //         if(this.sortDirection == 'desc') {dirSign = -1;}
        //         if(a[this.currentSortName] < b[this.currentSortName]) return (-1*dirSign );
        //         else if(a[this.currentSortName] > b[this.currentSortName]) return (1*dirSign);
        //         else return 0;
        //     });
        // }
    
}
</script>






<style>
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
    .arrow-up {
        width: 0; 
        height: 0; 
        border-left: 5px solid transparent;
        border-right: 5px solid transparent;
        display:inline-block;
        vertical-align:top;
        border-top:0;
        border-bottom: 5px solid black;
    }
    .arrow-down {
        width: 0; 
        height: 0; 
        border-left: 5px solid transparent;
        border-right: 5px solid transparent;
        display:inline-block;
        vertical-align:top;
        border-top: 5px solid black;
    }

    .arrow-null {
        width: 0; 
        height: 0; 
        border-left: 0 solid transparent;
        border-right: 0 solid transparent;
        display:inline-block;
        vertical-align:top;
        border-bottom: 0 solid black;
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
