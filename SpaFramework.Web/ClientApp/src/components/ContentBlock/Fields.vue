<template>
  <form-template @submit="onSubmit" @cancel="onCancel">
    <template v-slot:save><slot name="save"></slot></template>
    <template>
      <b-row>
        <b-col xs="12" md="6">
          <b-card class="mt-3">
            <h4 slot="header">Basic</h4>
            <text-control label="Slug" v-model="item.slug" required description="A slug is the key for the content block. If the content block is a page, the slug is the identifier portion of the URL." :concurrency-check="item.concurrencyCheck"></text-control>
            <text-control label="Title" v-model="item.title" required description="If this content is a page or email, the title is displayed as the H1 or subject line." :concurrency-check="item.concurrencyCheck"></text-control>
            <text-control label="Description" v-model="item.description" :concurrency-check="item.concurrencyCheck"></text-control>
            <textarea-control label="Value" required description="The content uses Markdown for formatting." v-model="item.value" :concurrency-check="item.concurrencyCheck"></textarea-control>
          </b-card>
        </b-col>
        <b-col xs="12" md="6">
          <b-card class="mt-3">
            <h4 slot="header">Page</h4>
            <checkbox-control label="Is Page" v-model="item.isPage" :concurrency-check="item.concurrencyCheck"></checkbox-control>
            <p v-if="item.isPage"><b-link :to="pageUrl">View Page.</b-link></p>
          </b-card>
          <b-card class="mt-3">
            <h4 slot="header">Tokens</h4>
            <p v-if="item.allowedTokens && item.allowedTokens.length > 0">The following tokens can be used in the content. The token will be replaced by another value when it's rendered. To use a token, wrap it with percent signs. For instance, the token <code>Name</code>. would be entered in content as <code>%Name%</code>.</p>
            <p v-if="!item.allowedTokens || item.allowedTokens.length == 0">There are no tokens that can be used in this content.</p>
            <b-table :items="item.allowedTokens">
            </b-table>
          </b-card>
        </b-col>
      </b-row>
    </template>
  </form-template>
</template>

<script>
export default {
  name: "ContentBlockFields",
  props: [
      'item'
  ],
  data() {
    return {
    };
  },
  computed: {
    pageUrl: function () {
      return '/page/' + this.item.slug;
    }
  },
  methods: {
    onCancel(evt) {
      this.$emit('cancel');
    },
    onSubmit(evt) {
      this.$emit('submit');
    }
  }
};
</script>