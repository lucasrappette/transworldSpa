<template>
  <div>
    <ol class="step-indicator">
      <li v-for="(step, index) in steps" v-bind:key="index" :class="index == stepIndex ? 'active' : (index < stepIndex ? 'complete': '')" @click="changeStep(index)">
        <div class="step"><font-awesome-icon :icon="step.icon"></font-awesome-icon></div>
        <div class="caption hidden-xs hidden-sm" v-html="step.label"></div>
      </li>
    </ol>
  </div>
</template>

<script>
// Loosely based on https://vuejsexamples.com/bootstrap-wizard-with-vue-js/

export default {
  name: 'Steps',
  props: [
    'title',
    'steps',
    'stepIndex'
  ],
  data() {
    return {
    };
  },
  methods: {
    changeStep: function (index) {
      if (index < this.stepIndex)
        this.$emit('changeStep', index);
    }
  }
}
</script>