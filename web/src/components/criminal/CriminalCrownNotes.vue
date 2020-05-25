<template>
<body>
   <b-card bg-variant="white">
        <div>
            <h3 class="mx-2 font-weight-normal"> Crown Notes to JCM </h3>
            <hr class="mx-1 bg-light" style="height: 5px;"/> 
        </div>

        <b-card v-if="!(crownNotes.length>0)">
            <span class="text-muted"> No crown notes to JCM. </span>
        </b-card>

        <b-card bg-variant="white" v-if="isMounted && (crownNotes.length>0)">           
            <b-table        
            borderless
            :items="crownNotes"
            :fields="fields"            
            thead-class="d-none"
            responsive="sm"          
            striped
            >   
                <template v-for="(field,index) in fields" v-slot:[`head(${field.key})`]="data">
                    <b v-bind:key="index" :class="field.headerStyle" > {{ data.label }}</b>
                </template>
                <template v-for="(field,index) in fields" v-slot:[`cell(${field.key})`]="data" >                   
                    <span v-bind:key="index" :class="field.cellStyle" style= "white-space: pre" > {{ data.value }}</span>
                </template>
            </b-table>
        </b-card>
    </b-card>
</body>
</template>

<script lang="ts">
import { Component, Vue, Watch } from 'vue-property-decorator';
import { namespace } from 'vuex-class';
import '@store/modules/CriminalFileInformation';
const criminalState = namespace('CriminalFileInformation');
enum estimateLengthUnit {
    'Hours' = 0, 'Days' = 1, 'Months' = 2, 'M' = 3,
    'Years' = 4, 'Minutes' = 5, 'Weeks' = 6, 'HR' = 7
}

@Component
export default class CriminalCrownNotes extends Vue {

    @criminalState.State
    public criminalFileInformation!: any

    fields =[
        {key:"CrownNotes", label: "Crown Notes"}
    ];

    public getCrownNotes(): void {
       
        const data = this.criminalFileInformation.detailsData;
        const crownNotes: string[] = [];
        if (data.crown.length > 0) {
            for (const assignee of data.crown) {
                assignedCrown.push(this.formatNames(assignee.lastNm) + ", " + this.formatNames(assignee.givenNm))
            }
            console.log(assignedCrown)
        }
        
        this.isMounted = true;

    }
    
    public formatNames (name: string): string {
        return name.charAt(0).toUpperCase() + name.slice(1).toLowerCase();
    }

    mounted () {              
        this.getCrownNotes();  
    }

    crownNotes: any[] = [];
    isMounted = false  
}
</script>

<style scoped>
 .card {
        border: white;
    }
</style>