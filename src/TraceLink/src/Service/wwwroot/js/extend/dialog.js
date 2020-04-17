



/*
对话框方便
*/
function openDialog(dlgid, title) {
    $(dlgid).dialog("open").dialog("setTitle", title);
}

function closeDialog(dlgid) {
    $(dlgid).dialog("close");
}

function loadPanel(id, url, wid, hei, cached, onload) {
    $(id).panel({
        width: wid,
        height: hei,
        border: false,
        cache: cached,
        loadingMessage: "正在载入……",
        href: url,
        onLoad: onload
    });
}

function loadPanel2(id, url, onload) {
    $(id).panel({
        width: "auot",
        height: "auot",
        border: false,
        cache: false,
        loadingMessage: "正在载入……",
        href: url,
        onLoad: function () {
            if (onload)
                onload();
        }
    });
}
