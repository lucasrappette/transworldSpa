<template>
  <div>
    <form-template @submit="onSubmit" @cancel="onCancel">
      <template v-slot:save><slot name="save"></slot></template>
      <template>
        <b-alert v-if="isUserSuperAdmin" show variant="warning">
          This user is a super admin. Be extremely careful when making changes.
        </b-alert>
        <b-alert v-if="item.id && !item.hasPassword" show variant="warning">
          This user does not have a password and cannot log in.
        </b-alert>
        <b-row class="mt-3">
          <b-col xs="12" sm="6" lg="3">
            <h4>Account</h4>
            <hr />
            <text-control label="User ID" v-model="item.id" disabled></text-control>
            <text-control label="User Name" v-model="item.userName" :concurrency-check="item.concurrencyCheck"></text-control>
            <text-control label="Email Address" v-model="item.email" :concurrency-check="item.concurrencyCheck"></text-control>
            <hr />
          </b-col>
          <b-col xs="12" sm="6" lg="3">
            <h4>Basic</h4>
            <hr />
            <text-control label="First Name" v-model="item.firstName" :concurrency-check="item.concurrencyCheck"></text-control>
            <text-control label="Last Name" v-model="item.lastName" :concurrency-check="item.concurrencyCheck"></text-control>
            <hr />
            <div v-if="item.id">
              <b-button @click="showPasswordModal">{{ item.hasPassword ? 'Change Password' : 'Create Password' }}</b-button>
            </div>
            <div v-if="!item.id">
              <p class="small">Once the user has been created, you'll be able to set a password for the user.</p>
            </div>
          </b-col>
          <b-col xs="12" sm="6" lg="3">
            <h4>Roles</h4>
            <hr />
            <check-box-list-control label="Roles" v-model="item.roles" id-field="applicationRoleId" :options="nonNullApplicationRoleSelectOptions" name="applicationRoles" :concurrency-check="item.concurrencyCheck"></check-box-list-control>
          </b-col>
        </b-row>

      </template>
    </form-template>

    <b-modal id="passwordModal" header-bg-variant="dark" header-text-variant="light" title="Set Password" :hide-footer="true">
      <p>This will change the user's password.</p>
      <form-template @submit="onSubmitSetPassword" @cancel="onCancelSetPassword">
        <template v-slot:save>Set Password</template>
        <text-control label="New Password" v-model="changePasswordNewPassword" type="password" required description="We recommend a strong password that's at least 12 characters." />
        <text-control label="Confirm New Password" v-model="changePasswordConfirm" type="password" required :custom-validation="validateChangePasswordConfirm" />
      </form-template>
    </b-modal>
    
  </div>
</template>

<script>
import axios from "axios";
import { mapState, mapGetters } from 'vuex'
import TextControl from '../Controls/TextControl.vue';
import FormTemplate from '../Common/FormTemplate.vue';
import FormMixin from '../Mixins/FormMixin.vue';

export default {
  name: "ApplicationUserFields",
  mixins: [FormMixin],
  props: [
      'item'
  ],
  data() {
    return {
      changePasswordNewPassword: null,
      changePasswordConfirm: null,
    };
  },
  computed: {
    ...mapState('cachedData', ['applicationRoles']),
    nonNullApplicationRoleSelectOptions: function () {
      return this.applicationRoles.selectOptions.filter(x => x.value != null);
    },
    isUserSuperAdmin: function () {
      return this.isUserInRole('SuperAdmin');
    }
  },
  methods: {
    onCancel(evt) {
      this.$emit('cancel');
    },
    onSubmit(evt) {
      this.$emit('submit');
    },
    isUserInRole(roleName) {
      if (!this.item || !this.item.roles || this.item.roles.length == 0)
        return false;

      if (!this.applicationRoles || this.applicationRoles.values.length == 0)
        return false;

      let roleId = this.applicationRoles.values.find(x => x.name == roleName).id;
      return this.item.roles.find(x => x.applicationRoleId == roleId) ? true : false;
    },
    showPasswordModal() {
      this.$bvModal.show('passwordModal');
    },
    onCancelSetPassword() {
      this.$bvModal.hide('passwordModal');
    },
    onSubmitSetPassword() {
      let url = '/api/account/changeUserPassword';

      axios
        .post(url, {
          applicationUserId: this.item.id,
          newPassword: this.changePasswordNewPassword
        })
        .then(response => {
          this.$bvModal.hide('passwordModal');
          this.item.hasPassword = true;

          this.changePasswordNewPassword = null;
          this.changePasswordConfirm = null;

          this.showSuccessMessage('Password Changed', 'The user\'s password was successfully changed.');
        })
        .catch(error => {
          console.log(error);
        });
    },
    validateChangePasswordConfirm: function () {
      if (this.changePasswordNewPassword != this.changePasswordConfirm)
        return 'Passwords do not match';
      else
        return true;
    },
  },
  created () {
    this.$store.dispatch('cachedData/loadApplicationRoles');
  }
};
</script>