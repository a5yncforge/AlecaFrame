﻿var relicPlannerFilterType = "all";


var lastRelicPlannerFilterTippy;

function createRelicPlannerFilterTippy() {
    if (lastRelicPlannerFilterTippy != undefined) {
        lastRelicPlannerFilterTippy.destroy();
        lastRelicPlannerFilterTippy = undefined;
    }

    lastRelicPlannerFilterTippy = tippy('#settingFilterExpandingRelicPlanner', {
        content: document.getElementById("tooltipFiltersRelicPlannerTemplate").outerHTML,
        duration: 190,
        arrow: true,
        //  delay: [0, 0],
        offset: [0, 15],
        interactive: true,
        placement: 'top',
        theme: 'extraVaultedInfoTheme',
        animation: 'fade',
        allowHTML: true,
      //   trigger: 'click',
    })[0];
}



lastRelicPlannerComponentTooltips = tippy('#pushToRelicRecommendationOverlay', {
    content: "Push your current filters to the overlay.",
    duration: 190,
    arrow: true,
    //  delay: [0, 0],
    placement: 'top',
    theme: 'extraVaultedInfoTheme',
    animation: 'fade',
    allowHTML: true,
    // trigger: 'click',
});



function UpdatePushedRelicFiltersInPlugin() {
    if (localStorage["relicRecommendationLastFilters"] != undefined && localStorage["relicRecommendationLastFilters"] != "") {
        plugin.get().SetRelicRecommendationFilters(localStorage["relicRecommendationLastFilters"]);
    } else {
        plugin.get().SetRelicRecommendationFilters("");
    }
}

UpdatePushedRelicFiltersInPlugin();


function changedRelicPlannerSetting1(event) {

    var target = event.target;
    while ($(target).hasClass("topSetting") == false) {
        target = target.parentElement;
    }

    $(".topSetting.relicPlanner").removeClass("selected");
    $(target).addClass("selected");

    relicPlannerFilterType = $(target).attr("selectionData");

    //$("#relicPlannerSearch")[0].value = ""; //Clear search
    //changedRelicPlannerSettingSearch(undefined);

    RefreshRelicPlanner();
}

function changedRelicPlannerSettingSearch(event) {

    if (($("#relicPlannerSearch")[0].value == undefined || $("#relicPlannerSearch")[0].value == "") || isLastRelicPlannerFilterSearchEmpty == true) {
        $("#relicPlannerSearch")[0].parentElement.classList.remove("forceOpen");
    } else {
        $("#relicPlannerSearch")[0].parentElement.classList.add("forceOpen");
    }

    //Manual call to refresh the status of the search input field
    if (event == undefined) return;

  //  if (event.key === 'Enter' || event.keyCode === 13) {
        RefreshRelicPlanner();
  //  }
}


function UpdateDeleteFiltersRelicPlannerButtonShown() {
    if (Object.keys(currentRelicPlannerYESNOfilters).length == 0) {
        $("#settingFilterExpandingRelicPlanner")[0].parentElement.classList.remove("forceOpen");
    } else {
        $("#settingFilterExpandingRelicPlanner")[0].parentElement.classList.add("forceOpen");
    }
}

function removeRelicPlannerFilters() {
    currentRelicPlannerYESNOfilters = {};
    createRelicPlannerFilterTippy(); //Recreate the tippy bc for some weird bug the UI doesn't get updated if changed from JS
    UpdateDeleteFiltersRelicPlannerButtonShown();
    RefreshRelicPlanner();
}

function removeRelicPlannerSearch() {
    $("#relicPlannerSearch")[0].value = "";
    changedRelicPlannerSettingSearch(undefined);
    RefreshRelicPlanner();
}

var orderedRelicPlannerLargerToSmaller = true;

function changeRelicPlannerOrderingLargerOrSmallerFirst() {
    if (orderedRelicPlannerLargerToSmaller) {
        $("#relicOrderSetting").addClass("reverseOrder");
    } else {
        $("#relicOrderSetting").removeClass("reverseOrder");
    }
    orderedRelicPlannerLargerToSmaller = !orderedRelicPlannerLargerToSmaller;
    RefreshRelicPlanner();
}


var isLastRelicPlannerFilterSearchEmpty = true;
var lastRelicPlannerFilterSettings = {};


var currentRelicPlannerYESNOfilters = {};

function filterRelicPlannerCheckboxClicked(event, filter, newMode) {

    var shouldRemove = currentRelicPlannerYESNOfilters[filter] == newMode;

    if (shouldRemove) {
        delete currentRelicPlannerYESNOfilters[filter];
        event.target.classList.remove("pulsed");
    } else {
        currentRelicPlannerYESNOfilters[filter] = newMode;
        $(event.target.parentElement.children).removeClass("pulsed");
        event.target.classList.add("pulsed");
    }

    UpdateDeleteFiltersRelicPlannerButtonShown();
    RefreshRelicPlanner();
}


function filterRelicPlannerWrapperClicked(event) {
    if (event.target.firstChild != undefined) {
        event.target.firstChild.click()
    }
}


function RefreshRelicPlanner(forceUpdate=false,shouldShowAll=false) {
    var fullInventoryExperienceEnabled = settingsApp.fullInventoryExperienceEnabled;

    var filterSettings = {
        type: relicPlannerFilterType,
        search: $("#relicPlannerSearch")[0].value,
        order: $("#relicPlannerOrdering")[0].value,
        orderLargerToSmaller: orderedRelicPlannerLargerToSmaller,
        yesnoFilters: JSON.stringify(currentRelicPlannerYESNOfilters),
        squadSize: $("#relicPlannerSquadSize")[0].value
    };

    lastRelicPlannerFilterSettings = filterSettings;

    var newFilter = filterSettings.type + "|%-%|" + filterSettings.search + "|%-%|" + filterSettings.order + "|%-%|" + filterSettings.orderLargerToSmaller + "|%-%|" + JSON.stringify(filterSettings.yesnoFilters) + "|%-%|"+filterSettings.squadSize;

    isLastRelicPlannerFilterSearchEmpty = filterSettings.search == "";
    changedRelicPlannerSettingSearch(undefined);

    relicPlannerApp.loading = true;
   
    relicPlannerApp.updatedRecommended = false;
    relicPlannerApp.updateNeeded = false;
    relicPlannerApp.noItems = false;
    relicPlannerApp.inProgress = false;
    relicPlannerApp.progress = 0;
    relicPlannerApp.loadingProgressText = "Loading relics...";

    plugin.get().getFilteredRelicPlanner(JSON.stringify(filterSettings), forceUpdate,relicPlannerApp.onlyOwned, fullInventoryExperienceEnabled || shouldShowAll,true, (res, data, ducatCount) => {  });
}

plugin.get().onRelicPlannerUpdate.addListener((result, data,updatedRecommended) => {
  
    //setTimeout(() => {
        relicPlannerApp.loading = false;
   // }, 250);      

    relicPlannerApp.updatedRecommended = false;
    relicPlannerApp.updateNeeded = false;
    relicPlannerApp.inProgress = false;
    relicPlannerApp.progress = 0;
    
    if (result == "data") {
        relicPlannerApp.updateNeeded = updatedRecommended;
        relicPlannerApp.showData(JSON.parse(data));
    } else if (result == "updateNeeded") {
        relicPlannerApp.updateNeeded = true;
    } else if (result == "noItems") {
        relicPlannerApp.noItems = true;
    } else if (result == "traces") {
        relicPlannerApp.summaryTraces = data;
    } else if (result == "progress") {
        relicPlannerApp.inProgress = true;
        relicPlannerApp.setProgress(data);
        relicPlannerApp.loadingProgressText = updatedRecommended;
    }
    else {
        console.log("Relic Planner Error: " + result + "|"  + data);
    }
});


var lastRelicPlannerComponentTooltips = [];
var lastRelicPlannerExpectedTooltips = [];
var lastRelicPlannerVaultedTooltips = [];
var lastRelicPlannerWFMTooltips = [];

relicPlannerApp = Vue.createApp({
    data() {
        return {
            loading: false,
            updatedRecommended: false,
            updateNeeded: false,
            noItems: true,
            inProgress: false,
            progress: 0,
            loadingProgressText: "Loading relics...",
            items: [],
            summaryTraces: 0,
            onlyOwned: true,
        }
    },
    mounted: function () {
        createRelicPlannerFilterTippy();

        $("#pushToRelicRecommendationOverlay").click(() => {
            localStorage["relicRecommendationLastFilters"] = JSON.stringify(lastRelicPlannerFilterSettings);
            UpdatePushedRelicFiltersInPlugin();
            sendMetric("RelicPlanner_PushSettingsToOverlay", "");
            showSuccesfulToast("The current filters will now be used in the overlay");
        });
    },
    methods: {      
        setProgress(data) {
            this.progress = data;
        },       
        refresh() {            
            RefreshRelicPlanner();
        },
        showData(data) {
            this.noItems = data.length == 0;            
            this.items = data;
            setTimeout(() => {
                if (lastRelicPlannerComponentTooltips != undefined && lastRelicPlannerComponentTooltips.length > 0) {
                    for (var i = 0; i < lastRelicPlannerComponentTooltips.length; i++) {
                        lastRelicPlannerComponentTooltips[i].destroy();
                    }
                    lastRelicPlannerComponentTooltips = [];
                }

                if (lastRelicPlannerExpectedTooltips != undefined && lastRelicPlannerExpectedTooltips.length > 0) {
                    for (var i = 0; i < lastRelicPlannerExpectedTooltips.length; i++) {
                        lastRelicPlannerExpectedTooltips[i].destroy();
                    }
                    lastRelicPlannerExpectedTooltips = [];
                }                

                if (lastRelicPlannerVaultedTooltips != undefined && lastRelicPlannerVaultedTooltips.length > 0) {
                    for (var i = 0; i < lastRelicPlannerVaultedTooltips.length; i++) {
                        lastRelicPlannerVaultedTooltips[i].destroy();
                    }
                    lastRelicPlannerVaultedTooltips = [];
                }       

                if (lastRelicPlannerWFMTooltips != undefined && lastRelicPlannerWFMTooltips.length > 0) {
                    for (var i = 0; i < lastRelicPlannerWFMTooltips.length; i++) {
                        lastRelicPlannerWFMTooltips[i].destroy();
                    }
                    lastRelicPlannerWFMTooltips = [];
                }                


                lastRelicPlannerComponentTooltips = tippy('.relicPlannerRewardContainer>.foundryObjectComponentsComponent', {
                    duration: 190,
                    arrow: true,
                    //  delay: [0, 0],
                    placement: 'top',
                    theme: 'extraVaultedInfoTheme',
                    animation: 'fade',
                    allowHTML: true,
                    // trigger: 'click',
                });

                lastRelicPlannerExpectedTooltips = tippy('.relicDetailsBLTopExpected.relicPlanner', {
                    content: "Expected profits for the selected squad size.",
                    duration: 190,
                    arrow: true,
                    //  delay: [0, 0],
                    placement: 'top',
                    theme: 'extraVaultedInfoTheme',
                    animation: 'fade',
                    allowHTML: true,
                    // trigger: 'click',
                });

                lastRelicPlannerVaultedTooltips = tippy('.relicPlannerMiniIcons>.foundryObjectTopIcon.vaulted', {
                    content: "Vault status",
                    duration: 190,
                    arrow: true,
                    //  delay: [0, 0],
                    placement: 'top',
                    theme: 'extraVaultedInfoTheme',
                    animation: 'fade',
                    allowHTML: true,
                    // trigger: 'click',
                });            

                lastRelicPlannerWFMTooltips = tippy('.relicPlannerMiniIcons>.inventoryItemOrderPlaced', {
                    content: "WFMarket order placed",
                    duration: 190,
                    arrow: true,
                    //  delay: [0, 0],
                    placement: 'top',
                    theme: 'extraVaultedInfoTheme',
                    animation: 'fade',
                    allowHTML: true,
                    // trigger: 'click',
                });       
                
            }, 250);
        },
        openRelic(uID) {
            relicDetailsApp.open(uID);
        }
    }

}).mount("#tabRelicPlanner");