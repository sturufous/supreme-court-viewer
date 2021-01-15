<template>
    <div>    
        <b-overlay :show="show" rounded="sm"> 
            <slot/>
            <template v-slot:overlay>               
                <div style="text-align: center"> 
                        <loading-spinner/>
                </div>                
            </template> 
        </b-overlay>
    </div>
</template>
    
        
<script lang="ts">
import { Component, Vue, Prop} from 'vue-property-decorator';
@Component
export default class CustomOverlay extends Vue {
    @Prop({required: true})
    show!: boolean;

}
</script>

<style>

</style>