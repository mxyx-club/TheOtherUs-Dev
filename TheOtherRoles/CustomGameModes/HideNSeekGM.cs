using System;
using System.Collections.Generic;
using TheOtherRoles.Objects;
using TheOtherRoles.Utilities;
using TMPro;
using UnityEngine;
using Object = UnityEngine.Object;

namespace TheOtherRoles.CustomGameModes;

public static class HideNSeek
{
    // HideNSeek Gamemode
    public static bool isHideNSeekGM;
    public static TMP_Text timerText;
    public static Vent polusVent;
    public static bool isWaitingTimer = true;
    public static DateTime startTime = DateTime.UtcNow;

    public static float hunterSpeed => CustomOptionHolder.hideHuntSpeed.GetFloat();
    public static float crewSpeed => CustomOptionHolder.hideCrewSpeed.GetFloat();

    public static float timer = 300f;
    public static float hunterVision = 0.5f;
    public static float huntedVision = 2f;
    public static bool taskWinPossible;
    public static float taskPunish = 10f;
    public static int impNumber = 2;
    public static bool canSabotage;
    public static float killCooldown = 10f;
    public static float hunterWaitingTime = 15f;

    public static bool isHunter()
    {
        return isHideNSeekGM && CachedPlayer.LocalPlayer != null && CachedPlayer.LocalPlayer.Data.Role.IsImpostor;
    }

    public static List<CachedPlayer> getHunters()
    {
        var hunters = new List<CachedPlayer>(CachedPlayer.AllPlayers);
        hunters.RemoveAll(x => !x.Data.Role.IsImpostor);
        return hunters;
    }

    public static bool isHunted()
    {
        return isHideNSeekGM && CachedPlayer.LocalPlayer != null && !CachedPlayer.LocalPlayer.Data.Role.IsImpostor;
    }

    public static void clearAndReload()
    {
        isHideNSeekGM = ModOption.gameMode == CustomGamemodes.HideNSeek;
        if (timerText != null) Object.Destroy(timerText);
        timerText = null;
        if (polusVent != null) Object.Destroy(polusVent);
        polusVent = null;
        isWaitingTimer = true;
        startTime = DateTime.UtcNow;

        timer = CustomOptionHolder.hideNSeekTimer.GetFloat() * 60;
        hunterVision = CustomOptionHolder.hideNSeekHunterVision.GetFloat();
        huntedVision = CustomOptionHolder.hideNSeekHuntedVision.GetFloat();
        taskWinPossible = CustomOptionHolder.hideNSeekTaskWin.GetBool();
        taskPunish = CustomOptionHolder.hideNSeekTaskPunish.GetFloat();
        impNumber = Mathf.RoundToInt(CustomOptionHolder.hideNSeekHunterCount.GetFloat());
        canSabotage = CustomOptionHolder.hideNSeekCanSabotage.GetBool();
        killCooldown = CustomOptionHolder.hideNSeekKillCooldown.GetFloat();
        hunterWaitingTime = CustomOptionHolder.hideNSeekHunterWaiting.GetFloat();

        Hunter.clearAndReload();
        Hunted.clearAndReload();
    }
}

public static class Hunter
{
    public static List<Arrow> localArrows = new();
    public static List<byte> lightActive = new();
    public static bool arrowActive;
    public static Dictionary<byte, int> playerKillCountMap = new();

    public static float lightCooldown = 30f;
    public static float lightDuration = 5f;
    public static float lightVision = 2f;
    public static float lightPunish = 5f;
    public static float AdminCooldown = 30f;
    public static float AdminDuration = 5f;
    public static float AdminPunish = 5f;
    public static float ArrowCooldown = 30f;
    public static float ArrowDuration = 5f;
    public static float ArrowPunish = 5f;
    public static ResourceSprite buttonSpriteLight = new("LighterButton.png");
    public static ResourceSprite buttonSpriteArrow = new("HideNSeekArrowButton.png");

    public static bool isLightActive(byte playerId)
    {
        return lightActive.Contains(playerId);
    }

    public static void clearAndReload()
    {
        if (localArrows != null)
            foreach (var arrow in localArrows)
                if (arrow?.arrow != null)
                    Object.Destroy(arrow.arrow);
        localArrows.Clear();
        lightActive.Clear();
        arrowActive = false;

        lightCooldown = CustomOptionHolder.hunterLightCooldown.GetFloat();
        lightDuration = CustomOptionHolder.hunterLightDuration.GetFloat();
        lightVision = CustomOptionHolder.hunterLightVision.GetFloat();
        lightPunish = CustomOptionHolder.hunterLightPunish.GetFloat();
        AdminCooldown = CustomOptionHolder.hunterAdminCooldown.GetFloat();
        AdminDuration = CustomOptionHolder.hunterAdminDuration.GetFloat();
        AdminPunish = CustomOptionHolder.hunterAdminPunish.GetFloat();
        ArrowCooldown = CustomOptionHolder.hunterArrowCooldown.GetFloat();
        ArrowDuration = CustomOptionHolder.hunterArrowDuration.GetFloat();
        ArrowPunish = CustomOptionHolder.hunterArrowPunish.GetFloat();
    }
}

public static class Hunted
{
    public static List<byte> timeshieldActive = new();
    public static int shieldCount = 3;

    public static float shieldCooldown = 30f;
    public static float shieldDuration = 5f;
    public static float shieldRewindTime = 3f;
    public static bool taskPunish;

    public static void clearAndReload()
    {
        timeshieldActive.Clear();
        taskPunish = false;

        shieldCount = Mathf.RoundToInt(CustomOptionHolder.huntedShieldNumber.GetFloat());
        shieldCooldown = CustomOptionHolder.huntedShieldCooldown.GetFloat();
        shieldDuration = CustomOptionHolder.huntedShieldDuration.GetFloat();
        shieldRewindTime = CustomOptionHolder.huntedShieldRewindTime.GetFloat();
    }
}

[HarmonyPatch(typeof(PlayerPhysics), nameof(PlayerPhysics.FixedUpdate))]
public static class HideSpeedFix
{
    public static void Postfix(PlayerPhysics __instance)
    {
        if (__instance != null
            && __instance.AmOwner
            && InGame
            && CachedPlayer.LocalPlayer.PlayerPhysics != null
            && HideNSeek.isHideNSeekGM
            && !CachedPlayer.LocalPlayer.Data.IsDead
            && __instance.myPlayer.CanMove)
        {
            var players = CachedPlayer.LocalPlayer.PlayerControl;
            if (players.Data.Role.IsImpostor) __instance.body.velocity *= HideNSeek.hunterSpeed;
            else __instance.body.velocity *= HideNSeek.crewSpeed;
        }
    }
}