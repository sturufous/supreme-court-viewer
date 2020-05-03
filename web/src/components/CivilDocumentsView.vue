
<template>
<body>
    
    <!-- <div class="btn-group">
        <b-button-group>
            <b-button>Button 1</b-button>
            <b-button>Button 2</b-button>
            <b-button>Button 3</b-button>
        </b-button-group>
        <button>All</button>
        <button>Schedualed</button>
        <button>Pleadings</button>
    </div> -->

    <div id="tabs" class="container">
        <div class="tabs">
            <a v-for="(filterword, index) in filterWords" :key="index" v-on:click="activetab=index" v-bind:class="[ activetab === index ? 'active' : '' ]"> {{filterword}}</a> 
        </div>
    </div>

    <table class="table table-condensed table-striped">
      <thead>
          <tr>
              <th v-for="(column, index) in columns" :key="index"> {{column}}</th>
          </tr>
      </thead>
      <tbody>
        <tr v-for="(item, index) in filterBy(items, searchClient)"  :key="index">
            <td v-for="(column, indexColumn) in columns" :key="indexColumn">{{item[column]}}</td>
        </tr>
          <!-- <tr v-for="(item, index) in items" :key="index">
              <td v-for="(column, indexColumn) in columns" :key="indexColumn">{{item[column]}}</td>
          </tr> -->
      </tbody>
    </table>
</body>
</template>

Vue.filter('pluck', function (objects, key) {
    return objects.map(function(object) { 
        return object[key];
    });
});

<script lang="ts">
import Vue from 'vue'
import {mapGetters} from 'vuex'
export default Vue.extend({
    el: '#tabs',
    data() {
        return {
            activetab: 0,        

            items: [
                {
                    'Seq.':'1',
                    'Document Type': 'hyello',
                    'Act': 'ok ok',
                    'Date Filed': '2018-09-09',
                    'Issues': 'gfgxdf'
                },
                {
                    'Seq.':'2',
                    'Document Type': 'hello4',
                    'Act': 'ok',
                    'Date Filed': '18-09-09',
                    'Issues': 'gf555gxf'
                }
            ],
            columns: [ 'Seq.', 'Document Type', 'Act', 'Date Filed', 'Issues'],
            filterWords: ['All', 'Schedualed', 'Pleadings', 'Motions', 'FS/Affidavits', 'Orders', 'Concluded', 'Court Summary']            
        };
    },
    
    methods: {   
        
    },
    
    computed: {
        ...mapGetters(['ids','itemById']),
        descriptionById: function() {
            return (id) => this.itemById(id).description;
        }
    }

    // computed: {
    //     filtered () {
    //         const filtered = this.items.filter(item => {
    //             return Object.keys(this.filters).every(key =>
    //                 String(item[key]).includes(this.filters[key]))
    //         })
    //         return filtered.length > 0 ? filtered : [{
    //             id: '',
    //             issuedBy: '',
    //             issuedTo: ''
    //         }]
    //     }
    // }
    
})
</script>






<style>
    /* .btn-group {
        background-color: #F0F0F0; 
        border: 1px solid rgba(0, 128, 0, 0.034);
        color: white;
        padding: 15px 32px;
        text-align: center;
        text-decoration: none;
        display: inline-block;
        font-size: 16px;
        cursor: pointer;
        float: left;
    } */

    .container {  
        max-width: 1620px; 
        min-width: 420px;
        margin: 40px auto;
        font-family: Arial, Helvetica, sans-serif;
        font-size: 0.9em;
        color: #888;
        background-color: #F0F0F0; /* Green */
    }

    /* Style the tabs */
    .tabs {
        overflow: hidden;
        margin-left: 20px;
        margin-bottom: -2px; 
    }

    .tabs ul {
        list-style-type: none;
        margin-left: 20px;
    }

    .tabs a{
        float: left;
        cursor: pointer;
        padding: 12px 24px;
        transition: background-color 0.2s;
        border: 1px solid #ccc;
        border-right: none;
        background-color: #f1f1f1;
        border-radius: 5px 5px 5px 5px;
        font-weight: bold;
    }
    .tabs a:last-child { 
        border-right: 1px solid #ccc;
    }

    /* Change background color of tabs on hover */
    .tabs a:hover {
        background-color: #aaa;
        color: #fff;
    }

    /* Styling for active tab */
    .tabs a.active {
        background-color: #fff;
        color: #484848;
        border-bottom: 2px solid #fff;
        cursor: default;
    }

    /* Style the tab content */
    .tabcontent {
        padding: 30px;
        border: 1px solid #ccc;
        border-radius: 10px;
    box-shadow: 3px 3px 6px #e1e1e1
    }
</style>
