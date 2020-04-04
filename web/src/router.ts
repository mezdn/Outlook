import Vue from 'vue';
import Router from 'vue-router';

Vue.use(Router);

import Home from './views/Home/Home.vue';

export default new Router({
    mode: 'history',
    routes: [
        {
            path: "*",
            redirect: "/"
        },
        {
            path: "/",
            component: Home
        },
    ]
});