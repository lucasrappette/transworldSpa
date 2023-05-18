<template>
  <list-page-template page-title="Destinations">
    <filtered-table :settings="tableSettings" @rowClicked="onRowClicked" @newClicked="onNewClicked">
    </filtered-table>
  </list-page-template>
</template>

<script>
import axios from "axios";

export default {
  name: "DestinationList",
  props: {},
  data() {
    let base = this;
    return {
      tableSettings: {
        endpoint: '/api/destination',
        showNewButton: true,
        defaultLimit: 100,
        columns: [
          {
            key: 'destinationId',
            name: 'ID',
            visible: false,
            sortable: true,
            type: 'number'
          },
          {
            key: 'name',
            name: 'Name',
            visible: true,
            sortable: true,
            type: 'text'
          },
          {
            key: 'ftpHost',
            name: 'Ftp Host',
            visible: true,
            sortable: true,
            type: 'text'
          },
          {
            key: 'lastModification',
            name: 'Last Modification',
            visible: false,
            sortable: true,
            type: 'date'
          }
        ],
        getDefaultFilter: function () {
          return '';
        },
        includes: [
        ]
      }
    }
  },
  methods: {
    onRowClicked: function (item, context) {
      this.$router.push('/destination/' + item.destinationId);
    },
    onNewClicked: function (filters) {
      this.$router.push('/destination/add');
    }
  },
  computed: {
  },
  mounted () {
  }
};
</script>