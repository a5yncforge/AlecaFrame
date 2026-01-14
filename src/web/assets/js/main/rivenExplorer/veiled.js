
veiledRivensApp = Vue.createApp({
    data() {
        return {
            topLeftCategories: ["All", "Rifle", "Shotgun", "Pistol", "Melee", "Kitgun", "Zaw", "Arch"],
            selectedCategory: "All",
            
            itemGroups: [],
            loading: false,
            summaryPlatinum: "- -",
            summaryDucats: "- -",
            noItems: true,

            refreshedAtLeastOnce: false
        }
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

            this.refreshedAtLeastOnce = true;

            var filterSettings = {
                type: this.selectedCategory.toLowerCase(),
                search: "",
                order: "",
                orderLargerToSmaller: true,
                yesnoFilters: JSON.stringify({})
            };

            this.loading = true;
            this.itemGroups = [];
            plugin.get().GetFilteredVeiledRivensInfo(JSON.stringify(filterSettings),(success,data) => {
                if (success) {
                    setTimeout(() => { this.loading = false; }, 250);
                    this.noItems = false;
                    this.itemGroups = JSON.parse(data);                    
                    if (this.itemGroups == undefined || this.itemGroups.length == 0) this.noItems = true;
                } else {
                    this.itemGroups = [];
                    this.noItems = true;

                    setTimeout(() => { this.loading = false; }, 350);
                }
            });
        }
    }
}).mount("#veiledStashTab");