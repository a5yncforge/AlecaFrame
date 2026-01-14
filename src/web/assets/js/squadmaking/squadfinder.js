
var connection = new signalR.HubConnectionBuilder().withUrl((localStorage["squadsHTTP"] ? "http" : "https") + "://squads.tennofinder.com/api/squadfinder").build();

var tryingToReconnect = false;

connection.onclose(function () {
    //if (tryingToReconnect) {
    showErrorToast("Disconnected from the server. Trying to reconnect...");
    // }

    setTimeout(function () {
        TryStartConnection();
    }, 5000); // Restart connection after 5 seconds.
});

function TryStartConnection() {
    connection.start().then(() => {
        console.log("Connected to the server");

        setTimeout(() => {
            availableSquadsApp.bigModalErrorVisible = false;
            availableSquadsApp.bigModalConnectingVisible = false;
        }, 250);
        RefreshAvailableSquads();
        RefreshAccountState();
    }).catch((err) => {
        console.log(err);

        availableSquadsApp.bigModalErrorVisible = true;
        availableSquadsApp.bigModalConnectingVisible = false;

        setTimeout(function () {
            TryStartConnection();
        }, 5000); // Restart connection after 5 seconds.
    });
}

TryStartConnection();


availableSquadsApp = Vue.createApp({
    data() {
        return {

            gameModes: gameModeList,
            selectedCategory: gameModeList[0], //By default "All"

            squads: [],

            bigModalErrorVisible: false,
            bigModalConnectingVisible: true,

            summaryPlatinum: "- -",
            summaryDucats: "- -",
            noItems: true,

            squadWindowOpen: false,

            dropDownVisible: false,

            refreshedAtLeastOnce: false,

            reportUserVisible: false,

            lastRefreshDate: -1,
            isDataStale: false,

            accountState: undefined,

            squadListLoading: false,
            rightColumnLoading: false,

            selectedOrderingMode: "name",
            selectedOrderingLargerToSmaller: true,
            selectedSearch: "",
            yesnoFilters: {},

            createSquadVisible: false,
            createSquadCategory: "RelicOpeningBasic",
            createSquadTitle: "",
            createSquadExtraGameModeRequirement: "",
            createSquadMaxSquadSize: 4,
            createSquadMinReputation: 0,
            createSquadRequirementList: [],
            createSquadSelectedRequirements: [],
            createSquadLoading: false,
            addReqSearchText: "",
            squadCreationStep: 0,
            createSquadLevel: "casual",

            reportTermsAccepted: false,
            reportText: "",
            reportUsername: "",

            FTUEvisible: false,
            FTUEprogress: 0,
            ftueRulesAccepted: false,

            rulesShown: false
        }
    },
    mounted: function () {
        // createWFMordersFilterTippy();
        //  createWFMordersActionsTippy();
        setInterval(() => {
            DoCheckSquadOpenedWork();
        }, 1000);

        tippy('.createSquadButton', {
            content: "You can only create/join 1 squad at the same time!",
            duration: 190,
            arrow: true,
            //  delay: [0, 0],
            offset: [0, 15],
            interactive: true,
            placement: 'bottom',
            theme: 'extraVaultedInfoTheme',
            animation: 'fade',
            allowHTML: true,
            onShow(instance) { return availableSquadsApp.squadWindowOpen; }
            // trigger: 'click',
        });

        tippy.delegate('.squadListContainer', {
            target: '.squadItemJoin',
            content: "Not currently available",
            duration: 190,
            arrow: true,
            placement: 'bottom',
            theme: 'extraVaultedInfoTheme',
            animation: 'fade',
            onShow(instance) { instance.setContent(instance.reference.getAttribute("disable-reason")); return instance.reference.classList.contains("disabled"); },
            //trigger: 'click',
        });

        if (localStorage["tennoFinderFTUEdone"] !== "true") {
            this.FTUEvisible = true;
        }
    },
    methods: {
        FTUEdone() {

            if (this.rulesShown) { //The rules UI is part of the FTUE
                this.FTUEvisible = false;

                setTimeout(() => {
                    this.rulesShown = false;
                }, 500); //Wait for the animation to finish
                return;
            }

            if (!this.ftueRulesAccepted) return;

            localStorage["tennoFinderFTUEdone"] = "true";
            this.FTUEvisible = false;
        },
        changeSetting(category) {
            this.selectedCategory = category;
            this.refresh();
        },
        refreshIfNecessary() {
            this.refresh();
        },
        showReportUser(usernameToReport) {
            this.reportTermsAccepted = false;
            this.reportText = "";
            this.reportUsername = usernameToReport;
            this.reportUserVisible = true;
        },
        refreshSquadList() {
            RefreshAvailableSquads();
        }, refresh(rawData) {

            this.squads = rawData.availableGroups;
            this.squads.forEach((squad) => {
                squad.requirementsParsed = JSON.parse(squad.extraGameModeRequirements);
            });
        },
        getDisplayGameModeName(gameModeName) {
            return this.gameModes.find(p => p.internalName == gameModeName)?.name ?? "N/A";
        },
        getCreateSquadSelectedMode() {
            return this.gameModes.find(p => p.internalName == this.createSquadCategory);
        },
        getGameMode(gameModeInternalName) {
            return this.gameModes.find(p => p.internalName == gameModeInternalName);
        },
        createSquad() {

            plugin.get().GetSquadLoginData((success, userID, username) => {
                if (success) {
                    let squadCreationObject = {
                        warframeName: username,
                        userID: userID,
                        title: this.createSquadTitle,
                        extraGameModeRequirements: JSON.stringify(this.createSquadSelectedRequirements),
                        gameMode: this.createSquadCategory,
                        maxSlots: this.createSquadMaxSquadSize,
                        minReputation: this.createSquadMinReputation,
                        level: this.createSquadLevel
                    }

                    this.createSquadLoading = true;
                    connection.invoke("CreateSquad", squadCreationObject).then((createdSquadID) => {
                        this.createSquadLoading = false;
                        console.log("Created squad: " + createdSquadID);
                        showSuccesfulToast("Squad created: " + createdSquadID);
                        this.createSquadVisible = false;
                        this.joinSquad(createdSquadID);
                    }).catch((error) => {
                        this.createSquadLoading = false;
                        console.log("Failed to create squad: " + error);
                        showErrorToast(error);
                    });
                } else {
                    console.log("Cancelling squad creation because user is not logged in");
                    showErrorToast("Your inventory data needs to be recognized at least once to create a squad.");
                }
            });


        },
        showCreateSquad() {
            if (this.squadWindowOpen) return;  //Already in a squad

            this.createSquadVisible = true;
            this.createSquadCategory = "RelicOpeningBasic";
            this.createSquadSelectedRequirements = [];
            this.squadCreationStep = 0;
            this.addReqSearchText = "";
            this.createSquadLevel = "casual";
            this.UpdateSquadRequirementSuggestions();
        },
        createSquadCategoryChanged(internalName) {            
            if (internalName == 'RelicOpeningAdvanced') return; //Not supported yet
            this.createSquadCategory = internalName;
            this.squadCreationStep = 1;
            this.createSquadSelectedRequirements = [];
            this.UpdateSquadRequirementSuggestions();
        },
        UpdateSquadRequirementSuggestions() {
            plugin.get().GetSquadRequirementsOptions(this.createSquadCategory, (success, data) => {
                if (success) {
                    this.createSquadRequirementList = JSON.parse(data);                   
                } else {
                    this.createSquadRequirementList = [];
                    showErrorToast(data);
                }
            });
        },
        filteredReqs() {
            return this.createSquadRequirementList.filter(p => this.createSquadSelectedRequirements.find(q => q.name == p.name) == undefined && p.name.toLowerCase().includes(this.addReqSearchText.toLowerCase())).slice(0, 25);
        },
        addReqItemClicked(item) {
            this.createSquadSelectedRequirements.push(item);
        },
        removeReqItemClicked(item) {
            this.createSquadSelectedRequirements.splice(this.createSquadSelectedRequirements.indexOf(item), 1);
        },
        joinSquad(uniqueID, refreshSquadListAfterJoining = true) {
            if (this.squadWindowOpen) return;  //Already in a squad

            this.squadListLoading = true;

            connection.invoke("SquadHasFreeSlots", uniqueID).then((hasFreeSlots) => {
                if (hasFreeSlots) {
                    mainWindow.squadIDToJoin = uniqueID;
                    overwolf.windows.obtainDeclaredWindow("SquadMakingSquad", (result) => {
                        overwolf.windows.restore(result.window.id, (result2) => {
                            DoCheckSquadOpenedWork();
                            //Not needed anymore, squad list is refreshed when squad window is opened or closed
                            //if (refreshSquadListAfterJoining) setTimeout(() => { RefreshAvailableSquads(); }, 1500);
                        });
                    });
                } else {
                    showErrorToast("Failed to join squad. This squad is probably full or doesn't exist anymore");
                    console.log("Cancelling squad join because squad is full");
                    RefreshAvailableSquads();
                }
                setTimeout(() => { this.squadListLoading = false; }, 350);

            }).catch((error) => {
                console.log("Failed to check slots when joining squad: " + error);
                showErrorToast("Failed to join squad. This squad is probably full or doesn't exist anymore");
                setTimeout(() => { this.squadListLoading = false; }, 350);
                RefreshAvailableSquads();
            });


        },
        getUnableToJoinReason(squad) {
            if (this.squadWindowOpen) {
                return "You can only join 1 squad at the same time";
            } else if (squad.usedSlots >= squad.maxSlots) {
                return "Squad is full";
            }
        }, UnfocusElement() {
            document.activeElement.blur();
        }, getFilteredSquads() {
            let selectedSearchLowerCase = this.selectedSearch.toLowerCase();
            return this.squads.filter(p => (p.gameMode == this.selectedCategory.internalName || this.selectedCategory.internalName == "INTERNAL_ALL")
                &&
                (
                    p.title.toLowerCase().includes(selectedSearchLowerCase)
                    ||
                    p.requirementsParsed.some(req => req.name.toLowerCase().includes(selectedSearchLowerCase))
                )                
                )
                .sort((a, b) => { });
        },
        blockPlayer(player) {
            connection.invoke("BlockPlayer", player).then((success) => {
                if (success) {
                    showSuccesfulToast("Player blocked. You can unblock them from the 'Blocked players' list");
                } else {
                    showErrorToast("Failed to block player");
                }
                RefreshAccountState();
            }).catch((err) => {
                console.log("Failed to block player: " + err);
                showErrorToast(err);
            });
        },
        unblockPlayer(player) {
            connection.invoke("UnblockPlayer", player).then((success) => {
                if (success) {
                    showSuccesfulToast("Player unblocked successfully.");
                } else {
                    showErrorToast("Failed to unblock player");
                }
                RefreshAccountState();
            }).catch((err) => {
                console.log("Failed to unblock player: " + err);
                showErrorToast(err);
            });
        },
        sendReport() {

            if (!this.reportTermsAccepted) {
                showErrorToast("You need to accept the terms before reporting a player");
                return;
            }

            if (this.reportText.length < 5) {
                showErrorToast("You need to write a reason for reporting the player");
                return;
            }

            connection.invoke("ReportPlayer", this.reportUsername, this.reportText).then((success) => {
                if (success) {
                    showSuccesfulToast("Player reported successfully");
                    this.reportUserVisible = false;
                } else {
                    showErrorToast("Failed to report player");
                }
                RefreshAccountState();
            }).catch((err) => {
                console.log("Failed to report player: " + err);
                showErrorToast(err);
            });
        },
        repPlayer(player) {
            connection.invoke("ReviewPlayer", player).then((success) => {
                if (success) {
                    showSuccesfulToast("Player reviewed");
                } else {
                    showErrorToast("Failed to review player");
                }
                RefreshAccountState();
            }).catch((err) => {
                console.log("Failed to review player: " + err);
                showErrorToast(err);
            });
        },
        unrepPlayer(player) {
            connection.invoke("UnreviewPlayer", player).then((success) => {
                if (success) {
                    showSuccesfulToast("Player review removed");
                } else {
                    showErrorToast("Failed to remove player review");
                }
                RefreshAccountState();
            }).catch((err) => {
                console.log("Failed to remove review from player: " + err);
                showErrorToast(err);
            });
        },
        showRules() {
            this.rulesShown = true;
            this.FTUEprogress = 6;
            this.FTUEvisible = true;
        }
    }
}).mount("#vueMain");

setInterval(() => { RefreshAccountState(false); }, 60000);

function RefreshAccountState(showLoading = true) {

    if (connection.state == "Connected") {

        if (showLoading) availableSquadsApp.rightColumnLoading = true;
        plugin.get().GetSquadLoginData((success, userID, username) => {
            if (success) {
                connection.invoke("GetAccountState", userID, username).then((data) => {
                    availableSquadsApp.accountState = data;                  
                    if (showLoading) setTimeout(() => { availableSquadsApp.rightColumnLoading = false; }, 550);
                }).catch((err) => {
                    console.log("Failed to get account state: " + err);
                    if (showLoading) setTimeout(() => { availableSquadsApp.rightColumnLoading = false; }, 550);
                });
            } else {
                if (showLoading) availableSquadsApp.rightColumnLoading = false;
            }
        });

    }
}

function RefreshAvailableSquads(showLoading = true) {

    if(connection == undefined) return;

    if (connection.state == "Connected") {

        if (showLoading) availableSquadsApp.squadListLoading = true;

        connection.invoke("GetAvailableSquads").then((data) => {
            availableSquadsApp?.refresh(data);          
            if (showLoading) setTimeout(() => { availableSquadsApp.squadListLoading = false; }, 350);
        }).catch((err) => {
            console.log("Failed to get available squads: " + err);
            if (showLoading) setTimeout(() => { availableSquadsApp.squadListLoading = false; }, 350);
        });

        availableSquadsApp.lastRefreshDate = new Date();

    }
}

setInterval(() => { RefreshAvailableSquads(false); }, 60000);

/*function changeWFMOrderFilter(event, filter, newMode) {
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
}*/


function doTabHeaderWork(event) {
    var element = event.currentTarget;

    $(element.parentElement).children().removeClass("selected");
    $(element).addClass("selected");

    $("#" + element.getAttribute("tabid")).parent().children().removeClass("shown");
    $("#" + element.getAttribute("tabid")).addClass("shown");
};

function DoCheckSquadOpenedWork() {
    overwolf.windows.getWindow("SquadMakingSquad", (result) => {
        let newState = result.success;
        if (newState != availableSquadsApp.squadWindowOpen) {
            availableSquadsApp.squadWindowOpen = newState;

            if (newState) {
                //When joining a squad, it might take a while to actuallly join it.
                setTimeout(() => {
                    RefreshAvailableSquads();
                }, 1500);
            } else {
                setTimeout(() => {
                    RefreshAccountState();
                    RefreshAvailableSquads();
                }, 500);

            }

        }
    });
    this.isDataStale = this.lastRefreshDate != -1 && (Date.now() - this.lastRefreshDate) > 60000 * 1;
}