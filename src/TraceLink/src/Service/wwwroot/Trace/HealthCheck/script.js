/*此标记表明此文件可被设计器更新,如果不允许此操作,请删除此行代码.design by:agebull designer date:2020/4/17 15:42:51*/
function createEntity() {
    return {
        selected: false,
        Id : 0,
        CheckID : 0,
        Service : '',
        Url : '',
        Machine : '',
        Start : '',
        End : '',
        Level : 0,
        Details : ''
    };
}
extend_methods({
    getDef() {
        return createEntity();
    },
    checkListData(row) {
        if (typeof row.Id === 'undefined')
            row.Id = 0;
        if (typeof row.CheckID === 'undefined')
            row.CheckID = 0;
        if (typeof row.Service === 'undefined')
            row.Service = '';
        if (typeof row.Url === 'undefined')
            row.Url = '';
        if (typeof row.Machine === 'undefined')
            row.Machine = '';
        if (typeof row.Start === 'undefined')
            row.Start = '';
        if (typeof row.End === 'undefined')
            row.End = '';
        if (typeof row.Level === 'undefined')
            row.Level = 0;
        if (typeof row.Details === 'undefined')
            row.Details = '';
    }
});
var rules = {
    'CheckID':[{ required: true, message: '请输入检查标识', trigger: 'blur' }],
    'Level':[{ required: true, message: '请输入健康等级', trigger: 'blur' }]
};



function doReady() {
    try {
        vue_option.data.apiPrefix = 'trace_link/healthCheck/v1';
        vue_option.data.idField = 'Id';
        vue_option.data.form.def = createEntity();
        vue_option.data.form.data = createEntity();
        vue_option.data.form.rules = rules; 
        globalOptions.api.customHost = globalOptions.api.trace_linkApiHost;
        vueObject = new Vue(vue_option);
        vue_option.methods.loadList();
        
    } catch (e) {
        console.error(e);
    }
}
doReady();