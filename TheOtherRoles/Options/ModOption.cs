using System.Collections.Generic;
using AmongUs.GameOptions;
using TheOtherRoles.Utilities;
using UnityEngine;

namespace TheOtherRoles.Options;

internal class ModOption
{
    public static float ButtonCooldown => CustomOptionHolder.resteButtonCooldown.getFloat();
    public static bool PreventTaskEnd => CustomOptionHolder.disableTaskGameEnd.getBool();
    public static float KillCooddown => GameOptionsManager.Instance.currentNormalGameOptions.KillCooldown;
    public static int NumImpostors => GameOptionsManager.Instance.currentNormalGameOptions.NumImpostors;
    public static bool DebugMode => CustomOptionHolder.debugMode.getBool();
    public static bool DisableGameEnd => DebugMode && CustomOptionHolder.disableGameEnd.getBool();
    public static NormalGameOptionsV07 NormalOptions => GameOptionsManager.Instance.currentNormalGameOptions;

    // Set values
    public static int maxNumberOfMeetings = 10;
    public static bool blockSkippingInEmergencyMeetings;
    public static bool noVoteIsSelfVote;
    public static bool hidePlayerNames;
    public static bool allowParallelMedBayScans;
    public static bool showLighterDarker = true;
    public static bool showFPS = true;
    public static bool localHats;
    public static bool toggleCursor = true;
    public static bool enableSoundEffects = true;
    public static bool showKeyReminder;
    public static bool shieldFirstKill;
    public static bool hideVentAnim;
    public static bool impostorSeeRoles;
    public static bool transparentTasks;
    public static bool hideOutOfSightNametags;
    public static bool randomLigherPlayer;
    public static bool disableMedscanWalking;
    public static bool isCanceled;

    public static int restrictDevices;

    // public static float restrictAdminTime = 600f;
    //public static float restrictAdminTimeMax = 600f;
    public static float restrictCamerasTime = 600f;
    public static float restrictCamerasTimeMax = 600f;
    public static float restrictVitalsTime = 600f;
    public static float restrictVitalsTimeMax = 600f;
    public static bool disableCamsRoundOne;
    public static bool isRoundOne = true;
    public static bool camoComms;
    public static bool disableSabotage;
    public static bool fungleDisableCamoComms;
    public static bool randomGameStartPosition;
    public static bool allowModGuess;
    public static CustomGamemodes gameMode = CustomGamemodes.Classic;

    // Updating values
    public static int meetingsCount;
    public static List<SurvCamera> camerasToAdd = new();
    public static List<Vent> ventsToSeal = new();
    public static Dictionary<byte, PoolablePlayer> playerIcons = new();
    public static string firstKillName;
    public static PlayerControl firstKillPlayer;

    // public static bool canUseAdmin  { get { return restrictDevices == 0 || restrictAdminTime > 0f || CachedPlayer.LocalPlayer.PlayerControl == Hacker.hacker || CachedPlayer.LocalPlayer.Data.IsDead; }}

    //public static bool couldUseAdmin { get { return restrictDevices == 0 || restrictAdminTimeMax > 0f  || CachedPlayer.LocalPlayer.PlayerControl == Hacker.hacker || CachedPlayer.LocalPlayer.Data.IsDead; }}

    public static bool canUseCameras => restrictDevices == 0 || restrictCamerasTime > 0f ||
                                        CachedPlayer.LocalPlayer.PlayerControl == Hacker.hacker ||
                                        CachedPlayer.LocalPlayer.Data.IsDead;

    public static bool couldUseCameras => restrictDevices == 0 || restrictCamerasTimeMax > 0f ||
                                          CachedPlayer.LocalPlayer.PlayerControl == Hacker.hacker ||
                                          CachedPlayer.LocalPlayer.Data.IsDead;

    public static bool canUseVitals => restrictDevices == 0 || restrictVitalsTime > 0f ||
                                       CachedPlayer.LocalPlayer.PlayerControl == Hacker.hacker ||
                                       CachedPlayer.LocalPlayer.Data.IsDead;

    public static bool couldUseVitals => restrictDevices == 0 || restrictVitalsTimeMax > 0f ||
                                         CachedPlayer.LocalPlayer.PlayerControl == Hacker.hacker ||
                                         CachedPlayer.LocalPlayer.Data.IsDead;

    public static void clearAndReloadMapOptions()
    {
        meetingsCount = 0;
        camerasToAdd = [];
        ventsToSeal = [];
        playerIcons = new Dictionary<byte, PoolablePlayer>();

        maxNumberOfMeetings = Mathf.RoundToInt(CustomOptionHolder.maxNumberOfMeetings.getSelection());
        blockSkippingInEmergencyMeetings = CustomOptionHolder.blockSkippingInEmergencyMeetings.getBool();
        blockSkippingInEmergencyMeetings = CustomOptionHolder.blockSkippingInEmergencyMeetings.getBool();
        noVoteIsSelfVote = CustomOptionHolder.noVoteIsSelfVote.getBool();
        hidePlayerNames = CustomOptionHolder.hidePlayerNames.getBool();
        hideOutOfSightNametags = CustomOptionHolder.hideOutOfSightNametags.getBool();
        hideVentAnim = CustomOptionHolder.hideVentAnimOnShadows.getBool();
        allowParallelMedBayScans = CustomOptionHolder.allowParallelMedBayScans.getBool();
        disableMedscanWalking = CustomOptionHolder.disableMedbayWalk.getBool();
        camoComms = CustomOptionHolder.enableCamoComms.getBool();
        fungleDisableCamoComms = false;
        shieldFirstKill = CustomOptionHolder.shieldFirstKill.getBool();
        impostorSeeRoles = CustomOptionHolder.impostorSeeRoles.getBool();
        transparentTasks = CustomOptionHolder.transparentTasks.getBool();
        restrictDevices = CustomOptionHolder.restrictDevices.getSelection();
        //restrictAdminTime = restrictAdminTimeMax = CustomOptionHolder.restrictAdmin.getFloat();
        restrictCamerasTime = restrictCamerasTimeMax = CustomOptionHolder.restrictCameras.getFloat();
        restrictVitalsTime = restrictVitalsTimeMax = CustomOptionHolder.restrictVents.getFloat();
        disableCamsRoundOne = CustomOptionHolder.disableCamsRound1.getBool();
        randomGameStartPosition = CustomOptionHolder.randomGameStartPosition.getBool();
        randomLigherPlayer = CustomOptionHolder.randomLigherPlayer.getBool();
        allowModGuess = CustomOptionHolder.allowModGuess.getBool();
        disableSabotage = CustomOptionHolder.disableSabotage.getBool();
        firstKillPlayer = null;
        isRoundOne = true;
        isCanceled = false;
    }

    public static void reloadPluginOptions()
    {
        showFPS = Main.ShowFPS.Value;
        toggleCursor = Main.ToggleCursor.Value;
        enableSoundEffects = Main.EnableSoundEffects.Value;
        showKeyReminder = Main.ShowKeyReminder.Value;
        localHats = Main.LocalHats.Value;
    }

    public static void resetDeviceTimes()
    {
        //restrictAdminTime = restrictAdminTimeMax;
        restrictCamerasTime = restrictCamerasTimeMax;
        restrictVitalsTime = restrictVitalsTimeMax;
    }
}