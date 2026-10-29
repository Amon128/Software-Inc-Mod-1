# Software Inc. modding reference

Condensed from the [official wiki](https://softwareinc.coredumping.com/wiki/index.php/Modding) and the official FloorMod example, as of October 2026. When something here disagrees with what your IDE shows for the game's `Assembly-CSharp.dll`, trust the IDE.

## Where mods go

Each mod gets its own folder inside one of these game-root folders. Create them if they don't exist.

| Folder | Contents |
|---|---|
| `DLLMods/<Mod>/` | Compiled `.dll`, or `.cs` files for the game to compile |
| `Mods/<Mod>/` | Software types, company types, name generators, personalities (TyD) |
| `Furniture/<Mod>/` | Furniture (`.obj` + TyD) |
| `Materials/<Mod>/` | Wall, floor, roof and path textures |
| `Localization/<Lang>/` | Translations (TyD) |

## Code mods

### ModMeta (exactly one per mod)

| Member | Notes |
|---|---|
| `string Name { get; }` | Abstract. Display name, **and the key for save data**. |
| `void ConstructOptionsScreen(RectTransform parent, bool inGame)` | Abstract. Fill `parent` with the options UI. |
| `void Initialize(ModController.DLLMod parentMod)` | Optional. Runs once on load. Call `base.Initialize(parentMod)`. |
| `WriteDictionary Serialize(GameReader.LoadMode mode)` / `void Deserialize(WriteDictionary data, GameReader.LoadMode mode)` | Optional. Takes over all saving; the default calls each ModBehaviour's methods. Returning null saves nothing. |
| `void ReceiveNetworkMessage(SINetworking.NetworkPlayer player, MemoryStream stream)` | Optional. Multiplayer messages. |
| `public static bool GiveMeFreedom` | Unlocks file and network access (DLL mods only, player is warned). |

### ModBehaviour (any number; each is a Unity `MonoBehaviour`)

| Member | Notes |
|---|---|
| `void OnActivate()` / `void OnDeactivate()` | Abstract. Called when the mod is toggled on or off, and on launch if enabled. Undo all changes in `OnDeactivate`. |
| `Awake`, `Start`, `Update`, `OnDestroy` | Normal Unity lifecycle. The component is enabled only while the mod is active. |
| `LoadSetting<T>(key, default)`, `TryLoadSetting`, `SaveSetting(key, value)`, `DeleteSetting(key)` | Global settings shared by all saves. Simple types only (stored via `ToString()`). |
| `void Serialize(WriteDictionary data, GameReader.LoadMode mode)` / `void Deserialize(...)` | Per-save data (Beta 1.8.34+). `data["Key"] = value;` and `data.Get("Key", default)`. |
| `ParentMod.LoadTexture`, `LoadXMLFile`, `LoadFullXMLFile`, `LoadTydFile`, `LoadAudio`, `LoadGLTF`, `LoadOBJ` | Load files relative to the mod folder (`../` doesn't work). |
| `ParentMod.RegisterNetworkID(id)`, `ParentMod.SendNetworkMessage(byte[] data, MessageTarget target, byte targetID)` | Multiplayer. Pick an ID from 1 to 255 that's unlikely to clash with other mods. |

### UI helpers

- `WindowManager.SpawnLabel()`, `WindowManager.SpawnInputbox()` and the other `WindowManager.Spawn*` methods create elements.
- `WindowManager.AddElementToElement(child, parent, rect, anchor)` places them.
- `WindowManager.Instance.ShowMessageBox(text, true, DialogWindow.DialogType.Information)` shows a dialog.
- `WindowManager.GenerateUI(ParentMod.LoadFullXMLFile("UI.xml"), null, this)` builds UI from HTML-like XML. Tags include `window`, `panel`, `button`, `label`, `input`, `checkbox`, `combo`, `scrollview` and `verticallayout`. Attributes are case-sensitive.
- `HUD.Instance.AddBottomButton(panel, name, desc, sprite)` adds a button to the bottom HUD. `ObjectDatabase.Instance.GetIcon(name)` returns a built-in icon; `SHOW_ICONS` lists them.
- To find a UI element's path for `WindowManager.FindElementPath`, use the `UI_UNDER_MOUSE` console command.

### Entry points and events

| Entry point | What it manages |
|---|---|
| `GameSettings.Instance` | Most game managers |
| `GameSettings.Instance.MyCompany` | The player's company |
| `MarketSimulation.Active` | All companies and products |
| `GameSettings.Instance.sRoomManager` | Rooms, furniture, segments |
| `GameSettings.Instance.sActorManager` | Employees and teams |
| `TimeOfDay.Instance`, `HUD.Instance`, `SelectorController.Instance`, `ObjectDatabase.Instance` | Time, HUD, selection, furniture database |

Events: `GameSettings.IsDoneLoadingGame`, `GameSettings.GameReady` (player has control), `GameSettings.OnQuit`, `TimeOfDay.OnHourPassed` / `OnDayPassed` / `OnMonthPassed`, `MarketSimulation.OnProductReleased` / `OnProductRemoved` / `OnCompanyFounded` / `OnCompanyClosed` / `OnTechResearched` / `OnFrameworkReleased`. Check each event's delegate type in your IDE before subscribing.

Scenes: `MainMenu`, `MainScene` (an active game) and `Customization`. Hook `SceneManager.sceneLoaded` to react to them.

### Game compiler (loose `.cs` files and Workshop)

- C# 3 only, and enums crash it. Check with `dotnet build -p:WorkshopCompat=true`.
- It injects error handling into every method. The `-DisableModErrors` launch option turns that off while developing.
- It defines version symbols such as `SWINCRELEASE`, `SWINCBETA1`, `SWINCBETA1_7` for `#if` blocks.

## Data mods

Files are [TyD](https://softwareinc.coredumping.com/wiki/index.php/TyD): `Record value`, `{ }` for tables, `[ a; b ]` for lists, `#` for comments, and `\"` to escape quotes inside a quoted string.

| File | Root record | Notes |
|---|---|---|
| `meta.tyd` | `Name`, `Description`, `Author` | Display info for the mod. |
| `SoftwareTypes/*.tyd` | `SoftwareType { … }` | `Override True` edits a built-in type by name; `Override Delete` removes it. Overriding `Features` replaces all of them. |
| `CompanyTypes/*.tyd` | `CompanyType { … }` | `CompanyTypes/delete.txt` removes built-in company types, one per line. |
| `NameGenerators/*.txt` | Node graph | `-node(next1,next2)` lines, followed by words. Starts at `start`, ends at `stop`. A first line of `[REPLACE]` replaces the built-in generator with the same name; otherwise they merge. |
| `Personalities.tyd` | `PersonalityGraph { … }` | Merged with the built-in personalities unless it sets `Replace True`. |

Software type essentials: `Name`, `Description`, `Unlock`, `IdealPrice`, `OptimalDevTime`, `SubmarketNames` (3), `Categories`, `Features`. Each feature has a `Spec` (built-in or new specialization) and `Submarkets` weights in `SubmarketNames` order. Subfeatures have a `Level` from 1 to 3. Level 3 subfeatures run [SIPL](https://softwareinc.coredumping.com/wiki/index.php/SIPL) scripts through `Script_EndOfDay`, `Script_AfterSales`, `Script_OnRelease`, `Script_NewCopies` or `Script_WorkItemChange`. A script can be written inline or as a path like `"/Scripts/MyScript.txt"`. The AI never picks level 3 features.

Vanilla data to copy from: [Beta 1.7 data zip](https://softwareinc.coredumping.com/wiki/Beta17Data.zip).

## Not verified

- The ModBehaviour `Serialize`/`Deserialize` signatures come from the wiki text, not from code. If they don't compile against your game version, use the signatures your IDE shows.
- The `meta.tyd` layout (three top-level records) is inferred; the wiki describes the records but gives no example.
- macOS paths for the game's DLLs and mod folders are unconfirmed.
