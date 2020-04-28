<template>
  <div class="home">
    <img alt="BC Gov logo" src="../../public/images/bcid-192x192.png">
    <HelloWorld msg="Welcome to the Supreme Court Viewer App"/>
  </div>
</template>

<script>
// @ is an alias to /src
import HelloWorld from '@/components/HelloWorld.vue'

export default {
  name: 'Home',
  components: {
    HelloWorld
  }
}
</script>
