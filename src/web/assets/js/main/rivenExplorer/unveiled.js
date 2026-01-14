unveiledRivensApp = Vue.createApp({
    data() {
        return {
            topLeftCategories: ["All", "Rifle", "Shotgun", "Pistol", "Melee", "Kitgun", "Zaw", "Arch"],
            selectedCategory: "All",

            items: [],
            loading: false,
            summaryPlatinum: "- -",
            summaryDucats: "- -",
            noItems: true,

            selectedOrderingMode: "name",
            selectedOrderingLargerToSmaller: true,
            selectedSearch: "",
        }
    },
    methods: {
        changeSetting(category) {
            this.selectedCategory = category;
            this.refresh(false);
        },
        refreshIfNecessary() {
            this.refresh();
        },
        refresh(loadingEnabled = true) {

            var filterSettings = {
                type: this.selectedCategory.toLowerCase(),
                search: this.selectedSearch,
                order: this.selectedOrderingMode,
                orderLargerToSmaller: this.selectedOrderingLargerToSmaller,
                yesnoFilters: JSON.stringify({})
            };

            //console.log(filterSettings);

            if(loadingEnabled) this.loading = true;
            
            plugin.get().GetFilteredUnveiledRivensInfo(JSON.stringify(filterSettings), (success, data) => {
                if (success) {
                    if (loadingEnabled)  setTimeout(() => { this.loading = false; }, 225);
                    this.noItems = false;
                    this.items = JSON.parse(data);                   
                    if (this.items == undefined || this.items.length == 0) this.noItems = true;
                } else {
                    this.items = [];
                    this.noItems = true;

                    if (loadingEnabled) setTimeout(() => { this.loading = false; }, 350);
                }
            });
        },
        itemClicked(randomID) {
            rivenDetailsApp.open(randomID);
        }
    }
}).mount("#unveiledStashTab");