
rivenDetailsApp = Vue.createApp({
    data() {
        return {
            visible: false,
            loading: false,
            riven: {},
            selectedModLevel: 0,
            selectedWeaponIndex: 0,
            listType: 'direct',
            listVisibility: 'public',

            WFMdescription: '',
            WFMsellingPrice: 0,
            WFMstartingPrice: 0,
            WFMbuyoutPrice: 0,
            listType: 'direct',
            useLvl8Stats: false,
            listVisibility: 'public',
            WFMminReputation: 0,

            similarMode: 'all',

            isSelling: false,
            isEditing: false
        }
    },
    methods: {
        open(randomID) {


            this.WFMdescription = '';
            this.WFMsellingPrice = 0;
            this.WFMstartingPrice = 0;
            this.WFMbuyoutPrice = 0;
            this.WFMminReputation = 0;
            this.listType = 'direct';
            this.listVisibility = 'public';

            this.isSelling = true;
            this.isEditing = false;

            this.visible = true;
            escapeKeyHandlersStack.push(() => { this.visible = false });

            this.loading = true;
            this.riven = {};

            this.similarMode = 'all';

            // console.log("Opening relic details for item: " + uniqueID);
            plugin.get().GetSingleRivenDetails(randomID, (success, data) => {
                setTimeout(() => { this.loading = false; }, 150);
                if (success) {
                    this.riven = JSON.parse(data);
                    //console.log(JSON.parse(data));
                    this.selectedModLevel = this.riven.currentImprovementLevel;
                    this.selectedWeaponIndex = 0;
                } else {
                    console.error("Failed to get riven details: " + data);
                    showErrorToast("An error has occurred: " + data);
                    this.visible = false;
                }
            });
            sendMetric("Riven_Open", "inventory");
        },
        openFromWFM(wfmURL, editMode = false) {
            this.isSelling = false;

            this.visible = true;
            escapeKeyHandlersStack.push(() => { this.visible = false });
            this.loading = true;
            this.riven = {};

            this.isSelling = false;
            this.isEditing = editMode;

            console.log("Opening relic details for WFM item: " + wfmURL);
            plugin.get().GetSingleRivenDetailsFromWFM(wfmURL, (success, data) => {
                this.loading = false;
                if (success) {
                    this.riven = JSON.parse(data);

                    if (editMode) {
                        this.WFMdescription = this.riven.editWFMdescription;
                        this.WFMsellingPrice = this.riven.editWFMsellingPrice;
                        this.WFMstartingPrice = this.riven.editWFMstartingPrice;
                        this.WFMbuyoutPrice = this.riven.editWFMbuyoutPrice;
                        this.WFMminReputation = this.riven.editWFMminReputation;
                        this.listType = this.riven.editlistType;
                        this.listVisibility = this.riven.editlistVisibility;
                    }

                    //console.log(JSON.parse(data));
                    this.selectedModLevel = 0;
                    this.selectedWeaponIndex = 0;
                } else {
                    console.error("Failed to get WFM riven details: " + data);
                    showErrorToast(data);
                    this.visible = false;
                }
            });
            sendMetric("Riven_Open", "WFM");
        },
        selectSimilarMode(newMode) {
            this.similarMode = newMode;
        },
        listOnWFM() {
            this.loading = true;
            if (this.isEditing) {
                console.log("Editing riven on WFM...");
                plugin.get().EditRivenOnWFM(this.riven.randomID, this.listType, this.listVisibility,
                    this.WFMsellingPrice, this.WFMstartingPrice, this.WFMbuyoutPrice,
                    this.WFMminReputation, this.WFMdescription, this.useLvl8Stats,
                    (success, data) => {
                        this.loading = false;
                        if (success) {
                            showSuccesfulToast("Listing edited!");
                            this.visible = false;
                        } else {
                            showErrorToast(data);
                        }
                    }
                );
                sendMetric("WFM_Edit", "contract");
                sendMetric("Riven_Edit", "");
            } else {
                console.log("Listing riven on WFM...");
                plugin.get().ListRivenOnWFM(this.riven.randomID, this.listType, this.listVisibility,
                    this.WFMsellingPrice, this.WFMstartingPrice, this.WFMbuyoutPrice,
                    this.WFMminReputation, this.WFMdescription, this.useLvl8Stats, (success, data) => {
                        this.loading = false;
                        if (success) {
                            showSuccesfulToast("Listing created!");
                            this.visible = false;
                            setTimeout(unveiledRivensApp?.refresh, 1750);
                        } else {
                            showErrorToast(data);
                        }
                    }
                );
                sendMetric("Riven_PostWFM", "");
            }

        },
        createRivenMarketImportString(uid) {
            showSuccesfulToast("Generating import string. This might take a few seconds...");
            plugin.get().GetRivenMarketImportString(uid, (success, data) => {
                if (success) {
                    console.log("Import string: " + data);
                    overwolf.utils.placeOnClipboard(data);
                    overwolf.utils.openUrlInDefaultBrowser("https://riven.market/sell?import=" + data);
                    showSuccesfulToast("Import string copied to clipboard and page opened!");
                    this.riven.importString = data;
                } else {
                    showErrorToast(data);
                }
            });
            sendMetric("Riven_ImportToRivenMarket", "");
        },
        openAFRivenClicked(source, link) {
            rivenDetailsApp.openFromWFM(link);
        },
        openWebRivenClicked(link) {
            overwolf.utils.openUrlInDefaultBrowser(link);
        }
    }
}).mount("#modalRivenDetails");


function listenForOpenWFMRivenMessages(message) {
    if (message.id == "openWFMRiven") {
        console.log("Opening WFM riven from message: " + message.content);
        rivenDetailsApp.openFromWFM(message.content);
        setTimeout(() => { overwolf.windows.bringToFront(true, () => { }) }, 1000);
    }
}

overwolf.windows.onMessageReceived.removeListener(listenForOpenWFMRivenMessages);
overwolf.windows.onMessageReceived.addListener(listenForOpenWFMRivenMessages);

rivenFinderApp = Vue.createApp({
    data() {
        return {
            weaponData: {},
            weaponSearchOpen: false,
            weaponSearchString: '',
            weaponSearchData: [],

            similarRivens: [],

            originalWeaponSelectedID: '',

            platinumChart: undefined,

            similarMode: 'all',

            minPrice: '', //Usually numbers, buy if empty it means don't care
            maxPrice: '', //Usually numbers, buy if empty it means don't care
            minSimilarity: '',
            minRerolls: '',
            maxRerolls: '',
            negativeRequired: false,

            weaponLoading: false,
            similarLoading: false,
            sniperAddLoading: false,

            sniperConfigName: "",

            currentConfigCount: 0,
            maxConfigCount: 0,

            attrsToSelect: [
                {
                    positive: true, selectedAttrUID: "", required: false
                },
                { positive: true, selectedAttrUID: "", required: false },
                { positive: true, selectedAttrUID: "", required: false },
                { positive: false, selectedAttrUID: "", required: false }
            ]
        }
    },
    methods: {
        refreshWeaponSearch() {
            plugin.get().GetRivenWeapons(this.weaponSearchString, (success, data) => {
                if (success) {
                    this.weaponSearchData = JSON.parse(data);
                } else {
                    console.error("Failed to get riven weapons: " + data);
                }
            });
        },
        weaponSelected(weaponName) {
            this.weaponSearchOpen = false;
            this.originalWeaponSelectedID = weaponName;
            this.weaponLoading = true;
            plugin.get().GetRivenHistoryData(weaponName, getSubscriptionStatus(), (success, data) => {
                if (success) {
                    this.weaponData = JSON.parse(data);
                    this.refreshChart();
                    this.selectedAttrsChanged();
                } else {
                    console.error("Failed to get riven history data: " + data);
                    showErrorToast("An error has occurred: " + data);
                }
                this.weaponLoading = false;
            });
        },
        refreshChart() {
            refreshPlatinumChart();
        },
        selectedAttrsChanged() {

            this.similarLoading = true;

            let filters = {
                minPrice: this.parseOrDefault(this.minPrice, 0),
                maxPrice: this.parseOrDefault(this.maxPrice, 10000000),
                minSimilarity: this.parseOrDefault(this.minSimilarity, 0),
                minRerolls: this.parseOrDefault(this.minRerolls, 0),
                maxRerolls: this.parseOrDefault(this.maxRerolls, 10000000),
                negativeRequired: this.negativeRequired,
            };

            plugin.get().FinderRivenAttrsJustChanged(this.originalWeaponSelectedID, JSON.stringify(this.attrsToSelect), JSON.stringify(filters), (success, data) => {
                if (success) {
                    this.weaponData.attrs = JSON.parse(data);
                } else {
                    console.error("Failed to get riven selectedAttrsChanged data: " + data);
                    showErrorToast("An error has occurred: " + data);
                }
            },
                (success, data) => {
                    if (success) {
                        this.similarRivens = JSON.parse(data);
                    } else {
                        console.error("Failed to get riven selectedAttrsChanged similarRiven data: " + data);
                        showErrorToast("An error has occurred: " + data);
                    }
                    setTimeout(() => { this.similarLoading = false; }, 150);
                });
        },
        getPossibleAttrDataByUID(attrUID) {
            return this.weaponData?.attrs?.find(p => p.internalName == attrUID);
        },
        openAFRivenClicked(source, link) {
            rivenDetailsApp.openFromWFM(link);
        },
        openWebRivenClicked(link) {
            overwolf.utils.openUrlInDefaultBrowser(link);
        },
        parseOrDefault(value, defaultValue) {
            let parsed = parseInt(value);
            if (isNaN(parsed)) return defaultValue;
            return parsed;
        },
        addSniperConfig() {

            this.sniperAddLoading = true;


            let filters = {
                minPrice: this.parseOrDefault(this.minPrice, 0),
                maxPrice: this.parseOrDefault(this.maxPrice, 10000000),
                minSimilarity: this.parseOrDefault(this.minSimilarity, 0),
                minRerolls: this.parseOrDefault(this.minRerolls, 0),
                maxRerolls: this.parseOrDefault(this.maxRerolls, 10000000),
            };

            plugin.get().AddRivenSniperConfig(this.originalWeaponSelectedID, JSON.stringify(this.attrsToSelect), JSON.stringify(filters), this.sniperConfigName, (success, data) => {
                this.sniperAddLoading = false;
                if (success) {
                    showSuccesfulToast("Notification added!");
                    rivenSniperApp.refresh();
                    document.querySelector(".tabHeaderOption[tabid=rivenSniperTab]").click();
                } else {
                    console.error("Failed to get riven selectedAttrsChanged data: " + data);
                    showErrorToast("An error has occurred: " + data);
                }
            });
        },
    },
    mounted() {

    }
}).mount("#rivenFinderTab");

setTimeout(() => {
    rivenFinderApp.refreshWeaponSearch();
}, 3000);


var rivenPlatinumChart = undefined;
var rivenPlatinumChartData = undefined;
var rivenPlatinumChartDataBase = undefined;
function refreshPlatinumChart() {
    const ctx = document.getElementById("rivenFinderPrice");
    if (rivenPlatinumChart != undefined) rivenPlatinumChart.destroy();

    rivenPlatinumChartData = rivenFinderApp.weaponData.priceData.map((obj) => { return { x: Date.parse(obj.time).valueOf(), y: obj.price } });
    rivenPlatinumChartData.sort((a, b) => a.x - b.x);

    rivenPlatinumChartDataBase = rivenFinderApp.weaponData.priceData.map((obj) => { return { x: Date.parse(obj.time).valueOf(), y: obj.basePrice } });
    rivenPlatinumChartDataBase.sort((a, b) => a.x - b.x);


    rivenPlatinumChart = new Chart(ctx, {
        data: {
            datasets: [{
                type: 'line',
                label: 'Average',
                data: rivenPlatinumChartData,
                borderWidth: 2,
                tension: 0.4,
                spanGaps: true,
                fill: {
                    target: 'origin',
                    above: 'rgba(200, 200, 200, .08)',
                },
                borderColor: 'rgba(255,255,255,0.85)',
                yAxisID: 'y'
            }, {
                type: 'line',
                label: 'Base',
                data: rivenPlatinumChartDataBase,
                borderWidth: 2,
                tension: 0.4,
                spanGaps: true,
                fill: {
                    target: 'origin',
                    above: 'rgba(200, 200, 200, .08)',
                },
                borderColor: 'rgba(255,255,255,0.85)',
                yAxisID: 'y'
            }]
        },
        options: {
            tooltips: {
                callbacks: {
                    label: function (tooltipItem, data) {
                        return Math.round(data);
                    }
                }
            },
            scales: {
                x: {
                    type: 'time',
                    time: {
                        unit: 'month',
                        tooltipFormat: 'MMM'
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
                    radius: 2,
                }
            }
        },
        plugins: [legendHeightPlugin]
    });


    let valueToSet = Date.parse(rivenPlatinumChartData[rivenPlatinumChartData.length - 1].x).valueOf();
    let maxValueToSet = new Date().valueOf();

    rivenPlatinumChart.options.scales.x.min = valueToSet;
    rivenPlatinumChart.options.scales.x.max = maxValueToSet;

    //We need to update the charts outside the vue context to avoid recursive issues
    setTimeout(() => {
        rivenPlatinumChart.update();
    }, 100);

}



rivenSniperApp = Vue.createApp({
    data() {
        return {
            accountData: {},
            loading: false,
            notLoggedIn: false,
        }
    },
    methods: {
        refresh(auto = false) {            

            this.loading = true;
            plugin.get().GetRivenSniperAccount((success, data) => {
                if (success) {
                    this.accountData = JSON.parse(data);
                    rivenFinderApp.currentConfigCount = this.accountData?.notifications?.length;
                    rivenFinderApp.maxConfigCount = this.accountData?.maxSubscriptions;
                    //this.accountData.AlecaFrameSubscriptionStatus = "None";
                    setSubscriptionStatus(this.accountData?.AlecaFrameSubscriptionStatus);
                } else {
                    if (data == "not_logged_in") {
                        this.notLoggedIn = true;
                    } else {
                        console.error("Failed to get riven sniper account: " + data);
                        //showErrorToast("An error has occurred: " + data);
                    }
                    if (auto) {
                        enqueueRivenSniperRefresh();
                    }
                }
                this.loading = false;
            });
        },
        removeNotification(notificationID) {
            this.loading = true;
            plugin.get().DeleteRivenSniperConfig(notificationID, (success, data) => {
                if (success) {
                    showSuccesfulToast("Notification removed!");                    
                } else {
                    showErrorToast("Failed to remove notification: " + data);
                }
                this.refresh(); //loading is set to false inside refresh
            });
        },
        toggleNotificationEnabled(notificationID, newStatus) {
            this.loading = true;
            plugin.get().RivenSniperChangeNotificationEnabled(notificationID, newStatus, (success, data) => {
                if (success) {
                    showSuccesfulToast("Notification status updated!");                    
                } else {
                    showErrorToast("Failed to update notification status: " + data);
                }
                this.refresh(); //loading is set to false inside refresh
            });
        },
        notificationSettingsChanged() {

            let newSettings = {
                discordWebhook: this.accountData.notificationDiscordWebhook,
            }

            this.loading = true;
            plugin.get().RivenSniperChangeSettings(JSON.stringify(newSettings), (success, data) => {
                if (success) {
                    showSuccesfulToast("Notification settings updated!");                    
                } else {
                    showErrorToast("Failed to update notification settings: " + data);
                }
                this.refresh(); //loading is set to false inside refresh
            });            
        }
    },
    mounted() {

    }
}).mount("#rivenSniperTab");

function enqueueRivenSniperRefresh() {
    setTimeout(() => {
        rivenSniperApp.refresh(true);
    }, 5000);
}

//enqueueRivenSniperRefresh();