

var orderedLargerToSmaller = true;

var lastInventoryFilterTippy;

function createInventoryFilterTippy() {
    if (lastInventoryFilterTippy != undefined) {
        lastInventoryFilterTippy.destroy();
        lastInventoryFilterTippy = undefined;
    }

    lastInventoryFilterTippy = tippy('#settingFilterExpandingInventory', {
        content: document.getElementById("tooltipFiltersInventoryTemplate").outerHTML,
        duration: 190,
        arrow: true,
        //  delay: [0, 0],
        offset: [0, 15],
        interactive: true,
        placement: 'top',
        theme: 'extraVaultedInfoTheme',
        animation: 'fade',
        allowHTML: true,
        //  trigger: 'click',
    })[0];
}






function changedInventorySetting1(event) {

    var target = event.target;
    while ($(target).hasClass("topSetting") == false) {
        target = target.parentElement;
    }

    $(".inventorySetting").removeClass("selected");
    $(target).addClass("selected");

    inventoryApp.invFilterType = $(target).attr("selectionData");

    //$("#inventorySearch")[0].value = ""; //Clear search
    //changedInventorySettingSearch(undefined);

    RefreshInventory();
}

function changedInventorySettingSearch(event) {

    if (($("#inventorySearch")[0].value == undefined || $("#inventorySearch")[0].value == "") || isLastInventoryFilterSearchEmpty == true) {
        $("#inventorySearch")[0].parentElement.classList.remove("forceOpen");
    } else {
        $("#inventorySearch")[0].parentElement.classList.add("forceOpen");
    }

    //Manual call to refresh the status of the search input field
    if (event == undefined) return;

    // if (event.key === 'Enter' || event.keyCode === 13) {
    RefreshInventory();
    //  }
}


function UpdateDeleteFiltersInventoryButtonShown() {
    if (Object.keys(currentInventoryYESNOfilters).length == 0) {
        $("#settingFilterExpandingInventory")[0].parentElement.classList.remove("forceOpen");
    } else {
        $("#settingFilterExpandingInventory")[0].parentElement.classList.add("forceOpen");
    }
}

function removeInventoryFilters() {
    currentInventoryYESNOfilters = {};
    createInventoryFilterTippy(); //Recreate the tippy bc for some weird bug the UI doesn't get updated if changed from JS
    UpdateDeleteFiltersInventoryButtonShown();
    RefreshInventory();
}

function removeSearch() {
    $("#inventorySearch")[0].value = "";
    changedInventorySettingSearch(undefined);
    RefreshInventory();
}

function changeOrderingLargerOrSmallerFirst() {
    if (orderedLargerToSmaller) {
        $(".topSettingIcon.ordering").addClass("reverseOrder");
    } else {
        $(".topSettingIcon.ordering").removeClass("reverseOrder");
    }
    orderedLargerToSmaller = !orderedLargerToSmaller;
    RefreshInventory();
}


var isLastInventoryFilterSearchEmpty = true;



var currentInventoryYESNOfilters = {};

function filterCheckboxClicked(event, filter, newMode) {

    var shouldRemove = currentInventoryYESNOfilters[filter] == newMode;

    if (shouldRemove) {
        delete currentInventoryYESNOfilters[filter];
        event.target.classList.remove("pulsed");
    } else {
        currentInventoryYESNOfilters[filter] = newMode;
        $(event.target.parentElement.children).removeClass("pulsed");
        event.target.classList.add("pulsed");
    }

    UpdateDeleteFiltersInventoryButtonShown();
    RefreshInventory();
}


function filterWrapperClicked(event) {
    if (event.target.firstChild != undefined) {
        event.target.firstChild.click()
    }
}


var lastSetWarningTippies = [];
var lastInventorySetComponentTooltips = [];
var invModLevelUpTooltips = [];
var invOrderPlacedTooltips = [];
var invItemVaultedTooltips = [];
var invTargetOwnedTooltips = [];

var randomNumberForInventoryPrices = 0;

tippy.delegate('#tabInventory', {
    target: '.inventoryItemOrderPlaced',
    content: "WFMarket order placed",
    duration: 190,
    arrow: true,
    placement: 'top',
    theme: 'extraVaultedInfoTheme',
    animation: 'fade'
});
tippy.delegate('#tabInventory', {
    target: '.foundryVaultedHolder',
    content: "Vault status",
    duration: 190,
    arrow: true,
    placement: 'top',
    theme: 'extraVaultedInfoTheme',
    animation: 'fade'
});
tippy.delegate('#tabInventory', {
    target: '.inventoryTargetOwned',
    content: "Item owned/mastered",
    duration: 190,
    arrow: true,
    placement: 'top',
    theme: 'extraVaultedInfoTheme',
    animation: 'fade'
});
tippy.delegate('#tabInventory', {
    target: '.inventoryItemFav',
    content: "Marked as favorite",
    duration: 190,
    arrow: true,
    placement: 'top',
    theme: 'extraVaultedInfoTheme',
    animation: 'fade'
});
tippy.delegate('#tabInventory', {
    target: '.modLeveledUp',
    content: "Mod leveled up",
    duration: 190,
    arrow: true,
    placement: 'top',
    theme: 'extraVaultedInfoTheme',
    animation: 'fade'
});


function RefreshInventory(showAll = false) {
    inventoryApp.refresh(showAll);
}

function default2Zero(input) {
    return input == undefined ? 0 : input;
}

function onBuySellItemClicked(event, isWTB = false) {
    event.preventDefault();
    var target = event.target;
    while ($(target).hasClass("inventoryObject") == false) {
        target = target.parentElement;
    }

    buySellPanelApp.currentMode = isWTB ? "buy" : "sell";

    itemName = target.getElementsByClassName("inventoryItemName")[0].innerText;

    itemRank = target.getElementsByClassName("inventoryItemName")[0].getAttribute("rank");
    if (itemRank == undefined)
        itemRank = 0;
    else
        itemRank = parseInt(itemRank);

    showBuySellPanelWithItem(itemName, itemRank);
}

function onBuySellItemClickedAttr(event) {
    event.preventDefault();
    var target = event.currentTarget;

    showBuySellPanelWithItem(target.getAttribute("data-item-name"));
}

function showBuySellPanelWithItem(itemName, itemRank = 0, realName = "") {
    LoadBuySellWindow(itemName, itemRank, realName);
}



function LoadBuySellWindow(itemName, itemRank = 0, realItemName = "") {

    document.getElementById("tabWFMarketBuySell").click();
    buySellPanelApp.doingSearch = false;

    //TODO: Show loading animation here
    buySellPanelApp.loading = true;
    buySellPanelApp.nothingSelected = false;

    plugin.get().GetBuySellWindowData(itemName, (success, dataToShowJSON, subtypeList) => {
        if (success) {
            dataToShow = JSON.parse(dataToShowJSON);

            //buySellPanelApp.currentMode = 'sell';

            buySellPanelApp.sellListings = dataToShow.sellListings;
            buySellPanelApp.buyListings = dataToShow.buyListings;
            buySellPanelApp.postingSettings = dataToShow.postingSettings;
            buySellPanelApp.suggestedSubtypes = JSON.parse(subtypeList);
            if (realItemName != "") buySellPanelApp.postingSettings.readableName = realItemName;
            buySellPanelApp.dataJustUpdated()

            if (buySellPanelApp.postingSettings.isMod) {
                buySellPanelApp.mod_rank = itemRank;
            } else {
                buySellPanelApp.mod_rank = "";
            }

            if (buySellPanelApp.postingSettings.isAyatan) {
                buySellPanelApp.cyan_stars = 0;
                buySellPanelApp.amber_stars = 0;
            } else {
                buySellPanelApp.cyan_stars = "";
                buySellPanelApp.amber_stars = "";
            }

        } else {
            buySellPanelApp.sellListings = [];
            buySellPanelApp.buyListings = [];
            buySellPanelApp.error = true;
            buySellPanelApp.errorMessage = "Listing not found";
            setTimeout(() => { buySellPanelApp.error = false }, 2000);
        }

        buySellPanelApp.loading = false;
    });
}



inventoryApp = Vue.createApp({
    data() {
        return {
            items: [],
            loading: false,
            summaryPlatinum: "- -",
            summaryDucats: "- -",
            noItems: true,
            useDucanator: false,
            onlyOwned: true,
            invFilterType: "allParts"
        }
    },
    methods: {
        refresh(showAll = false) {
            var fullInventoryExperienceEnabled = settingsApp.fullInventoryExperienceEnabled;

            let filterSettings = {
                type: this.invFilterType,
                search: $("#inventorySearch")[0].value,
                order: $("#inventoryOrdering")[0].value,
                orderLargerToSmaller: orderedLargerToSmaller,
                yesnoFilters: JSON.stringify(currentInventoryYESNOfilters),
                onlyOwned: this.onlyOwned
            };

            let newFilter = filterSettings.type + "|%-%|" + filterSettings.search + "|%-%|" + filterSettings.order + "|%-%|" + filterSettings.orderLargerToSmaller + "|%-%|" + JSON.stringify(filterSettings.yesnoFilters);

            isLastInventoryFilterSearchEmpty = filterSettings.search == "";
            changedInventorySettingSearch(undefined);

            this.loading = true;
            this.summaryPlatinum = "- -";

            plugin.get().getFilteredInventory(JSON.stringify(filterSettings), fullInventoryExperienceEnabled || showAll, (res, data, ducatCount, platCount) => {
                if (res) {
                    //setTimeout(() => {
                    this.loading = false;
                    // }, 250);

                    this.noItems = false;
                    this.items = JSON.parse(data);
                    // console.log(inventoryApp.items);
                    if (this.items == undefined || this.items.length == 0) this.noItems = true;
                    this.summaryDucats = ducatCount;
                    this.summaryPlatinum = platCount;

                    this.useDucanator = filterSettings.order == "ducanator";

                    setTimeout(() => {

                        if (lastInventorySetComponentTooltips != undefined && lastInventorySetComponentTooltips.length > 0) {
                            for (var i = 0; i < lastInventorySetComponentTooltips.length; i++) {
                                lastInventorySetComponentTooltips[i].destroy();
                            }
                            lastInventorySetComponentTooltips = [];
                        }

                        lastInventorySetComponentTooltips = tippy('.inventoryComponent', {
                            duration: 190,
                            arrow: true,
                            //  delay: [0, 0],
                            placement: 'top',
                            theme: 'extraVaultedInfoTheme',
                            animation: 'fade',
                            allowHTML: true,
                            // trigger: 'click',
                        });

                        if (lastInventorySetComponentTooltips != undefined && lastInventorySetComponentTooltips.length > 0) {
                            for (let i = 0; i < lastInventorySetComponentTooltips.length; i++) {
                                lastInventorySetComponentTooltips[i].destroy();
                            }
                            lastInventorySetComponentTooltips = [];
                        }

                        lastInventorySetComponentTooltips = tippy('.inventoryComponent', {
                            duration: 190,
                            arrow: true,
                            //  delay: [0, 0],
                            placement: 'top',
                            theme: 'extraVaultedInfoTheme',
                            animation: 'fade',
                            allowHTML: true,
                            // trigger: 'click',
                        });

                        if (lastSetWarningTippies != undefined && lastSetWarningTippies.length > 0) {
                            for (let i = 0; i < lastSetWarningTippies.length; i++) {
                                lastSetWarningTippies[i].destroy();
                            }
                            lastSetWarningTippies = [];
                        }

                        lastSetWarningTippies = tippy('.inventoryItemButtonPostSellSmall.notEnough', {
                            content: "Be careful, you don't own the complete set!",
                            duration: 190,
                            arrow: true,
                            //  delay: [0, 0],
                            placement: 'top',
                            theme: 'extraVaultedInfoTheme',
                            animation: 'fade',
                            allowHTML: true,
                            // trigger: 'click',
                        });

                    }, 100);





                } else {
                    this.items = [];

                    this.summaryDucats = "- -";
                    this.summaryPlatinum = "- -";
                    this.noItems = true;

                    setTimeout(() => { this.loading = false; }, 450);
                }
            });
        },
        itemClicked(itemType, uniqueID, item) {
            if (itemType == "relic") {
                relicDetailsApp.open(uniqueID);
            } else if (itemType == "mod" || itemType == 'arcane') {
                if (item.modType != 'riven' && item.modType != 'requiem') {
                    foundryDetailsApp.open(uniqueID, item == undefined ? undefined : item.currentModRank);
                }
            }
        },
        onBuySellItemClickedVUE(eventt, isWTB = false) {
            onBuySellItemClicked(eventt, isWTB);
        },
        dataJustUpdated() {
            /*  this.nothingSelected = false;
              this.isSpecial = this.postingSettings.isRelic || this.postingSettings.isMod;
              this.mod_rank = 0;
              this.subtype = "";
              this.pricePosting = this.sellListings.length > 0 ? Math.max(1, this.sellListings[0].platimun - 1) : "";*/
        },
        componentClicked(component) {
            showBuySellPanelWithItem(component.name);
        }
    },
    mounted: function () {
        createInventoryFilterTippy();
    }
}).mount("#tabInventory");


buySellPanelApp = Vue.createApp({
    data() {
        return {
            count: 0,
            sellListings: [],
            buyListings: [],
            postingSettings: {
                isRelic: false,
                isMod: false,
                isFish: false,
                isVeiledRiven: false,
                isFish2: false,
                wfmItemName: "",
                readableName: ""
            },
            unitsPosting: 1,
            pricePosting: 10,
            isSpecial: false,

            mod_rank: 0, //Mod ranking
            subtype: "intact", //Type of relic

            cyan_stars: 0,
            amber_stars: 0,

            loading: false,
            nothingSelected: true,
            tick: false,
            error: false,
            errorMessage: "UNKNOWN",

            currentMode: 'sell',


            wfmSearchCurrentGuess: "",
            searchSuggestions: [],
            doingSearch: false
        }
    },
    methods: {
        updateSearchSuggestions() {
            plugin.get().GetWFMarketSearchSuggestions(this.wfmSearchCurrentGuess, (success, response) => {
                if (success) {
                    this.searchSuggestions = JSON.parse(response);
                } else {
                    console.log("Error getting WFM search suggestions: " + response);
                    showErrorToast("Failed to get WFMarket search suggestions: " + response);
                }
            });
        },
        showSearch() {
            this.doingSearch = true;
            escapeKeyHandlersStack.push(() => { this.doingSearch = false });
        },
        suggestionClicked(itemName, itemUrlName) {
            sendMetric("WFM_SearchSuggestion_Clicked", "");
            LoadBuySellWindow(itemUrlName, 0, itemName);
        },
        dataJustUpdated() {
            this.nothingSelected = false;
            this.isSpecial = this.postingSettings.isRelic || this.postingSettings.isMod || this.postingSettings.isFish || this.postingSettings.isFish2 || this.postingSettings.isVeiledRiven || this.postingSettings.isAyatan;
            //this.mod_rank = 0;

            this.subtype = "intact";
            if (this.postingSettings.isVeiledRiven) this.subtype = "revealed";

            if (this.suggestedSubtypes.length > 0) this.subtype = "";

            this.pricePosting = this.sellListings.length > 0 ? Math.max(1, this.sellListings[0].platimun) : "";

            this.postingSettings.readableName = this.postingSettings.readableName.replace(" Intact", "").replace(" Exceptional", "").replace(" Flawless", "").replace(" Radiant", "");

            if (this.sellListings != undefined && this.buyListings != undefined && this.sellListings.length > 0 && this.buyListings.length > 0) {
                var lowestSellPrice = this.sellListings[0].platimun;
                var highestBuyPrice = this.buyListings[0].platimun;

                for (var i = 0; i < this.buyListings.length; i++) {
                    if (this.buyListings[i].platimun >= lowestSellPrice) {
                        this.buyListings[i].goodPrice = true;
                    }
                }
            }
        },
        ExecutePost(type) {
            if (!$(".wfmLoginOverlay").hasClass("hidden")) { //Show login if not logged in
                $("#mainTabWFM").click();
                return;
            }

            this.loading = true;

            if (this.pricePosting == "" || this.unitsPosting == "" || (this.suggestedSubtypes.length > 0 && (this.subtype == "" || this.subtype == undefined))) {
                this.loading = false;
                this.error = true;
                this.errorMessage = "Missing information";
                setTimeout(() => { this.error = false }, 2000);
                return;
            }




            if (mainWindow.devMode) {
                this.loading = false;
                this.error = true;
                this.errorMessage = "DEMO mode enabled";
                setTimeout(() => { this.error = false }, 2500);
                showErrorToast("DEMO mode is enabled. Please disable it in the settings tab to access the market");
                return;
            }

            if (!this.postingSettings.isRelic && !this.postingSettings.isFish && !this.postingSettings.isFish2 && !this.postingSettings.isVeiledRiven && !(this.suggestedSubtypes?.length > 0)) this.subtype = "";
            //if (!this.postingSettings.isMod) this.mod_rank = "";

            plugin.get().DoWFMarketPostListing(type, this.postingSettings.wfmItemName, this.pricePosting, this.unitsPosting, this.subtype.toLowerCase(), this.mod_rank.toString(), this.cyan_stars.toString(), this.amber_stars.toString(), (success, error) => {
                this.loading = false;
                if (success) {
                    this.tick = true;
                    this.nothingSelected = true;
                    this.unitsPosting = 1;
                    setTimeout(() => {
                        this.tick = false;

                        RefreshInventory();
                    }, 1750); //After this time the WFM orders probably have also updated,
                    //so the next inventory response will contain the correct order markers
                } else {
                    this.error = true;
                    this.errorMessage = error;
                    setTimeout(() => { this.error = false }, 2000);
                }
            })
            sendMetric("WFM_CreateListing", type);
        },
        messageClicked(listing, isSell) {
            if (listing.cliked == undefined || listing.clicked == false) {
                listing.clicked = true;

                var extraMessageData = "";
                var preExtraMessageData = "";

                if (this.postingSettings.isRelic) {

                    var relicTypeName = "";
                    switch (listing.specialValue) {
                        case "I":
                            relicTypeName = "Intact";
                            break;
                        case "E":
                            relicTypeName = "Exceptional";
                            break;
                        case "F":
                            relicTypeName = "Flawless";
                            break;
                        case "R":
                            relicTypeName = "Radiant";
                            break;
                        default:
                            relicTypeName = listing.specialValue;
                    }

                    extraMessageData = "(" + relicTypeName + ")";
                }

                if (this.postingSettings.isAyatan) {
                    extraMessageData = "(" + listing.specialValue + ")";
                }

                if (this.postingSettings.isVeiledRiven) {
                    preExtraMessageData = listing.specialValue;
                }

                if (this.postingSettings.isMod) extraMessageData = "(Rank " + listing.specialValue + ")";
                var messageToSend = "/w " + listing.playerName + " Hi! I want to " + (isSell ? "buy" : "sell") + ": \"" + preExtraMessageData + (preExtraMessageData == "" ? "" : " ") + this.postingSettings.readableName + (extraMessageData == "" ? "" : " ") + extraMessageData + "\" for " + listing.platimun + " platinum. (warframe.market through AlecaFrame)";
                overwolf.utils.placeOnClipboard(messageToSend);
                if (listing.clicked) setTimeout(() => { listing.clicked = false; }, 1500);

                sendMetric("WFM_CopyMessage", (isSell ? "buy" : "sell"));
            }

        }
    }

}).mount('#inventoryWFMtabBUYSELL')


function UpdateTopWidgetWFMStatus() {
    plugin.get().GetWFMarketStatus((isLogedIn, connectingToWS, wfmStatus, unreadMessages) => {
        wfStatusApp.visible = isLogedIn;
        wfStatusApp.selectedStatus = wfmStatus;
        wfStatusApp.connectingToWS = connectingToWS;
        wfStatusApp.unreadMessages = unreadMessages;
    });
}

plugin.get().onWFMarketStatusChanged.addListener((nothing) => {
    UpdateTopWidgetWFMStatus();
});



plugin.get().onWFMarketLoginStatusChanged.addListener((nothing) => {
    UpdateTopWidgetWFMStatus();
});

setTimeout(UpdateTopWidgetWFMStatus, 1000);



wfStatusApp = Vue.createApp({
    data() {
        return {
            visible: false,
            selectedStatus: "invisible",
            autoMode: false,
            unreadMessages: 0,
            connectingToWS: false
        }
    },
    methods: {
        statusChanged(showModalKeep = true) {
            plugin.get().SetWFMMarketStatus(this.selectedStatus, this.autoMode, (a) => { });
            if (showModalKeep && this.autoMode === true) AutoKeepModal.open(); //Only show this when enabling it
            localStorage["autoModeLastStatus"] = this.autoMode.toString();
        },
        statusChangedAuto() {
            this.statusChanged();
            sendMetric("WFM_ChangeStatusAuto", this.autoMode);
        },
        statusChangedManual() {
            this.statusChanged();
            sendMetric("WFM_ChangeStatusManual", this.selectedStatus);
        },
        doRegister() {
            overwolf.utils.openUrlInDefaultBrowser("https://warframe.market/")
            sendMetric("WFM_OpenRegister", "");
        }
    }

}).mount(".wfInfoOnlineStatus");


tippy('.inventorySummary', {
    content: "Total of current selection",
    duration: 75,
    arrow: true,
    //  delay: [0, 0],
    placement: 'top',
    theme: 'extraVaultedInfoTheme',
    animation: 'fade',
    allowHTML: true,
    // trigger: 'click',
});