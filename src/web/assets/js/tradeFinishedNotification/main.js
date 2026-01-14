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

tradeApp = Vue.createApp({
    data() {
        return {        
            data: {},
            loadingBarProgress: 100,
            tickVisible: false,
            errorVisible: false,
            loadingVisible: false,
            mouseMoved: false,
            timerWorkDone: false,

            freeText: '',    

            lastError:'',
            
            textEditing: false,
            currentAction: 'reputation'
        }
    },
    methods: {
        finishOK() {
            this.timerWorkDone = true;
            this.loadingVisible = true;

            if (this.data.isContract) {
                plugin.get().WFMContractsRemove(this.data.listingOrContractID, (a) => {
                    this.loadingVisible = false;
                    if (a) {
                        this.tickVisible = true;
                    } else {
                        this.errorVisible = true;
                        this.lastError = "Unknown error";
                    }
                    setTimeout(() => { closeNow(); }, 1000);
                });
                sendMetric("WFM_NotificationMarkAsDone", "contract");
            } else {
                plugin.get().DoWFMarketMarkAsDoneListing(this.data.listingOrContractID,this.data.itemAmount, (a, b) => {
                    this.loadingVisible = false;
                    if (a) {
                        this.tickVisible = true;
                    } else {
                        this.errorVisible = true;
                        this.lastError = b;
                    }
                    setTimeout(() => { closeNow(); }, 1000);
                });
                sendMetric("WFM_NotificationMarkAsDone", "listing");
            }

            this.loadingBarProgress = 0;
        },
        submitClicked() {
            if (this.currentAction == 'report') {
                this.loadingVisible = true;
                plugin.get().SendWFMReport(this.data.remoteUsername,this.freeText, (a, b) => {
                    this.loadingVisible = false;
                    if (a) {
                        this.finishOK();
                    } else {
                        this.errorVisible = true;
                        this.textEditing = false;
                        this.lastError = b;   
                        setTimeout(() => { this.errorVisible = false; }, 3000);
                    }
                });
                sendMetric("WFM_NotificationSendReport", "");
            } else if (this.currentAction == 'reputation') {
                this.loadingVisible = true;
                plugin.get().SendWFMReputation(this.data.remoteUsername, this.freeText, (a, b) => {
                    this.loadingVisible = false;
                    if (a) {                        
                        this.finishOK();
                    } else {
                        this.errorVisible = true;
                        this.textEditing = false;
                        this.lastError = b;
                        setTimeout(() => { this.errorVisible = false; }, 3000);
                    }
                });
                sendMetric("WFM_NotificationSendReputation", "");
            }
        }
    },
    mounted() {
        let data = plugin.get().TradeFinishedNotificationData;
        
        this.data =  JSON.parse(data);
        console.log(JSON.parse(data));    

        setInterval(() => {     

            if (this.timerWorkDone) {
                return;
            }

            if (this.mouseMoved) {
                this.loadingBarProgress = 0;
            } else {
                if (this.loadingBarProgress > 0)
                    this.loadingBarProgress -= 0.9;
                else if (this.loadingBarProgress == -1000) {

                } else {                    
                    this.finishOK();                    
                }       
            }
            
                 
        }, 70);

        tippy('#closeIcon', {         
            content: "You can also disable this window in the settings menu",
            duration: 190,
            arrow: true,
            placement: 'top',
            theme: 'extraVaultedInfoTheme',
            animation: 'fade',
            maxWidth: 250,
            // trigger: 'click',
        });
    }

}).mount("#vueContainer");

function ExitQuietly(error) {
    console.log("Failed to start window: " + error);
    closeNow();
}

overwolf.windows.getCurrentWindow((windowData) => {
    var windowID = windowData.window.id;
    let currentMonitorDPI = windowData.window.dpiScale;

    overwolf.windows.changeSize({
        "window_id": windowID,
        "width": 450,
        "height": 200,
        "auto_dpi_resize": true
    }, (a) => {

        
        let selectedZoom = 0;
        if (currentMonitorDPI > 1.6) {
            selectedZoom = -2;
        }

        overwolf.windows.setZoom(selectedZoom);


        overwolf.games.getRunningGameInfo2((gameInfo) => {
            if (!gameInfo.success || gameInfo.gameInfo == null) {
                ExitQuietly("Failed to get running game info");
                return;
            }
            overwolf.windows.getCurrentWindow((windowData2) => {
                overwolf.windows.changePosition(windowID, gameInfo.gameInfo.logicalWidth - (windowData2.window.width) - 20, gameInfo.gameInfo.logicalHeight - (windowData2.window.height) - 20, (changePosArgs) => { });
            });            

            console.log("Game size: width=" + gameInfo.gameInfo.logicalWidth + ", height=" + gameInfo.gameInfo.logicalHeight);
        });
    });
});

function closeNow() {
    try {
       // plugin.get().onRelicRecommendationUpdate.removeListener(functionOnRelicUpdateEvent);
      //  plugin.get().onCloseRelicRecommendation.removeListener(functionOnCloseEvent);
        //Remove any listeners before closing the window
    } catch {
        console.log("Failed to remove listeners!");
    }
    close();
}

if (localStorage["tradeFinishedOverlay"] === "false") {
    ExitQuietly("Overlay not enabled");
}