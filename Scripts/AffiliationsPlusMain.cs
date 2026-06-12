// Affiliations Plus Mod
// by rithessa at Nexus Mods

using UnityEngine;
using DaggerfallWorkshop;
using DaggerfallWorkshop.Game;
using DaggerfallWorkshop.Game.Entity;
using DaggerfallWorkshop.Game.Utility.ModSupport;
using DaggerfallWorkshop.Game.Utility.ModSupport.ModSettings;
using DaggerfallWorkshop.Game.Utility;
using DaggerfallWorkshop.Game.UserInterface;
using DaggerfallWorkshop.Game.UserInterfaceWindows;
using DaggerfallWorkshop.Utility;
using DaggerfallConnect.Arena2;
using DaggerfallWorkshop.Game.Guilds;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;


namespace AffiliationsPlus
{
	public class AffiliationsPlusMain : MonoBehaviour
	{
		private static Mod mod;
		private static AffiliationsPlusMain instance;
		public static AffiliationsPlusMain Instance { get; private set; }
        public static IUserInterfaceManager uiManager = DaggerfallUI.UIManager;
        public static List<TextFile.Token> guildsAndLocalsTokens = new List<TextFile.Token>();
        public static List<TextFile.Token> religiousTokens = new List<TextFile.Token>();
        public static List<TextFile.Token> covensAndClansTokens = new List<TextFile.Token>();
        public static List<TextFile.Token> notablePeopleTokens = new List<TextFile.Token>();
        public static List<TextFile.Token> knightsTokens = new List<TextFile.Token>();
        public static PlayerEntity playerEntity = GameManager.Instance.PlayerEntity;
        public static FactionFile factionFile = new FactionFile();
        public static TextFile.Token tab = new TextFile.Token();
        public static TextFile.Token tab1 = new TextFile.Token();
        public static TextFile.Token tab2 = new TextFile.Token();
        public static TextFile.Token tab3 = new TextFile.Token(); 

        // Mod Settings
        public static bool useRawNumbers { get; set; }
        public static float textScale { get; set; }
        public static bool overrideDefaultSettings { get; set; }
        public static int twoColumnTabSetting { get; set; }
        public static int fourColumnTabSetting1 { get; set; }
        public static int fourColumnTabSetting2 { get; set; }
        public static int fourColumnTabSetting3 { get; set; }

		// INIT
		[Invoke(StateManager.StateTypes.Start, -1)]
	    public static void Init(InitParams initParams)
	    {
	    	mod = initParams.Mod;
	    	instance = new GameObject("AffiliationsPlus").AddComponent<AffiliationsPlusMain>();

            // Tab Formatting
            if (overrideDefaultSettings == true)
            {
                tab = TextFile.TabToken;
                tab.x = twoColumnTabSetting;
                tab1.formatting = TextFile.Formatting.PositionPrefix;
                tab1.x = fourColumnTabSetting1;
                tab2.formatting = TextFile.Formatting.PositionPrefix;
                tab2.x = fourColumnTabSetting2;
                tab3.formatting = TextFile.Formatting.PositionPrefix;
                tab3.x = fourColumnTabSetting3;
            }

            if (overrideDefaultSettings == false)
            {
                tab = TextFile.TabToken;
                tab.x = 125;
                tab1.formatting = TextFile.Formatting.PositionPrefix;
                tab1.x = 70;
                tab2.formatting = TextFile.Formatting.PositionPrefix;
                tab2.x = 125;
                tab3.formatting = TextFile.Formatting.PositionPrefix;
                tab3.x = 180;
            }

            // Faction File Data
            factionFile = null;
            if (DaggerfallUnity.Instance != null && DaggerfallUnity.Instance.ContentReader != null)
            {
                FieldInfo factionFileReaderField = typeof(DaggerfallWorkshop.Utility.ContentReader).GetField("factionFileReader", BindingFlags.Instance | BindingFlags.NonPublic);
                if (factionFileReaderField != null)
                {
                    factionFile = factionFileReaderField.GetValue(DaggerfallUnity.Instance.ContentReader) as FactionFile;
                }
            }

            if (factionFile == null)
            {
                Debug.LogError("[Stats+] FactionFile not available. Cannot process any additional factions.");
                return;
            }

            // Mod Settings
            mod.LoadSettingsCallback = LoadSettings;

	    	Debug.Log($"[Stats+] Started with Shift+A keybind for Affiliations menu.");
	    }

        private static void LoadSettings(ModSettings modSettings, ModSettingsChange change)
        {
            useRawNumbers = mod.GetSettings().GetValue<bool>("ReputationDisplay", "useRawNumbers");
            textScale = mod.GetSettings().GetValue<float>("TextDisplay", "textScale");
            overrideDefaultSettings = mod.GetSettings().GetValue<bool>("TextDisplay", "overrideDefaultSettings");
            twoColumnTabSetting = mod.GetSettings().GetValue<int>("TextDisplay", "twoColumnTabSetting");
            fourColumnTabSetting1 = mod.GetSettings().GetValue<int>("TextDisplay", "fourColumnTabSetting1");
            fourColumnTabSetting2 = mod.GetSettings().GetValue<int>("TextDisplay", "fourColumnTabSetting2");
            fourColumnTabSetting3 = mod.GetSettings().GetValue<int>("TextDisplay", "fourColumnTabSetting3");
        }

		public static void ReplaceAffiliations()
		{
            if (overrideDefaultSettings == true)
            {
                tab = TextFile.TabToken;
                tab.x = twoColumnTabSetting;
                tab1.formatting = TextFile.Formatting.PositionPrefix;
                tab1.x = fourColumnTabSetting1;
                tab2.formatting = TextFile.Formatting.PositionPrefix;
                tab2.x = fourColumnTabSetting2;
                tab3.formatting = TextFile.Formatting.PositionPrefix;
                tab3.x = fourColumnTabSetting3;
            }

            guildsAndLocalsTokens.Clear();
			GetGuildMemberships();
            GetLocalReputation();
            GetClassReputations();

            knightsTokens.Clear();
            GetKnightsTokens();

            religiousTokens.Clear();
            GetReligiousReputations();

            covensAndClansTokens.Clear();
            GetCovensAndClansReputations();

            notablePeopleTokens.Clear();
            GetNotablePeopleTokens();

            var affiliationsWindow = new AffiliationsPlusWindow(uiManager);
            uiManager.PushWindow(affiliationsWindow);
        }

        public static void GetGuildMemberships()
        {
            List<IGuild> guildMemberships = GameManager.Instance.GuildManager.GetMemberships();

            guildsAndLocalsTokens.Add(TextFile.CreateTextToken(" "));
            guildsAndLocalsTokens.Add(TextFile.NewLineToken);
            guildsAndLocalsTokens.Add(new TextFile.Token() 
            {
                text = TextManager.Instance.GetLocalizedText("affiliation"),
                formatting = TextFile.Formatting.TextHighlight
            });
            guildsAndLocalsTokens.Add(tab);
            guildsAndLocalsTokens.Add(new TextFile.Token()
            {
                text = TextManager.Instance.GetLocalizedText("rank"),
                formatting = TextFile.Formatting.TextHighlight
            });
            guildsAndLocalsTokens.Add(TextFile.NewLineToken);

            if (guildMemberships.Count == 0)
            {
                guildsAndLocalsTokens.Add(new TextFile.Token() { text = "You do not belong to any guilds or groups.", formatting = TextFile.Formatting.TextQuestion });
                guildsAndLocalsTokens.Add(TextFile.NewLineToken);
                return;
            }

            foreach (IGuild guild in guildMemberships)
            {
                string affiliationText = string.Format(TextManager.Instance.GetLocalizedText("affiliationFormatString"), guild.GetTitle(), guild.GetReputation(playerEntity).ToString());
                guildsAndLocalsTokens.Add(TextFile.CreateTextToken(guild.GetAffiliation()));
                guildsAndLocalsTokens.Add(tab);
                guildsAndLocalsTokens.Add(TextFile.CreateTextToken(affiliationText));
                guildsAndLocalsTokens.Add(TextFile.NewLineToken);
            }
        }

        public static void GetLocalReputation()
        {
            guildsAndLocalsTokens.Add(TextFile.NewLineToken);
            guildsAndLocalsTokens.Add(new TextFile.Token() { text = "Local Reputation", formatting = TextFile.Formatting.TextHighlight });
            guildsAndLocalsTokens.Add(tab);
            guildsAndLocalsTokens.Add(new TextFile.Token() { text = "Reputation", formatting = TextFile.Formatting.TextHighlight });
            guildsAndLocalsTokens.Add(TextFile.NewLineToken);

            if (GameManager.Instance.PlayerGPS != null)
            {
                string currentRegionName = GameManager.Instance.PlayerGPS.CurrentRegion.Name;
                //Debug.Log($"[Stats+] Raw Current Region Name: '{currentRegionName}'");

                // Check for special dumb cases
                int peopleOfRegionFactionId = 0;
                int courtOfRegionFactionId = 0;
                if (currentRegionName == "Alik'r Desert") { currentRegionName = "Alikra"; peopleOfRegionFactionId = 590; courtOfRegionFactionId = 509; }
                if (currentRegionName == "Dragontail Mountains") { currentRegionName = "Dragontail"; peopleOfRegionFactionId = 507; courtOfRegionFactionId = 506;}
                if (currentRegionName == "Wrothgarian Mountains") { currentRegionName = "Wrothgaria"; peopleOfRegionFactionId = 504; courtOfRegionFactionId = 503; }
                if (currentRegionName == "Orsinium Area") { currentRegionName = "Orsinium"; peopleOfRegionFactionId = 599; courtOfRegionFactionId = 598; }
                if (currentRegionName == "Isle of Balfiera") { currentRegionName = "Isle of Balfiera"; peopleOfRegionFactionId = 198; courtOfRegionFactionId = 244;}

                string peopleFactionName = "People of " + currentRegionName;
                if (peopleOfRegionFactionId == 0)
                {
                    peopleOfRegionFactionId = factionFile.GetFactionID(peopleFactionName);
                }

                if (peopleOfRegionFactionId > 0)
                {
                    FactionFile.FactionData peopleFactionData;
                    if (factionFile.GetFactionData(peopleOfRegionFactionId, out peopleFactionData))
                    {
                        int reputation = GameManager.Instance.PlayerEntity.FactionData.GetReputation(peopleFactionData.id);
                        string reputationRank = AffiliationsPlus.ReputationText.GetReputationRankString(reputation);

                        if (useRawNumbers)
                        {
                            reputationRank = reputation.ToString();
                        }

                        guildsAndLocalsTokens.Add(TextFile.CreateTextToken(peopleFactionData.name));
                        guildsAndLocalsTokens.Add(tab);
                        guildsAndLocalsTokens.Add(TextFile.CreateTextToken(reputationRank));
                        guildsAndLocalsTokens.Add(TextFile.NewLineToken);
                        //Debug.Log($"[Stats+] Added dynamic faction: {peopleFactionData.name} (ID: {peopleFactionData.id}, Rep: {reputation}, Rank: {reputationRank})");
                    }
                }
                else
                {
                    Debug.LogWarning($"[Stats+] Could NOT find 'People of' faction for '{currentRegionName}'. (ID: {peopleOfRegionFactionId})");
                }

                string courtFactionName = "Court of " + currentRegionName;
                if (courtOfRegionFactionId == 0)
                {
                    courtOfRegionFactionId = factionFile.GetFactionID(courtFactionName);
                }

                if (courtOfRegionFactionId > 0)
                {
                    FactionFile.FactionData courtFactionData;
                    if (factionFile.GetFactionData(courtOfRegionFactionId, out courtFactionData))
                    {
                        int reputation = GameManager.Instance.PlayerEntity.FactionData.GetReputation(courtFactionData.id);
                        string reputationRank = AffiliationsPlus.ReputationText.GetReputationRankString(reputation);

                        if (useRawNumbers)
                        {
                            reputationRank = reputation.ToString();
                        }

                        guildsAndLocalsTokens.Add(TextFile.CreateTextToken(courtFactionData.name));
                        guildsAndLocalsTokens.Add(tab);
                        guildsAndLocalsTokens.Add(TextFile.CreateTextToken(reputationRank));
                        guildsAndLocalsTokens.Add(TextFile.NewLineToken);
                        //Debug.Log($"[Stats+] Added dynamic faction: {courtFactionData.name} (ID: {courtFactionData.id}, Rep: {reputation}, Rank: {reputationRank})");
                    }
                }
                else
                {
                    Debug.LogWarning($"[Stats+] Could NOT find 'Court of' faction for '{currentRegionName}'. (ID: {courtOfRegionFactionId})");
                }

                // LegalRep Implementation
                if (GameManager.Instance.PlayerEntity != null && GameManager.Instance.PlayerGPS != null)
                {
                    int currentRegionDataIndex = GameManager.Instance.PlayerGPS.CurrentRegionIndex;

                    if (currentRegionDataIndex >= 0 && currentRegionDataIndex < GameManager.Instance.PlayerEntity.RegionData.Length)
                    {
                        short legalRep = GameManager.Instance.PlayerEntity.RegionData[currentRegionDataIndex].LegalRep;
                        string legalRepRank = AffiliationsPlus.ReputationText.GetLegalReputationRankString(legalRep);

                        if (useRawNumbers)
                        {
                            legalRepRank = legalRep.ToString();
                        }

                        guildsAndLocalsTokens.Add(TextFile.CreateTextToken("Legal Standing"));
                        guildsAndLocalsTokens.Add(tab);
                        guildsAndLocalsTokens.Add(TextFile.CreateTextToken(legalRepRank));
                        guildsAndLocalsTokens.Add(TextFile.NewLineToken);
                        //Debug.Log($"[Stats+] Added Legal Standing: (Rep: {legalRep}, Rank: {legalRepRank}) for region data index {currentRegionDataIndex}");
                    }
                    else
                    {
                        Debug.LogWarning($"[Stats+] CurrentRegionDataIndex ({currentRegionDataIndex}) is out of bounds for PlayerEntity.RegionData array. Cannot display Legal Standing.");
                    }
                }
                else
                {
                    Debug.LogWarning("[Stats+] PlayerEntity or PlayerGPS not available. Cannot display Legal Standing.");
                }
            }

            else
            {
                Debug.LogWarning("[Stats+] PlayerGPS or CurrentRegion not loaded for dynamic faction lookup.");
            }
        }

        public static void GetClassReputations()
        {
            guildsAndLocalsTokens.Add(TextFile.NewLineToken);
            guildsAndLocalsTokens.Add(new TextFile.Token() { text = "Class Reputation", formatting = TextFile.Formatting.TextHighlight });
            guildsAndLocalsTokens.Add(tab);
            guildsAndLocalsTokens.Add(new TextFile.Token() { text = "Reputation", formatting = TextFile.Formatting.TextHighlight });
            guildsAndLocalsTokens.Add(TextFile.NewLineToken);

            System.Collections.Generic.List<int> classFactionIDs = new System.Collections.Generic.List<int>()
            {
                510, // The_Merchants
                60,  // The_Scholars
                61,  // The_Nobility
                802, // The Crafters
                512, // The_Prostitutes
                511, // The Bards
                514, // The Children
            };

            PopulateTokens(guildsAndLocalsTokens, classFactionIDs, false);
        }

        public static void GetReligiousReputations()
        {
            religiousTokens.Add(TextFile.NewLineToken);
            religiousTokens.Add(new TextFile.Token() { text = "The Gods", formatting = TextFile.Formatting.TextQuestion });
            religiousTokens.Add(TextFile.NewLineToken);
            religiousTokens.Add(new TextFile.Token() { text = "Aedra", formatting = TextFile.Formatting.TextHighlight });
            religiousTokens.Add(tab);
            religiousTokens.Add(new TextFile.Token() { text = "Opinion of You", formatting = TextFile.Formatting.TextHighlight });
            religiousTokens.Add(TextFile.NewLineToken);

            System.Collections.Generic.List<int> aedraFactionIDs = new System.Collections.Generic.List<int>()
            {
                21, // Arkay
                22, // Zenithar
                24, // Mara
                25, // Ebonarm
                26, // Akatosh
                27, // Julianos
                29, // Dibella
                33, // Stendarr
                35, // Kynareth
            };

            PopulateTokens(religiousTokens, aedraFactionIDs, false);

            religiousTokens.Add(TextFile.NewLineToken);
            religiousTokens.Add(new TextFile.Token() { text = "Daedra", formatting = TextFile.Formatting.TextHighlight });
            religiousTokens.Add(tab);
            religiousTokens.Add(new TextFile.Token() { text = "Opinion of You", formatting = TextFile.Formatting.TextHighlight });
            religiousTokens.Add(TextFile.NewLineToken);

            System.Collections.Generic.List<int> daedraFactionIDs = new System.Collections.Generic.List<int>()
            {
                1, // Clavicus Vile
                2, // Mehrunes Dagon
                3, // Molag Bal
                4, // Hircine
                5, // Sanguine
                6, // Peryite
                7, // Malacath
                8, // Hermaeus Mora
                9, // Sheogorath
                10, // Boethiah
                11, // Namira
                12, // Meridia
                13, // Vaermina
                14, // Nocturnal
                15, // Mephala
                16, // Azura
            };

            PopulateTokens(religiousTokens, daedraFactionIDs, false);

            if (overrideDefaultSettings == false)
            {
                tab1.x = 69;
                tab2.x = 110;
            }

            religiousTokens.Add(TextFile.NewLineToken);
            religiousTokens.Add(new TextFile.Token() { text = "Temple Orders", formatting = TextFile.Formatting.TextQuestion });
            religiousTokens.Add(TextFile.NewLineToken);
            religiousTokens.Add(new TextFile.Token() { text = "Order", formatting = TextFile.Formatting.TextHighlight });
            religiousTokens.Add(tab1);
            religiousTokens.Add(new TextFile.Token() { text = "Reputation", formatting = TextFile.Formatting.TextHighlight });
            religiousTokens.Add(tab2);
            religiousTokens.Add(new TextFile.Token() { text = "Allied To:", formatting = TextFile.Formatting.TextHighlight });
            religiousTokens.Add(tab3);
            religiousTokens.Add(new TextFile.Token() { text = "Enemy Of:", formatting = TextFile.Formatting.TextHighlight });
            religiousTokens.Add(TextFile.NewLineToken);

            System.Collections.Generic.List<int> templeOrderFactionIDs = new System.Collections.Generic.List<int>()
            {
                82, // Order of Arkay
                83, // Knights of the Circle
                84, // Resolution of Zen
                85, // Knights of Iron
                88, // Benevolence of Mara
                89, // The Maran Knights
                90, // Citadel of Ebonarm
                91, // The Battlelords
                92, // Akatosh Chantry
                93, // Order of the Hours
                94, // Schools of Julianos
                95, // The Knights Mentor
                98, // House of Dibella
                99, // Order of the Lily
                106, // Temple of Stendarr
                107, // The Crusaders
            };

            PopulateTokens(religiousTokens, templeOrderFactionIDs, true);

            if (overrideDefaultSettings == false)
            {
                tab2.x = 125;
            }

            religiousTokens.Add(TextFile.NewLineToken);
            religiousTokens.Add(new TextFile.Token() { text = "Groups of the Eight", formatting = TextFile.Formatting.TextQuestion });
            religiousTokens.Add(TextFile.NewLineToken);
            religiousTokens.Add(new TextFile.Token() { text = "Akatosh", formatting = TextFile.Formatting.TextHighlight });
            religiousTokens.Add(tab);
            religiousTokens.Add(new TextFile.Token() { text = "Reputation", formatting = TextFile.Formatting.TextHighlight });
            religiousTokens.Add(TextFile.NewLineToken);

            System.Collections.Generic.List<int> akatoshFactionIDs = new System.Collections.Generic.List<int>()
            {
                473, // Apothecaries
                474, // Mixers
                475, // Summoners
                247, // Teachers
            };

            PopulateTokens(religiousTokens, akatoshFactionIDs, false);

            religiousTokens.Add(TextFile.NewLineToken);
            religiousTokens.Add(new TextFile.Token() { text = "Arkay", formatting = TextFile.Formatting.TextHighlight });
            religiousTokens.Add(tab);
            religiousTokens.Add(new TextFile.Token() { text = "Reputation", formatting = TextFile.Formatting.TextHighlight });
            religiousTokens.Add(TextFile.NewLineToken);

            System.Collections.Generic.List<int> arkayFactionIDs = new System.Collections.Generic.List<int>()
            {
                453, // Apothecaries
                454, // Mixers
                455, // Binders
                456, // Summoners
                241, // Teachers
            };

            PopulateTokens(religiousTokens, arkayFactionIDs, false);

            religiousTokens.Add(TextFile.NewLineToken);
            religiousTokens.Add(new TextFile.Token() { text = "Dibella", formatting = TextFile.Formatting.TextHighlight });
            religiousTokens.Add(tab);
            religiousTokens.Add(new TextFile.Token() { text = "Reputation", formatting = TextFile.Formatting.TextHighlight });
            religiousTokens.Add(TextFile.NewLineToken);

            System.Collections.Generic.List<int> dibellaFactionIDs = new System.Collections.Generic.List<int>()
            {
                485, // Apothecaries
                487, // Mixers
                488, // Summoners
                250, // Teachers
            };

            PopulateTokens(religiousTokens, dibellaFactionIDs, false);

            religiousTokens.Add(TextFile.NewLineToken);
            religiousTokens.Add(new TextFile.Token() { text = "Julianos", formatting = TextFile.Formatting.TextHighlight });
            religiousTokens.Add(tab);
            religiousTokens.Add(new TextFile.Token() { text = "Reputation", formatting = TextFile.Formatting.TextHighlight });
            religiousTokens.Add(TextFile.NewLineToken);

            System.Collections.Generic.List<int> julianosFactionIDs = new System.Collections.Generic.List<int>()
            {
                480, // Crafters
                481, // Smiths
                482, // Summoners
                249, // Teachers
            };

            PopulateTokens(religiousTokens, julianosFactionIDs, false);

            religiousTokens.Add(TextFile.NewLineToken);
            religiousTokens.Add(new TextFile.Token() { text = "Kynareth", formatting = TextFile.Formatting.TextHighlight });
            religiousTokens.Add(tab);
            religiousTokens.Add(new TextFile.Token() { text = "Reputation", formatting = TextFile.Formatting.TextHighlight });
            religiousTokens.Add(TextFile.NewLineToken);

            System.Collections.Generic.List<int> kynarethFactionIDs = new System.Collections.Generic.List<int>()
            {
                496, // Enchanters
                497, // Spellsmiths
                498, // Summoners
                254, // Teachers
            };

            PopulateTokens(religiousTokens, kynarethFactionIDs, false);

            religiousTokens.Add(TextFile.NewLineToken);
            religiousTokens.Add(new TextFile.Token() { text = "Mara", formatting = TextFile.Formatting.TextHighlight });
            religiousTokens.Add(tab);
            religiousTokens.Add(new TextFile.Token() { text = "Reputation", formatting = TextFile.Formatting.TextHighlight });
            religiousTokens.Add(TextFile.NewLineToken);

            System.Collections.Generic.List<int> maraFactionIDs = new System.Collections.Generic.List<int>()
            {
                468, // Apothecaries
                469, // Mixers
                470, // Summoners
                245, // Teachers
            };

            PopulateTokens(religiousTokens, maraFactionIDs, false);

            religiousTokens.Add(TextFile.NewLineToken);
            religiousTokens.Add(new TextFile.Token() { text = "Stendarr", formatting = TextFile.Formatting.TextHighlight });
            religiousTokens.Add(tab);
            religiousTokens.Add(new TextFile.Token() { text = "Reputation", formatting = TextFile.Formatting.TextHighlight });
            religiousTokens.Add(TextFile.NewLineToken);

            System.Collections.Generic.List<int> stendarrFactionIDs = new System.Collections.Generic.List<int>()
            {
                490, // Apothecaries
                491, // Mixers
                492, // Summoners
                252, // Teachers
            };

            PopulateTokens(religiousTokens, stendarrFactionIDs, false);

            religiousTokens.Add(TextFile.NewLineToken);
            religiousTokens.Add(new TextFile.Token() { text = "Zenithar", formatting = TextFile.Formatting.TextHighlight });
            religiousTokens.Add(tab);
            religiousTokens.Add(new TextFile.Token() { text = "Reputation", formatting = TextFile.Formatting.TextHighlight });
            religiousTokens.Add(TextFile.NewLineToken);

            System.Collections.Generic.List<int> zenitharFactionIDs = new System.Collections.Generic.List<int>()
            {
                462, // Apothecaries
                463, // Mixers
                464, // Summoners
                243, // Teachers
            };

            PopulateTokens(religiousTokens, zenitharFactionIDs, false);
        }

        public static void GetCovensAndClansReputations()
        {
            covensAndClansTokens.Add(TextFile.NewLineToken);
            covensAndClansTokens.Add(new TextFile.Token() { text = "Vampire Clans", formatting = TextFile.Formatting.TextHighlight });
            covensAndClansTokens.Add(tab1);
            covensAndClansTokens.Add(new TextFile.Token() { text = "Reputation", formatting = TextFile.Formatting.TextHighlight });
            covensAndClansTokens.Add(tab2);
            covensAndClansTokens.Add(new TextFile.Token() { text = "Allied To:", formatting = TextFile.Formatting.TextHighlight });
            covensAndClansTokens.Add(tab3);
            covensAndClansTokens.Add(new TextFile.Token() { text = "Enemy Of:", formatting = TextFile.Formatting.TextHighlight });

            covensAndClansTokens.Add(TextFile.NewLineToken);

            System.Collections.Generic.List<int> vampireFactionIDs = new System.Collections.Generic.List<int>()
            {
                150, // The Vraseth
                151, // The Haarvenu
                152, // The Thrafey
                153, // The Lyrezi
                154, // The Montalion
                155, // The Khulari
                156, // The Garlythi
                157, // The Anthotis
                158, // The Selenu
            };

            PopulateTokens(covensAndClansTokens, vampireFactionIDs, true);

            covensAndClansTokens.Add(TextFile.NewLineToken);
            covensAndClansTokens.Add(new TextFile.Token() { text = "Witch Covens", formatting = TextFile.Formatting.TextHighlight });
            covensAndClansTokens.Add(tab1);
            covensAndClansTokens.Add(new TextFile.Token() { text = "Reputation", formatting = TextFile.Formatting.TextHighlight });
            covensAndClansTokens.Add(TextFile.NewLineToken);

            System.Collections.Generic.List<int> witchCovenFactionIDs = new System.Collections.Generic.List<int>()
            {
                419, // Glenmoril
                420, // Dust Witches
                421, // Devilrock
                422, // Tamarilyn
                423, // Sisters of the Bluff
                424, // Wroth Daughters
                425, // Skeffington
                426, // Marsh
                427, // Mountain
                428, // Daggerfall
                429, // Beldama
                430, // Kykos
                431, // Tide
                432, // Alcaire
            };

            PopulateTokens(covensAndClansTokens, witchCovenFactionIDs, true);
        }

        public static void GetNotablePeopleTokens()
        {
            if (overrideDefaultSettings == false)
            {
                tab1.x = 70;
                tab2.x = 118;
            }

            // High Rock
            notablePeopleTokens.Add(TextFile.NewLineToken);
            notablePeopleTokens.Add(new TextFile.Token() { text = "High Rock", formatting = TextFile.Formatting.TextQuestion });
            notablePeopleTokens.Add(TextFile.NewLineToken);
            notablePeopleTokens.Add(new TextFile.Token() { text = "Person", formatting = TextFile.Formatting.TextHighlight });
            notablePeopleTokens.Add(tab1);
            notablePeopleTokens.Add(new TextFile.Token() { text = "Reputation", formatting = TextFile.Formatting.TextHighlight });
            notablePeopleTokens.Add(tab2);
            notablePeopleTokens.Add(new TextFile.Token() { text = "Allied To:", formatting = TextFile.Formatting.TextHighlight });
            notablePeopleTokens.Add(tab3);
            notablePeopleTokens.Add(new TextFile.Token() { text = "Enemy Of:", formatting = TextFile.Formatting.TextHighlight });
            notablePeopleTokens.Add(TextFile.NewLineToken);
            
            System.Collections.Generic.List<int> notablePeopleHighRockFactionIDs = new System.Collections.Generic.List<int>()
            {
                // Daggerfall
                364, // King Gothryd
                365, // Queen Aubk-i
                367, // Lord Bridwell
                377, // Lady Bridwell
                501, // Lady Northbridge
                360, // Lord Kilbar
                366, // Mynisera
                303, // Nulfaga
                500, // Cyndassa
                363, // Medora
                371, // Mobar
                361, // Chulmore Quill
                362, // The Quill Circus
                // Anticlere
                401, // Lord Auberon Flyte
                400, // Lady Flyte
                402, // Lord Quistley
                404, // Lord Perwright
                // Ykalon
                359, // Lord Plessington

                403, // Farrington
                // Wayrest
                390, // King Eadwyre
                391, // Queen Barenziah
                392, // Princess Elysana
                393, // Prince Helseth
                394, // Princess Morgiah
                395, // Lord Castellian
                397, // Lord Darkworth
                398, // Lord Woodbourne
                396, // Karethys
                // Glenpoint
                375, // Lord Bertram Spode
                378, // Sylch Greenwood
                // Orsinium
                357, // Gortwog
            };

            PopulateTokens(notablePeopleTokens, notablePeopleHighRockFactionIDs, true);

            // Hammerfell
            notablePeopleTokens.Add(TextFile.NewLineToken);
            notablePeopleTokens.Add(new TextFile.Token() { text = "Hammerfell", formatting = TextFile.Formatting.TextQuestion });
            notablePeopleTokens.Add(TextFile.NewLineToken);
            notablePeopleTokens.Add(new TextFile.Token() { text = "Person", formatting = TextFile.Formatting.TextHighlight });
            notablePeopleTokens.Add(tab1);
            notablePeopleTokens.Add(new TextFile.Token() { text = "Reputation", formatting = TextFile.Formatting.TextHighlight });
            notablePeopleTokens.Add(tab2);
            notablePeopleTokens.Add(new TextFile.Token() { text = "Allied To:", formatting = TextFile.Formatting.TextHighlight });
            notablePeopleTokens.Add(tab3);
            notablePeopleTokens.Add(new TextFile.Token() { text = "Enemy Of:", formatting = TextFile.Formatting.TextHighlight });
            notablePeopleTokens.Add(TextFile.NewLineToken);

            System.Collections.Generic.List<int> notablePeopleHammerfellFactionIDs = new System.Collections.Generic.List<int>()
            {
                // Sentinel
                380, // Queen Akorithi
                381, // Prince Greklith
                382, // Prince Lhotun
                383, // Lord Vhosek
                386, // Lord Kavar
                387, // Lord Provlith
                301, // The Oracle
                302, // The Acolyte

                385, // Charvek-si

                // Lainlyn
                379, // Baron Shrike
                405, // Baronness Dhemka
                406, // Lord Khane
                407, // Br'itsa

                // Dragontail Mtns
                355, // Lord Harth
            };

            PopulateTokens(notablePeopleTokens, notablePeopleHammerfellFactionIDs, true);

            // Mages Guild
            notablePeopleTokens.Add(TextFile.NewLineToken);
            notablePeopleTokens.Add(new TextFile.Token() { text = "Mages Guild", formatting = TextFile.Formatting.TextQuestion });
            notablePeopleTokens.Add(TextFile.NewLineToken);
            notablePeopleTokens.Add(new TextFile.Token() { text = "Person", formatting = TextFile.Formatting.TextHighlight });
            notablePeopleTokens.Add(tab1);
            notablePeopleTokens.Add(new TextFile.Token() { text = "Reputation", formatting = TextFile.Formatting.TextHighlight });
            notablePeopleTokens.Add(tab2);
            notablePeopleTokens.Add(new TextFile.Token() { text = "Allied To:", formatting = TextFile.Formatting.TextHighlight });
            notablePeopleTokens.Add(tab3);
            notablePeopleTokens.Add(new TextFile.Token() { text = "Enemy Of:", formatting = TextFile.Formatting.TextHighlight });
            notablePeopleTokens.Add(TextFile.NewLineToken);

            System.Collections.Generic.List<int> notablePeopleMagesGuildFactionIDs = new System.Collections.Generic.List<int>()
            {
                40, // The Mages Guild
                68, // The Archmagister
                369, // Popudax
                376, // Baltham Greyman
            };

            PopulateTokens(notablePeopleTokens, notablePeopleMagesGuildFactionIDs, true);

            // Thieves Guild
            notablePeopleTokens.Add(TextFile.NewLineToken);
            notablePeopleTokens.Add(new TextFile.Token() { text = "Thieves Guild", formatting = TextFile.Formatting.TextQuestion });
            notablePeopleTokens.Add(TextFile.NewLineToken);
            notablePeopleTokens.Add(new TextFile.Token() { text = "Person", formatting = TextFile.Formatting.TextHighlight });
            notablePeopleTokens.Add(tab1);
            notablePeopleTokens.Add(new TextFile.Token() { text = "Reputation", formatting = TextFile.Formatting.TextHighlight });
            notablePeopleTokens.Add(tab2);
            notablePeopleTokens.Add(new TextFile.Token() { text = "Allied To:", formatting = TextFile.Formatting.TextHighlight });
            notablePeopleTokens.Add(tab3);
            notablePeopleTokens.Add(new TextFile.Token() { text = "Enemy Of:", formatting = TextFile.Formatting.TextHighlight });
            notablePeopleTokens.Add(TextFile.NewLineToken);

            System.Collections.Generic.List<int> notablePeopleThievesGuildFactionIDs = new System.Collections.Generic.List<int>()
            {
                42, // The Thieves Guild
                373, // The Crow
                399, // The Squid
                374, // Thyr Topfield
                388, // Thaik
                389 // Whitka
            };

            PopulateTokens(notablePeopleTokens, notablePeopleThievesGuildFactionIDs, true);

            // The Blades
            notablePeopleTokens.Add(TextFile.NewLineToken);
            notablePeopleTokens.Add(new TextFile.Token() { text = "The Blades", formatting = TextFile.Formatting.TextQuestion });
            notablePeopleTokens.Add(TextFile.NewLineToken);
            notablePeopleTokens.Add(new TextFile.Token() { text = "Person", formatting = TextFile.Formatting.TextHighlight });
            notablePeopleTokens.Add(tab1);
            notablePeopleTokens.Add(new TextFile.Token() { text = "Reputation", formatting = TextFile.Formatting.TextHighlight });
            notablePeopleTokens.Add(tab2);
            notablePeopleTokens.Add(new TextFile.Token() { text = "Allied To:", formatting = TextFile.Formatting.TextHighlight });
            notablePeopleTokens.Add(tab3);
            notablePeopleTokens.Add(new TextFile.Token() { text = "Enemy Of:", formatting = TextFile.Formatting.TextHighlight });
            notablePeopleTokens.Add(TextFile.NewLineToken);

            System.Collections.Generic.List<int> notablePeopleBladesFactionIDs = new System.Collections.Generic.List<int>()
            {
                129, // The Blades
                352, // Lady Brisienna
                351, // The Great Knight
            };

            PopulateTokens(notablePeopleTokens, notablePeopleBladesFactionIDs, true);

            // Mysterious Forces
            notablePeopleTokens.Add(TextFile.NewLineToken);
            notablePeopleTokens.Add(new TextFile.Token() { text = "Mysterious Forces", formatting = TextFile.Formatting.TextQuestion });
            notablePeopleTokens.Add(TextFile.NewLineToken);
            notablePeopleTokens.Add(new TextFile.Token() { text = "Person", formatting = TextFile.Formatting.TextHighlight });
            notablePeopleTokens.Add(tab1);
            notablePeopleTokens.Add(new TextFile.Token() { text = "Reputation", formatting = TextFile.Formatting.TextHighlight });
            notablePeopleTokens.Add(tab2);
            notablePeopleTokens.Add(new TextFile.Token() { text = "Allied To:", formatting = TextFile.Formatting.TextHighlight });
            notablePeopleTokens.Add(tab3);
            notablePeopleTokens.Add(new TextFile.Token() { text = "Enemy Of:", formatting = TextFile.Formatting.TextHighlight });
            notablePeopleTokens.Add(TextFile.NewLineToken);

            System.Collections.Generic.List<int> notablePeopleMysteryFactionIDs = new System.Collections.Generic.List<int>()
            {
                305, // King of Worms
                306, // The Necromancers
                353, // Underking
                354, // Agents of the Underking
                356, // The Night Mother
                108, // The Dark Brotherhood
            };

            PopulateTokens(notablePeopleTokens, notablePeopleMysteryFactionIDs, true);
        }

        public static void GetKnightsTokens()
        {
            // Knightly Orders
            if (overrideDefaultSettings == false)
            {
                tab1.x = 75;
                tab2.x = 120;
            }

            knightsTokens.Add(TextFile.NewLineToken);
            knightsTokens.Add(new TextFile.Token() { text = "Knightly Order", formatting = TextFile.Formatting.TextHighlight });
            knightsTokens.Add(tab1);
            knightsTokens.Add(new TextFile.Token() { text = "Reputation", formatting = TextFile.Formatting.TextHighlight });
            knightsTokens.Add(tab2);
            knightsTokens.Add(new TextFile.Token() { text = "Allied To:", formatting = TextFile.Formatting.TextHighlight });
            knightsTokens.Add(tab3);
            knightsTokens.Add(new TextFile.Token() { text = "Enemy Of:", formatting = TextFile.Formatting.TextHighlight });
            knightsTokens.Add(TextFile.NewLineToken);
            
            System.Collections.Generic.List<int> knightOrderFactionIDs = new System.Collections.Generic.List<int>()
            {
                408, // Order of the Candle
                409, // Knights of the Rose
                410, // Knights of the Flame
                411, // Host of the Horn
                412, // Host of the True Horn
                413, // Knights of the Owl
                414, // Order of the Raven
                415, // Knights of the Wheel
                416, // Order of the Scarab
                417, // Knights of the Hawk
                418, // Order of the Cup
            };

            PopulateTokens(knightsTokens, knightOrderFactionIDs, true);

            // Generic Groups
            knightsTokens.Add(TextFile.NewLineToken);
            knightsTokens.Add(new TextFile.Token() { text = "Knight Order Groups", formatting = TextFile.Formatting.TextHighlight });
            knightsTokens.Add(tab1);
            knightsTokens.Add(new TextFile.Token() { text = "Reputation", formatting = TextFile.Formatting.TextHighlight });
            knightsTokens.Add(TextFile.NewLineToken);

            System.Collections.Generic.List<int> knightOrderGenericFactionIDs = new System.Collections.Generic.List<int>()
            {
                //844, // Generic Knightly Order
                845, // The Smiths
                846, // The Questers
                847, // The Healers
                848 // The Seneschal
            };

            PopulateTokens(knightsTokens, knightOrderGenericFactionIDs, true);
        }

        public static void PopulateTokens(List<TextFile.Token> tokenList, System.Collections.Generic.List<int> factionIDsList, bool alliesAndEnemies)
        {
            foreach (int factionId in factionIDsList)
            {
                FactionFile.FactionData factionData;
                if (factionFile.GetFactionData(factionId, out factionData))
                {
                    string factionName = factionData.name;
                    string allyName = "";
                    string enemyName = "";
                    int allyID = factionData.ally1;
                    int enemyID = factionData.enemy1;
                    int reputation = GameManager.Instance.PlayerEntity.FactionData.GetReputation(factionData.id);
                    string reputationRank = "";

                    // Raw Number vs. Text check
                    if (!useRawNumbers)
                    {
                        if (tokenList == guildsAndLocalsTokens || tokenList == notablePeopleTokens) { reputationRank = AffiliationsPlus.ReputationText.GetReputationRankString(reputation); }
                        if (tokenList == knightsTokens) { reputationRank = AffiliationsPlus.ReputationText.GetKnightlyReputationRankString(reputation); }
                        if (tokenList == covensAndClansTokens) { reputationRank = AffiliationsPlus.ReputationText.GetCovenAndClanReputationRankString(reputation); }
                        if (tokenList == religiousTokens) { reputationRank = AffiliationsPlus.ReputationText.GetReligiousReputationRankString(reputation); }
                    }

                    if (useRawNumbers)
                    {
                        reputationRank = reputation.ToString();
                    }

                    FactionFile.FactionData allyData;
                    FactionFile.FactionData enemyData;

                    if (alliesAndEnemies == true)
                    {
                        if (factionFile.GetFactionData(allyID, out allyData))
                        {
                            allyName = allyData.name;
                        }

                        if (factionFile.GetFactionData(enemyID, out enemyData))
                        {
                            enemyName = enemyData.name;
                        }

                        if (overrideDefaultSettings == false && factionIDsList.Contains(92)) // if its the Temple Order list, which needs special formatting
                        {
                            tab1.x = 69;
                            tab2.x = 110;
                        }

                        if (overrideDefaultSettings == false && factionIDsList.Contains(414)) // if its the Knight Order list, which needs special formatting
                        {
                            tab1.x = 75;
                            tab2.x = 120;
                        }

                        if (overrideDefaultSettings == false && factionIDsList.Contains(352)) // if its the Notable People list, which needs special formatting
                        {
                            tab1.x = 70;
                            tab2.x = 118;
                        }

                        tokenList.Add(TextFile.CreateTextToken(factionName));
                        tokenList.Add(tab1);
                        tokenList.Add(TextFile.CreateTextToken(reputationRank));
                        tokenList.Add(tab2);
                        tokenList.Add(TextFile.CreateTextToken(allyName));
                        tokenList.Add(tab3);
                        tokenList.Add(TextFile.CreateTextToken(enemyName));
                        tokenList.Add(TextFile.NewLineToken);
                    }

                    else
                    {
                        tokenList.Add(TextFile.CreateTextToken(factionName));
                        tokenList.Add(tab);
                        tokenList.Add(TextFile.CreateTextToken(reputationRank));
                        tokenList.Add(TextFile.NewLineToken);
                    }

                    //Debug.Log($"[Stats+] Added hardcoded faction: {factionName} (ID: {factionId}, Rep: {reputation}, Rank: {reputationRank})");
                }
                else
                {
                    Debug.LogWarning($"[Stats+] Could NOT find FactionData for hardcoded ID: {factionId}. This ID might not exist in your FactionFile.");
                }
            }
        }

        private void Update()
        {
            // Open Affiliations menu with Shift+A while on character sheet
            if (Input.GetKeyDown(KeyCode.A) && Input.GetKey(KeyCode.LeftShift))
            {
                if (DaggerfallUI.Instance.UserInterfaceManager.TopWindow as DaggerfallCharacterSheetWindow != null)
                {
                    ReplaceAffiliations();
                }
            }
        }
	}

	public static class ReputationText
    {
        public static string GetReputationRankString(int reputation)
        {
            if (reputation >= 100) return "Kindred";
            if (reputation > 80) return "Revered";
            if (reputation > 60) return "Esteemed";
            if (reputation > 40) return "Honored";
            if (reputation > 20) return "Admired";
            if (reputation > 10) return "Respected";
            if (reputation > 0)  return "Dependable";
            if (reputation == 0) return "Common Citizen"; 
            if (reputation < -80) return "Hated";        
            if (reputation < -60) return "Pond Scum";
            if (reputation < -40) return "Villain";
            if (reputation < -20) return "Criminal";
            if (reputation < -10) return "Scoundrel";
            if (reputation < 0) return "Undependable"; 

            return "Unknown Rank"; 
        }

        public static string GetLegalReputationRankString(int reputation)
        {
            if (reputation > 80) return "Revered";
            if (reputation > 60) return "Esteemed";
            if (reputation > 40) return "Honored";
            if (reputation > 20) return "Admired";
            if (reputation > 10) return "Respected";
            if (reputation > 0)  return "Dependable";
            if (reputation == 0) return "Unremarkable"; 
            if (reputation < -100) return "Public Enemy No. 1";
            if (reputation < -80) return "Hated";        
            if (reputation < -60) return "Pond Scum";
            if (reputation < -40) return "Villain";
            if (reputation < -20) return "Criminal";
            if (reputation < -10) return "Scoundrel";
            if (reputation < 0) return "Sketchy"; 

            return "Unknown Rank";
        }

        public static string GetKnightlyReputationRankString(int reputation)
        {
            if (reputation >= 100) return "Honorary Knight";
            if (reputation > 80) return "Revered";
            if (reputation > 60) return "Honored";
            if (reputation > 40) return "Admired";
            if (reputation > 20) return "Respected";
            if (reputation > 10) return "Dependable";
            if (reputation > 0)  return "Unremarkable";
            if (reputation == 0) return "Civilian";
            if (reputation < -100) return "Sworn Enemy";
            if (reputation < -80) return "Hated";        
            if (reputation < -60) return "Pond Scum";
            if (reputation < -40) return "Villain";
            if (reputation < -20) return "Criminal";
            if (reputation < -10) return "Scoundrel";
            if (reputation < 0) return "Trifling"; 

            return "Unknown Rank"; 
        }

        public static string GetReligiousReputationRankString(int reputation)
        {
            if (reputation >= 100) return "Messiah";
            if (reputation > 80) return "Exalted";
            if (reputation > 60) return "Revered";
            if (reputation > 40) return "Honored";
            if (reputation > 20) return "Accepted";
            if (reputation > 10) return "Respected";
            if (reputation > 0)  return "Acknowledged";
            if (reputation == 0) return "Ambivalent";
            if (reputation < -100) return "Fatwah Issued";
            if (reputation < -80) return "Pariah";        
            if (reputation < -60) return "Condemned";
            if (reputation < -40) return "Disgusted";
            if (reputation < -20) return "Sinful";
            if (reputation < -10) return "Displeased";
            if (reputation < 0) return "Ignored"; 

            return "Unknown Rank"; 
        }

        public static string GetCovenAndClanReputationRankString(int reputation)
        {
            if (reputation >= 100) return "Blood Relative";
            if (reputation > 80) return "Clankin";
            if (reputation > 60) return "Wise One";
            if (reputation > 40) return "Trusted Agent";
            if (reputation > 20) return "Accepted";
            if (reputation > 10) return "Acknowledged";
            if (reputation > 0)  return "Unremarkable";
            if (reputation == 0) return "Outsider";
            if (reputation < -100) return "Anathema";
            if (reputation < -80) return "Kinkiller";        
            if (reputation < -60) return "Loathed";
            if (reputation < -40) return "Despised";
            if (reputation < -20) return "Fresh Meat";
            if (reputation < -10) return "Displeased";
            if (reputation < 0) return "Ignored"; 

            return "Unknown Rank"; 
        }
    }
}
