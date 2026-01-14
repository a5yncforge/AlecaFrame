var lastWFMOrderFilterTippy;

function createWFMordersFilterTippy() {
    if (lastWFMOrderFilterTippy != undefined) {
        lastWFMOrderFilterTippy.destroy();
        lastWFMOrderFilterTippy = undefined;
    }

    lastWFMOrderFilterTippy = tippy('#settingFilterExpandingWFMOrders', {
        content: document.getElementById("tooltipFiltersWFMordersTemplate").outerHTML,
        duration: 190,
        arrow: true,
        //  delay: [0, 0],
        offset: [0, 15],
        interactive: true,
        placement: 'bottom',
        theme: 'extraVaultedInfoTheme',
        animation: 'fade',
        allowHTML: true,
        //   trigger: 'click',
    })[0];
}

var lastWFMContractFilterTippy;
function createWFMcontractsFilterTippy() {
    if (lastWFMContractFilterTippy != undefined) {
        lastWFMContractFilterTippy.destroy();
        lastWFMContractFilterTippy = undefined;
    }

    lastWFMContractFilterTippy = tippy('#settingFilterExpandingWFMContracts', {
        content: document.getElementById("tooltipFiltersWFMcontractsTemplate").outerHTML,
        duration: 190,
        arrow: true,
        //  delay: [0, 0],
        offset: [0, 15],
        interactive: true,
        placement: 'bottom',
        theme: 'extraVaultedInfoTheme',
        animation: 'fade',
        allowHTML: true,
        //   trigger: 'click',
    })[0];
}

function createWFMordersActionsTippy() {
    tippy('#settingFilterExpandingWFMOrdersActions', {
        content: document.getElementById("tooltipFiltersWFMordersActionsTemplate").outerHTML,
        duration: 190,
        arrow: true,
        //  delay: [0, 0],
        offset: [0, 15],
        interactive: true,
        placement: 'bottom',
        theme: 'extraVaultedInfoTheme',
        animation: 'fade',
        allowHTML: true,
        onHide: function (instance) {
            //Wait for the animation and then reset the confirmations
            setTimeout(() => {
                $(document.getElementById("tooltipFiltersWFMordersActionsTemplate")).find(".filterOption").removeClass("confirming")
            }, 350);
        },
        //   trigger: 'click',
    });
}

function createWFMcontractsActionsTippy() {
    tippy('#settingFilterExpandingWFMContractsActions', {
        content: document.getElementById("tooltipFiltersWFMcontractsActionsTemplate").outerHTML,
        duration: 190,
        arrow: true,
        //  delay: [0, 0],
        offset: [0, 15],
        interactive: true,
        placement: 'bottom',
        theme: 'extraVaultedInfoTheme',
        animation: 'fade',
        allowHTML: true,
        onHide: function (instance) {
            //Wait for the animation and then reset the confirmations
            setTimeout(() => {
                $(document.getElementById("tooltipFiltersWFMcontractsActionsTemplate")).find(".filterOption").removeClass("confirming")
            }, 350);
        },
        //   trigger: 'click',
    });
}


tippy.delegate('#MyOrdersTab', {
    target: '.wfmItemOwnedWarning',
    content: "Missing items detected. Click here to remove them",
    duration: 190,
    arrow: true,
    placement: 'top',
    theme: 'extraVaultedInfoTheme',
    animation: 'fade',
    // trigger: 'click',
});

wfmOrdersApp = Vue.createApp({
    data() {
        return {
            topLeftCategories: ["All", "Parts", "Relics", "Mods", "Arcanes", "Misc", "Sets"],
            selectedCategory: "All",

            items: [],
            loading: false,
            summaryPlatinum: "- -",
            summaryDucats: "- -",
            noItems: true,

            refreshedAtLeastOnce: false,

            selectedOrderingMode: "name",
            selectedOrderingLargerToSmaller: true,
            selectedSearch: "",
            yesnoFilters: {},

            totalSellPlat: 0,
            totalBuyPlat: 0
        }
    },
    mounted: function () {
        createWFMordersFilterTippy();
        createWFMordersActionsTippy();
    },
    methods: {
        changeSetting(category) {
            this.selectedCategory = category;
            this.refresh();
        },
        refreshIfNecessary() {
            this.refresh();
        },
        refresh() {

            var filterSettings = {
                type: this.selectedCategory.toLowerCase(),
                search: this.selectedSearch,
                order: this.selectedOrderingMode,
                orderLargerToSmaller: this.selectedOrderingLargerToSmaller,
                yesnoFilters: JSON.stringify(this.yesnoFilters)
            };

            //console.log(filterSettings);

            this.loading = true;
            // this.items = [];

            plugin.get().GetWFMOrders(JSON.stringify(filterSettings), (success, data, totalWTS, totalWTB) => {
                if (success) {
                    setTimeout(() => { this.loading = false; }, 100);
                    this.noItems = false;
                    this.items = JSON.parse(data);

                    this.totalSellPlat = totalWTS;
                    this.totalBuyPlat = totalWTB;
                    if (this.items != undefined && this.items.length > 0) {
                        this.refreshedAtLeastOnce = true;
                    }
                    //  console.log(this.items);
                    if (this.items == undefined || this.items.length == 0) this.noItems = true;
                } else {
                    this.items = [];
                    this.noItems = true;

                    setTimeout(() => { this.loading = false; }, 350);
                }
            });
        },
        removeOrder(id) {
            this.loading = true;
            plugin.get().DoWFMarketRemoveListing(id, (success, error) => {
                setTimeout(() => {
                    this.loading = false;
                    if (success) {
                        // setTimeout(RefreshInventory, 1000);
                    } else {
                        console.log("Failed to remove order: " + error);
                        showErrorToast("Failed to remove order: " + error);
                    }
                }, 400);
            })

            sendMetric("WFM_Remove", "listing");
        },
        markAsSold(id) {
            this.loading = true;
            plugin.get().DoWFMarketMarkAsDoneListing(id, 1, (success, error) => {
                setTimeout(() => {
                    this.loading = false;
                    if (success) {
                        // setTimeout(RefreshInventory, 1000);
                    } else {
                        console.log("Failed to mark as sold order: " + error);
                        showErrorToast("Failed to mark as sold order: " + error);
                    }
                }, 400);
            })

            sendMetric("WFM_MarkAsSold", "");
        },
        changeVisibility(id, plat, amount, visible) {
            this.loading = true;
            plugin.get().DoWFMarketUpdateListing(id, amount, plat, !visible, (success, error) => {
                setTimeout(() => {
                    this.loading = false;
                    if (success) {
                        //setTimeout(RefreshInventory, 1000);
                    } else {
                        console.log("Failed to change visibility to order: " + error);
                        showErrorToast("Failed to change visibility to order: " + error);
                    }
                }, 400);
            })
            sendMetric("WFM_Visibility", "listing");
        },
        addOne(id, plat, amount, visible) {
            this.loading = true;
            plugin.get().DoWFMarketUpdateListing(id, amount + 1, plat, visible, (success, error) => {
                setTimeout(() => {
                    this.loading = false;
                    if (success) {
                        //setTimeout(RefreshInventory, 1000);
                    } else {
                        console.log("Failed to add one to order: " + error);
                        showErrorToast("Failed to add one to order: " + error);
                    }
                }, 300);
            })
            sendMetric("WFM_AddOne", "");
        },
        toggleEditMode(order) {
            if (order.editing === true) {
                this.loading = true;
                plugin.get().DoWFMarketUpdateListing(order.randomID, order.amountOnSale, order.platinumPerItem, order.orderVisible, (success, error) => {
                    setTimeout(() => {
                        this.loading = false;
                        if (success) {
                            //setTimeout(RefreshInventory, 1000);
                        } else {
                            console.log("Failed to edit order: " + error);
                            showErrorToast("Failed to edit order: " + error);
                        }
                    }, 400);
                })
                sendMetric("WFM_Edit", "listing");
            } else {
                order.editing = true;
            }
        },
        showMore(order) {
            showBuySellPanelWithItem(order.urlName, order.modLevel, order.name);
            buySellPanelApp.currentMode = "sell";
        },
        setAllVisible() {
            this.loading = true;
            plugin.get().WFMAllVisible((success, error) => {
                this.loading = false;
                if (!success) {
                    console.log("Failed to set all visible: " + error);
                    showErrorToast("Failed to set all visible: " + error);
                }
            });
            sendMetric("WFM_AllVisibile", "listing");
        },
        setAllInvisible() {
            this.loading = true;
            plugin.get().WFMAllInvisible((success, error) => {
                this.loading = false;
                if (!success) {
                    console.log("Failed to set all invisible: " + error);
                    showErrorToast("Failed to set all invisible: " + error);
                }
            });
            sendMetric("WFM_AllInvisibile", "listing");
        },
        removeAll() {
            this.loading = true;
            plugin.get().WFMRemoveAll((success, error) => {
                this.loading = false;
                if (!success) {
                    console.log("Failed to remove all: " + error);
                    showErrorToast("Failed to remove all: " + error);
                }
            });
            sendMetric("WFM_RemoveAll", "listing");
        },
        fixOrders() {
            this.loading = true;
            plugin.get().FixWFMOrders("all", (success, error) => {
                this.loading = false;
                if (!success) {
                    console.log("Failed to fix orders: " + error);
                    showErrorToast("Failed to fix orders: " + error);
                }
            });
            sendMetric("WFM_FixAll", "listing");
        },
        fixSingleOrder(order) {
            this.loading = true;
            plugin.get().FixWFMOrders(order.randomID, (success, error) => {
                this.loading = false;
                if (!success) {
                    console.log("Failed to fix single order: " + error);
                    showErrorToast("Failed to fix single order: " + error);
                }
            });
            sendMetric("WFM_FixSingle", "listing");
        }
    }
}).mount("#MyOrdersTab");

function changeWFMOrderFilter(event, filter, newMode) {
    var shouldRemove = wfmOrdersApp.yesnoFilters[filter] == newMode;

    if (shouldRemove) {
        delete wfmOrdersApp.yesnoFilters[filter];
        event.target.classList.remove("pulsed");
    } else {
        wfmOrdersApp.yesnoFilters[filter] = newMode;
        $(event.target.parentElement.children).removeClass("pulsed");
        event.target.classList.add("pulsed");
    }

    wfmOrdersApp.refresh();
}

setTimeout(wfmOrdersApp.refresh, 1000);


plugin.get().onWFMarketOrdersUpdated.addListener(function (text) {
    wfmOrdersApp.refresh();
});


var lastContractsAppExclamationTooltips = [];

tippy.delegate('#MyContractsTab', {
    target: '.wfmItemOwnedWarning',
    content: "Missing items detected. Click here to remove them",
    duration: 190,
    arrow: true,
    placement: 'top',
    theme: 'extraVaultedInfoTheme',
    animation: 'fade',
    // trigger: 'click',
});

wfmContractsApp = Vue.createApp({
    data() {
        return {
            topLeftCategories: ["All", "Rifle", "Shotgun", "Pistol", "Melee", "Kitgun", "Zaw", "Arch"],
            selectedCategory: "All",

            items: [],
            loading: false,
            summaryPlatinum: "- -",
            summaryDucats: "- -",
            noItems: true,

            refreshedAtLeastOnce: false,

            selectedOrderingMode: "name",
            selectedOrderingLargerToSmaller: true,
            selectedSearch: "",
            yesnoFilters: {}
        }
    },
    mounted: function () {
        createWFMcontractsActionsTippy();
        createWFMcontractsFilterTippy();
    },
    methods: {
        changeSetting(category) {
            this.selectedCategory = category;
            this.refresh();
        },
        refreshIfNecessary() {
            this.refresh();
        },
        refresh() {

            var filterSettings = {
                type: this.selectedCategory.toLowerCase(),
                search: this.selectedSearch,
                order: this.selectedOrderingMode,
                orderLargerToSmaller: this.selectedOrderingLargerToSmaller,
                yesnoFilters: JSON.stringify(this.yesnoFilters)
            };

            //console.log(filterSettings);

            this.loading = true;
            // this.items = [];

            plugin.get().GetWFMContracts(JSON.stringify(filterSettings), (success, data) => {
                if (success) {
                    setTimeout(() => { this.loading = false; }, 100);
                    this.noItems = false;
                    this.items = JSON.parse(data);
                    if (this.items != undefined && this.items.length > 0) {
                        this.refreshedAtLeastOnce = true;
                    }
                    //   console.log(this.items);
                    if (this.items == undefined || this.items.length == 0) this.noItems = true;

                    setTimeout(() => {
                        if (lastContractsAppExclamationTooltips != undefined && lastContractsAppExclamationTooltips.length > 0) {
                            for (var i = 0; i < lastContractsAppExclamationTooltips.length; i++) {
                                lastContractsAppExclamationTooltips[i].destroy();
                            }
                            lastContractsAppExclamationTooltips = [];
                        }

                        /*lastContractsAppExclamationTooltips = tippy('.wfmItemOwnedWarning', {
                            content: "Missing items. Click to fix (remove them from WFMarket)",
                            duration: 190,
                            arrow: true,
                            //  delay: [0, 0],
                            placement: 'top',
                            theme: 'extraVaultedInfoTheme',
                            animation: 'fade',
                            allowHTML: true,
                            // trigger: 'click',
                        });*/

                    }, 250);
                } else {
                    this.items = [];
                    this.noItems = true;

                    setTimeout(() => { this.loading = false; }, 350);
                }
            });
        },
        removeOrder(id) {
            this.loading = true;
            plugin.get().WFMContractsRemove(id, (success, error) => {
                setTimeout(() => {
                    this.loading = false;
                    if (success) {
                        setTimeout(unveiledRivensApp?.refresh(), 1750);
                    } else {
                        console.log("Failed to remove contract: " + error);
                        showErrorToast("Failed to remove contract: " + error);
                    }
                }, 400);
            })

            sendMetric("WFM_Remove", "contract");
        },
        changeVisibility(id, visible) {
            this.loading = true;
            plugin.get().WFMContractsSetVisibility(id, visible, (success, error) => {
                setTimeout(() => {
                    this.loading = false;
                    if (success) {
                        //setTimeout(RefreshInventory, 1000);
                    } else {
                        console.log("Failed to change visibility to contract: " + error);
                        showErrorToast("Failed to change visibility to contract: " + error);
                    }
                }, 400);
            })
            sendMetric("WFM_Visibility", "contract");
        },
        toggleEditMode(order) {
            rivenDetailsApp.openFromWFM(order.randomID, true);
        },
        setAllVisible() {
            this.loading = true;
            plugin.get().WFMContractsAllVisibility(true, (success, error) => {
                this.loading = false;
                if (!success) {
                    console.log("Failed to set all visible: " + error);
                    showErrorToast("Failed to set all visible: " + error);
                }
            })
            sendMetric("WFM_AllVisible", "contract");
        },
        setAllInvisible() {
            this.loading = true;
            plugin.get().WFMContractsAllVisibility(false, (success, error) => {
                this.loading = false;
                if (!success) {
                    console.log("Failed to set all invisible: " + error);
                    showErrorToast("Failed to set all invisible: " + error);
                }
            });
            sendMetric("WFM_AllInvisible", "contract");
        },
        removeAll() {
            this.loading = true;
            plugin.get().WFMContractsRemoveAll((success, error) => {
                this.loading = false;
                if (!success) {
                    console.log("Failed to remove all: " + error);
                    showErrorToast("Failed to remove all: " + error);
                }
            });
            sendMetric("WFM_RemoveAll", "contract");
        },
        openLink(item) {
            overwolf.utils.openUrlInDefaultBrowser("https://warframe.market/auction/" + item.randomID);
            sendMetric("WFM_OpenContractLink", "");
        },
        fixOrders() {
            this.loading = true;
            plugin.get().FixWFMContracts("", (success, error) => {
                this.loading = false;
                if (!success) {
                    console.log("Failed to fix contracts: " + error);
                    showErrorToast("Failed to fix contracts: " + error);
                }
            });
            sendMetric("WFM_FixAll", "contract");
        },
        fixSingleOrder(order) {
            this.loading = true;
            plugin.get().FixWFMContracts(order.randomID, (success, error) => {
                this.loading = false;
                if (!success) {
                    console.log("Failed to fix single contracts: " + error);
                    showErrorToast("Failed to fix single contracts: " + error);
                }
            });
            sendMetric("WFM_FixSingle", "contract");
        }
    }
}).mount("#MyContractsTab");


function changeWFMContractFilter(event, filter, newMode) {
    var shouldRemove = wfmContractsApp.yesnoFilters[filter] == newMode;

    if (shouldRemove) {
        delete wfmContractsApp.yesnoFilters[filter];
        event.target.classList.remove("pulsed");
    } else {
        wfmContractsApp.yesnoFilters[filter] = newMode;
        $(event.target.parentElement.children).removeClass("pulsed");
        event.target.classList.add("pulsed");
    }

    wfmContractsApp.refresh();
}

setTimeout(wfmContractsApp.refresh, 1000);

plugin.get().onWFMarketContractsUpdated.addListener(function (text) {
    wfmContractsApp.refresh();
});


//Login stuff

function UpdateWFMTabLoginStuff() {
    plugin.get().GetWFMarketStatus((isLogedIn, wfmStatus) => {
        //console.log("Logged in update: " + isLogedIn);
        if (isLogedIn) {
            $(".wfmLoginOverlay").addClass("hidden");
        } else {
            if (plugin.get().IsWarframeMarketSlow()) {
                $(".wfmLoginOverlay").addClass("hidden");
                $(".wfmDownOverlay").removeClass("hidden");
            } else {
                $(".wfmLoginOverlay").removeClass("hidden");
                $(".wfmDownOverlay").addClass("hidden");
            }
        }
    });
}
setTimeout(UpdateWFMTabLoginStuff, 1000);

plugin.get().onWFMarketLoginStatusChanged.addListener((nothing) => {
    UpdateWFMTabLoginStuff();
});

function DoWFMTabLogout() {
    plugin.get().DoWFMarketLogout((success) => { });
    sendMetric("WFM_Logout", "");
}

function doWFMRegister() {
    overwolf.utils.openUrlInDefaultBrowser("https://warframe.market/")
    sendMetric("WFM_TabRegister", "");
}

function doWFMLogin() {
    $("#wfmLoginLoadingOverlay").removeClass("hidden");

    setTimeout(() => {
        plugin.get().DoWFMarketLogin($("#wfmLoginEmail")[0].value, $("#wfmLoginPassword")[0].value, (success, error) => {
            $("#wfmLoginLoadingOverlay").addClass("hidden");
            $("#wfmLoginPassword")[0].value = "";
            if (success) {
                $("#wfmLoginEmail")[0].value = "";
                showSuccesfulToast("Login successful!");
                sendMetric("WFM_Login", "");
                //loginShown = false;
            } else {
                console.log("Failed to login: " + error);
                showErrorToast("Login failed: " + error);
            }
        });
    }, 300);
}

//This function shows a warning sign on the first click and, if clicked again, executes the function
function WFMarketConfirmButtonClick(event, functionToCall) {

    let targetToUse = event.srcElement;
    if (targetToUse.classList.contains("wfmFilterConfirmOverlay")) {
        targetToUse = targetToUse.parentElement;
    }

    if (targetToUse.classList.contains("confirming")) {
        targetToUse.classList.remove("confirming");
        functionToCall();
    } else {
        targetToUse.classList.add("confirming");
    }
}


