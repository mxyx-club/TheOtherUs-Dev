﻿using System;
using System.Linq;
using TheOtherRoles.Utilities;
using TMPro;
using UnityEngine;
using static MeetingHud;
using Object = UnityEngine.Object;

namespace TheOtherRoles.Roles.Impostor;

public class WolfLord
{
    public static PlayerControl Player;
    public static Color color = Palette.ImpostorRed;

    public static bool Revealed;
    public static bool Killed;

    public static Sprite TargetSprite = new ResourceSprite("TargetIcon.png", 150);

    public static void ClearAndReload()
    {
        Player = null;
        Revealed = false;
        Killed = false;
    }

    public static void WolfLordkilled(byte targetId)
    {
        var target = playerById(targetId);
        Revealed = true;
        if (target == null) return;

        Killed = true;
        target.Exiled();
        GameHistory.OverrideDeathReasonAndKiller(target, CustomDeathReason.Kill, Player);
        if (Constants.ShouldPlaySfx()) SoundManager.Instance.PlaySound(target.KillSfx, false, 0.8f);

        if (CachedPlayer.LocalPlayer.PlayerControl == target)
            FastDestroyableSingleton<HudManager>.Instance.KillOverlay.ShowKillAnimation(Player.Data, target.Data);
    }

    [HarmonyPatch]
    public static class WolfLord_Patch
    {
        private static TextMeshPro meetingExtraButtonLabel;
        public static GameObject MeetingExtraButton;

        [HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.Start))]
        [HarmonyPostfix]
        internal static void MeetingStartPostfix(MeetingHud __instance)
        {
            if (__instance && !Killed && Revealed) { ButtonToggle(__instance); return; }
            if (Player.IsAlive() && PlayerControl.LocalPlayer == Player && !Revealed)
            {
                var meetingUI = Object.FindObjectsOfType<Transform>().FirstOrDefault(x => x.name == "PhoneUI");

                var buttonTemplate = __instance.playerStates[0].transform.FindChild("votePlayerBase");
                var maskTemplate = __instance.playerStates[0].transform.FindChild("MaskArea");
                var textTemplate = __instance.playerStates[0].NameText;
                var meetingExtraButtonParent = new GameObject().transform;
                meetingExtraButtonParent.SetParent(meetingUI);
                var meetingExtraButton = Object.Instantiate(buttonTemplate, meetingExtraButtonParent);
                MeetingExtraButton = meetingExtraButton.gameObject;

                var meetingExtraButtonMask = Object.Instantiate(maskTemplate, meetingExtraButtonParent);
                meetingExtraButtonLabel = Object.Instantiate(textTemplate, meetingExtraButton);
                meetingExtraButton.GetComponent<SpriteRenderer>().sprite = ShipStatus.Instance.CosmeticsCache.GetNameplate("nameplate_NoPlate").Image;

                meetingExtraButtonParent.localPosition = new Vector3(0, -2.225f, -5);
                meetingExtraButtonParent.localScale = new Vector3(0.55f, 0.55f, 1f);
                meetingExtraButtonLabel.alignment = TextAlignmentOptions.Center;
                meetingExtraButtonLabel.transform.localPosition =
                    new Vector3(0, 0, meetingExtraButtonLabel.transform.localPosition.z);

                var localScale = meetingExtraButtonLabel.transform.localScale;
                localScale = new Vector3(
                    localScale.x * 1.5f,
                    localScale.x * 1.7f,
                    localScale.x * 1.7f);
                meetingExtraButtonLabel.transform.localScale = localScale;
                meetingExtraButtonLabel.text = cs(color, "猎杀时刻");

                var passiveButton = meetingExtraButton.GetComponent<PassiveButton>();
                passiveButton.OnClick.RemoveAllListeners();
                if (Player.IsAlive()) passiveButton.OnClick.AddListener((Action)(() => ButtonToggle(__instance)));

                meetingExtraButton.parent.gameObject.SetActive(false);
                __instance.StartCoroutine(Effects.Lerp(7.27f, new Action<float>(p =>
                {
                    if ((int)p == 1) meetingExtraButton.parent.gameObject.SetActive(true);
                })));
            }
        }

        public static void ClearButton()
        {
            if (MeetingExtraButton != null) Object.Destroy(MeetingExtraButton);
        }

        private static void ButtonToggle(MeetingHud __instance)
        {
            __instance.playerStates[0].Cancel(); // This will stop the underlying buttons of the template from showing up
            if (__instance.state == VoteStates.Results || Player.IsDead()) return;

            var writer = StartRPC(CachedPlayer.LocalPlayer.PlayerControl, CustomRPC.WolfLordkilled);
            writer.Write(byte.MaxValue);
            writer.EndRPC();
            WolfLordkilled(byte.MaxValue);

            Object.Destroy(MeetingExtraButton);

            foreach (var playerState in __instance.playerStates)
            {
                var guesser = playerState.transform.FindChild("ShootButton");
                if (guesser != null) Object.Destroy(guesser.gameObject);
            }

            if (Guesser.guesserUI != null && Guesser.guesserUIExitButton != null)
                Guesser.guesserUIExitButton.OnClick.Invoke();

            if (PlayerControl.LocalPlayer == Player && PlayerControl.LocalPlayer.IsAlive())
            {
                foreach (var pva in __instance.playerStates)
                {
                    var player = playerById(pva.TargetPlayerId);
                    if (player.IsAlive() && player != Player && !player.isImpostor())
                    {
                        GameObject template = pva.Buttons.transform.Find("CancelButton").gameObject;
                        GameObject targetBox = Object.Instantiate(template, pva.transform);
                        targetBox.name = "WolfLordIcon";
                        targetBox.transform.localPosition = new Vector3(1f, 0.03f, -1f);
                        SpriteRenderer renderer = targetBox.GetComponent<SpriteRenderer>();
                        renderer.sprite = TargetSprite;
                        renderer.color = Color.red;
                        PassiveButton button = targetBox.GetComponent<PassiveButton>();
                        button.OnClick.RemoveAllListeners();
                        button.OnClick.AddListener(() => WolfLordOnClick(pva, __instance));
                    }
                }
            }
        }

        public static void WolfLordOnClick(PlayerVoteArea pva, MeetingHud __instance)
        {
            var target = playerById(pva.TargetPlayerId);
            if (Player == null || !Revealed || Killed || target == null) return;
            if (__instance.state is not (VoteStates.Voted or VoteStates.NotVoted)) return;
            var writer = StartRPC(CachedPlayer.LocalPlayer.PlayerControl, CustomRPC.WolfLordkilled);
            writer.Write(target.PlayerId);
            writer.EndRPC();
            WolfLordkilled(target.PlayerId);

            foreach (var playerState in __instance.playerStates)
            {
                var icon = playerState.transform.FindChild("WolfLordIcon");
                if (icon != null) Object.Destroy(icon.gameObject);

                var guesser = playerState.transform.FindChild("ShootButton");
                if (guesser != null) Object.Destroy(guesser.gameObject);
            }

            if (Guesser.guesserUI != null && Guesser.guesserUIExitButton != null)
                Guesser.guesserUIExitButton.OnClick.Invoke();
        }
    }
}
