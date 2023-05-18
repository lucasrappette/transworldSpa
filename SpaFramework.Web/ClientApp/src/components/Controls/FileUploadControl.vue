<template>
  <b-form-group :label="label" :description="description" :invalid-feedback="validationError" label-cols="4" content-cols="8">
    <small v-if="topDescription" class="text-muted">{{topDescription}}</small>
    <div v-if="isUploading">
      <b-spinner label="Spinning"></b-spinner> Uploading file...
    </div>
    <div v-if="content && !isUploading">
      <b-link @click="downloadFile()">View Uploaded File</b-link>
      <b-button-group>
        <b-button class="ml-3" size="sm" @click="onReplace">Replace</b-button>
        <b-button size="sm" @click="onDelete">Delete</b-button>
      </b-button-group>
    </div>
    <b-form-file v-if="!isUploading && (!content || isReplacing)" v-model="pendingFile" @input="uploadFile()" :state="state" :accept="accept"></b-form-file>
  </b-form-group>
</template>

<script>
export default {
  name: "FileUploadControl",
  props: {
    label: {},
    description: {},
    topDescription: {},
    value: {
      required: true
    },
    accept: {
    },
    required: {
      default: false
    },
    requiredError: {
      default: 'This field is required'
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
      pendingFile: null,
      isUploading: false,
      isReplacing: false,
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
    uploadFile() {
      this.isUploading = true;
      this.$emit('upload', this.pendingFile);
    },
    downloadFile() {
      this.$emit('download', this.content);
    },
    onReplace() {
      this.isReplacing = true;
    },
    onDelete() {
      this.content = null;
      this.isUploading = false;
      this.isReplacing = false;
      this.$emit('input', this.content);
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
      this.isUploading = false;
      this.isReplacing = false;
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