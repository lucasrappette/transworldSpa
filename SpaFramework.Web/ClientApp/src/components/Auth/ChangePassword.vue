<template>
  <div>
    <b-container class="py-3">
      <b-row>
        <b-col>
          <auth-form page="change" class="mt-3" />
        </b-col>
      </b-row>
    </b-container>
  </div>
</template>

<script>
import axios from "axios";

export default {
  name: "ChangePassword",
  props: {},
  data() {
    return {
    }
  },
  methods: {
  },
  mounted () {
  }
};
</script>