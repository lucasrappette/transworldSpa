<template>
  <form-page-template :page-title="pageTitle" :item="item">
    <hr />
    <dealer-fields :item="item" v-on:submit="onSubmit" v-on:cancel="onCancel">
      <template v-slot:save>Save Dealer</template>
    </dealer-fields>
  </form-page-template>
</template>

<script>
import axios from "axios";
import FormMixin from '../Mixins/FormMixin.vue';
import { mapState, mapGetters, mapMutations, mapActions } from 'vuex'

export default {
  name: "DealerEdit",
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
      return this.item.name;
    },
    pageTitle: function () {
      return 'Dealer: ' + this.itemTitle;
    }
  },
  methods: {
    ...mapActions('cachedData', ['setKnownPageName']),
    load: function () {
      let url = '/api/dealer/' + this.id + '?context=WebApiElevated';

      axios
        .get(url)
        .then(response => {
          this.item = response.data;

          this.setKnownPageName({ path: this.$route.path, name: this.item.name});
        })
        .catch(error => {
          console.log(error);
        });
    },
    onCancel(evt) {
      this.goToParentPage();
    },
    onSubmit(evt) {
      let url = '/api/dealer/' + this.id + '?context=WebApiElevated';

      axios
        .put(url, this.item)
        .then(response => {
          this.item = response.data;
          this.$store.dispatch('cachedData/reloadDealers');
          this.processEditSuccessResponse(response, 'dealer');

          this.goToParentPage();
        })
        .catch(error => {
          this.processEditErrorResponse(error, 'dealer');
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