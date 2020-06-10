<template>
<body>
    <b-row cols="3">
        <b-col md="4">
            <b-form-group
                label="Location *"
                label-cols-sm="3"
                label-align-sm="right"
                label-size="md"
                label-for="initialSortSelect"
                class="mb-0"
                >
                <b-form-select
                    v-model="sortDirection"
                    id="initialSortSelect"
                    size="sm"
                    :options="['asc', 'desc', 'last']"
                ></b-form-select>
            </b-form-group>
        </b-col>
        <b-col md="2">
             <!-- <b-form-group
                id="input-group-1"
                label="Email address:"
                label-for="input-1"
                description="We'll never share your email with anyone else.">                                            
                    <b-form-input
                    id="input-1"
                    v-model="form.email"
                    type="email"
                    required
                    placeholder="Enter email"
                    ></b-form-input>
            </b-form-group>  -->
        </b-col>
        <b-col md="2">
            <b> jkdfvdh </b> 
        </b-col>
    </b-row>

</body>
</template>

<script lang="ts">
import { Component, Vue } from 'vue-property-decorator';
import { namespace } from 'vuex-class';
import '@store/modules/CriminalFileInformation';
const criminalState = namespace('CriminalFileInformation');

@Component
export default class CriminalCaseDetails extends Vue {


}

</script>