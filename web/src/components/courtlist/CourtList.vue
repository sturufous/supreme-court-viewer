<template>
<div>
    <b-card bg-variant="light" v-if= "!isLocationDataMounted && !isLocationDataReady">
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

    <b-card bg-variant="light" v-else-if= "isLocationDataMounted && !isLocationDataReady">
        <b-card >
            <span >No Court Location Found. </span>            
        </b-card>       
    </b-card>

    <b-card v-else >
        <b-navbar type="white" variant="white" style="height:45px">
            <h2 class="ml-1 mt-2"> Court List </h2>
            <b-navbar-nav class="ml-auto">
                <b-button 
                    size="sm"
                    :disabled="!searchAllowed"
                    @click="BackToPreviouDay" 
                    variant="primary" 
                    class="my-2 my-sm-0">
                        <b-icon icon="chevron-left"></b-icon>
                        Back to Previous Day
                </b-button>
                <b-button 
                    size="sm"
                    :disabled="!searchAllowed"
                    @click="JumpToNextDay"   
                    variant="primary" 
                    class="ml-2 my-2 my-sm-0">
                        Jump to Next Day
                        <b-icon icon="arrow-right-short"></b-icon>
                </b-button>
            </b-navbar-nav>    
        </b-navbar>

        <b-row class = "mt-2 ml-2">
            <b-col md="4">          
                <b-form-group>
                    <label for="locationSelect">Location*</label>
                    <b-form-select
                        v-model="selectedCourtLocation"
                        id="locationSelect"
                        :disabled="!searchAllowed"
                        :state = "selectedCourtLocationState?null:false"
                        @change="LocationChanged"
                        :options="courtRoomsAndLocations"                        
                        style="height:39px">
                    </b-form-select>
                </b-form-group>
            </b-col>
            <b-col md="3">
                <label for="datepicker">Date* (YYYY-MM-DD)</label>
               
                <b-input-group class="mb-3">
                    <b-form-input
                        id="datepicker"
                        v-model="selectedDate"
                        type="text"
                        :disabled="!searchAllowed"
                        placeholder="YYYY-MM-DD"
                        autocomplete="off"
                        :state = "selectedDateState?null:false"
                    ></b-form-input>
                    <b-input-group-append>
                        <b-form-datepicker
                        v-model="selectedDate"
                        button-only
                        :disabled="!searchAllowed"
                        right
                        locale="en-US"                        
                        @context="onCalenderContext"
                        ></b-form-datepicker>
                    </b-input-group-append>
                </b-input-group>           
            </b-col>
            <b-col md="2">
                <b-form-group class = "mr-3"> 
                    <label for="roomSelect">Room*</label>
                    <b-form-select
                        v-if="syncFlag"
                        v-model="selectedCourtRoom"
                        id="roomSelect"
                        :disabled="!searchAllowed"
                        @change="RoomChanged"
                        :options="selectedCourtLocation.Rooms"
                        :state = "selectedCourtRoomState?null:false"                       
                        style="height:39px">
                    </b-form-select>
                </b-form-group>
            </b-col>
        </b-row>

        <b-row class = "ml-2 mt-2">
            <b-col md="4"> 
                <b-button 
                    @click="searchForCourtList"
                    :disabled="!searchAllowed"
                    variant="primary" 
                    class="mb-2">
                        <b-icon icon="search"></b-icon>
                        Search
                </b-button>
            </b-col>
        </b-row>

        <b-card bg-variant="light" v-if= "searchingRequest">
            <b-card class="mb-2">
                <b-navbar type="white" variant="white" style="height:40px;" >
                    <b-nav-text class="text-primary mr-2 mt-2">               
                        <h2>{{fullSelectedDate}}</h2>                
                    </b-nav-text>

                    <b-nav-text class="text-muted ml-5 mt-2">               
                        <h3>{{courtListLocation}}</h3>               
                    </b-nav-text>
                    <b-nav-text class="text-muted ml-1" style="padding-top:18px">               
                        <h4>({{courtListLocationID}})</h4>               
                    </b-nav-text>

                    <b-nav-text class=" ml-5 mt-2">               
                        <h3> CourtRoom: </h3>                
                    </b-nav-text>            
                    <b-nav-text class=" ml-1 mt-2 ">               
                        <h3>{{courtListRoom}}</h3>                
                    </b-nav-text>
                </b-navbar>
            </b-card>      

            <b-card bg-variant="light" v-if= "!isMounted && !isDataReady">
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

            <b-card class="mt-1" v-if= "isMounted && !isDataReady">
                <span class="ml-3" >No appearances. </span> 
            </b-card>
    
            <b-card no-body v-if="isDataReady">
                <b-row cols="1" class = "mx-2 mt-2 mb-5">
                    <criminal-list/>
                    <civil-list civilClass='family'/>
                    <civil-list civilClass='civil'/>
                    
                </b-row> 
            </b-card>
        </b-card> 

    </b-card>
</div>
</template>



<script lang="ts">
import { Component, Vue } from 'vue-property-decorator';
import { namespace } from 'vuex-class';
import * as _ from 'underscore';

import CriminalList from "@components/courtlist/CriminalList.vue";
import CivilList from "@components/courtlist/CivilList.vue";

import '@store/modules/CourtListInformation';
const courtListState = namespace('CourtListInformation');

@Component({
    components: {
        CriminalList,
        CivilList
    }
})
export default class CourtList extends Vue {

    @courtListState.State
    public courtListInformation!: any

    @courtListState.Action
    public UpdateCourtList!: (newCourtListInformation: any) => void 

    mounted () {                 
        this.getListOfAvailableCourts();
    }

    public getListOfAvailableCourts(): void 
    {
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

    public getCourtListDetails(): void {
        
        this.isDataReady = false;
        this.isMounted = false;
        this.searchingRequest = true;
        //console.log('before call')
        //console.log(this.searchingRequest)
       
        this.$http.get('/api/courtlist/court-list?agencyId='+ this.courtListLocationID +'&roomCode='+ this.courtListRoom+'&proceeding=' +this.validSelectedDate)
            .then(Response => Response.json(), err => {this.errorCode= err.status;this.errorText= err.statusText;console.log(err);}        
            ).then(data => {
                if(data){

                   // console.log(data)
                    this.courtListInformation.detailsData = data; 
                    // this.participantJson = data.participant                
                    this.UpdateCourtList(this.courtListInformation);               
                    // this.ExtractFileInfo()
                    if((data.civilCourtList.length>0) || (data.criminalCourtList.length>0))
                    {                    
                        this.isDataReady = true;
                    }
                }
                this.isMounted = true;
                this.searchAllowed = true;
            });
        
    }

    errorCode=''
    errorText=''
    isDataReady = false
    isMounted = false
    searchingRequest = false
    isLocationDataReady = false;
    isLocationDataMounted = false;
    searchAllowed = true;
    syncFlag = true


    courtRoomsAndLocationsJson;
    courtRoomsAndLocations: any[] = [];

    selectedDate = (new Date).toISOString().substring(0,10);
    validSelectedDate = this.selectedDate;
    fullSelectedDate = '';

    selectedDateState = true
    selectedCourtRoom= 'null';
    selectedCourtRoomState=true;
    selectedCourtLocation: any;
    selectedCourtLocationState=true;
    
    courtListLocation = "Vancouver"
    courtListLocationID = '4801'
    courtListRoom = "005"
    

    public ExtractCourtRoomsAndLocationsInfo()
    {
        for(const jroomAndLocation of this.courtRoomsAndLocationsJson)
        {            
            if(jroomAndLocation.courtRooms.length>0)
            {
                const locationInfo = {};
                locationInfo["text"]= jroomAndLocation.name + ' (' +jroomAndLocation.locationId+')';             
                        
                const rooms: any[] = [];         
                for(const jroom of jroomAndLocation.courtRooms)
                {              
                    const roomInfo = {};                   
                    roomInfo["value"]= jroom.room 
                    roomInfo["text"]= jroom.room                        
                    rooms.push(roomInfo);
                }

                locationInfo["value"] ={
                    "Location": jroomAndLocation.name,
                    "LocationID": jroomAndLocation.locationId,
                    "Rooms" : rooms
                };

               // console.log(locationInfo)
                this.courtRoomsAndLocations.push(locationInfo);
            }                
        }
        this.courtRoomsAndLocations =  _.sortBy(this.courtRoomsAndLocations, 'text')
        this.selectedCourtLocation = this.courtRoomsAndLocations[0].value;       
        //console.log(this.courtRoomsAndLocations)
    }

    public onCalenderContext(datePicked) {
        
        //console.log(datePicked.selectedFormatted)        
        //console.log(datePicked.selectedYMD)
        this.searchingRequest = false
        
        if(datePicked.selectedYMD)
        {
            this.validSelectedDate = datePicked.selectedYMD
            this.fullSelectedDate = datePicked.selectedFormatted
            //console.log('Date OK')
        } 
    }

    public BackToPreviouDay()
    {    
        if(!this.checkDateInValid())
        {
            this.searchAllowed = false;   
            const date=new Date(this.selectedDate)
            date.setDate(date.getDate() - 1)
            this.selectedDate = date.toISOString().substring(0,10)
            //console.log( this.selectedDate)
            // console.log('pre day')
            //console.log(this.searchingRequest)
            setTimeout(() => { this.searchForCourtList(); }, 500);
        }
    }

    public JumpToNextDay()
    {
        if(!this.checkDateInValid())
        {
            this.searchAllowed = false;   
            const date=new Date(this.selectedDate)
            date.setDate(date.getDate() + 1)
            this.selectedDate = date.toISOString().substring(0,10)
            //console.log( this.selectedDate)
            //console.log('next day')
            //console.log(this.searchingRequest)
            setTimeout(() => { this.searchForCourtList(); }, 500);
        }
    }

    
    public checkDateInValid()
    {
        if(this.isValidDate(this.selectedDate))
        {
           // console.log('date ok')
           // console.log(this.validSelectedDate)
            this.selectedDateState = true;
            return false
        }
        else
        {
          // console.log('date error')
           this.selectedDateState = false;
           return true
        }         
    }

    public isValidDate(dateString) 
    { 
        if(!/^\d{4}-\d{1,2}-\d{1,2}$/.test(dateString))
            return false;

        const parts = dateString.split("-");
        const day = parseInt(parts[2], 10);
        const month = parseInt(parts[1], 10);
        const year = parseInt(parts[0], 10);
        
        if(year < 1800 || year > 3000 || month == 0 || month > 12)
            return false;

        const monthLength = [ 31, 28, 31, 30, 31, 30, 31, 31, 30, 31, 30, 31 ];

        // Adjust for leap years
        if(year % 400 == 0 || (year % 100 != 0 && year % 4 == 0))
            monthLength[1] = 29;

        return day > 0 && day <= monthLength[month - 1];
    } 
        

    public searchForCourtList()
    {        
        if(this.selectedCourtLocation ==null)
        {
            //console.log(this.selectedCourtLocation)
            this.selectedCourtLocationState=false;
            this.searchAllowed = true;
        }
        else
        {
            
            if(this.checkDateInValid())
            {                
                this.searchAllowed = true;
            }
            else
            {
                //console.log('selectedCourtLocation')                
                this.courtListLocation = this.selectedCourtLocation.Location;
                this.courtListLocationID = this.selectedCourtLocation.LocationID;

                if(this.selectedCourtRoom =='null' || this.selectedCourtRoom== undefined)
                {
                // console.log(this.selectedCourtRoom)
                    this.selectedCourtRoomState = false;
                    this.searchAllowed = true;
                }
                else
                {
                    //console.log('this.selectedCourtRoom')
                    //console.log(this.selectedCourtRoom) 
                    this.courtListRoom = this.selectedCourtRoom;
                    // console.log('search')
                    //console.log(this.searchingRequest)
                    this.searchAllowed = false;   
                    this.getCourtListDetails();
                }
            }
        }
    }

    public LocationChanged()
    {   
        this.searchingRequest = false;    
        this.selectedCourtRoom = 'null'
        this.selectedCourtLocationState=true;        
        this.syncFlag = false; 
        this.syncFlag = true;     
    }

    public RoomChanged()
    {   
        this.searchingRequest = false;     
        this.selectedCourtRoomState=true;        
    }

}

</script>

<style scoped>
 .card {
        border: white;
    }
</style>