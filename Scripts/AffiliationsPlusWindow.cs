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
    public class AffiliationsPlusWindow : DaggerfallPopupWindow
    {
        Panel mainPanel = new Panel();
        Panel textPanel = new Panel();
        protected ScrollingPanel scrollingPanel = new ScrollingPanel();
        protected VerticalScrollBar scroller = new VerticalScrollBar();
        MultiFormatTextLabel textLabel = new MultiFormatTextLabel();
        Button guildsTabButton;
        Button religiousTabButton;
        Button knightsTabButton;
        Button covensAndClansTabButton;
        Button notablePeopleTabButton;
        Button scrollUpButton;
        Button scrollDownButton;
        Button exitButton;
        Texture2D nativeTexture;
        protected Texture2D buttonTexture;
        const string nativeImgName = "BOOK00I0.IMG";
        const string buttonImgName = "AP_BUTTON_BG";

        // Spacing Values
        int currentY = 22;
        int lineHeight = 5;

        public AffiliationsPlusWindow(IUserInterfaceManager uiManager) : base(uiManager)
        {
            ParentPanel.BackgroundColor = Color.clear;
        }

        protected override void Setup()
        {
            nativeTexture = DaggerfallUI.GetTextureFromImg(nativeImgName);
            if (!nativeTexture)
                throw new Exception("[StatsPlus] Could not load background texture.");

            Texture2D tex;
            bool textureLoaded = TextureReplacement.TryImportTexture(buttonImgName, true, out tex);
            buttonTexture = tex;

            if (textureLoaded && buttonTexture != null)
            {
                buttonTexture.filterMode = FilterMode.Point;
                //Debug.Log("[StatsPlus] Successfully loaded texture: " + baseTextureName);
            }
            else
            {
                Debug.LogError("[StatsPlus] FAILED to load texture: " + buttonImgName + ". Check path and filename.");
            }

            if (AffiliationsPlusMain.overrideDefaultSettings == true)
            {
                int baseHeight = 7;
                lineHeight = Mathf.Max(1, Mathf.FloorToInt(baseHeight * (AffiliationsPlusMain.textScale)));
                Debug.Log($"[StatsPlus] line height is now {lineHeight}");
            }

            float horizontalPadding = 10; 
            float verticalPadding = 20;  
            float scrollerWidth = 8;

            mainPanel.HorizontalAlignment = HorizontalAlignment.Center;
            mainPanel.VerticalAlignment = VerticalAlignment.Middle;
            //mainPanel.BackgroundTexture = baseTexture;
            mainPanel.Position = Vector2.zero;
            mainPanel.Size = ParentPanel.Size;
            //mainPanel.Outline.Enabled = true;
            mainPanel.Outline.Color = Color.green; // Make it a bright color for debugging
            NativePanel.Components.Add(mainPanel);

            mainPanel.Size = new Vector2(320, 200);

            // Center mainPanel
            mainPanel.Position = new Vector2(
                (ParentPanel.Size.x / 2) - (mainPanel.Size.x / 2),
                (ParentPanel.Size.y / 2) - (mainPanel.Size.y / 2)
            );

            // Text Panel
            textPanel.Position = new Vector2(horizontalPadding, verticalPadding);
            textPanel.Size = new Vector2(mainPanel.Size.x - (2 * horizontalPadding) - scrollerWidth - 2, mainPanel.Size.y - (2 * verticalPadding));
            // textPanel.BackgroundColor = Color.clear;
            textPanel.BackgroundTexture = nativeTexture;
            textPanel.BackgroundTextureLayout = BackgroundLayout.StretchToFill;
            //textPanel.Outline.Enabled = true; // Debugging
            textPanel.Outline.Color = Color.yellow; // Yellow outline for debugging
            mainPanel.Components.Add(textPanel);

            // Scrolling Panel
            scrollingPanel.Position = new Vector2(horizontalPadding, verticalPadding);
            scrollingPanel.Size = new Vector2(textPanel.Size.x - 35, textPanel.Size.y - 35); 
            //scrollingPanel.Outline.Enabled = true; // Debugging
            scrollingPanel.Outline.Color = Color.cyan; // Blue outline for debugging
            scrollingPanel.HorizontalAlignment = HorizontalAlignment.Center;
            scrollingPanel.VerticalAlignment = VerticalAlignment.Middle;
            scrollingPanel.OnMouseScrollUp += ScrollingPanel_OnMouseScrollUp;
            scrollingPanel.OnMouseScrollDown += ScrollingPanel_OnMouseScrollDown;
            textPanel.Components.Add(scrollingPanel);

            // Scroller
            scroller.Position = new Vector2(scrollingPanel.Position.x + scrollingPanel.Size.x + 2, scrollingPanel.Position.y);
            scroller.Size = new Vector2(scrollerWidth, scrollingPanel.Size.y);
            scroller.OnScroll += Scroller_OnScroll;
            textPanel.Components.Add(scroller);

            scrollingPanel.ScrollTransform = 5;

            // Text Label
            textLabel.Position = new Vector2 (0, 0);
            textLabel.Size = new Vector2(scrollingPanel.Size.x - 10, scrollingPanel.Size.y - 10);
            textLabel.TextScale = 0.85f;
            if (AffiliationsPlusMain.overrideDefaultSettings == true)
            {
                textLabel.TextScale = AffiliationsPlusMain.textScale;
            }
            textLabel.HorizontalAlignment = HorizontalAlignment.Center;
            scrollingPanel.Components.Add(textLabel);

            // Buttons
            guildsTabButton = CreateTabButton("Guilds & Locals", new Vector2(10, 2), OnGuildsTabClicked);
            knightsTabButton = CreateTabButton("Knightly Orders", new Vector2(70, 2), OnKnightsTabClicked);
            religiousTabButton = CreateTabButton("Deities & Daedra", new Vector2(130, 2), OnReligiousTabClicked);
            covensAndClansTabButton = CreateTabButton("Covens & Clans", new Vector2(190, 2), OnCovensAndClansTabClicked);
            notablePeopleTabButton = CreateTabButton("Notable People", new Vector2(250, 2), OnNotablePeopleTabClicked);

            // Scroll Buttons
            scrollUpButton = CreateExitButton(new Vector2(164, 170), () => OnClickScrollUp());
            scrollDownButton = CreateExitButton(new Vector2(192, 170), () => OnClickScrollDown());

            // Exit button now routes through CancelWindow(), matching ESC key behavior.
            exitButton = CreateExitButton(new Vector2(262, 170), OnExitClicked);

            mainPanel.Components.Add(guildsTabButton);
            mainPanel.Components.Add(knightsTabButton);
            mainPanel.Components.Add(religiousTabButton);
            mainPanel.Components.Add(covensAndClansTabButton);
            mainPanel.Components.Add(notablePeopleTabButton);
            mainPanel.Components.Add(scrollUpButton);
            mainPanel.Components.Add(scrollDownButton);
            mainPanel.Components.Add(exitButton);

            OnGuildsTabClicked(); // default tab
        }

        Button CreateTabButton(string text, Vector2 pos, System.Action handler)
        {
            var button = new Button
            {
                Position = pos,
                Size = new Vector2(56, 18),
                Label = { Text = text },
                BackgroundTexture = buttonTexture
            };
            button.OnMouseClick += (sender, args) => handler();
            return button;
        }

        Button CreateExitButton(Vector2 pos, System.Action handler)
        {
            var button = new Button
            {
                Position = pos,
                Size = new Vector2(28, 10),
            };
            button.OnMouseClick += (sender, args) => handler();
            //button.Outline.Enabled = true; // Debugging
            //button.Outline.Color = Color.cyan; // Blue outline for debugging
            return button;
        }

        void OnGuildsTabClicked() => ShowGuildsText();
        void OnKnightsTabClicked() => ShowKnightsText();
        void OnReligiousTabClicked() => ShowReligiousText();
        void OnCovensAndClansTabClicked() => ShowCovensAndClansText();
        void OnNotablePeopleTabClicked() => ShowNotablePeopleText();

        void ShowGuildsText() => RefreshText(AffiliationsPlusMain.guildsAndLocalsTokens);
        void ShowKnightsText() => RefreshText(AffiliationsPlusMain.knightsTokens);
        void ShowReligiousText() => RefreshText(AffiliationsPlusMain.religiousTokens);
        void ShowCovensAndClansText() => RefreshText(AffiliationsPlusMain.covensAndClansTokens);
        void ShowNotablePeopleText() => RefreshText(AffiliationsPlusMain.notablePeopleTokens);

        void RefreshText(List<TextFile.Token> tokens)
        {
            currentY = 0; //22
            textLabel.Clear();
            textLabel.Position = Vector2.zero;
            SetText(tokens.ToArray());

            // Calculate total scroll steps for ScrollingPanel
            scrollingPanel.ScrollSteps = (int)Mathf.Ceil((float)currentY / scrollingPanel.ScrollTransform) - (int)Mathf.Floor((float)scrollingPanel.Size.y / scrollingPanel.ScrollTransform);
            if (scrollingPanel.ScrollSteps < 0) scrollingPanel.ScrollSteps = 0;

            // Configure the VerticalScrollBar
            scroller.TotalUnits = currentY; 
            scroller.DisplayUnits = (int)scrollingPanel.Size.y; 
            scroller.ScrollIndex = 0; 

            // Play the Button Click
            DaggerfallUI.Instance.PlayOneShot(SoundClips.ButtonClick);
        }

        public void SetText(TextFile.Token[] tokens)
        {
            currentY = 0;
            int lineCount = 1; // start with 1 line by default

            for (int i = 0; i < tokens.Length; i++)
            {
                if (tokens[i].formatting == TextFile.Formatting.NewLine)
                {
                    lineCount++;
                }
            }

            currentY = lineCount * lineHeight;

            // Set tokens
            textLabel.SetText(tokens);
        }

        // public bool IsStructuralToken(TextFile.Token token)
        // {
        //     return token.formatting == TextFile.Formatting.PositionPrefix ||
        //     token.formatting == TextFile.Formatting.TextHighlight ||
        //     token.formatting == TextFile.Formatting.TextQuestion ||
        //     token.formatting == TextFile.Formatting.NewLine;
        // }

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

        void OnClickScrollDown()
        {
            scrollingPanel.ScrollIndex++;
            scroller.ScrollIndex = scrollingPanel.ScrollIndex * scrollingPanel.ScrollTransform;
            DaggerfallUI.Instance.PlayOneShot(SoundClips.ButtonClick);
        }

        void OnClickScrollUp()
        {
            scrollingPanel.ScrollIndex--;
            scroller.ScrollIndex = scrollingPanel.ScrollIndex * scrollingPanel.ScrollTransform;
            DaggerfallUI.Instance.PlayOneShot(SoundClips.ButtonClick);
        }

        void OnExitClicked()
        {
            // Keep physical clicks and ESC cancellation on the exact same closure path.
            CancelWindow();
        }

        public override void CancelWindow()
        {
            // ESC key cancellation must also release the hotkey guard in AffiliationsPlusMain.
            AffiliationsPlusMain.ResetWindowOpenState();
            base.CancelWindow();
        }

        public class ScrollingPanel : Panel
        {
            int scrollTransform = 5;
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

                Rect panelRect = this.Rectangle;

                foreach (BaseScreenComponent component in Components)
                {
                    if (!component.Enabled)
                        continue;

                    Vector2 originalPos = component.Position;
                    MultiFormatTextLabel multi = component as MultiFormatTextLabel;
                    component.Position += new Vector2(0, -scrollIndex * scrollTransform);

                    if (multi != null)
                    {
                        foreach (TextLabel label in multi.TextLabels)
                        {
                            Rect labelRect = label.Rectangle;

                            if (panelRect.Overlaps(labelRect))
                                label.Draw();
                        }
                    }
                    else
                    {
                        Rect compRect = component.Rectangle;

                        if (panelRect.Overlaps(compRect))
                            component.Draw();
                    }

                    component.Position = originalPos;
                }
            }
        }
    }
}
