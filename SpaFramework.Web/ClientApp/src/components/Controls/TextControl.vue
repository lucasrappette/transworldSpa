<template>
  <!-- <b-form-group :label="label" :description="description" :invalid-feedback="validationError" label-cols="4" content-cols="8"> -->
  <b-form-group :label="label" :description="description" :invalid-feedback="validationError" class="form-floating-label-group">
    <small v-if="topDescription" class="text-muted">{{topDescription}}</small>
    <b-form-input ref="formInput" v-model="content" :key="componentKey" @input="updateContent()" :type="type" :state="state" :placeholder="placeholder" class="floating-label"></b-form-input>
  </b-form-group>
</template>

<script>
export default {
  name: "TextControl",
  props: {
    label: {},
    description: {},
    topDescription: {},
    placeholder: {},
    value: {
      required: true
    },
    type: {
      default: 'text'
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
        return 'Please provide a value at least ' + this.minLength + ' characters long';
      }      
    },
    maxLength: {
      default: null
    },
    maxLengthError: {
      default: function () {
        return 'Please provide a value less than ' + this.maxLength + ' characters long';
      }
    },
    emailError: {
      default: 'Please provide a valid email address.'
    },
    customValidation: {},
    formName: {
      default: 'Default'
    },
    concurrencyCheck: {
      default: null
    },
    template: {
      default: null
    }
  },
  data() {
    return {
      content: this.toTemplate(this.value),
      originalValue: this.value,
      state: null,
      validationError: null,
      componentKey: 0
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
    toTemplate(x) {
      if (x && this.template == 'phone') {
        let y = this.fromTemplate(x);

        if (y.length >= 6)
          return y.substring(0, 3) + '-' + y.substring(3, 6) + '-' + y.substring(6);
        else if (y.length == 6)
          return y.substring(0, 3) + '-' + y.substring(3, 6) + '-';
        else if (y.length >= 4)
          return y.substring(0, 3) + '-' + y.substring(3);
        else if (y.length == 3)
          return y.substring(0, 3) + '-';
        else
          return y;
      } else
        return x;
    },
    fromTemplate(x) {
      if (x && this.template == 'phone')
        return x.replace(/[- ()]/g, "");
      else
        return x;
    },
    updateContent() {
      if (this.template == 'phone') {
        // The "clean value" is just numbers and hyphens
        let cleanValue = this.content.replace(/[^0-9-]+/g, '');

        // And the "clean value" only has hyphens at index 3 and 7 ("xxx-xxx-xxxx")
        for (var i = 0; i < cleanValue.length; ++i)
          if (i < cleanValue.length && i != 3 && i != 7 && cleanValue[i] == '-')
            cleanValue = cleanValue.substring(0, i) + cleanValue.substring(i + 1);

        // If this isn't clean -- that is, there are any non-numeric value or hyphens in the wrong spot, use the clean value instead
        if (this.content != cleanValue) {
          this.content = cleanValue;

          // If we just set this.content, it doesn't seem to trigger the refresh we'd expect -- not sure if this is a BootstrapVue bug? So we increment the component key
          this.componentKey += 1;

          // And incrementing the component key seems to cause us to lose focus -- so this puts focus back on the input box
          window.setTimeout(() => {
            this.$refs.formInput.focus();
          });
        }
      }

      this.$emit('input', this.fromTemplate(this.content));
    },
    checkValidity(templatedNewValue, templatedOldValue) {
      let validationError = null;

      let newValue = this.fromTemplate(templatedNewValue);
      let oldValue = this.fromTemplate(templatedOldValue);

      if (this.required !== null && this.required !== false && (newValue === null || newValue === '' || typeof(newValue) === 'undefined'))
        validationError = this.requiredError;
      else if (this.minLength && this.minLength > 0 && newValue !== null && newValue.length < this.minLength)
        validationError = this.minLengthError;
      else if (this.maxLength && this.maxLength > 0 && newValue !== null && newValue.length > this.maxLength)
        validationError = this.maxLengthError;
      else if (this.type == 'email' && newValue !== null && newValue !== '' && !/\S+@\S+\.\S+/.test(newValue))
        validationError = this.emailError;
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
      this.content = this.toTemplate(newValue);
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