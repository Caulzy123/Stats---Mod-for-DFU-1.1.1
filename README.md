Special thanks to Rith Essa, the creator of the original mods.

## 🛠️ Important Note for Developers & Testers
Because this project is in active development, it is currently intended for manual compilation and testing.

⚠️ Mac Compilation Note: As a macOS user, building final .dfmod files can be problematic. Community testing and feedback are highly appreciated!

* Alternative: Compile a fresh .dfmod.json configuration directly via the Unity editor's Build Mod menu.

------------------------------
## 🎯 Project Goals

* Keep Great Mods Alive: Bundle and preserve the excellent work of Rith Essa (DaggerStats and AffiliationsPlus).
* Deep Optimisation: Rewrite backend logic to stop data-swallowing, eliminate Garbage Collection stuttering, and ensure compatibility across all devices.
* Streamlined Architecture: Unify similar UI tools under one hood to reduce mod-list bloat and improve game loading stability.
* Viewable Skill Progress will be bundled into this soon (Permission recieved), with unified framework!

------------------------------
## 🚀 Progress So Far

* Zero Harmony.dfmod or .dll: No more .dll file overrides or risky vanilla code hooking. This massively reduces conflicts with other mods.
* Dedicated Window Keybinds: Access mod windows directly from your character sheet without cluttering the main HUD.
* Press X on the character sheet to open DaggerStats.
   * Press Shift + A on the character sheet to open AffiliationsPlus.
* Preserved Vanilla Layouts: Clicking the native "Affiliation" button still opens the smaller, vanilla quick-check window for easy reading.
