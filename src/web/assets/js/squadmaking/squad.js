
var connection = new signalR.HubConnectionBuilder().withUrl((localStorage["squadsHTTP"]?"http":"https")+"://squads.tennofinder.com/api/squadfinder").build();

var tryingToReconnect = false;

connection.on("SquadUpdate", (squad) => {
    //console.log(squad);
    squadApp?.refresh(squad);
});

connection.on("SquadEnd", () => {
    squadApp.squadEnded = true;
    console.log("Received SquadEnd event");
    connection?.stop();
});

connection.on("Kicked", () => {
    squadApp.kicked = true;
    console.log("Received Kicked event");
    connection?.stop();
});

//type can be "join" "leave" "kick" "disband" "message"
connection.on("SquadNotification", (type) => {
    console.log("Received SquadNotification event: " + type);

    if (type.includes("message-")) {
        showBlueToast("A message was sent to the squad.");
        plugin.get().showInGameNotification("New message from " + type.replace("message-", "").replace("-", ": "), "TennoFinder", true);
    } else {
        switch (type) {
            case "join":
                showBlueToast("A player joined the squad.");
                plugin.get().showInGameNotification("A player has joined your squad", "TennoFinder",true);
                break;
            case "leave":
                showBlueToast("A player left the squad.");
                plugin.get().showInGameNotification("A player has left your squad", "TennoFinder",true);
                break;
            case "guestAdded":
                showBlueToast("A guest player was added.");
                plugin.get().showInGameNotification("A guest player has been added", "TennoFinder",false);
                break;
            case "guestRemoved":
                showBlueToast("A guest player was removed.");
                plugin.get().showInGameNotification("A guest player has been removed", "TennoFinder",false);
                break;
            case "kick":
                showBlueToast("A player was kicked from the squad.");
                plugin.get().showInGameNotification("A player was kicked from the squad", "TennoFinder", false);
                break;
            case "disband":
                showBlueToast("The squad was disbanded.");
                plugin.get().showInGameNotification("The squad was disbanded", "TennoFinder", false);
                break;
            default:
                break;
        }    
    }
               
});

connection.onclose(function () {
    //if (tryingToReconnect) {
    showErrorToast("Disconnected from the server.");
    squadApp.bigModalErrorVisible = true;
    squadApp.bigModalConnectingVisible = false;      
    // }

});



function TryStartConnection() {

    connection.start().then(() => {
        console.log("Connected to the server");    

        plugin.get().GetSquadLoginData((success, userID, username) => {
            if (success) {
                squadApp.userID = userID;
                squadApp.username = username;
                squadApp.squadID = mainWindow.squadIDToJoin;

                connection.invoke("JoinSquad", squadApp.userID, squadApp.username, squadApp.squadID).then((data) => {
                    squadApp?.refresh(data);
                    setTimeout(() => {
                        squadApp.bigModalErrorVisible = false;
                        squadApp.bigModalConnectingVisible = false;
                    }, 250);
                }).catch((err) => {
                    console.log(err);
                    showErrorToast(err);                   
                    squadApp.bigModalErrorVisible = true;
                    squadApp.bigModalConnectingVisible = false;                 
                });
            } else {
                console.log("Cancelling squad join because user is not logged in!");
                showErrorToast("Your inventory data needs to be recognized at least once to join a squad.");

                squadApp.bigModalErrorVisible = true;
                squadApp.bigModalConnectingVisible = false;

                $("#errorText1").text("Your warframe account needs to be detected at least once to use this feature.");
                $("#errorText1").text("You can follow the troubleshooter in the main window or ask for help on Discord");
            }
        });
        
        
    }).catch((err) => {
        console.log(err);
        showErrorToast(err);

        squadApp.bigModalErrorVisible = true;
        squadApp.bigModalConnectingVisible = false;     

        //setTimeout(function () {
        //    TryStartConnection();
        //}, 5000); // Restart connection after 5 seconds.
    });
}


squadApp = Vue.createApp({
    data() {
        return {

            userID: "",
            username: "",
            squadID: "",

            bigModalErrorVisible: false,
            bigModalConnectingVisible: true,

            gameModes: gameModeList,

            squadData: {},
            chatInput: "",


            squadEnded: false,
            kicked: false
        }
    },
    mounted: function () {
        // createWFMordersFilterTippy();
        //  createWFMordersActionsTippy();
    },
    updated: function () {
        //Get last chat element (if exists)
        const el = document.querySelectorAll('.squadChatMessage:last-child')[0];
        //Scroll to it
        if (el) {
            el.scrollIntoView();
        }
    },
    methods: {
        refresh(rawData) {

            this.squadData = rawData;
            this.squadData.squadInfo.requirementsParsed = JSON.parse(this.squadData.squadInfo.extraGameModeRequirements);
            console.log(this.squadData);

            return;

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
        sendChatMessage() {
            if (this.chatInput == "") return;

            if (this.chatInput.length > 100) {
                showErrorToast("Message too long!");
                return;
            }

           // this.squadData.messages.push({ guid: "temp", message: this.chatInput, username: this.username });
            connection.invoke("SendChatMessage", this.chatInput).catch((err) => {
                showErrorToast(err);
                console.log("Failed to send message: " + err);
            });
            this.chatInput = "";
        },
        addGuest() {
            connection.invoke("AddGuest").catch((err) => {
                showErrorToast(err);
                console.log("Failed to add guest: " + err);
            });
        },
        removeGuest() {
            connection.invoke("RemoveGuest").catch((err) => {
                showErrorToast(err);
                console.log("Failed to remove guest: " + err);
            });
        },
        closeWindow() {
            close();
        },
        hosting() {
            return this.squadData.squadInfo.ownerData.username == this.username;
        },
        kickPlayer(username) {
            connection.invoke("KickPlayer", username).catch((err) => {
                showErrorToast(err);
                console.log("Failed to kick player: " + err);
            });
        },
        invitePlayer(username) {            
            let commandText = "/invite " + username;
            overwolf.utils.placeOnClipboard(commandText);    
            showSuccesfulToast("Invite command copied to clipboard.");
        }
    }
}).mount("#vueMain");



TryStartConnection();