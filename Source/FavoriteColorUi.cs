using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace FavoriteColorPicker;

public static class FavoriteColorUi
{
    private static List<Color>? colors;

    public static void Open(Pawn pawn)
    {
        if (pawn.story?.favoriteColor == null)
        {
            return;
        }

        ColorDef current = pawn.story.favoriteColor;
        Find.WindowStack.Add(new Dialog_ChooseColor(
            "FCP.DialogHeader".Translate(),
            current.color,
            Colors,
            color => Apply(pawn, color)));
    }

    public static void TryMakeRectClickable(Rect rect, Pawn pawn)
    {
        if (pawn.story?.favoriteColor == null)
        {
            return;
        }

        Widgets.DrawHighlightIfMouseover(rect);
        TooltipHandler.TipRegion(rect, "FCP.ClickToChange".Translate());
        if (Widgets.ButtonInvisible(rect))
        {
            SoundDefOf.Click.PlayOneShotOnCamera();
            Open(pawn);
        }
    }

    public static void Apply(Pawn pawn, Color color)
    {
        if (pawn.story == null)
        {
            return;
        }

        ColorDef? match = DefFor(color);
        if (match == null)
        {
            return;
        }

        pawn.story.favoriteColor = match;
    }

    public static List<Color> Colors
    {
        get
        {
            if (colors == null)
            {
                colors = new List<Color>();
                foreach (ColorDef def in DefDatabase<ColorDef>.AllDefsListForReading)
                {
                    if (def.colorType != ColorType.Ideo && def.colorType != ColorType.Misc)
                    {
                        continue;
                    }

                    if (colors.Any(existing => existing.IndistinguishableFrom(def.color)))
                    {
                        continue;
                    }

                    colors.Add(def.color);
                }

                colors.SortByColor(c => c);
            }

            return colors;
        }
    }

    public static ColorDef? DefFor(Color color)
    {
        return DefDatabase<ColorDef>.AllDefsListForReading.FirstOrDefault(def =>
            (def.colorType == ColorType.Ideo || def.colorType == ColorType.Misc)
            && def.color.IndistinguishableFrom(color));
    }
}
