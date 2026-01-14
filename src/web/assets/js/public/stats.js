
var forceWeb = false;
var forcedUID = "";
var forcedPublicToken = "";
//EMPTY TO DISABLE!

var statsInApp = typeof overwolf != 'undefined' && !forceWeb;
console.log("Stats running in app: " + statsInApp);


Chart.defaults.responsive = true;
Chart.defaults.maintainAspectRatio = false;
Chart.defaults.color = '#d4d3d2';
//Chart.defaults.plugins.legend.display = false;
Chart.defaults.parsing = false;

var statsChartObjects = [];

let statsPUBLIC_ENUM_None = 0;
let statsPUBLIC_ENUM_Trades = 1;
let statsPUBLIC_ENUM_Platinum = 2;
let statsPUBLIC_ENUM_Ducats = 4;
let statsPUBLIC_ENUM_Endo = 8;
let statsPUBLIC_ENUM_Credits = 16;
let statsPUBLIC_ENUM_AccountData = 32;
let statsPUBLIC_ENUM_Aya = 64;

if(statsInApp){
    plugin.get().onStatsUpdateNeeded.removeListener(reloadStatsTabEvent);
    plugin.get().onStatsUpdateNeeded.addListener(reloadStatsTabEvent);
}


function reloadStatsTabEvent(temp) {
    if (statsApp.refreshedAtLeastOnce) {
        statsApp.refresh();
    }
}

const legendHeightPlugin = {
    beforeInit(chart) {
        // Get reference to the original fit function
        const originalFit = chart.legend.fit;

        // Override the fit function
        chart.legend.fit = function fit() {
            // Call original function and bind scope in order to use `this` correctly inside it
           // this.right = 300; 
           // this._margins.right = 300;
            originalFit.bind(chart.legend)();
            // Change the height as suggested in another answers
            this.height += 15;
        }
    },
}

function ConvertToSI(input) {
    return Math.abs(input) >= 1000000 ? (input / 1000000).toFixed(1) + 'M' : Math.abs(input) >= 1000 ? (input / 1000).toFixed(1) + 'K' : input;
}

var lastStatsItemTooltips;

statsApp = Vue.createApp({
    data() {
        return {
            fromOverwolf: statsInApp,

            userNotReady: false,
            error: false,
            loading: false,

            notEnabled: false,
            tradeNotReady: false,

            refreshedAtLeastOnce: false,

            statsPanelShown : true,

            timeframe: 30,
            totalSampleCount: 30,           

            charts: [
                { name: "Platinum", icon: "platinum", type: "normal", getFunc: (dayData) => dayData?.plat ?? 0, featurePublicENUM: statsPUBLIC_ENUM_Platinum },
                { name: "Ducats", icon: "ducats", type: "normal", getFunc: (dayData) => dayData?.ducats ?? 0, featurePublicENUM: statsPUBLIC_ENUM_Ducats },
                { name: "Endo", icon: "endo", type: "normal", getFunc: (dayData) => dayData?.endo ?? 0, featurePublicENUM: statsPUBLIC_ENUM_Endo },
                { name: "Credits", icon: "credits", type: "normal", getFunc: (dayData) => dayData?.credits ?? 0, featurePublicENUM: statsPUBLIC_ENUM_Credits },
                { name: "Aya", icon: "aya", type: "normal", getFunc: (dayData) => dayData?.aya ?? 0, featurePublicENUM: statsPUBLIC_ENUM_Aya },
                { name: "Relics opened", icon: "relic", type: "bar", getFunc: (dayData) => dayData?.relicOpened ?? 0, featurePublicENUM: statsPUBLIC_ENUM_AccountData },
                { name: "Daily trades", icon: "trade", type: "bar", getFunc: (dayData) => dayData?.trades ?? 0, featurePublicENUM: statsPUBLIC_ENUM_Trades },
                { name: "Days played", icon: "time", type: "yesNo", getFunc: (dayData) => (dayData?.ts ?? 0) > 0,  featurePublicENUM: statsPUBLIC_ENUM_None },
            ],

            anyChartExpanded: false,

            wholeParsedData: {},
            lastDataPoint: {},
            selectedSearch: "",
            selectedFilter: "all",
            selectedOrdering: "date",
            isOrderingReversed: true,
            shouldShowViewMoreMessage: true,

            showAllOnce: false, //set this to true to show all items (it will itself go back to false)

        }
    },
    mounted: function () {
        tippy('#statsCreateLinkButton', {
            content: document.getElementById("sharePublicLinkTemplate").outerHTML,
            duration: 190,
            arrow: true,
            //  delay: [0, 0],
            offset: [0, 15],
            interactive: true,
            placement: 'bottom',
            theme: 'extraVaultedInfoTheme',
            animation: 'fade',
            allowHTML: true,
            trigger: 'click',
        });

        tippy('#statsDataExportButton', {
            content: document.getElementById("exportDataTemplate").outerHTML,
            duration: 190,
            arrow: true,
            //  delay: [0, 0],
            offset: [0, 15],
            interactive: true,
            placement: 'bottom',
            theme: 'extraVaultedInfoTheme',
            animation: 'fade',
            allowHTML: true,
            trigger: 'click',
        });

        tippy('#foundryUnlocksStats', {
            content: "% of prime unlocks",
            duration: 190,
            arrow: true,
            //  delay: [0, 0],
            placement: 'bottom',
            theme: 'extraVaultedInfoTheme',
            animation: 'fade',
            allowHTML: true,
            // trigger: 'click',
        });

        if(!this.fromOverwolf){
            this.refresh();
        }
    },
    updated: function () {
        if (lastStatsItemTooltips != undefined && lastStatsItemTooltips.length > 0) {
            for (var i = 0; i < lastStatsItemTooltips.length; i++) {
                lastStatsItemTooltips[i].destroy();
            }
            lastStatsItemTooltips = [];
        }

        lastStatsItemTooltips = tippy(".statsTradedItem", {
            duration: 190,
            arrow: true,
            //  delay: [0, 0],
            placement: 'top',
            theme: 'extraVaultedInfoTheme',
            animation: 'fade',
            allowHTML: true,
            // trigger: 'click',
        });
    },
    methods: {
        toSI(input) {
            return ConvertToSI(input);
        },
        ifPublicFeatureEnabled(fetaure) {
            return (((this.wholeParsedData?.publicParts ?? 0) & fetaure) == fetaure) || statsInApp;
        },
        isTradeEnabled() {
            return (((this.wholeParsedData?.publicParts ?? 0) & statsPUBLIC_ENUM_Trades) == statsPUBLIC_ENUM_Trades) || statsInApp;
        },
        lineIfZeroOrUndefined(input) {
            if (input == undefined) return "-";
            if (input == 0) return "-";
            return input;
        },
        openItemDetails(itemName) {
            if (itemName.includes("/AF_Special/")) {
                return;
            }
            if (!this.fromOverwolf) return;
          
            foundryDetailsApp.open(itemName);
        },
        openAFDownloadPage() {
            if (this.fromOverwolf) {
                overwolf.utils.openUrlInDefaultBrowser('https://www.alecaframe.com/?promo=publicStats');
            }
        },        
        createCharts() {

            this.charts.forEach(chartData => {
                const ctx = document.getElementById('chart' + chartData.name);
                if (statsChartObjects[chartData.name] != undefined) statsChartObjects[chartData.name].destroy();

                switch (chartData.type) {
                    case "normal":
                        statsChartObjects[chartData.name] = new Chart(ctx, {
                            data: {
                                datasets: [{
                                    type: 'line',
                                    label: 'Value',
                                    data: this.wholeParsedData.generalDataPoints.map((obj) => { return { x: obj.ts, y: chartData.getFunc(obj) } }),
                                    borderWidth: 2,
                                    tension: 0.55,
                                    spanGaps: true,
                                    fill: {
                                        target: 'origin',
                                        above: 'rgba(200, 200, 200, .08)',
                                    },
                                    borderColor: 'rgba(255,255,255,0.85)',
                                    yAxisID: 'y'
                                },
                                {
                                    type: 'bar',
                                    label: 'Change       ', //Extra spaces to add empty space on the right lel :)
                                    data: this.wholeParsedData.generalDataPoints.map((obj, index, array) => { return { x: obj.ts, y: index > 0 ? (chartData.getFunc(obj) - chartData.getFunc(array[index - 1])) : 0 } }),
                                    borderWidth: 1,
                                    barThickness: (aa) => {
                                        var totalPixels = statsChartObjects[chartData?.name]?.width ?? 320;
                                        var toReturn = ((totalPixels - 100) / this.totalSampleCount) / 1.25;
                                        return toReturn;
                                    },
                                    barPercentage: 0.4,
                                    backgroundColor:
                                        !chartData.name.includes("relic") ?
                                            ((ctx2) => ctx2.parsed.y > 0 ? '#00FF0040' : '#FF000040') :
                                            "#FFFFFF20",
                                    borderColor: 'rgba(255,255,255,0.85)',
                                    yAxisID: 'y1'
                                }],
                            },
                            options: {
                                tooltips: {
                                    callbacks: {
                                        label: function (tooltipItem, data) {
                                            return tooltipItem.yLabel.toFixed(2).replace(/\d(?=(\d{3})+\.)/g, '$&,');
                                        }
                                    }
                                },
                                scales: {
                                    x: {
                                        type: 'time',                                        
                                        time: {
                                            unit: 'day',
                                            tooltipFormat: 'dd MMM yyyy'
                                        }
                                    },
                                    y: {
                                        type: 'linear',
                                        label: 'Value       ', //Extra spaces to add empty space on the right lel :)
                                        beginAtZero: true,
                                        ticks: {
                                            callback: function (value, index, values) {
                                                return ConvertToSI(value);
                                            }
                                        }
                                    },
                                    y1: {
                                        display: false,
                                    }
                                },
                                plugins: {
                                    legend: {
                                        labels: {
                                            usePointStyle: true,
                                            pointStyle: 'rect',
                                            boxWidth: 10,
                                            useBorderRadius: true,
                                        },
                                        align: 'end'
                                    },
                                    tooltip: {
                                        callbacks: {
                                            label: function (tooltipItem) {
                                                return tooltipItem.dataset.label.trim() + ": " + tooltipItem.formattedValue;
                                            }
                                        }
                                    }
                                },
                                elements: {
                                    point: {
                                        radius: 3,
                                    }
                                }
                            },
                            plugins: [legendHeightPlugin]
                        });
                        break;
                    case "bar":
                        statsChartObjects[chartData.name] = new Chart(ctx, {
                            data: {
                                datasets: [{
                                    label: 'Value       ', //Extra spaces to add empty space on the right lel :)
                                    data: this.wholeParsedData.generalDataPoints.map(obj => { return { x: obj.ts, y: chartData.getFunc(obj) } }),
                                    borderWidth: 1,
                                    barThickness: (aa) => {
                                        var totalPixels = statsChartObjects[chartData.name]?.width ?? 320;
                                        var toReturn = ((totalPixels - 100) / this.totalSampleCount) / 1.25;
                                        return toReturn;
                                    },
                                    backgroundColor:
                                        chartData.name.includes("gains") ?
                                            ((ctx2) => ctx2.parsed.y > 0 ? '#00FF0040' : '#FF000040') : "#FFFFFF40",
                                    borderColor: 'rgba(255,255,255,0.85)',

                                    type: 'bar',
                                }],
                            },
                            options: {
                                scales: {
                                    x: {
                                        type: 'time',                                       
                                        time: {
                                            unit: 'day',
                                            tooltipFormat: 'dd MMM yyyy'
                                        }
                                    },
                                    y: {
                                        type: 'linear',
                                        beginAtZero: true
                                    }
                                },
                                plugins: {
                                    legend: {
                                        labels: {
                                            usePointStyle: true,
                                            pointStyle: 'rect',
                                            boxWidth: 10,
                                            useBorderRadius: true,
                                        },
                                        align: 'end'
                                    },
                                    tooltip: {
                                        callbacks: {
                                            label: function (tooltipItem) {
                                                return tooltipItem.dataset.label.trim() + ": " + tooltipItem.formattedValue;
                                            }
                                        }
                                    }
                                },
                            },
                            plugins: [legendHeightPlugin]
                        });
                        break;
                    case "yesNo":
                        statsChartObjects[chartData.name] = new Chart(ctx, {
                            data: {
                                datasets: [{
                                    label: 'Value       ', //Extra spaces to add empty space on the right lel :)
                                    data: this.wholeParsedData.generalDataPoints.map(obj => { return { x: obj.ts, y: chartData.getFunc(obj) } }),
                                    borderWidth: 1,
                                    barThickness: (aa) => {
                                        var totalPixels = statsChartObjects[chartData.name]?.width ?? 320;
                                        var toReturn = ((totalPixels - 100) / this.totalSampleCount) / 1.25;
                                        return toReturn;
                                    },
                                    backgroundColor:
                                        chartData.name.includes("gains") ?
                                            ((ctx2) => ctx2.parsed.y > 0 ? '#00FF0040' : '#FF000040') : "#FFFFFF40",
                                    borderColor: 'rgba(255,255,255,0.85)',

                                    type: 'bar',
                                }],
                            },
                            options: {
                                scales: {
                                    x: {
                                        type: 'time',                                       
                                        time: {
                                            unit: 'day',
                                            tooltipFormat: 'dd MMM yyyy'
                                        }
                                    },
                                    y: {
                                        type: 'linear',
                                        beginAtZero: true,
                                        ticks: {
                                            callback: function (value, index, values) {
                                                return Math.abs(value) > 0 ? "Yes" : "No";
                                            },
                                            stepSize: 1
                                        },
                                        min: 0,
                                        max: 1,
                                    }
                                },
                                plugins: {
                                    legend: {
                                        labels: {
                                            usePointStyle: true,
                                            pointStyle: 'rect',
                                            boxWidth: 10,
                                            useBorderRadius: true,
                                        },
                                        align: 'end'
                                    },
                                    tooltip: {
                                        callbacks: {
                                            label: function (tooltipItem) {
                                                return tooltipItem.dataset.label.trim() + ": " + tooltipItem.formattedValue;
                                            }
                                        }
                                    }
                                },
                            },
                            plugins: [legendHeightPlugin]
                        });
                        break;
                }
            });

            this.timeframeUpdated();
        },
        timeframeUpdated() {

            let valueToSet;
            let maxValueToSet = new Date().valueOf();
            if (this.timeframe == 0) {
                let dateTemp = this.wholeParsedData?.generalDataPoints[0]?.ts;
                valueToSet = dateTemp.valueOf();
            } else {
                valueToSet = new Date().setDate(new Date().getDate() - this.timeframe).valueOf();
            }
            this.totalSampleCount =  Math.round((maxValueToSet - valueToSet) / 1000 / 3600 / 24);
        
            this.charts.forEach(chartData => {
                statsChartObjects[chartData.name].options.scales.x.min = valueToSet;
                statsChartObjects[chartData.name].options.scales.x.max = maxValueToSet;
            });

            //We need to update the charts outside the vue context to avoid recursive issues
            setTimeout(() => {
                this.charts.forEach(chartData => {                  
                    statsChartObjects[chartData.name].update();
                });
            }, 100);
        },
        refreshIfNecessary() {
            this.refresh();
        },
        minimizeAllGraphs() {
            this.charts.forEach(chart => { chart.expanded = false; });
            this.anyChartExpanded = false;
        },
        refresh() {
            this.loading = true;
            this.error = false;
            this.userNotReady = false;
            this.tradeNotReady = false;
            this.notEnabled = false;

            this.refreshedAtLeastOnce = true;

            let urlToRequest = "";

            if (this.fromOverwolf) {
                if (!settingsApp.statsTabEnabled) {
                    this.loading = false;
                    this.notEnabled = true;
                    return;
                }
                if (!plugin.get().IsTradeHistoryEnabled()) {
                    this.tradeNotReady = true;
                }
            }
            
            try {
               
                if (this.fromOverwolf) {
                    let userIdentifier = plugin.get().GetUserIdentifier();

                    if (forcedUID != null && forcedUID != "") userIdentifier = forcedUID;

                    if (userIdentifier == undefined || userIdentifier == "") {
                        this.userNotReady = true;
                        this.loading = false;
                        return;
                    }
                    this.userNotReady = false;
                    urlToRequest = "https://stats.alecaframe.com/api/stats/" + userIdentifier + "?secretToken=" + plugin.get().GetUserIdentifierSecret();
                } else {
                    let urlParams = new URLSearchParams(window.location.search);
                    let pk = urlParams.get("publicToken");
                    if (forcedPublicToken != null && forcedPublicToken != "") pk = forcedPublicToken;

                    if (pk == undefined || pk == "") {
                        this.error = true;
                        this.loading = false;
                        return;
                    }
                    urlToRequest = "https://stats.alecaframe.com/api/stats/public?token=" + encodeURIComponent(pk);
                }
            } catch (e) {
                console.log("Error when creating url: " + e);
                this.loading = false;
                this.error = true;
            }            

            Promise.all([
                fetch(urlToRequest).then((response) => response.json(), (error) => {
                    console.log("Error when loading stats: " + error);
                    this.loading = false;
                    this.error = true;
                }),
                fetch("https://cdn.alecaframe.com/warframeData/custom/basic.json").then((response) => response.json(), (error) => {
                    console.log("Error when loading item data: " + error);
                    this.loading = false;
                    this.error = true;
                })
            ]).then(([parsedData, itemData]) => {

                try {
                    //console.log("Stat data loaded");
                    //console.log(parsedData);
                    //console.log(itemData);

                    if (parsedData.trades == undefined) {
                        parsedData.trades = [];
                    }
                    if (parsedData.generalDataPoints == undefined) {
                        parsedData.generalDataPoints = [];
                    }

                    let lastDataPointPlat = undefined;
                    parsedData.generalDataPoints.forEach(point => {
                        point.ts = new Date(point.ts);
                        point.ts.setUTCHours(0);
                        point.ts.setUTCMinutes(0);
                        point.ts.setUTCSeconds(0);
                        point.ts.setUTCMilliseconds(0);
                        if (lastDataPointPlat != undefined) point.platGain = point.plat - lastDataPointPlat;
                        lastDataPointPlat = point.plat;
                    });

                    const calculatePlatInTrade = (tradex) => {
                        let inRX = tradex.rx.filter(itemx => itemx.name == "/AF_Special/Platinum").reduce((acc, curr) => acc + curr.cnt, 0);
                        let inTX = tradex.tx.filter(itemx => itemx.name == "/AF_Special/Platinum").reduce((acc, curr) => acc + curr.cnt, 0);
                        return inRX + inTX;
                    }

                    parsedData.trades.forEach(trade => {
                        function doWorkForItem(tradeItem) {
                            let item = itemData.items[tradeItem.name];
                            if (item != undefined) {
                                tradeItem.displayName = item.name;
                                tradeItem.picture = item.pic;
                                if (tradeItem.picture.indexOf("https://") != 0) {
                                    tradeItem.picture = "https://cdn.alecaframe.com/warframeData/img/" + tradeItem.picture;
                                }
                            } else if (tradeItem.name.startsWith("/AF_Special/Riven")) {
                                tradeItem.picture = itemData.items["/AF_Special/Riven"].pic;
                                tradeItem.displayName = tradeItem.name.replace("/AF_Special/Riven/", "").replace("/", " ");
                            } else {
                                tradeItem.picture = itemData.items["/AF_Special/Other"].pic;
                                tradeItem.displayName = tradeItem.name.replace("/AF_Special/Other/", "");
                                //console.log("Item data not found: " + tradeItem.name);                            
                            }                            
                        }
                        trade.ts = new Date(trade.ts);
                        trade.rx.forEach(doWorkForItem);
                        trade.tx.forEach(doWorkForItem);
                        trade.totalPlat = calculatePlatInTrade(trade);
                    });

                    

                    //console.log(parsedData);

                    this.wholeParsedData = parsedData;
                    this.lastDataPoint = parsedData.generalDataPoints[parsedData.generalDataPoints.length - 1];
                    this.loading = false;
                    this.error = false;

                    this.createCharts();
                } catch (e) {
                    console.log("Error when parsing received data: " + e);
                    this.loading = false;
                    this.error = true;
                }    
                

            }).catch((error) => {
                console.log("Error when loading data: " + error);
                this.loading = false;
                this.error = true;
            });
        },
    }, computed: {
        orderedTrades: function () {
            let filtered = this.wholeParsedData?.trades;
            if (filtered == undefined) return filtered;
            
            if (this.selectedOrdering == "date") {
                filtered.sort((trade1, trade2) => { return trade1.ts.getTime() - trade2.ts.getTime(); });     
            } else if (this.selectedOrdering == "user") {
                filtered.sort((trade1, trade2) => trade1.user.localeCompare(trade2.user));  
            } else if (this.selectedOrdering == "platPrice") {
                filtered.sort((trade1, trade2) => { return trade1.totalPlat - trade2.totalPlat; });
            } 

            let shouldReverse = this.isOrderingReversed;
            if (this.selectedOrdering == 'user') shouldReverse = !shouldReverse; //If we are ordering by name, do it the other way around

            if (shouldReverse) filtered.reverse();

            return filtered;
        },
        orderedFilteredTrades: function () {    
            let ordered = this.orderedTrades;

            let filterToNumber = 0; //Sales
            if (this.selectedFilter == 'purchases') filterToNumber = 1;
            if (this.selectedFilter == 'trades') filterToNumber = 2;
            let toReturn = ordered?.filter(trade =>
                (trade.type == filterToNumber || this.selectedFilter == 'all') && (
                    trade?.user?.toLowerCase().includes(this.selectedSearch.toLowerCase())
                    || trade.rx.some(item => item.displayName.toLowerCase().includes(this.selectedSearch.toLowerCase()))
                    || trade.tx.some(item => item.displayName.toLowerCase().includes(this.selectedSearch.toLowerCase())))
            );

            this.shouldShowViewMoreMessage = !this.showAllOnce;

            if (this.showAllOnce) {
                this.showAllOnce = false;
                return toReturn;
            } else {                
                return toReturn?.slice(0, 50);
            }
        }
    }
}).mount(".statsVueHolder");

function copyAPIToken() {
    let myToken = plugin.get().GetUserIdentifier();
    overwolf.utils.placeOnClipboard(myToken);
    showSuccesfulToast("Personal token copied to clipboard. Please be careful with it!");

    if (statsInApp) sendMetric("Stats_CopyAPIToken", "");
}
function exportJSON() {
    plugin.get().statsExport("json", JSON.stringify(statsApp.wholeParsedData), (success, errorMessage) => {
        if (success) {
            showSuccesfulToast("Data exported to your desktop");
        } else {
            showErrorToast("Failed to export data: "+errorMessage);
        }
    });
    if (statsInApp) sendMetric("Stats_ExportJSON","");   
}
function exportCSV() {
    plugin.get().statsExport("csv", JSON.stringify(statsApp.wholeParsedData), (success, errorMessage) => {
        if (success) {
            showSuccesfulToast("Data exported to your desktop");
        } else {
            showErrorToast("Failed to export data: " + errorMessage);
        }
    });
    if (statsInApp) sendMetric("Stats_ExportCSV", "");
}

function generateStatsShareLink(onlyToken = false) {
    showBlueToast("Generating link, please wait...");

    let accountDataEnabled = document.getElementById("publicToggleAccountData").checked;
    let tradeDataEnabled = document.getElementById("publicToggleTrades").checked;
    let platinumDataEnabled = document.getElementById("publicTogglePlatinum").checked;
    let ducatsDataEnabled = document.getElementById("publicToggleDucats").checked;
    let endoDataEnabled = document.getElementById("publicToggleEndo").checked;
    let creditsDataEnabled = document.getElementById("publicToggleCredits").checked;
    let ayaDataEnabled = document.getElementById("publicToggleAya").checked;
    let relcisDataEnabled = document.getElementById("publicToggleRelics").checked;

    let dataEnabledArray = [accountDataEnabled, tradeDataEnabled, platinumDataEnabled, ducatsDataEnabled, endoDataEnabled, creditsDataEnabled, ayaDataEnabled, relcisDataEnabled];

    setTimeout(() => {
        plugin.get().GenerateStatsPublicToken(forcedUID, dataEnabledArray, (success, data) => {
            if (success) {
                if (onlyToken) {
                    showSuccesfulToast("Public token copied to clipboard");
                    overwolf.utils.placeOnClipboard(data);
                    if (statsInApp) sendMetric("Stats_GeneratePublicToken", "");
                } else {
                    showSuccesfulToast("Link copied to clipboard");
                    overwolf.utils.placeOnClipboard("https://stats.alecaframe.com/stats?publicToken=" + encodeURIComponent(data));
                    overwolf.utils.openUrlInDefaultBrowser("https://stats.alecaframe.com/stats?publicToken=" + encodeURIComponent(data));
                    if (statsInApp) sendMetric("Stats_GeneratePublicLink", "");
                }
              
            } else {
                showErrorToast("Failed to generate link: " + data);
            }
        });
    }, 800);

    
    
}



function isPublicPartEnabled(enumValue) { //Check whether we are showing the public part of the stats. If we are in the app, always show them (it is our own info)
    return ((statsApp?.wholeParsedData?.publicParts ?? 0) & enumValue) == enumValue || statsInApp;
}