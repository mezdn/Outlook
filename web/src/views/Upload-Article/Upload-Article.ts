import { Component, Vue } from "vue-property-decorator";
import { api } from '../../services/api';

@Component
export default class UploadArticle extends Vue {
    private Form = new FormData();
    private ProcessCompleted = false;

    sendFile() {
        this.ProcessCompleted = false;
        api.uploadArticle(this.Form).then(status => {
            if (status > 199 && status < 300) {
                this.ProcessCompleted = true;
            }
        });
    }

    updateFile(file: any) {
        this.Form.append('file', file, file.name);
    }
}