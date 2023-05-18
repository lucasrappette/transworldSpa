<template>
  <form-page-template :page-title="pageTitle" :item="item">
    <application-user-fields :item="item" v-on:submit="onSubmit" v-on:cancel="onCancel">
      <template v-slot:save>Add User</template>
    </application-user-fields>
  </form-page-template>
</template>

<script>
import axios from "axios";
import FormMixin from '../Mixins/FormMixin.vue';

export default {
  name: "UserAdd",
  mixins: [FormMixin],
  props: [],
  data() {
    return {
      item: {
        firstName: null,
        lastName: null,
        agentId: null,
        email: null,
        roles: [],
      }
    };
  },
  computed: {
    itemTitle: function () {
      if (this.item.userName)
        return this.item.userName;
      else
        return null;
    },
    pageTitle: function () {
      return 'New User' + (this.itemTitle && this.itemTitle.length > 0 ? ': ' + this.itemTitle : '');
    }
  },
  methods: {
    load: function () {
    },
    onCancel(evt) {
      this.$router.push('/applicationUser');
    },
    onSubmit(evt) {
      let url = '/api/applicationUser?context=WebApiElevated';

      axios
        .post(url, this.item)
        .then(response => {
          this.item = response.data;

          this.processAddSuccessResponse(response, 'User');

          this.$router.push('/applicationUser/' + this.item.id);
        })
        .catch(error => {
          this.processAddErrorResponse(error, 'User');
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