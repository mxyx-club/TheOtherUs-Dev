﻿using UnityEngine;

namespace TheOtherRoles.Roles.Neutral;

public class Sidekick
{
    public static PlayerControl sidekick;
    public static Color color = new Color32(0, 180, 235, byte.MaxValue);

    public static PlayerControl currentTarget;

    public static bool wasTeamRed;
    public static bool wasImpostor;
    public static bool wasSpy;

    public static float cooldown = 30f;
    public static bool canUseVents = true;
    public static bool canKill = true;
    public static bool promotesToJackal = true;
    public static bool hasImpostorVision;

    public static void clearAndReload()
    {
        sidekick = null;
        currentTarget = null;
        cooldown = CustomOptionHolder.jackalKillCooldown.GetFloat();
        canUseVents = CustomOptionHolder.sidekickCanUseVents.GetBool();
        canKill = CustomOptionHolder.sidekickCanKill.GetBool();
        promotesToJackal = CustomOptionHolder.sidekickPromotesToJackal.GetBool();
        hasImpostorVision = CustomOptionHolder.jackalAndSidekickHaveImpostorVision.GetBool();
        wasTeamRed = wasImpostor = wasSpy = false;
    }
}
