import Vue from 'vue';
import Vuex from 'vuex';

import CachedDataModule from './module.cachedData'
import AuthModule from './module.auth'

Vue.use(Vuex);

export const store = new Vuex.Store({
  modules: {
    cachedData: CachedDataModule,
    auth: AuthModule,
  }
})