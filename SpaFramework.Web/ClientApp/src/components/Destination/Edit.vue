<template>
  <form-page-template :page-title="pageTitle" :item="item">
    <hr />
    <destination-fields :item="item" v-on:submit="onSubmit" v-on:cancel="onCancel">
      <template v-slot:save>Save Destination</template>
    </destination-fields>
  </form-page-template>
</template>

<script>
import axios from "axios";
import FormMixin from '../Mixins/FormMixin.vue';
import { mapState, mapGetters, mapMutations, mapActions } from 'vuex'

export default {
  name: "DestinationEdit",
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
      return 'Destination: ' + this.itemTitle;
    }
  },
  methods: {
    ...mapActions('cachedData', ['setKnownPageName']),
    load: function () {
      let url = '/api/destination/' + this.id + '?context=WebApiElevated';

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
      let url = '/api/destination/' + this.id + '?context=WebApiElevated';

      axios
        .put(url, this.item)
        .then(response => {
          this.item = response.data;
          this.processEditSuccessResponse(response, 'destination');

          this.goToParentPage();
        })
        .catch(error => {
          this.processEditErrorResponse(error, 'destination');
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