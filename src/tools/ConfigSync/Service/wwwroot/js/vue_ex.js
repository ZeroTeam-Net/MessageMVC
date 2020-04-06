var vue_obj;
var vue_option = {
    el: '#work_space',
    data: {
        ws_active: false,
        isCollapse: false
    },
    filters: {
        formatDate(time) {
            var date = new Date(time);
            return formatDate(date, 'MM-dd hh:mm:ss');
        },
        formatUnixDate(unix) {
            if (unix === 0)
                return "*";
            var date = new Date(unix * 1000);
            return formatDate(date, 'MM-dd hh:mm:ss');
        },
        formatNumber(number) {
            if (number) {
                return number.toFixed(4);
            } else {
                return "0.0";
            }
        },
        thousandsNumber(number) {
            if (number) {
                return toThousandsInt(number);
            } else {
                return "0";
            }
        },
        formatNumber1(number) {
            if (number) {
                return number.toFixed(4);
            } else {
                return "0.0";
            }
        },
        formatNumber0(number) {
            if (number) {
                return number.toFixed(0);
            } else {
                return "0";
            }
        },
        formatHex(number) {
            if (number) {
                return number.toString(16).toUpperCase();
            } else {
                return "-";
            }
        }
    },
    methods: {
        go_home() {
            location.href = "/Home";
        },
        go_monitor() {
            location.href = "/Monitor";
        },
        go_trace() {
            location.href = "/Flow";
        },
        go_plan() {
            location.href = "/Plan";
        },
        go_doc() {
            location.href = "/Doc";
        },
        go_event() {
            location.href = "/MachineEvent";
        },
        go_github() {
            location.href = "https://github.com/ZeroTeam-Net/MessageMVC";
        }
    }
};

function extend_data(data) {
    vue_option.data = $.extend(vue_option.data, data);
}
function extend_filter(filters) {
    vue_option.filters = $.extend(vue_option.filters, filters);
}
function extend_methods(methods) {
    vue_option.methods = $.extend(vue_option.methods, methods);
}

function ws_state(active) {
    vue_option.data.ws_active = active;
}

/**
 * 更新到vue数组
 * @param {any} array 数组
 * @param {any} item 节点
 * @param {any} key 主键
 */
function vue_update_array(array, item, key) {
    for (var idx = 0; idx < array.length; idx++) {
        var old = array[idx];
        if (old[key] === item[key]) {
            Vue.set(array, idx, item);
            return;
        }
    }
    array.push(item);
}

/**
 * ajax get 操作
 * @param {string} url 地址
 * @param {function} arg 参数
 * @param {string} job 标题
 * @param {function} callback 回调
 */
function do_sync_post(url, arg, job, callback) {
    console.log(url);
    axios
        .post(url, arg)
        .then(resp => {
            var res = resp.data;
            if (res.success) {
                if (callback)
                    callback(res.data);

                vue_obj.$message({
                    title: job,
                    message: '操作成功',
                    type: 'success'
                });
                return;
            }

            vue_obj.$message({
                title: job,
                message: res.message,
                type: 'warning'
            });
        })
        .catch(err => {
            vue_obj.$notify.error({
                title: job,
                message: '请求失败：' + err.status + ',' + err.statusText,
                position: 'bottom-right',
                duration: 2000
            });
        });
}
