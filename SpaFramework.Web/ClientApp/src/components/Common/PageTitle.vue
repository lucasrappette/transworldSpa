<template>
  <div>
    <h1>{{title}}</h1>
  </div>
</template>

<script>
export default {
  name: 'PageTitle',
  props: [ 'title' ],
  mounted() {
    document.title = this.title;
  },
  watch: {
    title(newValue, oldValue) {
      //console.log('title change from ' + oldValue + ' to ' + newValue);
    }
  }
}
</script>