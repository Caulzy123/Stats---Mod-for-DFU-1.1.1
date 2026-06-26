using StatsPlus;
using DaggerfallWorkshop;
using DaggerfallWorkshop.Game;
using DaggerfallWorkshop.Game.Entity;
using DaggerfallWorkshop.Game.Formulas;
using DaggerfallWorkshop.Game.Items;
using DaggerfallWorkshop.Game.Guilds;
using DaggerfallWorkshop.Game.Questing;
using DaggerfallWorkshop.Game.UserInterface;
using DaggerfallWorkshop.Game.UserInterfaceWindows;
using DaggerfallConnect;
using DaggerfallConnect.Arena2;
using DaggerfallWorkshop.Utility;
using DaggerfallWorkshop.Utility.AssetInjection;
using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

namespace StatsPlus
{
    public class DaggerStatsWindow : DaggerfallPopupWindow
    {
        #region UI Controls
        Panel mainPanel = new Panel();
        protected ScrollingPanel scrollingPanel = new ScrollingPanel();
        protected VerticalScrollBar scroller = new VerticalScrollBar();
        protected Button exitButton = new Button();

        protected TextLabel generalStatsLabel = new TextLabel();
        protected TextLabel questStatsLabel = new TextLabel();
        protected TextLabel combatStatsLabel = new TextLabel();
        protected TextLabel magicStatsLabel = new TextLabel();
        protected TextLabel craftStatsLabel = new TextLabel();
        protected TextLabel crimeStatsLabel = new TextLabel();

        #endregion

        #region UI Textures
        protected Texture2D baseTexture;
        #endregion

        # region Fields
        const string baseTextureName = "DS_WINDOW_BG";
        #endregion

        #region Constructors
        public DaggerStatsWindow(IUserInterfaceManager uiManager) : base(uiManager)
        {
            ParentPanel.BackgroundColor = Color.clear;
        }
        #endregion

        #region Setup Methods
        protected override void Setup()
        {
            base.Setup();

            Texture2D tex;
            bool textureLoaded = TextureReplacement.TryImportTexture(baseTextureName, true, out tex);
            baseTexture = tex;

            if (textureLoaded && baseTexture != null)
            {
                baseTexture.filterMode = FilterMode.Point;
                //Debug.Log("[StatsPlus] Successfully loaded texture: " + baseTextureName);
            }
            else
            {
                Debug.LogError("[StatsPlus] FAILED to load texture: " + baseTextureName + ". Check path and filename.");
            }

            // Main Panel
            mainPanel.HorizontalAlignment = HorizontalAlignment.Center;
            mainPanel.VerticalAlignment = VerticalAlignment.Middle;
            mainPanel.BackgroundTexture = baseTexture;
            mainPanel.Position = Vector2.zero;
            mainPanel.Size = ParentPanel.Size;
            mainPanel.Outline.Enabled = false;
            mainPanel.Outline.Color = Color.green; // Make it a bright color for debugging
            NativePanel.Components.Add(mainPanel);

            // Scroller Padding
            float horizontalPadding = 10; 
            float verticalPadding = 10;  
            float scrollerWidth = 8;

            mainPanel.Size = new Vector2(320, 200);

            // Center mainPanel
            mainPanel.Position = new Vector2(
                (ParentPanel.Size.x / 2) - (mainPanel.Size.x / 2),
                (ParentPanel.Size.y / 2) - (mainPanel.Size.y / 2)
            );

            // Scrolling Panel
            scrollingPanel.Position = new Vector2(horizontalPadding, verticalPadding + 12);
            scrollingPanel.Size = new Vector2(mainPanel.Size.x - (2 * horizontalPadding) - scrollerWidth - 2, mainPanel.Size.y - (2 * verticalPadding) - 33);
            scrollingPanel.BackgroundColor = new Color(0.5f, 0.5f, 0.5f, 0.5f);
            scrollingPanel.Outline.Enabled = true; // Debugging
            scrollingPanel.Outline.Color = Color.yellow; // Yellow outline for debugging
            scrollingPanel.OnMouseScrollUp += ScrollingPanel_OnMouseScrollUp;
            scrollingPanel.OnMouseScrollDown += ScrollingPanel_OnMouseScrollDown;
            mainPanel.Components.Add(scrollingPanel);

            // Scroller
            scroller.Position = new Vector2(scrollingPanel.Position.x + scrollingPanel.Size.x + 2, scrollingPanel.Position.y + 12);
            scroller.Size = new Vector2(scrollerWidth, scrollingPanel.Size.y);
            scroller.OnScroll += Scroller_OnScroll;
            mainPanel.Components.Add(scroller);

            scrollingPanel.ScrollTransform = 7;

            // Spacing Values
            int currentY = 22;
            int lineHeight = 7; 
            int sectionSpacing = 15; 
            int column1X = 10; 
            int column2X = 125;


            // Adding Stats
            Action<string, string, int, int> AddStatLine = (label, value, xOffsetLabel, xOffsetValue) =>
            {
                TextLabel statLabel = new TextLabel();
                statLabel.Position = new Vector2(xOffsetLabel, currentY);
                statLabel.Text = label;
                statLabel.TextColor = Color.yellow;
                statLabel.TextScale = 1.0f;
                statLabel.WrapText = false;
                float labelWidth = xOffsetValue - xOffsetLabel - 5;
                if (labelWidth <= 0)
                {
                    Debug.LogWarning($"[StatsPlus] Calculated statLabel width is too small or negative ({labelWidth}). Defaulting to 100.");
                    labelWidth = 100;
                }
                statLabel.Size = new Vector2(labelWidth, lineHeight);

                //Debug.Log($"[StatsPlus] AddStatLine: Label='{label}', Pos={statLabel.Position}, Size={statLabel.Size}");
                scrollingPanel.Components.Add(statLabel);


                TextLabel statValue = new TextLabel();
                statValue.Position = new Vector2(xOffsetValue, currentY);
                statValue.Text = value;
                statValue.TextColor = Color.yellow;
                statValue.TextScale = 1.0f;
                float valueWidth = scrollingPanel.Size.x - xOffsetValue - 5;
                if (valueWidth <= 0)
                {
                    Debug.LogWarning($"[StatsPlus] Calculated statValue width is too small or negative ({valueWidth}). Defaulting to 100.");
                    valueWidth = 100;
                }
                statValue.Size = new Vector2(valueWidth, lineHeight);
                scrollingPanel.Components.Add(statValue);

                //Debug.Log($"[StatsPlus] AddStatLine: Value='{value}', Pos={statValue.Position}, Size={statValue.Size}");
                currentY += lineHeight;
            };

            // Spacers
            Action<string, int, int> AddSpacerLine = (value, xOffsetLabel, xOffsetValue) =>
            {
                TextLabel spacerValue = new TextLabel();
                spacerValue.Position = new Vector2(xOffsetValue, currentY);
                spacerValue.Text = value;
                float valueWidth = scrollingPanel.Size.x - xOffsetValue - 5;
                if (valueWidth <= 0)
                {
                    Debug.LogWarning($"[StatsPlus] Calculated statValue width is too small or negative ({valueWidth}). Defaulting to 100.");
                    valueWidth = 100;
                }
                spacerValue.Size = new Vector2(valueWidth, lineHeight);
                scrollingPanel.Components.Add(spacerValue);
                currentY += lineHeight;
            };

            // GENERAL STATS
            generalStatsLabel.Position = new Vector2(column1X, currentY);
            generalStatsLabel.TextColor = Color.cyan;
            generalStatsLabel.TextScale = 1.3f;
            generalStatsLabel.Text = "General";
            scrollingPanel.Components.Add(generalStatsLabel);
            currentY += sectionSpacing;

            AddStatLine("Days Passed", DaggerStatsMain.Instance.daysPassed.ToString(), column1X, column2X);
            AddStatLine("Years Passed", DaggerStatsMain.Instance.yearsPassed.ToString(), column1X, column2X);
            AddStatLine("Hours Loitered", DaggerStatsMain.Instance.hoursLoitered.ToString(), column1X, column2X);
            AddStatLine("Hours Slept (in a bed)", DaggerStatsMain.Instance.hoursSlept.ToString(), column1X, column2X);
            AddStatLine("Longest Nap (hours)", DaggerStatsMain.Instance.longestCrashSession.ToString(), column1X, column2X);
            AddStatLine("Time Spent Climbing", DaggerStatsMain.Instance.timeSpentClimbing, column1X, column2X);
            AddStatLine("Time Spent Swimming", DaggerStatsMain.Instance.timeSpentSwimming, column1X, column2X);
            AddStatLine("Time Spent Levitating", DaggerStatsMain.Instance.timeSpentLevitating, column1X, column2X);
            AddStatLine("Tarhielian Expeditions", DaggerStatsMain.Instance.tarhielianExpeditions.ToString(), column1X, column2X);
            AddSpacerLine("   ", column1X, column2X);
            AddStatLine("Regions Visited", DaggerStatsMain.Instance.regionsVisited.ToString(), column1X, column2X);
            AddStatLine("Towns Visited", DaggerStatsMain.Instance.townsVisited.ToString(), column1X, column2X);
            AddStatLine("Homes Visited", DaggerStatsMain.Instance.homesVisited.ToString(), column1X, column2X);
            AddStatLine("Dungeons Visited", DaggerStatsMain.Instance.dungeonsVisited.ToString(), column1X, column2X);
            AddStatLine("Temples Visited", DaggerStatsMain.Instance.templesVisited.ToString(), column1X, column2X);
            AddStatLine("Library Cards", DaggerStatsMain.Instance.libraryCards.ToString(), column1X, column2X);
            AddStatLine("Favorite Place", DaggerStatsMain.Instance.favoritePlace, column1X, column2X);
            AddSpacerLine("   ", column1X, column2X);
            AddStatLine("Total Account Balance:", DaggerStatsMain.Instance.totalAccountBalance.ToString(), column1X, column2X);
            AddStatLine("Gold Deposited", DaggerStatsMain.Instance.goldDeposited.ToString(), column1X, column2X);
            AddStatLine("Gold Withdrawn", DaggerStatsMain.Instance.goldWithdrawn.ToString(), column1X, column2X);
            AddStatLine("Gold Borrowed", DaggerStatsMain.Instance.goldBorrowed.ToString(), column1X, column2X);
            AddStatLine("Gold Repaid", DaggerStatsMain.Instance.goldRepaid.ToString(), column1X, column2X);
            AddStatLine("Houses Bought", DaggerStatsMain.Instance.housesOwned.ToString(), column1X, column2X);
            AddStatLine("Houses Sold", DaggerStatsMain.Instance.housesSold.ToString(), column1X, column2X);
            AddStatLine("Ships Bought", DaggerStatsMain.Instance.shipsBought.ToString(), column1X, column2X);
            AddStatLine("Ships Sold", DaggerStatsMain.Instance.shipsSold.ToString(), column1X, column2X);
            AddSpacerLine("   ", column1X, column2X);
            AddStatLine("Diseases Contracted", DaggerStatsMain.Instance.diseasesContracted.ToString(), column1X, column2X);
            AddStatLine("Days as a Lycanthrope", DaggerStatsMain.Instance.daysAsLycanthrope.ToString(), column1X, column2X);
            AddStatLine("Days as a Vampire", DaggerStatsMain.Instance.daysAsVampire.ToString(), column1X, column2X);
            //AddStatLine("Longest Fast", DaggerStatsMain.Instance.libraryCards.ToString(), column1X, column2X);
            


            currentY += sectionSpacing;

            // QUEST STATS
            questStatsLabel.Position = new Vector2(column1X, currentY);
            Color questColor = new Color(0.28f, 0.76f, 1f, 1f);
            questStatsLabel.TextColor = questColor; // Color.blue;
            questStatsLabel.TextScale = 1.3f;
            questStatsLabel.Text = "Quests";
            scrollingPanel.Components.Add(questStatsLabel);
            currentY += sectionSpacing;

            AddStatLine("Main Quests Completed", DaggerStatsMain.Instance.mainQuestCompleted.ToString(), column1X, column2X);
            AddStatLine("Dark Brotherhood Quests Completed", DaggerStatsMain.Instance.darkBrotherhoodQuestsCompleted.ToString(), column1X, column2X);
            AddStatLine("Fighter's Guild Quests Completed", DaggerStatsMain.Instance.fightersGuildQuestsCompleted.ToString(), column1X, column2X);
            AddStatLine("Mage's Guild Quests Completed", DaggerStatsMain.Instance.magesGuildQuestsCompleted.ToString(), column1X, column2X);
            AddStatLine("Thieves Guild Quests Completed", DaggerStatsMain.Instance.thievesGuildQuestsCompleted.ToString(), column1X, column2X);
            AddStatLine("Temple Quests Completed", DaggerStatsMain.Instance.templeQuestsCompleted.ToString(), column1X, column2X);
            AddStatLine("Knight Order Quests Completed", DaggerStatsMain.Instance.knightOrderQuestsCompleted.ToString(), column1X, column2X);
            AddStatLine("Merchant Quests Completed", DaggerStatsMain.Instance.merchantQuestsCompleted.ToString(), column1X, column2X);
            AddStatLine("Commoner Quests Completed", DaggerStatsMain.Instance.commonerQuestsCompleted.ToString(), column1X, column2X);
            AddStatLine("Nobility Quests Completed", DaggerStatsMain.Instance.nobilityQuestsCompleted.ToString(), column1X, column2X);
            AddStatLine("Prostitute Quests Completed", DaggerStatsMain.Instance.prostituteQuestsCompleted.ToString(), column1X, column2X);
            AddStatLine("Witch Coven Quests Completed", DaggerStatsMain.Instance.witchCovenQuestsCompleted.ToString(), column1X, column2X);
            AddStatLine("Vampire Clan Quests Completed", DaggerStatsMain.Instance.vampireClanQuestsCompleted.ToString(), column1X, column2X);
            //AddStatLine("Cure Quests Completed", DaggerStatsMain.Instance.cureQuestsCompleted.ToString(), column1X, column2X);
            //AddStatLine("Daedra Quests Completed", DaggerStatsMain.Instance.daedricQuestsCompleted.ToString(), column1X, column2X);
            currentY += sectionSpacing;

            // COMBAT STATS
            combatStatsLabel.Position = new Vector2(column1X, currentY);
            combatStatsLabel.TextColor = Color.red;
            combatStatsLabel.TextScale = 1.3f;
            combatStatsLabel.Text = "Combat";
            scrollingPanel.Components.Add(combatStatsLabel);
            currentY += sectionSpacing;

            AddStatLine("Animals Killed", DaggerStatsMain.Instance.animalKills.ToString(), column1X, column2X);
            AddStatLine("Monsters Killed", DaggerStatsMain.Instance.monsterKills.ToString(), column1X, column2X);
            AddStatLine("Humans Killed", DaggerStatsMain.Instance.humanKills.ToString(), column1X, column2X);
            AddStatLine("Orcs Killed", DaggerStatsMain.Instance.orcKills.ToString(), column1X, column2X);
            AddStatLine("Lycanthropes Killed", DaggerStatsMain.Instance.lycanthropeKills.ToString(), column1X, column2X);
            AddStatLine("Atronachs Killed", DaggerStatsMain.Instance.atronachKills.ToString(), column1X, column2X);
            AddStatLine("Undead Killed", DaggerStatsMain.Instance.undeadKills.ToString(), column1X, column2X);
            AddStatLine("Daedra Killed", DaggerStatsMain.Instance.daedraKills.ToString(), column1X, column2X);
            AddStatLine("Favorite Weapon", DaggerStatsMain.Instance.favoriteWeapon, column1X, column2X);
            AddSpacerLine("   ", column1X, column2X);
            AddStatLine("Centaurs Calmed", DaggerStatsMain.Instance.centaursCalmed.ToString(), column1X, column2X);
            AddStatLine("Daedra De-Escalated", DaggerStatsMain.Instance.daedraDeescalated.ToString(), column1X, column2X);
            AddStatLine("Dragons Defused", DaggerStatsMain.Instance.dragonsDefused.ToString(), column1X, column2X);
            AddStatLine("Giants Gentled", DaggerStatsMain.Instance.giantsGentled.ToString(), column1X, column2X);
            AddStatLine("Harpies Hushed", DaggerStatsMain.Instance.harpiesHushed.ToString(), column1X, column2X);
            AddStatLine("Imps Impressed", DaggerStatsMain.Instance.impsImpressed.ToString(), column1X, column2X);
            AddStatLine("Nymphs Nurtured", DaggerStatsMain.Instance.nymphsNurtured.ToString(), column1X, column2X);
            AddStatLine("Orcs Offset", DaggerStatsMain.Instance.orcsOffset.ToString(), column1X, column2X);
            AddStatLine("Spriggans Shooed", DaggerStatsMain.Instance.spriggansShushed.ToString(), column1X, column2X);

            currentY += sectionSpacing;

            // MAGIC STATS
            magicStatsLabel.Position = new Vector2(column1X, currentY);
            magicStatsLabel.TextColor = Color.magenta;
            magicStatsLabel.TextScale = 1.3f;
            magicStatsLabel.Text = "Magic";
            scrollingPanel.Components.Add(magicStatsLabel);
            currentY += sectionSpacing;

            AddStatLine("Spells Learned", DaggerStatsMain.Instance.spellsLearned.ToString(), column1X, column2X);
            AddStatLine("Spells Cast", DaggerStatsMain.Instance.spellsCast.ToString(), column1X, column2X);
            AddSpacerLine("   ", column1X, column2X);
            AddStatLine("Favorite Alteration Spell", DaggerStatsMain.Instance.favoriteAlterationSpell, column1X, column2X);
            AddStatLine("Favorite Destruction Spell", DaggerStatsMain.Instance.favoriteDestructionSpell, column1X, column2X);
            AddStatLine("Favorite Illusion Spell", DaggerStatsMain.Instance.favoriteIllusionSpell, column1X, column2X);
            AddStatLine("Favorite Mysticism Spell", DaggerStatsMain.Instance.favoriteMysticismSpell, column1X, column2X);
            AddStatLine("Favorite Restoration Spell", DaggerStatsMain.Instance.favoriteRestorationSpell, column1X, column2X);
            AddStatLine("Favorite Thaumaturgy Spell", DaggerStatsMain.Instance.favoriteThaumaturgySpell, column1X, column2X);

            currentY += sectionSpacing;

            // CRIME STATS
            crimeStatsLabel.Position = new Vector2(column1X, currentY);
            crimeStatsLabel.TextColor = Color.green;
            crimeStatsLabel.TextScale = 1.3f;
            crimeStatsLabel.Text = "Crime";
            scrollingPanel.Components.Add(crimeStatsLabel);
            currentY += sectionSpacing;

            AddStatLine("Days Jailed", DaggerStatsMain.Instance.daysJailed.ToString(), column1X, column2X);
            AddStatLine("Assaults", DaggerStatsMain.Instance.assaultCount.ToString(), column1X, column2X);
            AddStatLine("Attempted B&Es", DaggerStatsMain.Instance.attemptedBNECount.ToString(), column1X, column2X);
            AddStatLine("Breaking and Entering", DaggerStatsMain.Instance.BNECount.ToString(), column1X, column2X);
            AddStatLine("Criminal Conspiracies", DaggerStatsMain.Instance.criminalConspiracyCount.ToString(), column1X, column2X);
            AddStatLine("Murders", DaggerStatsMain.Instance.murderCount.ToString(), column1X, column2X);
            AddStatLine("Pockets Picked", DaggerStatsMain.Instance.pickpocketCount.ToString(), column1X, column2X);
            AddStatLine("Stormcloak Rebellions", DaggerStatsMain.Instance.highTreasonCount.ToString(), column1X, column2X);
            AddStatLine("Thefts", DaggerStatsMain.Instance.theftCount.ToString(), column1X, column2X);
            AddStatLine("Trespasses", DaggerStatsMain.Instance.trespassingCount.ToString(), column1X, column2X);
            AddStatLine("Vagrancies", DaggerStatsMain.Instance.vagrancyCount.ToString(), column1X, column2X);

            // Calculate total scroll steps for ScrollingPanel
            scrollingPanel.ScrollSteps = (int)Mathf.Ceil((float)currentY / scrollingPanel.ScrollTransform) - (int)Mathf.Floor((float)scrollingPanel.Size.y / scrollingPanel.ScrollTransform);
            if (scrollingPanel.ScrollSteps < 0) scrollingPanel.ScrollSteps = 0;

            // Configure the VerticalScrollBar
            scroller.TotalUnits = currentY; 
            scroller.DisplayUnits = (int)scrollingPanel.Size.y; 
            scroller.ScrollIndex = 0; 

            
            exitButton.Size = new Vector2(43, 15);
            exitButton.Position = new Vector2(mainPanel.Size.x - exitButton.Size.x - horizontalPadding - 15, verticalPadding - 3);
            exitButton.OnMouseClick += ExitButton_OnMouseClick;
            //exitButton.Label.Text = "Exit";
            //exitButton.BackgroundColor = Color.red;
            mainPanel.Components.Add(exitButton); 
        {}
        #endregion
        DaggerfallUI.Instance.PlayOneShot(SoundClips.ButtonClick);
        }
        public override void CancelWindow()
        {
        DaggerStatsMain.openedDaggerStatsWindow = false;
        base.CancelWindow();
        
        }

        private void ExitButton_OnMouseClick(BaseScreenComponent sender, Vector2 position)
        {
            DaggerfallUI.Instance.PlayOneShot(SoundClips.ButtonClick);
            CloseWindow();
            DaggerStatsMain.openedDaggerStatsWindow = false;
        }

        private void Scroller_OnScroll()
        {
            int newScrollStep = (int)(scroller.ScrollIndex / scrollingPanel.ScrollTransform);
            scrollingPanel.ScrollIndex = newScrollStep;
        }

        protected virtual void ScrollingPanel_OnMouseScrollDown(BaseScreenComponent sender)
        {
            scrollingPanel.ScrollIndex++;
            scroller.ScrollIndex = scrollingPanel.ScrollIndex * scrollingPanel.ScrollTransform;
        }

        protected virtual void ScrollingPanel_OnMouseScrollUp(BaseScreenComponent sender)
        {
            scrollingPanel.ScrollIndex--;
            scroller.ScrollIndex = scrollingPanel.ScrollIndex * scrollingPanel.ScrollTransform;
        }

        public class ScrollingPanel : Panel
        {
            int scrollTransform = 7;
            int scrollSteps = 0;
            int scrollIndex = 0;

            public int ScrollTransform
            {
                get { return scrollTransform; }
                set { scrollTransform = value; }
            }

            public int ScrollSteps
            {
                get { return scrollSteps; }
                set { scrollSteps = value; }
            }

            public int ScrollIndex
            {
                get { return scrollIndex; }
                set { SetScrollIndex(value); }
            }

            void SetScrollIndex(int value)
            {
                scrollIndex = Mathf.Clamp(value, 0, scrollSteps);
            }

            public override void Draw()
            {
                if (!Enabled)
                    return;

                Rect myRect = new Rect(Position, Size);
                foreach (BaseScreenComponent component in Components)
                {
                    if (component.Enabled)
                    {
                        Vector2 pos = component.Position;
                        component.Position += new Vector2(0, -scrollIndex * scrollTransform);

                        if (myRect.Contains(component.Position))
                            component.Draw();

                        component.Position = pos;
                    }
                }
            }
        }
    }
}