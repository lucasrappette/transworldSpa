<template>
  <form-page-template :page-title="pageTitle" :item="item">
    <application-user-fields :item="item" v-on:submit="onSubmit" v-on:cancel="onCancel">
      <template v-slot:save>Save User</template>
    </application-user-fields>
  </form-page-template>
</template>

<script>
import axios from "axios";
import FormMixin from '../Mixins/FormMixin.vue';
import { mapState, mapGetters, mapMutations, mapActions } from 'vuex'

export default {
  name: "ApplicationUserEdit",
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
      return this.item.userName;
    },
    pageTitle: function () {
      return 'User: ' + this.itemTitle;
    }
  },
  methods: {
    ...mapActions('cachedData', ['setKnownPageName']),
    load: function () {
      let url = '/api/applicationUser/' + this.id + "?context=WebApiElevated&includes=roles";

      axios
        .get(url)
        .then(response => {
          this.item = response.data;

          this.setKnownPageName({ path: this.$route.path, name: this.item.userName});
        })
        .catch(error => {
          console.log(error);
        });
    },
    onCancel(evt) {
      this.$router.push('/applicationUser');
    },
    onSubmit(evt) {
      let url = '/api/applicationUser/' + this.id + '?context=WebApiElevated';

      axios
        .put(url, this.item)
        .then(response => {
          this.item = response.data;

          this.processEditSuccessResponse(response, 'User');

          this.$router.push('/applicationUser');
        })
        .catch(error => {
          this.processEditErrorResponse(error, 'User');
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