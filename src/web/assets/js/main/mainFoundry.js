﻿tippy.delegate('#tabFoundry', {
    target: '.foundryObjectTopIcon.isMastered',
    content: "Mastered",
    duration: 190,
    arrow: true,
    placement: 'top',
    theme: 'extraVaultedInfoTheme',
    animation: 'fade',
    // trigger: 'click',
});

tippy.delegate('#tabFoundry', {
    target: '.foundryObjectTopIcon.pending',
    content: "Item in the foundry",
    duration: 190,
    arrow: true,
    placement: 'top',
    theme: 'extraVaultedInfoTheme',
    animation: 'fade',
    // trigger: 'click',
});

tippy.delegate('#tabFoundry', {
    target: '.foundryObjectTopIcon.helminth',
    content: "Subsumed into the Helminth System",
    duration: 190,
    arrow: true,
    placement: 'top',
    theme: 'extraVaultedInfoTheme',
    animation: 'fade',
    // trigger: 'click',
});

tippy.delegate('#tabFoundry', {
    target: '.foundryObjectTopIcon.componentInSomething',
    duration: 190,
    arrow: true,
    placement: 'top',
    theme: 'extraVaultedInfoTheme',
    animation: 'fade',
    allowHTML: true,
    // trigger: 'click',
});

/*tippy.delegate('#tabFoundry', {
    target: '.foundryObjectComponentsComponent',
    duration: 190,
    arrow: true,
    placement: 'top',
    theme: 'extraVaultedInfoTheme',
    animation: 'fade',
    allowHTML: true,
    // trigger: 'click',
});*/

var lastFoundryComponentTooltips = [];
var lastFoundryArchonShardsTooltips = [];

var foundryApp = Vue.createApp({
    data() {
        return {
            items: [],
            loading: false,
            noItems: false,

            topLeftCategories: ["All", "Warframe", "Primary", "Secondary", "Melee", "Modular", "Arch", "Companion"],
            selectedCategory: "Warframe",

            refreshedAtLeastOnce: false,

            componentTippyData: [],
            componentTippyInstance: undefined,
            tippyInstanceIsWaitingForUpdate: false,

            selectedSearch: "",
            yesnoFilters: {
                //"primeType" : "prime"
            },

            showAllEnabled: false //Used to remember the showAll between small refreshes
        }
    },
    mounted: function () {
        createFoundryFilterTippy(this);
        //this.refresh();
    },
    updated: function () {
        if (this.tippyInstanceIsWaitingForUpdate) {
            this.componentTippyInstance.setContent($("#foundryComponentTooltipTemplate")[0].innerHTML);
            this.tippyInstanceIsWaitingForUpdate = false;
        }
    },
    methods: {
        setFavStatus(uniqueID, isFav) {
            plugin.get().SetFavouriteStatus(uniqueID, isFav);
        },
        changeSetting(category) {
            this.selectedCategory = category;
            this.showAllEnabled = false;
            this.refresh();
        },
        refreshIfNecessary() {
            this.refresh();
        },
        refresh(showAll = false) {
            var fullInventoryExperienceEnabled = settingsApp.fullInventoryExperienceEnabled;
            if(showAll) this.showAllEnabled = true; //Remember that the user wants to see all items

            var filterSettings = {
                type: this.selectedCategory.toLowerCase(),
                search: this.selectedSearch,
                order: "name",
                prime: "prime",
                orderLargerToSmaller: false,
                yesnoFilters: JSON.stringify(this.yesnoFilters)
            };

            this.loading = true;

            plugin.get().getFilteredFoundry(JSON.stringify(filterSettings), fullInventoryExperienceEnabled || showAll || this.showAllEnabled, (success, data) => {
                if (success) {
                    this.loading = false;
                    this.noItems = false;
                    this.items = JSON.parse(data);

                    if (this.items != undefined && this.items.length > 0) {
                        this.refreshedAtLeastOnce = true;
                    }

                    if (this.items == undefined || this.items.length == 0) this.noItems = true;

                    setTimeout(() => {

                        if (lastFoundryComponentTooltips != undefined && lastFoundryComponentTooltips.length > 0) {
                            for (var i = 0; i < lastFoundryComponentTooltips.length; i++) {
                                lastFoundryComponentTooltips[i].destroy();
                            }
                            lastFoundryComponentTooltips = [];
                        }

                        lastFoundryComponentTooltips = tippy.delegate('#tabFoundry', {
                            target: ".foundryObjectComponents>.foundryObjectComponentsComponent",
                            duration: 190,
                            arrow: true,
                            content: '',  // without content it doesn't work
                            onShow(instance) {
                                const id = instance.reference.getAttribute('itemUID');
                                const componentName = instance.reference.getAttribute('componentName');
                                instance.setContent("Loading...");

                                plugin.get().getFoundryComponentTooltip(id,componentName, (success, data) => {
                                    if (success) {
                                        let deserialized = JSON.parse(data);                                        
                                        foundryApp.componentTippyData = deserialized;   
                                        foundryApp.componentTippyInstance = instance;
                                        foundryApp.tippyInstanceIsWaitingForUpdate = true;
                                    } else {
                                        showErrorToast("Failed to load component tooltip: " + data);
                                    }
                                });
                                return;
                            },
                            onClose(instance) {
                                foundryApp.componentTippyData = [];
                                foundryApp.tippyInstanceIsWaitingForUpdate = false;
                                foundryApp.componentTippyInstance = undefined;
                            },
                            //  delay: [0, 0],
                            placement: 'top',
                            theme: 'extraVaultedInfoTheme',
                            animation: 'fade',
                            allowHTML: true,
                            //interactive: true,
                            //trigger: 'click',
                        });
                        if (lastFoundryArchonShardsTooltips != undefined && lastFoundryArchonShardsTooltips.length > 0) {
                            for (var i = 0; i < lastFoundryArchonShardsTooltips.length; i++) {
                                lastFoundryArchonShardsTooltips[i].destroy();
                            }
                            lastFoundryArchonShardsTooltips = [];
                        }

                        lastFoundryArchonShardsTooltips = tippy.delegate('#tabFoundry', {
                            target: ".archonShards",
                            content: '',
                            // without content it doesn't work
                            onShow(instance) {
                                const id = instance.reference.getAttribute('data-template');
                                const template = document.getElementById(id);
                                instance.setContent(template.innerHTML);
                            },
                            duration: 190,
                            arrow: true,
                            offset: [0, 15],
                            interactive: false,
                            placement: 'top',
                            theme: 'extraVaultedInfoTheme',
                            animation: 'fade',
                            maxWidth: 650,
                            allowHTML: true,
                            //trigger: 'click',
                        });

                    }, 100);
                } else {
                    this.items = [];
                    this.noItems = true;

                    setTimeout(() => { this.loading = false; }, 350);
                }
            });
        },
        openDetails(uID, componentUID = "") {
            foundryDetailsApp.open(uID, componentUID);
            sendMetric("Foundry_OpenDetails", "");
        },
        openCraftingTree(uID) {
            craftingTreeApp.open(uID);
            sendMetric("Foundry_OpenCraftingTree", "");
        }
    }
}).mount("#tabFoundry");

var lastFoundryFilterTippy;

function createFoundryFilterTippy(foundryAppLocal) {
    if (lastFoundryFilterTippy != undefined) {
        lastFoundryFilterTippy.destroy();
        lastFoundryFilterTippy = undefined;
    }

    if (foundryAppLocal == undefined) foundryAppLocal = foundryApp;

    var htmlToUse = document.getElementById("tooltipFiltersFoundryTemplate").outerHTML;
    //if (foundryAppLocal.yesnoFilters.primeType == "prime") htmlToUse = htmlToUse.replace("primePlaceholder", "pulsed");

    lastFoundryFilterTippy = tippy('#settingFilterExpandingFoundry', {
        content: htmlToUse,
        duration: 190,
        arrow: true,
        //  delay: [0, 0],
        offset: [0, 15],
        interactive: true,
        placement: 'top',
        theme: 'extraVaultedInfoTheme',
        animation: 'fade',
        allowHTML: true,
        // trigger: 'click',
    })[0];
}




function filterFoundryCheckboxClicked(event, filter, newMode) {

    var shouldRemove = foundryApp.yesnoFilters[filter] == newMode;

    if (shouldRemove) {
        delete foundryApp.yesnoFilters[filter];
        event.target.classList.remove("pulsed");
    } else {
        foundryApp.yesnoFilters[filter] = newMode;
        $(event.target.parentElement.children).removeClass("pulsed");
        event.target.classList.add("pulsed");
    }

    foundryApp.refresh();
}



function Refreshfoundry(showAll = false) {
    if (foundryApp != null) foundryApp.refresh(showAll);
}



var playerStatusAndAccountDataApp = Vue.createApp({
    data() {
        return {
            playerState: {},
            lastTimeDetailedRequested: 0,
        }
    },
    mounted: function () {
        //   createFoundryFilterTippy(this);
        tippy('#foundryUnlocks', {
            content: "% of prime unlocks",
            duration: 190,
            arrow: true,
            //  delay: [0, 0],
            placement: 'top',
            theme: 'extraVaultedInfoTheme',
            animation: 'fade',
            allowHTML: true,
            // trigger: 'click',
        });

        tippy('#currentStatusOK', {
            content: "Last update: " + plugin.get().lastDataUpdateTime == undefined ? "Never" : plugin.get().lastDataUpdateTime,
            duration: 190,
            arrow: true,
            //  delay: [0, 0],
            offset: [0, 15],
            interactive: true,
            placement: 'bottom',
            theme: 'extraVaultedInfoTheme',
            animation: 'fade',
            allowHTML: true,
            onShow(instance) {
                instance.setContent("Last update: " + plugin.get().lastDataUpdateTime == undefined ? "Never" : plugin.get().lastDataUpdateTime);
            },
            // trigger: 'click',
        });

        tippy('#currentStatusCACHE', {
            content: '<div class="statusCacheTooltipHolder">WF connection not ready, using old data. <button class="startTroubleshoot" onclick="helpApp.open()">Troubleshoot</button></div>',
            duration: 190,
            arrow: true,
            //  delay: [0, 0],
            offset: [0, 15],
            interactive: true,
            placement: 'bottom',
            theme: 'extraVaultedInfoTheme',
            animation: 'fade',
            allowHTML: true,
            // trigger: 'click',
        });

        tippy('#currentStatusERROR', {
            content: '<div class="statusCacheTooltipHolder">Warframe connection not ready<button class="startTroubleshoot" onclick="helpApp.open()">Troubleshoot</button></div>',
            duration: 190,
            arrow: true,
            //  delay: [0, 0],
            offset: [0, 15],
            interactive: true,
            placement: 'bottom',
            theme: 'extraVaultedInfoTheme',
            animation: 'fade',
            allowHTML: true,
            // trigger: 'click',
        });

        this.refresh();
        setInterval(this.refresh, 1000);
    },
    methods: {
        refresh() {
            plugin.get().getFoundryPlayerStats((success, data) => {
                if (success) {
                    this.playerState = JSON.parse(data);
                }
            });
        }
    }
}).mount("#warframeStatusAndAccountData");



var topBarPRtippy = undefined;
var topBarBarotippy = undefined;


var worldTimersApp = Vue.createApp({
    data() {
        return {
            worldState: {},
            primeResurgenceState: {},
            baroState: {},
            circuitState: {},

            notiEarthDay: false,
            notiEarthNight: false,
            notiCetusDay: false,
            notiCetusNight: false,
            notiVallisCold: false,
            notiVallisWarm: false,
            notiCambionVome: false,
            notiCambionFass: false,

            lastTimeDetailedRequested: 0,

            selectedFissureType: 'normal',
            fissureNotificationEnabled: false,
            fissureNotificationSettings: [], //Add one default settting (the user can add more later)
        }
    },
    mounted: function () {      
        topBarPRtippy = tippy('.topBarPrimeResurgence', {
            content: $("#primeResurgenceTemplate")[0].innerHTML,
            duration: 190,
            arrow: true,
            //  delay: [0, 0],
            offset: [0, 15],
            interactive: true,
            placement: 'bottom',
            theme: 'extraVaultedInfoTheme',
            animation: 'fade',
            maxWidth: 650,
            allowHTML: true,
            onShown(instance) {
                topBarBarotippy[0].hide();
            },
            //trigger: 'click',
        });

        topBarBarotippy = tippy('.topBarBaro', {
            content: $("#baroTemplate")[0].innerHTML,
            duration: 190,
            arrow: true,
            //  delay: [0, 0],
            offset: [0, 15],
            interactive: true,
            placement: 'bottom',
            theme: 'extraVaultedInfoTheme',
            animation: 'fade',
            maxWidth: 950,
            allowHTML: true,
            onShown(instance) {
                topBarPRtippy[0].hide();
            },
            //trigger: 'click',
        });

        try {
            if (localStorage.getItem("lastFissureNotificationSettings") != undefined) {
                this.fissureNotificationSettings = JSON.parse(localStorage.getItem("lastFissureNotificationSettings"));
            }
        } catch (e) {
            this.fissureNotificationSettings = [];
        }
        if (this.fissureNotificationSettings == undefined) this.fissureNotificationSettings = [];     

        if (localStorage.getItem("lastFissureNotificationEnabled") != undefined) {
            this.fissureNotificationEnabled = localStorage.getItem("lastFissureNotificationEnabled") == "true";            
        }

        this.notificationSettingsUpdated(); //This also calls refresh()
        setInterval(this.refresh, 1000);
    },
    methods: {
        refresh(forceDetailed = false) {
            let requestDetailed = (Date.now() - this.lastTimeDetailedRequested > 15000) || forceDetailed;

            if (requestDetailed) {
                this.lastTimeDetailedRequested = Date.now();
            }

            if (requestDetailed) this.lastTimeDetailedRequested = Date.now();
            plugin.get().getFoundryWorldStats(requestDetailed, (success, data) => {
                if (success) {
                    let parsed = JSON.parse(data);

                    this.worldState = parsed.timerData;
                    
                    if (requestDetailed) {

                        if (!parsed.dataLoaded) { //If the backend data is not loaded yet, ignore the response
                            this.lastTimeDetailedRequested = 0;
                        } else {
                            this.baroState = parsed.baroData;
                            this.primeResurgenceState = parsed.primeResurgenceData;
                            this.circuitState = parsed.circuitData;
                            setTimeout(() => {
                                if (topBarPRtippy != undefined && topBarPRtippy.length > 0) topBarPRtippy[0].setContent($("#primeResurgenceTemplate")[0].innerHTML);
                                if (topBarBarotippy != undefined && topBarBarotippy.length > 0) topBarBarotippy[0].setContent($("#baroTemplate")[0].innerHTML);
                            }, 100);
                        }                        
                    }
                }
            });           
        },
        notificationSettingsUpdated() {

            if (this.fissureNotificationEnabled) {
                if (this.fissureNotificationSettings.length == 0) {
                    this.addFissureNotification(); //Always add at least one
                }
            }

            plugin.get().SetTimerNotificationSettings(this.notiEarthDay, this.notiEarthNight, this.notiCetusDay, this.notiCetusNight, this.notiVallisCold, this.notiVallisWarm, this.notiCambionVome, this.notiCambionFass);
            plugin.get().SetFissureNotificationSettings(this.fissureNotificationEnabled, JSON.stringify(this.fissureNotificationSettings));

            localStorage["lastFissureNotificationSettings"] = JSON.stringify(this.fissureNotificationSettings);
            localStorage["lastFissureNotificationEnabled"] = this.fissureNotificationEnabled;
             
            this.refresh();

            sendMetric("TopBar_TimerNotifications", ""); //This is incorrect (this function is also called when the fissure notification settings are updated)
        },
        addFissureNotification() {
            this.fissureNotificationSettings.push({ type: 'all', mode: 'all', location: 'all', steelPath: 'all', id: this.fissureNotificationSettings.length });
            this.notificationSettingsUpdated();
            sendMetric("TopBar_FissureAddNotification", "");
        },
        deleteFissureNotification(settingIndex) {
            this.fissureNotificationSettings.splice(settingIndex, 1);
            if (this.fissureNotificationSettings.length == 0) {
                this.fissureNotificationEnabled = false;
            }
            this.notificationSettingsUpdated();
        }, 
        openFoundryDetails(uID) {
            foundryDetailsApp.open(uID);
        }
    }
}).mount("#timerEventsTab");

function openTopBarPRItem(event) {
    foundryDetailsApp.open(event.currentTarget.getAttribute("itemUID"));
    topBarPRtippy[0].hide();
    topBarBarotippy[0].hide();
    sendMetric("TopBar_OpenItemFromPRorBaro", "");
}








var aboutTabApp = Vue.createApp({
    data() {
        return {
            buttonText: 'Check for updates',
            buttonClass: 'normal',
            version: '0.0.0',
            working: false
        }
    },
    mounted: function () {
        overwolf.extensions.current.getManifest((manifest) => {
            this.version = manifest.meta.version;
        });
    },
    methods: {
        updateButtonClicked() {

            if (this.working) return;

            switch (this.buttonText) {
                case "Check for updates":

                    this.working = true;
                    sendMetric("About_CheckForUpdates", "");
                    this.buttonText = "Checking...";

                    setTimeout(() => {
                        overwolf.extensions.checkForExtensionUpdate((result) => {
                            console.log(JSON.stringify(result));
                            if (result.success) {
                                if (result.state == 'UpdateAvailable') {
                                    this.buttonText = "Downloading update...";

                                    overwolf.extensions.updateExtension((result2) => {
                                        if (result2.success) {
                                            showSuccesfulToast("New version downloaded. Please restart AlecaFrame to apply the update.");
                                            this.buttonText = "Restart AlecaFrame";
                                            this.working = false;
                                        } else {
                                            showErrorToast("Please close the game and try again");
                                            this.buttonText = "Check for updates";
                                            this.working = false;
                                        }
                                    });

                                } else if (result.state == "PendingRestart") {
                                    showSuccesfulToast("New version downloaded. Please restart AlecaFrame to apply the update.");
                                    sendMetric("About_UpdateCompleted", "");
                                    this.buttonText = "Restart AlecaFrame";
                                    this.working = false;
                                } else {
                                    showSuccesfulToast("You are already running the latest version!");
                                    this.buttonText = "Check for updates";
                                    this.working = false;
                                }
                            } else {
                                this.buttonText = "Check for updates";
                                this.working = false;
                            }
                        });
                    }, 1000);

                    break;
                case "Restart AlecaFrame":
                    overwolf.extensions.relaunch();
                    break;
                default:
                    break;
            }
        }
    }
}).mount("#tabAbout");


function doZoomWork(){
    let zoomLevel = parseInt(localStorage["zoomLevel"]??"100");
    $("#zoomLevel").text(zoomLevel + "%");
    overwolf.windows.setZoom((zoomLevel/100) - 1, "");
}

function decreaseZoom(){
    localStorage["zoomLevel"] = Math.max(Math.min(parseInt(localStorage["zoomLevel"] ?? "100") - 25,175),25);
    doZoomWork();
}

function increaseZoom(){
    localStorage["zoomLevel"] = Math.max(Math.min(parseInt(localStorage["zoomLevel"] ?? "100") + 25, 175), 25);
    doZoomWork();
}

doZoomWork();