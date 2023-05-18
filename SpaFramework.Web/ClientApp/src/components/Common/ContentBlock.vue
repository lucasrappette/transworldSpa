<template>
  <div>
    <div v-if="item" v-html="contentBlock"></div>
  </div>
</template>

<script>
import axios from "axios";
import marked from 'marked'

export default {
  name: "ContentBlock",
  mixins: [
  ],
  props: [
    'slug',
    'tokens',
  ],
  data() {
    return {
      item: null,
    };
  },
  computed: {
    contentBlock: function () {
      let value = this.item.contentText;

      if (this.tokens) {
        let keys = Object.keys(this.tokens);

        for (let i = 0; i < keys.length; ++i) {
          let key = keys[i];
          value = value.replace('%' + key + '%', this.tokens[key]);
        }
      }

      return marked(value);
    }
  },
  methods: {
    load: function () {

      axios
        .get('/api/contentBlock/slug/' + this.slug)
        .then(response => {
          this.item = response.data;
        })
        .catch(error => {
          console.log(error);
        });
    }
  },
  mounted () {
    this.load();
    // Use this instead of the previous line to test the "Loading" bar
    //window.setTimeout(this.load, 3000);
  }
};
</script>