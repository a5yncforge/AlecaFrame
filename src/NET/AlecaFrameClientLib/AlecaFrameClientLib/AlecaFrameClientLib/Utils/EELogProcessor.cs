using AlecaFrameClientLib.Data;
using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AlecaFrameClientLib.Utils
{
    public static class EELogProcessor
    {
        private static bool gettingTradeMessageMultiline = false;

        private static bool waitingForTradeMessageConfirm = false;

        private static DateTime lastRelicDetected = DateTime.MinValue;

        private static DateTime lastRelicRecommendation = DateTime.MinValue;

        private static DateTime lastUIConsoleOpen = DateTime.MinValue;

        private static bool inRelicRecommendationAfterRelic = false;

        private static DateTime lastPurchaseItemUIShown = DateTime.MinValue;

        private static Action actionToRunIfUserChoosesYesOnMessagebox = null;

        private static DateTime actionToRunInUserYesValidUntil = DateTime.MinValue;

        private static int lastRivenHudVis = 0;

        private static AutoResetEvent instantProjectionEvent = new AutoResetEvent(initialState: false);

        public static bool isCurrentMissionRequiem = false;

        public static void ProcessLine(string message)
        {
            while (gettingTradeMessageMultiline)
            {
                if (message.Contains("[Info]") || message.Contains("[Error]") || message.Contains("[Warning]"))
                {
                    gettingTradeMessageMultiline = false;
                    StatsHandler.TradeLogsFinished();
                    waitingForTradeMessageConfirm = true;
                    continue;
                }
                StatsHandler.ReceivedTradeLogMessage(message);
                return;
            }
            if (message.Contains("\"activeMissionTag\" : \""))
            {
                isCurrentMissionRequiem = message.Substring(message.IndexOf("\"activeMissionTag\" : \"") + 21).Replace("\"", "").Replace(",", "")
                    .Contains("VoidT5");
                StaticData.Log(OverwolfWrapper.LogType.INFO, "New mission detected (client). Is requiem: " + isCurrentMissionRequiem);
            }
            else if (message.Contains("activeMissionTag="))
            {
                isCurrentMissionRequiem = message.Substring(message.IndexOf("activeMissionTag=") + 17).Replace("\"", "").Replace(",", "")
                    .Contains("VoidT5");
                StaticData.Log(OverwolfWrapper.LogType.INFO, "New mission detected (host). Is requiem: " + isCurrentMissionRequiem);
            }
            else if (message.Contains("Got rewards"))
            {
                if (!isCurrentMissionRequiem)
                {
                    if (StaticData.RelicOverlayEnabled)
                    {
                        StaticData.Log(OverwolfWrapper.LogType.WARN, "Starting to get rewards");
                        lastRelicDetected = DateTime.UtcNow;
                        instantProjectionEvent.Reset();
                        Task.Run(delegate
                        {
                            instantProjectionEvent.WaitOne(TimeSpan.FromSeconds(0.65));
                            OCRHelper.DoScreenshotRequestWork(Misc.IsWarframeUIScaleLegacy());
                        });
                    }
                    else
                    {
                        StaticData.Log(OverwolfWrapper.LogType.WARN, "Relic overlay is disabled. Not showing it!");
                    }
                    StatsHandler.RelicJustOpened_BG();
                }
                else
                {
                    StaticData.Log(OverwolfWrapper.LogType.WARN, "A relic was opened but last mission joined was of Requiem type. Not showing overlay");
                }
            }
            else if (message.Contains("ThemedProjectionManager.lua: LoadingCompleteEnd"))
            {
                if (StaticData.RelicRecommendationEnabled)
                {
                    if ((DateTime.UtcNow - lastUIConsoleOpen).TotalMilliseconds > 1000.0)
                    {
                        if ((DateTime.UtcNow - lastRelicDetected).TotalSeconds < 23.0)
                        {
                            inRelicRecommendationAfterRelic = true;
                        }
                        else
                        {
                            inRelicRecommendationAfterRelic = false;
                        }
                        lastRelicRecommendation = DateTime.UtcNow;
                        Task.Run(delegate
                        {
                            OverlaysHandler.OpenRelicRecommendationHandler(debugMode: false, null, inRelicRecommendationAfterRelic);
                        });
                    }
                    else
                    {
                        StaticData.Log(OverwolfWrapper.LogType.INFO, "Not opening relic recommendation because the console was open quite recently");
                    }
                }
                else
                {
                    StaticData.Log(OverwolfWrapper.LogType.WARN, "Relic recommendation is disabled. Not showing it!");
                }
            }
            else if (message.Contains("UIConsoleTrigger::Open()"))
            {
                lastUIConsoleOpen = DateTime.UtcNow;
            }
            else if (message.Contains("InitMapping for all devices with bindings"))
            {
                if (inRelicRecommendationAfterRelic && (DateTime.UtcNow - lastRelicRecommendation).TotalSeconds < 3.5)
                {
                    StaticData.Log(OverwolfWrapper.LogType.WARN, "Skipped relic close because it was too close to the recommendation start!");
                    return;
                }
                if ((DateTime.UtcNow - lastRelicRecommendation).TotalMilliseconds < 500.0)
                {
                    StaticData.Log(OverwolfWrapper.LogType.WARN, "Skipped relic close because it was too close to the recommendation start! (2)");
                    return;
                }
                Task.Run(delegate
                {
                    OverlaysHandler.CloseRelicRecommendationHandler();
                });
            }
            else if (message.Contains("ProjectionRewardChoice.lua: Missing icon data!"))
            {
                Thread.Sleep(650);
                instantProjectionEvent.Set();
            }
            else if (message.Contains("ChatRedux::AddTab: Adding tab with channel name"))
            {
                Task.Run(delegate
                {
                    try
                    {
                        string text = message.Substring(message.IndexOf("channel name: ") + 14);
                        text = text.Substring(0, text.IndexOf(" to index"));
                        if (text.StartsWith("F"))
                        {
                            if (Encoding.UTF8.GetByteCount(text.Substring(text.Length - 1)) != 1)
                            {
                                text = text.Substring(0, text.Length - 1);
                            }
                            OCRHelper.SendNewInGameConversationNotification(text.Substring(1));
                        }
                    }
                    catch (Exception ex)
                    {
                        StaticData.Log(OverwolfWrapper.LogType.ERROR, "Failed to send notification from chat message: " + message + ". Error: " + ex.Message);
                    }
                });
            }
            else if (message.Contains("[Info]: Dialog.lua: Dialog::CreateOkCancel(description=") && StatsHandler.IsBeginninigOfTradeLog(message))
            {
                StatsHandler.StartTradeLog(message);
                if (message.Contains(", leftItem=/Menu/Confirm_Item_Ok, rightItem=/Menu/Confirm_Item_Cancel)"))
                {
                    StatsHandler.TradeLogsFinished();
                    waitingForTradeMessageConfirm = true;
                }
                else
                {
                    gettingTradeMessageMultiline = true;
                }
            }
            else if (waitingForTradeMessageConfirm && message.Contains("The trade was successful!, leftItem="))
            {
                waitingForTradeMessageConfirm = false;
                StatsHandler.TradeAccepted();
            }
            else if (message.Contains("ThemedDetailedPurchaseDialog.lua: DBG: HudVis "))
            {
                int num = int.Parse(message.Substring(message.IndexOf("ThemedDetailedPurchaseDialog.lua: DBG: HudVis ") + 46));
                if (num < lastRivenHudVis)
                {
                    Task.Run(delegate
                    {
                        RivenOverlays.ChatRivenClosedDetected();
                    });
                }
                else
                {
                    lastPurchaseItemUIShown = DateTime.UtcNow;
                }
                lastRivenHudVis = num;
            }
            else if (DateTime.UtcNow - lastPurchaseItemUIShown < TimeSpan.FromSeconds(2.0) && message.Contains("ThemedDetailedPurchaseDialog.lua: PopulateInfo->/Lotus/StoreItems/Upgrades/Mods/Randomized/"))
            {
                Task.Run(delegate
                {
                    try
                    {
                        RivenOverlays.ChatRivenOpenedDetected();
                    }
                    catch (Exception ex)
                    {
                        StaticData.Log(OverwolfWrapper.LogType.ERROR, "Failed to show chat riven overlay. Error: " + ex);
                    }
                });
            }
            else if (message.Contains("OmegaRerollSelection.lua: Diorama setup"))
            {
                Task.Run(delegate
                {
                    RivenOverlays.RivenSelectedForRerollDetected();
                });
            }
            else if (RivenOverlays.isRerollUIOpen && (message.Contains("ytes of recycled effects") || message.Contains("NpcManager::ClearAgents() ReadyToCreateAgents = false")))
            {
                Task.Run(delegate
                {
                    RivenOverlays.RivenRerollClosedDetected();
                });
            }
            else if (message.Contains("Dialog::CreateOkCancel(description=Are you sure you want to cycle"))
            {
                actionToRunIfUserChoosesYesOnMessagebox = RivenOverlays.RivenRerollDetected;
                actionToRunInUserYesValidUntil = DateTime.UtcNow.AddSeconds(300.0);
            }
            else if (message.Contains("Dialog::CreateOkCancel(description=Cycle Riven into current selection?, "))
            {
                actionToRunIfUserChoosesYesOnMessagebox = RivenOverlays.RivenRerollCycleComplete;
                actionToRunInUserYesValidUntil = DateTime.UtcNow.AddSeconds(300.0);
            }
            else if (message.Contains("Dialog.lua: Dialog::SendResult(4)") && actionToRunIfUserChoosesYesOnMessagebox != null && DateTime.UtcNow < actionToRunInUserYesValidUntil)
            {
                Task.Run(delegate
                {
                    actionToRunIfUserChoosesYesOnMessagebox();
                });
            }
        }
    }
}
