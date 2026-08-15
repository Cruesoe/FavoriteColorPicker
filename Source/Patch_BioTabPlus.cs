using System;
using System.Reflection;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace FavoriteColorPicker;

/// <summary>
/// Bio Tab+ skips vanilla CharacterCardUtility.DoTopStack and draws its own
/// identity-tag swatch, which has no click handler. While those tags draw,
/// treat a rounded rect in the pawn's favorite color as the picker button.
/// </summary>
[HarmonyPatch]
public static class Patch_BioTabPlus_DrawIdentityTags
{
    internal static Pawn? CurrentPawn;

    public static bool Prepare()
    {
        if (CardType() == null)
        {
            return false;
        }

        Log.Message("[Favorite Color Picker] Bio Tab+ detected; favorite-color swatch will be clickable.");
        return true;
    }

    public static MethodBase? TargetMethod()
    {
        return AccessTools.Method(CardType(), "DrawIdentityTags");
    }

    public static void Prefix(Pawn pawn)
    {
        CurrentPawn = pawn;
    }

    public static void Postfix()
    {
        CurrentPawn = null;
    }

    internal static Type? CardType()
    {
        return AccessTools.TypeByName("BioTabEnhanced.EnhancedCharacterCard");
    }
}

[HarmonyPatch]
public static class Patch_BioTabPlus_DrawRoundedRect
{
    public static bool Prepare()
    {
        return Patch_BioTabPlus_DrawIdentityTags.CardType() != null;
    }

    public static MethodBase? TargetMethod()
    {
        return AccessTools.Method(Patch_BioTabPlus_DrawIdentityTags.CardType(), "DrawRoundedRect");
    }

    public static void Postfix(Rect rect, Color color)
    {
        Pawn? pawn = Patch_BioTabPlus_DrawIdentityTags.CurrentPawn;
        ColorDef? favorite = pawn?.story?.favoriteColor;
        if (favorite == null || !color.IndistinguishableFrom(favorite.color))
        {
            return;
        }

        FavoriteColorUi.TryMakeRectClickable(rect, pawn!);
    }
}
