# SoftwareIncMod1

A [Software Inc.](https://store.steampowered.com/app/362620/Software_Inc/) mod with two halves:

- **Code mod** (`src/`): a C# class library the game loads from `DLLMods/`. It can change behaviour, add UI and hook game events.
- **Data mod** (`data/`): TyD and text files the game loads from `Mods/`. They add software types, AI company types, name generators and personalities.

Both halves are optional. Delete the one you don't need.

## Layout

```
├── Directory.Build.props          Mod name, version and game path defaults
├── GamePath.user.props.example    Copy to GamePath.user.props to set your install path
├── SoftwareIncMod1.sln
├── src/SoftwareIncMod1/
│   ├── SoftwareIncMod1.csproj     Build, deploy and package targets
│   ├── ModMain.cs                 ModMeta: mod name and options panel
│   └── WelcomeBehaviour.cs        Sample ModBehaviour: lifecycle, settings, save data
├── data/                          Deployed to <game>/Mods/SoftwareIncMod1/
│   ├── meta.tyd                   Name, description and author shown in game
│   ├── SoftwareTypes/             New or overridden software types
│   ├── CompanyTypes/              AI companies that make your software types
│   └── NameGenerators/            Product and company name generators
└── docs/modding-reference.md      API and file-format cheat sheet
```

## Setup

1. Install Software Inc. and the [.NET SDK](https://dotnet.microsoft.com/download) (8 or newer). You don't need a separate .NET Framework install; the reference assemblies come from NuGet automatically. Visual Studio 2022 and Rider open the `.sln` directly.
2. Tell the build where the game is. If it's in the default Steam library, skip this step.
   - Copy `GamePath.user.props.example` to `GamePath.user.props` and set `SoftwareIncDir`, **or**
   - set an environment variable named `SoftwareIncDir`.

   | OS | Default `SoftwareIncDir` |
   |---|---|
   | Windows | `C:\Program Files (x86)\Steam\steamapps\common\Software Inc` |
   | Linux | `~/.local/share/Steam/steamapps/common/Software Inc` |
   | macOS | `~/Library/Application Support/Steam/steamapps/common/Software Inc` |

3. In game, bind a key to the debug console (Options → Key bindings, at the bottom). It shows mod errors and enables the reload commands below.

## Build and test

```bash
dotnet build
```

Each build compiles the DLL against the game's assemblies, then copies it to `<game>/DLLMods/SoftwareIncMod1/` and copies `data/` to `<game>/Mods/SoftwareIncMod1/`. To build without copying, add `-p:DeployToGame=false`. Treat those game folders as build output and edit files here, because the deploy step overwrites them.

Enable the mod in the **Mods** section of the options window. The game warns before it activates code mods.

Useful console commands while iterating:

| Command | Use |
|---|---|
| `RELOAD_DLL_MOD`, `RECOMPILE_DLL_MOD`, `UNLOAD_DLL_MOD` | Reload the code mod without restarting |
| `RELOAD_MOD SoftwareIncMod1` | Reload the data mod (does not change a game already running) |
| `TEST_DEV_MOD <mod> <software> <category>` | Check that a software type's submarkets can reach 100% |
| `EXECUTE <SIPL code>` | Run code against the live game, e.g. `EXECUTE GameSettings.MyCompany` |
| `SHOW_INSPECTOR` | Browse every object in the scene |

## Release

```bash
dotnet build -c Release -t:Package
```

This writes `dist/SoftwareIncMod1-<version>.zip`, laid out like the game folder (`DLLMods/…` and `Mods/…`), so players can extract it into their install. Set the version in `Directory.Build.props`.

**Steam Workshop:** the Workshop accepts code mods only as `.cs` source that the game compiles itself, not as DLLs. That compiler only understands **C# 3** and crashes on **enums**. To check that the code still meets those rules:

```bash
dotnet build -p:WorkshopCompat=true
```

That build fails on newer syntax (`=>` members, `$"..."` strings, `?.`, optional parameters) and on enum declarations. The sample code already passes it.

## Rules worth knowing

- **Don't change `ModMain.Name` after release.** The game stores each mod's save data under that name, so renaming it loses the data in existing saves.
- Do per-frame work in a ModBehaviour's `Update()`. Don't use threads or timers, because Unity objects can't be touched from other threads.
- Classes saved through `WriteDictionary` need an empty constructor. Saving your own classes also breaks saves once the mod is uninstalled, so prefer simple types.
- File writes and network access need `public static bool GiveMeFreedom` on the ModMeta class. That only works for DLL mods (not Workshop), and the player is warned.
- A software type without a matching company type is never released by AI companies.

## Renaming the mod

Change `ModName` in `Directory.Build.props`, `Name` in `ModMain.cs`, the `namespace` lines and log prefixes in `src/`, `Name` in `data/meta.tyd`, and rename the `src/SoftwareIncMod1/` folder and `.csproj` (then update the path in the `.sln`).

## References

- [Modding overview](https://softwareinc.coredumping.com/wiki/index.php/Modding)
- [Code modding](https://softwareinc.coredumping.com/wiki/index.php/Code_Modding)
- [Data modding](https://softwareinc.coredumping.com/wiki/index.php/Data_Modding)
- [TyD format](https://softwareinc.coredumping.com/wiki/index.php/TyD)
- [SIPL scripting](https://softwareinc.coredumping.com/wiki/index.php/SIPL)
- [Official example code mod (FloorMod.zip)](https://softwareinc.coredumping.com/FloorMod.zip)
