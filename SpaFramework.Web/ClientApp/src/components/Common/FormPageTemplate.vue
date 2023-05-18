<template>
  <div>
    <breadcrumb-bar />
    <b-container fluid>
      <b-row>
        <b-col>
          <span class="float-right">
            <slot name="header-right"></slot>
          </span>
          <page-loading v-if="!hideTitle && !item" />
          <page-title v-if="!hideTitle && item" :title="pageTitle" />
          <hr v-if="!hideTitle" />
          <div v-if="item" :class="hideTitle ? '' : 'mt-3'">
            <slot></slot>
          </div>
        </b-col>
      </b-row>
    </b-container>
  </div>
</template>

<script>
export default {
  name: 'FormPageTemplate',
  props: [ 
    'hideTitle',
    'item',
    'pageTitle'
  ]
}
</script>