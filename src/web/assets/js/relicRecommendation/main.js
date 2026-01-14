//Load plugin
var plugin = overwolf.windows.getMainWindow().plugin;
var mainWindow = overwolf.windows.getMainWindow();

var startTime = Date.now();

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

var errorOccurred = false;

var skipMetrics = false; //Dont send metric if the relics need to load

var functionOnRelicUpdateEvent = (type, data, extra) => {

    console.log("Received update: " + type);

    relicApp.inProgress = false;
    relicApp.progress = 0;

    if (type == "data") {
        relicApp.showData(JSON.parse(data));
        console.log(JSON.parse(data));
        relicApp.loading = false;

        if (!skipMetrics) {
            var endTime = Date.now();
            var timeTakenMS = (endTime - startTime);
            console.log("Time taken: " + timeTakenMS + "ms");

            plugin.get().SendRelicRecommendationMetrics("success", timeTakenMS);
        }
    } else if (type == "progress") {
        skipMetrics = true;
        relicApp.inProgress = true;
        relicApp.setProgress(data);
        relicApp.loadingProgressText = extra;
        relicApp.loading = false;
    } else if (type == "traces") {
        relicApp.traceCount = data;
    }
    else {
        console.log("Relic recommended Error: " + data);
        errorOccurred = true;
        relicApp.error = true;
        relicApp.loading = false;
    }

};
plugin.get().onRelicRecommendationUpdate.addListener(functionOnRelicUpdateEvent);

plugin.get().relicRecommendationReady();



relicApp = Vue.createApp({
    data() {
        return {
            loading: false,
            inProgress: false,
            loading: true,
            progress: 0,
            traceCount: "-",
            error: false,
            loadingProgressText: "Loading relics...",
            items: []
        }
    },
    methods: {
        setProgress(data) {
            this.progress = data;
        },
        showData(data) {
            this.items = data;
        }
    }

}).mount("#vueContainer");

function ExitQuietly(error) {
    console.log("Failed to start window: " + error);
    closeNow();
}

var functionOnCloseEvent = (a) => {
    if (errorOccurred) {
        relicApp.error = true;
        relicApp.loading = false;
        setTimeout(closeNow, 1000);
    } else {
        closeNow();
    }

};
plugin.get().onCloseRelicRecommendation.addListener(functionOnCloseEvent);

overwolf.windows.getCurrentWindow((windowData) => {
    var windowID = windowData.window.id;

    let currentMonitorDPI = windowData.window.dpiScale;
    let selectedZoom = 0;
    if (currentMonitorDPI > 1.4) {
        selectedZoom = -2.5;
    } else if (currentMonitorDPI > 1.2) {
        selectedZoom = -1.5;
    }

    overwolf.windows.setZoom(selectedZoom);
    console.log("Set zoom to " + selectedZoom);
    let zoomMultiplier = 1 / (0.175 * selectedZoom + 1);
    if (zoomMultiplier > 1.15) {
        zoomMultiplier *= 0.85;
    }
    

    console.log("Zoom multiplier: " + zoomMultiplier);


    overwolf.windows.changeSize({
        "window_id": windowID,
        "width": parseInt(480 / zoomMultiplier),
        "height": parseInt(220 / zoomMultiplier),
        "auto_dpi_resize": false
    }, (a) => {
        overwolf.games.getRunningGameInfo2((gameInfo) => {
            if (!gameInfo.success || gameInfo.gameInfo == null) {
                ExitQuietly("Failed to get running game info");
                return;
            }
            overwolf.windows.getCurrentWindow((windowData2) => {
                overwolf.windows.changePosition(windowID, gameInfo.gameInfo.logicalWidth - (windowData2.window.width) - 20, 20, (changePosArgs) => { });
            });



            console.log("Game size: width=" + gameInfo.gameInfo.logicalWidth + ", height=" + gameInfo.gameInfo.logicalHeight);
        });
    });
});

function closeNow() {
    try {
        plugin.get().onRelicRecommendationUpdate.removeListener(functionOnRelicUpdateEvent);
        plugin.get().onCloseRelicRecommendation.removeListener(functionOnCloseEvent);
    } catch {
        console.log("Failed to remove listeners!");
    }
    close();
}