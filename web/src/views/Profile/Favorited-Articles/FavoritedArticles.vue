<template>
    <div class="favorites">
        <article-thumbnail v-for="article in Articles" :article="article" :language-specific="false"/>
    </div>
</template>

<script lang="ts" src="./FavoritedArticles.ts"></script>
<style lang="scss" src="./FavoritedArticles.scss"></style>