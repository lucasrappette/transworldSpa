<template>
  <b-form-group :label="label" :description="description" :invalid-feedback="validationError" class="form-floating-label-group">
    <!-- <b-form-group :label="label" :description="description" :invalid-feedback="validationError" label-cols="4" content-cols="8"> -->
    <small v-if="topDescription" class="text-muted">{{topDescription}}</small>
    <b-form-textarea v-model="content" @input="updateContent()" rows="4" max-rows="12" :state="state" :placeholder="placeholder" class="floating-label"></b-form-textarea>
  </b-form-group>
</template>

<script>
export default {
  name: "TextareaControl",
  props: {
    label: {},
    description: {},
    topDescription: {},
    placeholder: {},
    value: {
      required: true
    },
    required: {
      default: false
    },
    requiredError: {
      default: 'This field is required'
    },
    minLength: {
      default: null
    },
    minLengthError: {
      default: function () {
        return 'This field must be at least ' + this.minLength + ' characters long';
      }      
    },
    maxLength: {
      default: null
    },
    maxLengthError: {
      default: function () {
        return 'This field must not be more than ' + this.minLength + ' characters long';
      }
    },
    customValidation: {},
    formName: {
      default: 'Default'
    },
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

      if (this.required !== null && this.required !== false && (newValue == null || newValue == ''))
        validationError = this.requiredError;
      else if (this.minLength && this.minLength > 0 && newValue.length < this.minLength)
        validationError = this.minLengthError;
      else if (this.maxLength && this.maxLength > 0 && newValue.length > this.maxLength)
        validationError = this.maxLengthError;
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