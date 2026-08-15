# Favorite Color Picker

RimWorld 1.6 Ideology mod. The small favorite-color square on a pawn's **Bio** tab becomes a button. Click it to pick a new favorite color from the same Ideology / misc color set vanilla uses.

Works with [Bio Tab+](https://steamcommunity.com/sharedfiles/filedetails/?id=3781978940), which relocates that square.

Requires [Harmony](https://steamcommunity.com/sharedfiles/filedetails/?id=2009463077) and **Ideology**.

## Install

Copy this folder to `RimWorld\Mods\`, or add it as a local mod in RimSort.

## Build

```
dotnet build Source\FavoriteColorPicker.csproj -c Debug
```

The DLL is copied to `1.6\Assemblies\FavoriteColorPicker.dll` and to `RimWorld\Mods\Favorite Color Picker\1.6\Assemblies\` if that folder exists.
