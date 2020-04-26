<template>
  <div class="sample-api">
    <h1>Sample page fetching data from .NET Core API</h1>
    <h3>The following values are received through the .NET Core API:</h3>
    <ul class="values-list">
      <li v-for="value in values" :key="value">{{ value }}</li>
    </ul>
  </div>
</template>

<script>
export default {
  name: 'sample-api',
  data () {
    return {
      values: []
    }
  },
  created () {
    // Send request to the ASP.NET Core application.
    return fetch('http://localhost:5000/api/values')
      .then(res => res.json())
      .then(data => { this.values = data })
  }
}
</script>

<style scoped>
.values-list {
  list-style: none;
}
</style>
