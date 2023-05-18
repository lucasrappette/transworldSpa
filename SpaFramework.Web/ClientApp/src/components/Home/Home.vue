<template>
  <div>
    <b-container fluid class="py-3">
      <b-row>
        <b-col>
          <b-card-group deck>
            <b-card no-body v-if="!isAuthenticated">
              <b-jumbotron header="SPA Framework" lead="ASP.NET Core + VueJS" class="text-center">
                <p>There's a readme file with some additional information.</p>
              </b-jumbotron>
              <div class="text-center my-3">
                <b-button to="/login" variant="primary">Log In to Get Started</b-button>
              </div>
            </b-card>
            <b-card v-if="isAuthenticated">
              <template #header>
                <h4 class="mb-0">Dashboard</h4>
              </template>
              <b-card-text>
                <content-block slug="dashboard" />
              </b-card-text>
            </b-card>
          </b-card-group>
        </b-col>
      </b-row>
    </b-container>
  </div>
</template>

<script>
import axios from "axios";
import { mapState, mapGetters, mapMutations } from 'vuex'

export default {
  name: "Home",
  props: {},
  data() {
    return {
    }
  },
  computed: {
    ...mapGetters('auth', ['isAuthenticated'])
  },
  methods: {

  },
  watch: {
  },
  created() {
  },
  async mounted () {
  }
};
</script>