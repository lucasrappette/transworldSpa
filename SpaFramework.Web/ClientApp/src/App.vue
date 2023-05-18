<template>
  <body>

    <b-navbar toggleable="lg" type="dark" variant="dark">
      <b-navbar-brand to="/" id="navbar-brand">
        <b-icon icon="truck" />
        SPA Framework
      </b-navbar-brand>

      <b-navbar-toggle target="nav-collapse"></b-navbar-toggle>

      <b-collapse id="nav-collapse" is-nav>
        <b-navbar-nav>
          <b-nav-item to="/">Home</b-nav-item>
          <b-nav-item v-if="isSuperAdmin" to="/dealer">Dealers</b-nav-item>
          <b-nav-item v-if="isSuperAdmin" to="/export">Exports</b-nav-item>
          <b-nav-item v-if="isSuperAdmin" to="/layout">Layouts</b-nav-item>
          <b-nav-item v-if="isSuperAdmin" to="/destination">Destinations</b-nav-item>
          <b-nav-item v-if="isSuperAdmin" to="/applicationUser">User Management</b-nav-item>
        </b-navbar-nav>

        <b-nav-text v-if="environment != 'prod'" id="dev-site-warning">
          <div class="navbar-warning"><b-icon icon="exclamation-triangle-fill" /> Dev Site</div>
        </b-nav-text>
        <b-popover target="dev-site-warning" triggers="hover" placement="bottom">
          <template #title>Dev Site</template>
          <b>This is not real data.</b> This is a dev/sandbox/test site, not the production site.
        </b-popover>
          
        <!-- Right aligned nav items -->
        <b-navbar-nav class="ml-auto">

          <b-nav-item right to="/page/help"><b-icon icon="question-circle" /></b-nav-item>
          <b-nav-item-dropdown right>
            <template #button-content>
              <b-icon icon="gear" />
            </template>
            <b-dropdown-item href="#" @click="setBodyZoom('90')"><b-icon :icon="bodyZoom == '90' ? 'check-circle' : 'circle'" /> Use Small Display</b-dropdown-item>
            <b-dropdown-item href="#" @click="setBodyZoom('100')"><b-icon :icon="bodyZoom == '100' ? 'check-circle' : 'circle'" /> Use Normal Display</b-dropdown-item>
            <b-dropdown-item href="#" @click="setBodyZoom('110')"><b-icon :icon="bodyZoom == '110' ? 'check-circle' : 'circle'" /> Use Large Display</b-dropdown-item>
            <b-dropdown-divider />
            <b-dropdown-item href="#"><b-icon icon="square" /> Some Other Preference</b-dropdown-item>
          </b-nav-item-dropdown>

          <b-nav-item-dropdown right v-if="isSuperAdmin">
            <template #button-content>
              <b-icon icon="cone-striped" />
            </template>
            <b-dropdown-item href="/healthchecks-ui" target="_blank">Health Checks</b-dropdown-item>
            <b-dropdown-item href="/swagger" target="_blank">Swagger</b-dropdown-item>
            <b-dropdown-item href="/documentation" target="_blank">Data Documentation</b-dropdown-item>
          </b-nav-item-dropdown>

          <b-nav-item v-if="showTestingHelpers" right v-b-modal.modalTestingHelpers><b-icon icon="easel" />Testing Helpers</b-nav-item>

          <b-nav-item-dropdown right>
            <template slot="button-content">
              {{isAuthenticated ? authenticatedUsername : 'Not Logged In'}}
            </template>
            <b-dropdown-item to="/changePassword" v-if="isAuthenticated">Change Password</b-dropdown-item>
            <b-dropdown-item to="/login" v-if="!isAuthenticated">Log In</b-dropdown-item>
            <b-dropdown-item to="/logout" v-if="isAuthenticated">Log Out</b-dropdown-item>
          </b-nav-item-dropdown>

        </b-navbar-nav>
      </b-collapse>
    </b-navbar>

    <b-modal id="modalTestingHelpers" size="xl" title="Testing Helpers">
      <testing-helpers @onClosed="$bvModal.hide('modalTestingHelpers')" />
    </b-modal>
    
    <b-container fluid>
      <b-row>
        <main role="main" class="w-100">
          <!-- This key refreshes the component whenever the path changes. https://stackoverflow.com/questions/43461882/update-vuejs-component-on-route-change -->
          <router-view :key="routerViewKey"></router-view>
          <!-- <router-view></router-view> -->
        </main>
      </b-row>
    </b-container>
  </body>
</template>

<script>
import axios from "axios";
import * as signalR from '@microsoft/signalr';
import { mapState, mapGetters, mapActions, mapMutations } from 'vuex'

export default {
  name: 'app',
  data() {
    return {
      applicationUser: null,
      bodyZoom: '100',
      connection: null
    };
  },
  computed: {
    ...mapState('cachedData', ['agentStates']),
    ...mapGetters('auth', ['isAuthenticated', 'authenticatedUsername', 'authenticatedUserId', 'isSuperAdmin', 'isProjectManager', 'isProjectViewer', 'isContentManager', 'isJwtExpired']),
    routerViewKey: function () {
      return this.$route.fullPath;
    },
    environment: function () {
      return process.env.VUE_APP_ENVIRONMENT;
    },
    showTestingHelpers: function () {
      return this.environment != 'prod';
    },
  },
  methods: {
    setBodyZoom: function (bodyZoom) {
      this.bodyZoom = bodyZoom;
      document.body.className = 'zoom-' + bodyZoom;
    },
    showToast: function (text, options) {
      this.$bvToast.toast(text, options);
    },
    init: function () {
      this.$store.dispatch('cachedData/loadCachedData');

      this.$store.subscribe((mutation, state) => {
        if (mutation.type == 'auth/PROCESS_LOG_IN') {
          this.startConnection();
        } else if (mutation.type == 'auth/PROCESS_LOG_OUT') {
          this.stopConnection();
        }
      })

      if (this.$store.state.auth.jwt) {
        this.startConnection();
      }
      if (this.isJwtExpired) {
        this.showToast('Your session may have expired. You may need to log out and back in.', {
          title: 'Session Expired',
          variant: 'warning'
        });
      }
    },
    stopConnection: async function () {
      if (!this.connection)
        return;

      console.log('Stopping SignalR connection');

      this.connection.stop();
      this.connection = null;
    },
    startConnection: async function () {
      if (this.connection) {
        return;
      }

      console.log('Starting SignalR connection');

      this.connection = new signalR.HubConnectionBuilder()
          .configureLogging(signalR.LogLevel.Information)
          .withUrl("/notification-hub", { accessTokenFactory: () => this.$store.state.auth.jwt })
          .withAutomaticReconnect()
          .build();

      // register callbacks
      this.connection.on('OnAlert', (data) => {
        this.showToast(data.message, {
          title: data.title,
          variant: data.type
        });
      });

      try {
          await this.connection.start();
          console.log("SignalR connected");
      } catch (err) {
          console.log(err);
          setTimeout(this.startConnection, 5000);
      }
    }
  },
  created() {
    this.init();
    //this.checkForAuth();
  }
}
</script>
