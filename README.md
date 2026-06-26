StatsPlusBETA! - Daggerstats, AffiliationPlus and Viewable Skill Progress as one Character Sheet overhaul!

## 🎯 More Project Goals & Architecture

* Deep Optimization: Rewritten C# backend architecture engineered to eliminate garbage collection (GC) stuttering, stop memory-swallowing loops, and keep your memory footprint perfectly flat over long play sessions.
* Streamlined Footprint: Unifies redundant UI tools under a single engine hood to drastically reduce mod-list bloat and enhance game initialization speed—perfect for heavy mod setups.
* Keep Great Mods Alive: Preserves and future-proofs the brilliant community work of Rith Essa and Magicono43 for modern DFU builds.
* Upcoming Integration: Viewable Skills Progress will be fully integrated into this unified framework soon (Permission officially received!). - It should be working now but I will be ironing out bugs before release.

------------------------------
## 🚀 Progress & Performance Breakthroughs So Far:

* 100% Harmony-Free: Completely stripped out heavy Harmony .dll reflection wrappers and method detours. By removing risky vanilla code hooking, this bundle eradicates mod conflicts and minimizes runtime processing overhead.
* Preserved Vanilla Integrity: Clicking the native "Affiliation" button on your character sheet still opens the clean, vanilla quick-check layout for quick reference.
* Dedicated UI Stack Keybinds: Access your expanded data overhauls safely directly from your character menu via custom input listeners that automatically respect UI window focus:
* Unified controls:
    * Press X on the character sheet to open the enhanced DaggerStats panel.
    * Press Shift + A on the character sheet to toggle the deep AffiliationsPlus window.
    * Click any skill to view your progress on the character sheet!
    * Press Esc Fix: to smoothly dismiss any of the custom screens and instantly return your cursor to the game.

------------------------------
## 🛠️ Developer, Tester, & Mac Compilation Notes

[!IMPORTANT]
This project is in active development and is currently intended for manual source-code review, peer compilation, and community testing.
Community testing, code reviews on our update loops, and compiled .dfmod feedback are highly appreciated! Please feel free to open an Issue, submit a Pull Request, or reach out in the Discord thread.

## Yet to Fix
* Everything seems to be working well other than save persistence on the DaggerStatsMain script. For some reason the SaveData is not saving across play sessions (causing the stats menu to whipe and reset when when the player loads game). I should have this fixed soon.
