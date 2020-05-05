<template>
    <div class="app-outer fill-body" id="app">
        <navigation-topbar />
        <main class="app-content fill-body">
            <router-view />
        </main>        
        <navigation-footer class="navbar-fixed-bottom" id="footer" />
    </div>
</template>

<script lang="ts">
    import NavigationTopbar from "./components/NavigationTopbar.vue";
    import NavigationFooter from "./components/NavigationFooter.vue";
    import { Component, Vue } from 'vue-property-decorator';

    @Component({
        components: {
            NavigationTopbar,
            NavigationFooter
        }
    })
    
    export default class App extends Vue { }
</script>

<style>

/* .main {
    min-height:100vh;

} */




</style>
