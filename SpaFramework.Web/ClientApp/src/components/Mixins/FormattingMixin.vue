<script>
import { DateTime } from 'luxon';
import marked from 'marked';

let customRenderer = new marked.Renderer();

customRenderer.heading = function (text, level, raw, slugger) {
  return `<h${level}>${text}</h${level}>`;
};

export default {
  name: "FormattingMixin",
  props: [
  ],
  data() {
    return {
    }
  },
  computed: {
  },
  methods: {
    getFormattedDate: function (d) {
      let dt = DateTime.fromISO(d);
      return dt.toLocaleString(DateTime.DATE_MED_WITH_WEEKDAY);
    },
    getFormattedDateTime: function (d) {
      if (!d)
        return null;
        
      let dt = DateTime.fromISO(d);
      return dt.toLocaleString(DateTime.DATE_MED_WITH_WEEKDAY) + ' ' + dt.toFormat('h:mm a');
    },
    getSelectText: function (value, selectOptions) {
      let opt = selectOptions.find(x => x.value == value);
      if (opt)
        return opt.text;
      else
        return opt;
    },
    getCamelSpacedText: function (value) {
      return value.replace(/([A-Z])/g, ' $1').trim()
    },
    getSentenceSpacedText: function (value) {
      let result = value.replace(/([A-Z])/g, " $1");
      return result.charAt(0).toUpperCase() + result.slice(1);
    },
    getFormattedPhoneNumber: function (value) {
      if (value.length != 10)
        return value;

      return value.substring(0, 3) + '-' + value.substring(3, 6) + '-' + value.substring(6);
    },
    getFormattedDollar: function (value) {
      var formatter = new Intl.NumberFormat('en-US', {
        style: 'currency',
        currency: 'USD',
      });

      return formatter.format(value);
    },
    getOrdinal: function (i) {
      var j = i % 10,
          k = i % 100;
      if (j == 1 && k != 11) {
          return i + "st";
      }
      if (j == 2 && k != 12) {
          return i + "nd";
      }
      if (j == 3 && k != 13) {
          return i + "rd";
      }
      return i + "th";
    }
  },
  mounted () {
  }
};
</script>