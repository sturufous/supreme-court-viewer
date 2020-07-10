<template>
    <b-card bg-variant="white" no-body>
        <div>
            <h2 class="mx-4 mt-5 font-weight-normal text-criminal ">Criminal</h2>            
            <hr class="mx-3 bg-criminal" style="height: 5px;"/> 
        </div>

        <b-card bg-variant="light" v-if="isMounted &&!isDataReady">
            <b-card  style="min-height: 100px;">
                <span v-if="errorCode==404">This <b>File-Number '{{this.$route.query.fileNumber}}'</b> at <b> location '{{this.$route.query.location}}' </b> doesn't exist in the <b>criminal</b> records. </span>
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
                        @click="OpenCriminalFilePage(data.value)"                        
                        variant="outline-primary text-criminal" 
                        class="mr-2">                            
                            {{data.value}}
                    </b-button>
                </template>
                <template  v-slot:cell(Participants)="data">
                    <span v-for="(participant, index) in data.value" v-bind:key="index" style= "white-space: pre-line">
                        {{ participant }} <br>
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
import {criminalFileInformationType, fileSearchCriminalInfoType} from '../../types/criminal';
import {inputNamesType} from '../../types/common';
import "@store/modules/CommonInformation";
const commonState = namespace("CommonInformation");
import "@store/modules/CriminalFileInformation";
const criminalState = namespace("CriminalFileInformation");

enum CourtLevel {'P'= 'Provincial','S' = 'Supreme'}

@Component
export default class CriminalFileSearchResultsView extends Vue {

    @criminalState.State
    public criminalFileInformation!: criminalFileInformationType

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
        {key:'File Id',             tdClass: 'border-top'},        
        {key:'Participants',        tdClass: 'border-top'},
        {key:'Level',               tdClass: 'border-top'}        
    ];

     mounted() {
        this.getList();       
    }

    public getList(): void {
        this.$http.get('/api/files/criminal?location='+ this.$route.query.location +'&fileNumber='+ this.$route.query.fileNumber)
            .then(Response => Response.json(), err => {
                this.errorCode= err.status;
                this.errorText= err.statusText;
                console.log(err);
                this.isMounted = true;
                }        
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
                            criminalListInfo["Level"] = CourtLevel[jcriminalList.courtLevelCd];                            
                            this.criminalList.push(criminalListInfo);
                        }                               
                        if(this.criminalList.length)
                        {                    
                            this.isDataReady = true;
                        }    
                        this.isMounted = true;
                    } else if (data.length == 1) {
                        this.criminalFileInformation.fileNumber = data[0].physicalFileId;
                        this.UpdateCriminalFile(this.criminalFileInformation)                     
                        this.$router.push({name:'CriminalCaseDetails', params: {fileNumber: this.criminalFileInformation.fileNumber}})
                    }               
                }
            });
    }
    
    public OpenCriminalFilePage(fileNumber) {
        this.criminalFileInformation.fileNumber = fileNumber;
        this.UpdateCriminalFile(this.criminalFileInformation);        
        const routeData = this.$router.resolve({name:'CriminalCaseDetails', params: {fileNumber: this.criminalFileInformation.fileNumber}})
        window.open(routeData.href, '_blank');
    }

    get SortedList() {                
        return  _.sortBy(this.criminalList, 'File Id')
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