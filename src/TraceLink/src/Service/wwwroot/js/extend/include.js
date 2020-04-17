var version = 20190417;

var cssFiles = [
    "/styles/css/default.css",
    "/scripts/themes/metro/easyui.css",
    "/scripts/themes/icon.css",
    "/styles/model/Site.css",
    "/styles/model/icon.css"
];
var jsFiles = [
    //JQuery
    "/scripts/jquery-3.3.1.min.js",
    //ueditor
    //"/ueditor/ueditor.config.js",
    //"/ueditor/ueditor.all.min.js",
    //"/ueditor/ueditor.parse.min.js",
    //"/ueditor/lang/zh-cn/zh-cn.js",
    //"/ueditor/third-party/codemirror/codemirror.js",
    //"/ueditor/third-party/zeroclipboard/ZeroClipboard.min.js",
    //EasyUI
    "/scripts/jquery.easyui.min.js",
    "/scripts/datagrid-detailview.js",
    "/scripts/locale/easyui-lang-zh_CN.js",
    //Extend
    "/scripts/extend/core.js",
    "/scripts/extend/button.js",
    "/scripts/extend/ajax.js",
    "/scripts/extend/extend.js",

    "/scripts/extend/page.js",
    "/scripts/extend/dialog.js",
    "/scripts/extend/tree.js",
    "/scripts/extend/grid_panel.js",
    "/scripts/extend/card_panel.js",
    "/scripts/extend/type.js",
    "/scripts/extend/ueditor_ex.js",
    //Business
    "/scripts/business/type.js",
    "/scripts/business/business.js",
    "/scripts/business/userjob.js",
    //"script.js"
];


function dynamicLoad() {
    var host = `http://${location.host}`;
    var loca = host;
    var lo = location.pathname.split('/');
    if (lo.length > 1) {
        for (var i = 0; i < lo.length - 1; i++) {
            loca = loca + lo[i] + '/';
        }
    }
    /**
 * 动态加载CSS
 * @param {string} url 样式地址
 */
    function dynamicLoadCss(url) {
        var head = document.getElementsByTagName('head')[0];
        var link = document.createElement('link');
        link.type = 'text/css';
        link.rel = 'stylesheet';
        link.href = url;
        head.appendChild(link);
    }

    var idx;
    for (idx = 0; idx < cssFiles.length; idx++)
        dynamicLoadCss(`${host}/${cssFiles[idx]}`);

    /**
    * 动态加载JS
    * @param {string} url 脚本地址
    * @param {function} callback  回调函数
    */
    function dynamicLoadJs(url) {
        console.log(url);
        var head = document.getElementsByTagName('head')[0];
        var script = document.createElement('script');
        script.src = url;
        script.type = 'text/javascript';
        script.charset = "utf-8";
        script.onload = script.onreadystatechange = function () {
            console.log(this.readyState);
            if (!this.readyState || this.readyState === "loaded" || this.readyState === "complete") {
                script.onload = script.onreadystatechange = null;
                ++idx;
                if (idx < jsFiles.length)
                    dynamicLoadJs(`${host}${jsFiles[idx]}`);
                else if (idx === jsFiles.length)
                    dynamicLoadJs(`${loca}script.js`);
            }
        };
        head.appendChild(script);
    }

    idx = 0;
    dynamicLoadJs(`${host}/${jsFiles[idx]}`);
}
dynamicLoad();