<template>
   <b-card bg-variant="white">
        <div>
            <h3 class="mx-2 font-weight-normal"> Crown Notes to JCM </h3>
            <hr class="mb-0 bg-light" style="height: 5px;"/> 
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
                <template v-for="(field,index) in fields" v-slot:[`cell(${field.key})`]="data" >                   
                    <span v-bind:key="index" style= "white-space: pre" > {{ data.value }}</span>
                </template>
            </b-table>
        </b-card>
    </b-card>
</template>

<script lang="ts">
import { Component, Vue, Watch } from 'vue-property-decorator';
import { namespace } from 'vuex-class';
import '@store/modules/CriminalFileInformation';
const criminalState = namespace('CriminalFileInformation');

@Component
export default class CriminalCrownNotes extends Vue {

    @criminalState.State
    public criminalFileInformation!: any

    fields = [
        {key:'CrownNotes', label: 'Crown Notes'}
    ];

    public getCrownNotes(): void {
       
        const data = this.criminalFileInformation.detailsData;
        
        if (data.trialRemark.length > 0) {
            for (const note of data.trialRemark) {
                const crownNote = {};
                crownNote['CrownNotes'] = note.commentTxt
                this.crownNotes.push(crownNote)
            }
        }        
        this.isMounted = true;
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