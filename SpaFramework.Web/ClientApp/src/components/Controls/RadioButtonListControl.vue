<template>
  <b-form-group :label="label" :description="description" :invalid-feedback="validationError" class="form-floating-label-group">
    <!-- <b-form-group :label="label" :description="description" :invalid-feedback="validationError" label-cols="4" content-cols="8"> -->
    <b-form-radio-group v-model="content" @input="updateContent()" :options="options" :state="state" :name="name" :stacked="stacked" :buttons="buttons" class="floating-label" />
  </b-form-group>
</template>

<script>
export default {
  name: "RadioButtonListControl",
  props: {
    label: {},
    name: {
      required: true
    },
    stacked: {
      default: true
    },
    buttons: {
      default: false
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
      content: this.value,
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
    updateContent() {
      this.$emit('input', this.content);
    },
    checkValidity(newValue, oldValue) {
      let validationError = null;

      if (this.required !== null && this.required !== false && (newValue == null || newValue == '') && newValue !== true && newValue !== false)
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
        if (this.originalValue == newValue)
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
    value: function (newValue, oldValue) {
      this.content = newValue;
    },
    concurrencyCheck: function (newValue, oldValue) {
      this.originalValue = this.content;
      this.state = null;
    }
  },
  mounted () {
  }
};
</script>