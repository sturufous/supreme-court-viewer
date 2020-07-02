<template>
   <b-card  v-if= "isMounted" no-body>
       <div>         
            <h3 class="mx-4 font-weight-normal"> Case Notes and Comments </h3>
            <hr class="mx-3 bg-light" style="height: 5px;"/>         
        </div>
        <b-card class="mb-5" v-if="!notesExist">
            <span class="text-muted ml-4"> No notes or comments. </span>
        </b-card>       
        <b-card v-if="notesExist" bg-variant="white" no-body class="mx-3 mb-5">
            <b-table
            :items="civilNotes"
            :fields="fields"            
            thead-class="d-none"
            responsive="sm"
            borderless 
            small              
            striped
            >
                <template  v-slot:cell(NotesFieldName)="data">                    
                    <span>
                        <b> {{ data.value }}</b>
                    </span>                    
                </template> 
            </b-table>            
        </b-card>       
   </b-card> 
</template>

<script lang="ts">
import { Component, Vue } from 'vue-property-decorator';
import { namespace } from 'vuex-class';
import '@store/modules/CivilFileInformation';
import {civilFileInformationType, civilNotesType } from '../../types/civil';
const civilState = namespace('CivilFileInformation');

@Component
export default class CivilCommentNotes extends Vue {

    @civilState.State
    public civilFileInformation!: civilFileInformationType

    civilNotes: civilNotesType[] = [];    
    isMounted = false
    notesExist = false

    fields = [
        {key:"NotesFieldName", tdClass: 'border-top', label: "Notes Field Name"},
        {key:"NotesValue", tdClass: 'border-top', label: "Notes Value"}
    ];

    public getCivilNotes(): void {
       
        const data = this.civilFileInformation.detailsData;
        
        let notesInfo = {} as civilNotesType;
        notesInfo['NotesFieldName'] = "Trial Remark";
        notesInfo['NotesValue'] = data.trialRemarkTxt? data.trialRemarkTxt: '';
        this.civilNotes.push(notesInfo);
        notesInfo = {} as civilNotesType;

        notesInfo['NotesFieldName'] = "Comment To Judge";
        notesInfo['NotesValue'] = data.commentToJudgeTxt? data.commentToJudgeTxt: '';
        this.civilNotes.push(notesInfo);
        notesInfo = {} as civilNotesType;

        notesInfo['NotesFieldName'] = "File Comment";
        notesInfo['NotesValue'] = data.fileCommentText? data.fileCommentText: '';
        this.civilNotes.push(notesInfo);        
        if (data.trialRemarkTxt || data.commentToJudgeTxt || data.fileCommentText) {
            this.notesExist = true;
        }
        
        this.isMounted = true;
    }

    mounted () {              
        this.getCivilNotes();  
    }      
}
</script>

<style scoped>
 .card {
        border: white;
    }
</style>