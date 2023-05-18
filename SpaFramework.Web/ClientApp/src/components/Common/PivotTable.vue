<template>
  <div>
    <div v-if="(!item || !item.reportData) && isLoading">
      <b-row>
        <b-col>
          <b-alert show variant="secondary" class="py-3">
            <span class="float-left">
              <b-spinner label="Spinning"></b-spinner>
            </span>
            <span class="ml-2">Please wait, loading...</span>
          </b-alert>
        </b-col>
      </b-row>
    </div>
    <div v-if="(item && item.reportData)">
      <b-row>
        <b-col>
          <b-button-toolbar class="mb-2">
            <b-button-group>
              <b-dropdown>
                <template slot="button-content">
                  <b-icon icon="list-nested" />
                </template>
                <b-dropdown-group header="Pivots/Groupings" />
                <b-dropdown-item-button v-for="grouping in settings.groupings" v-bind:key="grouping.key" @click="toggleGrouping(grouping)">
                  <b-icon icon="square" v-if="!visibleGroupings[grouping.key]" />
                  <b-icon icon="check-square" v-if="visibleGroupings[grouping.key]" />
                  {{grouping.displayName}}
                </b-dropdown-item-button>
              </b-dropdown>
              <b-dropdown>
                <template slot="button-content">
                  <b-icon icon="search" />
                </template>
                <b-dropdown-group header="Filters" />
                <b-dropdown-item-button v-for="grouping in settings.groupings" v-bind:key="grouping.key" v-bind:disabled="!visibleGroupings[grouping.key] || grouping.disableFilter" @click="openFilterModal(grouping)">
                  {{grouping.displayName}}
                </b-dropdown-item-button>
              </b-dropdown>
              <b-dropdown>
                <template slot="button-content">
                  <b-icon icon="table" />
                </template>
                <b-dropdown-group header="Columns" />
                <b-dropdown-item-button v-for="column in settings.columns" v-bind:key="column.key" @click="toggleColumn(column)">
                  <b-icon icon="square" v-if="!visibleColumns[column.key]" />
                  <b-icon icon="check-square" v-if="visibleColumns[column.key]" />
                  {{column.name}}
                </b-dropdown-item-button>
              </b-dropdown>
              <b-dropdown>
                <template slot="button-content">
                  <b-icon icon="card-checklist" />
                </template>
                <b-dropdown-group header="Display Options">
                  <b-dropdown-item-button @click="showGroupingNameOnRow = !showGroupingNameOnRow"><b-icon :icon="showGroupingNameOnRow ? 'check-square' : 'square'" /> Show Grouping Name</b-dropdown-item-button>
                  <b-dropdown-item-button @click="showVerticalHeaders = !showVerticalHeaders"><b-icon :icon="showVerticalHeaders ? 'check-square' : 'square'" /> Show Vertical Headers</b-dropdown-item-button>
                  <b-dropdown-item-button @click="showEmptyRows = !showEmptyRows; onClickRefreshData()"><b-icon :icon="showEmptyRows ? 'check-square' : 'square'" /> Show Empty Rows</b-dropdown-item-button>
                </b-dropdown-group>
              </b-dropdown>
              <b-button v-if="settings.showRefreshButton" @click="onClickRefreshData()"><b-icon icon="arrow-clockwise"></b-icon></b-button>
            </b-button-group>
            <b-button-group class="ml-2" v-if="!this.settings.hideCalendarControlFn || !this.settings.hideCalendarControlFn">
              <b-input-group>
                <template #prepend>
                  <b-dropdown>
                    <template slot="button-content">
                      <b-icon icon="calendar3" /> {{selectedDateRange}}
                    </template>
                    <b-dropdown-item-button v-for="dateRange in dateRanges" v-bind:key="dateRange.name" @click="setDateRange(dateRange)"><b-icon :icon="selectedDateRange == dateRange.name ? 'check-circle' : 'circle'" /> {{dateRange.name}}</b-dropdown-item-button>
                    <b-dropdown-item-button @click="selectedDateRange = 'Custom'"><b-icon :icon="selectedDateRange == 'Custom' ? 'check-circle' : 'circle'" /> Custom Date Range</b-dropdown-item-button>
                  </b-dropdown>
                </template>
                <b-form-datepicker size="lg" v-model="dateRangeStart" class="pivot-table-date-picker" :date-format-options="{ year: 'numeric', month: 'numeric', day: 'numeric' }" locale="en"></b-form-datepicker>
                <b-form-datepicker size="lg" v-model="dateRangeEnd" class="pivot-table-date-picker" :date-format-options="{ year: 'numeric', month: 'numeric', day: 'numeric' }" locale="en"></b-form-datepicker>
              </b-input-group>
            </b-button-group>
            <slot name="extraButtons"></slot>
            <b-button-group class="ml-2" v-if="!settings.hideExportFn || !settings.hideExportFn()">
              <b-dropdown>
                <template slot="button-content">
                  <b-icon icon="file-earmark-spreadsheet" /> Export
                </template>
                <b-dropdown-group header="Export Results">
                  <b-dropdown-item-button @click="exportResults('CSV')">Download CSV</b-dropdown-item-button>
                  <b-dropdown-item-button @click="exportResults('PDF')">Download PDF</b-dropdown-item-button>
                  <b-dropdown-item-button @click="exportResults('ViewPDF')">View PDF</b-dropdown-item-button>
                </b-dropdown-group>
              </b-dropdown>
            </b-button-group>
          </b-button-toolbar>
        </b-col>
      </b-row>
      <b-row v-if="isLoading">
        <b-col>
          <b-alert show variant="secondary" class="py-3">
            <span class="float-left">
              <b-spinner label="Spinning"></b-spinner>
            </span>
            <span class="ml-2">Please wait, loading...</span>
          </b-alert>
        </b-col>
      </b-row>
      <b-row v-if="item && item.reportData">
        <b-col>
          <b-table-simple bordered>
            <b-thead>
              <b-tr>
                <b-th></b-th>
                <b-th v-for="(col, colIndex) in visibleColumnList" v-bind:key="colIndex" class="pivot-table-header">
                  <span :class="'pivot-table-header-name' + (showVerticalHeaders ? ' pivot-table-header-name-vertical' : '')">
                    {{col.name}}
                  </span>
                </b-th>
              </b-tr>
            </b-thead>
            <b-tbody>
              <b-tr v-for="(row, rowIndex) in item.reportData" v-bind:key="rowIndex">
                <b-td>
                  <span :style="'padding-left: ' + (row._data.depth * 15) + 'px'">
                    {{(showGroupingNameOnRow && row._data.grouping) ? row._data.grouping.displayName + ': ' : ''}}
                    {{row._data.name}}
                  </span>
                </b-td>
                <b-td v-for="(col, colIndex) in visibleColumnList" v-bind:key="colIndex" class="text-right">
                  {{(col.render ? col.render(row[col.key], row, col) : row[col.key])}}
                </b-td>
                <!-- <b-td>
                  {{row._data}}
                </b-td> -->
              </b-tr>
            </b-tbody>
          </b-table-simple>
        </b-col>
      </b-row>
    </div>
    <b-modal id="modalGroupingFilters" @ok="refreshData" header-bg-variant="secondary" header-text-variant="light" :ok-only="true" :ok-title="'Apply Filters'">
      <template #modal-title>
        Filter Options
      </template>
      <b-table :items="shownGroupFilterOptions" :fields="fields" :per-page="perGroupFilterPage" :current-page="currentGroupFilterPage">
        <template #cell(selected)="data">
          <b-form-checkbox @change="handleGroupingFilters(data)" v-model="data.item.selected" />
        </template>
      </b-table>
      <template #modal-footer="{ ok }">
        <b-pagination :per-page="perGroupFilterPage" v-model="currentGroupFilterPage" :total-rows="groupFilterRows"></b-pagination>
        <div class="mr-5"></div>
        <b-button size="sm" variant="success" @click="ok()">
          Apply Filters
        </b-button>
      </template>
    </b-modal>
    <b-modal id="modalExportResults" hide-footer header-bg-variant="secondary" header-text-variant="light">
      <template #modal-title>
        Export Results
      </template>
      <div>
        <b-spinner label="Spinning"></b-spinner>
      </div>
      <p>Please wait, downloading results...</p>
      <p>{{exportStep}}</p>
    </b-modal>
    <b-modal id="modalViewPDF" size="xl" hide-footer @shown="onModalViewPDFShown" header-bg-variant="secondary" header-text-variant="light">
      <template #modal-title>
        View PDF
      </template>
      <object ref="viewPDFResults" style="height: 80vh; width: 100%">
      </object>
    </b-modal>
  </div>
</template>

<script>
import axios from "axios";
import { DateTime } from 'luxon';
import { saveAs } from 'file-saver';
import { mapState, mapGetters } from 'vuex'
import jsPDF from 'jspdf'
import 'jspdf-autotable'

export default {
  name: "PivotTable",
  props: {
    settings: {
      type: Object
    },
    reportFilters: {}
  },
  data() {
    return {
      item: null,
      rowStates: [],
      visibleColumns: {},
      visibleGroupings: {},
      showGroupingNameOnRow: false,
      showVerticalHeaders: false,
      showEmptyRows: false,
      dateRangeStart: null,
      dateRangeEnd: null,
      exportStep: '',
      viewPDFResults: null,
      allGroupFilterOptions: [],
      shownGroupFilterOptions: [],
      fields: ['name', 'selected'],
      cachedGroupFilterOptions: {},
      cachedGroupFilterOptionsNeedsReload: true,
      perGroupFilterPage: 10,
      currentGroupFilterPage: 1,
      isLoading: true
    }
  },
  computed: {
    visibleColumnList: function () {
      return this.settings.columns.filter(c => this.visibleColumns[c.key]);
    },
    visibleGroupingList: function () {
      return this.settings.groupings.filter(c => this.visibleGroupings[c.key]);
    },
    selectedDateRange: function () {
      let sel = this.dateRanges.find(x => x.start == this.dateRangeStart && x.end == this.dateRangeEnd);

      if (sel && (!this.settings.hideCalendarControlFn || !this.settings.hideCalendarControlFn()))
        return sel.name;
      else
        return 'Custom';
    },
    dateRanges: function () {
      let x = [];
      let now = DateTime.now();

      x.push({
        name: 'Today',
        start: now.toFormat('yyyy-MM-dd'),
        end: now.toFormat('yyyy-MM-dd')
      });

      x.push({
        name: 'Yesterday',
        start: now.minus({days: 1}).toFormat('yyyy-MM-dd'),
        end: now.minus({days: 1}).toFormat('yyyy-MM-dd')
      });

      x.push({
        name: 'Last 7 Days',
        start: now.minus({days: 7}).toFormat('yyyy-MM-dd'),
        end: now.minus({days: 1}).toFormat('yyyy-MM-dd')
      });

      x.push({
        name: 'Last 30 Days',
        start: now.minus({days: 30}).toFormat('yyyy-MM-dd'),
        end: now.minus({days: 1}).toFormat('yyyy-MM-dd')
      });

      x.push({
        name: 'Last 90 Days',
        start: now.minus({days: 90}).toFormat('yyyy-MM-dd'),
        end: now.minus({days: 1}).toFormat('yyyy-MM-dd')
      });

      return x;
    },
    groupFilterRows() {
      return this.shownGroupFilterOptions.length
    }
  },
  methods: {
    load: function () {
      let vc = {};
      this.settings.columns.forEach(col => {
        if (typeof col.visible === 'undefined')
          vc[col.key] = true;
        else
          vc[col.key] = col.visible;
      });
      this.visibleColumns = vc;

      let gr = {};
      this.settings.groupings.forEach(grp => {
        if (typeof grp.visible === 'undefined')
          gr[grp.key] = true;
        else
          gr[grp.key] = grp.visible;
      });
      this.visibleGroupings = gr;

      let now = DateTime.now();
      if (!this.settings.hideCalendarControlFn || !this.settings.hideCalendarControlFn())
      {
        if (this.settings.defaultDateRangeStart)
          this.dateRangeStart = this.settings.defaultDateRangeStart;
        else
          this.dateRangeStart = now.toFormat('yyyy-MM-dd');

        if (this.settings.defaultDateRangeEnd)
          this.dateRangeEnd = this.settings.defaultDateRangeEnd;
        else
          this.dateRangeEnd = now.toFormat('yyyy-MM-dd');
      }
      this.refreshData();
    },
    onClickRefreshData: function () {
      this.refreshData();
    },
    setDateRange: function (dateRange) {
      this.dateRangeStart = dateRange.start;
      this.dateRangeEnd = dateRange.end;
      this.cachedGroupFilterOptionsNeedsReload = true;
      this.refreshData();
    },
    toggleColumn: function (column) {
      this.visibleColumns[column.key] = !this.visibleColumns[column.key];
    },
    toggleGrouping: function (grouping) {
      this.visibleGroupings[grouping.key] = !this.visibleGroupings[grouping.key];
      this.refreshData();
    },
    refreshData: function () {
      this.isLoading = true;
      let url = this.settings.endpoint;
      var reqParts = {};
      reqParts.groupings = this.visibleGroupingList.map(x => x.key);
      
      if (!this.settings.hideCalendarControlFn || !this.settings.hideCalendarControlFn()) {
        reqParts.startDate = this.dateRangeStart;
        reqParts.endDate = DateTime.fromISO(this.dateRangeEnd).plus({days: 1}).toFormat('yyyy-MM-dd');
      }

      if (this.reportFilters) {
        reqParts.filters = new Object();
        for(var index = 0; index < this.reportFilters.length; index++) {
          var filter = this.reportFilters[index];
          reqParts.filters[filter.name] = filter.value;
        }
      }

      if (this.allGroupFilterOptions && this.allGroupFilterOptions.length > 0)
      {
        reqParts.groupFilters = new Object();

        for(var i = 0; i < this.allGroupFilterOptions.length; i++)
        {
          if(this.visibleGroupings[this.allGroupFilterOptions[i].name])
          {
            if(!reqParts.groupFilters[this.allGroupFilterOptions[i].name])
              reqParts.groupFilters[this.allGroupFilterOptions[i].name] = [];

              reqParts.groupFilters[this.allGroupFilterOptions[i].name].push(this.allGroupFilterOptions[i].value);
          }
          else
          {
            this.allGroupFilterOptions = this.allGroupFilterOptions.slice(i, 1);
          }
        }
      }
      axios
        .post(url, reqParts)
        .then(response => {
          this.item = response.data;

          let prev = null;

          for (let rowIndex = 0; rowIndex < this.item.reportData.length; ++rowIndex) {
            let row = this.item.reportData[rowIndex];

            for (let groupingIndex = 0; groupingIndex < this.visibleGroupingList.length; ++groupingIndex) {
              if (!row._data) {
                let grouping = this.visibleGroupingList[groupingIndex];

                if (!prev || row[grouping.id] != prev[grouping.id]) {
                  let name = '';
                  if (typeof grouping.name === 'function')
                    name = grouping.name(row, rowIndex);
                  else
                    name = row[grouping.name];

                  let selected = false;
                  if(this.allGroupFilterOptions && this.allGroupFilterOptions.filter(e => e.value === name).length > 0)
                  { 
                    selected = true;
                  }

                  row._data = { 
                    id: row[grouping.id],
                    depth: (!prev ? 0 : groupingIndex + 1),
                    name: (!prev ? this.settings.totalRowName : name),
                    grouping: prev ? grouping : null,
                    selected: selected
                  };
                }
              }
            }

            prev = row;
          }

          if (!this.showEmptyRows)
            this.item.reportData = this.item.reportData.filter(x => x._data.name != '(None)');

          this.isLoading = false;
        })
        .catch(error => {
          console.log(error);

          this.isLoading = false;
        });
    },
    getValue: function (row, col) {
      let p = col.key.split('.');
      let v = row;
      p.forEach(k => {
        if (v === undefined || v === null)
          v = null;
        else
          v = v[k];
      });

      return v;
    },
    getCellValue: function (row, col) {
      if (this.visibleColumns[col.key]) {
        let v = this.getValue(row, col);

        if (col.render)
          return col.render(v, row);
        else
          return v;
      }
    },
    openFilterModal: function(grouping) {
      if(this.item && this.item.reportData)
      {
        if(!this.cachedGroupFilterOptions.hasOwnProperty(grouping.key) || this.cachedGroupFilterOptionsNeedsReload)
        {
          var newOptions = [];
          this.item.reportData.forEach((val, indx) => {
            if(val && val._data.grouping && val._data.grouping.key == grouping.key)
            {
              newOptions.push(val._data);
            }
          });
          this.cachedGroupFilterOptions[grouping.key] = newOptions; 
          this.shownGroupFilterOptions = newOptions;
          this.cachedGroupFilterOptionsNeedsReload = false;
        }
        else {
          this.shownGroupFilterOptions = this.cachedGroupFilterOptions[grouping.key];
        }
        this.$bvModal.show('modalGroupingFilters');
      }
    },
    handleGroupingFilters: function(data) {
      if(this.allGroupFilterOptions && data.item)
      {
        if(data.item.selected)
        {
          this.allGroupFilterOptions.push({name: data.item.grouping.key, value: data.item.name});
        }
        else if(!data.item.selected)
        {
          this.allGroupFilterOptions = this.allGroupFilterOptions
          .filter(x => x.name != data.item.grouping.key || (x.name == data.item.grouping.key && x.value != data.item.name));

        }
      }
    },
    exportResults: function (fileType) {
      this.exportStep = 'Processing...';
      this.$bvModal.show('modalExportResults');
      let exportItems = this.item.reportData;
      if (fileType == 'CSV') {
        let rows = [];
        let cols = [];
        this.visibleGroupingList.forEach(grp => {
          cols.push(grp.displayName)
        });
        cols.push('');
        this.settings.columns.forEach(col => {
          if(this.visibleColumns[col.key])
          cols.push(col.name);
        });
        rows.push(cols.join(','));
        exportItems.forEach(row => {
          cols = [];
          let totalPrinted = false;
          this.visibleGroupingList.every((val, idx, readonly) => { 
            if(this.visibleGroupings[val.key] && !totalPrinted)
            {
              if(row[val.name] || row[val.name] === "")
                cols.push('"' + row[val.name] + '"');
              else if(typeof val.name === 'function' && val.name(row, idx))
                cols.push('"' + val.name(row, idx) + '"');
              else
              {
                cols.push('Total');
                totalPrinted = true;
              }
            }
            else
              cols.push('');
            return true;
          });
          if(!totalPrinted)
            cols.push('Total');
          else
            cols.push('');
          this.settings.columns.forEach(col => {
            if (this.visibleColumns[col.key])
              cols.push(this.getCellValue(row, col));
          });

          rows.push(cols.join(','));
        });

        let output = rows.join('\n');

        this.exportStep = 'Exporting...';

        let blob = new Blob([output], {type: "text/csv;charset=utf-8"});
        var FileSaver = require('file-saver');
        FileSaver.saveAs(blob, "export.csv");
      } else if (fileType == 'PDF' || fileType == 'ViewPDF') {
        let head = [];
        let body = [];
        head.push('Grouping')
        this.settings.columns.forEach(col => {
          if(this.visibleColumns[col.key])
          head.push(col.name);
        });

        exportItems.forEach(row => {
          let cols = [];
          cols.push(row._data.name + (row._data.grouping ? ' [' + row._data.grouping.displayName + ']' : ''));
          this.settings.columns.forEach(col => {
            if (this.visibleColumns[col.key])
              cols.push(this.getCellValue(row, col));
          });

          body.push(cols);
        });

        let doc = new jsPDF('l', 'in', [8.5, 11], true);

        doc.autoTable({
          head: [head],
          body: body,
          styles: {
            font: 'Helvetica',
            fontSize: 9,
            cellWidth: 'auto',
            overflow: 'ellipsize'
          },
          tableWidth: 'wrap',
          horizontalPageBreak: true,
          horizontalPageBreakRepeat: 0
        });

        if (fileType == 'PDF') {
          doc.save('results.pdf');
        } else if (fileType == 'ViewPDF') {
          this.viewPDFResults = doc.output('datauristring');

          this.$bvModal.hide('modalExportResults');
          this.$bvModal.show('modalViewPDF');
        }
      }
      window.setInterval(() => { this.$bvModal.hide('modalExportResults') }, 600);
    },
    onModalViewPDFShown: function () {
      this.$refs.viewPDFResults.data = this.viewPDFResults;
    }
    
  },
  mounted () {
    this.load();
  },
  watch: {
    reportFilters(newFilters, oldFilters)
    {
      this.cachedGroupFilterOptionsNeedsReload = true;
      this.refreshData();
    }
  },
  activated () {
  }
};
</script>