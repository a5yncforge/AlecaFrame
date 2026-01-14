google.charts.load('current', { 'packages': ['corechart'] });

var defaultGooglePieChartOptions = {
    width: 243,
    height: 243,

    textStyle: { color: '#FFF' },
    colors: ['#4b63cb', '#4a9424', '#393d4a', '#eba000', '#c65373', '#831a90', '#5295c6'],

    chartArea: {
        //   top: 0,
        width: 230,
        height: 230,
    },
    legend: {
        position: 'none',
        alignment: 'center',
        maxLines: 5,
        textStyle: { color: '#FFF' },
    },
    backgroundColor: {
        fill: '#FF0000',
        fillOpacity: 0
    },
};


var proAnalyticsApp = Vue.createApp({
    data() {
        return {
            gotPatreon: false,
            data: {},
            loading: false,
            filters: {
                bestItemsOrder: "value",
                topSalesOrder: "value",
                topPurchasesOrder: "value",
                tradePartnersOrder: "saleValue",
                statementSummaryOrder: "profit"
            },
            refreshedAtLeastOnce: false
        }
    },
    mounted: function () {
    },
    methods: {
        refreshIfNecessary() {
            if (!this.refreshedAtLeastOnce) {
                this.refreshedAtLeastOnce = true;
                this.refresh();
            }
        },
        refresh() {

            if (!this.unlocked) return;

            let filtersJSON = JSON.stringify(this.filters);
            this.loading = true;
            plugin.get().GetProAnalytics(filtersJSON, (success, data) => {

                setTimeout(() => {
                    this.loading = false;
                }, 150);
                if (success) {

                    this.data = JSON.parse(data);                 

                    var bestItemsChartData = [];
                   
                    for (var i = 0; i < this.data.bestItemsForTradingCategories.length; i++) {
                        var item = this.data.bestItemsForTradingCategories[i];
                        bestItemsChartData.push([item.name, item.value]);
                    }
                    bestItemsChartData.sort((a, b) => (a[0] > b[0]) ? 1 : -1);
                    bestItemsChartData.unshift(['Type', 'Value']);
                    var chart = new google.visualization.PieChart(document.getElementById('proAnalyticsBestItemsPie'));
                    chart.draw(google.visualization.arrayToDataTable(bestItemsChartData), defaultGooglePieChartOptions);


                    var statementChartData = [];
                    
                    for (var i = 0; i < this.data?.proAnalyticsStatement?.statementByType.length; i++) {
                        var item = this.data?.proAnalyticsStatement?.statementByType[i];
                        statementChartData.push([item.type, Math.max(item.graphValue, 0)]);
                    }
                    statementChartData.sort((a, b) => (a[0] > b[0]) ? 1 : -1);
                    statementChartData.unshift(['Type', 'Value']);
                   
                    var chart2 = new google.visualization.PieChart(document.getElementById('proAnalyticsStatementPie'));
                    chart2.draw(google.visualization.arrayToDataTable(statementChartData), defaultGooglePieChartOptions);


                } else {
                    showErrorToast("Failed to get pro analytics: " + data);
                }
            });
        },
        filtersChanged() {
            this.refresh();
        },
        toSI(number) {
            if (number > 1000000) {
                return (number / 1000000).toFixed(2) + "M";
            } else if (number > 1000) {
                return (number / 1000).toFixed(1) + "K";
            } else if (number == undefined || number == 0) {
                return "-";
            }
            else {
                return number;
            }

        }
    },
    computed: {
        unlocked: function () {
            return this.gotPatreon;
        }
    }

}).mount("#proAnalyticsTab");
