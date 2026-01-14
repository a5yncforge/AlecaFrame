//Load plugin
var plugin = overwolf.windows.getMainWindow().plugin;
var mainWindow = overwolf.windows.getMainWindow();

var startTime = Date.now();

overwolf.games.inputTracking.init((a) => { });

var lastGameWidthRealPX = 0;
var lastGameHeightRealPX = 0;

var overlayInteractive = false;

var displayData = [];
var currentGameMonitorHandle = 0;

overwolf.windows.setZoom(-0.5); // Set zoom to 90% to make it fit in small screens

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


rivenOverlayApp = Vue.createApp({
    data() {
        return {
            data: {},
            loading: false,
            selectedModLevel: 8,
            selectedWeaponIndex: 0,
            similarMode: 'all',
            tipNumber: 0,

            leftShown: false,
            rightShown: false,

            shouldRightHide: false

        }
    },
    methods: {
        refresh() {
            plugin.get().GetRivenOverlayData((status, jsonData) => {
                let deserialized = JSON.parse(jsonData);
                //console.log(deserialized);

                if (deserialized.enabled == false) {
                    console.log("Riven overlay disabled, closing...")                   
                    setTimeout(closeNow, 1000);
                    this.data.rivenLeft.show = false;
                    this.data.rivenRight.show = false;
                } else {
                    this.data = deserialized;
                }
            });            
        },
        selectSimilarMode(newMode) {
            this.similarMode = newMode;
        },
        openWebRivenClicked(link) {
            overwolf.utils.openUrlInDefaultBrowser(link);
        }, openAFRivenClicked(source, link) {            
            overwolf.windows.obtainDeclaredWindow("MainWindow", (result) => {
                overwolf.windows.sendMessage(result.window.id, "openWFMRiven", link, (a) => {
                    showSuccesfulToast("Riven details opened in the main desktop window")
                });
            });
        }
    }, mounted() {
        setInterval(() => { this.tipNumber = (this.tipNumber+1) % 2 }, 7000);
    }

}).mount("#vueContainer");

function ExitQuietly(error) {
    console.log("Failed to start window: " + error);
    closeNow();
}


function RelocateWindowAndRefresh() {
    overwolf.windows.getCurrentWindow((windowData) => {
        var windowID = windowData.window.id;

        overwolf.games.getRunningGameInfo2((gameInfo) => {
            if (!gameInfo.success || gameInfo.gameInfo == null) {
                ExitQuietly("Failed to get running game info");
                return;
            }

            //Only run this if the game size has changed (overwolf raises one of this events sometimes with the same size, and that creates problems/artifacts)
            if (lastGameWidthRealPX == gameInfo.gameInfo.logicalWidth && lastGameHeightRealPX == gameInfo.gameInfo.logicalHeight) {
                return;
            }
            

            overwolf.windows.getCurrentWindow((windowData2) => {
                overwolf.windows.changePosition(windowID, 0, 0, (changePosArgs) => { });
            });

            overwolf.utils.getMonitorsList((monitors) => {
                if (!monitors.success) return;
                displayData = monitors.displays;
            });

            //console.log("Game size: log_width=" + gameInfo.gameInfo.width + ", log_height=" + gameInfo.gameInfo.height);

            let gameScaled = gameInfo.gameInfo.logicalWidth / gameInfo.gameInfo.width > 1.22;

            currentGameMonitorHandle = gameInfo.gameInfo.monitorHandle.value;

            let selectedScale = -0.5;

            if (gameInfo.gameInfo.height < 750) {
                if (gameScaled) {
                    selectedScale=-2.25;
                } else {
                    selectedScale=-2;
                }
            }else if (gameInfo.gameInfo.height < 850) {  
                if (gameScaled) selectedScale=-1.5;
            } else {
                selectedScale=-0.5;
            }

            //console.log("Selected scale: " + selectedScale);
            overwolf.windows.setZoom(selectedScale);

            overwolf.windows.changeSize({
                "window_id": windowID,
                "width": gameInfo.gameInfo.logicalWidth,
                "height": gameInfo.gameInfo.logicalHeight,
                "auto_dpi_resize": true
            }, (a) => {
                lastGameWidthRealPX = gameInfo.gameInfo.logicalWidth;
                lastGameHeightRealPX = gameInfo.gameInfo.logicalHeight;
            });

            //console.log("Game size: log_width=" + gameInfo.gameInfo.logicalWidth + ", log_height=" + gameInfo.gameInfo.logicalHeight);

            rivenOverlayApp.refresh();
        });
    });
}


function functionOnRivenChange(aa) {
    rivenOverlayApp.refresh();
}
plugin.get().onRivenOverlayChange.addListener(functionOnRivenChange);

function onResolutionChanged(data) {
    overlayInteractive = data.gameInfo.overlayInfo.isCursorVisible;
    if (data.resolutionChanged) {
        RelocateWindowAndRefresh();
    }

}
overwolf.games.onGameInfoUpdated.addListener(onResolutionChanged);


function closeNow() {
    try {
        overwolf.games.onGameInfoUpdated.removeListener(onResolutionChanged);
        plugin.get().onRivenOverlayChange.removeListener(functionOnRivenChange);
    } catch {
        console.log("Failed to remove listeners!");
    }
    close();
}

setInterval(() => {
    overwolf.games.inputTracking.getMousePosition((a) => {
        if (!a.success) return;

        if (overlayInteractive) { //In interactive mode we don't hide the overlay
            rivenOverlayApp.shouldRightHide = false;
            return;
        }

        let currentMonitor = displayData.find((x) => x.handle.value == currentGameMonitorHandle);
        //If we are not in the overlay interactive mode and we know our current monitor, we need to substract the general monitor offset from the mouse position
        let x = a.mousePosition.x - currentMonitor?.x ?? 0;
        let y = a.mousePosition.y - currentMonitor?.y ?? 0;

        let mouseOnBottomRight = (y > (0.66 * lastGameHeightRealPX)) && (x > 0.73 * lastGameWidthRealPX);
        rivenOverlayApp.shouldRightHide = mouseOnBottomRight;
    });
}, 200);

RelocateWindowAndRefresh();

