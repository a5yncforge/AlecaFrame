using AlecaFrameClientLib.Data;
using AlecaFrameClientLib.Data.Types;
using AlecaFrameClientLib.Utils;
using AlecaFramePublicLib;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AlecaFrameClientLib
{
    public static class CraftingTreeHelper
    {
        public class CraftingTreeData
        {
            public class CraftingTreeDataSummary
            {
                public List<CraftingTreeDataTreeItem> blueprintsNeeded;

                public List<CraftingTreeDataTreeItem> resourcesNeeded;

                public string credits = "";

                public string time = "";

                public string shortestTime = "";
            }

            public class CraftingTreeDataTreeItem
            {
                public class CraftingTreeDataTreeItemData
                {
                    public string uniqueName;

                    public string name;

                    public string picture;

                    public string wikiLink;
                }

                public CraftingTreeDataTreeItemData data;

                public int credits;

                public List<CraftingTreeDataTreeItem> children = new List<CraftingTreeDataTreeItem>();

                public bool dim;

                public bool gotEnough;

                public bool shouldLinkToAnotherTree;

                [NonSerialized]
                public TimeSpan timeRAW = TimeSpan.Zero;

                public string time = "";

                public int quantityOwned;

                public int amountNeeded;

                public bool craftable;

                public int amountMissing;

                public int recipeNumOut;

                public int amountToCraft;

                public List<FoundryDetailsComponentDrop> drops;

                public OverwolfWrapper.ItemPriceSmallResponse wfmarket;
            }

            public CraftingTreeDataSummary craftingTreeDataSummary;

            public CraftingTreeDataTreeItem treeData;

            public bool found;

            public List<CraftingTreeDataTreeItem> parentWeapons;
        }

        public static CraftingTreeData GetCraftingTreeForItem(string uniqueID, bool hideCompleted, bool requestPrices = true)
        {
            ExtendedCraftingRemoteDataItem orDefault = StaticData.dataHandler.craftingData.craftsByUUID.GetOrDefault(uniqueID);
            if (orDefault == null)
            {
                return new CraftingTreeData
                {
                    found = false
                };
            }
            CraftingTreeData craftingTreeData = new CraftingTreeData();
            craftingTreeData.found = true;
            craftingTreeData.treeData = CreateCraftingTreeComponent(orDefault.ToComponentData(), isParent: true, requestPrices);
            craftingTreeData.craftingTreeDataSummary = new CraftingTreeData.CraftingTreeDataSummary();
            List<CraftingTreeData.CraftingTreeDataTreeItem> source = FlattenComponents(craftingTreeData.treeData);
            AssignItemStatsRecursively(materialsLeft: (from p in source
                                                       group p by p.data.uniqueName).ToDictionary((IGrouping<string, CraftingTreeData.CraftingTreeDataTreeItem> x) => x.Key, (IGrouping<string, CraftingTreeData.CraftingTreeDataTreeItem> x) => x.First().quantityOwned), treeData: craftingTreeData.treeData, hideCompleted: hideCompleted);
            int largestBranchCraftTime;
            Dictionary<string, int> materialsNeeded = CalculateMissingResourcesRecursively(craftingTreeData.treeData, out largestBranchCraftTime);
            craftingTreeData.craftingTreeDataSummary.shortestTime = TimeSpan.FromSeconds(largestBranchCraftTime).ToTimeString();
            craftingTreeData.craftingTreeDataSummary.credits = materialsNeeded.GetOrDefault("credits").GetSIRepresentation();
            craftingTreeData.craftingTreeDataSummary.time = TimeSpan.FromSeconds(materialsNeeded.GetOrDefault("time")).ToTimeString();
            Dictionary<string, CraftingTreeData.CraftingTreeDataTreeItem> source2 = (from p in source
                                                                                     where p.data.uniqueName.Contains("Blueprint")
                                                                                     group p by p.data.uniqueName).ToDictionary((IGrouping<string, CraftingTreeData.CraftingTreeDataTreeItem> p) => p.Key, (IGrouping<string, CraftingTreeData.CraftingTreeDataTreeItem> p) => p.First());
            Dictionary<string, CraftingTreeData.CraftingTreeDataTreeItem> source3 = (from p in source
                                                                                     where !p.data.uniqueName.Contains("Blueprint")
                                                                                     group p by p.data.uniqueName).ToDictionary((IGrouping<string, CraftingTreeData.CraftingTreeDataTreeItem> p) => p.Key, (IGrouping<string, CraftingTreeData.CraftingTreeDataTreeItem> p) => p.First());
            FillParents(craftingTreeData);
            craftingTreeData.treeData.shouldLinkToAnotherTree = false;
            craftingTreeData.craftingTreeDataSummary.blueprintsNeeded = (from p in source2.Select(delegate (KeyValuePair<string, CraftingTreeData.CraftingTreeDataTreeItem> x)
                {
                    CraftingTreeData.CraftingTreeDataTreeItem craftingTreeDataTreeItem = new CraftingTreeData.CraftingTreeDataTreeItem();
                    craftingTreeDataTreeItem.data = new CraftingTreeData.CraftingTreeDataTreeItem.CraftingTreeDataTreeItemData();
                    CraftingTreeData.CraftingTreeDataTreeItem.CraftingTreeDataTreeItemData data = x.Value.data;
                    craftingTreeDataTreeItem.data.uniqueName = data.uniqueName;
                    craftingTreeDataTreeItem.data.name = data.name;
                    craftingTreeDataTreeItem.data.picture = data.picture;
                    craftingTreeDataTreeItem.amountNeeded = materialsNeeded.GetOrDefault(data.uniqueName);
                    return craftingTreeDataTreeItem;
                })
                                                                         where p.amountNeeded > 0
                                                                         orderby p.data.name
                                                                         select p).ToList();
            craftingTreeData.craftingTreeDataSummary.resourcesNeeded = (from p in source3.Select(delegate (KeyValuePair<string, CraftingTreeData.CraftingTreeDataTreeItem> x)
                {
                    CraftingTreeData.CraftingTreeDataTreeItem craftingTreeDataTreeItem = new CraftingTreeData.CraftingTreeDataTreeItem();
                    craftingTreeDataTreeItem.data = new CraftingTreeData.CraftingTreeDataTreeItem.CraftingTreeDataTreeItemData();
                    CraftingTreeData.CraftingTreeDataTreeItem.CraftingTreeDataTreeItemData data = x.Value.data;
                    craftingTreeDataTreeItem.data.uniqueName = data.uniqueName;
                    craftingTreeDataTreeItem.data.name = data.name;
                    craftingTreeDataTreeItem.data.picture = data.picture;
                    craftingTreeDataTreeItem.amountNeeded = materialsNeeded.GetOrDefault(data.uniqueName);
                    return craftingTreeDataTreeItem;
                })
                                                                        where p.amountNeeded > 0
                                                                        orderby p.data.name
                                                                        select p).ToList();
            if (requestPrices)
            {
                PriceHelper.Flush(TimeSpan.FromSeconds(7.0));
            }
            return craftingTreeData;
        }

        private static void FillParents(CraftingTreeData toReturn)
        {
            toReturn.parentWeapons = new List<CraftingTreeData.CraftingTreeDataTreeItem>();
            BigItem bigItemRefernceOrNull = Misc.GetBigItemRefernceOrNull(toReturn.treeData.data.uniqueName, onlyFoundryItems: true);
            if (bigItemRefernceOrNull == null)
            {
                return;
            }
            foreach (BigItem item in bigItemRefernceOrNull.isPartOf)
            {
                CraftingTreeData.CraftingTreeDataTreeItem craftingTreeDataTreeItem = new CraftingTreeData.CraftingTreeDataTreeItem();
                craftingTreeDataTreeItem.data = new CraftingTreeData.CraftingTreeDataTreeItem.CraftingTreeDataTreeItemData();
                craftingTreeDataTreeItem.data.uniqueName = item.uniqueName;
                craftingTreeDataTreeItem.data.name = item.name;
                craftingTreeDataTreeItem.data.picture = Misc.GetFullImagePath(item.imageName);
                craftingTreeDataTreeItem.shouldLinkToAnotherTree = true;
                craftingTreeDataTreeItem.quantityOwned = ((bigItemRefernceOrNull.IsOwned() || bigItemRefernceOrNull.IsFullyMastered()) ? 1 : 0);
                toReturn.parentWeapons.Add(craftingTreeDataTreeItem);
            }
        }

        private static Dictionary<string, int> CalculateMissingResourcesRecursively(CraftingTreeData.CraftingTreeDataTreeItem treeData, out int largestBranchCraftTime)
        {
            Dictionary<string, int> dictionary = new Dictionary<string, int>();
            int num = treeData.amountToCraft + treeData.amountMissing / treeData.recipeNumOut;
            if (treeData.children.Count == 0)
            {
                if (!treeData.dim)
                {
                    dictionary.Add(treeData.data.uniqueName, num);
                }
                largestBranchCraftTime = 0;
            }
            else
            {
                int num2 = (dictionary["time"] = (int)treeData.timeRAW.TotalSeconds * num);
                dictionary["credits"] = treeData.credits * num;
                List<(string uniqueID, int time)> list = new List<(string uniqueID, int time)>();
                foreach (CraftingTreeData.CraftingTreeDataTreeItem child in treeData.children)
                {
                    int largestBranchCraftTime2;
                    Dictionary<string, int> dictionary2 = CalculateMissingResourcesRecursively(child, out largestBranchCraftTime2);
                    list.Add((child.data.uniqueName, largestBranchCraftTime2));
                    foreach (KeyValuePair<string, int> item in dictionary2)
                    {
                        if (!item.Key.Contains("Blueprint"))
                        {
                            _ = item.Value;
                        }
                        else
                        {
                            _ = item.Value;
                        }
                        if (dictionary.ContainsKey(item.Key))
                        {
                            dictionary[item.Key] += item.Value;
                        }
                        else
                        {
                            dictionary.Add(item.Key, item.Value);
                        }
                    }
                }
                largestBranchCraftTime = (from p in list
                                          group p by p.uniqueID into p
                                          select p.Sum(((string uniqueID, int time) k) => k.time)).Max() + num2;
            }
            return dictionary;
        }

        private static void AssignItemStatsRecursively(CraftingTreeData.CraftingTreeDataTreeItem treeData, Dictionary<string, int> materialsLeft, bool hideCompleted)
        {
            if (materialsLeft.GetOrDefault(treeData.data.uniqueName) >= treeData.amountNeeded)
            {
                treeData.gotEnough = true;
                materialsLeft[treeData.data.uniqueName] -= treeData.amountNeeded;
                if (hideCompleted)
                {
                    treeData.children.Clear();
                    return;
                }
                ApplyFunctionRecursively(treeData, delegate (CraftingTreeData.CraftingTreeDataTreeItem x)
                {
                    x.dim = true;
                    if (x.amountNeeded > x.quantityOwned)
                    {
                        x.amountMissing = x.amountNeeded - x.quantityOwned;
                    }
                });
                return;
            }
            treeData.amountMissing = treeData.amountNeeded - materialsLeft.GetOrDefault(treeData.data.uniqueName);
            materialsLeft[treeData.data.uniqueName] = 0;
            treeData.gotEnough = false;
            if (treeData.children.Count > 0)
            {
                while (treeData.amountMissing > 0)
                {
                    foreach (CraftingTreeData.CraftingTreeDataTreeItem child in treeData.children)
                    {
                        AssignItemStatsRecursively(child, materialsLeft, hideCompleted);
                    }
                    if (!treeData.children.All((CraftingTreeData.CraftingTreeDataTreeItem p) => p.gotEnough || p.craftable))
                    {
                        break;
                    }
                    treeData.amountMissing -= treeData.recipeNumOut;
                    treeData.amountToCraft += treeData.recipeNumOut;
                }
            }
            treeData.craftable = treeData.amountMissing == 0;
        }

        private static void ApplyFunctionRecursively(CraftingTreeData.CraftingTreeDataTreeItem treeData, Action<CraftingTreeData.CraftingTreeDataTreeItem> value)
        {
            value(treeData);
            foreach (CraftingTreeData.CraftingTreeDataTreeItem child in treeData.children)
            {
                ApplyFunctionRecursively(child, value);
            }
        }

        public static List<CraftingTreeData.CraftingTreeDataTreeItem> FlattenComponents(CraftingTreeData.CraftingTreeDataTreeItem treeData)
        {
            List<CraftingTreeData.CraftingTreeDataTreeItem> list = new List<CraftingTreeData.CraftingTreeDataTreeItem>();
            list.Add(treeData);
            foreach (CraftingTreeData.CraftingTreeDataTreeItem child in treeData.children)
            {
                list.AddRange(FlattenComponents(child));
            }
            return list;
        }

        private static CraftingTreeData.CraftingTreeDataTreeItem CreateCraftingTreeComponent(ExtendedCraftingRemoteDataItemComponent item, bool isParent, bool isDim = false, bool requestPrices = true)
        {
            CraftingTreeData.CraftingTreeDataTreeItem craftingTreeDataTreeItem = new CraftingTreeData.CraftingTreeDataTreeItem();
            craftingTreeDataTreeItem.dim = isDim;
            BasicRemoteDataItemData orDefault = StaticData.dataHandler.basicRemoteData.items.GetOrDefault(item.uniqueName);
            bool flag = false;
            if (orDefault == null)
            {
                orDefault = StaticData.dataHandler.basicRemoteData.items.GetOrDefault(item.parentComponent?.uniqueName);
                if (orDefault != null)
                {
                    flag = true;
                }
            }
            craftingTreeDataTreeItem.data = new CraftingTreeData.CraftingTreeDataTreeItem.CraftingTreeDataTreeItemData();
            craftingTreeDataTreeItem.data.uniqueName = item.uniqueName;
            ItemComponent value = item.GetItemComponentComponentProblemAware();
            craftingTreeDataTreeItem.shouldLinkToAnotherTree = Misc.GetBigItemRefernceOrNull(craftingTreeDataTreeItem.data.uniqueName, onlyFoundryItems: true) != null;
            if (orDefault == null)
            {
                if (value != null)
                {
                    craftingTreeDataTreeItem.data.name = value.GetRealExternalName();
                    craftingTreeDataTreeItem.data.picture = Misc.GetFullImagePath(value.imageName);
                }
                else if (StaticData.dataHandler.warframeParts.TryGetValue(craftingTreeDataTreeItem.data.uniqueName.Replace("Blueprint", "Component"), out value) || StaticData.dataHandler.weaponParts.TryGetValue(craftingTreeDataTreeItem.data.uniqueName.Replace("Blueprint", "Component"), out value))
                {
                    craftingTreeDataTreeItem.data.name = value.GetRealExternalName();
                    craftingTreeDataTreeItem.data.picture = Misc.GetFullImagePath(value.imageName);
                }
                else
                {
                    craftingTreeDataTreeItem.data.name = "???";
                    craftingTreeDataTreeItem.data.picture = "";
                }
            }
            else
            {
                craftingTreeDataTreeItem.data.name = orDefault.name;
                if (orDefault.pic == "FROM_PARENT")
                {
                    string text = item.parentComponent?.GetItemComponentComponentProblemAware()?.imageName ?? item.parentComponent?.basicInfo.pic ?? item.parentItem?.bigItem?.imageName;
                    if (text == "FROM_PARENT")
                    {
                        text = item.parentComponent?.parentComponent?.basicInfo?.pic ?? item.parentComponent?.parentItem?.bigItem?.imageName;
                    }
                    craftingTreeDataTreeItem.data.picture = Misc.GetFullImagePath(text);
                }
                else
                {
                    craftingTreeDataTreeItem.data.picture = Misc.GetFullImagePath(orDefault.pic);
                }
                if (craftingTreeDataTreeItem.data.uniqueName.EndsWith("Component") && craftingTreeDataTreeItem.data.name.Contains("Blueprint"))
                {
                    craftingTreeDataTreeItem.data.name = craftingTreeDataTreeItem.data.name.Replace("Blueprint", "");
                }
                if (craftingTreeDataTreeItem.data.name == "Blueprint" && item.parentComponent?.basicInfo != null)
                {
                    craftingTreeDataTreeItem.data.name = item.parentComponent.basicInfo?.name + " Blueprint";
                }
                if (flag && !craftingTreeDataTreeItem.data.name.EndsWith("Blueprint") && craftingTreeDataTreeItem.data.uniqueName.EndsWith("Blueprint"))
                {
                    craftingTreeDataTreeItem.data.name += " Blueprint";
                }
            }
            BigItem bigItemRefernceOrNull = Misc.GetBigItemRefernceOrNull(item.uniqueName);
            craftingTreeDataTreeItem.data.wikiLink = bigItemRefernceOrNull?.wikiaUrl?.Replace("https://warframe.fandom.com/wiki/", "https://wiki.warframe.com/w/");
            if (craftingTreeDataTreeItem.data.wikiLink == null)
            {
                craftingTreeDataTreeItem.data.wikiLink = Misc.GetWikiaURL(item.basicInfo?.wiki);
            }
            if (isParent)
            {
                craftingTreeDataTreeItem.quantityOwned = 0;
            }
            else
            {
                craftingTreeDataTreeItem.quantityOwned = FindCraftingTreeAmountOwned(craftingTreeDataTreeItem.data.uniqueName);
            }
            craftingTreeDataTreeItem.amountNeeded = item.neededCount;
            craftingTreeDataTreeItem.credits = item.credits;
            craftingTreeDataTreeItem.recipeNumOut = ((item.num == 0) ? 1 : item.num);
            List<Drop> list = new List<Drop>();
            if (value?.drops != null)
            {
                list.AddRange(value?.drops);
            }
            else
            {
                list.AddRange(bigItemRefernceOrNull?.drops ?? new Drop[0]);
            }
            if (StaticData.dataHandler.misc.ContainsKey(craftingTreeDataTreeItem.data.uniqueName))
            {
                DataMisc dataMisc = StaticData.dataHandler.misc[craftingTreeDataTreeItem.data.uniqueName];
                if (dataMisc.drops != null)
                {
                    list.AddRange(dataMisc.drops);
                }
            }
            else if (StaticData.dataHandler.resources.ContainsKey(craftingTreeDataTreeItem.data.uniqueName))
            {
                DataResource dataResource = StaticData.dataHandler.resources[craftingTreeDataTreeItem.data.uniqueName];
                if (dataResource.drops != null)
                {
                    list.AddRange(dataResource.drops);
                }
            }
            craftingTreeDataTreeItem.drops = FoundryDetailsComponentsItem.CreateParsedDrops(list.ToArray());
            craftingTreeDataTreeItem.time = (craftingTreeDataTreeItem.timeRAW = TimeSpan.FromSeconds(item.time)).ToTimeString();
            foreach (ExtendedCraftingRemoteDataItemComponent component in item.components)
            {
                craftingTreeDataTreeItem.children.Add(CreateCraftingTreeComponent(component, isParent: false, isDim, requestPrices));
            }
            if (requestPrices)
            {
                craftingTreeDataTreeItem.wfmarket = PriceHelper.GetLazyItemPrice(craftingTreeDataTreeItem.data.name);
            }
            return craftingTreeDataTreeItem;
        }

        private static int FindCraftingTreeAmountOwned(string uniqueName)
        {
            if (StaticData.dataHandler?.warframeRootObject == null)
            {
                return 0;
            }
            return 0 + (StaticData.dataHandler.warframeRootObject.LongGuns?.Count((Longgun x) => x.ItemType == uniqueName) ?? 0) + (StaticData.dataHandler.warframeRootObject.Melee?.Count((Melee x) => x.ItemType == uniqueName) ?? 0) + (StaticData.dataHandler.warframeRootObject.Pistols?.Count((Pistol x) => x.ItemType == uniqueName) ?? 0) + (StaticData.dataHandler.warframeRootObject.Sentinels?.Count((Sentinel x) => x.ItemType == uniqueName) ?? 0) + (StaticData.dataHandler.warframeRootObject.MechSuits?.Count((Suit x) => x.ItemType == uniqueName) ?? 0) + (StaticData.dataHandler.warframeRootObject.CrewShips?.Count((Crewship x) => x.ItemType == uniqueName) ?? 0) + (StaticData.dataHandler.warframeRootObject.Suits?.Count((Suit x) => x.ItemType == uniqueName) ?? 0) + (StaticData.dataHandler.warframeRootObject.Hoverboards?.Count((Hoverboard x) => x.ItemType == uniqueName) ?? 0) + (StaticData.dataHandler.warframeRootObject.MoaPets?.Count((Moapet x) => x.ItemType == uniqueName) ?? 0) + (StaticData.dataHandler.warframeRootObject.KubrowPets?.Count((Kubrowpet x) => x.ItemType == uniqueName) ?? 0) + (StaticData.dataHandler.warframeRootObject.SpaceSuits?.Count((Spacesuit x) => x.ItemType == uniqueName) ?? 0) + (StaticData.dataHandler.warframeRootObject.SpaceMelee?.Count((Spacemelee x) => x.ItemType == uniqueName) ?? 0) + (StaticData.dataHandler.warframeRootObject.SpaceGuns?.Count((Spacegun x) => x.ItemType == uniqueName) ?? 0) + (StaticData.dataHandler.warframeRootObject.SpecialItems?.Count((Specialitem x) => x.ItemType == uniqueName) ?? 0) + StaticData.dataHandler.warframeRootObject.MiscItemsLookup[uniqueName].Sum((Miscitem x) => x.ItemCount) + (StaticData.dataHandler.warframeRootObject.Recipes?.Where((Miscitem x) => x.ItemType == uniqueName).Sum((Miscitem x) => (x.ItemCount == 0) ? 1 : x.ItemCount) ?? 0);
        }
    }
}
