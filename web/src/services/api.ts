import axios from 'axios';

const APP_URL = process.env.VUE_APP_OUTLOOK;
const API_URL = APP_URL + '/api/';

export class Api {
    async getVolumeNumbers() {
        var response = await axios.get(API_URL + 'volumes');
        return response.data;
    }

    async getIssues(volumeId: Number) {
        var response = await axios.get(API_URL + 'issues/' + volumeId);
        return response.data;
    }

    async getCategories() {
        var response = await axios.get(API_URL + 'categories');
        return response.data;
    }

    async getLanguageFile(lang: string | null) {
        if (lang == null) {
            return;
        }
        var response = await fetch(lang + '.json').then(d => d.json());
        return response;
    }

    async getIcons() {
        var response = await fetch('font-awesome.json').then(d => d.json());
        return response;
    }
}

export const api = new Api();