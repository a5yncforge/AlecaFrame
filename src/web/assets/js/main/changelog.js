
var changelogList = [
    {
        //versions holds an array of versions, which, if updated to them, the changelog will be shown unless the previous version is also a part of it
        versions: ["2.6.0", "2.6.11", "2.6.12", "2.6.13", "2.6.16", "2.6.17", "2.6.18", "2.6.19", "2.6.20", "2.6.21", "2.6.22", "2.6.23", "2.6.24", "2.6.25", "2.6.26", "2.6.27", "2.6.28", "2.6.33", "2.6.34", "2.6.36", "2.6.37", "2.6.38", "2.6.39", "2.6.40", "2.6.41", "2.6.42", "2.6.43", "2.6.44", "2.6.45", "2.6.47", "2.6.49", "2.6.51", "2.6.52", "2.6.53", "2.6.54", "2.6.55", "2.6.56", "2.6.57", "2.6.58", "2.6.59", "2.6.60", "2.6.61", "2.6.62", "2.6.63", "2.6.64", "2.6.65", "2.6.66", "2.6.67", "2.6.68", "2.6.69"], changes: [
            { type: "big", text: "New 'Mastery helper' tab", image: "assets/img/changelog/2.6/masteryHelper.png", imageHeight: 250 },
            { type: "big", text: "New overlay for riven rerolling (Only English supported)", image: "assets/img/changelog/2.6/rivenReroll.png", imageHeight: 204 },
            { type: "big", text: "You can now support AlecaFrame in Patreon and get new exclusive perks!", image: "assets/img/patreon2.png", imageHeight: 110 },            
            { type: "big", text: "New overlay for chat rivens (Only English supported)", image: "assets/img/changelog/2.6/rivenChat.png", imageHeight: 245 },  
            { type: "big", text: "New Trading Analytics tab (Premium only)", image: "assets/img/changelog/2.6/tradingAnalytics.png", imageHeight: 214 },
            { type: "big", text: "New 'Riven Finder' tab. Detailed riven info, good rolls and find your dream riven in the market", image: "assets/img/changelog/2.6/rivenFinder.png", imageHeight: 321 },
            { type: "big", text: "New 'Riven Sniper' tab. Get notified whenever a riven your want is listed online", image: "assets/img/changelog/2.6/rivenSniper.png", imageHeight: 302 },
            { type: "big", text: "Added WFMarket message notifications", image: "assets/img/changelog/2.6/wfmarket.png", imageHeight: 30 },
            { type: "big", text: "Crafting trees! Rendering implemented by @f4nat1c", image: "assets/img/changelog/2.6/craftingTrees.png", imageHeight: 320 },
            { type: "big", text: "New main window layout" },
            { type: "big", text: "Added current void fissures (with customizable notifications) and reset timers" },            
            { type: "big", text: "Your rivens (and attributes) are now given a grade depending on how good they are. (Based on @44bananas excelent work)" },
            { type: "big", text: "See which relics can drop a blueprint when hovering over a part in the foundry" },
            { type: "small", text: "You can now fav relics" },
            { type: "small", text: "Archon shards in a Warframe are now shown in the foundry. Implemented by @atomeistee" },
            { type: "small", text: "Added a tracker for the current Circuit rotation." },
            { type: "small", text: "New visuals in the relic overlay to show the set price. Implemented by @f4nat1c" },
            { type: "small", text: "Added 'favorite' icons to the relic overlay." },
            { type: "small", text: "The relic reward overlay should now adapt better to weird scale/resolution combinations" },
            { type: "small", text: "Your Warframe scaling settings can now be detected automatically" },
            { type: "small", text: "The WFMarket panel will now show a 'Connecting...' overlay if WFMarket is having issues" },
            { type: "small", text: "Aya is now also tracked in the stats tab" },
            { type: "small", text: "Improve OCR response time significantly" },
            { type: "small", text: "Added 'set' entry to remaining weapons in the foundry details dialog" }, 
            { type: "small", text: "Added support for pet imprints in the inventory tab" },
            { type: "small", text: "Added support for faction emotes in the inventory tab" },
            { type: "small", text: "Added support for K-Drives and Amps in the foundry (modular tab)" },
            { type: "small", text: "Added more detailed error messages when posting rivens to WFMarket" }, 
            { type: "small", text: "Reduced relic overlay size when subscribed. (Ad space removed completelly)" },
            { type: "fix", text: "Fixed the \"New items\" notification not working on some accounts. Reported by @leeryway" },    
            { type: "fix", text: "Fixed some scroll bars appearing when not needed" },    
            { type: "fix", text: "Fixed some negative attributes showing a grade opposite than expected" },   
            { type: "fix", text: "Fixed wrong Shedu Chassis icon" },   
            { type: "fix", text: "Changed rivens to use the new 'x1.15' system in faction damage attributes" }, 
            { type: "fix", text: "Fixed an issue with component ownership for weapons made of other weapons" }, 
            { type: "fix", text: "Fixed max mod levels in Railjack mods" }, 
        ]
    },
    {
        //versions holds an array of versions, which, if updated to them, the changelog will be shown unless the previous version is also a part of it
        versions: ["2.5.0", "2.5.1", "2.5.2", "2.5.3", "2.5.4", "2.5.5", "2.5.6", "2.5.7", "2.5.8", "2.5.9", "2.5.10", "2.5.12"], changes: [
            { type: "big", text: "New tab: TennoFinder (Squads). Find squads to open relics, kill bosses and many other things way faster than using the in-game chat!", image: "assets/img/changelog/2.5/tennofinder.png", imageHeight: 125 },
            { type: "small", text: "You can now change the zoom level of the main window.", image: "assets/img/changelog/2.5/zoom.png", imageHeight: 25 },
            { type: "small", text: "The relic planner now shows all relic tiers in the same slot. Suggested by @Scholar_Andrew and @WalfHero", image: "assets/img/changelog/2.5/relicplanner.png", imageHeight: 85 },
            { type: "small", text: "Added a WFMarket icon next to each riven to indicate whether a contract already exists. Suggested by @stevens"},           
            { type: "small", text: "You can now see mods that you don't own in the inventory tab. Suggested by @Why Wholesuhm?" },
            { type: "small", text: "Improved the looks of the mods section in the inventory tab." },
            { type: "small", text: "Added a new setting to prevent AlecaFrame from moving itself automatically to a secondary monitor on startup" },
            { type: "fix", text: "Removed Parallax Blueprint from the Parallax set. Suggested by @Scholar_Andrew" },
            { type: "fix", text: "If the relic recommendation overlay fails to detect the relic type it will now automatically close." },
            { type: "fix", text: "Opening requiem relics won't trigger the relic rewards overlay anymore." },
            { type: "fix", text: "Fixed a few bugs related to parts being built in the Foundry" },
            { type: "fix", text: "Fixed some non-tradeable parts appearing in the inventory sets tab" },
            { type: "fix", text: "Some weapon parts (Ambassador) weren't recongized in the sets tab" },
        ]
    },
    {
        //versions holds an array of versions, which, if updated to them, the changelog will be shown unless the previous version is also a part of it
        versions: ["2.4.32", "2.4.31", "2.4.30", "2.4.29","2.4.28","2.4.27", "2.4.26", "2.4.23","2.4.22", "2.4.21", "2.4.20","2.4.19", "2.4.18", "2.4.17","2.4.16", "2.4.15","2.4.14","2.4.13", "2.4.12","2.4.11"], changes: [
            { type: "big" ,text: "You can now see all relics (including not owned) in the relic planner.", image: "assets/img/changelog/2.4.11/notOwned.png", imageHeight: 60 },
            { type: "big", text: "Added a search bar to find listings for any item in WFMarket", image: "assets/img/changelog/2.4.11/search.png", imageHeight: 105 },
            { type: "big", text: "Items will be automatically marked as sold in WFMarket when a sale is completed. You can also +rep or report a user there.", image: "assets/img/changelog/2.4.11/tradeNotification.png", imageHeight: 125 },
            { type: "big", text: "Added a helminth tracker to the foundry (With its respective filter)", image: "assets/img/changelog/2.4.11/helminth.png", imageHeight: 110 },

            { type: "small", text: "Added the \"Favorite\" filter to the foundry" },
            { type: "small", text: "Added the \"Used for crafting\" filter to the foundry" },
            { type: "small", text: "The spaced reserved for an ad will now be hidden if the user is subscribed" },    
            { type: "small", text: "Added support for more  Baro Ki'Teer items" },    
            { type: "small", text: "Added support for scenes and landing craft parts" },
            { type: "small", text: "Added support for in-app changelogs (Like this one :D)" },
            { type: "small", text: "Added more error messages for WFMarket" },
            { type: "small", text: "Added a new window to help users fix common scaling issues" },

            
            { type: "fix", text: "Fixed the name of some components where its prefix would appear twice" },
            { type: "fix", text: "Fixed the picture of some weapon components (such as stocks)" },
        ]
    }
];


changelogApp = Vue.createApp({
    data() {
        return {
            visible: false,     
            data: null,
        }
    },
    methods: {
        openIfNeccessary() {
            if (typeof overwolf === 'undefined') return;

            let newVersionInstalled = plugin.get().newVersionAvailable;
            let versionInstalledLastTime = plugin.get().lastSavedVersion;
            if (newVersionInstalled == undefined || newVersionInstalled == "") return;

            let changelogVersionToShow = changelogList.find(p => p.versions.some(p => p == newVersionInstalled));
            if (changelogVersionToShow != undefined) {
                if (changelogVersionToShow.versions.some(p => p == versionInstalledLastTime)) {
                    console.log("New version detected: " + newVersionInstalled + " but the changelog was already shown for version " + versionInstalledLastTime);
                } else {
                    this.open(newVersionInstalled)
                }                
            } else {
                console.log("New version detected: " + newVersionInstalled + " but no changelog found.");
            }         
        },
        open(version) {
            console.log("Showing changelog for version " + version);
            let changelogObject = changelogList.find(p => p.versions.some(p => p == version));
            this.data = changelogObject;
            this.data.version = version;

            this.visible = true;
            escapeKeyHandlersStack.push(() => { this.visible = false });

            sendMetric("Modal_ChangelogOpen", version);
        },
        getDiscordChangelog(version) {
            console.log("Creating Discord changelog for version " + version);
            let changelogObject = changelogList.find(p => p.versions.some(p => p == version));

            let changelog = "";
            changelog += "Version **" + version + "** changelog:\n";
            changelogObject.changes.forEach(change => {
                changelog += "- " + this.getDiscordEmote(change.type) + " " + change.text + "\n";
            });
            console.log(changelog);
                
        },
        getDiscordEmote(type) {
            switch (type) {
                case "big": return ":green_circle:";
                case "small": return ":WFMadd:";
                case "fix": return ":wrench:";
            }        
        }        
    },
    mounted() {

     
    }

}).mount("#modalUpdated");