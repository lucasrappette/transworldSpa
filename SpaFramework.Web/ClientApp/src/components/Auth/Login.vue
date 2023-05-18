<template>
  <div>
    <b-container class="py-3">
      <b-row>
        <b-col>
          <page-title v-if="isAuthenticated" title="Log In" />
          <div v-if="isAuthenticated">
            <hr />
            <p>You are already logged in as {{authenticatedUsername}}.</p>
          </div>
          <auth-form v-if="!isAuthenticated" @authenticated="onAuthenticated" page="login" class="mt-3" />
        </b-col>
      </b-row>
    </b-container>
  </div>
</template>

<script>
import axios from "axios";
import { mapState, mapGetters, mapMutations } from 'vuex'

export default {
  name: "Login",
  props: {},
  data() {
    return {
    }
  },
  computed: {
    ...mapGetters('auth', ['isAuthenticated', 'authenticatedUsername']),
  },
  methods: {
    onAuthenticated: function () {
      if (this.$route.query.redirect)
        this.$router.push(this.$route.query.redirect);
      else
        this.$router.push('/');
    }
  },
  mounted () {
  }
};
</script>