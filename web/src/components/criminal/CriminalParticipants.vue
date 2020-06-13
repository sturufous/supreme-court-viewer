<template>

    <b-card bg-variant="white" no-body>
        <div>
            <h3 class="mx-4 font-weight-normal"> Participants ({{numberOfParticipants}}) </h3>
            <hr class="mx-3 bg-light" style="height: 5px;"/> 
        </div>

        <b-card no-body class="mx-3 mb-5">
            <b-table
            :items="participantList"
            :fields="fields"
            :sort-by.sync="sortBy"
            :sort-desc.sync="sortDesc"
            :no-sort-reset="true"
            sort-icon-left
            borderless
            small
            responsive="sm"
            >   
                <template v-for="(field,index) in fields" v-slot:[`head(${field.key})`]="data">
                    <b v-bind:key="index" :class="field.headerStyle" > {{ data.label }}</b>
                </template>  

                <template v-for="(field,index) in fields" v-slot:[`cell(${field.key})`]="data" >
                    <b-badge class = "mt-1"  style="font-weight:normal; font-size:16px" variant="white" v-bind:key="index" >  {{ data.value }} </b-badge>
                </template>

                <template v-slot:cell(Name)="data" >               
                                               
                        <b-dropdown size="sm" style="height:35px;" no-caret variant="text-info"  >
                            <template v-slot:button-content>
                                <b-button
                                    :variant="data.item.Charges.length>0? 'outline-primary text-info':'white'" 
                                    :disabled="data.item.Charges.length==0"
                                    style="transform: translate(-10px,-4px); border:0px; font-size:16px"
                                    size="sm"> 
                                    {{ data.value }}
                                    <b-icon v-if="data.item.Charges.length>0" class="ml-1" icon="caret-down-fill" font-scale="1"></b-icon>
                                </b-button>
                            </template>
                            <b-dropdown-text variant="white text-danger">Charges</b-dropdown-text>
                            <b-dropdown-divider></b-dropdown-divider>
                            <b-dropdown-item-button 
                                disabled                                                               
                                v-for="(file,index) in data.item.Charges" 
                                :key="index">                                
                                    <b>{{file["Code"]}}</b> &mdash; {{file["Description"]}}
                            </b-dropdown-item-button> 
                        </b-dropdown>                   
                </template>
 
                <template v-slot:cell(Status)="data" >
                        <b-badge  
                            v-for="(field,index) in data.value"
                            :key="index" 
                            class="mr-1 mt-2"
                            style="font-weight: normal; font-size: 14px;"
                            v-b-tooltip.hover 
                            :title='field.key' > 
                            {{ field.abbr }} 
                        </b-badge>
                </template>
            </b-table>
        </b-card>
       
    </b-card> 

</template>

<script lang="ts">
import { Component, Vue } from "vue-property-decorator";
import { namespace } from "vuex-class";
import "@store/modules/CriminalFileInformation";
import "@store/modules/CommonInformation";
const criminalState = namespace("CriminalFileInformation");
const commonState = namespace("CommonInformation");


@Component
export default class CriminalParticipants extends Vue {

    @commonState.State
    public displayName!: string;

    /* eslint-disable */
    @criminalState.State
    public criminalFileInformation!: any; 

    @commonState.Action
    public UpdateDisplayName!: (newInputNames: any) => void

    participantList: any[] = [];
    /* eslint-enable */ 
    

    isMounted = false;
    participantJson;
    numberOfParticipants = 0;
    sortBy = 'Name';
    sortDesc = false;

    fields =  
    [
        {key:'Name',                    sortable:true,  tdClass: 'border-top',  headerStyle:'text-primary', cellStyle:''},
        {key:'D.O.B.',                  sortable:false, tdClass: 'border-top',  headerStyle:'text',         cellStyle:''},
        {key:'Status',                  sortable:false, tdClass: 'border-top', headerStyle:'text',          cellStyle:''},
        {key:'Counsel',                 sortable:false, tdClass: 'border-top', headerStyle:'text',          cellStyle:''},
        {key:'Counsel Designation Filed',sortable:false, tdClass: 'border-top', headerStyle:'text',         cellStyle:''},
    ];

    mounted() {
        this.getParticipants();
    }

    public getParticipants(): void {   
        this.participantList = this.criminalFileInformation.participantList 
        this.numberOfParticipants = this.participantList.length;
        this.isMounted = true;          
    } 
}
</script>

<style scoped>
 .card {
        border: white;
    }

</style>