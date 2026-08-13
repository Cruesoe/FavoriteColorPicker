using HarmonyLib;
using Verse;

namespace FavoriteColorPicker;

[StaticConstructorOnStartup]
public static class Startup
{
    static Startup()
    {
        new Harmony("cruesoe.favoritecolorpicker").PatchAll();
    }
}
