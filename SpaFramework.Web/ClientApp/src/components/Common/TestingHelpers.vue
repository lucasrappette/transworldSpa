<template>
  <div>
    <b-row>
      <b-col>
        <b-card title="Test Accounts">
          <p>There is just one test account right now:</p>
          <ul>
            <li>Username: admin</li>
            <li>Password: abcd1234</li>
          </ul>
        </b-card>
      </b-col>
      <b-col>
        <b-card title="Send Global Alert">
          <form-template @submit="onSendGlobalAlert" :hide-cancel="true">
            <p>This sends a global alert to all users.</p>
            <hr />
            <select-list-control v-model="sendGlobalAlert.type" :options="sendGlobalAlertTypeSelectOptions" label="Type" description="The type of alert to send" />
            <text-control v-model="sendGlobalAlert.title" label="Title" description="The title displayed above the alert" />
            <text-control v-model="sendGlobalAlert.message" label="Message" />
            <template v-slot:save>Send Global Alert</template>
          </form-template>
        </b-card>
      </b-col>
    </b-row>
  </div>
</template>

<script>
import axios from "axios";
import { mapActions } from 'vuex'
import FormMixin from '../Mixins/FormMixin.vue';

export default {
  name: "TestingHelpers",
  mixins: [FormMixin],
  props: {
  },
  data() {
    return {
      sendGlobalAlert: {
        type: 'primary',
        title: 'Hello, World',
        message: 'This is an alert that\'s pushed to users via SignalR. Pretty nifty.'
      },
      sendGlobalAlertTypeSelectOptions: ['primary', 'secondary', 'info', 'warning', 'danger']
    }
  },
  computed: {
  },
  methods: {
    onSendGlobalAlert: function () {
      axios
        .post('/api/debug/sendGlobalAlert', this.sendGlobalAlert)
        .then(response => {
          this.showSuccessMessage('Success', 'Global alert sent successfully');
          this.$emit('onClosed');
        })
        .catch(error => {
          this.showErrorMessage('Error', 'Something went wrong sending the global alert.');
        });

    }
  },
  watch: {
  },
  mounted () {
  }
};
</script>