<template>
  <form-page-template :page-title="pageTitle" :item="item">
    <dealer-fields :item="item" v-on:submit="onSubmit" v-on:cancel="onCancel">
      <template v-slot:save>Add Dealer</template>
    </dealer-fields>
  </form-page-template>
</template>

<script>
import axios from "axios";
import FormMixin from '../Mixins/FormMixin.vue';

export default {
  name: "DealerAdd",
  mixins: [FormMixin],
  props: [],
  data() {
    return {
      item: {
        name: null
      }
    };
  },
  computed: {
    itemTitle: function () {
      if (this.item && this.item.name)
        return this.item.name;
      else
        return null;
    },
    pageTitle: function () {
      return 'New Dealer' + (this.itemTitle && this.itemTitle.length > 0 ? ': ' + this.itemTitle : '');
    }
  },
  methods: {
    load: function () {
      axios
        .get('/api/dealer/new')
        .then(response => {
          this.item = response.data;
        })
        .catch(error => {
          this.processAddErrorResponse(error, 'dealer');
        });
    },
    onCancel(evt) {
      this.$router.push('/dealer');
    },
    onSubmit(evt) {
      let url = '/api/dealer?context=WebApiElevated';

      axios
        .post(url, this.item)
        .then(response => {
          this.item = response.data;

          this.processAddSuccessResponse(response, 'dealer');

          this.$store.dispatch('cachedData/reloadDealers');

          this.$router.push('/dealer');
        })
        .catch(error => {
          this.processAddErrorResponse(error, 'dealer');
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