using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace FavoriteColorPicker;

/// <summary>
/// Vanilla draws the Bio-tab favorite-color square inside a compiler-generated
/// stack-element drawer. Patch that drawer so the square is a button.
/// </summary>
[HarmonyPatch]
public static class Patch_FavoriteColorBox
{
    // CharacterCardUtility.DoTopStack tooltip id for the favorite-color box.
    private const int FavoriteColorTooltipId = 837472764;

    public static MethodBase? TargetMethod()
    {
        foreach (Type nested in typeof(CharacterCardUtility).GetNestedTypes(BindingFlags.NonPublic))
        {
            foreach (MethodInfo method in AccessTools.GetDeclaredMethods(nested))
            {
                ParameterInfo[] parameters = method.GetParameters();
                if (parameters.Length != 1 || parameters[0].ParameterType != typeof(Rect))
                {
                    continue;
                }

                try
                {
                    if (PatchProcessor.GetOriginalInstructions(method).Any(IsFavoriteColorTooltipId))
                    {
                        return method;
                    }
                }
                catch (Exception e)
                {
                    Log.Warning("[Favorite Color Picker] Could not inspect " + method.FullDescription() + ": " + e.Message);
                }
            }
        }

        Log.Error("[Favorite Color Picker] Could not find the Bio tab favorite-color drawer. The color box will not be clickable.");
        return AccessTools.Method(typeof(Patch_FavoriteColorBox), nameof(DummyTarget));
    }

    private static void DummyTarget(Rect r)
    {
    }

    private static bool IsFavoriteColorTooltipId(CodeInstruction instruction)
    {
        return instruction.opcode == OpCodes.Ldc_I4 && instruction.operand is int id && id == FavoriteColorTooltipId;
    }

    public static void Postfix(object __instance, Rect r)
    {
        Pawn? pawn = GetCapturedPawn(__instance);
        if (pawn?.story?.favoriteColor == null)
        {
            return;
        }

        Widgets.DrawHighlightIfMouseover(r);
        TooltipHandler.TipRegion(r, "FCP.ClickToChange".Translate());
        if (Widgets.ButtonInvisible(r))
        {
            SoundDefOf.Click.PlayOneShotOnCamera();
            OpenPicker(pawn);
        }
    }

    private static Pawn? GetCapturedPawn(object displayClass)
    {
        FieldInfo? field = AccessTools.Field(displayClass.GetType(), "pawn");
        if (field?.GetValue(displayClass) is Pawn named)
        {
            return named;
        }

        foreach (FieldInfo candidate in AccessTools.GetDeclaredFields(displayClass.GetType()))
        {
            if (candidate.FieldType == typeof(Pawn) && candidate.GetValue(displayClass) is Pawn pawn)
            {
                return pawn;
            }
        }

        return null;
    }

    private static void OpenPicker(Pawn pawn)
    {
        ColorDef current = pawn.story.favoriteColor;
        Find.WindowStack.Add(new Dialog_ChooseColor(
            "FCP.DialogHeader".Translate(),
            current.color,
            FavoriteColorPalette.Colors,
            color => ApplyColor(pawn, color)));
    }

    private static void ApplyColor(Pawn pawn, Color color)
    {
        if (pawn.story == null)
        {
            return;
        }

        ColorDef? match = FavoriteColorPalette.DefFor(color);
        if (match == null)
        {
            return;
        }

        pawn.story.favoriteColor = match;
    }
}

public static class FavoriteColorPalette
{
    private static List<Color>? colors;

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
