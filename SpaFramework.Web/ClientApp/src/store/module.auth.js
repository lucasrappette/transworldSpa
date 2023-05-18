import Vue from 'vue';
import Vuex from 'vuex';
import axios from "axios";

export default {
  namespaced: true,
  state: () => ({
    username: null,
    id: null,
    roles: null,
    jwt: null
  }),
  mutations: {
    PROCESS_LOG_IN (state, authData) {
      console.log('logging in');
      
      state.username = authData.userName;
      state.id = authData.id;
      state.roles = authData.roles;
      state.jwt = authData.jwtToken;

      localStorage.setItem('jwt', state.jwt);
      localStorage.setItem('username', state.username);
      localStorage.setItem('id', state.id);
      localStorage.setItem('roles', JSON.stringify(state.roles));

      axios.defaults.headers.common['Authorization'] = "Bearer " + state.jwt;
    },
    PROCESS_LOG_OUT (state) {
      state.username = null;
      state.id = null;
      state.roles = null;
      state.jwt = null;

      localStorage.removeItem('jwt');
      localStorage.removeItem('username');
      localStorage.removeItem('id');
      localStorage.removeItem('roles');

      axios.defaults.headers.common['Authorization'] = null;
    }
  },
  actions: {
    checkForAuth({ state, commit, rootState }) {
      if (state.username != null)
        return;

      if (!localStorage.getItem('jwt'))
        return;

      commit('PROCESS_LOG_IN', {
        userName: localStorage.getItem('username'),
        id: localStorage.getItem('id'),
        roles: JSON.parse(localStorage.getItem('roles')),
        jwtToken: localStorage.getItem('jwt')
      });
    },
    makeRequest({ state, commit, rootState }, data) {
      return new Promise((resolve, reject) => {
        axios.post(data.url, data.payload)
        .then(response => {
          commit('PROCESS_LOG_IN', response.data);
          resolve();
        })
        .catch(error => {
          let errMsg = 'An unexpected error occurred. Please try again.';

          if (error && error.response && error.response.data && error.response.data.Details) {
            let parts = [];
            parts.push(error.response.data.Details);
  
            if (error.response.data.SubErrors)
              parts.push(...error.response.data.SubErrors);
  
            errMsg = parts.join('<br />');
          }

          reject(new Error(errMsg));
        });
      });      
    },
    logIn({ commit, dispatch }, payload) {
      return dispatch('makeRequest', { url: '/api/account/login', payload: payload });
    },
    logOut({ state, commit, rootState }) {
      commit('PROCESS_LOG_OUT');
    },
    signUp({ commit, dispatch }, payload) {
      return dispatch('makeRequest', { url: '/api/account/register', payload: payload });
    },
    requestPasswordReset({ commit, dispatch }, payload) {
      return dispatch('makeRequest', { url: '/api/account/requestPasswordReset', payload: payload });
    },
    resetPassword({ commit, dispatch }, payload) {
      return dispatch('makeRequest', { url: '/api/account/resetPassword', payload: payload });
    },
    changePassword({ commit, dispatch }, payload) {
      return dispatch('makeRequest', { url: '/api/account/changePassword', payload: payload });
    },
  },
  getters: {
    isAuthenticated: state => { return state.username != null; },

    isSuperAdmin: state => { return state.username != null && state.roles != null && state.roles.includes('SuperAdmin'); },
    isProjectManager: state => { return state.username != null && state.roles != null && state.roles.includes('ProjectManager'); },
    isProjectViewer: (state) => { return state.username != null && state.roles != null && state.roles.includes('ProjectViewer'); },
    isContentManager: (state) => { return state.username != null && state.roles != null && state.roles.includes('ContentManager'); },

    authenticatedUsername: state => { return state.username; },
    authenticatedUserId: state => { return state.id; },

    parsedJwt: state => {
      let token = state.jwt;
      if (!token)
        return null;

      let base64Url = token.split('.')[1];
      let base64 = base64Url.replace(/-/g, '+').replace(/_/g, '/');
      let jsonPayload = decodeURIComponent(window.atob(base64).split('').map(function(c) {
          return '%' + ('00' + c.charCodeAt(0).toString(16)).slice(-2);
      }).join(''));

      return JSON.parse(jsonPayload);
    },
    isJwtExpired: (state, getters) => {
      let parsedJwt = getters.parsedJwt;
      if (!parsedJwt)
        return false;

      let exp = new Date(parsedJwt.exp * 1000);
      let now = new Date();

      return now > exp;
    }
  }
}