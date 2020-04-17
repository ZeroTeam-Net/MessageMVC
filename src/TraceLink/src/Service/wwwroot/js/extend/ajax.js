var hpc_ajax = {
    /**
     * 原始AJAX
     * @param {string} title 任务标题
     * @param {string} api 调用的API
     * @param {string} args 参数
     * @param {Function} onSucceed 成功回调
     * @param {Function} onFailed 失败回调
     * @param {string} host 站点
     */
    post(title, api, args, onSucceed, onFailed, host) {
        common.showBusy(title);
        var url = globalOptions.geturl(api, host);
        console.log(url);
        if (!args)
            args = {};
        args.__page = location.pathname;

        var that = this;
        var serachtimer;
        $.ajax({
            url: url,
            type: 'post',
            dataType: 'json',
            data: args,
            timeout: globalOptions.api.timeout,
            headers: {
                "Authorization": "Bearer " + globalOptions.user.getToken()
            },
            /**
             * 成功回调
             * @param {string} jsonStr 文本
             * @returns {void} 
             */
            success: function (jsonStr) {
                try {
                    if (onSucceed)
                        onSucceed(jsonStr);
                } catch (ex) {
                    console.error(`${url} : ${ex}`);
                    that.showError(title, "结果处理失败!");
                }
            },
            /**
             * 异常回调
             * @param {object} xhr  xhr
             * @param {object} type type
             * @param {object} errorThrown errorThrown
             * @returns {void}  
             */
            error: function (xhr, type, errorThrown) {
                console.log(`${url} : ${type}`);
                clearTimeout(serachtimer);
                serachtimer=setTimeout(function(){
                    common.hideBusy();
                },5000)
                that.showServerNoFind(title);
                if (onFailed)
                    onFailed();
            },
            complete: function () {
                common.hideBusy();
            }
        });
    },

    /**
     * 远程调用
     * @param {string} title 任务标题
     * @param {string} url 调用的API
     * @param {string} args 参数
     * @param {Function} onSucceed 成功回调,注意参数为返回原始内容
     * @param {Function} onFailed 失败回调
     * @param {string} host 站点
     * @param {any} tip 是否显示提示
     */
    call(title, url, args, onSucceed, onFailed, host, tip) {
        if (tip === null || tip === undefined) {
            tip = true;
        }
        var that = this;
        this.post(title, url, args, function (jsonStr) {
            var result = that.evalResult(jsonStr);
            if (!result || !result.success) {
                var st = that.checkApiStatus(url, title, result, tip, () => {
                    that.call(title, url, args, onSucceed, onFailed, host, tip);
                });
                if (st && onFailed)
                    onFailed(result);
            } else if (onSucceed) {
                onSucceed(result);
            }
        }, onFailed, host);
    },
    /**
     * 远程调用(载入值)
     * @param {string} title 任务标题
     * @param {string} url 调用的API
     * @param {string} args 参数
     * @param {Function} onSucceed 成功回调,注意参数为返回的data
     * @param {string} host 站点
     * @param {any} tip 是否显示提示
     */
    load(title, url, args, onSucceed, host) {
        var that = this;
        this.post(title, url, args, function (jsonStr) {
            var result = that.evalResult(jsonStr);
            if (!result || !result.success) {
                that.checkApiStatus(url, title, result, true, () => {
                    that.load(title, url, args, onSucceed, host);
                });
            } else if (onSucceed) {
                onSucceed(result.data);
            }
        }, null, host);
    },

    /**
     * 远程调用(正常结果提示)
     * @param {string} title 任务标题
     * @param {string} url 调用的API
     * @param {string} args 参数
     * @param {Function} onSucceed 成功回调
     * @param {string} host 站点
     */
    operator(title, url, args, onSucceed, host) {
        var that = this;
        this.call(title, url, args, (res) => {
            if (onSucceed)
                onSucceed(res);
            else
                that.showSuccess(title, '操作成功');
        }, (res) => {
            if (res && res.status && res.status.msg)
                that.showError(title, res.status.msg);
            else
                that.showServerNoFind();
        }, host, true);
    },

    /**
     * 执行远程操作,操作显示一个确认框
     * @public 
     * @param {string} title 操作标题
     * @param {string} url 远程URL
     * @param {object} arg 参数
     * @param {function} onSucceed 执行完成后的回调方法
     * @param {string} confirmMessage 确认操作的消息
     * @param {string} host 如Api的站点名称与配置不同,则设置
     * @returns {void} 
     */
    callByConfirm(title, url, arg, onSucceed, confirmMessage, host) {
        if (!confirmMessage)
            confirmMessage = "确定要执行操作吗?";
        var that = this;
        common.confirm(title, confirmMessage, () => {
            that.call(title, url, arg, (res) => {
                if (onSucceed)
                    onSucceed(res);
                else
                    that.showSuccess(title, '操作成功');
            }, (res) => {
                if (res && res.status && res.status.msg)
                    that.showError(title, res.status.msg);
                else
                    that.showServerNoFind();
            }, host, true);
        });
    },
    /**
     * 校验API返回的标准状态
     * @param {string} url 原始URL
     * @param {string} title 任务标题
     * @param {string} result 返回值
     * @param {string} tip 是否显示提示
     * @param {Function} callback 令牌恢复时的回调
     * @returns {boolean} true表示可以继续后续操作,false表示应中断操作
     */
    checkApiStatus(url, title, result, tip, callback) {
        if (!result || !result.status) {
            console.log(`${url} : 无返回内容`);
            if (tip)
                this.showError(title, "发生未知错误，操作失败!");
            return true;
        }
        if (globalOptions.user.checkSysErrorCode(result, callback)) {
            return false;
        }
        if (result.status.code == 404) {
            console.log(`${url} : ${result.status.msg}`);
            if (tip)
                this.showError(title, `接口(${url})不通!`);
        } else if (result.status.msg) {
            console.log(`${url} : ${result.status.msg}`);
            if (tip)
                this.showError(title, result.status.msg + '!');
        } else {
            console.log(`${url} : 无具体消息`);
            if (tip)
                this.showError(title, "发生未知错误，操作失败!");
        }
        return true;
    },
    /**
     * 转换返回值
     * @param {any} result 返回值对象或文本
     * @returns {object} 对象
     */
    evalResult(result) {
        if (!result)
            return null;
        try {
            if (typeof result === "string") {
                console.log(result);
                return eval("(" + result + ")");
            }
        } catch (ex) {
            console.log(ex);
            return null;
        }
        return result;
    },
    /**
     * 显示服务器无法访问
     * @returns {void}
     */
    showServerNoFind() {
        common.showError("服务器无法访问");
    },

    /**
     * 显示成功
     * @param {string} title 标题
     * @param {string} message 消息
     * @returns {void}
     */
    showSuccess(title, message) {
        if (!message)
            message = "操作成功";
        common.showMessage(title, message);
    },
    /**
     * 显示警告
     * @param {string} title 标题
     * @param {string} message 消息
     * @returns {void} 
     */
    showWarning(title, message) {
        common.showMessage(title, message, "warning");
    },

    /**
     * 显示错误
     * @param {string} title 标题
     * @param {string} message 消息
     * @returns {void} 
     */
    showError(title, message) {
        common.showMessage(title, message, "error");
    }
};


/**
 * 原始AJAX
 * @param {string} title 任务标题
 * @param {string} api 调用的API
 * @param {string} args 参数
 * @param {Function} onSucceed 成功回调
 * @param {Function} onFailed 失败回调
 * @param {string} host 站点
 */
function ajax_post(title, api, args, onSucceed, onFailed, host) {
    hpc_ajax.post(title, api, args, onSucceed, onFailed, host);
}

/**
 * 远程调用
 * @param {string} title 任务标题
 * @param {string} url 调用的API
 * @param {string} args 参数
 * @param {Function} onSucceed 成功回调,注意参数为返回原始内容
 * @param {Function} onFailed 失败回调
 * @param {string} host 站点
 * @param {any} tip 是否显示提示
 */
function call_ajax(title, url, args, onSucceed, onFailed, host, tip) {
    hpc_ajax.call(title, url, args, onSucceed, onFailed, host, tip);
}
/**
 * 远程调用(载入值)
 * @param {string} title 任务标题
 * @param {string} url 调用的API
 * @param {string} args 参数
 * @param {Function} onSucceed 成功回调,注意参数为返回的data
 * @param {string} host 站点
 * @param {any} tip 是否显示提示
 */
function ajaxLoadValue(title, url, args, onSucceed, host) {
    hpc_ajax.load(title, url, args, onSucceed, host);
}

/**
 * 远程调用(正常结果提示)
 * @param {string} title 任务标题
 * @param {string} url 调用的API
 * @param {string} args 参数
 * @param {Function} onSucceed 成功回调
 * @param {string} host 站点
 */
function doOperator(title, url, args, onSucceed, host) {
    hpc_ajax.operator(title, url, args, onSucceed, host);
}

/**
 * 远程调用(无提示)
 * @param {string} title 任务标题
 * @param {string} url 调用的API
 * @param {string} args 参数
 * @param {Function} onSucceed 成功回调
 * @param {Function} onFailed 失败回调
 * @param {string} host 站点
 */
function doSilentOperator(title, url, args, onSucceed, onFailed, host) {
    hpc_ajax.call(title, url, args, onSucceed, onFailed, host, false);
}

/**
 * 执行远程操作,操作显示一个确认框
 * @public 
 * @param {string} title 操作标题
 * @param {string} url 远程URL
 * @param {object} arg 参数
 * @param {function} onSucceed 执行完成后的回调方法
 * @param {string} confirmMessage 确认操作的消息
 * @param {string} host 如Api的站点名称与配置不同,则设置
 * @returns {void} 
 */
function call_remote(title, url, arg, onSucceed, confirmMessage, host) {
    hpc_ajax.callByConfirm(title, url, arg, onSucceed, confirmMessage, host);
}

