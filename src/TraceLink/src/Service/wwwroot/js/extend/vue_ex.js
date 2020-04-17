
var vueObject;
var vue_option = {
    el: '#work_space',
    data: {
        /**
         * API前缀
         */
        apiPrefix: '',
        ws_active: false,
        idField: "id",
        stateData: false,
        historyData: false,
        auditData: false,
        currentRow: null,
        list: {
            field: '_any_',
            keyWords: null,
            dataState: -1,
            audit: -1,
            sort: null,
            order: null,
            rows: [],
            page: 1,
            pageSize: 15,
            pageSizes: [15, 20, 30, 50, 100],
            pageCount: 0,
            total: 0,
            multipleSelection: []
        },
        form: {
            readonly: false,
            visible: false,
            loading: false,
            edit: false,
            data: null,
            rules: null
        }
    },
    filters: {
        timeFn(startTime,endTime) {
            if(!startTime && !endTime){
                return;
            }
            var startTime = NewDate(startTime).format("yyyy-MM-dd hh:mm:ss");
            var endTime = NewDate(endTime).format("yyyy-MM-dd hh:mm:ss");
            var dateDiff = Math.abs(new Date(endTime).getTime() - new Date(startTime).getTime());//时间差的毫秒数
            var dayDiff = Math.floor(dateDiff / (24 * 3600 * 1000));//计算出相差天数
            var leave1=dateDiff%(24*3600*1000)    //计算天数后剩余的毫秒数
            var hours=Math.floor(leave1/(3600*1000))//计算出小时数
            var leave2=leave1%(3600*1000)    //计算小时数后剩余的毫秒数
            var minutes=Math.floor(leave2/(60*1000))//计算相差分钟数
            var leave3=leave2%(60*1000)      //计算分钟数后剩余的毫秒数
            var seconds=Math.round(leave3/1000)
    
            var a="",b="",c="",d="";
            if(dayDiff>0){
               a = dayDiff+"天" ;
            }
            if(hours>0){
               b = hours+"时" ;
            }
            if(minutes>0){
               c = minutes+"分" ;
            }
            if(seconds>0){
               d = seconds+"秒" ;
            }
            if(a+b+c+d){
                return a+b+c+d;
            }else{
                return "0";
            }
        },
        boolFormater(b) {
            return b ? "是" : "否";
        },
        formatTime(time) {
            return !time ? null : NewDate(time).format("yyyy-MM-dd hh:mm:ss");
        },
        formatDate(date) {
            return !date ? null : NewDate(date).format("yyyy-MM-dd");
        },
        emptyNumber(number) {
            if (number) {
                return number;
            } else {
                return "-";
            }
        },
        formatUnixDate(unix) {
            if (unix === 0)
                return "";
            return new Date(unix * 1000).format("MM-dd hh:mm:ss");
        },
        formatNumber(number) {
            if (number) {
                return number.toFixed(4);
            } else {
                return "0.0";
            }
        },
        formatNumber2(number) {
            if (number) {
                return number.toFixed(2);
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
        formatMoney(number) {
            if (number) {
                return "￥" + number.toFixed(2);
            } else {
                return "￥0.00";
            }
        },
        formatNumber1(number) {
            if (number) {
                return number.toFixed(1);
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
        },
        dataStateIcon(dataState) {
            switch (dataState) {
                case 0:
                    return "el-icon-edit";
                case 1:
                    return "el-icon-circle-check";
                case 2:
                    return "el-icon-remove-outline";
                case 0xE:
                    return "el-icon-user";
                case 0xF:
                    return "icon_a_end";
                case 0x10:
                    return "el-icon-delete";
                case 255:
                    return "el-icon-close";
            }
            return "el-icon-question";
        },
        dataStateIcon2(dataState) {
            switch (dataState) {
                case 0:
                    return "el-icon-error";
                case 1:
                    return "el-icon-success";
                case 2:
                    return "el-icon-remove";
            }
            return "el-icon-question";
        },
        formatAudit(value) {
            return arrayFormat(value, auditType);
        }
    },
    methods: {
        doQuery() {
            vue_option.data.list.rows = [];
            vue_option.data.list.page = 0;
            vue_option.data.list.pageCount = 0;
            vue_option.data.list.total = 0;
            this.loadList();
        },
        loadList(callback) {
            var that = this;
            var arg = this.getQueryArgs();
            call_ajax("载入列表数据",
                `${vue_option.data.apiPrefix}/edit/list`,
                arg,
                function (result) {
                    if (result.success) {
                        that.onListLoaded(result.data);
                        if (typeof callback === "function")
                            callback(result.data);
                        else {
                            vue_option.data.list.rows = result.data.rows;
                            vue_option.data.list.page = result.data.page;
                            vue_option.data.list.pageSize = result.data.pageSize;
                            vue_option.data.list.pageCount = result.data.pageCount;
                            vue_option.data.list.total = result.data.total;
                        }
                    }
                    else {
                        showStatus(result);
                    }
                });
        },
        getQueryArgs() {
            return {
                _field_: vue_option.data.list.field,
                _value_: vue_option.data.list.keyWords,
                _dataState_: vue_option.data.list.dataState,
                _audit_: vue_option.data.list.audit,
                page: vue_option.data.list.page,
                rows: vue_option.data.list.pageSize,
                sort: vue_option.data.list.sort,
                order: vue_option.data.list.order
            };
        },
        onListLoaded(data) {
            if (data.rows) {
                for (var idx = 0; idx < data.rows.length; idx++) {
                    var row = data.rows[idx];
                    if (!row.selected)
                        row.selected = false;
                    this.checkListData(row);
                }
            }
            else {
                data.rows = [];
            }
        },
        checkListData(row) {
        },
        dblclick(row, column, event) {
            this.currentRow = row;
            this.doEdit();
        },
        sizeChange(size) {
            vue_option.data.list.pageSize = size;
            this.loadList();
        },
        pageChange(page) {
            vue_option.data.list.page = page;
            this.loadList();
        },
        onSort(arg) {
            vue_option.data.list.sort = arg.prop;
            vue_option.data.list.order = arg.order === "ascending" ? "asc" : "desc";
            this.loadList();
        },
        currentRowChange(row) {
            this.currentRow = row;
        },
        selectionRowChange(val) {
            this.list.multipleSelection = val;
            console.log(this.list.multipleSelection);
        },
        doAddNew() {
            var data = this.form;
            data.data = this.getDef();
            data.readonly = false;
            data.edit = false;
            data.visible = true;
        },
        getDef() {
            return {};
        },
        doEdit() {
            if (this.list.multipleSelection.length != 1) {
                this.$message.error('请单击一行');
                return;
            }
            var data = this.form;
            data.data = this.list.multipleSelection[0];
            data.readonly = this.checkReadOnly(data.data);
            data.edit = true;
            data.visible = true;
        },
        checkReadOnly(row) {
            return !row || row.auditState && row.auditState >= 2 || row.isFreeze || row.dataState && row.dataState !== 0;
        },
        save() {
            var that = this;
            var data = that.form;
            this.$refs['dataForm'].validate((valid) => {
                if (!valid) {
                    that.$message.error('内容不合理');
                    return false;
                }
                data.loading = true;
                call_ajax("执行", data.edit ? `${vue_option.data.apiPrefix}/edit/update?id=${data.data[that.idField]}` : `${vue_option.data.apiPrefix}/edit/addnew`, data.data, function (result) {
                    data.loading = false;
                    if (result.success) {
                        that.$message({
                            message: '操作成功',
                            type: 'success'
                        });
                        data.visible = false;
                        that.loadList();
                    }
                    else {
                        that.$message.error('操作失败:' + result.status.msg);
                    }
                    data.loading = false;
                }, function (result) {
                    data.loading = false;
                    data.visible = false;
                    that.$message.error(result && result.status && result.status.msg ? result.status.msg : '更新失败');
                });
                return true;
            });
        },
        doDelete() {
            var me = this;
            this.mulitSelectAction("删除", `${vue_option.data.apiPrefix}/edit/delete`, function (row) {
                if (!me.stateData || row.dataState === 255)
                    return true;
                if (row.dataState !== 0)
                    return false;
                if (me.auditData) {
                    return row.auditState === 0;
                } return true;
            });
        },
        doEnable() {
            var me = this;
            this.mulitSelectAction("启用", `${vue_option.data.apiPrefix}/state/enable`, function (row) {
                if (me.auditData) {
                    return row.auditState === 4 && row.dataState !== 1 && row.dataState < 0x10;//&& !row.IsFreeze;
                } else
                    return row.dataState !== 1 && row.dataState < 0x10;// && !row.IsFreeze;
            });
        },
        doDisable() {
            var me = this;
            this.mulitSelectAction("禁用", `${vue_option.data.apiPrefix}/state/disable`, function (row) {
                if (me.auditData) {
                    return row.auditState === 4 && row.dataState === 1;//&& !row.IsFreeze;
                } else return row.dataState === 1;// && !row.IsFreeze;
            });
        },
        doDiscard() {
            var me = this;
            this.mulitSelectAction("废弃", `${vue_option.data.apiPrefix}/state/discard`, function (row) {
                if (row.dataState !== 0)
                    return false;
                if (me.auditData) {
                    return row.auditState <= 1;
                } return true;
            });
        },
        doReset() {
            this.mulitSelectAction("重置", `${vue_option.data.apiPrefix}/state/reset`, function (row) {
                return row.dataState !== 0;
            });
        },
        doLock() {
            var me = this;
            this.mulitSelectAction("锁定", `${vue_option.data.apiPrefix}/state/lock`, function (row) {
                if (me.auditData) {
                    return row.auditState === 3;
                } else return row.dataState < 0x10;
            });
        },
        doPass() {
            var me = this;
            this.mulitSelectAction("审核通过", `${vue_option.data.apiPrefix}/audit/pass`, function (row) {
                if (me.auditData) {
                    return row.auditState <= 1;
                } else return row.dataState < 0x10;
            });
        },
        doReDo() {
            var me = this;
            this.mulitSelectAction("恢复到未审核状态", `${vue_option.data.apiPrefix}/audit/redo`, function (row) {
                if (me.auditData) {
                    return row.auditState > 1;
                } else return row.dataState < 0x10;
            });
        }, 
        getSelectedRows(filter) {
            var rows = this.list.multipleSelection;
            if (!rows || rows.length === 0)
                return null;
            var ids = null;
            for (var idx = 0; idx < rows.length; idx++) {
                if (!filter || filter(rows[idx])) {
                    if (!ids)
                        ids = rows[idx][this.idField];
                    else
                        ids += `,${rows[idx][this.idField]}`;
                }
            }
            return ids;
        },
        mulitSelectAction(title, api, filter, arg, callback) {
            var ids = this.getSelectedRows(filter);
            if (!ids) {
                this.$message.error('请选择一或多行数据');
                return;
            }
            var that = this;
            vueObject.$confirm(`你确定要${title}所选择的数据吗?`, title, {
                confirmButtonText: '确定',
                cancelButtonText: '取消',
                type: 'warning'
            }).then(() => {
                if (!arg)
                    arg = { selects: ids };
                else
                    arg.selects = ids;
                call_ajax(title, api, arg, function (result) {
                    if (result.success) {
                        that.$message({
                            message: result.status && result.status.msg ? result.status.msg : '操作成功',
                            type: 'success'
                        });
                        if (callback)
                            callback(result);
                        else
                            that.loadList();
                    }
                    else {
                        that.$message.error('操作失败:' + result.status.msg);
                    }
                }, function (result) {
                    that.$message.error(result && result.status ? result.status.msg : '操作失败' );
                });
            });
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

ws_state = function (active) {
    vue_option.data.ws_active = active;
};

function do_get(url, that, callback) {
    $.get(url).then(data => {
        try {
            if (data.success) {
                callback();
                that.$message({
                    message: '操作成功',
                    type: 'success'
                });
            }
            else {
                that.$message.error(`操作失败:${data.status.msg}`);
            }
        } catch (e) {
            that.$message.error(`操作失败${e}`);
        }
    }, e => {
        that.$message.error(`操作失败${e}`);
    }).catch(e => {
        that.$message.error(`操作失败${e}`);
    });

}
