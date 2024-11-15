﻿using TheOtherRoles.Utilities;
using UnityEngine;

namespace TheOtherRoles.Roles.Impostor;

public static class Camouflager
{
    public static PlayerControl camouflager;
    public static Color color = Palette.ImpostorRed;

    public static float cooldown = 30f;
    public static float duration = 10f;
    public static float camouflageTimer;
    public static bool camoComms;

    public static ResourceSprite buttonSprite = new("CamoButton.png");

    public static void resetCamouflage()
    {
        if (isCamoComms) return;
        camouflageTimer = 0f;
        foreach (PlayerControl p in CachedPlayer.AllPlayers)
        {
            if ((p == Ninja.ninja && Ninja.isInvisble)
                || (p == Swooper.swooper && Swooper.isInvisable)
                || (p == Jackal.jackal && Jackal.isInvisable))
                continue;
            p.setDefaultLook();
            camoComms = false;
        }
    }

    public static void clearAndReload()
    {
        resetCamouflage();
        camoComms = false;
        camouflager = null;
        camouflageTimer = 0f;
        cooldown = CustomOptionHolder.camouflagerCooldown.getFloat();
        duration = CustomOptionHolder.camouflagerDuration.getFloat();
    }
}
