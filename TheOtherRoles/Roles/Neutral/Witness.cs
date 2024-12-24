﻿using System;
using System.Linq;
using TheOtherRoles.Utilities;
using UnityEngine;
using Object = UnityEngine.Object;

namespace TheOtherRoles.Roles.Neutral;

public class Witness
{
    public static PlayerControl player;
    public static Color color = new Color32(123, 170, 255, byte.MaxValue);
    public static PlayerControl target;
    public static PlayerControl killerTarget;

    public static int markTimer;
    public static int exileToWin;
    public static bool meetingDie;

    public static Sprite TargetSprite = new ResourceSprite("TargetIcon.png", 150);

    public static int exiledCount;
    public static DateTime startTime;
    public static float timeLeft;
    public static bool endTime;
    public static bool triggerWitnessWin;

    public static void ClearAndReload()
    {
        player = null;
        target = null;
        killerTarget = null;
        endTime = false;
        triggerWitnessWin = false;
        exiledCount = 0;
        markTimer = CustomOptionHolder.witnessMarkTimer.GetInt() + 8;
        exileToWin = CustomOptionHolder.witnessWinCount.GetInt();
        meetingDie = CustomOptionHolder.witnessMeetingDie.GetBool();
    }

    internal static void WitnessReport(byte targetId)
    {
        var target = playerById(targetId);

        if (target == null || !player.IsAlive() || PlayerControl.LocalPlayer != player) return;

        killerTarget = DetermineKillerTarget(target) ?? null;

        static PlayerControl DetermineKillerTarget(PlayerControl target)
        {
            if (target == null)
            {
                return GameHistory.GetLastKiller();
            }

            var deadPlayer = GameHistory.DeadPlayers?
                .Where(dp => dp.Player?.PlayerId == target.PlayerId && dp.KillerIfExisting != null && dp.KillerIfExisting.IsAlive())
                .OrderByDescending(dp => dp.TimeOfDeath)
                .FirstOrDefault();

            return deadPlayer?.KillerIfExisting ?? GameHistory.GetLastKiller();
        }
    }

    [HarmonyPatch]
    public class Witness_Patch
    {
        public static void MeetingOnClick(PlayerVoteArea pva, MeetingHud __instance)
        {
            if (player == null) return;
            var Target = playerById(pva.TargetPlayerId);

            var writer = StartRPC(CachedPlayer.LocalPlayer.PlayerControl, CustomRPC.WitnessSetTarget);
            writer.Write(Target.PlayerId);
            writer.EndRPC();
            target = Target;

            foreach (var playerState in __instance.playerStates)
            {
                var witnessIcon = playerState.transform.FindChild("WitnessIcon");
                if (witnessIcon != null) Object.Destroy(witnessIcon.gameObject);
            }
        }

        [HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.Start))]
        [HarmonyPostfix]
        internal static void MeetingHudStartPostfix(MeetingHud __instance)
        {
            if (player.IsAlive() && killerTarget == null) WitnessReport(byte.MaxValue);

            if (PlayerControl.LocalPlayer == player && PlayerControl.LocalPlayer.IsAlive())
            {
                foreach (var pva in __instance.playerStates)
                {
                    var player = playerById(pva.TargetPlayerId);
                    if (player.IsAlive() && player != Witness.player)
                    {
                        GameObject template = pva.Buttons.transform.Find("CancelButton").gameObject;
                        GameObject targetBox = Object.Instantiate(template, pva.transform);
                        targetBox.name = "WitnessIcon";
                        targetBox.transform.localPosition = new Vector3(1f, 0.03f, -1f);
                        SpriteRenderer renderer = targetBox.GetComponent<SpriteRenderer>();
                        renderer.sprite = TargetSprite;
                        renderer.color = Color.red;
                        PassiveButton button = targetBox.GetComponent<PassiveButton>();
                        button.OnClick.RemoveAllListeners();
                        button.OnClick.AddListener(() => MeetingOnClick(pva, __instance));
                    }
                }
                endTime = false;
                startTime = DateTime.UtcNow;
            }
        }

        [HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.Update))]
        [HarmonyPostfix]
        internal static void TimeUpdatePostfix(MeetingHud __instance)
        {
            if (player.IsDead() || endTime || target != null) return;
            timeLeft = markTimer - (float)(DateTime.UtcNow - startTime).TotalSeconds;
            if (timeLeft <= 0)
            {
                foreach (var playerState in __instance.playerStates)
                {
                    var witnessIcon = playerState.transform.FindChild("WitnessIcon");
                    if (witnessIcon != null)
                    {
                        Object.Destroy(witnessIcon.gameObject);
                    }
                }
                endTime = true;
            }
        }
    }
}
