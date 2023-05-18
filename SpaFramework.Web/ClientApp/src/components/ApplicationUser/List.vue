<template>
  <list-page-template page-title="Users">
    <filtered-table :settings="tableSettings" @rowClicked="onRowClicked" @newClicked="onNewClicked">
    </filtered-table>
  </list-page-template>
</template>

<script>
import axios from "axios";

export default {
  name: "ApplicationUserList",
  props: {},
  data() {
    let base = this;
    return {
      tableSettings: {
        endpoint: '/api/applicationUser',
        showNewButton: true,
        columns: [
          {
            key: 'id',
            name: 'ID',
            visible: false,
            sortable: true,
            type: 'guid'
          },
          {
            key: 'email',
            name: 'Email Address',
            visible: true,
            sortable: true,
            type: 'text'
          },
          {
            key: 'userName',
            name: 'User Name',
            visible: true,
            sortable: true,
            type: 'text'
          },
          {
            key: 'firstName',
            name: 'First Name',
            visible: true,
            sortable: true,
            type: 'text'
          },
          {
            key: 'lastName',
            name: 'Last Name',
            visible: true,
            sortable: true,
            type: 'text'
          },
          {
            key: 'roles',
            name: 'Roles',
            visible: true,
            sortable: false,
            type: 'multiselect',
            filterSelector: 'roleId',
            displaySelector: 'applicationRole.name',
            selectOptions: [],
            selectOptionsSource: { storeModule: 'cachedData', storeAction: 'loadApplicationRoles', storeGetter: 'applicationRoles' }
          }
        ],
        getDefaultFilter: function () {
          return '';
        },
        includes: ['roles', 'roles.applicationRole']
      }
    }
  },
  computed: {
  },
  methods: {
    onRowClicked: function (item, context) {
      this.$router.push('/applicationUser/' + item.id);
    },
    onNewClicked: function (filters) {
      this.$router.push('/applicationUser/add');
    }
  },
  mounted () {
  }
};
</script>