<template>
  <b-form-group :label="label" :description="description" :invalid-feedback="validationError" class="form-floating-label-group">
    <b-form-checkbox-group v-model="content" @input="updateContent()" :options="options" :state="state" :name="name" :stacked="stacked" class="floating-label" />
  </b-form-group>
</template>

<script>
export default {
  name: "CheckBoxListControl",
  props: {
    label: {},
    name: {
      required: true
    },
    stacked: {
      default: true
    },
    idField: {
      required: true
    },
    description: {},
    options: {},
    value: {
      required: true
    },
    required: {
      default: false
    },
    requiredError: {
      default: 'This field is required'
    },
    customValidation: {},
    concurrencyCheck: {
      default: null
    }
  },
  data() {
    return {
      content: this.value ? this.value.map(x => x[this.idField]) : [],
      originalContent: this.value ? this.value.map(x => x[this.idField]) : [],
      originalValue: this.value,
      state: null,
      validationError: null
    };
  },
  computed: {
    isValid: function () {
      if (this.state === null)
        this.checkValidity(this.content, this.content);

      return this.state === true || this.state === null;
    }
  },
  methods: {
    getStructuredContent(content, originalValue) {
      let existingItems = originalValue ? originalValue.filter(x => content.includes(x[this.idField])) : [];
      let newItems = content ? content.filter(x => !originalValue || !originalValue.some(y => y[this.idField] == x)).map(x => { let z = {}; z[this.idField] = x; return z; }) : [];
      return existingItems.concat(newItems);
    },
    updateContent() {
      this.$emit('input', this.getStructuredContent(this.content, this.originalValue));
    },
    checkValidity(newValue, oldValue) {
      let validationError = null;

      if (this.required !== null && this.required !== false && (newValue == null || newValue == ''))
        validationError = this.requiredError;
      else if (typeof(this.customValidation) === 'function') {
        let retVal = this.customValidation(newValue, oldValue);
        if (retVal !== true && retVal !== null)
          validationError = retVal;
        else {
          this.state = retVal;
          validationError = null;
        }
      }

      this.validationError = validationError;
      if (this.validationError != null)
        this.state = false;
      else {
        if (this.originalContent.every(x => newValue.some(y => x == y)) && newValue.every(x => this.originalContent.some(y => x == y)))
          this.state = null;
        else
          this.state = true;
      }
    }
  },
  watch: {
    content: function (newValue, oldValue) {
      this.checkValidity(newValue, oldValue);
    },
    concurrencyCheck: function (newValue, oldValue) {
      this.originalContent = this.content;
      this.state = null;
    }
  },
  mounted () {
  }
};
</script>