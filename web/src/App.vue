<template>
    <div id="app">
        <navigation-topbar />
        <Home />
        <CivilDocumentsView/>
        <navigation-footer id="footer" />
    </div>
</template>

<script lang="ts">
    import NavigationTopbar from "./components/NavigationTopbar.vue";
    import NavigationFooter from "./components/NavigationFooter.vue";
    import CivilDocumentsView from "./components/CivilDocumentsView.vue";
    import { Component, Vue } from 'vue-property-decorator';
    import Home from './components/Home.vue';

    @Component({
        components: {
            Home,
            NavigationTopbar,
            NavigationFooter,
            CivilDocumentsView
        }
    })
    export default class App extends Vue { }
</script>

<style>
</style>
