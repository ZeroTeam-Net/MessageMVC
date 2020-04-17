
function showServerNoFind(title) {
    showTip(title, "服务器无法访问");
}

function showSuccess(message) {
    if (!message)
        message = "操作成功";
    vue_option.data.message = message;
    vueObject.$message({
        message: message,
        type: "success"
    });
}

/**
 * 显示提示（右下角自动隐藏那种）
 * @param {string} title 标题
 * @param {string} message 消息
 * @returns {void} 
 */
function showTip(title, message) {
    vue_option.data.message = message;
    vueObject.$message(message);
}

/**
 * 显示警告（右下角自动隐藏那种）
 * @param {string} message 消息
 * @returns {void} 
 */
function showWarning(message) {
    vue_option.data.message = message;
    vueObject.$message({
        message: message,
        type: "warning"
    });
}

/**
 * 显示提示（对话框类型）
 * @param {string} title 标题
 * @param {string} message 消息
 * @returns {void} 
 */
function showMessage(title, message) {
    vue_option.data.message = message;
    vueObject.$message.error(message);
}

globalOptions.api.customHost = "Import/";

extend_data({
    id: null,
    name: null,
    title: null,
    step: 0,
    state: "process",
    stepStates: [],
    fileName: null,
    oid: null,
    sid: null,
    orgName: null,
    orgList: [],
    intask: false,
    file: null,
    csvurl: null,
    message: "",
    real_message: "",
    info: null,
    errors: [],
    similars: []
});

function stateString(state) {
    switch (state) {
    case 0:
        return "process";
    case 1:
        return "process";
    case 2:
        return "error";
    case 3:
        return "success";
    }
    return "wait";
}

function syncTask(task, reset) {
    var localTask = vue_option.data;
    if (!task) {
        vue_option.data.intask = false;
        localTask.id = null;
        localTask.name = null;
        localTask.title = null;
        localTask.step = 0;
        localTask.state = "wait";
        localTask.stepStates = [];
        localTask.fileName = null;
        localTask.oid = 0;
        localTask.orgName = null;
        localTask.sid = null;
        localTask.desc = null;
    } else {
        vue_option.data.intask = true;
        localTask.id = task.id;
        localTask.name = task.name;
        localTask.title = task.title;
        localTask.step = task.step;
        localTask.state = stateString(task.state);
        var stepStates = [];
        if (task.stepStates) {
            for (var idx = 0; idx < task.stepStates.length; idx++) {
                stepStates.push(stateString(task.stepStates[idx]));
            }
        }
        localTask.stepStates = stepStates;
        localTask.fileName = task.fileName;
        localTask.oid = task.oid;
        localTask.orgName = task.orgName;
        localTask.sid = task.sid;
        localTask.desc = task.desc;
    }
    if (reset) {
        vue_option.data.file = null;
        vue_option.data.csvurl = null;
        vue_option.data.real_message = "";
        vue_option.data.message = "";
        vue_option.data.real_message = "";
        vue_option.data.info = null;
        vue_option.data.errors = [];
        vue_option.data.similars = [];
    }
}

function showRequest() {
    downloadFile(null);
    vue_option.data.message = "正在发送请求...";
    vue_option.data.info = null;
    vue_option.data.real_message = null;
    vue_option.data.errors = [];
    vue_option.data.similars = [];
}

function showStatus(res) {
    vue_option.data.message =
        res.status && res.status.msg
        ? res.status.msg
        : res.success
        ? "请求成功"
        : "网络错误";
}

extend_methods({
    start: function() {
        try {
            showRequest();
            call_ajax("执行",
                "v1/task/do",
                { arg1: vue_option.data.name },
                function(res) {
                    showStatus(res);
                    if (res.success && res.data) {
                        syncTask(res.data);
                        if (vue_option.data.step === 1) {
                            loadOrgList();
                        }
                    }
                });
        } catch (e) {
            console.error(e);
        }
    },
    test: function() {
        downloadFile("test.txt", "test text");
    },
    next: function() {
        try {
            showRequest();
            if (vue_option.data.step === 1) {
                call_ajax("执行",
                    "v1/task/do",
                    { arg1: vue_option.data.oid },
                    function(res) {
                        showStatus(res);
                        if (res.success && res.data) {
                            syncTask(res.data);
                        }
                    });
                return;
            }

            if (vue_option.data.step === 6) {
                vueObject.$confirm("是否确认你的操作?",
                    "提示",
                    {
                        confirmButtonText: "确定",
                        cancelButtonText: "取消",
                        type: "warning"
                    }).then(() => {
                    call_ajax("执行",
                        "v1/task/do",
                        null,
                        function(res) {
                            showStatus(res);
                            if (res.success && res.data) {
                                syncTask(res.data);
                            }
                        });
                });
                return;
            }
            if (vue_option.data.step === 3 || vue_option.data.step === 4)
                call_ajax("下一步",
                    "v1/task/next",
                    null,
                    function(res) {
                        showStatus(res);
                        if (res.success && res.data) {
                            syncTask(res.data);
                        }
                    });
            else
                call_ajax("执行",
                    "v1/task/do",
                    { arg1: vue_option.data.csvurl, arg2: vue_option.data.fileName },
                    function(res) {
                        showStatus(res);
                        if (res.success && res.data) {
                            syncTask(res.data);
                        }
                    });
        } catch (e) {
            console.error(e);
        }
    },
    execStep: function() {
        vueObject.$confirm("是否确认你的操作?",
            "提示",
            {
                confirmButtonText: "确定",
                cancelButtonText: "取消",
                type: "warning"
            }).then(() => {
            try {
                showRequest();
                vue_option.data.state = "process";
                call_ajax("执行",
                    "v1/task/do",
                    { arg1: vue_option.data.csvurl },
                    function(res) {
                        showStatus(res);
                        if (res.success && res.data) {
                            syncTask(res.data);
                        }
                    });
            } catch (e) {
                console.error(e);
            }
        });
    },
    close: function() {
        try {

            showRequest();
            call_ajax("载入",
                "v1/task/cancel",
                null,
                function(res) {
                    try {
                        showStatus(res);
                        if (res.success) {
                            syncTask(null, true);
                        }
                    } catch (er) {
                        console.error(er);
                    }
                });
        } catch (e) {
            console.error(e);
        }
    },
    create: function() {
        try {
            showRequest();
            call_ajax("载入",
                "v1/task/create",
                null,
                function(res) {
                    showStatus(res);
                    if (res.success && res.data) {
                        syncTask(res.data, true);
                    }
                });
        } catch (e) {
            console.error(e);
        }
    },
    addName: function(row) {
        try {
            vue_option.data.message = "正在发送请求...";
            call_ajax("载入",
                "v1/task/newName",
                { type: row.type, name: row.name },
                function(res) {
                    showStatus(res);
                });
        } catch (e) {
            console.error(e);
        }
    },
    handlePreview(file) {
        vue_option.data.file = file;
    },
    requestFile: doUpload
});

function doUpload() {
    try {
        if (!vue_option.data.file) {
            vue_option.data.message = "请先选择文件";
            vueObject.$message.error("请先选择文件");
            return;
        }
        vue_option.data.message = "正在上传文件...";
        var upUrl = "/api/v1/ueditor/action?action=uploadtext";
        var file = vue_option.data.file;
        vue_option.data.fileName = file.name;
        console.log("name:" + file.name + " size:" + file.size + " type:" + file.type);
        var form = new FormData(); // FormData 对象
        form.append("upfile", file.raw); // 文件对象
        var xhr = new XMLHttpRequest(); // XMLHttpRequest 对象
        xhr.onreadystatechange = function() {

            switch (xhr.readyState) {
            default:
                return;
            case 4:
                break;
            }
            try {
                if (xhr.status !== 200) {
                    vue_option.data.message = "网络异常";
                    vueObject.$message.error("网络异常");
                    return;
                }
                var result = eval("(" + xhr.responseText + ")");
                if (result.error === "0x0") {
                    vue_option.data.message = "上传成功:" + result.url;
                    vue_option.data.csvurl = result.url;
                    vueObject.$message({
                        message: "上传成功",
                        type: "success"
                    });
                    vue_option.data.file = null;
                } else {
                    vue_option.data.message = "上传失败:" + result.state;
                    vueObject.$message.error("上传失败:" + result.state);
                }
            } catch (ex) {
                console.error(ex);
            }
        };
        xhr.open("post", upUrl, true);
        xhr.send(form);
    } catch (e) {
        console.error(e);
    }
}

var mq_socket = new ws({
    address: "ws://" + location.host + "/mq",
    sub: globalOptions.user.getAccessToken(),
    onmessage: onUserMessage
});

function onUserMessage(res) {
    if (!res.status)
        return;
    switch (res.status.guide) {
    case "0":
        vue_option.data.real_message = res.status.msg;
        break;
    case "1":
        vue_option.data.info = res.data;
        break;
    case "2":
        vue_option.data.info = res.data;
        showStatus(res);
        vue_option.data.state = res.status.msg === "Success" ? "success" : "error";
        vueObject.$message({
            message: "后台操作结束",
            type: res.status.msg === "Success" ? "success" : "error"
        });
        break;
    case "3":
        if (vue_option.data.errors.length < 100)
            vue_option.data.errors.push(res.status.msg);
        vue_option.data.real_message = res.status.msg;
        break;
    case "4":
        vue_option.data.similars.push({
            type: res.status.point,
            name: res.status.msg
        });
        break;
    case "5":
        downloadFile(res.status.msg);
        break;
    }
}

function loadTask() {
    call_ajax("载入",
        "v1/task/get",
        {},
        function(res) {
            showStatus(res);
            if (res.success && res.data) {
                syncTask(res.data, true);
            }
        });
}

function loadOrgList() {
    call_ajax("执行",
        "v1/task/org",
        null,
        function(re2) {
            showStatus(re2);
            if (re2.success)
                vue_option.data.orgList = re2.data;
        });
}


function doReady() {
    try {
        vueObject = new Vue(vue_option);

        showRequest();

        mq_socket.open();

        loadTask();

        loadOrgList();
    } catch (e) {
        console.error(e);
    }
}

doReady();


function downloadFile(content) {
    var aLink = document.getElementById("test");
    if (!aLink)
        return;
    if (!content) {
        aLink.innerText = "-";
        aLink.href = null;
        aLink.download = null;
        return;
    }
    var bytes = "\uFEFF" + content;
    aLink.innerText = "查看文件";
    var blob = new Blob([bytes], { type: "application/vnd.ms-excel", endings: "native" });
    aLink.download = vue_option.data.fileName;
    aLink.href = URL.createObjectURL(blob);
}