<template>
<body>
    <b-card>
        <b-navbar type="white" variant="white" style="height:45px">
            <h2 class="ml-1 mt-2"> Court List </h2>
            <b-navbar-nav class="ml-auto">
                <b-button 
                    size="sm"
                    @click="BackToPreviouDay" 
                    variant="primary" 
                    class="my-2 my-sm-0">
                        <b-icon icon="chevron-left"></b-icon>
                        Back to Previous Day
                </b-button>
                <b-button 
                    size="sm"
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
                        :options="['Vancouver', 'Kelowna', 'NorthShore']"
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
                        no-caret
                        placeholder="YYYY-MM-DD"
                        autocomplete="off"
                    ></b-form-input>
                    <b-input-group-append>
                        <b-form-datepicker
                        v-model="selectedDate"
                        button-only
                        right
                        locale="en-US"
                        
                        @context="onContext"
                        ></b-form-datepicker>
                    </b-input-group-append>
                </b-input-group>           
            </b-col>
            <b-col md="2">
                <b-form-group            
                    class = "mr-3"> 
                    <label for="roomSelect">Room*</label>
                    <b-form-select
                        v-model="selectedCourtRoom"
                        id="roomSelect"
                        :options="['101', 'JCM', '80', '005']"
                        style="height:39px">
                    </b-form-select>
                </b-form-group>
            </b-col>
        </b-row>

        <b-row class = "ml-2 mt-2">
            <b-col md="4"> 
                <b-button 
                    @click="searchForCourtList"
                    variant="primary" 
                    class="mb-2">
                        <b-icon icon="search"></b-icon>
                        Search
                </b-button>
            </b-col>
        </b-row>

    <b-card bg-variant="light" v-if= "searchingRequest">
        
        <b-navbar type="white" variant="white" >
            <b-nav-text class="text-primary mr-2">               
                <h1>{{fullSelectedDate}}</h1>                
            </b-nav-text>

            <b-nav-text class="text-muted ml-5">               
                <h3>{{selectedCourtLocation}}</h3>                
            </b-nav-text>

            <b-nav-text class=" ml-5">               
                <h2> CourtRoom: </h2>                
            </b-nav-text>            
            <b-nav-text class=" ml-1 ">               
                <h3>{{selectedCourtRoom}}</h3>                
            </b-nav-text>
        </b-navbar>     
   

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

    <b-card bg-variant="light" v-if= "isMounted && !isDataReady">
        <b-card >
            <span >No appearances. </span>            
        </b-card>       
    </b-card>
 
    <b-card no-body v-if="isDataReady">
        <b-row cols="1" class = "mx-2 mt-2 mb-5">
            <criminal-list/>
            <civil-list/>
        </b-row> 
    </b-card>
  </b-card>  
</b-card>
</body>
</template>



<script lang="ts">
import { Component, Vue } from 'vue-property-decorator';
import { namespace } from 'vuex-class';
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

    // mounted () {                 
    //     this.getCourtListDetails();
    // }

    public onContext(ctx) {
        // The date formatted in the locale, or the `label-no-date-selected` string
        //console.log(ctx.selectedFormatted)
        // The following will be an empty string until a valid date is entered
        //console.log(ctx.selectedYMD)
        this.searchingRequest = false
        const tempDate = new Date(this.selectedDate)
        //console.log(tempDate)
        if(!isNaN(tempDate.getTime()))
        {
            console.log('date ok')
            this.selectedDate = ctx.selectedYMD
            this.lastValidSelectedDate = ctx.selectedYMD 
            this.fullSelectedDate = ctx.selectedFormatted
        }
        else
        {
            console.log('date error')
            this.selectedDate = this.lastValidSelectedDate
        }        
    }

    

    public getCourtListDetails(): void {
        
        this.isDataReady = false;
        this.isMounted = false;
       
        this.$http.get('/api/courtlist/court-list?agencyId='+ this.courtListLocationID +'&roomCode='+ this.courtListRoom+'&proceeding=' +this.selectedDate)
            .then(Response => Response.json(), err => {this.errorCode= err.status;this.errorText= err.statusText;console.log(err);}        
            ).then(data => {
                if(data){

                    console.log(data)
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
                       
            });
    }

    errorCode=''
    errorText=''
    isDataReady = false
    isMounted = false
    searchingRequest = false

    selectedDate = (new Date).toISOString().substring(0,10);
    lastValidSelectedDate = this.selectedDate;
    fullSelectedDate = '';

    selectedCourtRoom = "005"
    selectedCourtLocation = "Vancouver"
    
    location = "Vancouver"
    courtListLocationID = '4801'
    courtListRoom = "005"
    

    public BackToPreviouDay()
    {       
        const date=new Date(this.selectedDate)
        date.setDate(date.getDate() - 1)
        this.selectedDate = date.toISOString().substring(0,10)
        console.log( this.selectedDate)
        //this.getCourtListDetails();
    }

    public JumpToNextDay()
    {
        const date=new Date(this.selectedDate)
        date.setDate(date.getDate() + 1)
        this.selectedDate = date.toISOString().substring(0,10)
        console.log( this.selectedDate)
        //this.getCourtListDetails();
    }

    public searchForCourtList()
    {
        this.searchingRequest = true
        this.getCourtListDetails();
    }

}

</script>

<style scoped>
 .card {
        border: white;
    }
</style>