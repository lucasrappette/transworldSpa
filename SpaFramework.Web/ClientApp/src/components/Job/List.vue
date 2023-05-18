<template>
  <list-page-template page-title="Jobs">
    <filtered-table :settings="tableSettings">
    </filtered-table>
  </list-page-template>
</template>

<script>
import axios from "axios";

export default {
  name: "JobList",
  props: {},
  data() {
    let base = this;
    return {
      tableSettings: {
        endpoint: '/api/job',
        showNewButton: false,
        columns: [
          {
            key: 'id',
            name: 'ID',
            visible: false,
            sortable: true,
            type: 'guid'
          },
          {
            key: 'name',
            name: 'Name',
            visible: true,
            sortable: true,
            type: 'text'
          },
          {
            key: 'created',
            name: 'Created',
            visible: true,
            sortable: true,
            type: 'datetime'
          },
          {
            key: 'started',
            name: 'Started',
            visible: true,
            sortable: true,
            type: 'datetime'
          },
          {
            key: 'updated',
            name: 'Updated',
            visible: true,
            sortable: true,
            type: 'datetime'
          },
          {
            key: 'ended',
            name: 'Ended',
            visible: true,
            sortable: true,
            type: 'datetime'
          },
          {
            key: 'expectedCount',
            name: 'Expected Count',
            visible: true,
            sortable: false,
            type: 'number'
          },
          {
            key: 'successCount',
            name: 'Success Count',
            visible: true,
            sortable: false,
            type: 'number'
          },
          {
            key: 'failureCount',
            name: 'Failure Count',
            visible: true,
            sortable: false,
            type: 'number'
          },
          {
            key: 'itemType',
            name: 'Item Type',
            visible: false,
            sortable: false,
            type: 'text'
          }
        ],
        getDefaultFilter: function () {
          return '';
        },
        defaultSortColumn: 'created',
        defaultSortOrder: 'desc',
        defaultLimit: 100
      }
    }
  },
  methods: {
    // onRowClicked: function (item, context) {
    //   this.$router.push('/job/' + item.id);
    // }
  },
  computed: {
  },
  mounted () {
  }
};
</script>