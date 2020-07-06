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
                        <b-button @click="navigateToDocumentsView(fileInformation)">Search</b-button>
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
    import {criminalFileInformationType} from '../types/criminal';
    import {civilFileInformationType} from '../types/civil';
    import {fileSearchType} from '../types/common';
    const civilState = namespace('CivilFileInformation');
    const criminalState = namespace('CriminalFileInformation');
    const commonState = namespace('CommonInformation');

    @Component
    export default class Home extends Vue {

        selected = 'criminal';
        options= [
          { value: 'civil', text: 'Civil' },
          { value: 'criminal', text: 'Criminal' }
        ]
        
        fileInformation = { }

        fileSearch = {}

        @civilState.Action
        public UpdateCivilFile!: (newCivilFileInformation: civilFileInformationType) => void

        @criminalState.Action
        public UpdateCriminalFile!: (newCriminalFileInformation: criminalFileInformationType) => void
        
        @commonState.Action
        public UpdateFileSearch!: (newFileSearch: fileSearchType) => void        
        
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
            this.UpdateFileSearch(fileSearch)
            this.$router.push({name:'CriminalCaseDetails', params: {fileNumber: fileSearch.fileNumber, location: fileSearch.location}})
                   
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
                            this.searchByRouterParams();                        
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
                    roomAndLocationInfo["text"]= jroomAndLocation.name + ' (' +jroomAndLocation.locationId+')';             
                            
                    const rooms: roomsInfoType[] = [];         
                    for(const jroom of jroomAndLocation.courtRooms)
                    {              
                        const roomInfo = {} as roomsInfoType;                   
                        roomInfo["value"]= jroom.room 
                        roomInfo["text"]= jroom.room                        
                        rooms.push(roomInfo);
                    }               
                    roomAndLocationInfo.value = {} as locationInfoType;
                    roomAndLocationInfo.value["Location"] = jroomAndLocation.name;
                    roomAndLocationInfo.value["LocationID"] = jroomAndLocation.locationId;
                    roomAndLocationInfo.value["Rooms"] = rooms
                
                    this.courtRoomsAndLocations.push(roomAndLocationInfo);
                }                
            }
            this.courtRoomsAndLocations =  _.sortBy(this.courtRoomsAndLocations, 'text')
            this.selectedCourtLocation = this.courtRoomsAndLocations[0].value; 
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
