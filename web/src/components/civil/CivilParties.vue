<template>
<body>
    <b-card bg-variant="white">
        <b-row cols="2" >
            <b-col class="mt-2" md="6" cols="6">
                <div>
                    <h3 class="mx-2 font-weight-normal"> {{leftRole}} ({{numberOfLeftParties}}) </h3>
                    <hr class="mb-0 bg-light" style="height: 5px;"/> 
                </div>

                <b-card bg-variant="white" style="height: 400px; overflow-y: auto;">           
                    <b-table
                    :items="leftPartiesInfo"
                    :fields="fields"
                    :sort-by.sync="sortBy"
                    :sort-desc.sync="sortDesc"
                    :no-sort-reset="true"
                    borderless
                    striped
                    small
                    sort-icon-left
                    responsive="sm"
                    >   
                        <template v-for="(field,index) in fields" v-slot:[`head(${field.key})`]="data">
                            <b v-bind:key="index" :class="field.headerStyle" > {{ data.label }}</b>
                        </template>                
                        <template v-for="(field,index) in fields" v-slot:[`cell(${field.key})`]="data" >
                            <span v-bind:key="index" :style="field.cellStyle" v-if="data.field.key != 'Counsel'">  {{ data.value }} </span>
                            <span v-bind:key="index" :style="field.cellStyle" v-if="data.field.key == 'Counsel'"><span v-for="(counsel, counselIndex) in data.value"  v-bind:key="counselIndex" style= "white-space: pre-line" >CEIS: {{ counsel }}</span> </span>
                        </template>
                        
                    </b-table>
                </b-card>
            </b-col>
            <b-col class="mt-2" md="6" cols="6" >
                <div>
                    <h3 class="mx-2 font-weight-normal"> {{rightRole}} ({{numberOfRightParties}}) </h3>
                    <hr class="mb-0 bg-light" style="height: 5px;"/> 
                </div>

                <b-card bg-variant="white" style="height: 400px; overflow-y: auto;">           
                    <b-table
                    :items="rightPartiesInfo"
                    :fields="fields"
                    :sort-by.sync="sortBy"
                    :sort-desc.sync="sortDesc"
                    :no-sort-reset="true"
                    borderless
                    striped
                    small
                    sort-icon-left
                    responsive="sm"
                    >   
                        <template v-for="(field,index) in fields" v-slot:[`head(${field.key})`]="data">
                            <b v-bind:key="index" :class="field.headerStyle" > {{ data.label }}</b>
                        </template>                
                        <template v-for="(field,index) in fields" v-slot:[`cell(${field.key})`]="data" >
                            <span v-bind:key="index" :style="field.cellStyle" v-if="data.field.key != 'Counsel'">  {{ data.value }} </span>
                            <span v-bind:key="index" :style="field.cellStyle" v-if="data.field.key == 'Counsel'"><span v-for="(counsel, counselIndex) in data.value"  v-bind:key="counselIndex" style= "white-space: pre-line" >CEIS: {{ counsel }}</span> </span>
                        </template>
                        
                    </b-table>
                </b-card>

            </b-col>    

        </b-row>
    </b-card> 

</body>
</template>

<script lang="ts">
import { Component, Vue } from "vue-property-decorator";
import { namespace } from "vuex-class";
import "@store/modules/CivilFileInformation";
const civilState = namespace("CivilFileInformation");

@Component
export default class CivilParties extends Vue {

    @civilState.State
    public civilFileInformation!: any;

    mounted() {
        this.getParties();
    }

    public getParties(): void {
        this.leftRole = this.civilFileInformation.detailsData.leftRoleDsc;
        this.rightRole = this.civilFileInformation.detailsData.rightRoleDsc;       
        this.leftPartiesInfo = this.civilFileInformation.leftPartiesInfo
        this.rightPartiesInfo = this.civilFileInformation.rightPartiesInfo
        this.numberOfLeftParties = this.leftPartiesInfo.length;  
        this.numberOfRightParties = this.rightPartiesInfo.length;

        this.isMounted = true;          
    } 
  
    isMounted = false;
    leftPartiesInfo = [];
    rightPartiesInfo = [];
    leftRole = '';
    rightRole = '';
    numberOfLeftParties = 0
    numberOfRightParties = 0;
    sortBy = 'Name';
    sortDesc = false;

    fields =  
    [
        {key:'Name',                  sortable:true,  tdClass: 'border-top',  headerStyle:'text-primary',   cellStyle:'font-weight: bold; font-size: 14px;'},
        {key:'Role',                  sortable:false, tdClass: 'border-top',  headerStyle:'text',         cellStyle:'font-size: 14px;'},
        {key:'Counsel',               sortable:false, tdClass: 'border-top', headerStyle:'text',         cellStyle:'font-size: 14px;'}
    ]; 

    public getNameOfJustin(lastName, givenName) {      

        if(!lastName && !givenName)        
            return
        if(!lastName)        
            return 'JUSTIN: '+givenName;       
        else if(!givenName)       
            return 'JUSTIN: '+lastName;      
        else         
            return ('JUSTIN: '+givenName +' ' + lastName );        
    }

}
</script>

<style scoped>
 .card {
        border: white;
    }

</style>