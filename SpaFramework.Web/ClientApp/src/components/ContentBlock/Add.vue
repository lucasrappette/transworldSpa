<template>
  <form-page-template :page-title="pageTitle" :item="item">
    <content-block-fields :item="item" v-on:submit="onSubmit" v-on:cancel="onCancel">
      <template v-slot:save>Add Content Block</template>
    </content-block-fields>
  </form-page-template>
</template>

<script>
import axios from "axios";
import FormMixin from '../Mixins/FormMixin.vue';

export default {
  name: "ContentBlockAdd",
  mixins: [FormMixin],
  props: ['id'],
  data() {
    return {
      item: {
        title: 'New Content Block',
        isPage: true,
        description: '',
        value: ''
      }
    };
  },
  computed: {
    itemTitle: function () {
      return this.item.title;
    },
    pageTitle: function () {
      return 'New Content Block' + (this.itemTitle && this.itemTitle.length > 0 ? ': ' + this.itemTitle : '');
    }
  },
  methods: {
    load: function () {
    },
    onCancel(evt) {
      this.$router.push('/contentBlock');
    },
    onSubmit(evt) {
      let url = '/api/contentBlock';

      axios
        .post(url, this.item)
        .then(response => {
          this.item = response.data;

          this.processAddSuccessResponse(response, 'Content Block');

          this.$router.push('/contentBlock');
        })
        .catch(error => {
          this.processAddErrorResponse(error, 'Content Block');
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