﻿using UnityEngine;

namespace TheOtherRoles.Roles.Crewmate;

public static class TimeMaster
{
    public static PlayerControl timeMaster;
    public static Color color = new Color32(112, 142, 239, byte.MaxValue);

    public static bool reviveDuringRewind;
    public static float rewindTime = 3f;
    public static float shieldDuration = 3f;
    public static float cooldown = 30f;

    public static bool shieldActive;
    public static bool isRewinding;

    public static ResourceSprite buttonSprite = new("TimeShieldButton.png");

    public static void clearAndReload()
    {
        timeMaster = null;
        isRewinding = false;
        shieldActive = false;
        rewindTime = CustomOptionHolder.timeMasterRewindTime.GetFloat();
        shieldDuration = CustomOptionHolder.timeMasterShieldDuration.GetFloat();
        cooldown = CustomOptionHolder.timeMasterCooldown.GetFloat();
    }
}
