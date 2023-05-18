import Vue from 'vue'
import VueRouter from 'vue-router'
import BootstrapVue from 'bootstrap-vue'
import App from './App.vue'
import Vuex from 'vuex'
import VueMask from 'v-mask'

import GSignInButton from 'vue-google-signin-button'
import FBSignInButton from 'vue-facebook-signin-button'

import './custom.scss'

import { store } from './store/store';

// Bootstrap Icons
import { BootstrapVueIcons } from 'bootstrap-vue'

// Mixins
import FormMixin from './components/Mixins/FormMixin.vue'
import FormattingMixin from './components/Mixins/FormattingMixin.vue'

// Form Controls
import TextControl from './components/Controls/TextControl.vue'
import TextareaControl from './components/Controls/TextareaControl.vue'
import SelectListControl from './components/Controls/SelectListControl.vue'
import CheckboxControl from './components/Controls/CheckboxControl.vue'
import RadioButtonListControl from './components/Controls/RadioButtonListControl.vue'
import CheckBoxListControl from './components/Controls/CheckBoxListControl.vue'
import DatePickerControl from './components/Controls/DatePickerControl.vue'
import FileUploadControl from './components/Controls/FileUploadControl.vue'
import StaticControl from './components/Controls/StaticControl.vue'

// Widgets
import PageLoading from './components/Common/PageLoading.vue'
import PageTitle from './components/Common/PageTitle.vue'
import FormPageTemplate from './components/Common/FormPageTemplate.vue'
import ListPageTemplate from './components/Common/ListPageTemplate.vue'
import FormTemplate from './components/Common/FormTemplate.vue'
import FilteredTable from './components/Common/FilteredTable.vue'
import PivotTable from './components/Common/PivotTable.vue'
import ContentBlock from './components/Common/ContentBlock.vue'
import Steps from './components/Common/Steps.vue'
import BreadcrumbBar from './components/Common/BreadcrumbBar.vue'

import AuthForm from './components/Auth/AuthForm.vue'

// Pages
import Home from './components/Home/Home.vue'
import TestingHelpers from './components/Common/TestingHelpers.vue'
import Login from './components/Auth/Login.vue'
import Logout from './components/Auth/Logout.vue'
import ChangePassword from './components/Auth/ChangePassword.vue'
import ApplicationUserList from './components/ApplicationUser/List.vue'
import ApplicationUserFields from './components/ApplicationUser/Fields.vue'
import ApplicationUserEdit from './components/ApplicationUser/Edit.vue'
import ApplicationUserAdd from './components/ApplicationUser/Add.vue'
import ContentBlockList from './components/ContentBlock/List.vue'
import ContentBlockFields from './components/ContentBlock/Fields.vue'
import ContentBlockAdd from './components/ContentBlock/Add.vue'
import ContentBlockEdit from './components/ContentBlock/Edit.vue'
import ContentBlockView from './components/ContentBlock/View.vue'
import DealerList from './components/Dealer/List.vue'
import DealerFields from './components/Dealer/Fields.vue'
import DealerAdd from './components/Dealer/Add.vue'
import DealerEdit from './components/Dealer/Edit.vue'

import DestinationList from './components/Destination/List.vue'
import DestinationFields from './components/Destination/Fields.vue'
import DestinationAdd from './components/Destination/Add.vue'
import DestinationEdit from './components/Destination/Edit.vue'
import JobList from './components/Job/List.vue'

Vue.use(VueRouter);
Vue.use(BootstrapVue);
Vue.use(BootstrapVueIcons);
Vue.use(require('vue-moment'));
Vue.use(Vuex);
Vue.use(VueMask);

Vue.use(FormMixin);
Vue.use(FormattingMixin);

Vue.use(GSignInButton);
Vue.use(FBSignInButton);

Vue.component('testing-helpers', TestingHelpers);

Vue.component('page-loading', PageLoading);
Vue.component('page-title', PageTitle);
Vue.component('form-page-template', FormPageTemplate);
Vue.component('list-page-template', ListPageTemplate);
Vue.component('form-template', FormTemplate);
Vue.component('filtered-table', FilteredTable);
Vue.component('pivot-table', PivotTable);
Vue.component('content-block', ContentBlock);
Vue.component('steps', Steps);
Vue.component('breadcrumb-bar', BreadcrumbBar);

Vue.component('auth-form', AuthForm);

Vue.component('text-control', TextControl);
Vue.component('textarea-control', TextareaControl);
Vue.component('select-list-control', SelectListControl);
Vue.component('checkbox-control', CheckboxControl);
Vue.component('radio-button-list-control', RadioButtonListControl);
Vue.component('check-box-list-control', CheckBoxListControl);
Vue.component('date-picker-control', DatePickerControl);
Vue.component('file-upload-control', FileUploadControl);
Vue.component('static-control', StaticControl);

Vue.component('application-user-fields', ApplicationUserFields);
Vue.component('content-block-fields', ContentBlockFields);
Vue.component('dealer-fields', DealerFields);
Vue.component('destination-fields', DestinationFields);


Vue.config.productionTip = false;

const routes = [
  { path: '/', component: Home, meta: { public: true } },
  { path: '/login', component: Login, meta: { public: true } },
  { path: '/logout', component: Logout, meta: { public: true } },
  { path: '/changePassword', component: ChangePassword },
  { path: '/applicationUser', component: ApplicationUserList },
  { path: '/applicationUser/add', component: ApplicationUserAdd },
  { path: '/applicationUser/:id', component: ApplicationUserEdit, props: true },
  { path: '/contentBlock', component: ContentBlockList },
  { path: '/contentBlock/add', component: ContentBlockAdd },
  { path: '/contentBlock/:id', component: ContentBlockEdit, props: true },
  { path: '/dealer', component: DealerList },
  { path: '/dealer/add', component: DealerAdd },
  { path: '/dealer/:id', component: DealerEdit, props: route => ({ id: route.params.id })},

  { path: '/destination', component: DestinationList },
  { path: '/destination/add', component: DestinationAdd },
  { path: '/destination/:id', component: DestinationEdit, props: route => ({ id: route.params.id })},
  { path: '/page/:slug', component: ContentBlockView, props: true, meta: { public: true } },
  { path: '/job', component: JobList },
];

export const router = new VueRouter({
  routes // short for `routes: routes`
});

router.beforeEach((to, from, next) => {
  store.dispatch('auth/checkForAuth');

  if (store.state.auth.username != null) {
    // Allow authenticated users through
    next();
  } else if (to.matched.some(record => record.meta.public)) {
    // Allow all users to hit any page marked as public
    next();
  } else {
    // Redirect to the login page, passing the requested page as the redirect param
    next({
      path: '/login',
      query: { redirect: to.fullPath }
    });
  }
});

router.afterEach((to, from) => {
  // gtag('config', window.GA_TRACKING_ID, {
  //   page_path: to.fullPath,
  //   app_name: 'Newer Brain',
  //   send_page_view: true,
  // });
});

new Vue({
  store,
  router,
  render: h => h(App),
  data: {
    // authenticatedUser: {
    //   username: null,
    //   id: null,
    //   roles: []
    // }
  }
}).$mount('body');

