<template>
    <!-- <div class="app-outer fill-body" id="app" style="overflow:hidden"> -->
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

<style lang="scss">
  
  @import "assets/_custom.scss";
  @import "~bootstrap/scss/bootstrap.scss";
  @import '~bootstrap-vue/dist/bootstrap-vue.css';

  body {
    margin: 0;
  }
 
</style>