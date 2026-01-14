var plugin = new OverwolfPlugin("AlecaFrameWrapper", true);


/////////////
var devMode = false;
var relicDebugMode = false;
/////////////


var squadIDToJoin = undefined;

var ready = false;
var errorLoading = false;
var inventoryDataReady = false;
var initializingInProgress = true;
var initializationSuccessful = false;

var lastInGameNotificationText = "";
var lastInGameNotificationIcon = "";
var lastInGameNotificationSource = "";


function closeBackground() {
    close();
}

console.log("Instantiating plugin...");
plugin.initialize(function (status) {
    console.log("Plugin instantiated. Doing early initialization...")

    if (status == false) {

        console.log("Plugin couldn't be loaded??");
        return;
    } else {

        plugin.get().DoEarlyInitialization((res, reason) => {
            if (!res) {
                console.log("Plugin early initialization failed: " + reason);
                return;
            }

            console.log("Plugin early initialization completed!");

            plugin.get().logInBackground.addListener(function (type, text) {
                switch (type) {
                    case "INFO":
                        console.log("[INFO] " + text);
                        break;
                    case "WARN":
                        console.warn("[WARN] " + text);
                        break;
                    case "ERROR":
                        console.error("[ERROR] " + text);
                        break;
                }
            });

            plugin.get().onRelicScreenshotRequest.addListener(function (arg) {
                overwolf.media.takeWindowsScreenshotByName("Warframe", false, (fileData) => {

                    if (fileData.success || relicDebugMode) {
                        plugin.get().SetNewRelicScreenshot(fileData.path, relicDebugMode, () => { });
                    } else {
                        console.log("Failed to take OW screenshot for relic window!");
                    }

                });
            });

            plugin.get().onRelicOpenRequest.addListener((arg) => {
                console.log("Opening relic window...");

                obtainDeclaredWindowAsync("RelicOverlay").then((arga) => {
                    closeWindowAsync(arga.window.id).then(() => {
                        obtainDeclaredWindowAsync("RelicOverlay").then((arga) => {
                            openWindowAsync(arga.window.id);
                        });
                    });
                });
            });

            plugin.get().onOpenRelicRecommendation.addListener((arg) => {
                console.log("Opening relic recommendation...");

                obtainDeclaredWindowAsync("RelicRecommendation").then((arga) => {
                    closeWindowAsync(arga.window.id).then(() => {
                        obtainDeclaredWindowAsync("RelicRecommendation").then((arga) => {
                            openWindowAsync(arga.window.id);
                        });
                    });
                });
            });

            plugin.get().onRivenOverlayOpen.addListener((arg) => {
                console.log("Opening riven overlay...");

                obtainDeclaredWindowAsync("RivenOverlay").then((arga) => {
                    obtainDeclaredWindowAsync("RivenOverlay").then((arga) => {
                        openWindowAsync(arga.window.id);
                    });
                });
            });

            plugin.get().OnTradeFinishedNotification.addListener((arg) => {
                console.log("Opening trade finished overlay...");

                obtainDeclaredWindowAsync("TradeFinishedNotification").then((arga) => {
                    closeWindowAsync(arga.window.id).then(() => {
                        obtainDeclaredWindowAsync("TradeFinishedNotification").then((arga) => {
                            openWindowAsync(arga.window.id);
                        });
                    });
                });
            });

            plugin.get().OnInGameNotification.addListener((notificationText, notificationIcon, notificationSource) => {
                console.log("Opening in-game notification overlay with text " + notificationText);
                lastInGameNotificationText = notificationText;
                lastInGameNotificationIcon = notificationIcon;
                lastInGameNotificationSource = notificationSource;

                obtainDeclaredWindowAsync("InGameNotification").then((arga) => {
                    closeWindowAsync(arga.window.id).then(() => {
                        obtainDeclaredWindowAsync("InGameNotification").then((arga) => {
                            openWindowAsync(arga.window.id);
                        });
                    });
                });
            });    

            overwolf.profile.getCurrentUser((userProfile) => {
                var owID = userProfile.userId;
                if (owID == undefined) owID = "N/A";

                var campaign = "none";
                if (userProfile.installParams != undefined) campaign = userProfile.installParams.campaign;
                if (campaign == undefined) campaign = "none";

                plugin.get().SetAnalyticsData(owID, campaign, (a) => {
                    overwolf.extensions.current.getManifest((manifest) => {
                        console.log("Extension version: " + manifest.meta.version);
                        plugin.get().setLocalVersion(manifest.meta.version); //This will run synchronously

                        showMainWindow();

                        console.log("Plugin initialization started...");

                        plugin.get().Initialize((res, data) => {
                            initializationSuccessful = res;
                            initializingInProgress = false;
                            if (res) {

                                getInventoryData();                                

                                errorLoading = false;
                                ready = true;
                                console.log("Plugin initialization successful!");
                            } else {
                                console.error("Plugin initialization error!");
                                errorLoading = true;
                                ready = false;
                            }
                        });
                    });
                });
            });
        });
    }    
});

onErrorListener = function (info) {
    console.log("EVEMT Error: " + JSON.stringify(info));
}
overwolf.games.events.onError.addListener(onErrorListener);



var g_interestedInFeatures = [
    'inventory',
    'match_info'
];
function setRequiredFeatures() {
    overwolf.games.events.setRequiredFeatures(g_interestedInFeatures, function (info) {
        if (info.success == false) {
           // console.log("Could not set required features: " + info.error);
            setTimeout(setRequiredFeatures, 3000);
        } else {
            console.log("Required features set successfully");
        }
    });
}

setRequiredFeatures();

overwolf.games.events.onInfoUpdates2.addListener(function (info) {    
    if (info != undefined && info.info != undefined && info.info.match_info != undefined) {
        if (info.info.match_info.inventory != undefined) {
            plugin.get().SetWarframeData(info.info.match_info.inventory, SetInventoryDataFeedback);
            console.log("New inventory data (" + info.info.match_info.inventory.length+" B): " + info.info.match_info.inventory.substring(0, Math.min(100, info.info.match_info.inventory.length)));
        } else {
            if (info.info.match_info.highlighted != undefined) {
                plugin.get().ItemJustHighlighted(info.info.match_info.highlighted)
                //console.log("New highlighted data: " + info.info.match_info.highlighted)
            } else {
                var toLog = JSON.stringify(info.info.match_info);
                console.log("Other new data:" + toLog.substring(0, Math.min(100, toLog.length)));
            }    
        }
    } else {
        var toLog = info;
        console.log("Other weird data:" + toLog.substring(0, Math.min(100, toLog.length)));
    }
    
  

});


function CloseWithDelay(time) {

    try {
        plugin.get().DoOnClosingWork();
    } catch (e) {
        console.error("Failed to do on closing work: " + e);
    }

    setTimeout(() => {
        close();
    }, time);
 }

function getInventoryData() {
    overwolf.games.events.getInfo((p) => {
        try {
            if (devMode) {
                plugin.get().SetWarframeData("|USE_DEMO|", SetInventoryDataFeedback);
            } else {
                if (p.success) {
                  //  console.log(JSON.stringify(p));
                    if (p.res.match_info != undefined && p.res.match_info.inventory != undefined) {
                        plugin.get().SetWarframeData(p.res.match_info.inventory, SetInventoryDataFeedback);
                    } else {
                        plugin.get().SetWarframeData("|NOT_AVAILABLE|", SetInventoryDataFeedback);
                        console.log("Game is open,but can not get warframe data");
                    }
                } else {
                    plugin.get().SetWarframeData("|NOT_AVAILABLE|", SetInventoryDataFeedback);
                    //console.log("Game is closed, can not get warframe data");
                }
            }            
        } catch (e) {
            plugin.get().SetWarframeData("|NOT_AVAILABLE|", SetInventoryDataFeedback);
            console.log("An unknown error happened, can not get warframe data");
        }        
    });
}

var lastWarframeDataReady = false;
var lastWarframeDataIsCached = false;
var lastWarframeErrorCode = -1;

function SetInventoryDataFeedback(dataReady, isCached,errorCode) {
    lastWarframeDataReady = dataReady;
    lastWarframeDataIsCached = isCached;
    lastWarframeErrorCode = errorCode;
    if (lastWarframeErrorCode == undefined) lastWarframeErrorCode = -1;
}


overwolf.windows.onMainWindowRestored.addListener(function () {
    showMainWindow();
});

function showMainWindow() {
    //  closeWindowAsync("MainWindow").then((arg15) => { //Forces a new window to be created
    
    obtainDeclaredWindowAsync("MainWindow").then((arg) => {
        openWindowAsync(arg.window.id);
    });
    // });
    mainWindowShown = true;
}



function closeMainWindow() {
    closeWindowAsync("MainWindow");
    mainWindowShown = false;
}

function hideMainWindow() {
    hideWindowAsync("MainWindow");
    mainWindowShown = false;
}

