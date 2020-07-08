<template>

    <b-card bg-variant="white" no-body>        
        
        <div>            
            <h2 class="mx-4 mt-5 font-weight-normal text-civil ">Civil</h2>
            <hr class="mx-3 bg-civil" style="height: 5px;"/> 
        </div>

        <b-card v-if="isMounted &&!isDataReady" no-body>
            <span class="text-muted ml-4 mb-5"> No Civil List has been found. </span>
        </b-card>

        <b-card bg-variant="light" v-if= "!isMounted && !isDataReady" >
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

        <b-card bg-variant="white" v-if="isDataReady" no-body class="mx-3" style="overflow:auto">           
            <b-table
            :items="SortedList"
            :fields="fields"            
            borderless
            small
            responsive="sm"
            >   
                <template v-slot:head()="data">
                    <b> {{ data.label }}</b>
                </template>               
                
                <template  v-slot:[`cell(${fields[0].key})`]="data">
                    <b-button
                        style="font-size:16px; font-weight: bold;" 
                        size="sm" 
                        @click="OpenCivilFilePage(data)"                        
                        variant="outline-primary border-white text-civil" 
                        class="mr-2">                            
                            {{data.value}}
                    </b-button>
                </template>            
                
            </b-table>
        </b-card>      
    </b-card> 

</template>

<script lang="ts">
import { Component, Vue } from "vue-property-decorator";
import { namespace } from "vuex-class";
import * as _ from 'underscore';
import {civilFileInformationType, fileSearchCivilInfoType} from '../../types/civil';
import {inputNamesType, fileSearchType} from '../../types/common';
import "@store/modules/CommonInformation";
const commonState = namespace("CommonInformation");
import "@store/modules/CivilFileInformation";
const criminalState = namespace("CivilFileInformation");

@Component
export default class CriminalList extends Vue {    
    
    @commonState.State
    public fileSearch!: fileSearchType;           

    @criminalState.Action
    public UpdateCivilFile!: (newCivilFileInformation: civilFileInformationType) => void    
    
    @commonState.State
    public displayName!: string; 

    @commonState.Action
    public UpdateDisplayName!: (newInputNames: inputNamesType) => void
    
    civilList: fileSearchCivilInfoType[] = [];    
    isMounted = false;
    isDataReady = false;
    errorCode=0
    errorText=''
    fileInformation = {};
    
    fields =  
    [        
        {key:'File Id',             tdClass: 'border-top', headerStyle:'', cellStyle:''},        
        {key:'Parties',             tdClass: 'border-top', headerStyle:'', cellStyle:'text-primary'}        
    ];

     mounted() {
        this.getList();       
    }

    public getList(): void 
    {            
        this.$http.get('/api/files/civil?location='+ this.fileSearch.location +'&fileNumber='+ this.fileSearch.fileNumber)
            .then(Response => Response.json(), err => {this.errorCode= err.status;this.errorText= err.statusText;console.log(err);}        
            ).then(data => {
                if(data){
                    if (data.length > 1) {

                        for (const civilListIndex in data) {
                            const civilListInfo = {} as fileSearchCivilInfoType;
                            const jcivilList = data[civilListIndex];
                            const partyInfo: string[] = [];
                            const leftRole = jcivilList.leftRoleDsc;
                            const rightRole = jcivilList.rightRoleDsc;
                            for(const jParty of jcivilList.party) {
                                const firstName = jParty.givenNm? jParty.givenNm: '';
                                const lastName =  jParty.lastNm? jParty.lastNm: jParty.orgNm ;
                                this.UpdateDisplayName({'lastName': lastName, 'givenName': firstName});
                                const roleDsc = (jParty.leftRightCd == "R")? rightRole: leftRole;
                                partyInfo.push(this.displayName + " (" + roleDsc + ")");
                            }
                            civilListInfo.Parties = partyInfo;
                            civilListInfo["File Id"] = jcivilList.physicalFileId;
                            this.civilList.push(civilListInfo);
                        }

                        if(this.civilList.length)
                        {                    
                            this.isDataReady = true;
                        }    
                        this.isMounted = true;
                    } else if (data.length == 1) {
                        this.fileInformation["fileNumber"] = data[0].physicalFileId;
                        const civilFileInformation = {} as civilFileInformationType;
                        civilFileInformation.fileNumber = data[0].physicalFileId;
                        this.UpdateCivilFile(civilFileInformation)                        
                        this.$router.push({name:'CivilCaseDetails', params: {fileNumber: civilFileInformation.fileNumber}})
                    }               
                }
            });
    }    

    public OpenCivilFilePage(data)
    {
        const fileInformation = { } as civilFileInformationType
        fileInformation['fileNumber'] = data.value
        this.UpdateCivilFile(fileInformation)
        const routeData = this.$router.resolve({name:'CivilCaseDetails', params: {fileNumber: fileInformation['fileNumber']}})
        window.open(routeData.href, '_blank');
    }

    get SortedList()
    {                
        return  _.sortBy(this.civilList, 'File Id')
    }
}
</script>

<style scoped>
 .card {
        border: white;
    }
</style>