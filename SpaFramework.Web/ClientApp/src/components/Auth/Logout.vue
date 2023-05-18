<template>
  <div>
    <b-container class="py-3">
      <b-row>
        <b-col>
          <page-title title="Log Out" />
          <hr />
          <p>You have been logged out.</p>
          <b-link to="/">Continue</b-link>
        </b-col>
      </b-row>
    </b-container>
  </div>
</template>

<script>
import axios from "axios";
import { mapState, mapGetters, mapActions, mapMutations } from 'vuex'

export default {
  name: "Logout",
  props: {},
  data() {
    return {
    }
  },
  methods: {
    ...mapActions('auth', ['logOut'])
  },
  mounted () {
    this.logOut();
  }
};
</script>