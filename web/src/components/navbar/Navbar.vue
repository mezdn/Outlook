<template>
    <nav id="navbar" class="navbar" :class="$store.getters.Language['window-expand']">

        <!-- <logo> -->
        <div class="logo-container">
            <div class="logo" @click="$emit('expansion-toggled')">
                <svg-outlook />
            </div>
        </div>
        <!-- </logo> -->
            
        <!-- <options> -->
        <div class="options">
            <template v-if="!$store.getters.IsAuthenticated">
                <!-- <Login> -->
                <router-link :to="{ name: 'login' }" class="btn-group">
                    <button class="option"><svg-signin /></button>
                </router-link>
                <!-- </Login> -->

                <!-- <Register> -->
                <router-link :to="{ name: 'register' }" class="btn-group">
                    <button class="option"><svg-signup /></button>
                </router-link>
                <!-- </Register> -->
            </template>

            <template v-else>
                <!-- <Logout> -->
                <router-link :to="{ 'name': 'profile' }">{{$store.getters.User.username}}</router-link>
                <button class="option" @click="logout"><svg-signout /></button>
                <!-- </Logout> -->
            </template>

            <!-- <Home> -->
            <router-link :to="{ name: 'home' }" class="btn-group d-sm-none">
                <button class="option"><svg-home /></button>
            </router-link>
            <!-- </Home> -->

            <!-- <Archives> -->
            <router-link :to="{ name: 'archives' }" class="btn-group">
                <button class="option"><svg-archives /></button>
            </router-link>
            <!-- </Archives> -->

            <!-- <Change Language> -->
            <button class="option" @click="$emit('language-toggled')"><svg-language /></button>
            <!-- </Change Language> -->

        </div>
        <!-- </options> -->
    </nav>
</template>

<script lang="ts" src="./Navbar.ts"></script>
<style lang="scss" src="./Navbar.scss" scoped></style>
