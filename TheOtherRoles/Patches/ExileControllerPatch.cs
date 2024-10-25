using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AmongUs.GameOptions;
using Hazel;
using PowerTools;
using TheOtherRoles.Buttons;
using TheOtherRoles.Objects;
using TheOtherRoles.Utilities;
using TMPro;
using UnityEngine;
using Object = UnityEngine.Object;

namespace TheOtherRoles.Patches;

[HarmonyPatch(typeof(ExileController), nameof(ExileController.Begin))]
[HarmonyPriority(Priority.First)]
internal class ExileControllerBeginPatch
{
    public static GameData.PlayerInfo lastExiled;
    public static TextMeshPro confirmImpostorSecondText;
    private static bool IsSec;
    public static bool Prefix(ExileController __instance, [HarmonyArgument(0)] ref GameData.PlayerInfo exiled, [HarmonyArgument(1)] bool tie)
    {
        lastExiled = exiled;

        if (Balancer.currentAbilityUser != null && Balancer.IsDoubleExile && !IsSec)
        {
            IsSec = true;
            __instance.exiled = null;
            ExileController controller = Object.Instantiate(__instance, __instance.transform.parent);
            controller.exiled = Balancer.targetplayerright.Data;
            controller.Begin(controller.exiled, false);
            IsSec = false;
            controller.completeString = string.Empty;

            controller.Text.gameObject.SetActive(false);
            controller.Player.UpdateFromEitherPlayerDataOrCache(controller.exiled, PlayerOutfitType.Default, PlayerMaterial.MaskType.Exile, includePet: false);
            controller.Player.ToggleName(active: false);
            SkinViewData skin = ShipStatus.Instance.CosmeticsCache.GetSkin(controller.exiled.Outfits[PlayerOutfitType.Default].SkinId);
            controller.Player.FixSkinSprite(skin.EjectFrame);
            AudioClip sound = null;
            if (controller.EjectSound != null)
            {
                sound = new(controller.EjectSound.Pointer);
            }
            controller.EjectSound = null;
            void createlate(int index)
            {
                new LateTask(() => { controller.StopAllCoroutines(); controller.StartCoroutine(controller.Animate()); }, 0.025f + index * 0.025f);
            }
            new LateTask(() => controller.StartCoroutine(controller.Animate()), 0f);
            for (int i = 0; i < 23; i++)
            {
                createlate(i);
            }
            new LateTask(() => { controller.StopAllCoroutines(); controller.EjectSound = sound; controller.StartCoroutine(controller.Animate()); }, 0.6f);
            ExileController.Instance = __instance;
            __instance.exiled = Balancer.targetplayerleft.Data;
            exiled = __instance.exiled;
            if (isFungle)
            {
                Helpers.SetActiveAllObject(controller.gameObject.GetChildren(), "RaftAnimation", false);
                controller.transform.localPosition = new(-3.75f, -0.2f, -60f);
            }
            if (!IsSec) return true;
        }

        // Medic shield
        if (Medic.medic != null && AmongUsClient.Instance.AmHost && Medic.futureShielded != null && !Medic.medic.Data.IsDead)
        {
            // We need to send the RPC from the host here, to make sure that the order of shifting and setting the shield is correct(for that reason the futureShifted and futureShielded are being synced)
            var writer = AmongUsClient.Instance.StartRpcImmediately(CachedPlayer.LocalPlayer.PlayerControl.NetId,
                (byte)CustomRPC.MedicSetShielded, SendOption.Reliable);
            writer.Write(Medic.futureShielded.PlayerId);
            AmongUsClient.Instance.FinishRpcImmediately(writer);
            RPCProcedure.medicSetShielded(Medic.futureShielded.PlayerId);
        }

        if (Medic.usedShield) Medic.meetingAfterShielding = true; // Has to be after the setting of the shield

        // Shifter shift
        if (Shifter.shifter != null && AmongUsClient.Instance.AmHost && Shifter.futureShift != null)
        {
            // We need to send the RPC from the host here, to make sure that the order of shifting and erasing is correct (for that reason the futureShifted and futureErased are being synced)
            var writer = AmongUsClient.Instance.StartRpcImmediately(CachedPlayer.LocalPlayer.PlayerControl.NetId,
                (byte)CustomRPC.ShifterShift, SendOption.Reliable);
            writer.Write(Shifter.futureShift.PlayerId);
            AmongUsClient.Instance.FinishRpcImmediately(writer);
            RPCProcedure.shifterShift(Shifter.futureShift.PlayerId);
        }

        Shifter.futureShift = null;

        if (PartTimer.partTimer != null && PartTimer.partTimer.IsAlive())
        {
            if (PartTimer.deathTurn <= 0 && PartTimer.target == null) PartTimer.partTimer.Exiled();
        }

        if (Doomsayer.doomsayer != null && AmongUsClient.Instance.AmHost && !Doomsayer.canGuess) Doomsayer.canGuess = true;

        if (Specoality.specoality != null && Specoality.canNoGuess != null) Specoality.canNoGuess = null;

        if (Butcher.butcher != null)
        {
            Butcher.dissected = null;
            Butcher.canDissection = true;
        }

        // Eraser erase
        if (Eraser.eraser != null && AmongUsClient.Instance.AmHost && Eraser.futureErased != null)
        {
            // We need to send the RPC from the host here, to make sure that the order of shifting and erasing is correct (for that reason the futureShifted and futureErased are being synced)
            foreach (var target in Eraser.futureErased)
            {
                var writer = AmongUsClient.Instance.StartRpcImmediately(CachedPlayer.LocalPlayer.PlayerControl.NetId,
                    (byte)CustomRPC.ErasePlayerRoles, SendOption.Reliable);
                writer.Write(target.PlayerId);
                AmongUsClient.Instance.FinishRpcImmediately(writer);
                RPCProcedure.erasePlayerRoles(target.PlayerId);
                Eraser.alreadyErased.Add(target.PlayerId);
            }
        }
        Eraser.futureErased = new List<PlayerControl>();

        // Trickster boxes
        if (Trickster.trickster != null && JackInTheBox.hasJackInTheBoxLimitReached()) JackInTheBox.convertToVents();

        // Activate portals.
        Portal.meetingEndsUpdate();

        // Witch execute casted spells
        if (Witch.witch != null && Witch.futureSpelled != null && AmongUsClient.Instance.AmHost)
        {
            var exiledIsWitch = exiled != null && exiled.PlayerId == Witch.witch.PlayerId;
            var witchDiesWithExiledLover = exiled != null && Lovers.existing() && Lovers.bothDie &&
                                           (Lovers.lover1.PlayerId == Witch.witch.PlayerId ||
                                            Lovers.lover2.PlayerId == Witch.witch.PlayerId) &&
                                           (exiled.PlayerId == Lovers.lover1.PlayerId ||
                                            exiled.PlayerId == Lovers.lover2.PlayerId);

            if ((witchDiesWithExiledLover || exiledIsWitch) && Witch.witchVoteSavesTargets)
                Witch.futureSpelled = new List<PlayerControl>();
            foreach (var target in Witch.futureSpelled)
            {
                if (target != null && !target.Data.IsDead && checkMuderAttempt(Witch.witch, target, true) ==
                    MurderAttemptResult.PerformKill)
                {
                    if (target == Lawyer.target && Lawyer.lawyer != null)
                    {
                        var writer2 = AmongUsClient.Instance.StartRpcImmediately(CachedPlayer.LocalPlayer.PlayerControl.NetId,
                            (byte)CustomRPC.LawyerPromotesToPursuer, SendOption.Reliable);
                        AmongUsClient.Instance.FinishRpcImmediately(writer2);
                        RPCProcedure.lawyerPromotesToPursuer();
                    }

                    if (target == Executioner.target && Executioner.executioner != null)
                    {
                        var writer2 = AmongUsClient.Instance.StartRpcImmediately(CachedPlayer.LocalPlayer.PlayerControl.NetId,
                            (byte)CustomRPC.ExecutionerPromotesRole, SendOption.Reliable);
                        AmongUsClient.Instance.FinishRpcImmediately(writer2);
                        RPCProcedure.executionerPromotesRole();
                    }

                    var writer = AmongUsClient.Instance.StartRpcImmediately(CachedPlayer.LocalPlayer.PlayerControl.NetId,
                        (byte)CustomRPC.UncheckedExilePlayer, SendOption.Reliable);
                    writer.Write(target.PlayerId);
                    AmongUsClient.Instance.FinishRpcImmediately(writer);
                    RPCProcedure.uncheckedExilePlayer(target.PlayerId);

                    var writer3 = AmongUsClient.Instance.StartRpcImmediately(CachedPlayer.LocalPlayer.PlayerControl.NetId,
                        (byte)CustomRPC.ShareGhostInfo, SendOption.Reliable);
                    writer3.Write(CachedPlayer.LocalPlayer.PlayerId);
                    writer3.Write((byte)RPCProcedure.GhostInfoTypes.DeathReasonAndKiller);
                    writer3.Write(target.PlayerId);
                    writer3.Write((byte)DeadPlayer.CustomDeathReason.WitchExile);
                    writer3.Write(Witch.witch.PlayerId);
                    AmongUsClient.Instance.FinishRpcImmediately(writer3);

                    GameHistory.overrideDeathReasonAndKiller(target, DeadPlayer.CustomDeathReason.WitchExile, Witch.witch);
                }
            }
        }

        Witch.futureSpelled = new List<PlayerControl>();

        // SecurityGuard vents and cameras
        var allCameras = MapUtilities.CachedShipStatus.AllCameras.ToList();
        ModOption.camerasToAdd.ForEach(camera =>
        {
            camera.gameObject.SetActive(true);
            camera.gameObject.GetComponent<SpriteRenderer>().color = Color.white;
            allCameras.Add(camera);
        });
        MapUtilities.CachedShipStatus.AllCameras = allCameras.ToArray();
        ModOption.camerasToAdd = new List<SurvCamera>();

        foreach (var vent in ModOption.ventsToSeal)
        {
            var animator = vent.GetComponent<SpriteAnim>();
            vent.EnterVentAnim = vent.ExitVentAnim = null;
            var newSprite = animator == null
                ? SecurityGuard.staticVentSealedSprite
                : SecurityGuard.getAnimatedVentSealedSprite();
            var rend = vent.myRend;
            if (isFungle)
            {
                newSprite = SecurityGuard.fungleVentSealedSprite;
                rend = vent.transform.GetChild(3).GetComponent<SpriteRenderer>();
                animator = vent.transform.GetChild(3).GetComponent<SpriteAnim>();
            }

            animator?.Stop();
            rend.sprite = newSprite;
            if (SubmergedCompatibility.IsSubmerged && vent.Id == 0) vent.myRend.sprite = SecurityGuard.submergedCentralUpperVentSealedSprite;
            if (SubmergedCompatibility.IsSubmerged && vent.Id == 14) vent.myRend.sprite = SecurityGuard.submergedCentralLowerVentSealedSprite;
            rend.color = Color.white;
            vent.name = "SealedVent_" + vent.name;
        }
        ModOption.ventsToSeal = new List<Vent>();
        // 1 = reset per turn
        if (ModOption.restrictDevices == 1) ModOption.resetDeviceTimes();

        return true;
    }

    public static void Postfix(ExileController __instance)
    {
        confirmImpostorSecondText = Object.Instantiate(__instance.ImpostorText, __instance.Text.transform);
        StringBuilder changeStringBuilder = new();

        if (GameManager.Instance.LogicOptions.currentGameOptions.GetBool(BoolOptionNames.ConfirmImpostor))
            confirmImpostorSecondText.transform.localPosition += new Vector3(0f, -0.4f, 0f);
        else confirmImpostorSecondText.transform.localPosition += new Vector3(0f, -0.2f, 0f);

        confirmImpostorSecondText.text = changeStringBuilder.ToString();
        confirmImpostorSecondText.gameObject.SetActive(true);

        if (Balancer.currentAbilityUser != null && Balancer.IsDoubleExile && __instance.exiled?.PlayerId == Balancer.targetplayerleft.PlayerId)
        {
            __instance.completeString = getString("二者一同放逐");
        }
    }

    [HarmonyPatch(typeof(ExileController), nameof(ExileController.ReEnableGameplay))]
    public class BalancerChatDisable
    {
        private static void Postfix()
        {
            if (confirmImpostorSecondText != null) confirmImpostorSecondText.gameObject?.SetActive(false);
        }
    }
}

[HarmonyPatch]
internal class ExileControllerWrapUpPatch
{
    // Workaround to add a "postfix" to the destroying of the exile controller (i.e. cutscene) and SpwanInMinigame of submerged
    [HarmonyPatch(typeof(Object), nameof(Object.Destroy), typeof(GameObject))]

    public static void Prefix(GameObject obj)
    {
        // Nightvision:
        if (obj != null && obj.name != null && obj.name.Contains("FungleSecurity"))
        {
            SurveillanceMinigamePatch.resetNightVision();
            return;
        }

        // submerged
        if (!SubmergedCompatibility.IsSubmerged) return;
        if (obj.name.Contains("ExileCutscene"))
        {
            WrapUpPostfix(ExileControllerBeginPatch.lastExiled);
        }
        else if (obj.name.Contains("SpawnInMinigame"))
        {
            AntiTeleport.setPosition();
            Chameleon.lastMoved.Clear();
        }
    }

    private static void WrapUpPostfix(GameData.PlayerInfo exiled)
    {
        // Prosecutor win condition
        if (exiled != null && Executioner.executioner != null && Executioner.target != null &&
            Executioner.target.PlayerId == exiled.PlayerId && !Executioner.executioner.Data.IsDead)
        {
            Executioner.triggerExecutionerWin = true;
        }
        // Mini exile lose condition
        else if (exiled != null && Mini.mini != null && Mini.mini.PlayerId == exiled.PlayerId && !Mini.isGrownUp() &&
                 !Mini.mini.Data.Role.IsImpostor && !isNeutral(Mini.mini))
            Mini.triggerMiniLose = true;
        // Jester win condition
        else if (exiled != null && Jester.jester != null && Jester.jester.PlayerId == exiled.PlayerId)
            Jester.triggerJesterWin = true;


        // Reset custom button timers where necessary
        CustomButton.MeetingEndedUpdate();

        // Clear all traps
        KillTrap.clearAllTraps();
        EvilTrapper.meetingFlag = false;
        Balancer.WrapUp(exiled == null ? null : exiled.Object);
        // Mini set adapted cooldown
        if (Mini.mini != null && CachedPlayer.LocalPlayer.PlayerControl == Mini.mini && Mini.mini.Data.Role.IsImpostor)
        {
            var multiplier = Mini.isGrownUp() ? 0.66f : 2f;
            Mini.mini.SetKillTimer(GameOptionsManager.Instance.currentNormalGameOptions.KillCooldown * multiplier);
        }

        // Seer spawn souls
        if (Seer.deadBodyPositions != null && Seer.seer != null &&
            CachedPlayer.LocalPlayer.PlayerControl == Seer.seer && (Seer.mode == 0 || Seer.mode == 2))
        {
            foreach (var pos in Seer.deadBodyPositions)
            {
                var soul = new GameObject();
                //soul.transform.position = pos;
                soul.transform.position = new Vector3(pos.x, pos.y, (pos.y / 1000) - 1f);
                soul.layer = 5;
                var rend = soul.AddComponent<SpriteRenderer>();
                soul.AddSubmergedComponent(SubmergedCompatibility.Classes.ElevatorMover);
                rend.sprite = Seer.soulSprite;

                if (Seer.limitSoulDuration)
                {
                    FastDestroyableSingleton<HudManager>.Instance.StartCoroutine(Effects.Lerp(Seer.soulDuration,
                        new Action<float>(p =>
                        {
                            if (rend != null)
                            {
                                var tmp = rend.color;
                                tmp.a = Mathf.Clamp01(1 - p);
                                rend.color = tmp;
                            }

                            if (p == 1f && rend != null && rend.gameObject != null) Object.Destroy(rend.gameObject);
                        })));
                }
            }

            Seer.deadBodyPositions = new List<Vector3>();
        }

        // Tracker reset deadBodyPositions
        Tracker.deadBodyPositions = new List<Vector3>();

        if (Blackmailer.blackmailer != null && Blackmailer.blackmailed != null)
        {
            // Blackmailer reset blackmailed
            var writer = AmongUsClient.Instance.StartRpcImmediately(CachedPlayer.LocalPlayer.PlayerControl.NetId,
                (byte)CustomRPC.UnblackmailPlayer, SendOption.Reliable);
            AmongUsClient.Instance.FinishRpcImmediately(writer);
            RPCProcedure.unblackmailPlayer();
        }

        // Arsonist deactivate dead poolable players
        if (Arsonist.arsonist != null && Arsonist.arsonist == CachedPlayer.LocalPlayer.PlayerControl)
        {
            var visibleCounter = 0;
            var newBottomLeft = IntroCutsceneOnDestroyPatch.bottomLeft;
            var BottomLeft = newBottomLeft + new Vector3(-0.25f, -0.25f, 0);
            foreach (PlayerControl p in CachedPlayer.AllPlayers)
            {
                if (!ModOption.playerIcons.ContainsKey(p.PlayerId)) continue;
                if (p.Data.IsDead || p.Data.Disconnected)
                {
                    ModOption.playerIcons[p.PlayerId].gameObject.SetActive(false);
                }
                else
                {
                    ModOption.playerIcons[p.PlayerId].transform.localPosition =
                        newBottomLeft + (Vector3.right * visibleCounter * 0.35f);
                    visibleCounter++;
                }
            }
        }

        // Deputy check Promotion, see if the sheriff still exists. The promotion will be after the meeting.
        if (Deputy.deputy != null) PlayerControlFixedUpdatePatch.deputyCheckPromotion(true);

        // Force Bounty Hunter Bounty Update
        if (BountyHunter.bountyHunter != null && BountyHunter.bountyHunter == CachedPlayer.LocalPlayer.PlayerControl)
            BountyHunter.bountyUpdateTimer = 0f;

        // Medium spawn souls
        if (Medium.medium != null && CachedPlayer.LocalPlayer.PlayerControl == Medium.medium)
        {
            if (Medium.souls != null)
            {
                foreach (var sr in Medium.souls) Object.Destroy(sr.gameObject);
                Medium.souls = new List<SpriteRenderer>();
            }

            if (Medium.futureDeadBodies != null)
            {
                foreach (var (db, ps) in Medium.futureDeadBodies)
                {
                    var s = new GameObject();
                    //s.transform.position = ps;
                    s.transform.position = new Vector3(ps.x, ps.y, (ps.y / 1000) - 1f);
                    s.layer = 5;
                    var rend = s.AddComponent<SpriteRenderer>();
                    s.AddSubmergedComponent(SubmergedCompatibility.Classes.ElevatorMover);
                    rend.sprite = Medium.soulSprite;
                    Medium.souls.Add(rend);
                }

                Medium.deadBodies = Medium.futureDeadBodies;
                Medium.futureDeadBodies = new List<Tuple<DeadPlayer, Vector3>>();
            }
        }

        // AntiTeleport set position
        AntiTeleport.setPosition();

        if (CustomOptionHolder.randomGameStartPosition.getBool()) MapData.RandomSpawnPlayers();

        // Invert add meeting
        if (Invert.meetings > 0) Invert.meetings--;

        Chameleon.lastMoved.Clear();

        foreach (var trap in Trap.traps) trap.triggerable = false;
        FastDestroyableSingleton<HudManager>.Instance.StartCoroutine(Effects.Lerp(
            (GameOptionsManager.Instance.currentNormalGameOptions.KillCooldown / 2) + 2, new Action<float>(p =>
            { if (p == 1f) foreach (var trap in Trap.traps) trap.triggerable = true; })));

        if (!Yoyo.markStaysOverMeeting)
            Silhouette.clearSilhouettes();
    }

    [HarmonyPatch(typeof(ExileController), nameof(ExileController.WrapUp))]
    private class BaseExileControllerPatch
    {
        public static void Postfix(ExileController __instance)
        {
            WrapUpPostfix(__instance.exiled);
        }
    }

    [HarmonyPatch(typeof(AirshipExileController), nameof(AirshipExileController.WrapUpAndSpawn))]
    private class AirshipExileControllerPatch
    {
        public static void Postfix(AirshipExileController __instance)
        {
            WrapUpPostfix(__instance.exiled);
        }

        public static bool Prefix(AirshipExileController __instance)
        {

            if (Balancer.currentAbilityUser != null && Balancer.IsDoubleExile && __instance != ExileController.Instance)
            {
                if (__instance.exiled != null)
                {
                    PlayerControl @object = __instance.exiled.Object;
                    if (@object)
                    {
                        @object.Exiled();
                    }
                    __instance.exiled.IsDead = true;
                }
                Object.Destroy(__instance.gameObject);
            }
            return true;
        }
    }
}

[HarmonyPatch(typeof(SpawnInMinigame),
    nameof(SpawnInMinigame.Close))] // Set position of AntiTp players AFTER they have selected a spawn.
internal class AirshipSpawnInPatch
{
    private static void Postfix()
    {
        AntiTeleport.setPosition();
        Chameleon.lastMoved.Clear();
    }
}

[HarmonyPatch(typeof(TranslationController), nameof(TranslationController.GetString), typeof(StringNames),
    typeof(Il2CppReferenceArray<Il2CppSystem.Object>))]
internal class ExileControllerMessagePatch
{
    private static void Postfix(ref string __result, [HarmonyArgument(0)] StringNames id)
    {
        try
        {
            if (ExileController.Instance != null && ExileController.Instance.exiled != null)
            {
                var player = playerById(ExileController.Instance.exiled.Object.PlayerId);
                if (player == null) return;
                // Exile role text
                if (id is StringNames.ExileTextPN or StringNames.ExileTextSN or StringNames.ExileTextPP or StringNames.ExileTextSP)
                    __result = $"{player.Data.PlayerName} 的职业是 {string.Join(" ", RoleInfo.getRoleInfoForPlayer(player, false).Select(x => x.Name).ToArray())}";
                // Hide number of remaining impostors on Jester win
                if (id is StringNames.ImpostorsRemainP or StringNames.ImpostorsRemainS)
                    if (Jester.jester != null && player.PlayerId == Jester.jester.PlayerId)
                        __result = "";
                if (Prosecutor.ProsecuteThisMeeting) __result += " (被起诉)";
                else if (Tiebreaker.isTiebreak)
                {
                    __result += " (破平)";
                    Tiebreaker.isTiebreak = false;
                }
            }
        }
        catch
        {
            // pass - Hopefully prevent leaving while exiling to softlock game
        }
    }
}