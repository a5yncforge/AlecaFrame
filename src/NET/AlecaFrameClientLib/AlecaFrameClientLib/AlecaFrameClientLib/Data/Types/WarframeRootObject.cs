using System.Linq;

namespace AlecaFrameClientLib.Data.Types
{
	public class WarframeRootObject
	{
		public string InventoryJSON = "";

		public Booster[] Boosters = new Booster[0];

		public Challengeprogress[] ChallengeProgress = new Challengeprogress[0];

		public Miscitem[] RawUpgrades = new Miscitem[0];

		public Suit[] Suits = new Suit[0];

		public Longgun[] LongGuns = new Longgun[0];

		public Pistol[] Pistols = new Pistol[0];

		public Melee[] Melee = new Melee[0];

		public Ship[] Ships = new Ship[0];

		public Questkey[] QuestKeys = new Questkey[0];

		public Miscitem[] FlavourItems = new Miscitem[0];

		public Scoop[] Scoops = new Scoop[0];

		public Miscitem[] MiscItems = new Miscitem[0];

		public Xpinfo[] XPInfo = new Xpinfo[0];

		public WarframeWorldStateDateMission[] Missions = new WarframeWorldStateDateMission[0];

		public Miscitem[] Recipes = new Miscitem[0];

		public Pendingrecipe[] PendingRecipes = new Pendingrecipe[0];

		public Weaponskin[] WeaponSkins = new Weaponskin[0];

		public Taunthistory[] TauntHistory = new Taunthistory[0];

		public Challengeinstancestate[] ChallengeInstanceStates = new Challengeinstancestate[0];

		public Upgrade[] Upgrades = new Upgrade[0];

		public Sentinel[] Sentinels = new Sentinel[0];

		public Kubrowpetegg[] KubrowPetEggs = new Kubrowpetegg[0];

		public Consumable[] Consumables = new Consumable[0];

		public Miscitem[] LevelKeys = new Miscitem[0];

		public Spacesuit[] SpaceSuits = new Spacesuit[0];

		public Spacemelee[] SpaceMelee = new Spacemelee[0];

		public Spacegun[] SpaceGuns = new Spacegun[0];

		public Lorefragmentscan[] LoreFragmentScans = new Lorefragmentscan[0];

		public Periodicmissioncompletion[] PeriodicMissionCompletions = new Periodicmissioncompletion[0];

		public Kubrowpet[] KubrowPets = new Kubrowpet[0];

		public Affiliation[] Affiliations = new Affiliation[0];

		public Pendingtrade[] PendingTrades = new Pendingtrade[0];

		public Spectreloadout[] SpectreLoadouts = new Spectreloadout[0];

		public Emailitem[] EmailItems = new Emailitem[0];

		public Personalgoalprogress[] PersonalGoalProgress = new Personalgoalprogress[0];

		public InGamePlexus[] CrewShipHarnesses = new InGamePlexus[0];

		public Lastsortiereward[] LastSortieReward = new Lastsortiereward[0];

		public Shipdecoration[] ShipDecorations = new Shipdecoration[0];

		public Drone[] Drones = new Drone[0];

		public KubrowPetPrint[] KubrowPetPrints = new KubrowPetPrint[0];

		public Completedjob[] CompletedJobs = new Completedjob[0];

		public Discoveredmarker[] DiscoveredMarkers = new Discoveredmarker[0];

		public Focusupgrade[] FocusUpgrades = new Focusupgrade[0];

		public Operatoramp[] OperatorAmps = new Operatoramp[0];

		public Stepsequencer[] StepSequencers = new Stepsequencer[0];

		public Specialitem[] SpecialItems = new Specialitem[0];

		public Operatorloadout[] OperatorLoadOuts = new Operatorloadout[0];

		public Seasonchallengehistory[] SeasonChallengeHistory = new Seasonchallengehistory[0];

		public Hoverboard[] Hoverboards = new Hoverboard[0];

		public Moapet[] MoaPets = new Moapet[0];

		public Custommarker[] CustomMarkers = new Custommarker[0];

		public Completedjobchain[] CompletedJobChains = new Completedjobchain[0];

		public Invasionchainprogress[] InvasionChainProgress = new Invasionchainprogress[0];

		public Dataknife1[] DataKnives = new Dataknife1[0];

		public Nemesishistory[] NemesisHistory = new Nemesishistory[0];

		public Personaltechproject[] PersonalTechProjects = new Personaltechproject[0];

		public Crewship[] CrewShips = new Crewship[0];

		public Crewshipammo[] CrewShipAmmo = new Crewshipammo[0];

		public Crewshipsalvagedweaponskin[] CrewShipSalvagedWeaponSkins = new Crewshipsalvagedweaponskin[0];

		public Crewshipsalvagedweapon[] CrewShipSalvagedWeapons = new Crewshipsalvagedweapon[0];

		public Crewshipweaponskin[] CrewShipWeaponSkins = new Crewshipweaponskin[0];

		public Crewshipweapon[] CrewShipWeapons = new Crewshipweapon[0];

		public Librarypersonalprogress[] LibraryPersonalProgress = new Librarypersonalprogress[0];

		public Collectiblesery[] CollectibleSeries = new Collectiblesery[0];

		public int SubscribedToEmails { get; set; }

		public Created Created { get; set; }

		public long RewardSeed { get; set; }

		public long RegularCredits { get; set; }

		public int PremiumCredits { get; set; }

		public int PremiumCreditsFree { get; set; }

		public int FusionPoints { get; set; }

		public Suitbin SuitBin { get; set; }

		public Weaponbin WeaponBin { get; set; }

		public Sentinelbin SentinelBin { get; set; }

		public Spacesuitbin SpaceSuitBin { get; set; }

		public Spaceweaponbin SpaceWeaponBin { get; set; }

		public Pvpbonusloadoutbin PvpBonusLoadoutBin { get; set; }

		public Pvebonusloadoutbin PveBonusLoadoutBin { get; set; }

		public Randommodbin RandomModBin { get; set; }

		public int Version { get; set; }

		public int TradesRemaining { get; set; }

		public int DailyAffiliation { get; set; }

		public int DailyAffiliationPvp { get; set; }

		public int DailyAffiliationLibrary { get; set; }

		public int DailyFocus { get; set; }

		public int GiftsRemaining { get; set; }

		public int HandlerPoints { get; set; }

		public int ChallengesFixVersion { get; set; }

		public bool ReceivedStartingGear { get; set; }

		public int TrainingRetriesLeft { get; set; }

		public ILookup<string, Miscitem> MiscItemsLookup { get; set; }

		public Loadoutpresets LoadOutPresets { get; set; }

		public int RandomUpgradesIdentified { get; set; }

		public string LastRegionPlayed { get; set; }

		public Webflags WebFlags { get; set; }

		public Trainingdate TrainingDate { get; set; }

		public int PlayerLevel { get; set; }

		public string[] CompletedAlerts { get; set; }

		public Sentientspawnchanceboosters SentientSpawnChanceBoosters { get; set; }

		public string[] DeathMarks { get; set; }

		public Sentinelweapon[] SentinelWeapons { get; set; }

		public string ActiveDojoColorResearch { get; set; }

		public bool ArchwingEnabled { get; set; }

		public string[] EquippedGear { get; set; }

		public object[] QualifyingInvasions { get; set; }

		public Miscitem[] FusionTreasures { get; set; }

		public int[] FactionScores { get; set; }

		public object[] PendingSpectreLoadouts { get; set; }

		public string[] CompletedSyndicates { get; set; }

		public Focusxp FocusXP { get; set; }

		public Alignment Alignment { get; set; }

		public string[] CompletedSorties { get; set; }

		public string ActiveAvatarImageType { get; set; }

		public string[] Wishlist { get; set; }

		public string[] EquippedEmotes { get; set; }

		public Operatorampbin OperatorAmpBin { get; set; }

		public int DailyAffiliationCetus { get; set; }

		public int DailyAffiliationQuills { get; set; }

		public string FocusAbility { get; set; }

		public bool HasContributedToDojo { get; set; }

		public int FocusCapacity { get; set; }

		public Alignmentreplay AlignmentReplay { get; set; }

		public int DailyAffiliationSolaris { get; set; }

		public int SubscribedToEmailsPersonalized { get; set; }

		public string ThemeStyle { get; set; }

		public int BountyScore { get; set; }

		public string[] LoginMilestoneRewards { get; set; }

		public int DailyAffiliationVentkids { get; set; }

		public int DailyAffiliationVox { get; set; }

		public string[] NodeIntrosCompleted { get; set; }

		public object[] RecentVendorPurchases { get; set; }

		public Guildid GuildId { get; set; }

		public bool HWIDProtectEnabled { get; set; }

		public Lastnemesisallyspawntime LastNemesisAllySpawnTime { get; set; }

		public Crewshipsalvagebin CrewShipSalvageBin { get; set; }

		public Playerskills PlayerSkills { get; set; }

		public int CrewShipFusionPoints { get; set; }

		public Settings Settings { get; set; }

		public bool PlayedParkourTutorial { get; set; }

		public Mechbin MechBin { get; set; }

		public int DailyAffiliationEntrati { get; set; }

		public int DailyAffiliationNecraloid { get; set; }

		public Lastinventorysync LastInventorySync { get; set; }

		public Nextrefill NextRefill { get; set; }

		public object[] ActiveLandscapeTraps { get; set; }

		public object[] CrewMembers { get; set; }

		public Suit[] MechSuits { get; set; }

		public object[] RepVotes { get; set; }

		public object[] LeagueTickets { get; set; }

		public object[] Quests { get; set; }

		public object[] Robotics { get; set; }

		public object[] UsedDailyDeals { get; set; }

		public Libraryavailabledailytaskinfo LibraryAvailableDailyTaskInfo { get; set; }

		public bool HasResetAccount { get; set; }

		public Pendingcoupon PendingCoupon { get; set; }

		public bool Harvestable { get; set; }

		public bool DeathSquadable { get; set; }

		public InfestedFoundry InfestedFoundry { get; set; }
	}
}
