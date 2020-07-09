<template>
    <b-card bg-variant="white" no-body>
        <div>            
            <h2 class="mx-4 mt-5 font-weight-normal text-civil ">Civil</h2>
            <hr class="mx-3 bg-civil" style="height: 5px;"/> 
        </div>

        <b-card bg-variant="light" v-if="isMounted &&!isDataReady">
            <b-card  style="min-height: 100px;">
                <span v-if="errorCode==404">This <b>File-Number '{{this.$route.query.fileNumber}}'</b> at <b> location '{{this.$route.query.location}}' </b> doesn't exist in the <b>civil</b> records. </span>
                <span v-else-if="errorCode==200 || errorCode==204"> Bad Data in search results! </span>
                <span v-else> Server is not responding. <b>({{errorText}})</b> </span>
            </b-card>
            <b-card>         
                <b-button id="backToLandingPage" variant="info" @click="navigateToLandingPage">Back to the Landing Page</b-button>
            </b-card>
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
            striped
            responsive="sm"
            >   
                 <template v-for="(field,index) in fields" v-slot:[`head(${field.key})`]="data">
                    <h3 v-bind:key="index" > {{ data.label }}</h3>
                </template> 
                <template  v-slot:[`cell(${fields[0].key})`]="data">
                    <b-button
                        style="font-size:16px; font-weight: bold; border: none;" 
                        size="sm" 
                        @click="OpenCivilFilePage(data.value)"                        
                        variant="outline-primary border-white text-civil" 
                        class="mr-2">                            
                            {{data.value}}
                    </b-button>
                </template>            
                <template  v-slot:cell(Parties)="data">
                    <span v-for="(party, index) in data.value" v-bind:key="index" style= "white-space: pre-line">
                        {{ party }} <br>
                    </span>
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
import {inputNamesType} from '../../types/common';
import "@store/modules/CommonInformation";
const commonState = namespace("CommonInformation");
import "@store/modules/CivilFileInformation";
const civilState = namespace("CivilFileInformation");

@Component
export default class CivilFileSearchResultsView extends Vue {    
    
    @civilState.State
    public civilFileInformation!: civilFileInformationType

    @civilState.Action
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
        {key:'File Id',             tdClass: 'border-top'},        
        {key:'Parties',             tdClass: 'border-top'}        
    ];

    mounted() {      
        this.getList();       
    }

    public getList(): void {
        this.errorCode=0;            
        this.$http.get('/api/files/civil?location='+ this.$route.query.location +'&fileNumber='+ this.$route.query.fileNumber)
            .then(Response => Response.json(), err => {
                this.errorCode= err.status;
                this.errorText= err.statusText;
                console.log(err);
                this.isMounted = true;
                }        
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
                        this.civilFileInformation.fileNumber = data[0].physicalFileId;
                        this.UpdateCivilFile(this.civilFileInformation)                        
                        this.$router.push({name:'CivilCaseDetails', params: {fileNumber: this.civilFileInformation.fileNumber}})
                    } else {
                        this.errorCode=200;
                    }               
                } else {
                    if(this.errorCode==0) this.errorCode=200;
                }
            });
    }    

    public OpenCivilFilePage(fileNumber) {
        this.civilFileInformation.fileNumber = fileNumber;
        this.UpdateCivilFile(this.civilFileInformation)
        const routeData = this.$router.resolve({name:'CivilCaseDetails', params: {fileNumber: this.civilFileInformation.fileNumber}})
        window.open(routeData.href, '_blank');
    }

    get SortedList() {                
        return  _.sortBy(this.civilList, 'File Id')
    }

    public navigateToLandingPage() {
        this.$router.push({name:'Home'})
    }
}

</script>

<style scoped>
 .card {
        border: white;
    }
</style>