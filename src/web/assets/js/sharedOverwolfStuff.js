

var mainWindow = overwolf.windows.getMainWindow();
var plugin = mainWindow.plugin;



//START OF PLUGIN STUFF
function OverwolfPlugin(extraObjectNameInManifest, addNameToObject) {
    var _pluginInstance = null;
    var _extraObjectName = extraObjectNameInManifest;
    var _addNameToObject = addNameToObject;

    // public
    this.initialize = function (callback) {
        return _initialize(callback);
    }

    this.initialized = function () {
        return _pluginInstance != null;
    };

    this.get = function () {
        return _pluginInstance;
    };

    // privates
    function _initialize(callback) {
        var proxy = null;

        try {
            proxy = overwolf.extensions.current.getExtraObject;
        } catch (e) {
            console.error(
                "overwolf.extensions.current.getExtraObject doesn't exist!");
            return callback(false);
        }

        proxy(_extraObjectName, function (result) {
            if (result.status != "success") {
                console.error(
                    "failed to create " + _extraObjectName + " object: " + result);
                return callback(false);
            }

            _pluginInstance = result.object;

            if (_addNameToObject) {
                _pluginInstance._PluginName_ = _extraObjectName;
            }

            return callback(true);
        });
    }
}
//END OF PLUGIN STUFF



//START OF WINDOW FUNCTIONS
function dragMove(event) {

    if (event.target.classList.contains("foundryWorldStats") && event.offsetY > 63) return;
    overwolf.windows.getCurrentWindow(function (result) {
        overwolf.windows.dragMove(result.window.id);
        changeMaximizeStatus(false);
    });
};
function closeWindow() {
    overwolf.windows.getCurrentWindow(function (result) {
        if (result.status == "success") {
            overwolf.windows.close(result.window.id);
        }
    });
};
function minimize() {
    overwolf.windows.getCurrentWindow(function (result) {
        if (result.status == "success") {
            overwolf.windows.minimize(result.window.id);
        }
    });
};
function maximize() {
    overwolf.windows.getCurrentWindow(function (result) {
        if (result.status == "success") {
            overwolf.windows.maximize(result.window.id);
        }
    });
};
function restore() {
    overwolf.windows.getCurrentWindow(function (result) {
        if (result.status == "success") {
            overwolf.windows.restore(result.window.id);
        }
    });
};function hide() {
    overwolf.windows.getCurrentWindow(function (result) {
        if (result.status == "success") {
            overwolf.windows.hide(result.window.id);
        }
    });
};


const closeWindowAsync = async (windowId) => {
    return new Promise(resolve => overwolf.windows.close(windowId, resolve));
};

const hideWindowAsync = async (windowId) => {
    return new Promise(resolve => overwolf.windows.hide(windowId, resolve));
};

const openWindowAsync = async (windowId) => {
    return new Promise(resolve => overwolf.windows.restore(windowId, resolve));
};

const obtainDeclaredWindowAsync = async (windowName) => {
    return new Promise(resolve => overwolf.windows.obtainDeclaredWindow(windowName, resolve));
};
//END OF WINDOW FUNCTIONS









//START OF AD STUFF
var owAdsReady = false;
var owAdsReady = false;
var isOWAdsCurrentlyStopped = false;
var lastOWAdEventTimestamp = -1;


function addAdEvent(eventType) {
    if (currentAd == undefined) return;

    currentAd.addEventListener(eventType, (extraInfo) => {
        //console.log(eventType, extraInfo);
        lastOWAdEventTimestamp = Date.now();
        if (eventType == "error" && extraInfo == "no ad") {
            isOWAdsCurrentlyStopped = true;
            //console.log("ads currently stopped");
        }
        let isAdVisible = currentAd._adManager._windowVisibility.isVisible && currentAd._adManager._windowVisibility.visibilityTracker.windowVisibilityTracker.isVisible;
        sendMetric("Ad_" + eventType, isAdVisible ? "visible" : "hidden");
        //console.log("Ad_" + eventType, isAdVisible ? "visible" : "hidden");
    });
}

function onOwAdReady() {
    if (!OwAd) {
        console.log("Failed to load ads!");
        return;
    }

    if (shouldAdsBeShown()) {
        console.log("Setting up ad containers");
        startOWADcontainers(document.getElementsByClassName("adAttr400x300"), [{ width: 400, height: 300 }, { width: 300, height: 250 }]);
        startOWADcontainers(document.getElementsByClassName("adAttr400x600"), [{ width: 400, height: 600 }, { width: 400, height: 300 }]);
    } else {
        console.log("Hiding ad/subscription elements because user is subscribed with status: " + getSubscriptionStatus());
        hideAdEelements();
    }
}

function startOWADcontainers(elements, adConfig) {

    if (elements == undefined || elements.length == undefined || elements.length == 0) return;

    for (element of elements) {
        isOWAdsCurrentlyStopped = false;
        lastOWAdEventTimestamp = -1;

        currentAd = new OwAd(element, adConfig);
        currentAd.addEventListener('ow_internal_rendered', () => {
            // It is now safe to call any API you want ( e.g. _owAd.refreshAd() or 
            owAdsReady = true;
        });
        addAdEvent("player_loaded");
        addAdEvent("display_ad_loaded");
        addAdEvent("play");
        addAdEvent("impression");
        addAdEvent("complete");
        addAdEvent("error");
        console.log("Ad container setup complete");
    };
}
//END OF AD STUFF






//START OF SUBSCRIPTION STUFF
const myPlanID = 112;
function isSubscribedInOverwolf(callback) {
    overwolf.profile.subscriptions.getActivePlans(function (info) {
        if (info.success && info.plans != null && info.plans.includes(myPlanID)) {
            callback(true);
        } else {
            callback(false);
        }
    });
}
function subscribeClick() {
    overwolf.profile.subscriptions.inapp.show(myPlanID);
    sendMetric("Patreon_OverwolfOpened","");
}

//This call will return instantly a cached value.
//Possible values: 'None', 'Overwolf', 'PatreonT1', 'PatreonT2', 'PatreonT3'
function getSubscriptionStatus() {
    return localStorage.getItem("subscriptionStatus") ?? 'None';
}

function shouldAdsBeShown() {
    return getSubscriptionStatus() == 'None';
}

//A newStatus of noupdate will not trigger any changes, will just for for overwolf subscription changes
function setSubscriptionStatus(newStatus) {

    let originalStatus = getSubscriptionStatus();
    let wasNoneBefore = getSubscriptionStatus() == 'None';

    if (newStatus == "noupdate") newStatus = originalStatus;

    isSubscribedInOverwolf((isOWSubscribed) => {       
        if (isOWSubscribed && newStatus == 'None') newStatus = 'Overwolf'; //Only overwrite with overwolf if the user status is none (patreon has priority)
        if (!isOWSubscribed && newStatus == 'Overwolf') newStatus = 'None'; //Only overwrite with none if the user status is Overwolf (patreon has priority)

        localStorage.setItem("subscriptionStatus", newStatus);

        if (newStatus != "None" && wasNoneBefore) {
            if (!shouldAdsBeShown()) hideAdEelements();
        }
        if (originalStatus != newStatus) {
            console.log("Subscription status changed from " + originalStatus + " to " + newStatus);
            DoSubscriptionWork();
        }
    });  
}

function hideAdEelements() {
    document.querySelectorAll(".adHideable").forEach((element) => {
        element.style.display = "none";
    });
}

function openSubscriptionWindow() {
    if (typeof overwolf !== 'undefined') {
        overwolf.utils.openUrlInDefaultBrowser("https://patreon.com/AlecaFrame");
        sendMetric("Patreon_WebOpened","");
    }
}

//END OF SUBSCRIPTION STUFF







//START OF MINIMIZE/MAXIMIZE STUFF
var wasLastMaximized = false;
function changeMaximizeStatus(newStatus) {
    if (newStatus) {
        document.documentElement.classList.add('maximized');
        $("#maximizeImage").attr("xlink:href", "assets/img/svg/sprite.svg#window-control_restore");
    } else {
        document.documentElement.classList.remove('maximized');
        $("#maximizeImage").attr("xlink:href", "assets/img/svg/sprite.svg#window-control_maximize");
    }

    wasLastMaximized = newStatus;
}
overwolf.windows.getCurrentWindow((res) => {
    changeMaximizeStatus(res.window.state == "Maximized");
});
function toggleMaximize() {

    overwolf.windows.getCurrentWindow(function (result) {
        if (result.status !== "success") {
            return;
        }

        if (wasLastMaximized) {
            overwolf.windows.restore(result.window.id);
            changeMaximizeStatus(false);

        } else {
            overwolf.windows.maximize(result.window.id);
            changeMaximizeStatus(true);

        }
    });
};
//END OF MINIMIZE/MAXIMIZE STUFF




//START OF MISC STUFF
function reloadStylesheets() {
    var queryString = '?reload=' + new Date().getTime();
    $('link[rel="stylesheet"]').each(function () {
        this.href = this.href.replace(/\?.*|$/, queryString);
    });
}
//END OF MISC STUFF