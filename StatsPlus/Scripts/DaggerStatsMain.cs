using UnityEngine;
using DaggerfallWorkshop;
using DaggerfallWorkshop.Game;
using DaggerfallWorkshop.Game.Banking;
using DaggerfallWorkshop.Game.Items;
using DaggerfallWorkshop.Game.Entity;
using DaggerfallWorkshop.Game.MagicAndEffects;
using DaggerfallWorkshop.Game.Player;
using DaggerfallWorkshop.Game.Questing;
using DaggerfallWorkshop.Game.UserInterface;
using DaggerfallWorkshop.Game.UserInterfaceWindows;
using DaggerfallWorkshop.Game.Utility;
using DaggerfallWorkshop.Game.Utility.ModSupport;
using DaggerfallWorkshop.Game.Utility.ModSupport.ModSettings;
using DaggerfallWorkshop.Game.Serialization;
using DaggerfallWorkshop.Utility;
using DaggerfallConnect;
using DaggerfallConnect.Arena2;
using DaggerfallConnect.FallExe;
using DaggerfallConnect.Save;
using DaggerfallConnect.Utility;
using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;

namespace DaggerStats
{
	// Save Data 
	[FullSerializer.fsObject("v1")]
	public class DaggerStatsSaveData
	{
		// INTERNAL
		public Dictionary<string, int> savedWeaponData;
		public Dictionary<DFCareer.MagicSkills, Dictionary<string, int>> savedSpellData;
		public Dictionary<string, int> savedLocationData;
		public Dictionary<string, int> savedReputationData;
		public List<int> savedVisitedRegions;
		public List<int> savedVisitedLibraries;
		public float savedTotalTimeClimbing;
		public float savedTotalTimeSwimming;
		public float savedTotalTimeLevitating;
		public int savedActiveLoan;
		public bool savedOwningHouse;
		public bool savedOwningShip;

		// GENERAL
		public uint savedDaysPassed;
		public uint savedYearsPassed;
		public uint savedHoursLoitered;
		public uint savedHoursSlept;
		public uint savedLongestCrashSession;
		public uint savedDaysAsLycanthrope;
		public uint savedDaysAsVampire;
		public string savedTimeSpentClimbing;
		public string savedTimeSpentSwimming;
		public string savedTimeSpentLevitating;
		public int savedTarhielianExpeditions;
		public int savedTownsVisited;
		public int savedHomesVisited;
		public int savedDungeonsVisited;
		public int savedTemplesVisited;
		public int savedRegionsVisited;
		public string savedFavoritePlace;
		public int savedTotalAccountBalance;
		public int savedGoldDeposited;
		public int savedGoldWithdrawn;
		public int savedGoldBorrowed;
		public int savedGoldRepaid;
		public int savedHousesOwned;
		public int savedHousesSold;
		public int savedShipsBought;
		public int savedShipsSold; 
		public int savedDiseasesContracted;
		public int savedLibraryCards;
		public int savedTeleportations;
		public string savedBestReputation;
		public string savedWorstReputation;

		// QUEST
		public int savedMainQuestCompleted;
		public int savedDarkBrotherhoodQuestsCompleted;
		public int savedFightersGuildQuestsCompleted;
		public int savedMagesGuildQuestsCompleted;
		public int savedThievesGuildQuestsCompleted;
		public int savedTempleQuestsCompleted;
		public int savedTempleOrderQuestsCompleted;
		public int savedKnightOrderQuestsCompleted;
		public int savedMerchantQuestsCompleted;
		public int savedCommonerQuestsCompleted;
		public int savedNobilityQuestsCompleted;
		public int savedProstituteQuestsCompleted;
		public int savedWitchCovenQuestsCompleted;
		public int savedVampireClanQuestsCompleted;
		public int savedCureQuestsCompleted;
		public int savedDaedricQuestsCompleted;

		// COMBAT
		public int savedAnimalKills;
		public int savedMonsterKills;
		public int savedHumanKills;
		public int savedOrcKills;
		public int savedLycanthropeKills;
		public int savedAtronachKills;
		public int savedUndeadKills;
		public int savedDaedraKills;
		public string savedFavoriteWeapon;
		public int savedCentaursCalmed;
		public int savedDaedraDeescalated;
		public int savedDragonsDefused;
		public int savedGiantsGentled;
		public int savedHarpiesHushed;
		public int savedImpsImpressed;
		public int savedNymphsNurtured;
		public int savedOrcsOffset;
		public int savedSpriggansShushed;

		// MAGIC
		public int savedSpellsLearned;
		public int savedSpellsCast;
		public string savedFavoriteAlterationSpell;
		public string savedFavoriteDestructionSpell;
		public string savedFavoriteIllusionSpell;
		public string savedFavoriteMysticismSpell;
		public string savedFavoriteRestorationSpell;
		public string savedFavoriteThaumaturgySpell;

		// CRAFTING & SERVICES


		// CRIME
		public uint savedDaysJailed;
		public int savedAssaultCount;
		public int savedAttemptedBNECount;
		public int savedBNECount;
		public int savedCriminalConspiracyCount;
		public int savedHighTreasonCount;
		public int savedLoanDefaultCount;
		public int savedMurderCount;
		public int savedPickpocketCount;
		public int savedPiracyCount;
		public int savedSmugglingCount;
		public int savedTaxEvasionCount;
		public int savedTheftCount;
		public int savedTreasonCount;
		public int savedTrespassingCount;
		public int savedVagrancyCount;

	}

	// Main Class
	public class DaggerStatsMain : MonoBehaviour, IHasModSaveData
	{
		private static Mod mod;
		private static DaggerStatsMain instance;
		public static DaggerStatsMain Instance { get; private set; }
		public EntityEffectManager playerEffectManager = GameManager.Instance.PlayerEffectManager;

		// INTERNAL - for use by the mod, not exposed stats
		public List<int> peopleOf = new List<int>();
		public List<int> courtOf = new List<int>();
		public Dictionary<string, WeaponUseTracker> weaponData;
		public Dictionary<DFCareer.MagicSkills, Dictionary<string, SpellCastTracker>> spellData;
		public Dictionary<string, LocationVisitTracker> locationData;
		public Dictionary<string, int> reputationData;
		public bool openedDaggerStatsWindow = false;
		public bool startedLoiteringCounter = false;
		public uint startedLoitering;
		public bool startedRestingCounter = false;
		public uint startedResting;
		public List<uint> crashSessions = new List<uint>();
		public bool startedLycanthropyCounter = false;
		public uint startedLycanthropy;
		public bool startedVampirismCounter = false;
		public uint startedVampirism;
		public List<int> visitedRegions = new List<int>();
		public List<string> mainQuestNames = new List<string>();
		public List<UInt64> mainQuestIDs = new List<UInt64>();
		public bool attackCounter = false;
		public bool spellCastCounter = false;
		public bool crimeCounter = false;
		public bool startedJailCounter = false;
		public uint startedJail;
		public bool startedClimbingCounter = false;
		public float timeClimbing;
		public float totalTimeClimbing;
		public bool startedSwimmingCounter = false;
		public float timeSwimming;
		public float totalTimeSwimming;
		public bool startedLevitatingCounter = false;
		public float timeLevitating;
		public float totalTimeLevitating;
		public int activeLoan;
		public bool owningHouse;
		public bool owningShip;
		public bool visitingLibrary;
		public List<int> visitedLibraries = new List<int>();
		public bool isMainQuest = false;
		public List<ulong> pacifiedEnemies = new List<ulong>();
		public bool pacifyingEnemyCounter = false;
		public bool talkingWithNPC = false;
		
		// GENERAL
		public uint daysPassed;
		public uint yearsPassed;
		public uint hoursLoitered;
		public uint hoursSlept;
		public uint longestCrashSession;
		public uint daysAsLycanthrope;
		public uint daysAsVampire;
		public string timeSpentClimbing;
		public string timeSpentSwimming;
		public string timeSpentLevitating;
		public int tarhielianExpeditions;
		public int townsVisited;
		public int homesVisited;
		public int dungeonsVisited;
		public int templesVisited;
		public int regionsVisited;
		public string favoritePlace;
		public int totalAccountBalance;
		public int goldDeposited;
		public int goldWithdrawn;
		public int goldBorrowed;
		public int goldRepaid;
		public int housesOwned;
		public int housesSold;
		public int shipsBought;
		public int shipsSold;
		public int diseasesContracted;
		public int libraryCards;
		public int teleportations;
		public string bestReputation;
		public string worstReputation;

		// QUEST
		public int mainQuestCompleted;
		public int darkBrotherhoodQuestsCompleted;
		public int fightersGuildQuestsCompleted;
		public int magesGuildQuestsCompleted;
		public int thievesGuildQuestsCompleted;
		public int templeQuestsCompleted;
		public int templeOrderQuestsCompleted;
		public int knightOrderQuestsCompleted;
		public int merchantQuestsCompleted;
		public int commonerQuestsCompleted;
		public int nobilityQuestsCompleted;
		public int prostituteQuestsCompleted;
		public int witchCovenQuestsCompleted;
		public int vampireClanQuestsCompleted;
		public int cureQuestsCompleted;
		public int daedricQuestsCompleted;

		// COMBAT
		public int animalKills;
		public int monsterKills;
		public int humanKills;
		public int orcKills;
		public int lycanthropeKills;
		public int atronachKills;
		public int undeadKills;
		public int daedraKills;
		public string favoriteWeapon;
		public int centaursCalmed;
		public int daedraDeescalated;
		public int dragonsDefused;
		public int giantsGentled;
		public int harpiesHushed;
		public int impsImpressed;
		public int nymphsNurtured;
		public int orcsOffset;
		public int spriggansShushed;
		
		// MAGIC
		public int spellsLearned;
		public int spellsCast;
		public string favoriteAlterationSpell;
		public string favoriteDestructionSpell;
		public string favoriteIllusionSpell;
		public string favoriteMysticismSpell;
		public string favoriteRestorationSpell;
		public string favoriteThaumaturgySpell;

		// CRAFTING & SERVICES


		// CRIME
		public uint daysJailed;
		public int assaultCount;
		public int attemptedBNECount;
		public int BNECount;
		public int criminalConspiracyCount;
		public int highTreasonCount;
		public int loanDefaultCount;
		public int murderCount;
		public int pickpocketCount;
		public int piracyCount;
		public int smugglingCount;
		public int taxEvasionCount;
		public int theftCount;
		public int treasonCount;
		public int trespassingCount;
		public int vagrancyCount;

		// -----------------------------------------------------------
	    // = SAVE / LOAD                                             =
	    // -----------------------------------------------------------
		public Type SaveDataType { get { return typeof(DaggerStatsSaveData); }}

		public object NewSaveData()
	    {
	        return new DaggerStatsSaveData
	        {
	        	// INTERNAL
	        	savedWeaponData = ConvertWeaponDataToSaveFormat(weaponData),
                savedSpellData = ConvertSpellDataToSaveFormat(spellData),
                savedLocationData = ConvertLocationDataToSaveFormat(locationData),
                savedReputationData = reputationData,
                savedVisitedLibraries = visitedLibraries,
                savedTotalTimeClimbing = totalTimeClimbing,
                savedTotalTimeSwimming = totalTimeSwimming,
                savedTotalTimeLevitating = totalTimeLevitating,
                savedOwningHouse = owningHouse,
                savedOwningShip = owningShip,
                savedActiveLoan = activeLoan,

	        	// GENERAL
	            savedDaysPassed = daysPassed,
	            savedYearsPassed = yearsPassed,
	            savedHoursLoitered = hoursLoitered,
	            savedHoursSlept = hoursSlept,
	            savedLongestCrashSession = longestCrashSession,
	            savedDaysAsLycanthrope = daysAsLycanthrope,
	            savedDaysAsVampire = daysAsVampire,
	            savedTimeSpentClimbing = timeSpentClimbing,
	            savedTimeSpentSwimming = timeSpentSwimming,
	            savedTimeSpentLevitating = timeSpentLevitating,
	            savedTarhielianExpeditions = tarhielianExpeditions,
	            savedTownsVisited = townsVisited,
	            savedHomesVisited = homesVisited,
	            savedDungeonsVisited = dungeonsVisited,
	            savedTemplesVisited = templesVisited,
	            savedRegionsVisited = regionsVisited,
	            savedFavoritePlace = favoritePlace,
	            savedTotalAccountBalance = totalAccountBalance,
	            savedGoldDeposited = goldDeposited,
	            savedGoldWithdrawn = goldWithdrawn,
	            savedGoldBorrowed = goldBorrowed,
	            savedGoldRepaid = goldRepaid,
	            savedHousesOwned = housesOwned,
	            savedHousesSold = housesSold,
	            savedShipsBought = shipsBought,
	            savedShipsSold = shipsSold,
	            savedDiseasesContracted = diseasesContracted,
	            savedLibraryCards = libraryCards,
	            savedTeleportations = teleportations,
	            savedBestReputation = bestReputation,
	            savedWorstReputation = worstReputation,

	            // QUESTS
	            savedMainQuestCompleted = mainQuestCompleted,
	            savedDarkBrotherhoodQuestsCompleted = darkBrotherhoodQuestsCompleted,
	            savedFightersGuildQuestsCompleted = fightersGuildQuestsCompleted,
	            savedMagesGuildQuestsCompleted = magesGuildQuestsCompleted,
	            savedThievesGuildQuestsCompleted = thievesGuildQuestsCompleted,
	            savedTempleQuestsCompleted = templeQuestsCompleted,
	            savedTempleOrderQuestsCompleted = templeOrderQuestsCompleted,
	            savedKnightOrderQuestsCompleted = knightOrderQuestsCompleted,
	            savedMerchantQuestsCompleted = merchantQuestsCompleted,
	            savedCommonerQuestsCompleted = commonerQuestsCompleted,
	            savedNobilityQuestsCompleted = nobilityQuestsCompleted,
	            savedProstituteQuestsCompleted = prostituteQuestsCompleted,
	            savedWitchCovenQuestsCompleted = witchCovenQuestsCompleted,
	            savedVampireClanQuestsCompleted = vampireClanQuestsCompleted,
	            savedCureQuestsCompleted = cureQuestsCompleted,
	            savedDaedricQuestsCompleted = daedricQuestsCompleted,

	            // COMBAT
                savedAnimalKills = animalKills,
                savedMonsterKills = monsterKills,
                savedHumanKills = humanKills,
                savedOrcKills = orcKills,
                savedLycanthropeKills = lycanthropeKills,
                savedAtronachKills = atronachKills,
                savedUndeadKills = undeadKills,
                savedDaedraKills = daedraKills,
                savedFavoriteWeapon = GetMostUsedWeapon(),
                savedCentaursCalmed = centaursCalmed,
                savedDaedraDeescalated = daedraDeescalated,
                savedDragonsDefused = dragonsDefused,
                savedGiantsGentled = giantsGentled,
                savedHarpiesHushed = harpiesHushed,
                savedImpsImpressed = impsImpressed,
                savedNymphsNurtured = nymphsNurtured,
                savedOrcsOffset = orcsOffset,
                savedSpriggansShushed = spriggansShushed,

                // MAGIC
                savedSpellsLearned = spellsLearned,
                savedSpellsCast = spellsCast,
                savedFavoriteAlterationSpell = GetMostCastSpellBySchool(DFCareer.MagicSkills.Alteration),
                savedFavoriteDestructionSpell = GetMostCastSpellBySchool(DFCareer.MagicSkills.Destruction),
                savedFavoriteIllusionSpell = GetMostCastSpellBySchool(DFCareer.MagicSkills.Illusion),
                savedFavoriteMysticismSpell = GetMostCastSpellBySchool(DFCareer.MagicSkills.Mysticism),
                savedFavoriteRestorationSpell = GetMostCastSpellBySchool(DFCareer.MagicSkills.Restoration),
                savedFavoriteThaumaturgySpell = GetMostCastSpellBySchool(DFCareer.MagicSkills.Thaumaturgy),

                // CRIME
                savedDaysJailed = daysJailed,
                savedAssaultCount = assaultCount,
                savedAttemptedBNECount = attemptedBNECount,
                savedBNECount = BNECount,
                savedCriminalConspiracyCount = criminalConspiracyCount,
                savedHighTreasonCount = highTreasonCount,
                savedLoanDefaultCount = loanDefaultCount,
                savedMurderCount = murderCount,
                savedPickpocketCount = pickpocketCount,
                savedPiracyCount = piracyCount,
                savedSmugglingCount = smugglingCount,
                savedTaxEvasionCount = taxEvasionCount,
                savedTheftCount = theftCount,
                savedTreasonCount = treasonCount,
                savedTrespassingCount = trespassingCount,
                savedVagrancyCount = vagrancyCount,
	        };
	    }

	    public object GetSaveData()
	    {
	        return new DaggerStatsSaveData
	        {
	        	// INTERNAL
	        	savedWeaponData = ConvertWeaponDataToSaveFormat(weaponData),
                savedSpellData = ConvertSpellDataToSaveFormat(spellData),
                savedLocationData = ConvertLocationDataToSaveFormat(locationData),
                savedReputationData = reputationData,
                savedTotalTimeClimbing = totalTimeClimbing,
                savedTotalTimeSwimming = totalTimeSwimming,
                savedTotalTimeLevitating = totalTimeLevitating,
                savedOwningHouse = owningHouse,
                savedOwningShip = owningShip,
                savedVisitedLibraries = visitedLibraries,
                savedActiveLoan = activeLoan,

	        	// GENERAL
	            savedDaysPassed = daysPassed,
	            savedYearsPassed = yearsPassed,
	            savedHoursLoitered = hoursLoitered,
	            savedHoursSlept = hoursSlept,
	            savedLongestCrashSession = longestCrashSession,
	            savedDaysAsLycanthrope = daysAsLycanthrope,
	            savedDaysAsVampire = daysAsVampire,
	            savedTimeSpentClimbing = timeSpentClimbing,
	            savedTimeSpentSwimming = timeSpentSwimming,
	            savedTimeSpentLevitating = timeSpentLevitating,
	            savedTarhielianExpeditions = tarhielianExpeditions,
	            savedTownsVisited = townsVisited,
	            savedHomesVisited = homesVisited,
	            savedDungeonsVisited = dungeonsVisited,
	            savedTemplesVisited = templesVisited,
	            savedRegionsVisited = regionsVisited,
	            savedFavoritePlace = favoritePlace,
	            savedTotalAccountBalance = totalAccountBalance,
	            savedGoldDeposited = goldDeposited,
	            savedGoldWithdrawn = goldWithdrawn,
	            savedGoldBorrowed = goldBorrowed,
	            savedGoldRepaid = goldRepaid,
	            savedHousesOwned = housesOwned,
	            savedHousesSold = housesSold,
	            savedShipsBought = shipsBought,
	            savedShipsSold = shipsSold,
	            savedDiseasesContracted = diseasesContracted,
	            savedLibraryCards = libraryCards,
	            savedTeleportations = teleportations,
	            savedBestReputation = bestReputation,
	            savedWorstReputation = worstReputation,

	            // QUESTS
	            savedMainQuestCompleted = mainQuestCompleted,
	            savedDarkBrotherhoodQuestsCompleted = darkBrotherhoodQuestsCompleted,
	            savedFightersGuildQuestsCompleted = fightersGuildQuestsCompleted,
	            savedMagesGuildQuestsCompleted = magesGuildQuestsCompleted,
	            savedThievesGuildQuestsCompleted = thievesGuildQuestsCompleted,
	            savedTempleQuestsCompleted = templeQuestsCompleted,
	            savedTempleOrderQuestsCompleted = templeOrderQuestsCompleted,
	            savedKnightOrderQuestsCompleted = knightOrderQuestsCompleted,
	            savedMerchantQuestsCompleted = merchantQuestsCompleted,
	            savedCommonerQuestsCompleted = commonerQuestsCompleted,
	            savedNobilityQuestsCompleted = nobilityQuestsCompleted,
	            savedProstituteQuestsCompleted = prostituteQuestsCompleted,
	            savedWitchCovenQuestsCompleted = witchCovenQuestsCompleted,
	            savedVampireClanQuestsCompleted = vampireClanQuestsCompleted,
	            savedCureQuestsCompleted = cureQuestsCompleted,
	            savedDaedricQuestsCompleted = daedricQuestsCompleted,

	            // COMBAT
                savedAnimalKills = animalKills,
                savedMonsterKills = monsterKills,
                savedHumanKills = humanKills,
                savedOrcKills = orcKills,
                savedLycanthropeKills = lycanthropeKills,
                savedAtronachKills = atronachKills,
                savedUndeadKills = undeadKills,
                savedDaedraKills = daedraKills,
                savedFavoriteWeapon = GetMostUsedWeapon(),
                savedCentaursCalmed = centaursCalmed,
                savedDaedraDeescalated = daedraDeescalated,
                savedDragonsDefused = dragonsDefused,
                savedGiantsGentled = giantsGentled,
                savedHarpiesHushed = harpiesHushed,
                savedImpsImpressed = impsImpressed,
                savedNymphsNurtured = nymphsNurtured,
                savedOrcsOffset = orcsOffset,
                savedSpriggansShushed = spriggansShushed,

                // MAGIC
                savedSpellsLearned = spellsLearned,
                savedSpellsCast = spellsCast,
                savedFavoriteAlterationSpell = GetMostCastSpellBySchool(DFCareer.MagicSkills.Alteration),
                savedFavoriteDestructionSpell = GetMostCastSpellBySchool(DFCareer.MagicSkills.Destruction),
                savedFavoriteIllusionSpell = GetMostCastSpellBySchool(DFCareer.MagicSkills.Illusion),
                savedFavoriteMysticismSpell = GetMostCastSpellBySchool(DFCareer.MagicSkills.Mysticism),
                savedFavoriteRestorationSpell = GetMostCastSpellBySchool(DFCareer.MagicSkills.Restoration),
                savedFavoriteThaumaturgySpell = GetMostCastSpellBySchool(DFCareer.MagicSkills.Thaumaturgy),

                // CRIME
                savedDaysJailed = daysJailed,
                savedAssaultCount = assaultCount,
                savedAttemptedBNECount = attemptedBNECount,
                savedBNECount = BNECount,
                savedCriminalConspiracyCount = criminalConspiracyCount,
                savedHighTreasonCount = highTreasonCount,
                savedLoanDefaultCount = loanDefaultCount,
                savedMurderCount = murderCount,
                savedPickpocketCount = pickpocketCount,
                savedPiracyCount = piracyCount,
                savedSmugglingCount = smugglingCount,
                savedTaxEvasionCount = taxEvasionCount,
                savedTheftCount = theftCount,
                savedTreasonCount = treasonCount,
                savedTrespassingCount = trespassingCount,
                savedVagrancyCount = vagrancyCount,
	        };
	    }

	    public void RestoreSaveData(object saveData)
	    {
	        var daggerStatsSaveData = (DaggerStatsSaveData)saveData;

	        // INTERNAL
	        InitializeWeaponData(daggerStatsSaveData.savedWeaponData);
            InitializeSpellData(daggerStatsSaveData.savedSpellData);
            InitializeLocationData(daggerStatsSaveData.savedLocationData);
            reputationData = daggerStatsSaveData.savedReputationData;
            totalTimeClimbing = daggerStatsSaveData.savedTotalTimeClimbing;
            totalTimeSwimming = daggerStatsSaveData.savedTotalTimeSwimming;
            totalTimeLevitating = daggerStatsSaveData.savedTotalTimeLevitating;
            activeLoan = daggerStatsSaveData.savedActiveLoan;
            owningHouse = daggerStatsSaveData.savedOwningHouse;
            owningShip = daggerStatsSaveData.savedOwningShip;
            visitedLibraries = daggerStatsSaveData.savedVisitedLibraries;

	        // GENERAL
	        daysPassed = daggerStatsSaveData.savedDaysPassed;
	        yearsPassed = daggerStatsSaveData.savedYearsPassed;
	        hoursLoitered = daggerStatsSaveData.savedHoursLoitered;
	        hoursSlept = daggerStatsSaveData.savedHoursSlept;
	        longestCrashSession = daggerStatsSaveData.savedLongestCrashSession;
	        daysAsLycanthrope = daggerStatsSaveData.savedDaysAsLycanthrope;
	        daysAsVampire = daggerStatsSaveData.savedDaysAsVampire;
	        timeSpentClimbing = daggerStatsSaveData.savedTimeSpentClimbing;
	        timeSpentSwimming = daggerStatsSaveData.savedTimeSpentSwimming;
	        timeSpentLevitating = daggerStatsSaveData.savedTimeSpentLevitating;
	        tarhielianExpeditions = daggerStatsSaveData.savedTarhielianExpeditions;
	        townsVisited = daggerStatsSaveData.savedTownsVisited;
	        homesVisited = daggerStatsSaveData.savedHomesVisited;
	        dungeonsVisited = daggerStatsSaveData.savedDungeonsVisited;
	        templesVisited = daggerStatsSaveData.savedTemplesVisited;
	        regionsVisited = daggerStatsSaveData.savedRegionsVisited;
	        favoritePlace = daggerStatsSaveData.savedFavoritePlace;
	        
	        totalAccountBalance = daggerStatsSaveData.savedTotalAccountBalance;
	        goldDeposited = daggerStatsSaveData.savedGoldDeposited;
	        goldWithdrawn = daggerStatsSaveData.savedGoldWithdrawn;
	        goldBorrowed = daggerStatsSaveData.savedGoldBorrowed;
	        goldRepaid = daggerStatsSaveData.savedGoldRepaid;
	        housesOwned = daggerStatsSaveData.savedHousesOwned;
	        housesSold = daggerStatsSaveData.savedHousesSold;
	        shipsBought = daggerStatsSaveData.savedShipsBought;
	        shipsSold = daggerStatsSaveData.savedShipsSold;
	        diseasesContracted = daggerStatsSaveData.savedDiseasesContracted;
	        libraryCards = daggerStatsSaveData.savedLibraryCards;
	        teleportations = daggerStatsSaveData.savedTeleportations;
	        bestReputation = daggerStatsSaveData.savedBestReputation;
	        worstReputation = daggerStatsSaveData.savedWorstReputation;

	        // QUESTS
	        mainQuestCompleted = daggerStatsSaveData.savedMainQuestCompleted;
	        darkBrotherhoodQuestsCompleted = daggerStatsSaveData.savedDarkBrotherhoodQuestsCompleted;
	        fightersGuildQuestsCompleted = daggerStatsSaveData.savedFightersGuildQuestsCompleted;
	        magesGuildQuestsCompleted = daggerStatsSaveData.savedMagesGuildQuestsCompleted;
	        thievesGuildQuestsCompleted = daggerStatsSaveData.savedThievesGuildQuestsCompleted;
	        templeQuestsCompleted = daggerStatsSaveData.savedTempleQuestsCompleted;
	        templeOrderQuestsCompleted = daggerStatsSaveData.savedTempleOrderQuestsCompleted;
	        knightOrderQuestsCompleted = daggerStatsSaveData.savedKnightOrderQuestsCompleted;
	        merchantQuestsCompleted = daggerStatsSaveData.savedMerchantQuestsCompleted;
	        commonerQuestsCompleted = daggerStatsSaveData.savedCommonerQuestsCompleted;
	        nobilityQuestsCompleted = daggerStatsSaveData.savedNobilityQuestsCompleted;
	        prostituteQuestsCompleted = daggerStatsSaveData.savedProstituteQuestsCompleted;
	        witchCovenQuestsCompleted = daggerStatsSaveData.savedWitchCovenQuestsCompleted;
	        vampireClanQuestsCompleted = daggerStatsSaveData.savedVampireClanQuestsCompleted;
	        cureQuestsCompleted = daggerStatsSaveData.savedCureQuestsCompleted;
	        daedricQuestsCompleted = daggerStatsSaveData.savedDaedricQuestsCompleted;

	        // COMBAT
	        animalKills = daggerStatsSaveData.savedAnimalKills;
            monsterKills = daggerStatsSaveData.savedMonsterKills;
            humanKills = daggerStatsSaveData.savedHumanKills;
            orcKills = daggerStatsSaveData.savedOrcKills;
            lycanthropeKills = daggerStatsSaveData.savedLycanthropeKills;
            atronachKills = daggerStatsSaveData.savedAtronachKills;
            undeadKills = daggerStatsSaveData.savedUndeadKills;
            daedraKills = daggerStatsSaveData.savedDaedraKills;
            favoriteWeapon = daggerStatsSaveData.savedFavoriteWeapon;
            centaursCalmed = daggerStatsSaveData.savedCentaursCalmed;
            daedraDeescalated = daggerStatsSaveData.savedDaedraDeescalated;
            dragonsDefused = daggerStatsSaveData.savedDragonsDefused;
            giantsGentled = daggerStatsSaveData.savedGiantsGentled;
            harpiesHushed = daggerStatsSaveData.savedHarpiesHushed;
            impsImpressed = daggerStatsSaveData.savedImpsImpressed;
            nymphsNurtured = daggerStatsSaveData.savedNymphsNurtured;
            orcsOffset = daggerStatsSaveData.savedOrcsOffset;
            spriggansShushed = daggerStatsSaveData.savedSpriggansShushed;

            // MAGIC
            spellsLearned = daggerStatsSaveData.savedSpellsLearned;
            spellsCast = daggerStatsSaveData.savedSpellsCast;
            favoriteAlterationSpell = daggerStatsSaveData.savedFavoriteAlterationSpell;
            favoriteDestructionSpell = daggerStatsSaveData.savedFavoriteDestructionSpell;
            favoriteIllusionSpell = daggerStatsSaveData.savedFavoriteIllusionSpell;
            favoriteMysticismSpell = daggerStatsSaveData.savedFavoriteMysticismSpell;
            favoriteRestorationSpell = daggerStatsSaveData.savedFavoriteRestorationSpell;
            favoriteThaumaturgySpell = daggerStatsSaveData.savedFavoriteThaumaturgySpell;

            // CRIME
            daysJailed = daggerStatsSaveData.savedDaysJailed;
            assaultCount = daggerStatsSaveData.savedAssaultCount;
            attemptedBNECount = daggerStatsSaveData.savedAttemptedBNECount;
            BNECount = daggerStatsSaveData.savedBNECount;
            criminalConspiracyCount = daggerStatsSaveData.savedCriminalConspiracyCount;
            highTreasonCount = daggerStatsSaveData.savedHighTreasonCount;
            loanDefaultCount = daggerStatsSaveData.savedLoanDefaultCount;
            murderCount = daggerStatsSaveData.savedMurderCount;
            pickpocketCount = daggerStatsSaveData.savedPickpocketCount;
            piracyCount = daggerStatsSaveData.savedPiracyCount;
            smugglingCount = daggerStatsSaveData.savedSmugglingCount;
            taxEvasionCount = daggerStatsSaveData.savedTaxEvasionCount;
            theftCount = daggerStatsSaveData.savedTheftCount;
            treasonCount = daggerStatsSaveData.savedTreasonCount;
            trespassingCount = daggerStatsSaveData.savedTrespassingCount;
            vagrancyCount = daggerStatsSaveData.savedVagrancyCount;
	    }

	    // Converters for Spell/Weapon data dictionaries
	    private void InitializeWeaponData(Dictionary<string, int> loadedSavedWeaponData)
        {
            weaponData = new Dictionary<string, WeaponUseTracker>(); 
            if (loadedSavedWeaponData != null)
            {
                foreach (var pair in loadedSavedWeaponData)
                {
                    WeaponUseTracker tracker = new WeaponUseTracker(pair.Key);
                    typeof(WeaponUseTracker).GetProperty("useCount").SetValue(tracker, pair.Value);
                    weaponData.Add(pair.Key, tracker);
                }
                //Debug.Log($"[DaggerStats] Weapon data initialized with {weaponData.Count} entries from save.");
            }
            else
            {
                //Debug.Log("[DaggerStats] No saved weapon data, initializing empty weaponData dictionary.");
            }
        }

        private Dictionary<string, int> ConvertWeaponDataToSaveFormat(Dictionary<string, WeaponUseTracker> runtimeData)
        {
            Dictionary<string, int> saveData = new Dictionary<string, int>();
            if (runtimeData != null)
            {
                foreach (var pair in runtimeData)
                {
                    saveData.Add(pair.Key, pair.Value.useCount);
                }
            }
            return saveData;
        }

        private void InitializeLocationData(Dictionary<string, int> loadedSavedLocationData)
        {
        	locationData = new Dictionary<string, LocationVisitTracker>();
        	if (loadedSavedLocationData != null)
        	{
        		foreach (var pair in loadedSavedLocationData)
        		{
        			LocationVisitTracker tracker = new LocationVisitTracker(pair.Key);
        			typeof(LocationVisitTracker).GetProperty("visitCount").SetValue(tracker, pair.Value);
        			locationData.Add(pair.Key, tracker);
        		}
        	}
        }

        private Dictionary<string, int> ConvertLocationDataToSaveFormat(Dictionary<string, LocationVisitTracker> runtimeData)
        {
        	Dictionary<string, int> saveData = new Dictionary<string, int>();
        	if (runtimeData != null)
        	{
        		foreach (var pair in runtimeData)
        		{
        			saveData.Add(pair.Key, pair.Value.visitCount);
        		}
        	}
        	return saveData;
        }

        private void InitializeSpellData(Dictionary<DFCareer.MagicSkills, Dictionary<string, int>> loadedSavedSpellData)
        {
            spellData = new Dictionary<DFCareer.MagicSkills, Dictionary<string, SpellCastTracker>>();
            SetupSpellCount();

            if (loadedSavedSpellData != null)
            {
                foreach (var schoolPair in loadedSavedSpellData)
                {
                    DFCareer.MagicSkills school = schoolPair.Key;
                    Dictionary<string, int> savedInnerDict = schoolPair.Value;
                    Dictionary<string, SpellCastTracker> runtimeInnerDict;

                    if (spellData.TryGetValue(school, out runtimeInnerDict))
                    {
                        foreach (var spellPair in savedInnerDict)
                        {
                            SpellCastTracker tracker = new SpellCastTracker(spellPair.Key);
                            typeof(SpellCastTracker).GetProperty("castCount").SetValue(tracker, spellPair.Value);
                            runtimeInnerDict.Add(spellPair.Key, tracker);
                        }
                    }
                }
            }
        }

        private Dictionary<DFCareer.MagicSkills, Dictionary<string, int>> ConvertSpellDataToSaveFormat(Dictionary<DFCareer.MagicSkills, Dictionary<string, SpellCastTracker>> runtimeData)
        {
            Dictionary<DFCareer.MagicSkills, Dictionary<string, int>> saveData = new Dictionary<DFCareer.MagicSkills, Dictionary<string, int>>();

            if (runtimeData != null)
            {
                foreach (var schoolPair in runtimeData)
                {
                    Dictionary<string, int> innerSaveData = new Dictionary<string, int>();
                    foreach (var spellPair in schoolPair.Value)
                    {
                        innerSaveData.Add(spellPair.Key, spellPair.Value.castCount);
                    }
                    saveData.Add(schoolPair.Key, innerSaveData);
                }
            }
            return saveData;
        }

	    // -----------------------------------------------------------
	    // = INIT                                                    =
	    // -----------------------------------------------------------
	    [Invoke(StateManager.StateTypes.Start, -1)]
	    public static void Init(InitParams initParams)
	    {
	    	mod = initParams.Mod;

	    	var go = new GameObject(mod.Title);
			
			instance = go.AddComponent<DaggerStatsMain>();
			Instance = instance;

			mod.SaveDataInterface = instance;

			// StreamingWorld.OnTeleportToCoordinates += OnTeleportToCoordinates;
			EnemyDeath.OnEnemyDeath += OnEnemyDeath;
			PlayerGPS.OnMapPixelChanged += OnMapPixelChanged;
			GameManager.Instance.PlayerEffectManager.OnAssignBundle += OnAssignBundle;
			QuestMachine.OnQuestEnded += OnQuestEnded;
			QuestMachine.OnQuestErrorTermination += OnQuestEnded;
			DaggerfallBankManager.OnBorrowLoan += OnBorrowLoan;
			DaggerfallBankManager.OnDepositGold += OnDepositGold;
			DaggerfallBankManager.OnRepayLoan += OnRepayLoan;
			DaggerfallBankManager.OnSellHouse += OnSellHouse;
			DaggerfallBankManager.OnSellShip += OnSellShip;
			DaggerfallBankManager.OnWithdrawGold += OnWithdrawGold;
	    }
		
	    void Awake()
	    {
    		SetupWeaponCount();
    		SetupLocationCount();
    		SetupSpellCount();
    		SetupMainQuests();
    		SetupQuestFactionIdLists();
    		//SetupVisitedRegions();
    		//SetupReputations();
	    }

	    void Update()
	    {
	    	// DaggerStats Window
	    	if (Input.GetKey(KeyCode.X) && openedDaggerStatsWindow == false && DaggerfallUI.Instance.UserInterfaceManager.TopWindow as DaggerfallCharacterSheetWindow != null)
	    	{
	    		uint now = DaggerfallUnity.Instance.WorldTime.DaggerfallDateTime.ToClassicDaggerfallTime();

	    		// Calculations necessary just before opening the window
	    		daysPassed = CalculateDaysPassed();
	    		yearsPassed = CalculateYearsPassed(daysPassed);
	    		longestCrashSession = CalculateLongestCrashSession();
	    		favoritePlace = GetMostVisitedLocation();
	    		CalculateTimeSpentAsLycanthope(now);
	    		CalculateTimeSpentAsVampire(now);
	    		CalculateClimbingSwimmingLevitatingTimes();
	    		CalculateRegionsVisited();
	    		CalculateAccountTotal();
	    		CalculateHousesOwned();
	    		CalculateShipsOwned();
	    		CalculateLibrariesVisited();
	    		// CalculateReputations();
	    		GetMainQuestsCompleted();
	    		SpellCount();

	    		// Open the window
	    		OpenStatsWindow();
	    		openedDaggerStatsWindow = true;
	    	}

	    	// Loitering Checks
	    	if (startedLoiteringCounter == false && GameManager.Instance.PlayerEntity.IsLoitering)
	    	{
	    		startedLoitering = DaggerfallUnity.Instance.WorldTime.DaggerfallDateTime.ToClassicDaggerfallTime();
	    		startedLoiteringCounter = true;
	    		//Debug.Log("[DaggerStats] startedLoitering time:" + startedLoitering);
	    	}

	    	if (startedLoiteringCounter == true && !GameManager.Instance.PlayerEntity.IsLoitering)
	    	{
	    		uint stoppedLoitering = DaggerfallUnity.Instance.WorldTime.DaggerfallDateTime.ToClassicDaggerfallTime();
	    		uint hoursSpentLoitering = (stoppedLoitering - startedLoitering) / 60;
	    		//Debug.Log("[DaggerStats] stopped loitering time: " + stoppedLoitering);
	    		//Debug.Log("[DaggerStats] hoursSpentLoitering: " + hoursSpentLoitering);
	    		hoursLoitered += hoursSpentLoitering;

	    		startedLoiteringCounter = false;
	    	}

	    	// Resting Checks
	    	if (startedRestingCounter == false && startedLoiteringCounter == false && GameManager.Instance.PlayerEntity.IsResting && GameManager.Instance.PlayerEnterExit.IsPlayerInsideTavern)
	    	{
	    		startedResting = DaggerfallUnity.Instance.WorldTime.DaggerfallDateTime.ToClassicDaggerfallTime();
	    		startedRestingCounter = true;
	    	}

	    	if (startedRestingCounter == true && startedLoiteringCounter == false && !GameManager.Instance.PlayerEntity.IsResting && GameManager.Instance.PlayerEnterExit.IsPlayerInsideTavern)
	    	{
	    		uint stoppedResting = DaggerfallUnity.Instance.WorldTime.DaggerfallDateTime.ToClassicDaggerfallTime();
	    		uint hoursSpentResting = (stoppedResting - startedResting) / 60;
	    		uint daysSpentResting = hoursSpentResting / 24;
	    		hoursSlept += hoursSpentResting;
	    		crashSessions.Add(hoursSpentResting);

	    		startedRestingCounter = false;
	    	}

	    	// Jail Time Checks
	    	if (startedJailCounter == false && GameManager.Instance.PlayerEntity.InPrison == true)
	    	{
	    		startedJail = DaggerfallUnity.Instance.WorldTime.DaggerfallDateTime.ToClassicDaggerfallTime();
	    		startedJailCounter = true;
	    	}

	    	if (startedJailCounter == true && GameManager.Instance.PlayerEntity.InPrison == false)
	    	{
	    		uint stoppedJail = DaggerfallUnity.Instance.WorldTime.DaggerfallDateTime.ToClassicDaggerfallTime();
	    		uint hoursSpentJailed = (stoppedJail - startedJail) / 60;
	    		uint daysSpentJailed = hoursSpentJailed / 24;
	    		daysJailed += daysSpentJailed;

	    		startedJailCounter = false;
	    	}

	    	// Lycanthropy Checks
	    	if (startedLycanthropyCounter == false && playerEffectManager.HasLycanthropy())
	    	{
	    		startedLycanthropy = DaggerfallUnity.Instance.WorldTime.DaggerfallDateTime.ToClassicDaggerfallTime();
	    		startedLycanthropyCounter = true;
	    	}

	    	if (startedLycanthropyCounter == true && !playerEffectManager.HasLycanthropy())
	    	{
	    		uint stoppedLycanthropy = DaggerfallUnity.Instance.WorldTime.DaggerfallDateTime.ToClassicDaggerfallTime();
	    		uint hoursSpentAsLycanthrope = (stoppedLycanthropy - startedLycanthropy) / 60;
	    		daysAsLycanthrope += hoursSpentAsLycanthrope;

	    		startedLycanthropyCounter = false;
	    	}

	    	// Vampire Checks
	    	if (startedVampirismCounter == false && playerEffectManager.HasVampirism())
	    	{
	    		startedVampirism = DaggerfallUnity.Instance.WorldTime.DaggerfallDateTime.ToClassicDaggerfallTime();
	    		startedVampirismCounter = true;
	    	}

	    	if (startedVampirismCounter == true && !playerEffectManager.HasVampirism())
	    	{
	    		uint stoppedVampirism = DaggerfallUnity.Instance.WorldTime.DaggerfallDateTime.ToClassicDaggerfallTime();
	    		uint hoursSpentAsVampire = (stoppedVampirism - startedVampirism) / 60;
	    		daysAsVampire += hoursSpentAsVampire;

	    		startedVampirismCounter = false;
	    	}

	    	// Climbing Check
	    	if (startedClimbingCounter == false && GameManager.Instance.PlayerMotor.IsClimbing == true)
	    	{
	    		startedClimbingCounter = true;
	    	}

	    	if (startedClimbingCounter == true && GameManager.Instance.PlayerMotor.IsClimbing == true)
	    	{
	    		timeClimbing += Time.deltaTime;
	    	}

	    	if (startedClimbingCounter == true && GameManager.Instance.PlayerMotor.IsClimbing == false)
	    	{
	    		totalTimeClimbing += timeClimbing;
	    		timeClimbing = 0;
	    		startedClimbingCounter = false;
	    	}

	    	// Swimming Check
	    	if (startedSwimmingCounter == false && GameManager.Instance.PlayerMotor.IsSwimming == true)
	    	{
	    		startedSwimmingCounter = true;
	    	}

	    	if (startedSwimmingCounter == true && GameManager.Instance.PlayerMotor.IsSwimming == true)
	    	{
	    		timeSwimming += Time.deltaTime;
	    	}

	    	if (startedSwimmingCounter == true && GameManager.Instance.PlayerMotor.IsSwimming == false)
	    	{
	    		totalTimeSwimming += timeSwimming;
	    		timeSwimming = 0;
	    		startedSwimmingCounter = false;
	    	}

	    	// Levitating Check
	    	if (startedLevitatingCounter == false && GameManager.Instance.PlayerMotor.IsLevitating == true)
	    	{
	    		startedLevitatingCounter = true;
	    	}

	    	if (startedLevitatingCounter == true && GameManager.Instance.PlayerMotor.IsLevitating == true)
	    	{
	    		timeLevitating += Time.deltaTime;
	    	}

	    	if (startedLevitatingCounter == true && GameManager.Instance.PlayerMotor.IsLevitating == false)
	    	{
	    		totalTimeLevitating += timeLevitating;
	    		timeLevitating = 0;

	    		// Tarhielian Expedition check
	    		CheckFallingDamage();

	    		startedLevitatingCounter = false;	
	    	} 

	    	// Library Check
	    	if (visitingLibrary == false && GameManager.Instance.PlayerEnterExit.BuildingType == DFLocation.BuildingTypes.Library)
	    	{
	    		if (visitedLibraries == null)
	    		{
	    			visitedLibraries = new List<int>();
	    		}

	    		CheckLibraryData();
	    		visitingLibrary = true;
	    	}

	    	if (visitingLibrary == true && GameManager.Instance.PlayerEnterExit.BuildingType != DFLocation.BuildingTypes.Library)
	    	{
	    		visitingLibrary = false;
	    	}

	    	// Pacified Enemy Checks
	    	if (pacifyingEnemyCounter == false)
	    	{
	    		CheckForPacifiedEnemies();
	    	}

	    	// Weapon Checks
	    	FPSWeapon weapon = GameManager.Instance.WeaponManager.ScreenWeapon;
	    	ItemEquipTable playerEquipTable = GameManager.Instance.PlayerEntity.ItemEquipTable;
	    	if (attackCounter == false && weapon.IsAttacking())
	    	{
	    		attackCounter = true;
	    		if (weapon != null)
	    		{
	    			DaggerfallUnityItem currentRightHand = playerEquipTable.GetItem(EquipSlots.RightHand);
	    			string weaponName;
	    			if (currentRightHand == null)
	    			{
	    				weaponName = "Unarmed";
	    			}
	    			else
	    			{
	    				weaponName = weapon.MetalType + " " + currentRightHand.shortName;
	    			}

	    			//Debug.Log("[DaggerStats] Weapon used: " + weaponName);
	    			RecordWeaponUse(weaponName);

	    			favoriteWeapon = GetMostUsedWeapon();
	    			//Debug.Log("[DaggerStats] Favorite weapon: " + favoriteWeapon);

	    		}
	    	}

	    	if (attackCounter == true && !GameManager.Instance.WeaponManager.ScreenWeapon.IsAttacking())
	    	{
	    		attackCounter = false;
	    	}

	    	// Spell Cast Checks
	    	if (spellCastCounter == false && GameManager.Instance.PlayerSpellCasting.IsPlayingAnim == true )
	    	{

	    		//Debug.Log("[DaggerStats] Casting a spell.");
	    		EntityEffectBundle lastSpell = playerEffectManager.ReadySpell;
	    		
	    		if (lastSpell != null && lastSpell.Settings.Effects != null && lastSpell.Settings.Effects.Length > 0)
	    		{
	    			string lastSpellKey = lastSpell.Settings.Effects[0].Key;
	    			string lastSpellName = lastSpell.Settings.Name;
	    			
    				IEntityEffect lastSpellTemplate = GameManager.Instance.EntityEffectBroker.GetEffectTemplate(lastSpellKey);
    				DFCareer.MagicSkills spellSchool = lastSpellTemplate.Properties.MagicSkill;

    				RecordSpellCast(spellSchool, lastSpellName);

    				favoriteAlterationSpell = GetMostCastSpellBySchool(DFCareer.MagicSkills.Alteration);
    				favoriteDestructionSpell = GetMostCastSpellBySchool(DFCareer.MagicSkills.Destruction);
    				favoriteIllusionSpell = GetMostCastSpellBySchool(DFCareer.MagicSkills.Illusion);
    				favoriteMysticismSpell = GetMostCastSpellBySchool(DFCareer.MagicSkills.Mysticism);
    				favoriteRestorationSpell = GetMostCastSpellBySchool(DFCareer.MagicSkills.Restoration);
    				favoriteThaumaturgySpell = GetMostCastSpellBySchool(DFCareer.MagicSkills.Thaumaturgy);
	    		}
	    		
	    		spellsCast++;
	    		spellCastCounter = true;
	    	}

	    	if (spellCastCounter == true && GameManager.Instance.PlayerSpellCasting.IsPlayingAnim == false)
	    	{
	    		spellCastCounter = false;
	    	}

	    	// Crime Checks
	    	if (crimeCounter == false)
	    	{
	    		if (GameManager.Instance.PlayerEntity.CrimeCommitted == PlayerEntity.Crimes.Assault) { assaultCount++; crimeCounter = true; }
	    		if (GameManager.Instance.PlayerEntity.CrimeCommitted == PlayerEntity.Crimes.Attempted_Breaking_And_Entering) { attemptedBNECount++; crimeCounter = true; }
	    		if (GameManager.Instance.PlayerEntity.CrimeCommitted == PlayerEntity.Crimes.Breaking_And_Entering) { BNECount++; crimeCounter = true; }
	    		if (GameManager.Instance.PlayerEntity.CrimeCommitted == PlayerEntity.Crimes.Criminal_Conspiracy) { criminalConspiracyCount++; crimeCounter = true; }
	    		if (GameManager.Instance.PlayerEntity.CrimeCommitted == PlayerEntity.Crimes.High_Treason) { highTreasonCount++; crimeCounter = true; }
	    		if (GameManager.Instance.PlayerEntity.CrimeCommitted == PlayerEntity.Crimes.LoanDefault) { loanDefaultCount++; crimeCounter = true; }
	    		if (GameManager.Instance.PlayerEntity.CrimeCommitted == PlayerEntity.Crimes.Murder) { murderCount++; crimeCounter = true; }
	    		if (GameManager.Instance.PlayerEntity.CrimeCommitted == PlayerEntity.Crimes.Pickpocketing) { pickpocketCount++; crimeCounter = true; }
	    		if (GameManager.Instance.PlayerEntity.CrimeCommitted == PlayerEntity.Crimes.Piracy) { piracyCount++; crimeCounter = true; }
	    		if (GameManager.Instance.PlayerEntity.CrimeCommitted == PlayerEntity.Crimes.Smuggling) { smugglingCount++; crimeCounter = true; }
	    		if (GameManager.Instance.PlayerEntity.CrimeCommitted == PlayerEntity.Crimes.Tax_Evasion) { taxEvasionCount++; crimeCounter = true; }
	    		if (GameManager.Instance.PlayerEntity.CrimeCommitted == PlayerEntity.Crimes.Theft) { theftCount++; crimeCounter = true; }
	    		if (GameManager.Instance.PlayerEntity.CrimeCommitted == PlayerEntity.Crimes.Treason) { treasonCount++; crimeCounter = true; }
	    		if (GameManager.Instance.PlayerEntity.CrimeCommitted == PlayerEntity.Crimes.Trespassing) { trespassingCount++; crimeCounter = true; }
	    		if (GameManager.Instance.PlayerEntity.CrimeCommitted == PlayerEntity.Crimes.Vagrancy) { vagrancyCount++; crimeCounter = true; }
	    	}

	    	if (crimeCounter == true && GameManager.Instance.PlayerEntity.CrimeCommitted == PlayerEntity.Crimes.None)
	    	{
	    		crimeCounter = false;
	    	}
	    }

	    // -----------------------------------------------------------
	    // = FUNCTIONS                                               =
	    // -----------------------------------------------------------

	    // UI
	    public static void OpenStatsWindow()
	    {
	    	UserInterfaceManager uiManager = DaggerfallUI.Instance.UserInterfaceManager;	
    		DaggerStatsWindow statsWindow = new DaggerStatsWindow(DaggerfallUI.UIManager);
    		statsWindow.AllowCancel = true;
    		DaggerfallUI.UIManager.PushWindow(statsWindow);
	    }

	    // Calculations
	    public uint CalculateDaysPassed()
		{
			uint classicGameStartTime = 523530 / 1440; // 523530 is the in-game minute of the canonical game start time
			uint minutesPassed = DaggerfallUnity.Instance.WorldTime.DaggerfallDateTime.ToClassicDaggerfallTime();
			
			minutesPassed = minutesPassed / 1440;
			minutesPassed = minutesPassed - classicGameStartTime;
			daysPassed = minutesPassed;

			if (daysPassed >= 0)
			{
				return daysPassed;
			}
			return 0;
		}

		public uint CalculateYearsPassed(uint daysToCheck)
		{
			yearsPassed = daysToCheck / 365;

			if (yearsPassed < 1)
			{
				return 0;
			}
			return yearsPassed;
		}

		public uint CalculateLongestCrashSession()
		{
			if (crashSessions.Count == 0)
			{
				return 0;
			}

			uint maxValue = crashSessions.Max();
			return maxValue;
		}

		public void CalculateTimeSpentAsLycanthope(uint currentDay)
		{
			if (currentDay == startedLycanthropy)
			{
				daysAsLycanthrope = 1;
			}
			else if (startedLycanthropy != 0)
			{
				daysAsLycanthrope = (currentDay - startedLycanthropy) / 1440;
			}
			else
			{
				daysAsLycanthrope = 0;
			}
		}

		public void CalculateTimeSpentAsVampire(uint currentDay)
		{
			if (currentDay == startedVampirism)
			{
				daysAsVampire = 1;
			}
			else if (startedVampirism != 0)
			{
				daysAsVampire = (currentDay - startedVampirism) / 1440;
			}
			else
			{
				daysAsLycanthrope = 0;
			}
		}

		public void CalculateClimbingSwimmingLevitatingTimes()
		{
			Instance.timeSpentClimbing = GetFormattedTime(Instance.totalTimeClimbing);
			Instance.timeSpentSwimming = GetFormattedTime(Instance.totalTimeSwimming);
			Instance.timeSpentLevitating = GetFormattedTime(Instance.totalTimeLevitating);
		}

    	// public void SetupVisitedRegions()
    	// {
    	// 	visitedRegions = new List<int>();
    	// }

    	// public void SetupReputations()
    	// {
    	// 	reputationData = new Dictionary<string,int>();
    	// 	MapsFile mapsFile = new MapsFile();
    	// 	PersistentFactionData factionData = new PersistentFactionData();
    	// 	DFRegion dfRegion;
    	// 	string regionName;
    	// 	int regionRep;
    	// 	Debug.Log($"[DaggerStats] setup reputation data");

    	// 	foreach (int region in visitedRegions)
    	// 	{
    	// 		Debug.Log($"[DaggerStats] Checking Region # {region}");
    	// 		dfRegion = mapsFile.GetRegion(region);
    	// 		Debug.Log($"[DaggerStats] Region is {dfRegion}");
    	// 		regionName = dfRegion.Name;
    	// 		Debug.Log($"[DaggerStats] Region Name is {regionName}");
    	// 		regionRep = (int)factionData.GetReputation(region);
    	// 		Debug.Log($"[DaggerStats] Region Rep is {regionRep}");

    	// 		Debug.Log($"[DaggerStats] Adding {regionName} with rep of {regionRep} to reputationData");
    	// 		reputationData.Add(regionName, regionRep);
    	// 	}
    	// }

    	// public void CalculateReputations()
    	// {
    	// 	SetupReputations(); // force setup because visitedRegions is too slow to populate

    	// 	List<int> valueList = new List<int>();
    	// 	string bestReputationLocation = "None";
    	// 	string worstReputationLocation = "None";
    	// 	int bestReputationValue;
    	// 	int worstReputationValue;

    	// 	foreach (var pair in reputationData)
    	// 	{
    	// 		valueList.Add(pair.Value);
    	// 	}

    	// 	if (valueList.Count > 0)
    	// 	{
    	// 		bestReputationValue = valueList.Max();
    	// 		worstReputationValue = valueList.Min();

    	// 		foreach (var pair in reputationData)
    	// 		{
    	// 			if (pair.Value == bestReputationValue)
    	// 			{
    	// 				bestReputationLocation = pair.Key;
    	// 			}
    	// 			if (pair.Value == worstReputationValue)
    	// 			{
    	// 				worstReputationLocation = pair.Key;
    	// 			}
    	// 		}

    	// 		Debug.Log($"[DaggerStats] Best Reputation is {bestReputationLocation} with {bestReputationValue}");
    	// 		Debug.Log($"[DaggerStats] Worst Reputation is {worstReputationLocation} with {worstReputationValue}");
    	// 		Instance.bestReputation = bestReputationLocation + " (" + bestReputationValue.ToString() + ")";
    	// 		Instance.worstReputation = worstReputationLocation + " (" + worstReputationValue.ToString() + ")";
    	// 	}
    	// }

    	public void CalculateAccountTotal()
    	{
    		Instance.totalAccountBalance = 0;

    		foreach (int region in visitedRegions)
    		{
    			Instance.totalAccountBalance += (int)DaggerfallBankManager.GetAccountTotal(region);
    		}
    	}

    	public void CalculateHousesOwned()
    	{
    		if (owningHouse == false && DaggerfallBankManager.OwnsHouse == true)
    		{
    			Instance.housesOwned++;
    			Instance.owningHouse = true;
    		}
    	}

    	public void CalculateShipsOwned()
    	{
    		if (owningShip == false && DaggerfallBankManager.OwnsShip == true)
    		{
    			Instance.shipsBought++;
    			Instance.owningShip = true;
    		}
    	}

    	public void CalculateRegionsVisited()
		{
			if (Instance.visitedRegions != null)
			{
				Instance.regionsVisited = Instance.visitedRegions.Count();
			}
			else
			{
				Instance.regionsVisited = 0;
			}
		}

		public static void CalculateLibrariesVisited()
		{
			if (Instance.visitedLibraries != null)
			{
				Instance.libraryCards = Instance.visitedLibraries.Count();
			}
			else
			{
				Instance.libraryCards = 0;
			}
		}

    	public static void OnDepositGold(TransactionType type, TransactionResult result, int amount)
    	{
    		//Debug.Log($"[DaggerStats] gold deposit: {type} {result} {amount}");
    		if (result == TransactionResult.NONE)
    		{
    			if (type == TransactionType.Depositing_gold)
    			{
    				Instance.goldDeposited += amount;
    			}
    		}
    	}

    	public static void OnWithdrawGold(TransactionType type, TransactionResult result, int amount)
    	{
    		//Debug.Log($"[DaggerStats] gold withdrawal: {type} {result} {amount}");
    		if (result == TransactionResult.NONE)
    		{
    			if (type == TransactionType.Withdrawing_gold)
    			{
    				Instance.goldWithdrawn += amount;
    			}
    		}
    	}

    	public static void OnBorrowLoan(TransactionType type, TransactionResult result, int amount)
    	{
    		//Debug.Log($"[DaggerStats] Loan borrowing: {type} {result} {amount}");
    		if (result == TransactionResult.NONE)
    		{
    			if (type == TransactionType.Borrowing_loan)
    			{
    				Instance.goldBorrowed += amount;
    				int interest = amount / 10;
    				Instance.activeLoan = amount + interest;
    			}
    		}
    	}

    	public static void OnRepayLoan(TransactionType type, TransactionResult result, int amount)
    	{
    		Debug.Log($"[Stats+] Loan repaying: {type} {result} {Instance.activeLoan}");
    		if (result == TransactionResult.NONE)
    		{
    			if (type == TransactionType.Repaying_loan || type == TransactionType.Repaying_loan_from_account)
    			{
    				if (!DaggerfallBankManager.HasLoan(GameManager.Instance.PlayerGPS.CurrentRegionIndex))
    				{
    					Instance.goldRepaid += Instance.activeLoan;
    				}
    			}
    		}
    	}

    	public static void OnSellHouse(TransactionType type, TransactionResult result, int amount)
    	{
    		//Debug.Log($"[DaggerStats] house selling: {type} {result} {amount}");
    		if (result == TransactionResult.NONE)
    		{
    			if (type == TransactionType.Sell_house)
    			{
    				Instance.housesSold++;
    				Instance.owningHouse = false;
    				Instance.CalculateHousesOwned();
    			}
    		}
    	}

    	public static void OnSellShip(TransactionType type, TransactionResult result, int amount)
    	{
    		//Debug.Log($"[DaggerStats] ship selling: {type} {result} {amount}");
    		if (result == TransactionResult.NONE)
    		{
    			if (type == TransactionType.Sell_ship)
    			{
    				Instance.shipsSold++;
    				Instance.owningShip = false;
    				Instance.CalculateShipsOwned();
    			}
    		}
    	}

    	// public static void OnTeleportToCoordinates(DFPosition worldPos)
    	// {
    	// 	if (worldPos != null)
		// 	{
		// 		Debug.Log($"[DaggerStats] Player teleported.");
		// 	    Instance.teleportations++;
		// 	}
    		
    	// }

		public static void OnEnemyDeath(object sender, EventArgs e)
		{
			EnemyDeath enemyDeath = sender as EnemyDeath;
			if (enemyDeath != null)
			{
				DaggerfallEntityBehaviour entityBehaviour = enemyDeath.GetComponent<DaggerfallEntityBehaviour>();
				if (entityBehaviour != null)
				{
					EnemyEntity enemyEntity = entityBehaviour.Entity as EnemyEntity;
					if (enemyEntity != null)
					{
						// Check and Increment on all Enemies in the game
						if (enemyEntity.MobileEnemy.ID == (int)MobileTypes.Acrobat) { Instance.humanKills++; }
						else if (enemyEntity.MobileEnemy.ID == (int)MobileTypes.AncientLich) { Instance.undeadKills++; }
						else if (enemyEntity.MobileEnemy.ID == (int)MobileTypes.Archer) { Instance.humanKills++; }
						else if (enemyEntity.MobileEnemy.ID == (int)MobileTypes.Assassin) { Instance.humanKills++; }
						else if (enemyEntity.MobileEnemy.ID == (int)MobileTypes.Barbarian) { Instance.humanKills++; }
						else if (enemyEntity.MobileEnemy.ID == (int)MobileTypes.Bard) { Instance.humanKills++; }
						else if (enemyEntity.MobileEnemy.ID == (int)MobileTypes.Battlemage) { Instance.humanKills++; }
						else if (enemyEntity.MobileEnemy.ID == (int)MobileTypes.Burglar) { Instance.humanKills++; }
						else if (enemyEntity.MobileEnemy.ID == (int)MobileTypes.Centaur) { Instance.monsterKills++; }
						else if (enemyEntity.MobileEnemy.ID == (int)MobileTypes.DaedraLord) { Instance.daedraKills++; }
						else if (enemyEntity.MobileEnemy.ID == (int)MobileTypes.DaedraSeducer) { Instance.daedraKills++; }
						else if (enemyEntity.MobileEnemy.ID == (int)MobileTypes.Daedroth) { Instance.daedraKills++; }
						else if (enemyEntity.MobileEnemy.ID == (int)MobileTypes.Dragonling) { Instance.monsterKills++; }
						else if (enemyEntity.MobileEnemy.ID == (int)MobileTypes.Dreugh) { Instance.monsterKills++; }
						else if (enemyEntity.MobileEnemy.ID == (int)MobileTypes.FireAtronach) { Instance.atronachKills++; }
						else if (enemyEntity.MobileEnemy.ID == (int)MobileTypes.FireDaedra) { Instance.daedraKills++; }
						else if (enemyEntity.MobileEnemy.ID == (int)MobileTypes.FleshAtronach) { Instance.atronachKills++; }
						else if (enemyEntity.MobileEnemy.ID == (int)MobileTypes.FrostDaedra) { Instance.daedraKills++; }
						else if (enemyEntity.MobileEnemy.ID == (int)MobileTypes.Gargoyle) { Instance.monsterKills++; }
						else if (enemyEntity.MobileEnemy.ID == (int)MobileTypes.Ghost) { Instance.undeadKills++; }
						else if (enemyEntity.MobileEnemy.ID == (int)MobileTypes.Giant) { Instance.monsterKills++; }
						else if (enemyEntity.MobileEnemy.ID == (int)MobileTypes.GiantBat) { Instance.animalKills++; }
						else if (enemyEntity.MobileEnemy.ID == (int)MobileTypes.GiantScorpion) { Instance.animalKills++; }
						else if (enemyEntity.MobileEnemy.ID == (int)MobileTypes.GrizzlyBear) { Instance.animalKills++; }
						else if (enemyEntity.MobileEnemy.ID == (int)MobileTypes.Harpy) { Instance.monsterKills++; }
						else if (enemyEntity.MobileEnemy.ID == (int)MobileTypes.Healer) { Instance.humanKills++; }
						else if (enemyEntity.MobileEnemy.ID == (int)MobileTypes.IceAtronach) { Instance.atronachKills++; }
						else if (enemyEntity.MobileEnemy.ID == (int)MobileTypes.Imp) { Instance.monsterKills++; }
						else if (enemyEntity.MobileEnemy.ID == (int)MobileTypes.IronAtronach) { Instance.atronachKills++; }
						else if (enemyEntity.MobileEnemy.ID == (int)MobileTypes.Knight) { Instance.humanKills++; }
						else if (enemyEntity.MobileEnemy.ID == (int)MobileTypes.Knight_CityWatch) { Instance.humanKills++; }
						else if (enemyEntity.MobileEnemy.ID == (int)MobileTypes.Lamia) { Instance.monsterKills++; }
						else if (enemyEntity.MobileEnemy.ID == (int)MobileTypes.Lich) { Instance.undeadKills++; }
						else if (enemyEntity.MobileEnemy.ID == (int)MobileTypes.Mage) { Instance.humanKills++; }
						else if (enemyEntity.MobileEnemy.ID == (int)MobileTypes.Monk) { Instance.humanKills++; }
						else if (enemyEntity.MobileEnemy.ID == (int)MobileTypes.Mummy) { Instance.undeadKills++; }
						else if (enemyEntity.MobileEnemy.ID == (int)MobileTypes.Nightblade) { Instance.humanKills++; }
						else if (enemyEntity.MobileEnemy.ID == (int)MobileTypes.Nymph) { Instance.monsterKills++; }
						else if (enemyEntity.MobileEnemy.ID == (int)MobileTypes.Orc) { Instance.orcKills++; }
						else if (enemyEntity.MobileEnemy.ID == (int)MobileTypes.OrcSergeant) { Instance.orcKills++; }
						else if (enemyEntity.MobileEnemy.ID == (int)MobileTypes.OrcShaman) { Instance.orcKills++; }
						else if (enemyEntity.MobileEnemy.ID == (int)MobileTypes.OrcWarlord) { Instance.orcKills++; }
						else if (enemyEntity.MobileEnemy.ID == (int)MobileTypes.Ranger) { Instance.humanKills++; }
						else if (enemyEntity.MobileEnemy.ID == (int)MobileTypes.Rat) { Instance.animalKills++; }
						else if (enemyEntity.MobileEnemy.ID == (int)MobileTypes.Rogue) { Instance.humanKills++; }
						else if (enemyEntity.MobileEnemy.ID == (int)MobileTypes.SabertoothTiger) { Instance.animalKills++; }
						else if (enemyEntity.MobileEnemy.ID == (int)MobileTypes.SkeletalWarrior) { Instance.undeadKills++; }
						else if (enemyEntity.MobileEnemy.ID == (int)MobileTypes.Slaughterfish) { Instance.animalKills++; }
						else if (enemyEntity.MobileEnemy.ID == (int)MobileTypes.Sorcerer) { Instance.humanKills++; }
						else if (enemyEntity.MobileEnemy.ID == (int)MobileTypes.Spellsword) { Instance.humanKills++; }
						else if (enemyEntity.MobileEnemy.ID == (int)MobileTypes.Spider) { Instance.animalKills++; }
						else if (enemyEntity.MobileEnemy.ID == (int)MobileTypes.Spriggan) { Instance.monsterKills++; }
						else if (enemyEntity.MobileEnemy.ID == (int)MobileTypes.Thief) { Instance.humanKills++; }
						else if (enemyEntity.MobileEnemy.ID == (int)MobileTypes.Vampire) { Instance.undeadKills++; }
						else if (enemyEntity.MobileEnemy.ID == (int)MobileTypes.VampireAncient) { Instance.undeadKills++; }
						else if (enemyEntity.MobileEnemy.ID == (int)MobileTypes.Warrior) { Instance.humanKills++; }
						else if (enemyEntity.MobileEnemy.ID == (int)MobileTypes.Wereboar) { Instance.lycanthropeKills++; }
						else if (enemyEntity.MobileEnemy.ID == (int)MobileTypes.Werewolf) { Instance.lycanthropeKills++; }
						else if (enemyEntity.MobileEnemy.ID == (int)MobileTypes.Wraith) { Instance.undeadKills++; }
						else if (enemyEntity.MobileEnemy.ID == (int)MobileTypes.Zombie) { Instance.undeadKills++; }
					}
				}
			}
		}

		public static void OnMapPixelChanged(DFPosition mapPixel)
		{
			// Region
			CheckRegionData();

			// Capitals

			// LocationTypes
            CheckLocationTypeData();

		}

		public static void OnAssignBundle(LiveEffectBundle bundleAdded)
		{
			if (bundleAdded.bundleType == BundleTypes.Disease)
			{
				//Debug.Log($"[DaggerStats] Bundle is a disease from: {bundleAdded.casterEntityType}.");
				if (bundleAdded.casterEntityType == EntityTypes.Player)
				{
					Instance.diseasesContracted++;
					//Debug.Log($"[DaggerStats] Contracted new disease, incrementing to: {Instance.diseasesContracted}");
				}	
			}
			else
			{
				//Debug.Log($"[DaggerStats] Bundle is not a disease.");
			}
		}

		public static void OnQuestEnded(Quest quest)
		{
			//Debug.Log($"[DaggerStats] Quest Ended. Quest is: {quest.DisplayName} and the faction ID is {quest.FactionId}");

			if (quest != null)
			{
				CheckMainQuestCompletion(quest);
				IncrementOtherQuestCompletions(quest);

				Instance.isMainQuest = false;
			}
			else
			{
				Debug.LogWarning($"[Stats+] {quest.DisplayName} not found in any list!");
			}
		}

		public static void CheckRegionData()
		{
			int regionIndex = GameManager.Instance.PlayerGPS.CurrentRegionIndex;

			if (Instance.visitedRegions.Contains(regionIndex))
			{
				//Debug.Log($"[DaggerStats] Region already visited {regionIndex}");
				return;
			}
			else
			{
				Instance.visitedRegions.Add(regionIndex);
				//Debug.Log($"[DaggerStats] Adding {regionIndex} to list of visited Regions");
			}
		}

		

		public static void CheckLibraryData()
		{
			int libraryID = GameManager.Instance.PlayerEnterExit.BuildingDiscoveryData.buildingKey;
			// Debug.Log($"[DaggerStats] the ID is {libraryID} and visitedLibraries is {Instance.visitedLibraries}");

			if (Instance.visitedLibraries.Contains(libraryID))
			{
				//Debug.Log($"[DaggerStats] Library already visited {libraryID}");
				return;
			}
			else
			{
				Instance.visitedLibraries.Add(libraryID);
				//Debug.Log($"[DaggerStats] New Library Card: {libraryID} ");
			}
		}

		public static void CheckLocationTypeData()
		{
			DFLocation dfLocation = GameManager.Instance.PlayerGPS.CurrentLocation;
			int mapPixelID = MapsFile.GetMapPixelIDFromLongitudeLatitude((int)dfLocation.MapTableData.Longitude, dfLocation.MapTableData.Latitude);

			// Record each location visit
			Instance.RecordLocationVisit();

			if (GameManager.Instance.PlayerGPS.HasDiscoveredLocation(mapPixelID))
            {
                //Debug.Log($"[DaggerStats] This location has already been discovered.");
                return;
            }
            else
            {
            	IncrementLocationVisited();
            }
		}

		public static void CheckMainQuestCompletion(Quest quest)
		{
			foreach (string name in Instance.mainQuestNames)
			{
				//Debug.Log($"[DaggerStats] Checking Name {name}");
				if (quest.DisplayName == name)
				{
					if (GameManager.Instance.QuestMachine.IsQuestComplete(quest.UID))
					{
						//Debug.Log($"[DaggerStats] Adding {name} with UID: {quest.UID}");
						Instance.mainQuestIDs.Add(quest.UID);
						Instance.isMainQuest = true;
					}
				}
			}
		}

		public void CheckFallingDamage()
        {
            if (GameManager.Instance.StreamingWorld.PlayerTileMapIndex == 0)
            {
                return; // Doesn't count if you fall above water.
            }
            
            Transform playerTransform = GameManager.Instance.PlayerMotor.smoothFollower;
            float fallingDamageThreshold = 8.0f;
            float fallStartLevel = playerTransform.position.y;
			Vector3 groundPosition = GameManager.Instance.PlayerMotor.FindGroundPosition();
			float fallDistance = fallStartLevel - groundPosition.y;

            if (!GameManager.Instance.ClimbingMotor.IsClimbing || GameManager.Instance.ClimbingMotor.IsSlipping)
            { 
                if (fallDistance > fallingDamageThreshold || fallDistance == 0)
                {
                	if (GameManager.Instance.PlayerEntity.IsSlowFalling == false)
                	{
                		tarhielianExpeditions++;
                    	return;
                	}  
                }
            }
        }

        public void CheckForPacifiedEnemies()
		{
			List<PlayerGPS.NearbyObject> nearbyEnemyList = new List<PlayerGPS.NearbyObject>();
			nearbyEnemyList = GameManager.Instance.PlayerGPS.GetNearbyObjects(PlayerGPS.NearbyObjectFlags.Enemy, 14f);

			if (nearbyEnemyList.Count == 0)
			{
				return; 	
			}

			pacifyingEnemyCounter = true;
			
			foreach (var enemy in nearbyEnemyList)
			{
				GameObject enemyObject = enemy.gameObject;
				EnemyMotor motor;
				EnemySenses senses;
				DaggerfallEnemy unit;
				ulong unitID;
				
				motor = enemyObject.GetComponent(typeof(EnemyMotor)) as EnemyMotor;
				senses = enemyObject.GetComponent(typeof(EnemySenses)) as EnemySenses;
				unit = enemyObject.GetComponent(typeof(DaggerfallEnemy)) as DaggerfallEnemy;
				unitID = unit.LoadID;

				if (motor.IsHostile == false && senses.HasEncounteredPlayer == true)
				{
					if (unit.MobileUnit != null)
					{
						if (unit.MobileUnit.Enemy.ID == (int)MobileTypes.Centaur) 
						{
							if (!Instance.pacifiedEnemies.Contains(unitID))
							{
								Instance.pacifiedEnemies.Add(unitID); 
								Instance.centaursCalmed++; 
							}
						}
						else if (unit.MobileUnit.Enemy.ID == (int)MobileTypes.Daedroth || unit.MobileUnit.Enemy.ID == (int)MobileTypes.DaedraLord || unit.MobileUnit.Enemy.ID == (int)MobileTypes.DaedraSeducer || unit.MobileUnit.Enemy.ID == (int)MobileTypes.FireDaedra || unit.MobileUnit.Enemy.ID == (int)MobileTypes.FrostDaedra) 
						{
							if (!Instance.pacifiedEnemies.Contains(unitID))
							{ 
								Instance.pacifiedEnemies.Add(unitID); 
								Instance.daedraDeescalated++; 
							}
						}
						else if (unit.MobileUnit.Enemy.ID == (int)MobileTypes.Dragonling) 
						{
							if (!Instance.pacifiedEnemies.Contains(unitID))
							{
								Instance.pacifiedEnemies.Add(unitID); 
								Instance.dragonsDefused++;
							}
						}
						else if (unit.MobileUnit.Enemy.ID == (int)MobileTypes.Giant || unit.MobileUnit.Enemy.ID == (int)MobileTypes.Gargoyle)
						{
							if (!Instance.pacifiedEnemies.Contains(unitID))
							{
								Instance.pacifiedEnemies.Add(unitID); 
								Instance.giantsGentled++;
							}
						}
						else if (unit.MobileUnit.Enemy.ID == (int)MobileTypes.Harpy)
						{
							if (!Instance.pacifiedEnemies.Contains(unitID))
							{ 
								Instance.pacifiedEnemies.Add(unitID); 
								Instance.harpiesHushed++; 
							}
						}
						else if (unit.MobileUnit.Enemy.ID == (int)MobileTypes.Imp || unit.MobileUnit.Enemy.ID == (int)MobileTypes.Dreugh) 
						{
							if (!Instance.pacifiedEnemies.Contains(unitID))
							{ 
								Instance.pacifiedEnemies.Add(unitID); 
								Instance.impsImpressed++; 
							}
						}
						else if (unit.MobileUnit.Enemy.ID == (int)MobileTypes.Nymph || unit.MobileUnit.Enemy.ID == (int)MobileTypes.Lamia) 
						{
							if (!Instance.pacifiedEnemies.Contains(unitID))
							{ 
								Instance.pacifiedEnemies.Add(unitID); 
								Instance.nymphsNurtured++; 
							}
						}
						else if (unit.MobileUnit.Enemy.ID == (int)MobileTypes.Orc || unit.MobileUnit.Enemy.ID == (int)MobileTypes.OrcShaman || unit.MobileUnit.Enemy.ID == (int)MobileTypes.OrcWarlord || unit.MobileUnit.Enemy.ID == (int)MobileTypes.OrcSergeant) 
						{
							if (!Instance.pacifiedEnemies.Contains(unitID))
							{ 
								Instance.pacifiedEnemies.Add(unitID); 
								Instance.orcsOffset++; 
							}
						}
						else if (unit.MobileUnit.Enemy.ID == (int)MobileTypes.Spriggan) 
						{
							if (!Instance.pacifiedEnemies.Contains(unitID))
							{ 
								Instance.pacifiedEnemies.Add(unitID); 
								Instance.spriggansShushed++; 
							}
						}
					}
				}
			}
			pacifyingEnemyCounter = false;
		}

		public void SetupMainQuests()
		{
			mainQuestNames.Add("Lady Brisienna");
			mainQuestNames.Add("Missing Prince");
			mainQuestNames.Add("Blackmailing Prince Helseth");
			mainQuestNames.Add("Freeing Medora");
			mainQuestNames.Add("Morgiah's Wedding");
			mainQuestNames.Add("Stronghold of the Blades");
			mainQuestNames.Add("Concern for Nulfaga");
			mainQuestNames.Add("Elysana's Robe");
			mainQuestNames.Add("The Werebeast");
			mainQuestNames.Add("Who Gets the Totem");
			mainQuestNames.Add("Elysana's Betrayal");
			mainQuestNames.Add("Mantella Revealed");
			mainQuestNames.Add("Barenziah's Book");
			mainQuestNames.Add("The Emperor's Courier");
			mainQuestNames.Add("Mynisera's Letters");
			mainQuestNames.Add("Lysandus' Revenge");
			mainQuestNames.Add("Journey to Aetherius");
			mainQuestNames.Add("Wayrest Painting");
			mainQuestNames.Add("Dust of Restful Death");
			mainQuestNames.Add("Orcish Treaty");
			mainQuestNames.Add("Lich's Soul");
			mainQuestNames.Add("Lysandus' Revelation");
		}

		public void SetupQuestFactionIdLists()
		{
			// This might be the ugliest thing I've ever written. I'm sorry.
			//   People             Court         Specific Nobles
			peopleOf.Add(194); courtOf.Add(195); courtOf.Add(242);
			peopleOf.Add(198); courtOf.Add(244); courtOf.Add(352);
			peopleOf.Add(504); courtOf.Add(246); courtOf.Add(355);
			peopleOf.Add(507); courtOf.Add(499); courtOf.Add(357);
			peopleOf.Add(517); courtOf.Add(503); courtOf.Add(359);
			peopleOf.Add(518); courtOf.Add(506); courtOf.Add(360);
			peopleOf.Add(520); courtOf.Add(509); courtOf.Add(361);
			peopleOf.Add(522); courtOf.Add(516); courtOf.Add(363);
			peopleOf.Add(523); courtOf.Add(519); courtOf.Add(364);
			peopleOf.Add(524); courtOf.Add(521); courtOf.Add(365);
			peopleOf.Add(525); courtOf.Add(527); courtOf.Add(366);
			peopleOf.Add(526); courtOf.Add(529); courtOf.Add(367);
			peopleOf.Add(528); courtOf.Add(531); courtOf.Add(369);
			peopleOf.Add(530); courtOf.Add(533); courtOf.Add(370);
			peopleOf.Add(532); courtOf.Add(535); courtOf.Add(371);
			peopleOf.Add(534); courtOf.Add(537); courtOf.Add(373);
			peopleOf.Add(536); courtOf.Add(539); courtOf.Add(374);
			peopleOf.Add(538); courtOf.Add(541); courtOf.Add(375);
			peopleOf.Add(538); courtOf.Add(543); courtOf.Add(376);
			peopleOf.Add(538); courtOf.Add(545); courtOf.Add(377);
			peopleOf.Add(538); courtOf.Add(547); courtOf.Add(378);
			peopleOf.Add(540); courtOf.Add(549); courtOf.Add(379);
			peopleOf.Add(542); courtOf.Add(551); courtOf.Add(380);
			peopleOf.Add(544); courtOf.Add(553); courtOf.Add(381);
			peopleOf.Add(546); courtOf.Add(555); courtOf.Add(382);
			peopleOf.Add(548); courtOf.Add(557); courtOf.Add(383);
			peopleOf.Add(550); courtOf.Add(559); courtOf.Add(384);
			peopleOf.Add(552); courtOf.Add(561); courtOf.Add(385);
			peopleOf.Add(554); courtOf.Add(563); courtOf.Add(386);
			peopleOf.Add(556); courtOf.Add(565); courtOf.Add(387);
			peopleOf.Add(558); courtOf.Add(567); courtOf.Add(388);
			peopleOf.Add(560); courtOf.Add(569); courtOf.Add(389);
			peopleOf.Add(562); courtOf.Add(571); courtOf.Add(390);
			peopleOf.Add(564); courtOf.Add(573); courtOf.Add(391);
			peopleOf.Add(566); courtOf.Add(575); courtOf.Add(392);
			peopleOf.Add(568); courtOf.Add(577); courtOf.Add(393);
			peopleOf.Add(570); courtOf.Add(579); courtOf.Add(394);
			peopleOf.Add(572); courtOf.Add(581); courtOf.Add(395);
			peopleOf.Add(574); courtOf.Add(583); courtOf.Add(396);
			peopleOf.Add(576); courtOf.Add(592); courtOf.Add(397);
			peopleOf.Add(578); courtOf.Add(595); courtOf.Add(398);
			peopleOf.Add(580); courtOf.Add(596); courtOf.Add(399);
			peopleOf.Add(582); courtOf.Add(597); courtOf.Add(400);
			peopleOf.Add(584); courtOf.Add(598); courtOf.Add(401);
			peopleOf.Add(590);					 courtOf.Add(402);
			peopleOf.Add(593);					 courtOf.Add(403);
			peopleOf.Add(599);					 courtOf.Add(404);
												 courtOf.Add(405);
												 courtOf.Add(406);
												 courtOf.Add(407);
												 courtOf.Add(500);
												 courtOf.Add(501);
												 courtOf.Add(852);
		}

		public void SetupSpellCount()
	    {
	    	
	    	spellData = new Dictionary<DFCareer.MagicSkills, Dictionary<string, SpellCastTracker>>();

	    	foreach (DFCareer.MagicSkills skill in System.Enum.GetValues(typeof(DFCareer.MagicSkills)))
	        {
	            spellData.Add(skill, new Dictionary<string, SpellCastTracker>());
	        }
	    }

	    public void SetupWeaponCount()
	    {
	    	weaponData = new Dictionary<string, WeaponUseTracker>();
	    }

	    public void SetupLocationCount()
	    {
	    	locationData = new Dictionary<string, LocationVisitTracker>();
	    }

		public static void IncrementOtherQuestCompletions(Quest quest)
		{
			if (quest != null && Instance.isMainQuest == false)
			{
				if (quest.FactionId == 108) { Instance.darkBrotherhoodQuestsCompleted++; }
				else if (quest.FactionId == 41) { Instance.fightersGuildQuestsCompleted++; }
				else if (quest.FactionId == 40) { Instance.magesGuildQuestsCompleted++; }
				else if (quest.FactionId == 42) { Instance.thievesGuildQuestsCompleted++; }
				else if (quest.FactionId == 21 || quest.FactionId == 22 || (quest.FactionId >= 24 && quest.FactionId <= 27) || quest.FactionId == 29 || quest.FactionId == 33 || quest.FactionId == 35 || quest.FactionId == 36) { Instance.templeQuestsCompleted++; }
				else if ((quest.FactionId >= 36 && quest.FactionId <= 37) || (quest.FactionId >= 82 && quest.FactionId <= 85) || (quest.FactionId >= 88 && quest.FactionId <= 95) || quest.FactionId == 98 || quest.FactionId == 99 || quest.FactionId == 106 || quest.FactionId == 107) { Instance.templeOrderQuestsCompleted++; }
				else if ((quest.FactionId >= 408 && quest.FactionId <= 418) || quest.FactionId == 368) { Instance.knightOrderQuestsCompleted++; }
				else if (quest.FactionId == 510) { Instance.merchantQuestsCompleted++; }
				else if (Instance.peopleOf.Contains(quest.FactionId)) { Instance.commonerQuestsCompleted++; }
				else if (Instance.courtOf.Contains(quest.FactionId)) { Instance.nobilityQuestsCompleted++; }
				else if (quest.FactionId == 512) { Instance.prostituteQuestsCompleted++; }
				else if (quest.FactionId >= 419 && quest.FactionId <= 432) { Instance.witchCovenQuestsCompleted++; }
				else if (quest.FactionId >= 150 && quest.FactionId <= 158) { Instance.vampireClanQuestsCompleted++; }
				// cure
				else if (quest.FactionId >= 1 && quest.FactionId <= 17) { Instance.daedricQuestsCompleted++; }
				
			}
		}

		public static void IncrementLocationVisited()
		{
			DFLocation dfLocation = GameManager.Instance.PlayerGPS.CurrentLocation;
			DFRegion.LocationTypes locationType = dfLocation.MapTableData.LocationType;

			if (locationType == DFRegion.LocationTypes.TownCity) { Instance.townsVisited++; } 
			if (locationType == DFRegion.LocationTypes.TownHamlet) { Instance.townsVisited++; }
			if (locationType == DFRegion.LocationTypes.TownVillage) { Instance.townsVisited++; }
			if (locationType == DFRegion.LocationTypes.Tavern) { Instance.townsVisited++; }
			if (locationType == DFRegion.LocationTypes.HomeFarms) { Instance.homesVisited++; }
			if (locationType == DFRegion.LocationTypes.HomePoor) { Instance.homesVisited++; } 
			if (locationType == DFRegion.LocationTypes.HomeWealthy) { Instance.homesVisited++; }
			if (locationType == DFRegion.LocationTypes.DungeonKeep) { Instance.dungeonsVisited++; }
			if (locationType == DFRegion.LocationTypes.DungeonLabyrinth) { Instance.dungeonsVisited++; }
			if (locationType == DFRegion.LocationTypes.DungeonRuin) { Instance.dungeonsVisited++; } 
			if (locationType == DFRegion.LocationTypes.Graveyard) { Instance.dungeonsVisited++; }
			if (locationType == DFRegion.LocationTypes.ReligionTemple) { Instance.templesVisited++; } 
			if (locationType == DFRegion.LocationTypes.ReligionCult) { Instance.templesVisited++; }
		}

		public void SpellCount()
		{
			PlayerEntity playerEntity = GameManager.Instance.PlayerEntity;
			if (playerEntity.SpellbookCount() != 0)
			{
				spellsLearned = playerEntity.SpellbookCount();
			}
		}

		public void RecordSpellCast(DFCareer.MagicSkills magicSchool, string spellEffectKey)
		{
			Dictionary<string, SpellCastTracker> schoolSpells;
			if (spellData.TryGetValue(magicSchool, out schoolSpells))
			{
				SpellCastTracker tracker;
				if (schoolSpells.TryGetValue(spellEffectKey, out tracker))
				{
					tracker.IncrementCastCount();
					//Debug.Log($"[DaggerStats] Incremented {spellEffectKey} for {magicSchool}. New count: {tracker.castCount}");
				}
				else
				{
					SpellCastTracker newTracker = new SpellCastTracker(spellEffectKey);
					newTracker.IncrementCastCount();
					schoolSpells.Add(spellEffectKey, newTracker);
					//Debug.Log($"[DaggerStats] Added new spell {spellEffectKey} to {magicSchool}. Count: {newTracker.castCount}");
				}
			}
			else
	        {
	            // This should ideally not happen if InitializeSpellAnalytics covers all schools.
	            Debug.LogWarning($"[Stats+] Attempted to record spell for uninitialized magic school: {magicSchool}");
	        }
		}

		public void RecordWeaponUse(string weaponUsed)
		{
			WeaponUseTracker tracker;
			if (weaponData.TryGetValue(weaponUsed, out tracker))
			{
				tracker.IncrementUseCount();
				//Debug.Log($"[DaggerStats] Incremented {weaponUsed} to {tracker.useCount}");
			}
			else
			{
				WeaponUseTracker newTracker = new WeaponUseTracker(weaponUsed);
				newTracker.IncrementUseCount();
				weaponData.Add(weaponUsed, newTracker);
				//Debug.Log($"[DaggerStats] Added {weaponUsed} to weaponData. Count: {newTracker.useCount}");
			}
		}

		public void RecordLocationVisit()
		{
			string locationName = GameManager.Instance.PlayerGPS.CurrentLocation.Name;

			if (string.IsNullOrEmpty(locationName))
		    {
		        return;
		    }

			LocationVisitTracker tracker;
			if (locationData.TryGetValue(locationName, out tracker))
			{
				tracker.IncrementVisitCount();
				//Debug.Log($"[DaggerStats] Incremented {locationName} to {tracker.visitCount}");
			}
			else
			{
				LocationVisitTracker newTracker = new LocationVisitTracker(locationName);
				newTracker.IncrementVisitCount();
				locationData.Add(locationName, newTracker);
				//Debug.Log($"[DaggerStats] Added {locationName} to locationData. Count: {newTracker.visitCount}");
			}
		}

		public string GetMostCastSpellBySchool(DFCareer.MagicSkills magicSchool)
		{
			Dictionary<string, SpellCastTracker> schoolSpells;
			if (spellData.TryGetValue(magicSchool, out schoolSpells))
			{
				SpellCastTracker mostCastSpell = null;
				int maxCount = 0;

				foreach (var pair in schoolSpells)
				{
					if (pair.Value.castCount > maxCount)
					{
						maxCount = pair.Value.castCount;
						mostCastSpell = pair.Value;
					}
				}

				if (mostCastSpell != null)
				{
					return mostCastSpell.spellEffectKey;
				}
				else
				{
					return "None";
				}
			}
			return null;
		}

		public string GetMostUsedWeapon()
		{
			WeaponUseTracker mostUsedWeapon = null;
			int maxCount = 0;

			foreach (var pair in weaponData)
			{
				if (pair.Value.useCount > maxCount)
				{
					maxCount = pair.Value.useCount;
					mostUsedWeapon = pair.Value;
				}
			}

			if (mostUsedWeapon != null)
			{
				return mostUsedWeapon.weaponName;
			}
			else
			{
				return "None";
			}
		}

		public string GetMostVisitedLocation()
		{
			LocationVisitTracker mostVisitedLocation = null;
			int maxCount = 0;

			foreach (var pair in locationData)
			{
				if (pair.Value.visitCount > maxCount)
				{
					maxCount = pair.Value.visitCount;
					mostVisitedLocation = pair.Value;
				}
			}

			if (mostVisitedLocation != null)
			{
				return mostVisitedLocation.locationName;
			}
			else
			{
				return "None";
			}
		}

		public void GetMainQuestsCompleted()
		{
			if (Instance.mainQuestIDs != null)
			{
				Instance.mainQuestCompleted = Instance.mainQuestIDs.Count();
			}
			else
			{
				Instance.mainQuestCompleted = 0;
			}
		}

		public string GetFormattedTime(float timeToCalculate)
        {
            int totalSeconds = Mathf.FloorToInt(timeToCalculate);
            int hours = totalSeconds / 3600;
            int minutes = (totalSeconds % 3600) / 60;
            int seconds = totalSeconds % 60;

            List<string> parts = new List<string>();
            if (hours > 0)
                parts.Add($"{hours}h");
            if (minutes > 0 || hours > 0)
                parts.Add($"{minutes}m");
            parts.Add($"{seconds}s");

            return string.Join(" ", parts);
        }

		// -----------------------------------------------------------
	    // = CLASSES                                                 =
	    // -----------------------------------------------------------

		public class SpellCastTracker
		{
			public string spellEffectKey { get; private set; }
			public int castCount { get; private set; }

			public SpellCastTracker(string SpellEffectKey)
			{
				spellEffectKey = SpellEffectKey;
				castCount = 0;
			}

			public void IncrementCastCount()
			{
				castCount++;
			}
		}

		public class WeaponUseTracker
		{
			public string weaponName { get; private set; }
			public int useCount { get; private set; }

			public WeaponUseTracker(string WeaponName)
			{
				weaponName = WeaponName;
				useCount = 0;
			}

			public void IncrementUseCount()
			{
				useCount++;
			}
		}

		public class LocationVisitTracker
		{
			public string locationName { get; private set; }
			public int visitCount { get; private set; }

			public LocationVisitTracker(string LocationName)
			{
				locationName = LocationName;
				visitCount = 0;
			}

			public void IncrementVisitCount()
			{
				visitCount++;
			}
		}
	}
}