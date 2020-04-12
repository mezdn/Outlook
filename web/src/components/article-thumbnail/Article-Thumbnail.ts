import { Component, Vue, Watch } from "vue-property-decorator";
import { ApiObject } from '../../models/apiObject';
import { api } from '@/services/api';

@Component({
    props: {
        article: {
            type: Object,
            default() { return new Object(); }
        }
    }
})
export default class ArticleThumbnail extends Vue {
    private Language = new ApiObject();
    private APP_URL = process.env.VUE_APP_OUTLOOK;
    private Colors: ApiObject | null = null;

    created() {
        this.getColors();
        this.UpdateLanguage();
    }

    getColors() {
        var colors = this.$root.$children[0].$data.Colors
        if (colors != undefined) {
            this.Colors = colors;
        }
        else {
            api.getColors().then(d => {
                this.Colors = d;
            });
        }
    }

    getCategoryColor(cat: string) {
        if (this.Colors == null) {
            return "";
        }
        return this.Colors[cat]
    }

    showArticle(article: ApiObject) {
        if (this.Language == undefined || article == undefined) {
            return false;
        }
        return article.lang == this.Language.lang;
    }

    @Watch("$parent.$data.Language")
    UpdateLanguage() {
        this.Language = this.$parent.$data.Language;
    }
}