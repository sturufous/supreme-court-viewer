<template>
    <b-card bg-variant="white" class="home">    
      
        <b-card  align="left" >
            
            <b-card >
                <b-card-title class="h2"> Temporary landing page for SCV </b-card-title>
            </b-card>

            <b-card >
                <b-card-text for="filenumber"> File number: </b-card-text>                
                <b-form-input id="filenumber" v-model="fileDocument.fileNumber" placeholder="Enter file number"></b-form-input>                
            </b-card>

            <b-card> 
                <b-card-text for="civil/criminal"> Civil/Criminal: </b-card-text>
                <b-form-select v-model="selected" :options="options"></b-form-select>               
            </b-card>
        
            <b-card >
                <b-button @click="navigateToDocumentsView(fileDocument)">Search</b-button>
            </b-card>          

        </b-card>
    </b-card>
</template>

<script lang="ts">
    import { Component, Vue } from 'vue-property-decorator';
    import { namespace } from 'vuex-class'
    import CivilFileDocuments from '../store/modules/CivilFileDocuments';
    import CriminalFileDocuments from '../store/modules/CriminalFileDocuments';
    const civilState = namespace('CivilFileDocuments');
    const criminalState = namespace('CriminalFileDocuments');

    @Component
    export default class Home extends Vue {

        selected = 'criminal';
        options= [
          { value: 'civil', text: 'Civil' },
          { value: 'criminal', text: 'Criminal' }
        ]
        
        fileDocument = { }

        @civilState.Action
        public UpdateCivilFile!: (newCivilFileDocument: any) => void

        @criminalState.Action
        public UpdateCriminalFile!: (newCriminalFileDocument: any) => void           
        
        // TODO: add validation so that the user has to enter values before clicking the search button
        navigateToDocumentsView(fileDocument): void {

            if(this.selected == 'civil') {
                this.UpdateCivilFile(fileDocument)
                this.$router.push({name:'CivilDocumentsView', params: {fileNumber: fileDocument.fileNumber}})
            } else if(this.selected == 'criminal') {
                this.UpdateCriminalFile(fileDocument)
                this.$router.push({name:'CriminalDocumentsView', params: {fileNumber: fileDocument.fileNumber}})
            }
            
        }        
    }
</script>

<!-- Add "scoped" attribute to limit CSS to this component only -->
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
