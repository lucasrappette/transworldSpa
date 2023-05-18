<template>
  <form-template @submit="onSubmit" @cancel="onCancel">
    <template v-slot:save><slot name="save"></slot></template>
    <template>
      <b-row class="mt-3">
        <b-col xs="12" sm="6" lg="3">
          <h4>Basic</h4>
          <hr />
          <text-control label="Name" required v-model="item.name" :concurrency-check="item.concurrencyCheck"></text-control>
          <text-control label="Ftp Host" required v-model="item.ftpHost" :concurrency-check="item.concurrencyCheck"></text-control>
          <text-control label="Ftp Username" required v-model="item.ftpUsername" :concurrency-check="item.concurrencyCheck"></text-control>
          <text-control label="Ftp Password" required v-model="item.ftpPassword" :concurrency-check="item.concurrencyCheck"></text-control>
          <text-control label="Ftp Remote Directory" v-model="item.ftpRemoteDir" :concurrency-check="item.concurrencyCheck"></text-control>
        </b-col>
        <!--
        <b-col xs="12" sm="6" lg="3">
          <h4>Address</h4>
          <hr />
          <text-control label="Address Line 1" required v-model="item.addressLineOne" :concurrency-check="item.concurrencyCheck"></text-control>
          <text-control label="Address Line 2" v-model="item.addressLineTwo" :concurrency-check="item.concurrencyCheck"></text-control>
          <text-control label="City" required v-model="item.city" :concurrency-check="item.concurrencyCheck"></text-control>
          <text-control label="State" required v-model="item.state" :concurrency-check="item.concurrencyCheck"></text-control>
          <text-control label="Postal" required v-model="item.postalCode" :concurrency-check="item.concurrencyCheck"></text-control>
        </b-col>
        <b-col xs="12" sm="6" lg="3">
            <h4>Sales Rep</h4>
            <hr />
            <select-list-control label="Sales Rep" required v-model="item.salesRepApplicationUserId" :options="nonNullApplicationUserSelectOptions" :concurrency-check="item.concurrencyCheck"></select-list-control>
            <h4 class="mt-2">Primary Project Manager</h4>
            <hr />
            <select-list-control label="Project Manager" required v-model="item.primaryProjectManagerApplicationUserId" :options="nonNullApplicationUserSelectOptions" :concurrency-check="item.concurrencyCheck"></select-list-control>
            <h4 class="mt-2">Inactive?</h4>
            <hr />
            <checkbox-control label="Inactive" v-model="item.inactive" :concurrency-check="item.concurrencyCheck"></checkbox-control>
          </b-col>
          -->
      </b-row>
    </template>
  </form-template>
</template>

<script>
import { mapState, mapGetters } from 'vuex'

export default {
  name: "DestinationFields",
  props: [
    'item'
  ],
  data() {
    return {
    };
  },
  computed: {
    ...mapState('cachedData', ['applicationUsers']),
    nonNullApplicationUserSelectOptions: function () {
      return this.applicationUsers.selectOptions.filter(x => x.value != null);
    },
  },
  methods: {
    onCancel(evt) {
      this.$emit('cancel');
    },
    onSubmit(evt) {
      this.$emit('submit');
    }
  }
};
</script>