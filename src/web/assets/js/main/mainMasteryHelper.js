

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



masteryHelperApp = Vue.createApp({
    data() {
        return {
            data: {},
            loading: false,          
            noItems: true,           
            extraPercentShowing: 0,
            orderingMode: "normal",
            itemsSelected: {}
        }
    },
    methods: {
        refresh() {
                   
            this.loading = true;
            this.itemsSelected = {};

            plugin.get().getMasteryTabData(this.orderingMode, (res, data) => {
                if (res) {

                    setTimeout(() => { this.loading = false; }, 250);
                    this.noItems = false;

                    this.data = JSON.parse(data);                  

                } else {
                    this.data = {};
                    this.noItems = true;

                    setTimeout(() => { this.loading = false; }, 450);
                }
            });
        },   
        selectOrderingMode(newMode) {
            this.orderingMode = newMode;
            this.refresh();
        },
        openDetails(uID) {
            if(uID == undefined || uID == '') return;
            foundryDetailsApp.open(uID);
            sendMetric("MasteryHelper_OpenDetails", "");
        },
        hoverStart(itemID, hoverXP, isFav) {

            if(isFav) return; //Favs are accounted for in the favExtraPercent
            
            if (hoverXP == undefined || hoverXP == 0 || this.data == undefined) return;
            this.itemsSelected[itemID] = hoverXP;
        },
        hoverStop(itemID) {
            if (this.itemsSelected[itemID] == undefined) return;
            delete this.itemsSelected[itemID];
        },
        setFavStatus(uniqueID,expGives, isFav) {
            plugin.get().SetFavouriteStatus(uniqueID, isFav);

            //When something gets fav, we don-t want to show it as selected in the extra percent
            if (this.itemsSelected[uniqueID] != undefined) {
                if(isFav) delete this.itemsSelected[uniqueID];
            } else {
                if(!isFav) this.itemsSelected[uniqueID] = expGives;
            }                            
        },
    },
    mounted: function () {

    },
    computed: {
        hoveringExtraPercent() {

            let total = 0;
            for (let key in this.itemsSelected) {
                total += this.itemsSelected[key];
            }

            //for (let itemIndex in this.data?.topItems) {
            //    let item = this.data.topItems[itemIndex];
            //    console.log("Item: " + item);
            //    if (item.selectedUI && this.itemsSelected[item.internalName]==undefined) {
            //        total += item.masteryViewData.potentialXPToGet;
            //    }
            //}

            var extraPercent = 100 * (total / this.data.nextLevelXp);
            if (this.data.percent + extraPercent > 100) {
                extraPercent = 100 - this.data.percent;
            }

            return extraPercent;

        },
        favExtraPercent() {
            let total = 0;
            for (let itemIndex in this.data?.topItems) {
                let item = this.data.topItems[itemIndex];
                if (item.isFav) {
                    total += item.masteryViewData.potentialXPToGet;
                }
            }

            var extraPercent = 100 * (total / this.data.nextLevelXp);
            if (this.data.percent + extraPercent > 100) {
                extraPercent = 100 - this.data.percent;
            }
            return extraPercent;
        }
    }
}).mount("#tabMasteryHelper");
