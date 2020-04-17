/*此标记表明此文件可被设计器更新,如果不允许此操作,请删除此行代码.design by:agebull designer date:2020/4/16 23:46:27*/
function createEntity() {
    return {
        selected: false,
        Id : 0,
        TraceId : '',
        Level : 0,
        ApiName : '',
        Start : '',
        End : '',
        LocalId : '',
        LocalApp : '',
        LocalMachine : '',
        CallId : '',
        CallApp : '',
        CallMachine : '',
        Context : '',
        Token : '',
        Headers : '',
        Message : '',
        FlowStep : ''
    };
}
extend_methods({
    getDef() {
        return createEntity();
    },
    checkListData(row) {
        if (typeof row.Id === 'undefined')
            row.Id = 0;
        if (typeof row.TraceId === 'undefined')
            row.TraceId = '';
        if (typeof row.Level === 'undefined')
            row.Level = 0;
        if (typeof row.ApiName === 'undefined')
            row.ApiName = '';
        if (typeof row.Start === 'undefined')
            row.Start = '';
        if (typeof row.End === 'undefined')
            row.End = '';
        if (typeof row.LocalId === 'undefined')
            row.LocalId = '';
        if (typeof row.LocalApp === 'undefined')
            row.LocalApp = '';
        if (typeof row.LocalMachine === 'undefined')
            row.LocalMachine = '';
        if (typeof row.CallId === 'undefined')
            row.CallId = '';
        if (typeof row.CallApp === 'undefined')
            row.CallApp = '';
        if (typeof row.CallMachine === 'undefined')
            row.CallMachine = '';
        if (typeof row.Context === 'undefined')
            row.Context = '';
        if (typeof row.Token === 'undefined')
            row.Token = '';
        if (typeof row.Headers === 'undefined')
            row.Headers = '';
        if (typeof row.Message === 'undefined')
            row.Message = '';
        if (typeof row.FlowStep === 'undefined')
            row.FlowStep = '';
    }
});




function doReady() {
    try {
        vue_option.data.apiPrefix = 'trace_link/message/v1';
        vue_option.data.idField = 'Id';
        vue_option.data.form.def = createEntity();
        vue_option.data.form.data = createEntity();
        
        globalOptions.api.customHost = globalOptions.api.trace_linkApiHost;
        vueObject = new Vue(vue_option);
        vue_option.methods.loadList();
        
    } catch (e) {
        console.error(e);
    }
}
doReady();