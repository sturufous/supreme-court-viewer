<template>
    <b-card bg-variant="white" class="home">    
      
        <b-card  align="left" >
            
            <b-card >
                <b-card-title class="h2"> Temporary landing page for SCV </b-card-title>
            </b-card>

            <b-card >
                <b-card-text for="filenumber"> File number: </b-card-text>                
                <b-form-input id="filenumber" v-model="fileInformation.fileNumber" placeholder="Enter file number"></b-form-input>                
            </b-card>

            <b-card> 
                <b-card-text for="civil/criminal"> Civil/Criminal: </b-card-text>
                <b-form-select v-model="selected" :options="options"></b-form-select>               
            </b-card>
        
            <b-card >
                <b-button @click="navigateToDocumentsView(fileInformation)">Search</b-button>
            </b-card>          

        </b-card>
    </b-card>
</template>

<script lang="ts">
    import { Component, Vue } from 'vue-property-decorator';
    import { namespace } from 'vuex-class'
    import '@store/modules/CivilFileInformation';
    import '@store/modules/CriminalFileInformation';
    const civilState = namespace('CivilFileInformation');
    const criminalState = namespace('CriminalFileInformation');

    @Component
    export default class Home extends Vue {

        selected = 'criminal';
        options= [
          { value: 'civil', text: 'Civil' },
          { value: 'criminal', text: 'Criminal' }
        ]
        
        fileInformation = { }

        @civilState.Action
        public UpdateCivilFile!: (newCivilFileInformation: any) => void

        @criminalState.Action
        public UpdateCriminalFile!: (newCriminalFileInformation: any) => void           
        
        // TODO: add validation so that the user has to enter values before clicking the search button
        navigateToDocumentsView(fileInformation): void {

            if(this.selected == 'civil') {
                this.UpdateCivilFile(fileInformation)
                this.$router.push({name:'CivilDocumentsView', params: {fileNumber: fileInformation.fileNumber}})
            } else if(this.selected == 'criminal') {
                this.UpdateCriminalFile(fileInformation)
                this.$router.push({name:'CriminalCaseDetails', params: {fileNumber: fileInformation.fileNumber}})
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
