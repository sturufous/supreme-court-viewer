
<template>
    <div class="home">
        <br />
        <div class="mb-2">
            <h2> Temporary landing page for SCV </h2>
        </div>
        <div>
            <b-col class="mb-2" sm="9">
                <label for="proceeding-datepicker">Proceeding date:</label>
                <b-form-datepicker id="proceeding-datepicker" v-model="proceedingDate" class="mb-2"></b-form-datepicker>
            </b-col>
            <b-col class="mb-2" sm="9">
                <label for="location"> Location: </label>
                <b-form-input id="location" v-model="location" placeholder="Enter location"></b-form-input>
            </b-col>
            <b-col class="mb-2" sm="9">
                <label for="room"> Room: </label>
                <b-form-input id="room" v-model="room" placeholder="Enter room"></b-form-input>
            </b-col>
            <b-col class="mb-3" sm="9">
                <label for="filenumber"> File number: </label>
                <b-form-input id="filenumber" v-model="fileNumber" placeholder="Enter file number"></b-form-input>
            </b-col>
            <b-col class="mb-3" sm="9">
                <b-button>Search</b-button>
            </b-col>
        </div>
    </div>
</template>

<script lang="ts">
    import axios from 'axios';
    import { Component, Vue } from 'vue-property-decorator';

    @Component
    export default class Home extends Vue {
        proceedingDate = '';
        location = '';
        room = '';
        fileNumber = '';

        fetchApi(): void {
            axios.defaults.baseURL = process.env.VUE_APP_API_URL;
            /*axios.get('/api/values')
                .then((response) =>  {
                    this.msg = `Data fetched: ${response.data}`;
                })
                .catch(() => {
                    this.msg = `Failed fetching data.`;
                });*/
        }
        mounted(): void {
            this.fetchApi();
        }
    }
</script>

<!-- Add "scoped" attribute to limit CSS to this component only -->
<style scoped>
    button {
        background-color: #036
    }

    input {
        width: 300px;
    }

    #proceeding-datepicker__outer_ {
        width: 300px;
    }

    .home {
        margin-left: 25px;
    }

    .fade-enter-active, .fade-leave-active {
        transition: opacity .5s;
    }

    .fade-enter, .fade-leave-to /* .fade-leave-active below version 2.1.8 */ {
        opacity: 0;
    }
</style>
