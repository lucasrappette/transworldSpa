<template>
  <div>
    <b-container fluid class="py-3">
      <b-row>
        <b-col>
          <page-loading v-if="!item" />
          <page-title v-if="item" :title="pageTitle" />
          <hr />
          <div v-if="item" class="mt-3">
            <div v-html="item.contentHtml"></div>
          </div>
        </b-col>
      </b-row>
    </b-container>
  </div>
</template>

<script>
import axios from "axios";

export default {
  name: "ContentBlockView",
  props: ['slug'],
  data() {
    return {
      item: null
    };
  },
  computed: {
    pageTitle: function () {
      return this.item.title;
    }
  },
  methods: {
    load: function () {
      let url = '/api/contentBlock/slug/' + this.slug;

      axios
        .get(url)
        .then(response => {
          this.item = response.data;
        })
        .catch(error => {
          console.log(error);
        });
    }    
  },
  watch: {
    '$route' (to, from) {
      this.load();
    }
  },
  mounted () {
    this.load();
    // Use this instead of the previous line to test the "Loading" bar
    //window.setTimeout(this.load, 3000);
  }
};
</script>