
//Load plugin
var plugin = overwolf.windows.getMainWindow().plugin;
var mainWindow = overwolf.windows.getMainWindow();

var gameModeList = [
    { name: "All", internalName: "INTERNAL_ALL", image: "assets/img/infinity.png", internal: true },
    {
        name: "Relics", internalName: "RelicOpeningBasic", image: "https://cdn.alecaframe.com/warframeData/img/axi-radiant.png",
        requirementsText: "Relics allowed: ",
        requirementsTip: "There are many filters filters available. You can try the name of a Warframe, weapon, a part, 'Any Lith', 'Radiant',..."
    },
    {
        name: "VRC Relics", internalName: "RelicOpeningAdvanced", image: "https://cdn.alecaframe.com/warframeData/img/axi-radiant.png",
        requirementsText: "Relics allowed: ",
        requirementsTip: "There are many filters filters available. You can try the name of a Warframe, weapon, a part, 'Any Lith', 'Radiant',..."
    },
    {
        name: "Farming", internalName: "Farming", image: "assets/img/custom.png",
        requirementsText: "Items/relics to farm: ",
        requirementsTip: "There are many filters filters available. You can try the name of any resource, including minerals, endo, ..."
    },
    {
        name: "Grand bosses", internalName: "GrandBosses", image: "https://cdn.alecaframe.com/warframeData/img/teralyst-sigil.png",
        requirementsText: "Requirements: ",
        requirementsTip: "You can select which boss you want to kill and (optionally) if any special Warframe is required"
    },
    {
        name: "Arbitration", internalName: "Arbitration", image: "https://cdn.alecaframe.com/warframeData/img/vitus-emblem.png",
        requirementsText: "Requirements (optional):",
        requirementsTip: "You can also specify a minimum length and/or any required Warframes"
    },
    {
        name: "Steel path", internalName: "HardMode", image: "assets/img/SteelEssence.webp",
        requirementsText: "Requirements (optional): ",
        requirementsTip: "You can also specify a minimum length and/or any required Warframes"
    },
    {
        name: "Duviri", internalName: "Duviri", image: "https://cdn.alecaframe.com/warframeData/img/display---tennocon-2022-duviri.png",
        requirementsText: "Squad type: ",
        requirementsTip: "You can also specify a minimum length (specially useful for The Circuit) and/or any required Warframes"
    },
    { name: "Lich", internalName: "Lich", image: "https://cdn.alecaframe.com/warframeData/img/lich-token.png", requirementsText: "Lich type: " },  
    { name: "Bounties", internalName: "BountyFarming", image: "assets/img/coins.png", requirementsText: "Which bounties do you want to farm: " },
    {
        name: "Sanctuary", internalName: "Sanctuary", image: "https://cdn.alecaframe.com/warframeData/img/cephalon-simaris-sigil.png",
        requirementsText: "Sanctuary type: ",
        requirementsTip: "You can (optionally) specify any required Warframes for the mission"
    },
    { name: "Daily sorties", internalName: "Sortie", image: "assets/img/Sortie_b.webp" },
    { name: "Archon", internalName: "Archon", image: "assets/img/ArchonShard.webp" },
    { name: "PvP", internalName: "PvP", image: "https://cdn.alecaframe.com/warframeData/img/conclave-sigil.png" },
    { name: "Custom", internalName: "Custom", image: "assets/img/miscGame.png" }
];


/*
overwolf.windows.getCurrentWindow((windowData) => {

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
});*/

overwolf.profile.subscriptions.onSubscriptionChanged.addListener((data) => {
    DoSubscriptionWork();
});

setTimeout(DoSubscriptionWork, 1000);
setTimeout(DoSubscriptionWork, 2500); //Just in case

//Stack that handles the press of the escape key. When a modal is opened, it registers a function to be called here
escapeKeyHandlersStack = [];
$(document).keydown(function (e) {
    if (e.key === "Escape" && escapeKeyHandlersStack.length > 0) { // escape key maps to keycode `27`
        var todo = escapeKeyHandlersStack.pop();
        todo();
    }
});



setInterval(() => {
    if (escapeKeyHandlersStack.length > 10) {
        escapeKeyHandlersStack = escapeKeyHandlersStack.slice(10);
    }

    //Show the relic help modal if the flag is set and it hasn't been shown in a while
    if (localStorage["relicErrorHappenedRecently"] === "true" && (localStorage["lastRelicHelpShowedTime"] === undefined || (new Date() - new Date(parseInt(localStorage["lastRelicHelpShowedTime"]))) > 3 * 24 * 60 * 60 * 1000)) {
        localStorage["lastRelicHelpShowedTime"] = new Date().getTime();
        localStorage["relicErrorHappenedRecently"] = "false";
        scaleErrorApp.open();
    }
}, 60000);

function DoSubscriptionWork() {
    isSubscribed((status) => {
        if (status) {
            try {
                if (currentAd != undefined) currentAd.shutdown();
            } catch (e) {
                //Kind of expected at launch, since the ads load later
            }

        }
        UpdateUIBasedOnSubscription(status);
    })
}


function UpdateUIBasedOnSubscription(status) {
    if (status) {
        // $(".subscribeButton").text("Thank you for your support!");
        $(".subscribeButton").text("");
        $(".subscribeButton").addClass("done")
        $("#mainAD")[0].style.display = 'none'; //Remove ad
    } else {
        $("#mainAD")[0].style.display = 'block'; //Add ad
        //This should never be needed
    }
}


window.onload = function () {
    DoStartupThingsWhenLoaded();
};

function DoStartupThingsWhenLoaded() {
   
}




var owAdsReady = false;
function addAdEvent(eventType) {
    if (currentAd == undefined) return;

    currentAd.addEventListener(eventType, (extraInfo) => {
        let isAdVisible = currentAd._adManager._windowVisibility.isVisible && currentAd._adManager._windowVisibility.visibilityTracker.windowVisibilityTracker.isVisible;
        sendMetric("Ad_" + eventType, isAdVisible ? "visible" : "hidden");
    });
}

function onOwAdReady() {
    if (!OwAd) {
        console.log("Failed to load ads!");
        return;
    }

    isSubscribed((status) => {
        console.log("Subscription status: " + status);
        if (!status) {
            currentAd = new OwAd(document.getElementById("mainAD"), [{ width: 400, height: 300 }, { width: 300, height: 250 }]);
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

            console.log("Main ad setup complete");
        } else {
            console.log("Main ad setup complete (disabled bc of subscription)");

            if (window.location.href.includes("SquadMakingSquad.html")) {
                $(".staticRightColumn").hide(); //Remove ad completelly for larger usable space
            }else if(window.location.href.includes("SquadMakingMain.html")){
                $(".subscribeButton.GoPremiumCallout").hide(); //Remove ad completelly for larger usable space
            }
        }
    });
}


function subscribeClick() {
    overwolf.profile.subscriptions.inapp.show(myPlanID);
}



setTimeout(() => {
    sendPage("squadmaking");
}, 2500);


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


var lastTimeWarframeJustUpdatedTimestamp = 0;
function WarframeDataJustUpdated() {

    if (lastTimeWarframeJustUpdatedTimestamp > Date.now() - 3000) return;
    lastTimeWarframeJustUpdatedTimestamp = Date.now();

    //TODO: Clear all the "can be joined flags" and recalculate them
}

plugin.get().onWarframeDataChanged.removeListener(WarframeDataJustUpdated);
plugin.get().onWarframeDataChanged.addListener(WarframeDataJustUpdated);

