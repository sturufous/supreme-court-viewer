<template>
  <footer :class="footerPosition">
    <b-navbar class="navbar navbar-expand-lg navbar-dark">
      <!-- Navbar content -->
      <ul class="navbar-nav">
        <li class="nav-item">
          <a
            class="nav-link"
            href="https://www2.gov.bc.ca/gov/content/home/disclaimer"
            target="_blank"
            >Disclaimer</a
          >
        </li>
        <li class="nav-item">
          <a class="nav-link" href="/terms" target="_blank">Terms of Use</a>
        </li>
        <li class="nav-item">
          <a
            class="nav-link"
            href="https://www2.gov.bc.ca/gov/content/home/privacy"
            target="_blank"
            >Privacy</a
          >
        </li>
        <li class="nav-item">
          <a
            class="nav-link"
            href="https://www2.gov.bc.ca/gov/content/home/accessibility"
            target="_blank"
            >Accessibility</a
          >
        </li>
        <li class="nav-item">
          <a
            class="nav-link"
            href="https://www2.gov.bc.ca/gov/content/home/copyright"
            target="_blank"
            >Copyright</a
          >
        </li>
        <li class="nav-item">
          <a
            class="nav-link"
            href="https://www2.gov.bc.ca/gov/content/home/contact-us"
            target="_blank"
            >Contact Us</a
          >
        </li>
      </ul>
    </b-navbar>
  </footer>
</template>


<script lang="ts">
  import { Component, Vue } from 'vue-property-decorator';

  @Component
  export default class NavigationFooter extends Vue {   

  created() {
    window.addEventListener("resize", this.onResize);
  }

  destroyed() {
    window.removeEventListener("resize", this.onResize);
  }

  public onResize() {
    if(window.innerWidth > 1100) { this.footerPosition ='fixed-bottom';}
    else {this.footerPosition = 'flex-bottom';}
  }

  footerPosition = 'fixed-bottom';
   
  
  }
</script>


