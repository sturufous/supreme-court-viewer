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
                    v-model="location"
                    id="locationSelect"
                    :options="['Vancouver', 'Kelowna', 'NorthShore']"
                    style="height:39px">
                </b-form-select>
            </b-form-group>
        </b-col>
        <b-col md="3">
            <label for="datepicker">Date*</label>
            <b-form-datepicker
                id="datepicker"
                v-model="selectedDate">
            </b-form-datepicker>            
        </b-col>
        <b-col md="2">
            <b-form-group            
                class = "mr-3"> 
                <label for="roomSelect">Room*</label>
                <b-form-select
                    v-model="courtListRoom"
                    id="roomSelect"
                    :options="['101', 'JCM', '80']"
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
        <b-navbar type="white" variant="white" style="height:45px">
            <h2 class="ml-1 mt-2">  </h2> 
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

        public mouseOver(data) {
               console.log(data)
        }


    @courtListState.State
    public courtListInformation!: any

    @courtListState.Action
    public UpdateCourtList!: (newCourtListInformation: any) => void 

    // mounted () {                 
    //     this.getCourtListDetails();
    // }

    public getCourtListDetails(): void {
        
        this.isDataReady = false;
        this.isMounted = false;
       
        this.$http.get('/api/files/court-list?agencyId='+ this.courtListLocationID +'&roomCode='+ this.courtListRoom+'&proceeding=' +this.courtListDate)
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

    selectedDate = new Date;
    location = "Vancouver"
    courtListLocationID = '4801'
    courtListRoom = "005"
    courtListDate = '2015-01-21'

    public BackToPreviouDay()
    {       
        //console.log(this.selectedDate.toISOString().substring(0,10))
        this.selectedDate.setDate(this.selectedDate.getDate() - 1)
        //console.log(this.selectedDate.toISOString().substring(0,10))
        this.courtListDate = this.selectedDate.toISOString().substring(0,10)
        console.log( this.courtListDate)
        //this.getCourtListDetails();
    }

    public JumpToNextDay()
    {
        //console.log(this.selectedDate.toISOString().substring(0,10))
        this.selectedDate.setDate(this.selectedDate.getDate() + 1)
        //console.log(this.selectedDate.toISOString().substring(0,10))
        this.courtListDate = this.selectedDate.toISOString().substring(0,10)
        console.log( this.courtListDate)
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