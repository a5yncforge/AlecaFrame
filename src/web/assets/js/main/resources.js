
resourcesApp = Vue.createApp({
    data() {
        return {
            data: {},
            loading: false,
            noItems: true,
            resourcesFavOnly: false,
            resourcesOrderingMode: "normal",
        }
    },
    methods: {
        refresh() {

            this.loading = true;
            this.itemsSelected = {};

            plugin.get().getResourcesTabData(this.resourcesFavOnly, this.resourcesOrderingMode, (res, data) => {
                if (res) {

                    setTimeout(() => { this.loading = false; }, 250);
                    this.noItems = false;

                    this.data = JSON.parse(data);

                    console.log(this.data);

                } else {
                    this.data = {};
                    this.noItems = true;

                    setTimeout(() => { this.loading = false; }, 450);
                }
            });
        },
        openDetails(uID) {
            if (uID == undefined || uID == '') return;
            foundryDetailsApp.open(uID);
            sendMetric("MasteryHelper_OpenDetails", "");
        },
    },
    mounted: function () {

    },
    computed: {
    }
}).mount("#tabResources");
