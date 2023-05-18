<template>
  <form-page-template :page-title="pageTitle" :item="item">
    <content-block-fields :item="item" v-on:submit="onSubmit" v-on:cancel="onCancel">
      <template v-slot:save>Save Content Block</template>
    </content-block-fields>
  </form-page-template>
</template>

<script>
import axios from "axios";
import FormMixin from '../Mixins/FormMixin.vue';

export default {
  name: "ContentBlockEdit",
  mixins: [FormMixin],
  props: ['id'],
  data() {
    return {
      item: null
    };
  },
  computed: {
    itemTitle: function () {
      if (!this.item)
        return null;
      return this.item.title;
    },
    pageTitle: function () {
      return 'Content: ' + this.itemTitle;
    }
  },
  methods: {
    load: function () {
      let url = '/api/contentBlock/' + this.id;

      axios
        .get(url)
        .then(response => {
          this.item = response.data;
        })
        .catch(error => {
          console.log(error);
        });
    },
    onCancel(evt) {
      this.$router.push('/contentBlock');
    },
    onSubmit(evt) {
      let url = '/api/contentBlock/' + this.id;

      axios
        .put(url, this.item)
        .then(response => {
          this.item = response.data;

          this.processEditSuccessResponse(response, 'Content Block');

          this.$router.push('/contentBlock');
        })
        .catch(error => {
          this.processEditErrorResponse(error, 'Content Block');
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