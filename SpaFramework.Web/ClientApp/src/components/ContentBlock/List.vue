<template>
  <list-page-template page-title="Content Blocks">
    <filtered-table :settings="tableSettings" @rowClicked="onRowClicked">
    </filtered-table>
  </list-page-template>
</template>

<script>
import axios from "axios";

export default {
  name: "ContentBlockList",
  props: {},
  data() {
    let base = this;
    return {
      tableSettings: {
        endpoint: '/api/contentBlock',
        newUrl: '/contentBlock/add',
        columns: [
          {
            key: 'id',
            name: 'ID',
            visible: false,
            sortable: true,
            type: 'guid'
          },
          {
            key: 'slug',
            name: 'Slug',
            visible: true,
            sortable: true,
            type: 'text'
          },
          {
            key: 'isPage',
            name: 'Is Page',
            visible: true,
            sortable: true,
            type: 'select',
            selectOptions: [
              {
                value: null,
                text: ''
              },
              {
                value: true,
                text: 'Yes'
              },
              {
                value: false,
                text: 'No'
              }
            ]
          },
          {
            key: 'title',
            name: 'Title',
            visible: true,
            sortable: true,
            type: 'text'
          },
          {
            key: 'description',
            name: 'Description',
            visible: true,
            sortable: true,
            type: 'text'
          }
        ],
        getDefaultFilter: function () {
          return '';
        }
      }
    }
  },
  methods: {
    onRowClicked: function (item, context) {
      this.$router.push('/contentBlock/' + item.id);
    }
  },
  mounted () {
  }
};
</script>