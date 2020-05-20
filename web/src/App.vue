<template>
    <div class="app-outer fill-body" id="app">
        <navigation-topbar />
            <router-view />
        <navigation-footer id="footer" />
    </div>
</template>

<script lang="ts">
    import NavigationTopbar from "@components/NavigationTopbar.vue";
    import NavigationFooter from "@components/NavigationFooter.vue";
    import { Component, Vue } from 'vue-property-decorator';

    @Component({
        components: {
            NavigationTopbar,
            NavigationFooter
        }
    })
    
    export default class App extends Vue { }
</script>