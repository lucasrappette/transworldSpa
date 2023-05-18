<template>
  <b-form-group :label="label" :description="description" class="form-floating-label-group">
    <!-- <b-form-group :label="label" :description="description" label-cols="4" content-cols="8"> -->
    <small v-if="topDescription" class="text-muted">{{topDescription}}</small>
    <span class="form-static-value floating-label"><slot>{{displayValue}}</slot></span>
  </b-form-group>
</template>

<script>
export default {
  name: "StaticControl",
  props: {
    label: {},
    description: {},
    topDescription: {},
    value: {},
    options: {}
  },
  data() {
    return {
    };
  },
  computed: {
    isValid: function () {
      return true;
    },
    displayValue: function () {
      if (!this.options)
        return this.value;

      let item = this.options.find(x => x.value == this.value);
      if (item)
        return item.text;

      return this.value;
    }
  },
  methods: {
  },
  watch: {
  },
  mounted () {
  }
};
</script>