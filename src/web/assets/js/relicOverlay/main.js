var sizeTestingMode = false;
var forceKeepOpen = false;

var autoClose = !forceKeepOpen;
console.log("Size testing mode: " + sizeTestingMode);
console.log("Auto close: " + autoClose);

var startTime = Date.now();

//Load plugin
var plugin = overwolf.windows.getMainWindow().plugin;
var mainWindow = overwolf.windows.getMainWindow();

var windowID = "";
var currentMonitorDPI = 1;

const minAspectRatioToShowAds = 1.45;

var colorData = localStorage.getItem("colorData");
if (colorData != null && !(localStorage["applyThemesToOverlay"] === "false")) {
    var lines = colorData.split('\n');
    for (var i = 0; i < lines.length; i++) {
        try {
            console.log(lines[i]);
            if (lines[i] != "" && lines[i] != undefined) {
                var internalName = lines[i].split(':')[0].replace('--', '').trim();
                var color = lines[i].split(':')[1].replace(';', '').trim();
                document.querySelector(":root").style.setProperty('--' + internalName, color);
            }
        } catch { }
    }
}


if (autoClose) setTimeout(() => {
    owAdsReady = false; //No ads should be shown from this point on
    hide(); 
    setTimeout(() => { close(); }, 1500); //Wait for things to register the window closing (mostly analytics and ads)
}, 16550); //This will hopefully never be called, since a more accurate version of this will be called later when we know more info


relicsApp = Vue.createApp({
    data() {
        return {
            relics: [],
            globalData: {
                platinum: -1,
                ducats: -1
            },
            loading: true,
            error: false,
            errorMessage: "UNKNOWN error"
        }
    },
    methods: {
        relicMaxPrice(relic) {
            return relic.platinum >= Math.max.apply(Math, relicsApp.relics.map(p => p.platinum));
        }
    }
}).mount(".relicPart");

var relicWindowInitialized = false;

function RelicWindowInitialization() {

    if (relicWindowInitialized) {
        console.log("Window already initialized, skipping");
        return;
    }

    relicWindowInitialized = true;

    console.log("Doing window.onload");

    setTimeout(() => {
        sendPage("relic");
    }, 2500);
    
    if (sizeTestingMode) $("body")[0].style.backgroundImage = "url('../../../ML/testPictures/test3.png')";

    overwolf.windows.getCurrentWindow((windowData) => {
        windowID = windowData.window.id;

        overwolf.utils.getMonitorsList((monitorArgs) => {

            overwolf.games.getRunningGameInfo2((result) => {
                
                currentMonitorDPI = monitorArgs.displays.find(element => element.id == windowData.window.monitorId).dpiY / 96;
                console.log("[METHOD_1] Current monitor ID: " + windowData.window.monitorId);
                console.log("[METHOD_1] Detected display DPI: " + currentMonitorDPI);

                /*if (result.success && result.gameInfo.executionPath.includes("Warframe")) {
                    console.log("Running game info valid, applying Display ID fix...");
                    var gameMonitorHandle = result.gameInfo.monitorHandle.value;
                    var gameMonitor = monitorArgs.displays.find(element => element.handle.value == gameMonitorHandle);
                    if (gameMonitor != undefined) {
                        currentMonitorDPI = gameMonitor.dpiY / 96;
                        console.log("[METHOD_2] Fixed game monitor ID: " + gameMonitor.id);
                        console.log("[METHOD_2] Detected display DPI: " + currentMonitorDPI);
                    } else {
                        console.log("Couldn't apply monitor ID fix!");
                    }
                } else {
                    console.log("Running game info not valid. Can not apply Dsiplay ID fix");
                }*/


                SetupRelicWindow(() => {
                    plugin.get().GetRelicWindowData(sizeTestingMode, (success, data, timeLeftSeconds) => {

                        if (autoClose) setTimeout(() => {
                            owAdsReady = false; //No ads should be shown from this point on
                            hide();
                            setTimeout(() => { close(); }, 1500); //Wait for things to register the window closing (mostly analytics and ads)
                        }, parseInt(timeLeftSeconds) * 1000); //Another hopefully ealier call to close everything with the new updated time

                        relicsApp.loading = false;


                        if (success) {
                            console.log("Got relic data successfully!");
                            var deserializedData = JSON.parse(data);
                            console.log(deserializedData);
                            relicsApp.relics = deserializedData.relicRewards;
                            relicsApp.globalData = deserializedData.globalData;
                            if (localStorage["showPlatDucatsSetting"] === "false") {
                                relicsApp.globalData.platinum = -1;
                                relicsApp.globalData.ducats = -1;
                            }

                            try {
                                if (localStorage["copyRewardDataSetting"] === "true") {
                                    var firstTimeSomethingAdded = true;
                                    var messageToSend = "";
                                    for (var i = 0; i < relicsApp.relics.length; i++) {
                                        if (relicsApp.relics[i].detected) {
                                            if (firstTimeSomethingAdded) {
                                                firstTimeSomethingAdded = false;
                                            } else {
                                                messageToSend += "  |   ";
                                            }
                                            messageToSend += "[" + relicsApp.relics[i].name.replace(" Blueprint", "") + "] " + relicsApp.relics[i].platinum + ":platinum:";
                                        }
                                    }
                                    overwolf.utils.placeOnClipboard(messageToSend + " -- (by AlecaFrame)");
                                }
                            } catch (e) {
                                console.log("Failed to set clipboard relic data!");
                            }

                        } else {
                            console.log("Failed to get relic data: " + data);

                            relicsApp.error = true;
                            relicsApp.errorMessage = data;
                        }

                        var endTime = Date.now();
                        var timeTakenMS = (endTime - startTime);
                        console.log("Time taken: " + timeTakenMS + "ms");

                        var statusToSend = "success";
                        if (relicsApp.error) {
                            if (relicsApp.errorMessage.toLowerCase().includes("in time")) {
                                statusToSend = "timeout";
                            } else if (relicsApp.errorMessage.toLowerCase().includes("language detected")) {
                                statusToSend = "language";
                            } else if (relicsApp.errorMessage.toLowerCase().includes("requiem")) {
                                statusToSend = "requiem";
                            } else if (relicsApp.errorMessage.toLowerCase().includes("warframe scaling settings")) {
                                statusToSend = "scaling";
                                localStorage["relicErrorHappenedRecently"] = "true"; //So that the prompt to show the error happens
                            }
                            else {
                                statusToSend = "error";
                            }
                        } else {
                            localStorage["relicErrorHappenedRecently"] = "false"; //A relic was played without the error, cancel the request to prompt the user
                        }
                        plugin.get().SendRelicRewardMetrics(statusToSend, timeTakenMS);

                    });
                });
            });
            
        });
    });   
};


function SetupRelicWindow(callbackWhenDone) {

    overwolf.games.getRunningGameInfo2((gameInfo) => {
        if (!gameInfo.success || gameInfo.gameInfo == null) {
            ExitQuietly("Failed to get running game info");
            return;
        }

        console.log("Game size: width=" + gameInfo.gameInfo.logicalWidth + ", height=" + gameInfo.gameInfo.logicalHeight)

        let selectedZoom = 0;
        if (currentMonitorDPI > 1.4) {
            selectedZoom = -2;
        } else if (currentMonitorDPI > 1.2) {
            selectedZoom = -1;
        }

        let aspectRatio = gameInfo.gameInfo.logicalWidth / gameInfo.gameInfo.logicalHeight;

        overwolf.windows.setZoom(selectedZoom);

        console.log("Selected zoom: " + selectedZoom);

        var pixelBounds = calculateContentPixelBounds(gameInfo.gameInfo.logicalWidth, gameInfo.gameInfo.logicalHeight, selectedZoom);
        var percentBounds = calculateContentPercentBounds(gameInfo.gameInfo.logicalWidth, gameInfo.gameInfo.logicalHeight, pixelBounds, selectedZoom);
        console.log("Bounds: " + JSON.stringify(pixelBounds));

        if (shouldAdsBeShown() && aspectRatio > minAspectRatioToShowAds) {
            document.documentElement.style.fontSize = (((pixelBounds.width) - 400) / 50) + "px";
        } else {
            document.documentElement.style.fontSize = (((pixelBounds.width)) / 50) + "px";
            hideAdEelements();
        }
        
        
        overwolf.windows.changeSize({
            "window_id": windowID,
            "width": sizeTestingMode ? gameInfo.gameInfo.logicalWidth : pixelBounds.width,
            "height": sizeTestingMode ? gameInfo.gameInfo.logicalHeight : pixelBounds.height,
            "auto_dpi_resize": true
        }, (changeSizeArgs) => {

            if (!changeSizeArgs.success) {
                ExitQuietly("Failed to resize window!");
                return;
            }

            overwolf.windows.changePosition(windowID,
                sizeTestingMode ? 0 : pixelBounds.left,
                sizeTestingMode ? 0 : pixelBounds.top, (changePosArgs) => {
                if (!changePosArgs.success) {
                    ExitQuietly("Failed to position window!");
                    return;
                }
                                
                var contentDiv = $(".content")[0];

                contentDiv.style.width = (sizeTestingMode ? percentBounds.width + "%" : "100%");
                contentDiv.style.height = (sizeTestingMode ? percentBounds.height + "%" : "100%");

                contentDiv.style.top = (sizeTestingMode ? percentBounds.top:0) + "%";
                contentDiv.style.left = (sizeTestingMode ? percentBounds.left : 0) + "%";
            })
        });
    });

    callbackWhenDone();
}

function ExitQuietly(error) {
    console.log("Failed to start window: " + error);
    close();
}


function calculateContentPixelBounds(screenWidth,screenHeight, selectedZoom) {

    let aspectRatio = screenWidth / screenHeight;
    let adsShown = shouldAdsBeShown() && aspectRatio > minAspectRatioToShowAds;    

    let zoomMultiplier = 1 / (0.175 * selectedZoom + 1);

    let extraHeight = 60;
    if (!adsShown) extraHeight = 0;


    let minHeight = ((295 + extraHeight) * currentMonitorDPI) / zoomMultiplier;

    ///

    const PXWidthOn1080 = 1000;     

    const PXTopOn1080 = 630;
    const PXoffsetRight1080 = -15*currentMonitorDPI;

    let PXextraWidth = 420 / zoomMultiplier;
    if (!adsShown) {
        PXextraWidth = 0;
    }
        

    //INTERMEDIATE VALUES, DO NOT TOUCH   
    const MultiplierFrom1080 = screenHeight / 1080.0;
    //INTERMEDIATE VALUES, DO NOT TOUCH



   
    var calculatedWidth = PXWidthOn1080 * MultiplierFrom1080 + (PXextraWidth * currentMonitorDPI);
    var calculatedHeight = minHeight;




        
    if (calculatedHeight < minHeight) calculatedHeight = minHeight;

    return {
        width: Math.round(calculatedWidth),
        height: Math.round(calculatedHeight),
        left: Math.round((screenWidth / 2) - (calculatedWidth / 2) + ((PXextraWidth * currentMonitorDPI) / 2) + PXoffsetRight1080),
        top: Math.round(PXTopOn1080 * MultiplierFrom1080)
    }
}

function calculateContentPercentBounds(screenWidth, screenHeight,pixelBounds) {       
    return {
        width: 100*pixelBounds.width / screenWidth,
        height: 100 *pixelBounds.height / screenHeight,
        left: 100 *pixelBounds.left / screenWidth,
        top: 100 *pixelBounds.top / screenHeight
    }
}



window.onload = RelicWindowInitialization;

//Sometimes the onload event doesn't fire, so we have to check for it manually
setTimeout(() => {
    if (!relicWindowInitialized) {
        //If the window hasn't been initialized yet, do it now
        console.log("Window.onload didn't fire, doing it manually");
        RelicWindowInitialization();
    }
}, 350);
