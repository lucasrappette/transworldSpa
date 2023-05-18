<template>
  <list-page-template page-title="Dealers">
    <filtered-table :settings="tableSettings" @rowClicked="onRowClicked" @newClicked="onNewClicked">
    </filtered-table>
  </list-page-template>
</template>

<script>
import axios from "axios";

export default {
  name: "DealerList",
  props: {},
  data() {
    let base = this;
    return {
      tableSettings: {
        endpoint: '/api/dealer',
        showNewButton: true,
        defaultLimit: 100,
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
          'dealerStats'
        ]
      }
    }
  },
  methods: {
    onRowClicked: function (item, context) {
      this.$router.push('/dealer/' + item.id);
    },
    onNewClicked: function (filters) {
      this.$router.push('/dealer/add');
    }
  },
  computed: {
  },
  mounted () {
  }
};
</script>