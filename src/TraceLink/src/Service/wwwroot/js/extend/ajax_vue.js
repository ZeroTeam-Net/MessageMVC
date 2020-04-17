
var common = {
    /**
     * 用户确认
     * @param {string} title 标题
     * @param {string} message 消息
     * @param {Function} callback 确认后的回调
     * @returns {void}
     */
    confirm(title, message, callback) {
        vueObject.$confirm(message, title, {
            confirmButtonText: '确定',
            cancelButtonText: '取消',
            type: 'warning'
        }).then(() => {
            callback();
        });
    },
    /**
     * 显示提示
     * @param {string} title 标题
     * @param {string} message 消息
     * @returns {void} 
     */
    showTip(title, message) {
        vueObject.$notify({
            title: title,
            message: message,
            duration: 0
        });
    },
    /**
     * 显示错误（对话框类型）
     * @param {string} title 标题
     * @param {string} message 消息
     */
    showError(title, message) {
        this.showMessage(title, message,'error');
    },
    /**
     * 显示提示
     * @param {string} title 标题
     * @param {string} message 消息
     * @param {string} type 类型
     * @returns {void} 
     */
    showMessage(title, message, type) {
        if (!type) {
            type = 'success';
        }
        vue_option.data.message = message;
        vueObject.$message({
            message: message,
            type: type
        });
    },
    /**
     * 显示提示
     * @param {object} res 返回值 标题
     * @returns {void} 
     */
    showStatus(res) {
        if (res.success) {
            this.showMessage(null, res.status && res.status.msg
                ? res.status.msg
                : "请求成功");
        }
        else {
            this.showMessage(null, res.status && res.status.msg
                ? res.status.msg
                : "网络错误", "error");
        }
    },
    loading: null,
    busyNum: 0,
    isBusy: false,
    showBusy(title) {
        if (this.isBusy || this.loading) {
            ++this.busyNum;
            return;
        }
        this.busyNum = 1;
        this.isBusy = true;
        var that = this;
        setTimeout(function () {
            if (!that.isBusy || this.busyNum === 0)
                return;
            that.loading = vueObject.$loading({
                lock: true,
                text: '拼命加载中',
                spinner: 'el-icon-loading',
                background: 'rgba(0, 0, 0, 0.7)'
            });
            if (!that.isBusy || this.busyNum === 0) {
                this.loading.close();
                this.loading = null;
            }
        }, 300);
    },
    hideBusy() {
        if (--this.busyNum === 0) {
            this.isBusy = false;
            if (this.loading) {
                this.loading.close();
                this.loading = null;
            }
        }
    }
};