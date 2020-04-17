
var vueObject;
var mq_socket;
var tenChart = null;

function updateChart(data, oldData) {

    for (var i = 0; i < data.length; i++) {
        if (oldData.length <= i)
            oldData.push(data[i]);
        else {
            oldData[i].update(data[i]);
        }
    }
}
function report(idx, msg, text) {
    //if (tenChart) {
    //    updateChart(data.series[0].data, tenChart.series[0].data);
    //    updateChart(data.series[1].data, tenChart.series[1].data);
    //    return;
    //}
    var data = msg.data;
    Highcharts.chart(`container${idx}`, {
        chart: {
            zoomType: 'xy'
        },
        title: {
            text: text
        },
        xAxis: [{
            categories: data.categories,
            crosshair: true
        }],
        yAxis: [{
            title: {
                text: '金额',
                style: {
                    color: Highcharts.getOptions().colors[0]
                }
            },
            labels: {
                style: {
                    color: Highcharts.getOptions().colors[0]
                },
                formatter() {
                    return `${this.value / 100} 万元`;
                }
            },
            opposite: true,
            allowDecimals: true
        }, {
            labels: {
                format: '{value} 人次',
                style: {
                    color: Highcharts.getOptions().colors[1]
                },
                formatter() {
                    return `${this.value} 人次`;
                }
            },
            title: {
                text: '进店',
                style: {
                    color: Highcharts.getOptions().colors[1]
                }
            },
            allowDecimals: true
        }, {
            title: {
                text: '订单',
                style: {
                    color: Highcharts.getOptions().colors[2]
                }
            },
            labels: {
                style: {
                    color: Highcharts.getOptions().colors[2]
                },
                formatter() {
                    return `${this.value} 笔`;
                }
            },
            opposite: true,
            allowDecimals: true
        }],
        tooltip: {
            shared: true, //是否共享提示，也就是X一样的所有点都显示出来
            useHTML: true, //是否使用HTML编辑提示信息
            formatter: function () {
                var label = msg.memo;
                if (!label)
                    for (var idx = 0; idx < data.categories.length; idx++) {
                        if (data.categories[idx] === this.x) {
                            label = data.series[1].label[idx];
                            break;
                        }
                    }
                return `<small>${this.x}</small><table style='minwidth:300px'>
<tr><td style="color: ${this.points[0].color}">金额 : ${this.points[0].y / 100}万元</td></tr>
<tr><td style="color: ${this.points[1].color}">进店 : ${this.points[1].y}人次</td></tr>
<tr><td style="color: ${this.points[2].color}">订单 : ${this.points[2].y}笔</td></tr>
<tr><td >${label}</td></tr>
</table>`;
            },
            valueDecimals: 2 //数据值保留小数位数
        },
        legend: {
            layout: 'vertical',
            align: 'left',
            x: 120,
            verticalAlign: 'top',
            y: 100,
            floating: true,
            backgroundColor: (Highcharts.theme && Highcharts.theme.legendBackgroundColor) || '#FFFFFF'
        },
        series: data.series
    });
}
var vue_option = {
    el: '#work_space',
    data: {
        col: {
            totalReg: 0,
            totalJoin: 0,
            totalOrder: 0,
            totalPay: 0,
            todayReg: 0,
            todayJoin: 0,
            todayOrder: 0,
            todayPay: 0
        }
    },
    filters: {
        formatMoney(number) {
            if (number) {
                return "￥" + number.toFixed(2);
            } else {
                return "￥0.00";
            }
        }
    },
    methods: {
    }
};

function on_mq_push(msg) {
    try {
        switch (msg.sub) {
            case "report":
                report(0, msg, "全局走势");
                break;
            case "real":
                vue_option.data.col = msg.data;
                break;
            default:
                report(msg.sub, msg, msg.title);
        }
    } catch (e) {
        console.error(e);
    }
}

function ready() {
    vueObject = new Vue(vue_option);
    mq_socket = new ws({
        address: "ws://" + location.host + "/mq",
        sub: "*",
        onmessage: on_mq_push
    });
    mq_socket.open();
    // real_line();
}

ready();