<template>
  <div>
    <b-row>
      <b-col md="3"></b-col>
      <b-col md="6" class="text-center">
        <div v-if="authPage == 'create'">
          <h2>{{signUpHeader}}</h2>
          <hr />
          <b-alert v-if="signUpError" show variant="danger"><span v-html="signUpError"></span></b-alert>
          <b-form @submit.prevent="onFormSignUp">
            <text-control form-name="signUp" label="Email Address" v-model="signUpEmail" type="email" required />
            <text-control form-name="signUp" label="New Password" v-model="signUpPassword" type="password" required description="We recommend a strong password that's at least 12 characters." />
            <text-control form-name="signUp" label="Confirm New Password" v-model="signUpConfirm" type="password" required :custom-validation="validateSignUpConfirm" />
            <b-button type="submit" variant="primary" class="mt-3">Sign Up</b-button>
          </b-form>
          <div class="mt-3">
            <a href="#" @click.prevent="authPage = 'login'">Already have an account? Log in.</a>
          </div>
          <div>
            <a href="#" @click.prevent="authPage = 'requestReset'">Forgot your password? Reset it.</a>
          </div>          
        </div>
        <div v-if="authPage == 'login'">
          <h2>{{logInHeader}}</h2>
          <hr />
          <b-alert v-if="logInError" show variant="danger"><span v-html="logInError"></span></b-alert>
          <b-form @submit.prevent="onFormLogIn">
            <text-control form-name="logIn" label="Username" v-model="logInUserName" required />
            <text-control form-name="logIn" label="Password" v-model="logInPassword" type="password" required />
            <b-button type="submit" variant="primary" class="mt-3">Log In</b-button>
          </b-form>
          <!-- <div class="mt-3">
            <a href="#" @click.prevent="authPage = 'create'">Don't have an account? Sign up.</a>
          </div>
          <div>
            <a href="#" @click.prevent="authPage = 'requestReset'">Forgot your password? Reset it.</a>
          </div>           -->
        </div>
        <div v-if="authPage == 'requestReset'">
          <h2>{{requestPasswordResetHeader}}</h2>
          <hr />
          <b-alert v-if="requestPasswordResetError" show variant="danger"><span v-html="requestPasswordResetError"></span></b-alert>
          <b-alert v-if="requestPasswordResetDone" show variant="success">An email has been sent to the address you provided with a link to reset your password.</b-alert>
          <b-form @submit.prevent="onFormRequestPasswordReset">
            <text-control form-name="requestReset" label="Email Address" v-model="requestPasswordResetEmail" type="email" required />
            <b-button type="submit" variant="primary" class="mt-3">Reset Password</b-button>
          </b-form>
          <div class="mt-3">
            <a href="#" @click.prevent="authPage = 'login'">Already have an account? Log in.</a>
          </div>
          <div>
            <a href="#" @click.prevent="authPage = 'create'">Don't have an account? Sign up.</a>
          </div>
        </div>
        <div v-if="authPage == 'reset'">
          <h2>{{resetPasswordHeader}}</h2>
          <hr />
          <b-alert v-if="resetPasswordError" show variant="danger"><span v-html="resetPasswordError"></span></b-alert>
          <b-form @submit.prevent="onFormResetPassword">
            <text-control form-name="resetPassword" label="Email Address" v-model="resetPasswordEmail" type="email" required />
            <text-control form-name="resetPassword" label="New Password" v-model="resetPasswordPassword" type="password" required description="We recommend a strong password that's at least 12 characters." />
            <text-control form-name="resetPassword" label="Confirm New Password" v-model="resetPasswordConfirm" type="password" required :custom-validation="validateResetPasswordConfirm" />
            <b-button type="submit" variant="primary" class="mt-3">Reset Password</b-button>
          </b-form>
          <div class="mt-3">
            <a href="#" @click.prevent="authPage = 'login'">Already have an account? Log in.</a>
          </div>
          <div>
            <a href="#" @click.prevent="authPage = 'create'">Don't have an account? Sign up.</a>
          </div>
        </div>
        <div v-if="authPage == 'change'">
          <h2>{{changePasswordHeader}}</h2>
          <hr />
          <b-alert v-if="changePasswordError" show variant="danger"><span v-html="changePasswordError"></span></b-alert>
          <b-form @submit.prevent="onFormChangePassword">
            <text-control form-name="changePassword" label="Current Password" v-model="changePasswordOldPassword" type="password" required />
            <text-control form-name="changePassword" label="New Password" v-model="changePasswordNewPassword" type="password" required min-length="8" />
            <text-control form-name="changePassword" label="Confirm New Password" v-model="changePasswordConfirm" type="password" required :custom-validation="validateChangePasswordConfirm" />
            <b-button type="submit" variant="primary" class="mt-3">Change Password</b-button>
          </b-form>
        </div>
      </b-col>
      <b-col md="3"></b-col>
    </b-row>
  </div>
</template>

<script>
import axios from "axios";
import FormMixin from '../Mixins/FormMixin.vue';
import { mapActions } from 'vuex'

export default {
  name: "AuthForm",
  mixins: [FormMixin],
  props: {
    page: {
      default: 'create'
    },
    signUpHeader: {
      default: 'Sign Up'
    },
    logInHeader: {
      default: 'Log In'
    },
    requestPasswordResetHeader: {
      default: 'Request Password Reset'
    },
    resetPasswordHeader: {
      default: 'Reset Password'
    },
    changePasswordHeader: {
      default: 'Change Password'
    }
  },
  data() {
    return {
      authPage: null,

      externalLogInError: null,

      // Sign up form
      signUpEmail: null,
      signUpPassword: null,
      signUpConfirm: null,
      signUpError: null,

      // Log in form
      logInUserName: null,
      logInPassword: null,
      logInError: null,

      // Reset password request form
      requestPasswordResetEmail: null,
      requestPasswordResetDone: false,
      requestPasswordResetError: null,

      // Reset password form
      resetPasswordEmail: null,
      resetPasswordToken: null,
      resetPasswordPassword: null,
      resetPasswordConfirm: null,
      resetPasswordError: null,

      // Change password form
      changePasswordError: null,
      changePasswordOldPassword: null,
      changePasswordNewPassword: null,
      changePasswordConfirm: null,
    }
  },
  computed: {
  },
  methods: {
    ...mapActions('auth', ['logIn', 'logOut', 'signUp', 'requestPasswordReset', 'resetPassword', 'changePassword']),
    validateSignUpConfirm: function () {
      if (this.signUpPassword != this.signUpConfirm)
        return 'Passwords do not match';
      else
        return true;
    },
    validateResetPasswordConfirm: function () {
      if (this.resetPasswordPassword != this.resetPasswordConfirm)
        return 'Passwords do not match';
      else
        return true;
    },
    validateChangePasswordConfirm: function () {
      if (this.changePasswordNewPassword != this.changePasswordConfirm)
        return 'Passwords do not match';
      else
        return true;
    },
    onFormSignUp: function () {
      if (!this.isFormValid('signUp'))
        return;

      this.signUp({
        "email": this.signUpEmail,
        "password": this.signUpPassword
      }).then(() => {
        this.$emit('authenticated');
      }).catch(error => {
        this.signUpError = error;
      });
    },
    onFormLogIn: function () {
      if (!this.isFormValid('logIn'))
        return;

      this.logIn({
        "userName": this.logInUserName,
        "password": this.logInPassword
      }).then(() => {
        this.$emit('authenticated');

        this.$router.push('/');
      }).catch(error => {
        this.logInError = error;
      });
    },
    onFormRequestPasswordReset: function () {
      if (!this.isFormValid('requestReset'))
        return;

      this.requestPasswordReset({
        "email": this.requestPasswordResetEmail
      }).then(() => {
        this.requestPasswordResetDone = true;
      }).catch(error => {
        this.requestPasswordResetError = error;
      });

    },
    onFormResetPassword: function () {
      if (!this.isFormValid('resetPassword'))
        return;

      this.resetPassword({
        "email": this.resetPasswordEmail,
        "token": this.resetPasswordToken,
        "password": this.resetPasswordPassword
      }).then(() => {
        this.$emit('authenticated');
      }).catch(error => {
        this.resetPasswordError = error;
      });
    },
    onFormChangePassword: function () {
      if (!this.isFormValid('changePassword'))
        return;

      this.changePassword({
        "oldPassword": this.changePasswordOldPassword,
        "newPassword": this.changePasswordNewPassword,
      }).then(() => {
        this.$emit('authenticated');

        this.showSuccessMessage('Password Changed', 'Your password was successfully changed.');

        this.$router.push('/');
      }).catch(error => {
        this.changePasswordError = error;
      });
    }
  },
  watch: {
  },
  mounted () {
    if (this.$route.query.token) {
      this.resetPasswordToken = this.$route.query.token;
      this.authPage = 'reset';
    } else if (this.defaultAuthPage) {
      this.authPage = this.defaultAuthPage;
    } else {
      this.authPage = this.page;
    }
  }
};
</script>