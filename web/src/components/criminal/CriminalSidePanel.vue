<template>
<body>
    <div class="m-2">
        <div style="background-color: #E3E3E3; width: 200px;">
            <p style="font-size: 13px;" class="m-1"> Files to Schedule/View (1) </p>
            <b-dropdown  variant=" text-dark bg-white  text-left"  :text="fileNumberText" block split class="m-1" size='sm'>    
                <b-dropdown-item>
                    {{fileNumberText}}
                </b-dropdown-item> 
            </b-dropdown>
            <div>
                <b-button  style="font-size: 11px;" class="m-1" variant="secondary" >Add File(s)</b-button>
                <b-button  style="font-size: 11px;" class="m-0" variant="secondary">Remove this File</b-button>        
            </div>
            <b-card bg-variant="transparent" style="border-color: #E3E3E3">
                <b-button  style="font-size: 12px;" block  variant="primary">Schedule Appearance</b-button>
            </b-card>
        </div>
        <br/>
        <div class="mx-auto bg-white" style="width: 200px;">
            <b-nav vertical>
                <b-nav-item style="font-size: 10px; font-weight: bold;" disabled>ON THIS FILE</b-nav-item>
                    <b-nav-item  
                        v-for="(panelItem, index) in panelItems" 
                        :key="index" 
                        style="line-height: 1.25;"
                        v-on:click="SelectPanelItem(panelItem)" >
                            {{panelItem}}
                    </b-nav-item>    
            </b-nav>
        </div>
    </div>
</body>    
</template>

<script lang="ts">
import { Component, Vue } from 'vue-property-decorator';
import { namespace } from 'vuex-class';
import '@store/modules/CriminalFileInformation';
const criminalState = namespace('CriminalFileInformation');

@Component
export default class CriminalSidePanel extends Vue {

    @criminalState.State
    public showSections    

    @criminalState.Action
    public UpdateShowSections!: (newShowSections: any) => void

    @criminalState.State
    public criminalFileInformation!: any
     
    mounted () {             
        const data = this.criminalFileInformation.detailsData;
        this.fileNumberText = data.fileNumberTxt;       
    }

    fileNumberText = ' ';    

    panelItems = [ 
       'Case Details', 'Future Appearance', 'Past Appearance', 'Witnesses', 'Documents', 'Sentence/Order Details'    
    ];

    public SelectPanelItem(panelItem) {

        const sections = this.showSections
    
        for(const item of this.panelItems)
        {
            if(item == panelItem )
                sections[item] = true;
            else
                sections[item] = false;
        }
        
        this.UpdateShowSections(sections);
    }

}
</script>