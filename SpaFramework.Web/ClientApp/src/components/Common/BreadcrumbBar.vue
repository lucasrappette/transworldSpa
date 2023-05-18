<template>
  <div>
    <b-breadcrumb :items="items"/>
  </div>
</template>

<script>
import axios from "axios";
import FormMixin from '../Mixins/FormMixin.vue';
import { mapState, mapGetters, mapMutations, mapActions } from 'vuex'
import { DateTime } from 'luxon';
import FormattingMixin from '../Mixins/FormattingMixin.vue';

export default {
  name: 'BreadcrumbBar',
  mixins: [],
  data() {
    return {
    };
  },
  methods: {
    getKnownPageName(path, pathPart, defaultValue) {
      let item = this.knownPageNames.find(x => x.path == path);
      if (item)
        return item.name;

      item = this.knownPageNames.find(x => x.path == '/' + pathPart);
      if (item)
        return item.name;

      return defaultValue;
    },
  },
  computed: {
    ...mapState('cachedData', ['knownPageNames']),
    ...mapGetters('auth', ['isAuthenticated', 'authenticatedUsername', 'authenticatedUserId', 'isSuperAdmin']),
    items: function () {
      let retVal = [];

      let p = '';
      this.$route.path.split('/').forEach(x => {
        if (x == '') {
          retVal.push({
            text: this.getKnownPageName('/'),
            to: '/'
          });
        } else {
          p += '/' + x;
          retVal.push({
            text: this.getKnownPageName(p, x, x),
            to: p
          });
        }
      });

      return retVal;
    }
  },
  mounted () {
  },
  watch: {
  }
}
</script>