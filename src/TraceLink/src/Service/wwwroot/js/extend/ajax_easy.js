
var common = {
    /**
     * 用户确认
     * @param {string} title 标题
     * @param {string} message 消息
     * @param {Function} callback 确认后的回调
     * @returns {void}
     */
    confirm(title, message, callback) {
        $.messager.confirm(title, message, function (s) {
            if (s) {
                callback();
            }
        });
    },
    /**
     * 显示错误（对话框类型）
     * @param {string} title 标题
     * @param {string} message 消息
     */
    showError(title, message) {
        $.messager.alert(!title ? "错误" : title, message);
    },
    /**
     * 显示提示（对话框类型）
     * @param {string} title 标题
     * @param {string} message 消息
     */
    showMessage(title, message) {
        $.messager.alert(!title ? "提示" : title, message);
    }, 
    /**
     * 显示提示
     * @param {string} title 标题
     * @param {string} message 消息
     * @param {string} type 类型
     * @returns {void} 
     */
    showTip(title, message, type) {
        var img;
        switch (type) {
            default:
            case 'success':
                img = 'succeed.png';
                break;
            case 'warning':
            case 'error':
                img = 'warning.png';
                break;
        }
        $.messager.show({
            title: !title ? "提示" : title,
            msg: `<div class="tip_block"><img src="/images/${img}"/><span>${message}</span></div>`,
            showType: "fade",
            showSpeed: 0,
            width: 180, height: 120,
            timeout: 3000,
            style: {
                left: 0,
                right: "",
                top: "",
                bottom: -document.body.scrollTop - document.documentElement.scrollTop
            }
        });
    },
    /**
     * 显示提示
     * @param {object} res 返回值 标题
     * @returns {void} 
     */
    showStatus(res) {
        if (res.success) {
            this.showTip(null, res.status && res.status.msg
                ? res.status.msg
                : "请求成功");
        }
        else {
            this.showTip(null, res.status && res.status.msg
                ? res.status.msg
                : "网络错误", "error");
        }
    },
    isBusy: false,
    showBusy(title) {
        isBusy = true;
        setTimeout(function () {
            if (!isBusy)
                return;
            var busy = document.getElementById("__loading__");
            if (busy) {
                $("#__loading__").css("visibility", "visible");
            } else {
                $.messager.progress({
                    title: title,
                    msg: "正在处理,请稍候……",
                    text: "**************************",
                    interval: 800
                });
            }
        }, 300);
    },
    hideBusy() {
        isBusy = false;
        var busy = document.getElementById("__loading__");
        if (busy) {
            $("#__loading__").css("visibility", "hidden");
        } else {
            $.messager.progress("close");
        }
    }
};
