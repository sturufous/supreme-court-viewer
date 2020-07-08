<template>

    <b-card bg-variant="white" no-body>
        <div>
            <h2 class="mx-4 mt-5 font-weight-normal text-criminal ">Criminal</h2>            
            <hr class="mx-3 bg-criminal" style="height: 5px;"/> 
        </div>

        <b-card v-if="isMounted &&!isDataReady" no-body>
            <span class="text-muted ml-4 mb-5"> No Criminal List has been found. </span>
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
                        @click="OpenCriminalFilePage(data)"                        
                        variant="outline-primary border-white text-criminal" 
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
import {criminalFileInformationType, fileSearchCriminalInfoType} from '../../types/criminal';
import {inputNamesType, fileSearchType } from '../../types/common'
import "@store/modules/CommonInformation";
const commonState = namespace("CommonInformation");
import "@store/modules/CriminalFileInformation";
const criminalState = namespace("CriminalFileInformation");

@Component
export default class CriminalList extends Vue {
    
    @commonState.State
    public fileSearch!: fileSearchType;

    @criminalState.Action
    public UpdateCriminalFile!: (newCriminalFileInformation: criminalFileInformationType) => void    
    
    @commonState.State
    public displayName!: string; 

    @commonState.Action
    public UpdateDisplayName!: (newInputNames: inputNamesType) => void    
   
    criminalList: fileSearchCriminalInfoType[] = [];    
    isMounted = false;
    isDataReady = false;
    errorCode=0
    errorText=''
    fileInformation = {};
    
    fields =  
    [        
        {key:'File Id',             tdClass: 'border-top', headerStyle:'', cellStyle:''},        
        {key:'Participants',        tdClass: 'border-top', headerStyle:'', cellStyle:'text-primary'}        
    ];

     mounted() {
        this.getList();       
    }

    public getList(): void 
    {
        this.$http.get('/api/files/criminal?location='+ this.fileSearch.location +'&fileNumber='+ this.fileSearch.fileNumber)
            .then(Response => Response.json(), err => {this.errorCode= err.status;this.errorText= err.statusText;console.log(err);}        
            ).then(data => {
                if(data){
                    if (data.length > 1) {
                        for (const criminalListIndex in data) {
                            const criminalListInfo = {} as fileSearchCriminalInfoType;
                            const jcriminalList = data[criminalListIndex];
                            const participantInfo: string[] = [];                            
                            for(const participant of jcriminalList.participant) {                                                     
                                const firstName = participant.givenNm.trim().length>0 ? participant.givenNm : "";
                                const lastName = participant.lastNm ? participant.lastNm : participant.orgNm;
                                this.UpdateDisplayName({'lastName': lastName, 'givenName': firstName});
                                participantInfo.push(this.displayName);
                            }
                            criminalListInfo.Participants = participantInfo;
                            criminalListInfo["File Id"] = jcriminalList.justinNo;
                            this.criminalList.push(criminalListInfo);
                        }
                               
                        if(this.criminalList.length)
                        {                    
                            this.isDataReady = true;
                        }    
                        this.isMounted = true;
                    } else if (data.length == 1) {
                        this.fileInformation["fileNumber"] = data[0].physicalFileId;
                        const criminalFileInformation = {} as criminalFileInformationType;
                        criminalFileInformation.fileNumber = data[0].physicalFileId;
                        this.UpdateCriminalFile(criminalFileInformation)                     
                        this.$router.push({name:'CriminalCaseDetails', params: {fileNumber: criminalFileInformation.fileNumber}})
                    }               
                }
            });    
        
    }
    
    public OpenCriminalFilePage(data)
    {
        const fileInformation = { } as criminalFileInformationType
        fileInformation.fileNumber = data.value
        this.UpdateCriminalFile(fileInformation);        
        const routeData = this.$router.resolve({name:'CriminalCaseDetails', params: {fileNumber: fileInformation.fileNumber}})
        window.open(routeData.href, '_blank');
    }

    get SortedList()
    {                
        return  _.sortBy(this.criminalList, 'File Id')
    }
}
</script>

<style scoped>
 .card {
        border: white;
    }
</style>