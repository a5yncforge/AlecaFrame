var pickerBig;

settingsApp = Vue.createApp({
    data() {
        return {
            visible: false,
            customScale: 100,
            scalingMode: 'full',
            discordWFMessageEnabled: false,

            themesEnabled: false,
            premiumFeaturesEnabled: false,

            exportName: "",
            exportAuthor: "",

            defaultTheme: {
                name: "Default",
                author: "alecamar",
                colorData: "",
                iconBase64: "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eD0iYWRvYmU6bnM6bWV0YS8iIHg6eG1wdGs9IlhNUCBDb3JlIDQuNC4wLUV4aXYyIj4KIDxyZGY6UkRGIHhtbG5zOnJkZj0iaHR0cDovL3d3dy53My5vcmcvMTk5OS8wMi8yMi1yZGYtc3ludGF4LW5zIyI+CiAgPHJkZjpEZXNjcmlwdGlvbiByZGY6YWJvdXQ9IiIKICAgIHhtbG5zOnhtcE1NPSJodHRwOi8vbnMuYWRvYmUuY29tL3hhcC8xLjAvbW0vIgogICAgeG1sbnM6c3RFdnQ9Imh0dHA6Ly9ucy5hZG9iZS5jb20veGFwLzEuMC9zVHlwZS9SZXNvdXJjZUV2ZW50IyIKICAgIHhtbG5zOmRjPSJodHRwOi8vcHVybC5vcmcvZGMvZWxlbWVudHMvMS4xLyIKICAgIHhtbG5zOkdJTVA9Imh0dHA6Ly93d3cuZ2ltcC5vcmcveG1wLyIKICAgIHhtbG5zOnRpZmY9Imh0dHA6Ly9ucy5hZG9iZS5jb20vdGlmZi8xLjAvIgogICAgeG1sbnM6eG1wPSJodHRwOi8vbnMuYWRvYmUuY29tL3hhcC8xLjAvIgogICB4bXBNTTpEb2N1bWVudElEPSJnaW1wOmRvY2lkOmdpbXA6NTgwZDVkYWMtZDY2YS00YjEwLWI2OWEtNGUyMDhmYjY3ZjkyIgogICB4bXBNTTpJbnN0YW5jZUlEPSJ4bXAuaWlkOjExYzMzYjE3LTVjNDYtNGQ1OC05NTcxLWIzYTM4ZGMzNWU5NCIKICAgeG1wTU06T3JpZ2luYWxEb2N1bWVudElEPSJ4bXAuZGlkOjFkNzBjYzUxLTZjZGQtNDNmMC1hMWYxLTczYmRjOGUxNDEyYSIKICAgZGM6Rm9ybWF0PSJpbWFnZS9wbmciCiAgIEdJTVA6QVBJPSIyLjAiCiAgIEdJTVA6UGxhdGZvcm09IldpbmRvd3MiCiAgIEdJTVA6VGltZVN0YW1wPSIxNjY3MTY2NzQ4MzY0MTcwIgogICBHSU1QOlZlcnNpb249IjIuMTAuMjQiCiAgIHRpZmY6T3JpZW50YXRpb249IjEiCiAgIHhtcDpDcmVhdG9yVG9vbD0iR0lNUCAyLjEwIj4KICAgPHhtcE1NOkhpc3Rvcnk+CiAgICA8cmRmOlNlcT4KICAgICA8cmRmOmxpCiAgICAgIHN0RXZ0OmFjdGlvbj0ic2F2ZWQiCiAgICAgIHN0RXZ0OmNoYW5nZWQ9Ii8iCiAgICAgIHN0RXZ0Omluc3RhbmNlSUQ9InhtcC5paWQ6YWUxNWM4MzEtODU4Mi00ODRlLWE4YWEtMmIwYzEzZTEwYjMwIgogICAgICBzdEV2dDpzb2Z0d2FyZUFnZW50PSJHaW1wIDIuMTAgKFdpbmRvd3MpIgogICAgICBzdEV2dDp3aGVuPSIyMDIyLTEwLTMwVDIyOjUyOjI4Ii8+CiAgICA8L3JkZjpTZXE+CiAgIDwveG1wTU06SGlzdG9yeT4KICA8L3JkZjpEZXNjcmlwdGlvbj4KIDwvcmRmOlJERj4KPC94OnhtcG1ldGE+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAKICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIAogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgCiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAKICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIAogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgCiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAKICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIAogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgCiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAKICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIAogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgCiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAKICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIAogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgCiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAKICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIAogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgCiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAKICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIAogICAgICAgICAgICAgICAgICAgICAgICAgICAKPD94cGFja2V0IGVuZD0idyI/PntEvJAAAAAGYktHRAALADoAW1vWYlQAAAAJcEhZcwAALiMAAC4jAXilP3YAAAAHdElNRQfmCh4VNBxDeaQRAAAAGXRFWHRDb21tZW50AENyZWF0ZWQgd2l0aCBHSU1QV4EOFwAACtJJREFUeNrtnXmsH1UVx7/ntbwuUFpqKQFSSEFKF0EwgQolZREUZQ3woBtQjaAgKmURqFpUIkRDSjSExYCyg2URigpEgmtlLYssZZHKYrVFukFb+l773sc/5vzSYfpbZ+a3zK/zTX55eTNz1+89dznn3HulHDly5MiRI0eOOgOYBgyOPDsJ2CWvnfYiemfgA2BU5Pm9wIfAVcBOeU21B9nXEWBM5PkDbMIq4CfAyLzGskv0WGCdEzo28u53bI4VwA+AT+S1ly2iDbg1ROT4yPuHKI33gO8Dw/KazAbZewMbQgTuFXn/MJWxFDgP2Cav0dYm+74IcXtH3v+B6vE2cBYwMK/Z1iN6YkSqAT4V+eZRasfrwAygM6/l1hmrf1+EqAmR7x4jPp4FjgY62rEOs1SoyZK+UKwdpFimfSXNl/QoMCknuzlS3U/SnBL5JeUymaRDJf0JmAd8Mie7sTjMCSiGviJkpYH+krokvQDMTXuNDpwLXAEMzQfpTZXSASwoM85GNWh/Jx1sdAXNSv//v8AZaUzigDM9foB3gW8DOwK2pZN9FNBbhpQ9It8/TnpYAUwF7gb6/NlTwEEJyjMd6C6S1jqP+xfARb46+DzwGWAUMLCtGwPQvwpJ3T0S5knSxfu+5DsW+Kc/2wDcCOxYY3m6ShBdDr2e3npgifdyNwHfAQ5tGxUwcEwVlbFbJMzTpI/3XMK2dcPK+lBDuBAYVEVZpgEfebgFwDeK6Azi4APgj64cGpJlqX6misLu2gCyC2P23p7GPl7BBSwGvg5sW6Qcg4Afhoj9NTAYGAIsTzmPbwE7Z5HsE0LjZDlE7dnPUD8sKShxvDGe5s8KWAb8EviK5382sMjf9QE/L0zwgO1Ck7+00AdMzBrRW7k2qxrsFAm7kPrirfA8ARjmkvt+ha72rPAEC9g9NCNPszEOzBrZXTUUcGSDyQZ4tUgjGwHM8mFkrU/EVro5dkyRMp5Yh3zNzhrRncA/aui2RjSB7MISbGiJMgwDdi03cfMlVppYXElB04oatBMl7VXltz2SepuUz/0k3VGs2zSzVWb2tpl9VILoQZKOTTEvfZLON7PVmSHbJy/fqyHIOkkbm5jlL0m6xnX3teA4STukmI+bJN2fNd34KZLG10j2hjrpxqvFTEmXVqvd8obxzRTTf1nSeWZGZsgGBki6uMZgKyR1NznrJmm2pNOq/P4ISQeklPYqSVMrdd+tKNldNUq1JL1ZTYtuAPpJuraSztyHqctT6n16JM00sxerDdDRIlLd6dJRK55uocY6SNJdUY1eBOcocJBIXGWSLjCzB7KoLTs9xlKjF9ivSFzP0VwsKLbkAj7tO1XS0JL9KJMWMDfdvRJTsdGvBcnG1aJhbdlIz28aRM+N6yPXCt34yZLGxQh3vZn1Vujq1japTGdLOtqJHi3pYUl7ptB1z/Xuuy+LUj0AeC1GC19eyo4bkuzVbp1a2CTpXgX8xv+m4TUzJ6nXa7Mle5qkPeIoEcxseZmlkCRt45J9lK9FG42hko73v0nwkaSvSboskxIdGqsXxWjla8vtvwaeD3071Z/t5r5eWcPbSVygWkmyT5E0Nka428zsnQpjWwGTJcnMFvs6fk1WZEHSXZL2N7O/Kcvwsfr1GC19PTCuQtzh2fiLkVnx6RWcF1sB/wOmtI1zoXtyxME9VcQddnroAbYPvbM6mBbTxJNRB8qsEz0w5pqzF9i/ivgXRtalh0feD4m5rq835gFb17PumzFmT4u55nw0hnrUJO3zsQdmH0o6Q5tby5qJ6yVNN7O1bUO2G/ovihG0T9KVVRo9osuTCZu1ALMFXsGtgBsknWNmrdT4UiH7ywlcgPpVmUbUlfipEt8N86VNM3F/W+4JT6AtA+iqIZ2nImGXlRoLfStOs/ACDT7bpZHd+HRJY2KEe0NSLaa8aFc/QlKpSp0naWET2v5qBU4Hq9qO7ARjtSRdY2Y9CcjukDS66OwtGCd/XCRMvRUms8zslUa3sEZJ9pSYUr1c0s0ppF9uQ/2Dkl5vYJ3PT6lMrUe2+5ZdEjP4r8xsZQzJieJ83/U4Lmo5MrONkm5vUH2vlPStzBo06jgDX1fBxadUeo9XUMwsAq4EDsPPQQMObtCkbJbaFQksWwC3x0xzQQ1pLHPN1S0NIPrFZu/D6l/n+E9WPMtWn6SrU8zHPQrcbg+VFNY9j1RgDWvEpOwCM1vfrlLdmWBd/URcqw/w10hcSwvrbM/TRILjqd+tcktwGniINj1brVDpMxNUzvQE6f45EtdzZZQ8xzkR9TR7bsjcnukYY3Vcy9J/SHCALJufcPhYhe87gEnAS3Ui++5W4aVeXUuX4nmMStKdZpbEoyS6rCkbl5n1uWFksqRFKddDt6RL25ZsV+x/NwFRtybMQtS9eG0Vee4wsxVOTJpbgG9phqaskbPxqTG1ZQWyx7qFa4MCe3T/0M8UbNHdIOlDScskfRAxfUZVq+srEL2dgl2YF5rZ3UCPpFmS9pc00GfSyyW952mPlrRtFWVZ46rYth2rBwAvpzDO9UZ+fZFfr/tSd/vse0woD3dG4rqxQp4neHyPFOJx96WBBKcODvXDcjr8N5TgHLJKuELtDODUJpkLbwzlYU7k3c0V8nxExE35ZuBIJ7Wf/zqB8QTnso0AtvaDdErhXe8x2lqqFzWJ7EdC+Tg+8u6+CvmeUiLOHuAd4A3f1VFYkz/mEn5Zmf1YM1qRozQnaHG1ZWkgvGvytci7Ssc9DijxfCtJoxRYzIZq006TSZK2V3CsRbHJ3G8l3dG2ZPsM/OImlmN4yG1pqU/eCtixgktTrb5fnd6ony/SsP4t6axWtWqlJdm1noWSNkb5zFkKvECWht7tpPL7rVbESG9f30H6U20yqa6U1GVmS/IZeP2xZyhPD0beHVgm/+Nj6MgfJziFsR/BkZW3EbmYpl0lu6vJUl1A2D886l9+eJlwi1X7Pu7PSnpI0mgzu9zMZpjZS21Nto/Vl7RIWcK7Hf+ij3usnFTG6tQt6ZkY6X1O0sNZcgVOKtmntIhUR8l+QtL7of8nSDq4WCDXvj0SM83BWRpyOxJI9QDF9xitB8bhB827k8A9kXJeCpRSD9+nzdWs1eDaGj1fMzsxm9aCm+Nmh/K3jytGwjivTHnm15jWy/XeiNcqRG+VcAbe43rtJL9iZ3W/GR5DCU7tD6PbVbpWpEz7Uv39HT3AZG0JILiZJq5Lz78ITtEfnPC3lx9QEyX9yFA+d2HzQ983Aj8L79sOfX9FlWW4eksheoB7SsbF2SnmxYDJbvnqLWblctejYhK7guCKhxOA4aGyVerOlxE54zwfq0sfCLNNHfLUAXzRlSnXFXl/qvuhl8Ia4F63dg3xHqMULtlSiE46Vs9qQB47Sjw/gMo3FPT5yQ1nlDBhriFyTUU7kz09wVi9hCbfO+Xd9Aw3U3bHKMP8LYXoWu7uKIZzW6gsBuwAfJXgRsBqb+G5YEshe0oCot+iRW+Tc+IPcv/xSr3WMe1O8mCCKwhXJyD7zAyUs8Nn7m+WKceB7UqyEdzymtR8+WyWjAXAcDdZFpPyie1I9NbADSlsi+kBDs5oQz+XTResFjCpHck+xLfhrHfCNpZx6y316wXmZLweDoucqHRIlstj5Vq3Anee7RS4/HQqsB71V3UXmvRJejbrpwy4S/BMBX5nV5nZq8qRI0eOHDkaj/8DT9Gcubzqz64AAAAASUVORK5CYII=",
            },

            themePresets: [
            ],

            colorPickerData: [
                { name: "Background 1", internalName: "color-bg-1", instance: undefined },
                { name: "Background 2", internalName: "color-bg-2", instance: undefined },
                { name: "Background 3", internalName: "color-bg-3", instance: undefined },
                { name: "Card", internalName: "color-card", instance: undefined },
                { name: "Card - Hover", internalName: "color-card-hover", instance: undefined },
                { name: "Card - Active", internalName: "color-card-active", instance: undefined },
                { name: "Tab", internalName: "color-tab", instance: undefined },
                { name: "Tab - Hover", internalName: "color-tab-hover", instance: undefined },
                { name: "Tab - Active", internalName: "color-tab-active", instance: undefined },
                { name: "Buy", internalName: "color-buy", instance: undefined },
                { name: "Buy - Hover", internalName: "color-buy-hover", instance: undefined },
                { name: "Buy - Active", internalName: "color-buy-active", instance: undefined },
                { name: "Sell", internalName: "color-sell", instance: undefined },
                { name: "Sell - Hover", internalName: "color-sell-hover", instance: undefined },
                { name: "Sell - Active", internalName: "color-sell-active", instance: undefined },
                { name: "Navigation", internalName: "color-navigation", instance: undefined },
                { name: "Navigation - Hover", internalName: "color-navigation-hover", instance: undefined },
                { name: "Navigation - Active", internalName: "color-navigation-active", instance: undefined },
                { name: "Text", internalName: "color-text", instance: undefined },
                { name: "Text on BG", internalName: "color-text-bg", instance: undefined },
                { name: "Icons", internalName: "color-icons", instance: undefined },
                { name: "Icons on BG", internalName: "color-icons-bg", instance: undefined },
            ]
        }
    },
    methods: {
        open() {
            this.visible = true;
            escapeKeyHandlersStack.push(() => { this.visible = false });
        },
        updateColorsToPickers() {
            var computedStyles = getComputedStyle(document.querySelector(':root'));
            for (var i = 0; i < this.colorPickerData.length; i++) {
                var color = computedStyles.getPropertyValue('--' + this.colorPickerData[i].internalName).trim();                
                this.colorPickerData[i].instance.setColor(color);
            }
        },
        copyCurrentThemeToClipboard() {
            overwolf.utils.placeOnClipboard(this.outputColorString());
        },
        importColorString(inputText, showToast = false) {

            for (var i = 0; i < this.colorPickerData.length; i++) {
                document.querySelector(":root").style.removeProperty('--' + this.colorPickerData[i].internalName);
            }

            var lines = inputText.split('\n');
            for (var i = 0; i < lines.length; i++) {
                try {
                    if (lines[i] != "" && lines[i] != undefined) {
                        var internalName = lines[i].split(':')[0].replace('--', '').trim();
                        var color = lines[i].split(':')[1].replace(';', '').trim();
                        document.querySelector(":root").style.setProperty('--' + internalName, color);
                    }
                } catch { }
            }
            this.updateColorsToPickers();

            this.SaveColors();

            if (showToast) showSuccesfulToast("Theme applied!");
        },
        outputColorString() {
            var computedStyles = getComputedStyle(document.querySelector(':root'));
            var outputStr = "";
            for (var i = 0; i < this.colorPickerData.length; i++) {
                var color = this.colorPickerData[i].instance.getColor().hex;
                outputStr += "--" + this.colorPickerData[i].internalName + ": " + color + ";\r\n";
            }
            return outputStr;
        },
        importFromClipboard() {
            overwolf.utils.getFromClipboard((result) => {
                this.importColorString(result);
            });
        },
        SaveColors() {
            var colorData = this.outputColorString();
            localStorage.setItem("colorData", colorData);
        },
        LoadColorsIfNeccessary() {
            var colorData = localStorage.getItem("colorData");
            if (colorData != null) {
                this.importColorString(colorData);
            }
        },
        exportAsThemeFile() {
            overwolf.utils.openFilePicker("*.png", (result) => {
                if (result.success) {
                    var colorData = this.outputColorString();
                    plugin.get().CreateNewThemePreset(colorData, result.file, this.exportName, this.exportAuthor, (exportSuccess, exportError) => {
                        if (exportSuccess) {
                            showSuccesfulToast("Theme created!");
                            this.loadAvailableThemes();
                        } else {
                            showErrorToast(exportError);
                        }
                    });
                }
            });

        },
        loadAvailableThemes() {
            plugin.get().GetAvailableThemePresets((success, themes) => {
                if (success) {
                    this.themePresets = JSON.parse(themes);
                }
            });
        },
        openThemesFolder() {
            plugin.get().OpenThemesFolder();
            showSuccesfulToast("The themes folder will open soon");
        },
        scaleAutodetect(noToastNotifications = false, automaticFirstTime = false) {
            plugin.get().AutodetectWarframeScaling(automaticFirstTime, (success, scaleMode, scale) => {
                if (success) {
                    this.customScale = scale;
                    this.scalingMode = scaleMode;

                    onPersistentSettingChanged();
                    if (!noToastNotifications) showSuccesfulToast("Scale autodetected successfully");
                } else {
                    if (!noToastNotifications) showErrorToast(scaleMode);
                }
            });
        }, exportDataToDesktop() {
            plugin.get().ExportDataToDesktop((success, data) => {
                if (success) {
                    showSuccesfulToast("Data exported successfully!");
                } else {
                    showErrorToast(data);
                }
            });
        }
    },
    mounted() {

        for (var i = 0; i < this.colorPickerData.length; i++) {
            var element = $(".colorPicker[colorName='" + this.colorPickerData[i].internalName + "']")[0];
            this.colorPickerData[i].instance = new Alwan(element, {});
            let internalName = this.colorPickerData[i].internalName;
            this.colorPickerData[i].instance.on("color", (color, element) => { miniColorUpdated(internalName, color) });
        }

        this.updateColorsToPickers();
        this.LoadColorsIfNeccessary();

        // this.loadAvailableThemes(); //Delay this until this tab is actually used
    }

}).mount("#modalSettings");



function miniColorUpdated(internalName, color) {
    document.querySelector(":root").style.setProperty('--' + internalName, color.hex);
    settingsApp.SaveColors();
}

FTUEapp = Vue.createApp({
    data() {
        return {
            visible: false,
            progress: 0,
        }
    },
    methods: {
        openIfNeccessary() {
            plugin.get().IsFTUEDone((response) => {
                if (!response) {
                    console.log("FTUE not completed. Showing it...")
                    FTUEapp.visible = true;
                    setTimeout(() => { currentAd.removeAd(); }, 1500);
                }
            });
        },
        done() {

            console.log("FTUE complete");
            plugin.get().MarkFTUEAsDone((response) => {
                console.log("FTUE mark as done result: " + response);
            });

            this.visible = false;
            currentAd.refreshAd();
            //localStorage.setItem("ftue", "true");
        }
    }

}).mount("#modalFTUE");



//if (localStorage.getItem("ftue") !== "true") {
//    console.log("FTUE not completed. Showing it...")
//    FTUEapp.visible = true;
//    setTimeout(() => { currentAd.removeAd(); }, 1500);
//}


CloseModalApp = Vue.createApp({
    data() {
        return {
            visible: false,
        }
    },
    methods: {
        CloseRequested() {
            if (localStorage["showWarningCloseWindowSetting"] === "false" || (!mainWindow.initializationSuccessful || mainWindow.initializingInProgress)) {
                mainWindow.closeBackground();
                close();
            } else {
                this.visible = true;
                if (typeof variable !== 'undefined') currentAd?.removeAd();
            }
        },
        closeAll() {
            if ($("#dontShowAgainInModel")[0].checked == true) {
                localStorage.setItem("showWarningCloseWindowSetting", "false");
            }

            mainWindow.CloseWithDelay(1500); //Close after 1.5 seconds (and do any closing work)

            close(); //Already close this window (the only visible one) to provide a good UX
        }, close() {
            this.visible = false;
            if (typeof variable !== 'undefined') currentAd?.refreshAd();
        }
    }

}).mount("#modalClose");



helpApp = Vue.createApp({
    data() {
        return {
            visible: false,
            allOK: false,
            checklist: [
                { name: "wfOpen", status: "na" },
                { name: "wfOpenAfterOw", status: "na" },
                { name: "wfNotAdmin", status: "na" },
                { name: "overlayEnabled", status: "na" },
                { name: "overwolfInstall", status: "na" },
                { name: "keepPlaying", status: "na" }
            ]
        }
    },
    methods: {
        open() {
            this.visible = true;
            escapeKeyHandlersStack.push(() => { this.close(); });
            currentAd.removeAd();
            refreshHelpModal();
            sendMetric("Modal_TroubleshooterOpen", "");
        },
        classObject(index) {
            return {
                OK: this.checklist[index].status == 'ok',
                ERROR: this.checklist[index].status == 'error',
                NA: this.checklist[index].status == 'na'
            }
        },
        close() {
            this.visible = false;
            currentAd.refreshAd();
        }
    },
    computed: {

    }

}).mount("#modalHelp");


tippy('#helperChecklist1', {
    content: "<center>Make sure the game is running and you are logged into your game account</center>",
    duration: 190,
    arrow: true,
    placement: 'top',
    theme: 'extraVaultedInfoTheme',
    animation: 'fade',
    allowHTML: true
});

tippy('#helperChecklist2', {
    content: "<center>Warframe needs to be started AFTER Overwolf/AlecaFrame</center>",
    duration: 190,
    arrow: true,
    placement: 'top',
    theme: 'extraVaultedInfoTheme',
    animation: 'fade',
    allowHTML: true
});

tippy('#helperChecklist3', {
    content: "<center>Make sure you are not starting Steam/Launcher/Game as an admin.</center>",
    duration: 190,
    arrow: true,
    placement: 'top',
    theme: 'extraVaultedInfoTheme',
    animation: 'fade',
    allowHTML: true
});

tippy('#helperChecklist4', {
    content: "<center>Make sure the Warframe overlay is enabled. Open the Overwolf settings menu, then go to Overlay & Hotkeys and finally make sure Warframe is enabled. You WILL NEED TO RESTART THE GAME after you change this.</center>",
    duration: 190,
    arrow: true,
    placement: 'top',
    theme: 'extraVaultedInfoTheme',
    animation: 'fade',
    allowHTML: true
});

tippy('#helperChecklist5', {
    content: "<center>If this is marked with an X, it means that your Overwolf install is corrupt. ONLY if marked with a X, please reinstall Overwolf or ask for help on Discord.</center>",
    duration: 190,
    arrow: true,
    placement: 'top',
    theme: 'extraVaultedInfoTheme',
    animation: 'fade',
    allowHTML: true
});

tippy('#helperChecklist6', {
    content: "<center>Sometimes it is neccessary to complete a few missions before Overwolf detects the game</center>",
    duration: 190,
    arrow: true,
    placement: 'top',
    theme: 'extraVaultedInfoTheme',
    animation: 'fade',
    allowHTML: true
});

function refreshHelpModal() {
    if (helpApp.visible) {
        plugin.get().GetHelpChecklist((success, jsonData) => {
            if (success) {
                var data = JSON.parse(jsonData);

                overwolf.games.events.getInfo((dataGames) => {
                    console.log("help game info: " + JSON.stringify(dataGames).substring(0, 300));
                    //  console.log(dataGames);
                    if (data[0] == "ok") {
                        console.log("appling help 4th fix");
                        data[3] = dataGames.success ? "ok" : "error";
                    }

                    for (var i = 0; i < helpApp.checklist.length; i++) {
                        helpApp.checklist[i].status = data[i];
                    }
                });

            } else {
                var data = JSON.parse(jsonData);
                for (var i = 0; i < helpApp.checklist.length; i++) {
                    helpApp.checklist[i].status = "na";
                }
                console.log("Failed to get help checklist: " + jsonData);
            }

            helpApp.allOK = (lastWarframeDataLocalStatus == "OK!");

        });
    }
}

setInterval(refreshHelpModal, 2500);


var lastAbilitiesTooltips = [];
var weaponTypeTooltips = [];
var lastFoundryDetailsSingleTooltip = undefined;
var lastFoundryDetailsCompoentTooltip = undefined;

foundryDetailsApp = Vue.createApp({
    data() {
        return {
            visible: false,
            selectedComponent: {},
            item: {},
            selectedWeaponShootMode: {},
            selectedWeaponMeleeMode: {},
            selectedModTier: {}
        }
    },
    methods: {
        setFavStatus(uniqueID, isFav) {
            plugin.get().SetFavouriteStatus(uniqueID, isFav);
        },
        open(uniqueID, extraOpeningArgs = undefined) {
            this.visible = true;
            escapeKeyHandlersStack.push(() => { this.visible = false });
            //  console.log("Opening foundry details for item: "+uniqueID);
            plugin.get().GetFoundryDetails(uniqueID, (success, data) => {
                if (success) {
                    this.item = JSON.parse(data);


                    // console.log(this.item);
                    if (this.item.components != undefined && this.item.components.length == 1) {
                        this.selectedComponent = this.item.components[0]; //Only one component, we probably want to show it
                    } else {
                        this.selectedComponent = undefined;
                    }
                    if (this.item.itemType == "weaponShoot") {
                        if (this.item.extraWeaponShootData.attacks.length > 0) {
                            this.selectedWeaponShootMode = this.item.extraWeaponShootData.attacks[0];
                        }
                        else {
                            this.selectedWeaponShootMode = undefined;
                        }
                    }


                    if (this.item.itemType == "weaponMelee") {
                        if (this.item.extraWeaponMeleeData.attacks.length > 0) {
                            this.selectedWeaponMeleeMode = this.item.extraWeaponMeleeData.attacks[0];
                        } else {
                            this.selectedWeaponMeleeMode = undefined;
                        }
                    }


                    if (this.item.extraModData != undefined && this.item.extraModData.tiers != undefined && this.item.extraModData.tiers.length > 0) {
                        this.selectedModTier = this.item.extraModData.tiers[this.item.extraModData.tiers.length - 1]; //Only one component, we probably want to show it
                    } else {
                        this.selectedModTier = undefined;
                    }

                    if (this.item.itemType == "mod" && extraOpeningArgs != undefined && extraOpeningArgs < this.item.extraModData.tiers.length) {
                        this.selectedModTier = this.item.extraModData.tiers[extraOpeningArgs];
                    } else if (extraOpeningArgs != undefined && extraOpeningArgs != "") {
                        //Take it as the component UID to select
                        for (var i = 0; i < this.item.components.length; i++) {
                            if (this.item.components[i].baseData.uniqueName == extraOpeningArgs) {
                                this.selectedComponent = this.item.components[i];
                                break;
                            }
                        }
                    }

                    var itemListToRequestPrice = [];
                    this.item.components.forEach((comp) => {
                        itemListToRequestPrice.push(comp.baseData.name);
                    });



                    setTimeout(() => {
                        plugin.get().getHugePriceList(JSON.stringify(itemListToRequestPrice), (success, priceData) => {
                            if (success) {
                                var priceDataDeserialized = JSON.parse(priceData);

                                for (var i = 0; i < this.item.components.length; i++) {
                                    if (priceDataDeserialized[i] != undefined) {
                                        if (priceDataDeserialized[i].post > 0) this.item.components[i].sellPrice = priceDataDeserialized[i].post;
                                        if (priceDataDeserialized[i].insta > 0) this.item.components[i].buyPrice = priceDataDeserialized[i].insta;
                                    }
                                }
                            }
                        });

                    }, 500);


                    //Destroy previous tooltip
                    if (lastFoundryDetailsCompoentTooltip != undefined) {
                        if (lastFoundryDetailsCompoentTooltip.length != undefined) {
                            if (lastFoundryDetailsCompoentTooltip.length > 0) lastFoundryDetailsCompoentTooltip[0].destroy();
                        } else {
                            lastFoundryDetailsCompoentTooltip.destroy();
                        }
                        lastFoundryDetailsCompoentTooltip = undefined;
                    }




                    //Give the UI time to load and then add tooltips (Workaround for using tippy with vue)
                    setTimeout(() => {

                        lastFoundryDetailsCompoentTooltip = tippy('.foundryDetailsTopTopUsedInOthers', {
                            duration: 190,
                            arrow: true,
                            //  delay: [0, 0],
                            placement: 'top',
                            theme: 'extraVaultedInfoTheme',
                            animation: 'fade',
                            allowHTML: true,
                            // trigger: 'click',
                        });

                        if (lastFoundryDetailsSingleTooltip != undefined && (lastFoundryDetailsSingleTooltip.length !== 0)) lastFoundryDetailsSingleTooltip.destroy();

                        switch (this.item.itemType) {
                            case "warframe":
                                {
                                    if (lastAbilitiesTooltips != undefined && lastAbilitiesTooltips.length > 0) {
                                        for (var i = 0; i < lastAbilitiesTooltips.length; i++) {
                                            lastAbilitiesTooltips[i].destroy();
                                        }
                                        lastAbilitiesTooltips = [];
                                    }

                                    lastAbilitiesTooltips = tippy('.foundryDetailsCustomWarframeAbility', {
                                        duration: 190,
                                        arrow: true,
                                        //  delay: [0, 0],
                                        placement: 'top',
                                        theme: 'extraVaultedInfoTheme',
                                        animation: 'fade',
                                        allowHTML: true,
                                        // trigger: 'click',
                                    });

                                    if ($('.foundryDetailsCustomGeneralEntryValueIcon.polarity.aura').length > 0) {
                                        lastFoundryDetailsSingleTooltip = tippy($('.foundryDetailsCustomGeneralEntryValueIcon.polarity.aura')[0], {
                                            duration: 190,
                                            content: "Aura polarity",
                                            arrow: true,
                                            //  delay: [0, 0],
                                            placement: 'top',
                                            theme: 'extraVaultedInfoTheme',
                                            animation: 'fade',
                                            allowHTML: true,
                                            // trigger: 'click',
                                        });
                                    }

                                }
                                break;
                            case "weaponShoot":
                                if (weaponTypeTooltips != undefined && weaponTypeTooltips.length > 0) {
                                    for (var i = 0; i < weaponTypeTooltips.length; i++) {
                                        weaponTypeTooltips[i].destroy();
                                    }
                                    weaponTypeTooltips = [];
                                }

                                weaponTypeTooltips = tippy('.extraWeaponShootDataTypeContainer', {
                                    duration: 190,
                                    arrow: true,
                                    content: "Mod type",
                                    //  delay: [0, 0],
                                    placement: 'top',
                                    theme: 'extraVaultedInfoTheme',
                                    animation: 'fade',
                                    allowHTML: true,
                                    // trigger: 'click',
                                });
                                break;
                            case "weaponMelee":
                                if ($('.foundryDetailsCustomGeneralEntryValueIcon.polarity.aura').length > 0) {
                                    lastFoundryDetailsSingleTooltip = tippy($('.foundryDetailsCustomGeneralEntryValueIcon.polarity.aura')[0], {
                                        duration: 190,
                                        content: "Stance polarity",
                                        arrow: true,
                                        //  delay: [0, 0],
                                        placement: 'top',
                                        theme: 'extraVaultedInfoTheme',
                                        animation: 'fade',
                                        allowHTML: true,
                                        // trigger: 'click',
                                    });
                                }
                                break;
                        }
                    }, 250);
                } else {
                    console.error("Failed to get foundry details: " + data);
                    showErrorToast(data);
                    this.visible = false;
                }
            });
        },
        openCraftingTree() {
            craftingTreeApp.open(this.item.internalName);
        },
        isSelected(component) {
            return {
                selected: component == this.selectedComponent,
                required: component.baseData.recipeNeccessaryComponents,
                isSet: component.isSet,
            }
        },
        isWeaponShootModeSelected(mode) {
            return {
                selected: mode == this.selectedWeaponShootMode
            }
        },
        isWeaponMeleeModeSelected(mode) {
            return {
                selected: mode == this.selectedWeaponMeleeMode
            }
        },
        noiseClasses(noiseType) {
            return {
                noisy: noiseType == "Alarming",
                silent: noiseType != "Alarming"
            }
        },
        dropClasses(drop) {
            return {
                required: drop.ownedAmount > 0,
                isNormalDrop: drop.dropType == 0,
                isMarketDrop: drop.dropType == 2,
            }
        },
        buySellClicked(price, partName, isWTB = false) {
            // if (price == undefined || price < 1) return;
            showBuySellPanelWithItem(partName);
            buySellPanelApp.currentMode = isWTB ? "buy" : "sell";
        },
        relicClicked(uid) {
            if (uid == undefined) return;
            relicDetailsApp.open(uid);
        }
    },
    computed: {

    }

}).mount("#modalFroundryDetails");




var lastRelicDetailsAppVaultedIcon = undefined;
var lastRelicDetailsExpectedValueTooltip = undefined;
var relicDetailsPercentMeaning = [];

relicDetailsApp = Vue.createApp({
    data() {
        return {
            visible: false,
            relic: {},
            selectedRarity: {},
            selectedRarityName: "Intact",
            squadSize: 1,
        }
    },
    methods: {
        open(uniqueID) {

            if (uniqueID.includes("Bronze"))
                this.selectedRarityName = "Intact"
            else if (uniqueID.includes("Silver"))
                this.selectedRarityName = "Exceptional"
            else if (uniqueID.includes("Gold"))
                this.selectedRarityName = "Flawless"
            else if (uniqueID.includes("Platinum"))
                this.selectedRarityName = "Radiant"

            this.squadSize = parseInt(document.getElementById("relicPlannerSquadSize").value);

            this.visible = true;
            escapeKeyHandlersStack.push(() => { this.visible = false });
            // console.log("Opening relic details for item: " + uniqueID);
            plugin.get().GetRelicDetails(uniqueID, false, (success, data) => {
                if (success) {
                    this.relic = JSON.parse(data);
                    this.selectRarity();
                    this.sellPrice = -1;
                    this.buyPrice = -1;

                    plugin.get().GetRelicDetails(uniqueID, true, (success, data) => {
                        if (success) {
                            this.relic = JSON.parse(data);
                            this.selectRarity();
                        } else {
                            console.log("failed to get relic details with platinum!");
                        }

                    });

                    plugin.get().getHugePriceList(JSON.stringify([this.relic.relicName + " Relic"]), (success, priceData) => {

                        if (success) {
                            var priceDataDeserialized = JSON.parse(priceData);
                            this.sellPrice = priceDataDeserialized[0].post;
                            this.buyPrice = priceDataDeserialized[0].insta;
                        }
                    });

                    setTimeout(() => {

                        if (lastRelicDetailsAppVaultedIcon != undefined && (lastRelicDetailsAppVaultedIcon.length !== 0)) lastRelicDetailsAppVaultedIcon.destroy();

                        lastRelicDetailsAppVaultedIcon = tippy($('.relicDetailsTop>.foundryVaultedHolder.relicDetails')[0], {
                            duration: 190,
                            content: "Vault status",
                            arrow: true,
                            placement: 'top',
                            theme: 'extraVaultedInfoTheme',
                            animation: 'fade'
                        });

                        if (lastRelicDetailsExpectedValueTooltip != undefined && (lastRelicDetailsExpectedValueTooltip.length !== 0)) lastRelicDetailsExpectedValueTooltip.destroy();

                        lastRelicDetailsExpectedValueTooltip = tippy($('#relicDetailsBLTopExpected')[0], {
                            duration: 190,
                            content: "Expected (average) returns of this relic",
                            placement: 'top',
                            theme: 'extraVaultedInfoTheme',
                            animation: 'fade'
                        });


                        if (relicDetailsPercentMeaning != undefined && relicDetailsPercentMeaning.length > 0) {
                            for (var i = 0; i < relicDetailsPercentMeaning.length; i++) {
                                relicDetailsPercentMeaning[i].destroy();
                            }
                            relicDetailsPercentMeaning = [];
                        }

                        relicDetailsPercentMeaning = tippy('.relicDetailsBLBottomItemChance', {
                            duration: 190,
                            arrow: true,
                            content: "Mod type",
                            //  delay: [0, 0],
                            placement: 'top',
                            theme: 'extraVaultedInfoTheme',
                            animation: 'fade',
                            allowHTML: true,
                            content: "Probability of getting AT LEAST 1 copy of this reward"
                            // trigger: 'click',
                        });

                    }, 250);
                } else {
                    console.error("Failed to get relic details: " + data);
                    showErrorToast("An error has occurred: " + data);
                    this.visible = false;
                }
            });
        },
        setFavStatus(uniqueID, isFav) {
            plugin.get().SetFavouriteStatus(uniqueID, isFav);
            relicPlannerApp.refresh();
        },
        selectRarity() {
            for (var i = 0; i < this.relic.levels.length; i++) {
                if (this.relic.levels[i].rarity == this.selectedRarityName) {
                    this.selectedRarity = this.relic.levels[i];
                    break;
                }
            }
        },
        dropClasses(drop) {
            return {
                /* required: drop.ownedAmount > 0,
                 isNormalDrop: drop.dropType == 0,
                 isMarketDrop: drop.dropType == 2,*/
            }
        },
    },
    computed: {

    }

}).mount("#modalRelicDetails");






deltaDetailsApp = Vue.createApp({
    data() {
        return {
            visible: false,
            items: {},
            loading: false,
            totalDucats: 0,
            totalPlatinum: 0,
            harrowChassis: 0,
        }
    },
    methods: {
        open() {
            this.visible = true;
            this.loading = true;
            plugin.get().GetDetailedDeltas((success, data) => {
                setTimeout(() => {
                    this.loading = false;
                }, 250);
                if (success) {
                    var parsedData = JSON.parse(data);
                    this.items = parsedData.items;
                    this.totalPlatinum = parsedData.totalPlatinum;
                    this.totalDucats = parsedData.totalDucats;
                    this.harrowChassis = parsedData.totalHarrows;
                } else {
                    console.error("Failed to get delta details: " + data);
                    showErrorToast(data);
                    this.visible = false;
                }
            });
            sendMetric("Modal_DeltasOpen", "");
        },
        onBuySellItemClickedVUE(eventt) {
            onBuySellItemClicked(eventt);
        },
        closeStuff() {
            this.visible = false;
            plugin.get().ClearDeltas();
            $('.deltaViewerNotification').removeClass('shown');
            plugin.get().GenerateDeltaEvent();
        }
    }
}).mount("#deltaDetails");





AutoKeepModal = Vue.createApp({
    data() {
        return {
            visible: false,
        }
    },
    methods: {
        open() {
            if (localStorage["skipWarningAutoKeep"] !== "true") {
                this.visible = true;
                currentAd.removeAd();
            }
        },
        close() {
            this.visible = false;
            currentAd.refreshAd();
        },
        answerYes() {
            localStorage["autoKeepEnabled"] = "true";
            if ($("#dontShowAgainKeepAuto")[0].checked == true) {
                localStorage["skipWarningAutoKeep"] = "true";
            }

            updatePersistentSettingsDisplay();

            this.close();
        },
        answerNo() {
            localStorage["autoKeepEnabled"] = "false";
            if ($("#dontShowAgainKeepAuto")[0].checked == true) {
                localStorage["skipWarningAutoKeep"] = "true";
            }

            updatePersistentSettingsDisplay();

            this.close();
        }
    }

}).mount("#modalKeepAuto");






scaleErrorApp = Vue.createApp({
    data() {
        return {
            visible: false
        }
    },
    methods: {
        open() {
            this.visible = true;
            sendMetric("Modal_ScalingErrorOpen", "");
            escapeKeyHandlersStack.push(() => { this.visible = false });
        }
    },
    mounted() {


    }
}).mount("#modalScalingIssues");




var PatreonApp = Vue.createApp({
    data() {
        return {
            visible: false,
        }
    },
    methods: {
        open() {
            this.visible = true;
            sendMetric("Patreon_ModalOpened", "");
        },
        close() {
            this.visible = false;
        },
        linkPatreonAccount() {
            plugin.get().GetPatreonLinkingURL((success, url) => {
                if (success) {
                    overwolf.utils.openUrlInDefaultBrowser(url);
                    showSuccesfulToast("The patreon linking page has been opened. Once finished, wait 1-2 minutes and restart AlecaFrame.");
                } else {
                    showErrorToast(url);
                }
            });
        }
    }

}).mount("#modalPatreon");



const createDiv = (className) => {
    const element = document.createElement('div')
    element.classList.add(className)
    return element
}
const borderCalcHandler = () => {
    const requirements = document.querySelectorAll(".crafting-tree-requirements")

    requirements.forEach(requirement => {
        const widths = []

        requirement.childNodes.forEach(child => widths.push(child.clientWidth))

        if (widths === 0) return

        const left = widths[0] / 2
        const right = widths[widths.length - 1] / 2

        requirement.style.setProperty('--left', `${left}px`)
        requirement.style.setProperty('--right', `${right}px`)
    })

}

var craftingTreeApp = Vue.createApp({
    data() {
        return {
            visible: false,
            data: {},
            hideCompleted: true,
            lastItemUID: "",
            selectedItem: {},

            loading: false,

            movementOccurred: false,

            treeZoom: 1,
            treeOffset: { x: 0, y: 0 }
        }
    },
    beforeMount() {
        document.addEventListener('treeRendered', borderCalcHandler)
        document.addEventListener('resize', borderCalcHandler)

        window.addEventListener('resize', borderCalcHandler)
    },
    beforeUnmount() {
        document.removeEventListener('treeRendered', borderCalcHandler)
        document.removeEventListener('resize', borderCalcHandler)

        window.removeEventListener('resize', borderCalcHandler)
    },
    methods: {
        renderNode(target, itemToRender, replace = false) {
            const container = createDiv('crafting-tree-item-container')
            const itemNode = createDiv('crafting-tree-item')

            const itemIconNode = createDiv('crafting-tree-item-icon')
            const url = `url(${itemToRender.data.picture})`
            itemIconNode.style.backgroundImage = `${url}`
            if (itemToRender?.dim) itemIconNode.classList.add("dim");

            const itemNameNode = createDiv('crafting-tree-item-name')
            itemNameNode.innerText = itemToRender.data.name

            if (itemToRender.credits > 0) {
                const tooltipNode = createDiv('crafting-tree-tooltip')

                const creditsNode = createDiv('tooltip-credits')
                creditsNode.innerHTML = 'Credits: <span class="bolderTooltip">' + itemToRender.credits + "</span>";

                const hoursNode = createDiv('tooltip-hours')
                hoursNode.innerHTML = 'Duration: <span class="bolderTooltip">' + itemToRender.time + "</span>";

                const outputNode = createDiv('tooltip-output')
                outputNode.innerHTML = 'Output items: <span class="bolderTooltip">' + itemToRender.recipeNumOut + "</span>";

                tooltipNode.appendChild(creditsNode)
                tooltipNode.appendChild(hoursNode)
                tooltipNode.appendChild(outputNode)
                itemNode.classList.add("withTooltip")

                itemNode.appendChild(tooltipNode)
            }

            if (itemToRender.amountNeeded > 1) {
                var amountNeededDiv = createDiv('crafting-tree-item-amount-needed')
                if (itemToRender.amountNeeded > 999) {
                    amountNeededDiv.innerText = Math.round((itemToRender.amountNeeded / 1000) * 10) / 10 + "K";
                } else {
                    amountNeededDiv.innerText = itemToRender.amountNeeded + "x";
                }

                itemNode.appendChild(amountNeededDiv)
            }

            var itemVerticalFlex = createDiv('crafting-tree-item-vertical-flex')
            itemVerticalFlex.appendChild(itemIconNode)
            itemVerticalFlex.appendChild(itemNameNode)

            itemNode.appendChild(itemVerticalFlex)

            itemNode.addEventListener('click', (event) => {

                if (this.movementOccurred) return;

                if (craftingTreeApp.selectedItem == itemToRender) {
                    craftingTreeApp.selectedItem = undefined;
                } else {
                    craftingTreeApp.selectedItem = itemToRender;                                     
                }
                event.stopPropagation();
            });

            if (itemToRender?.shouldLinkToAnotherTree == true) {
                let focusOnElement = createDiv('crafting-tree-item-focus-on');

                focusOnElement.addEventListener('click', (event) => {
                    if (this.movementOccurred) return;

                    craftingTreeApp.open(itemToRender?.data?.uniqueName);          
                    
                    event.stopPropagation();
                });

                itemNode.appendChild(focusOnElement);
            }

            

           


            //Create item ready checkmark
            if (itemToRender?.gotEnough) {
                const itemExtraIconNode = createDiv('crafting-tree-item-icon-extra')
                itemExtraIconNode.classList.add("gotEnough")
                itemNode.classList.add("gotEnough")
                itemIconNode.appendChild(itemExtraIconNode)
            }

            //Create item craftable checkmark
            if (itemToRender?.craftable) {
                const itemExtraIconNode = createDiv('crafting-tree-item-icon-extra')
                itemExtraIconNode.classList.add("craftable")
                itemNode.classList.add("craftable")
                itemIconNode.appendChild(itemExtraIconNode)
            }

            container.appendChild(itemNode)

            if (itemToRender?.children?.length) {
                const resourceNode = createDiv('crafting-tree-requirements')

                if (itemToRender?.gotEnough) {
                    resourceNode.classList.add("dim");
                }

                itemToRender.children.forEach(resource => {
                    this.renderNode(resourceNode, resource)
                });

                container.appendChild(resourceNode)
            }

            if (replace) {
                while (target.firstChild) {
                    target.removeChild(target.firstChild);
                }
            }

            target.appendChild(container);
        },
        renderTree() {
            const treeData = this.data.treeData;
            const container = document.querySelector(".craftingTreeContainer");
            const wrapper = createDiv('craftingTreeWrapper')

            while (container.firstChild) {
                container.removeChild(container.firstChild);
            }
            container.appendChild(wrapper)

            if (treeData != undefined) {
                this.renderNode(wrapper, treeData, true);
            }


            // Center scroll for container
            container.scroll({
                left: (container.scrollWidth / 2) - container.clientWidth / 2,
                behavior: "instant"
            })


            document.dispatchEvent(new Event('treeRendered'));

            //Create a div with the same size as the element with the class craftingTreeContainer

            let absoluteDiv = createDiv("craftingAbsolutePanning");
            absoluteDiv.id = "craftingTreePanning";
            absoluteDiv.style.width = absoluteDiv.style.minWidth = absoluteDiv.style.maxWidth = container.clientWidth + "px";
            absoluteDiv.style.height = absoluteDiv.style.minHeight = absoluteDiv.style.maxHeight = container.clientHeight + "px";

            absoluteDiv.appendChild(wrapper);


            absoluteDiv.addEventListener('wheel', (event) => {

                let oldZoom = this.treeZoom;

                if (event.deltaY > 0) {
                    this.treeZoom /= 1.1;
                } else {
                    this.treeZoom *= 1.1;
                }

                if (this.treeZoom > 1.6) this.treeZoom = 1.6;
                if (this.treeZoom < 0.15) this.treeZoom = 0.15;

                let newZoom = this.treeZoom;

                //Update the xy offset to zoom in on the mouse position
                //        The idea here is to calculate the vector between the mouse position and the center of scaling (in this case the center of the wrapper), 
                //        and calculate where there location where the mouse is pointing will end after the scaling.We calcualte that delta and then substract it to make it seem as if the pointed location isn't moving

                let mousePos = { x: event.clientX, y: event.clientY };

                let containerRect = container.getBoundingClientRect();

                const wrapper = $(".craftingTreeWrapper")[0]
                let wrapperSize = { x: wrapper.clientWidth, y: wrapper.clientHeight };

                let mousePosInContainer = { x: mousePos.x - containerRect.x, y: mousePos.y - containerRect.y };

                let centerOfWrapperInContainerCoords = { x: wrapperSize.x / 2 + this.treeOffset.x, y: wrapperSize.y / 2 + this.treeOffset.y };

                let deltaPointerToCenterOfWrapper = { x: centerOfWrapperInContainerCoords.x - mousePosInContainer.x, y: centerOfWrapperInContainerCoords.y - mousePosInContainer.y };

                let deltaPointerToCenterZoomStart = { x: deltaPointerToCenterOfWrapper.x / oldZoom, y: deltaPointerToCenterOfWrapper.y / oldZoom };
                let deltaPointerToCenterZoomEnd = { x: deltaPointerToCenterZoomStart.x * newZoom, y: deltaPointerToCenterZoomStart.y * newZoom };

                let deltaScaledToNotScaledM2 = { x: deltaPointerToCenterOfWrapper.x - deltaPointerToCenterZoomEnd.x, y: deltaPointerToCenterOfWrapper.y - deltaPointerToCenterZoomEnd.y };

                this.treeOffset.x -= deltaScaledToNotScaledM2.x;
                this.treeOffset.y -= deltaScaledToNotScaledM2.y;

                event.preventDefault();

                this.updatePanningTransform();
            });

            absoluteDiv.addEventListener('mousemove', (event) => {
                if (event.buttons > 0) {
                    this.treeOffset.x += event.movementX;
                    this.treeOffset.y += event.movementY;

                    this.movementOccurred = true;

                    this.updatePanningTransform();
                }
            });

            absoluteDiv.addEventListener('mousedown', (event) => {
                if (event.buttons > 0) {
                    this.movementOccurred = false;
                }
            });

            container.appendChild(absoluteDiv);

        },
        open(itemUID) {
            this.visible = true;
           
            escapeKeyHandlersStack.push(() => { this.close(); });
            sendMetric("Modal_CraftingTreeOpen", "");
            this.lastItemUID = itemUID;

            this.selectedItem = undefined;

            this.refresh();
        },
        refresh(callbackToDo) {

            if(this.lastItemUID==undefined || this.lastItemUID=="") return;

            this.loading = true;
            plugin.get().GetCraftingTreeForItem(this.lastItemUID, this.hideCompleted, (success, data) => {
                if (success) {
                    this.loading = false;

                    this.data = JSON.parse(data);

                    this.$nextTick(() => {
                        this.renderTree();
                        this.resetPanningTransform();
                        this.updatePanningTransform();
                    });
                } else {
                    showErrorToast(data);
                }
            });
        },
        dropClasses(drop) {
            return {
                required: drop.ownedAmount > 0,
                isNormalDrop: drop.dropType == 0,
                isMarketDrop: drop.dropType == 2,
            }
        },
        deselectFocusItem() {
            this.selectedItem = undefined;
        },
        close() {
            this.visible = false;
            document.querySelector(".craftingTreeContainer").innerHTML = ""
        },
        relicClicked(uid) {
            if (uid == undefined) return;
            relicDetailsApp.open(uid);
        },
        openBrowserLink(link) {
            overwolf.utils.openUrlInDefaultBrowser(link);
        },
        showSI(number) {
            if (number >= 1000000) return (number / 1000000).toFixed(1) + "M";
            if (number >= 1000) return (number / 1000).toFixed(1) + "K";
            return number;
        },
        updatePanningTransform() {
            const absoluteDiv = document.getElementById("craftingTreePanning");
            const wrapper = absoluteDiv.firstChild;

            let scale = this.treeZoom;
            let offset = this.treeOffset;

            wrapper.style.transform = `translate(${offset.x}px, ${offset.y}px) scale(${scale})`;
        },
        resetPanningTransform() {
            this.treeZoom = 1;

            const wrapper = $(".craftingTreeWrapper")[0]
            let wrapperSize = { x: wrapper.clientWidth, y: wrapper.clientHeight };

            let container = $(".craftingTreeContainer")[0];
            let containerSize = { x: container.clientWidth, y: container.clientHeight };

            this.treeOffset = { x: (containerSize.x - wrapperSize.x) / 2, y: (containerSize.y - wrapperSize.y) / 2 };
        }, resourceSummaryClicked(resourceName) {
            //Recursively find the resource and select it

            let findResource = (item) => {
                if (item.data.name == resourceName) {
                    this.selectedItem = item;
                    return true;
                }

                if (item.children != undefined) {
                    for (var i = 0; i < item.children.length; i++) {
                        if (findResource(item.children[i])) return true;
                    }
                }

                return false;
            }

            findResource(this.data.treeData);
        }
    },
    computed: {

    }

}).mount("#modalCraftingTree");

window.addEventListener('resize', craftingTreeApp.refresh);