var mainWindow = overwolf.windows.getMainWindow();
var plugin = mainWindow.plugin;

overwolf.windows.setZoom(-0.5);


var buildsApp = Vue.createApp({
    data() {
        return {

            initializing: true,
            showBigError: false,
            bigErrorMessage: "",

            buildSelectedItemIndex: 0,
            neededThingsShowTotal: true,
            statViewIndexSelected: 0,

            buildStatus: {},
            modBeingDraggedInfo: {},

            modBrowserData: {},
            modBrowserCategories: ["mod", "exilus", "aura", "stance", "arcane", "shard", "incarnon"],
            selectedModBrowserCategory: "mod",
            selectedModSlotIndex: -1,
            modBrowserSearch: "",
            modBrowserPreview: { visible: false },

            selectedWeaponModeID: 0,
            selectedWeaponZoomID: -1,

            availableEnemyPresets: {},
            selectedEnemyPreset: "",
            sourceEnemyList: {},
            enemyEditorVisible: false,
            enemyEditorFilteringGroups: ["All", "Grineer", "Corpus", "Infested", "Corrupted", "Sentient", "Amalgam", "Narmer", "Orokin", "Duviri", "Murmur", "Other"],
            enemyEditorSelectedFilter: "All",
            enemyEditorSearch: "",
            currentEnemySetupStatus: {},
            enemySetupDraggingEnemyUniqueID: "",

            simResults: {},

            adsShown: true,
        }
    },
    mounted: function () {
        this.initialize();
        this.adsShown = shouldAdsBeShown();
    },
    updated: function () {
    },
    methods: {
        initialize() {
            this.initializing = true;
            this.showBigError = false;

            plugin.get().BuildInitialize((success, message) => {
                this.initializing = false;
                if (success) {
                    this.refreshBuildState();
                    this.refreshEnemyPresets();
                } else {
                    this.showBigError = true;
                    this.bigErrorMessage = message;
                }
            });

        },
        refreshBuildState() {
            let requestData = {
                neededThingsShowTotal: this.neededThingsShowTotal,
                selectedItemIndex: this.buildSelectedItemIndex,
                statViewIndexSelected: this.statViewIndexSelected
            };

            plugin.get().GetBuildStatus(JSON.stringify(requestData), (success, data) => {
                if (success) {
                    let buildDataObject = JSON.parse(data);
                    console.log(buildDataObject);
                    this.buildStatus = buildDataObject;
                } else {
                    this.showBigError = true;
                    this.bigErrorMessage = data;
                }
            });
        },

        dragUpgradeStart(event, upgradeIndex, upgradeType) {
            this.modBeingDraggedInfo.upgradeIndex = upgradeIndex;
            this.modBeingDraggedInfo.uniqueID = "";
            this.modBeingDraggedInfo.upgradeType = upgradeType;
        },
        dragUpgradeOver(event, upgradeIndex, upgradeType) {
            if (upgradeIndex == undefined && this.modBeingDraggedInfo.upgradeIndex != -66) return; //Check the drop source index and also allow drops on the mod browser

            if (this.modBeingDraggedInfo.upgradeIndex == upgradeIndex) return; //Don't allow drags & drops of the same index
            if (this.modBeingDraggedInfo.upgradeType != upgradeType) return; //Don't allow drags & drops of the same type

            event.preventDefault(); //Allow everything else 
        },
        dragUpgradeDrop(event, upgradeIndex, upgradeType) {
            event.preventDefault(); //Prevent the browser from navigating to the file

            if (upgradeIndex == undefined && this.modBeingDraggedInfo.upgradeIndex != -66) return; //Check the drop source index and also allow drops on the mod browser

            let eventIndex = this.modBeingDraggedInfo.upgradeIndex;

            if (eventIndex == undefined) return; //Not a valid drop target)            

            if (this.modBeingDraggedInfo.upgradeIndex == upgradeIndex) return; //Don't allow drags & drops of the same index
            if (this.modBeingDraggedInfo.upgradeType != upgradeType) return; //Don't allow drags & drops of the same type



            if (this.modBeingDraggedInfo.upgradeIndex == -66) {
                this.placeNewMod(upgradeIndex, this.modBeingDraggedInfo.upgradeType, this.modBeingDraggedInfo.uniqueID);
            } else {
                this.switchMods(eventIndex, upgradeIndex ?? 0, this.modBeingDraggedInfo.upgradeType);
            }
            this.modBeingDraggedInfo = {};
        },
        switchMods(fromIndex, toIndex, specialSlotName) {
            let switchData = {
                currentItemIndex: this.buildSelectedItemIndex,
                specialSlotName: specialSlotName,
                fromIndex: fromIndex,
                toIndex: toIndex
            };
            plugin.get().BuildsSwitchMods(JSON.stringify(switchData), (success, message) => {
                if (success) {
                    this.refreshBuildState();
                    this.refreshModBrowser();
                } else {
                    showErrorToast("Failed to switch mods: " + message);
                }
            });
        },
        placeNewMod(index, specialSlotName, uniqueID) {
            let placeData = {
                currentItemIndex: this.buildSelectedItemIndex,
                index: index,
                specialSlotName: specialSlotName,
                uniqueID: uniqueID
            };
            plugin.get().BuildsPlaceNewMod(JSON.stringify(placeData), (success, message) => {
                if (success) {
                    this.refreshBuildState();
                    this.refreshModBrowser();
                } else {
                    showErrorToast("Failed to place new mod: " + message);
                }
            });
        },
        removeMod(index, specialSlotName) {
            let removeData = {
                currentItemIndex: this.buildSelectedItemIndex,
                index: index,
                specialSlotName: specialSlotName
            };
            console.log(removeData);

            plugin.get().BuildsRemoveModFromSlot(JSON.stringify(removeData), (success, message) => {
                if (success) {
                    this.refreshBuildState();
                    this.refreshModBrowser();
                } else {
                    showErrorToast("Failed to remove mod: " + message);
                }
            });
        },
        clickUpgrade(event, index, upgradeData, specialSlotName) {
            this.selectedModBrowserCategory = specialSlotName?.toLowerCase();
            this.selectedModSlotIndex = index;

            console.log(this.selectedModBrowserCategory);
            this.refreshModBrowser();
        },
        rightClickUpgrade(event, index, upgradeData, specialSlotName) {
            event.preventDefault(); //Prevent the browser from showing the context menu
            if (upgradeData.used == false) return; //Only allow right clicks on used upgrades

            this.removeMod(index, specialSlotName);
        },
        refreshModBrowser() {
            let requestData = {
                currentItemIndex: this.buildSelectedItemIndex,
                category: this.selectedModBrowserCategory,
                search: this.modBrowserSearch,
                selectedModSlotIndex: this.selectedModSlotIndex
            };
            plugin.get().BuildsGetModBrowser(JSON.stringify(requestData), (success, data) => {                
                if (!success) { //Successful responses go through the event
                    showErrorToast("Failed to get mod browser data: " + data);
                }
            });
        },
        dragModBrowserUpgradeStart(event, upgrade) {
            this.modBeingDraggedInfo.upgradeIndex = -66; //-66 means it's from the mod browser
            this.modBeingDraggedInfo.uniqueID = upgrade.uniqueName;
            this.modBeingDraggedInfo.upgradeType = upgrade.placingFilter;
            this.modBrowserMouseLeave(upgrade); //Hide the preview when dragging
        },
        modBrowserMouseEnter(upgrade) {
            this.modBrowserPreview.visible = true;
            this.modBrowserPreview.picture = upgrade.picture;
            this.modBrowserPreview.description = upgrade.description;
        },
        modBrowserMouseLeave(upgrade) {
            this.modBrowserPreview.visible = false;
        },
        buildSettingsChanged() {
            let requestData = {
                weaponModeID: this.selectedWeaponModeID,
                weaponZoomLevelID: this.selectedWeaponZoomID,
            };
            plugin.get().BuildsSetBuildSettings(JSON.stringify(requestData), (success, message) => {
                if (success) {
                    this.refreshBuildState();
                    this.refreshModBrowser();
                } else {
                    showErrorToast("Failed to set build settings: " + message);
                }
            });
        },
        refreshEnemyPresets() {
            plugin.get().BuildsEnemySetupGetList((success, data) => {
                if (success) {
                    console.log(JSON.parse(data));
                    this.availableEnemyPresets = JSON.parse(data);
                    this.selectedEnemyPreset = this.availableEnemyPresets.selectedPreset;
                }
                else {
                    showErrorToast("Failed to get enemy presets: " + data);
                }
            });
        },
        refreshEnemySelectionList(showAll = false) {
            let requestData = {
                search: this.enemyEditorSearch,
                faction: this.enemyEditorSelectedFilter,
                isBoss: "",
                isEximus: "",
                showAll: showAll
            };
            plugin.get().BuildsEnemyCustomizationGetEnemies(JSON.stringify(requestData), (success, data) => {
                if (success) {
                    console.log(JSON.parse(data));
                    this.sourceEnemyList = JSON.parse(data);
                }
                else {
                    showErrorToast("Failed to get enemy list: " + data);
                }
            });
        },
        selectEnemyPreset() {
            let requestData = {
                presetName: this.selectedEnemyPreset
            };
            plugin.get().BuildsEnemySetupSelect(JSON.stringify(requestData), (success, message) => {
                if (success) {
                    this.refreshEnemyPresets();
                    this.refreshModBrowser();
                } else {
                    showErrorToast("Failed to select enemy preset: " + message);
                }
            });
        },
        openEnemyEditor() {
            this.refreshEnemySelectionList();
            this.refreshCurrentEnemySetupStatus();
            this.enemyEditorVisible = true;
        },
        UnfocusElement() {
            document.activeElement.blur();
        },
        refreshCurrentEnemySetupStatus() {
            plugin.get().BuildsEnemyCustomizationGetCurrentEnemiesStatus((success, data) => {
                if (success) {
                    console.log(JSON.parse(data));
                    this.currentEnemySetupStatus = JSON.parse(data);
                }
                else {
                    showErrorToast("Failed to get current enemy setup: " + data);
                }
            });
        },
        addEnemyToCurrentSetup(uniqueName) {
            let requestData = {
                uniqueName: uniqueName
            };
            plugin.get().BuildsEnemyCustomizationAddEnemy(JSON.stringify(requestData), (success, message) => {
                if (success) {
                    this.refreshCurrentEnemySetupStatus();
                    this.refreshModBrowser();
                } else {
                    showErrorToast("Failed to add enemy to setup: " + message);
                }
            });
        },
        changeEnemySettings(index, level, amount) {
            let requestData = {
                index: index,
                level: level,
                amount: amount
            };
            plugin.get().BuildsEnemyCustomizationChangeEnemySettings(JSON.stringify(requestData), (success, message) => {
                if (success) {
                    this.refreshCurrentEnemySetupStatus();
                    this.refreshModBrowser();
                } else {
                    showErrorToast("Failed to change enemy settings: " + message);
                }
            });
        },
        enemySetupDragStart(event, uniqueID) {
            console.log("Hi");
            this.enemySetupDraggingEnemyUniqueID = uniqueID;
        },
        enemySetupDragOver(event) {
            event.preventDefault();
        },
        enemySetupDragDrop(event) {
            console.log(this.enemySetupDraggingEnemyUniqueID)
            event.preventDefault();
            this.addEnemyToCurrentSetup(this.enemySetupDraggingEnemyUniqueID);
        },
        toSI(number) {
            if (number > 1000000) {
                return (number / 1000000).toFixed(2) + "M";
            } else if (number > 1000) {
                return (number / 1000).toFixed(2) + "K";
            } else {
                return number;
            }
        },
        toTime(number) {
            let minutes = Math.floor(number / 60);
            let seconds = Math.floor(number % 60);
            return  (minutes>0?(minutes + "m "):"") + seconds + "s";
        },
        roundTo(number, decimals, includePlusIfPositive) {
            return ((number>=0) ? "+":"") + number.toFixed(decimals);
        },        
    },
    computed: {
        buildModList: function () {
            let toReturn = [];
            for (let i = 0; i < this.buildStatus.mods.mods.length; i++) {
                let mod = this.buildStatus.mods.mods[i];
                toReturn.push({ type: "mod", data: mod, extraClassName: "mod" + i, index: i });
            }

            if (this.buildStatus.mods.exilus != undefined) {
                toReturn.push({ type: "exilus", data: this.buildStatus.mods.exilus });
            }

            if (this.buildStatus.mods.aura != undefined) {
                toReturn.push({ type: "aura", data: this.buildStatus.mods.aura });
            }

            if (this.buildStatus.mods.stance != undefined) {
                toReturn.push({ type: "stance", data: this.buildStatus.mods.stance });
            }

            return toReturn;
        },
        filteredModBrowserMods: function () {
            //Use search and order by dps (or name if dps is not available)
            let toReturn = this.modBrowserData.mods;
            if (toReturn == undefined) return [];

            let searchLowerCased = this.modBrowserSearch.toLowerCase();

            //If you edit the filtering, also add it in the local backend. They must match (done this to optimize the calculations)
            toReturn = toReturn.filter((mod) => {
                if (mod.name.toLowerCase().includes(searchLowerCased)) return true;
                if (mod.description.toLowerCase().includes(searchLowerCased)) return true;
                if (mod.stats.some(x => x.stat.toLowerCase().includes(searchLowerCased))) return true;
                if (mod.stats.some(x => x.value.toLowerCase().includes(searchLowerCased))) return true;
                return false;
            });

            function getModeOrderingVariable(currentMod) {
                if (!currentMod.compatible) return -1000000 + currentMod.dps;
                if (currentMod.dps == undefined) return currentMod.name;
                return currentMod.dps;
            }

            toReturn.sort((a, b) => { return getModeOrderingVariable(b) - getModeOrderingVariable(a) });

            return toReturn;
        }
    }
}).mount("#buildAppHolder");

function OnSimulationResultsUpdate(eventName, eventDataStr) {
    switch (eventName) {
        case "results":
            let eventData = JSON.parse(eventDataStr);
            //console.log(eventName, eventData);
            buildsApp.simResults = eventData;
        default:
            console.log("Unhandled event", eventName, eventDataStr);
            break;
    }
}

plugin.get().OnSimulationResultsUpdate.removeListener(OnSimulationResultsUpdate);
plugin.get().OnSimulationResultsUpdate.addListener(OnSimulationResultsUpdate);


function OnBuildsModBrowserUpdate(success, data) {    
    if (success) {
        let modBrowserDataObject = JSON.parse(data);
        //console.log(modBrowserDataObject);
        buildsApp.modBrowserData = modBrowserDataObject;
    } else {
        console.log("Failed to get mod browser update: " + data);
        showErrorToast("Failed to get mod browser update: " + data);
    }
}

plugin.get().OnBuildsModBrowserUpdate.removeListener(OnBuildsModBrowserUpdate);
plugin.get().OnBuildsModBrowserUpdate.addListener(OnBuildsModBrowserUpdate);

