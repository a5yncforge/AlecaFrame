
//Load plugin

var plugin = overwolf.windows.getMainWindow().plugin;
var mainWindow = overwolf.windows.getMainWindow();


var lastSelectedTab = "tabFoundry"; //ID of the currently selected tab
var deferredTabUpdatesCompleted = [
    "tabFoundry"
]; //Used to store the tabs that have already been updated in a deferrred way

$(".tab").eq(1).hide();
$(".tab").eq(2).hide();
$(".tab").eq(3).hide();
$(".tab").eq(4).hide();
$(".tab").eq(5).hide();
$(".tab").eq(6).hide();
$(".tab").eq(7).hide();
$(".tab").eq(8).hide();

setInterval(UpdateStatusMessage, 1000);
setTimeout(UpdateStatusMessage, 100);

if (!(localStorage["moveAFToSecondary"] === "false")) {

    overwolf.windows.getCurrentWindow((windowData) => {
        //console.log(windowData);
        var windowID = windowData.window.id;

        overwolf.utils.getMonitorsList((result) => {
            //console.log(result)
            if (result.success && result.displays.length > 1) {
                let nonPrimaryDisplay = result.displays.find(p => p.is_primary == false);
                let targetX = nonPrimaryDisplay.x;
                let targetY = nonPrimaryDisplay.y;

                if (nonPrimaryDisplay.width > windowData.window.width) {
                    targetX += (nonPrimaryDisplay.width - windowData.window.width) / 2;

                }

                if (nonPrimaryDisplay.height > windowData.window.height) {
                    targetY += (nonPrimaryDisplay.height - windowData.window.height) / 2;
                }

                overwolf.windows.changePosition(windowID, parseInt(targetX, 10), parseInt(targetY, 10), () => { });


                console.log("Multiple monitors detected. Opening in the second monitor to avoid being intrusive");

            } else {
                console.log("Single monitor detected. Skipping second monitor optimization");
            }
        });

        // overwolf.windows.changeSize(sizeSettings, () => { });
    });
} else {
    console.log("Move to secondary monitor is disabled");
}


overwolf.profile.subscriptions.onSubscriptionChanged.addListener((data) => {    
    setSubscriptionStatus("noupdate"); //This will check internally whether the user is subscribed through overwolf or not
});

setTimeout(DoSubscriptionWork, 1000);
setTimeout(DoSubscriptionWork, 2500); //Just in case

setInterval(() => {
    //Show the relic help modal if the flag is set and it hasn't been shown in a while
    if (localStorage["relicErrorHappenedRecently"] === "true" && (localStorage["lastRelicHelpShowedTime"] === undefined || (new Date() - new Date(parseInt(localStorage["lastRelicHelpShowedTime"]))) > 3 * 24 * 60 * 60 * 1000)) {
        localStorage["lastRelicHelpShowedTime"] = new Date().getTime();
        localStorage["relicErrorHappenedRecently"] = "false";
        scaleErrorApp.open();
    }
}, 60000);

function DoSubscriptionWork() {      

    overwolf.settings.getExtensionSettings((extensionSettingsResult) => {
        if (getSubscriptionStatus() != 'None' || extensionSettingsResult.settings.channel == 'FastBeta') {
            settingsApp.themesEnabled = true;
        } 

        if (getSubscriptionStatus() == 'PatreonT2' || getSubscriptionStatus() == 'PatreonT3') {
            settingsApp.premiumFeaturesEnabled = true;
            proAnalyticsApp.gotPatreon = true;
            $(".subscriberIcon.tooltip").hide();
        } else {
            tippy('.subscriberIcon.tooltip', {
                duration: 190,
                arrow: true,
                //  delay: [0, 0],
                //offset: [0, 15],
                interactive: true,
                placement: 'top',
                theme: 'extraVaultedInfoTheme',
                animation: 'fade',
                allowHTML: true,
                //  trigger: 'click',
            });
        }


    });
}


window.onload = function () {
    DoStartupThingsWhenLoaded();       
};

function DoStartupThingsWhenLoaded() {

    //If still loading, just wait
    if (mainWindow.ready == false && mainWindow.errorLoading == false) {
        setTimeout(DoStartupThingsWhenLoaded, 250);
    }

    if (mainWindow.ready) {
        lastFoundryFilter = "";
        Refreshfoundry();    
        RefreshInventory();
        lastRelicPlannerFilter = "";
        RefreshRelicPlanner();
        rivenSniperApp.refresh(true);

        plugin.get().GetBannerData((bannerEnabled, bannerMessage) => {
            if (bannerEnabled) {
                console.log("Banner enabled: " + bannerMessage);
                $("#bannerText").html(bannerMessage);
                $("#banner").css('display', 'flex');
            } else {
                console.log("Banner disabled: " + bannerMessage);
            }
        });
    }

    if (mainWindow.errorLoading) {
        //SHOW LOADING ERROR
    }

    updatePersistentSettingsDisplay();

    if (!(localStorage["initialScaleAutodetectDone"] === "true")) {
        settingsApp.scaleAutodetect(true, true); //Auto detect scale wihtout showing notifications and bypassing the "game open" check
        localStorage["initialScaleAutodetectDone"] = "true";
    }
}


$(".expandableMenu").on("click", function (event) {
    event.stopPropagation();
});

$(".showMenuButton").on("click", function (event) {
    event.stopPropagation();
});


function menuPressed(event) {

    event.preventDefault();

    var target = event.target;
    while ($(target).hasClass("menuItem") == false) {
        target = target.parentElement;
    }

    var tabClicked = $(target).attr("tabId");

    if (tabClicked == "tabSquadMaker") {
        obtainDeclaredWindowAsync("SquadMakingMain").then((arga) => {
            openWindowAsync(arga.window.id);
        });
        return;
    }

    if (tabClicked == "tabAFBuilds") {
        obtainDeclaredWindowAsync("AFBuilds").then((arga) => {
            openWindowAsync(arga.window.id);
        });
        return;
    }
       
    $(".menuItem").removeClass("selected");
    $(target).addClass("selected");
      
    $(".tab").hide();
    $("#" + $(target).attr("tabId")).show();

    lastSelectedTab = tabClicked;

    //Check if deferredTabUpdatesCompleted contains the tabID

    if (!deferredTabUpdatesCompleted.includes(lastSelectedTab)) {
        deferredTabUpdatesCompleted.push(lastSelectedTab);

        RefreshTabByTabID(lastSelectedTab);
    }   

    sendMetric("Menu_NavigateTab", lastSelectedTab);
}

function UpdateStatusMessage() {
    if (mainWindow.lastWarframeDataReady) {
        if (mainWindow.lastWarframeDataIsCached) {
            setStatusMessage("cache");
        } else {
            setStatusMessage("OK!");            
        }
    } else {
        if (mainWindow.lastWarframeDataIsCached) {
            setStatusMessage("errorButCached");
        } else {
            setStatusMessage("error");
        }
       
    }
    
    
}

var lastWarframeDataLocalStatus = "error";

function setStatusMessage(status) {

    lastWarframeDataLocalStatus = status; //Used in the help modal

    if (status == "OK!") {        
        $(".currentStatus.OK")[0].classList.remove("hidden");
        $(".currentStatus.CACHE")[0].classList.add("hidden");
        $(".currentStatus.NOTOPEN")[0].classList.add("hidden");
        $("#textWhenUpdated").text(plugin.get().lastDataUpdateTime);
    } else if (status == "cache" || status =="errorButCached") {
        $(".currentStatus.OK")[0].classList.add("hidden");
        $(".currentStatus.CACHE")[0].classList.remove("hidden");
        $(".currentStatus.NOTOPEN")[0].classList.add("hidden");
    } else {
        $(".currentStatus.OK")[0].classList.add("hidden");
        $(".currentStatus.CACHE")[0].classList.add("hidden");
        $(".currentStatus.NOTOPEN")[0].classList.remove("hidden");
    }        
}

var lastActivitySignalTimestamp = 0;

//FIX FOR ADS: Overwolf does not recognize that the app window has been restored unless it was explicitelly minimzed with the - button, so this is a workaround for that
window.onclick = function (event) {
    let initialState = isOWAdsCurrentlyStopped;
    if ((isOWAdsCurrentlyStopped && (Date.now() - lastOWAdEventTimestamp) > 5000) || ((Date.now() - lastOWAdEventTimestamp) > 90000 && lastOWAdEventTimestamp != -1)) {        
        isOWAdsCurrentlyStopped = false;
        lastOWAdEventTimestamp = Date.now();
        sendMetric("Ad_" + "FIX", initialState ? "fromStop" : "fromNormal");        
        
        //console.log("ads currently running");
        $("#mainADinner").hide()
        setTimeout(() => {
            $("#mainADinner").show();
        },2500);        
    }

    if ((Date.now() - lastActivitySignalTimestamp) > 5000) {
        lastActivitySignalTimestamp = Date.now();
        plugin.get().UserIsActive();
    }
}



function onPersistentSettingChanged() {    
    localStorage["copyRewardDataSetting"] = $("#copyRewardDataSetting")[0].checked;
    localStorage["showPlatDucatsSetting"] = $("#showPlatDucatsSetting")[0].checked;
    localStorage["showWarningCloseWindowSetting"] = $("#showWarningCloseWindowSetting")[0].checked;  
    localStorage["selectedWarframeScaling"] = settingsApp.scalingMode;
    localStorage["selectedWarframeCustomScaling"] = settingsApp.customScale;
    localStorage["hideFoundersProgramRewardsSetting"] = $("#hideFoundersProgramRewardsSetting")[0].checked;  
    localStorage["showRelicOverlaySetting"] = $("#showRelicOverlaySetting")[0].checked;   
    localStorage["showRelicRecommendationSetting"] = $("#showRelicRecommendationSetting")[0].checked;

    localStorage["windowsWFMessageEnabled"] = settingsApp.windowsWFMessageEnabled;
    localStorage["discordWFMessageEnabled"] = settingsApp.discordWFMessageEnabled;  
    localStorage["onlyNotificationsBackground"] = settingsApp.onlyNotificationsBackground;  
    localStorage["discordWFMessageWebhook"] = settingsApp.discordWFMessageWebhook == undefined ? "" : settingsApp.discordWFMessageWebhook;
    localStorage["discordWFMessageTemplate"] = settingsApp.discordWFMessageTemplate == undefined ? "" : settingsApp.discordWFMessageTemplate;
    localStorage["takeRankIntoAccount"] = settingsApp.takeRankIntoAccount;

    localStorage["autoKeepEnabled"] = settingsApp.autoKeepEnabled;
    localStorage["skipWarningAutoKeep"] = settingsApp.skipWarningAutoKeep;

    localStorage["includeFormaLevelMasteryHelper"] = settingsApp.includeFormaLevelMasteryHelper;

    localStorage["applyThemesToOverlay"] = settingsApp.applyThemesToOverlay;

    localStorage["minutesAheadTimers"] = settingsApp.minutesAheadTimers;
    
    localStorage["statsTabEnabled"] = settingsApp.statsTabEnabled;

    localStorage["tradeFinishedOverlay"] = settingsApp.tradeFinishedOverlay;

    localStorage["soundNotificationsEnabled"] = settingsApp.soundNotificationsEnabled;

    localStorage["fullInventoryExperienceEnabled"] = settingsApp.fullInventoryExperienceEnabled;

    localStorage["moveAFToSecondary"] = settingsApp.moveAFToSecondary;

    localStorage["enableRivenOverlay"] = settingsApp.enableRivenOverlay;

    localStorage["discordWarframeMarketTemplate"] = settingsApp.discordWarframeMarketTemplate == undefined ? "" : settingsApp.discordWarframeMarketTemplate;

    UpdatePluginSettings();
}

function releaseChannelChanged() {
    overwolf.settings.setExtensionSettings({ channel: settingsApp.releaseChannel }, (result) => {
        if (result.success) {
            overwolf.settings.getExtensionSettings((callbackData) => {
                if (callbackData.success) {
                    let channelName = callbackData.settings.channel;
                    if (channelName == "") channelName = "Stable";
                    showSuccesfulToast("Release channel changed to " + channelName + ". Please restart Overwolf and force an update from the About tab");
                } else {
                    showErrorToast("Failed to set release channel. Error 1");
                }
            });
        } else {
            showErrorToast("Failed to set release channel. Error 2");
        }
    });
}

function updatePersistentSettingsDisplay() {
    $("#copyRewardDataSetting")[0].checked = localStorage["copyRewardDataSetting"] === "true"; //Default false
    $("#showPlatDucatsSetting")[0].checked = !(localStorage["showPlatDucatsSetting"] === "false"); //Default true
    $("#showWarningCloseWindowSetting")[0].checked = !(localStorage["showWarningCloseWindowSetting"] === "false"); //Default true
    

    var selectedScaling = localStorage["selectedWarframeScaling"];
    if (selectedScaling == undefined) selectedScaling = "full";
    settingsApp.scalingMode = selectedScaling;    
    
    settingsApp.customScale = localStorage["selectedWarframeCustomScaling"];
    if (settingsApp.customScale == undefined) settingsApp.customScale = 100;    

    $("#hideFoundersProgramRewardsSetting")[0].checked = localStorage["hideFoundersProgramRewardsSetting"] === "true"; //Default false 
    $("#showRelicOverlaySetting")[0].checked = !(localStorage["showRelicOverlaySetting"] === "false"); //Default true    
    $("#showRelicRecommendationSetting")[0].checked = !(localStorage["showRelicRecommendationSetting"] === "false"); //Default true

    settingsApp.windowsWFMessageEnabled = !(localStorage["windowsWFMessageEnabled"] === "false"); //Default false 
    settingsApp.discordWFMessageEnabled = localStorage["discordWFMessageEnabled"] === "true"; //Default false 
    settingsApp.discordWFMessageWebhook = localStorage["discordWFMessageWebhook"]; //Default false 
    settingsApp.onlyNotificationsBackground = !(localStorage["onlyNotificationsBackground"] === "false"); //Default true
    settingsApp.discordWFMessageTemplate = localStorage["discordWFMessageTemplate"]; //Default false 
    settingsApp.discordWarframeMarketTemplate = localStorage["discordWarframeMarketTemplate"]; //Default false
    if (settingsApp.discordWFMessageTemplate == undefined) settingsApp.discordWFMessageTemplate = "New in-game conversation from **<PLAYER_NAME>**";    
    if (settingsApp.discordWarframeMarketTemplate == undefined) settingsApp.discordWarframeMarketTemplate = "New <:warframe:771543444178337792> WFMarket message: **<WFM_MESSAGE>**";

    settingsApp.includeFormaLevelMasteryHelper = !(localStorage["includeFormaLevelMasteryHelper"] === "false"); //Default true

    settingsApp.takeRankIntoAccount = !(localStorage["takeRankIntoAccount"] === "false"); //Default true

    settingsApp.skipWarningAutoKeep = !(localStorage["skipWarningAutoKeep"] === "false"); //Default true
    settingsApp.autoKeepEnabled = localStorage["autoKeepEnabled"] === "true"; //Default false

    settingsApp.applyThemesToOverlay = !(localStorage["applyThemesToOverlay"] === "false"); //Default true

    settingsApp.minutesAheadTimers = localStorage["minutesAheadTimers"] ?? 3;

    settingsApp.statsTabEnabled = !(localStorage["statsTabEnabled"] === "false"); //Default true

    settingsApp.tradeFinishedOverlay = !(localStorage["tradeFinishedOverlay"] === "false"); //Default true

    settingsApp.soundNotificationsEnabled = !(localStorage["soundNotificationsEnabled"] === "false"); //Default true

    settingsApp.fullInventoryExperienceEnabled = localStorage["fullInventoryExperienceEnabled"] === "true"; // Default false

    settingsApp.moveAFToSecondary = !(localStorage["moveAFToSecondary"] === "false"); //Default true;

    settingsApp.enableRivenOverlay = !(localStorage["enableRivenOverlay"] === "false"); //Default true;

    overwolf.settings.getExtensionSettings((callbackData) => {
        if (callbackData.success) {
            settingsApp.releaseChannel = callbackData.settings.channel;           
        } else {
           console.log("Failed to get release channel");
        }
    });

    UpdatePluginSettings();  
}

function UpdatePluginSettings() {
    plugin.get().CustomScalingChanged(settingsApp.customScale, (a) => { });
    plugin.get().SelectedScalingChanged(settingsApp.scalingMode, (a) => { });
    plugin.get().HideFoundersProgramChanged(localStorage["hideFoundersProgramRewardsSetting"] === "true", (a) => { });
    plugin.get().ShowRelicOverlayChanged(!(localStorage["showRelicOverlaySetting"] === "false"), (a) => { });
    plugin.get().ShowRelicRecommendationChanged(!(localStorage["showRelicRecommendationSetting"] === "false"), (a) => { });
    plugin.get().ConversationNotificationsChanged(settingsApp.windowsWFMessageEnabled, settingsApp.discordWFMessageEnabled, settingsApp.discordWFMessageWebhook, settingsApp.onlyNotificationsBackground, settingsApp.discordWFMessageTemplate, settingsApp.minutesAheadTimers?.toString(), settingsApp.discordWarframeMarketTemplate);
    plugin.get().SetSettingsGeneric(settingsApp.takeRankIntoAccount, settingsApp.statsTabEnabled, settingsApp.soundNotificationsEnabled, settingsApp.includeFormaLevelMasteryHelper, settingsApp.enableRivenOverlay);
}

setTimeout(() => {
    updatePersistentSettingsDisplay();
}, 2500); //Just in case


setTimeout(() => {
    sendPage("main");
}, 2500);




plugin.get().OnDeltaUpdate.addListener((deltaCount) => {
    if (deltaCount > 0) {
        $("#deltaViewerNotificationMessage").text(deltaCount + " new items found");
        $(".deltaViewerNotification").addClass("shown");
    } else {
        $(".deltaViewerNotification").removeClass("shown");
    }
});

setTimeout(() => {
    plugin.get().GenerateDeltaEvent();
}, 500);

setTimeout(() => {
    if (localStorage["autoKeepEnabled"] === "true" && localStorage["autoModeLastStatus"] === "true") {
        wfStatusApp.autoMode = true;
        wfStatusApp.statusChanged(false);
    }
}, 1000);


loadingScreenInterval = setInterval(doLoadingScreenWork, 500);

function doLoadingScreenWork() {
    if (mainWindow.initializingInProgress) return;

    if (mainWindow.initializationSuccessful) {
        $("#loadingScreen").addClass("hidden");
        setTimeout(() => {
            $("#loadingScreen")[0].style.display = "none";
            changelogApp.openIfNeccessary();
            FTUEapp.openIfNeccessary();
        }, 550);
    } else {
        $("#loadingContainerloading")[0].style.display = "none";
        $("#loadingContainerError")[0].style.display = "flex";   
        $("#loadingContainerErrrorMessage").text("Error: " + plugin.get().GetInitializationError());
    }

    clearInterval(loadingScreenInterval);
}



function refreshMainTabs() {
    foundryApp.refresh();
    inventoryApp.refresh();
    relicPlannerApp.refresh();
}

plugin.get().OnFavouritesUpdate.removeListener(refreshMainTabs);
plugin.get().OnFavouritesUpdate.addListener(refreshMainTabs);

var lastTimeWarframeJustUpdatedTimestamp = 0;
function WarframeDataJustUpdated() {

    if (lastTimeWarframeJustUpdatedTimestamp > Date.now() - 3000) return;
    lastTimeWarframeJustUpdatedTimestamp = Date.now();

    RefreshTabByTabID(lastSelectedTab);
    deferredTabUpdatesCompleted = [];
    deferredTabUpdatesCompleted.push(lastSelectedTab);

    worldTimersApp.lastTimeDetailedRequested = 0;
}

function RefreshTabByTabID(tabID, ) {
    switch (tabID) {
        case "tabFoundry":
            foundryApp.refresh();
            break;
        case "tabInventory":
            inventoryApp.refresh();
            break;
        case "tabRelicPlanner":
            relicPlannerApp.refresh();
            break;
        case "tabRivenExplorer":
            veiledRivensApp.refresh();
            unveiledRivensApp.refresh();            
            rivenSniperApp.refresh();
            break;
        case "tabWarframeMarket":
            wfmOrdersApp.refresh();
            break;
        case "tabMasteryHelper":
            masteryHelperApp.refresh();
            break;
        case "tabStats":
            if (!statsApp.refreshedAtLeastOnce) statsApp.refresh(); //The refreshing for the stats tab is handled separatelly
            break;
        case "tabResources":
            resourcesApp.refresh();
            break;
    }
}

plugin.get().onWarframeDataChanged.removeListener(WarframeDataJustUpdated);
plugin.get().onWarframeDataChanged.addListener(WarframeDataJustUpdated);