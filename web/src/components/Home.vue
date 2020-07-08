<template>
    <b-card bg-variant="white" class="home">
        <b-card >
            <b-card-title class="h2"> Temporary landing page for SCV </b-card-title>
        </b-card>
        <b-row cols="2" >
            <b-col md="6" cols="6"> 
                <b-card  align="left" >
                    <b-card >
                        <b-card-title class="h3"> Search by File ID: </b-card-title>
                    </b-card>
                    <b-card >
                        <b-card-text for="fileId"> File ID: </b-card-text>                
                        <b-form-input id="fileId" v-model="fileInformation.fileNumber" placeholder="Enter File ID"></b-form-input>                
                    </b-card>
                    <b-card> 
                        <b-card-text for="civil/criminal"> Civil/Criminal: </b-card-text>
                        <b-form-select v-model="selected" :options="options"></b-form-select>               
                    </b-card>                
                    <b-card >
                        <b-button @click="navigateToDocumentsView(fileInformation)">Search</b-button>
                    </b-card>                    
                </b-card>
            </b-col>
            <b-col md="6" cols="6"> 
                <b-card  align="left" >
                    <b-card >
                        <b-card-title class="h3"> Search by File Number and Location: </b-card-title>
                    </b-card>
                    <b-card >
                        <b-card-text for="filenumber"> File Number: </b-card-text>                
                        <b-form-input id="filenumber" v-model="fileSearch.fileNumber" placeholder="Enter File Number"></b-form-input>                
                    </b-card>
                    <b-card> 
                        <b-card-text for="civil/criminal"> Civil/Criminal: </b-card-text>
                        <b-form-select v-model="selectedFileSearch" :options="options"></b-form-select>               
                    </b-card>
                    <b-card> 
                        <b-card-text for="location"> Location: </b-card-text>
                        <b-form-select
                        v-model="selectedCourtLocation"
                        id="locationSelect"
                        :disabled="!searchAllowed"
                        :state = "selectedCourtLocationState?null:false"
                        @change="LocationChanged"
                        :options="courtRoomsAndLocations"                        
                        style="height:39px">
                        </b-form-select>               
                    </b-card>                
                    <b-card >
                        <b-button @click="navigateToFilesView(fileSearch)">Search</b-button>
                    </b-card>                    
                </b-card>
            </b-col>
        </b-row>
        <b-card >
            <b-button @click="navigateToCourtList()">Court List</b-button>
        </b-card>    
    </b-card>
</template>

<script lang="ts">
    import { Component, Vue } from 'vue-property-decorator';
    import { namespace } from 'vuex-class'
    import * as _ from 'underscore';
    import '@store/modules/CivilFileInformation';
    import '@store/modules/CriminalFileInformation';
    import {criminalFileInformationType, fileSearchCriminalInfoType} from '../types/criminal';
    import {civilFileInformationType, fileSearchCivilInfoType} from '../types/civil';
    import {courtRoomsAndLocationsInfoType, locationInfoType} from '../types/courtlist';
    import {fileSearchType, inputNamesType} from '../types/common';
    const civilState = namespace('CivilFileInformation');
    const criminalState = namespace('CriminalFileInformation');
    const commonState = namespace('CommonInformation');

    @Component
    export default class Home extends Vue {
        
        @commonState.State
        public displayName!: string;

        @commonState.Action
        public UpdateDisplayName!: (newInputNames: inputNamesType) => void

        @civilState.Action
        public UpdateCivilFile!: (newCivilFileInformation: civilFileInformationType) => void

        @criminalState.Action
        public UpdateCriminalFile!: (newCriminalFileInformation: criminalFileInformationType) => void
        
        @commonState.Action
        public UpdateFileSearch!: (newFileSearch: fileSearchType) => void
        
        selected = 'criminal';
        selectedFileSearch = 'criminal';
        options= [
          { value: 'civil', text: 'Civil' },
          { value: 'criminal', text: 'Criminal' }
        ]
        
        fileInformation = {};
        fileSearch = {} as fileSearchType;        
        errorCode=0
        errorText=''
        syncFlag = true
        searchingRequest = false;
        searchAllowed = true;
        isLocationDataMounted = false;
        isLocationDataReady = false;
        courtRoomsAndLocationsJson;
        courtRoomsAndLocations: courtRoomsAndLocationsInfoType[] = []
        fileSearchCivilInfo: fileSearchCivilInfoType[] = []
        fileSearchCriminalInfo: fileSearchCriminalInfoType[] = []
        selectedCourtLocation = {} as locationInfoType;
        selectedCourtLocationState=true;
        
        // TODO: add validation so that the user has to enter values before clicking the search button
        navigateToDocumentsView(fileInformation): void {
            if(this.selected == 'civil') {
                this.UpdateCivilFile(fileInformation)
                this.$router.push({name:'CivilCaseDetails', params: {fileNumber: fileInformation.fileNumber}})
            } else if(this.selected == 'criminal') {
                this.UpdateCriminalFile(fileInformation)
                this.$router.push({name:'CriminalCaseDetails', params: {fileNumber: fileInformation.fileNumber}})
            }            
        }

        navigateToFilesView(fileSearch): void {
            if(this.selectedFileSearch == 'civil') {
                this.fileSearch.location = this.selectedCourtLocation.LocationID;
                this.UpdateFileSearch(fileSearch)
                this.$router.push({name:'CivilFileSearchResultList', params: {fileNumber: this.fileSearch.fileNumber, location: this.fileSearch.location}})
            } else if(this.selectedFileSearch == 'criminal') {
                this.fileSearch.location = this.selectedCourtLocation.LocationID;
                this.UpdateFileSearch(fileSearch)
                this.$router.push({name:'CriminalFileSearchResultList', params: {fileNumber: this.fileSearch.fileNumber, location: this.fileSearch.location}})
            }                    
        }

        mounted () { 
            this.getListOfAvailableCourts();
        }

        public getListOfAvailableCourts(): void 
        {
            this.errorCode = 0;
            this.$http.get('/api/location/court-rooms')
                .then(Response => Response.json(), err => {this.errorCode= err.status;this.errorText= err.statusText;console.log(err);}        
                ).then(data => {
                    if(data){
                        this.courtRoomsAndLocationsJson = data
                        this.ExtractCourtRoomsAndLocationsInfo();
                        if(this.courtRoomsAndLocations.length>0)
                        {                    
                            this.isLocationDataReady = true;                       
                        }
                    }
                    this.isLocationDataMounted = true;
                });
        }
        
        public ExtractCourtRoomsAndLocationsInfo()
        {
            for(const jroomAndLocation of this.courtRoomsAndLocationsJson)
            {            
                if(jroomAndLocation.courtRooms.length>0)
                {
                    const roomAndLocationInfo = {} as courtRoomsAndLocationsInfoType;
                    roomAndLocationInfo["text"]= jroomAndLocation.name + ' (' +jroomAndLocation.code+')';                    
                    roomAndLocationInfo.value = {} as locationInfoType;
                    roomAndLocationInfo.value["Location"] = jroomAndLocation.name;
                    roomAndLocationInfo.value["LocationID"] = jroomAndLocation.code;                
                    this.courtRoomsAndLocations.push(roomAndLocationInfo);
                }                
            }
            this.courtRoomsAndLocations =  _.sortBy(this.courtRoomsAndLocations, 'text')
            this.selectedCourtLocation = this.courtRoomsAndLocations[0].value; 
        }

        public LocationChanged()
        {   
            this.searchingRequest = false;
            this.selectedCourtLocationState=true;        
            this.syncFlag = false; 
            this.syncFlag = true;     
        }
        
        navigateToCourtList(): void {
            this.$router.push({name:'CourtList'})
        }
    }
</script>

<style scoped>
    button {
        background-color: #036
    }

    input, select {
        width: 300px;
    }

    .card {
        border: white;
    }

</style>
