﻿using System;
using System.Collections.Generic;
using System.Linq;
using Hazel;
using Reactor.Utilities;
using TheOtherRoles.CustomGameModes;
using TheOtherRoles.Patches;
using TheOtherRoles.Utilities;
using TMPro;
using UnityEngine;
using static TheOtherRoles.GameHistory;
using Object = UnityEngine.Object;

namespace TheOtherRoles.Roles;

public static class Guesser
{
    public static bool isGuesser(byte playerId)
    {
        if (Assassin.assassin.Any(item => item.PlayerId == playerId && Assassin.assassin != null)) return true;

        return Vigilante.vigilante != null && Vigilante.vigilante.PlayerId == playerId;
    }

    public static void clear(byte playerId)
    {
        if (Vigilante.vigilante != null && Vigilante.vigilante.PlayerId == playerId) Vigilante.vigilante = null;
        Assassin.assassin.RemoveAll(p => p.PlayerId == playerId);
    }

    public static int remainingShots(byte playerId, bool shoot = false)
    {
        if (Vigilante.vigilante != null && Vigilante.vigilante.PlayerId == playerId)
        {
            return shoot ? Mathf.Max(0, --Vigilante.remainingShotsNiceGuesser) : Vigilante.remainingShotsNiceGuesser;
        }

        if (Assassin.assassin != null && Assassin.assassin.Any(x => x.PlayerId == playerId))
        {
            return shoot ? Mathf.Max(0, --Assassin.remainingShotsEvilGuesser) : Assassin.remainingShotsEvilGuesser;
        }

        return 0;
    }


    public const int MaxOneScreenRole = 40;
    private static Dictionary<RoleType, List<Transform>> RoleButtons;
    private static Dictionary<RoleType, SpriteRenderer> RoleSelectButtons;
    public static RoleType currentTeamType;
    private static List<SpriteRenderer> PageButtons;
    public static GameObject guesserUI;
    public static PassiveButton guesserUIExitButton;
    public static int Page;
    public static byte guesserCurrentTarget;

    private static void guesserSelectRole(RoleType Role, bool SetPage = true)
    {
        currentTeamType = Role;
        if (SetPage) Page = 1;
        foreach (var RoleButton in RoleButtons)
        {
            int index = 0;
            RoleButtons.TryGetValue(RoleButton.Key, out var RoleButtonList);
            RoleButtonList ??= new();
            foreach (var RoleBtn in RoleButtonList)
            {
                if (RoleBtn == null) continue;
                index++;
                if (index <= (Page - 1) * MaxOneScreenRole) { RoleBtn.gameObject.SetActive(false); continue; }
                if ((Page * MaxOneScreenRole) < index) { RoleBtn.gameObject.SetActive(false); continue; }
                RoleBtn.gameObject.SetActive(RoleButton.Key == Role);
            }
        }
        foreach (var RoleButton in RoleSelectButtons)
        {
            if (RoleButton.Value == null) continue;
            RoleButton.Value.color = new(0, 0, 0, RoleButton.Key == Role ? 1 : 0.25f);
        }
    }

    public static void guesserOnClick(int buttonTarget, MeetingHud __instance)
    {
        if (guesserUI != null || !(__instance.state is MeetingHud.VoteStates.Voted
            or MeetingHud.VoteStates.NotVoted or MeetingHud.VoteStates.Discussion)) return;

        Page = 1;
        RoleButtons = new();
        RoleSelectButtons = new();
        PageButtons = new();

        __instance.playerStates.ForEach(x => x.gameObject.SetActive(false));

        Transform PhoneUI = Object.FindObjectsOfType<Transform>().FirstOrDefault(x => x.name == "PhoneUI");
        Transform container = Object.Instantiate(PhoneUI, __instance.transform);
        container.transform.localPosition = new Vector3(0, 0, -5f);
        guesserUI = container.gameObject;
        container.transform.localScale *= 0.75f;

        List<int> i = [0, 0, 0, 0];
        var buttonTemplate = __instance.playerStates[0].transform.FindChild("votePlayerBase");
        var maskTemplate = __instance.playerStates[0].transform.FindChild("MaskArea");
        var smallButtonTemplate = __instance.playerStates[0].Buttons.transform.Find("CancelButton");
        var textTemplate = __instance.playerStates[0].NameText;

        guesserCurrentTarget = __instance.playerStates[buttonTarget].TargetPlayerId;

        var exitButtonParent = new GameObject().transform;
        exitButtonParent.SetParent(container);
        var exitButton = Object.Instantiate(buttonTemplate.transform, exitButtonParent);
        var exitButtonMask = Object.Instantiate(maskTemplate, exitButtonParent);
        exitButton.gameObject.GetComponent<SpriteRenderer>().sprite = smallButtonTemplate.GetComponent<SpriteRenderer>().sprite;
        var transform = exitButtonParent.transform;
        transform.localPosition = new Vector3(3f, 2.1f, -5);
        transform.localScale = new Vector3(0.217f, 0.9f, 1);
        guesserUIExitButton = exitButton.GetComponent<PassiveButton>();
        guesserUIExitButton.OnClick.RemoveAllListeners();
        guesserUIExitButton.OnClick.AddListener((Action)(() =>
        {
            __instance.playerStates.ForEach(x =>
            {
                x.gameObject.SetActive(true);
                if (CachedPlayer.LocalPlayer.Data.IsDead && x.transform.FindChild("ShootButton") != null)
                    Object.Destroy(x.transform.FindChild("ShootButton").gameObject);
            });
            Object.Destroy(container.gameObject);
        }));

        var buttons = new List<Transform>();
        Transform selectedButton = null;

        // From SuperNewRoles
        var teamCount = ModOption.allowModGuess ? 4 : 3;
        for (int index = 0; index < teamCount; index++)
        {
            Transform TeambuttonParent = new GameObject().transform;
            TeambuttonParent.SetParent(container);
            Transform Teambutton = Object.Instantiate(buttonTemplate, TeambuttonParent);
            Teambutton.FindChild("ControllerHighlight").gameObject.SetActive(false);
            Transform TeambuttonMask = Object.Instantiate(maskTemplate, TeambuttonParent);
            TextMeshPro Teamlabel = Object.Instantiate(textTemplate, Teambutton);
            //Teambutton.GetComponent<SpriteRenderer>().sprite = ShipStatus.Instance.CosmeticsCache.GetNameplate("nameplate_NoPlate").Image;
            RoleSelectButtons.Add((RoleType)index, Teambutton.GetComponent<SpriteRenderer>());
            TeambuttonParent.localPosition = new(-2.75f + (index * 1.75f), 2.225f, -200);
            TeambuttonParent.localScale = new(0.55f, 0.55f, 1f);
            Teamlabel.color = getTeamColor((RoleType)index);
            //Info($"{Teamlabel.color} {(RoleTeam)index}");
            Teamlabel.text = GetString(((RoleType)index is RoleType.Crewmate ? "Crewmate" : ((RoleType)index).ToString()) + "RolesText");
            Teamlabel.alignment = TextAlignmentOptions.Center;
            Teamlabel.transform.localPosition = new Vector3(0, 0, Teamlabel.transform.localPosition.z);
            Teamlabel.transform.localScale *= 1.6f;
            Teamlabel.autoSizeTextContainer = true;

            static void CreateTeamButton(Transform Teambutton, RoleType type)
            {
                Teambutton.GetComponent<PassiveButton>().OnClick.AddListener((UnityEngine.Events.UnityAction)(() =>
                {
                    guesserSelectRole(type);
                    ReloadPage();
                }));
            }
            if (!PlayerControl.LocalPlayer.Data.IsDead) CreateTeamButton(Teambutton, (RoleType)index);
        }

        static void ReloadPage()
        {
            PageButtons[0].color = new(1, 1, 1, 1f);
            PageButtons[1].color = new(1, 1, 1, 1f);
            if (RoleButtons.Count != 0)
            {
                return;
            }
            else if (((RoleButtons[currentTeamType].Count / MaxOneScreenRole) +
                (RoleButtons[currentTeamType].Count % MaxOneScreenRole != 0 ? 1 : 0)) < Page)
            {
                Page -= 1;
                PageButtons[1].color = new(1, 1, 1, 0.1f);
            }
            else if (((RoleButtons[currentTeamType].Count / MaxOneScreenRole) +
                (RoleButtons[currentTeamType].Count % MaxOneScreenRole != 0 ? 1 : 0)) < Page + 1)
            {
                PageButtons[1].color = new(1, 1, 1, 0.1f);
            }
            if (Page <= 1)
            {
                Page = 1;
                PageButtons[0].color = new(1, 1, 1, 0.1f);
            }
            guesserSelectRole(currentTeamType, false);
            Info("Page:" + Page);
        }

        static void CreatePage(bool IsNext, MeetingHud __instance, Transform container)
        {
            var buttonTemplate = __instance.playerStates[0].transform.FindChild("votePlayerBase");
            var maskTemplate = __instance.playerStates[0].transform.FindChild("MaskArea");
            var smallButtonTemplate = __instance.playerStates[0].Buttons.transform.Find("CancelButton");
            var textTemplate = __instance.playerStates[0].NameText;
            Transform PagebuttonParent = new GameObject().transform;
            PagebuttonParent.SetParent(container);
            Transform Pagebutton = Object.Instantiate(buttonTemplate, PagebuttonParent);
            Pagebutton.FindChild("ControllerHighlight").gameObject.SetActive(false);
            Transform PagebuttonMask = Object.Instantiate(maskTemplate, PagebuttonParent);
            TextMeshPro Pagelabel = Object.Instantiate(textTemplate, Pagebutton);
            //Pagebutton.GetComponent<SpriteRenderer>().sprite = ShipStatus.Instance.CosmeticsCache.GetNameplate("nameplate_NoPlate").Image;
            PagebuttonParent.localPosition = IsNext ? new(3.535f, -2.2f, -200) : new(-3.475f, -2.2f, -200);
            PagebuttonParent.localScale = new(0.55f, 0.55f, 1f);
            Pagelabel.color = Color.white;
            Pagelabel.text = GetString(IsNext ? "下一页" : "上一页");
            Pagelabel.alignment = TextAlignmentOptions.Center;
            Pagelabel.transform.localPosition = new Vector3(0, 0, Pagelabel.transform.localPosition.z);
            Pagelabel.transform.localScale *= 1.6f;
            Pagelabel.autoSizeTextContainer = true;
            if (!IsNext && Page <= 1) Pagebutton.GetComponent<SpriteRenderer>().color = new(1, 1, 1, 0.1f);
            Pagebutton.GetComponent<PassiveButton>().OnClick.AddListener((UnityEngine.Events.UnityAction)(() =>
            {
                Info("翻页");
                if (IsNext) Page += 1;
                else Page -= 1;
                ReloadPage();
            }));
            PageButtons.Add(Pagebutton.GetComponent<SpriteRenderer>());
        }
        if (!PlayerControl.LocalPlayer.Data.IsDead)
        {
            CreatePage(false, __instance, container);
            CreatePage(true, __instance, container);
        }

        int ind = 0;

        #region 职业排除规则
        foreach (RoleInfo roleInfo in RoleInfo.allRoleInfos)
        {
            if (roleInfo == null) continue; // Not guessable roles

            if (RoleIsEnable.TryGetValue(roleInfo.roleId, out int isEnabled) && isEnabled == 0)
            {
                continue;
            }

            var guesserRole = CachedPlayer.LocalId == Vigilante.vigilante?.PlayerId ? RoleId.Vigilante : RoleId.Assassin;

            if (CachedPlayer.LocalId == Doomsayer.doomsayer?.PlayerId)
            {
                if (!Doomsayer.canGuessImpostor && roleInfo.roleType == RoleType.Impostor)
                    continue;
                if (!Doomsayer.canGuessNeutral && roleInfo.roleType == RoleType.Neutral)
                    continue;
            }

            if (roleInfo.roleType == RoleType.Modifier && ModOption.allowModGuess && !roleInfo.isGuessable)
                continue;

            if (roleInfo.roleType is not RoleType.Crewmate and not RoleType.Neutral and not RoleType.Impostor and not RoleType.Modifier)
                continue;

            // remove all roles that cannot spawn due to the settings from the ui.
            switch (roleInfo.roleId)
            {
                case RoleId.Spy when ModOption.NumImpostors <= 1:
                    continue;
                case RoleId.Poucher when Poucher.spawnModifier:
                    continue;
                case RoleId.Crewmate when !Assassin.evilGuesserCanGuessCrewmate && guesserRole == RoleId.Assassin:
                    continue;
                case RoleId.Spy when PlayerControl.LocalPlayer.isImpostor() && !HandleGuesser.evilGuesserCanGuessSpy:
                    continue;
                case RoleId.Mayor when Mayor.Revealed:
                    continue;
                case RoleId.WolfLord when WolfLord.Revealed:
                    continue;
                case RoleId.Vigilante when HandleGuesser.isGuesserGm || CachedPlayer.LocalPlayer.PlayerId == Vigilante.vigilante?.PlayerId:
                    continue;
                case RoleId.Sidekick when !CustomOptionHolder.jackalCanCreateSidekick.GetBool():
                    continue;
                case RoleId.Deputy when CustomOptionHolder.sheriffSpawnRate.GetSelection() == 0:
                    continue;
                case RoleId.Doomsayer when CachedPlayer.LocalPlayer.PlayerId == Doomsayer.doomsayer?.PlayerId:
                    continue;
            }

            if (Snitch.snitch != null && HandleGuesser.guesserCantGuessSnitch)
            {
                var (playerCompleted, playerTotal) = TasksHandler.taskInfo(Snitch.snitch.Data);
                var numberOfLeftTasks = playerTotal - playerCompleted;
                if (numberOfLeftTasks <= Snitch.taskCountForReveal && roleInfo.roleId == RoleId.Snitch) continue;
            }

            CreateRole(roleInfo);
        }
        #endregion

        void CreateRole(RoleInfo roleInfo = null)
        {
            if (roleInfo.roleType is RoleType.Ghost or RoleType.Special) return;
            RoleType team = roleInfo?.roleType ?? RoleType.Crewmate;
            //Color color = roleInfo?.color ?? Color.white;
            //RoleId role = roleInfo?.roleId ?? RoleId.Crewmate;

            var buttonParent = new GameObject().transform;
            buttonParent.SetParent(container);
            var button = Object.Instantiate(buttonTemplate, buttonParent);
            button.FindChild("ControllerHighlight").gameObject.SetActive(false);
            var buttonMask = Object.Instantiate(maskTemplate, buttonParent);
            var label = Object.Instantiate(textTemplate, button);
            button.GetComponent<SpriteRenderer>().sprite = ShipStatus.Instance.CosmeticsCache.GetNameplate("nameplate_NoPlate").Image;
            //button.GetComponent<SpriteRenderer>().sprite = FastDestroyableSingleton<HatManager>.Instance.GetNamePlateById("nameplate_NoPlate")?.viewData?.viewData?.Image;
            if (!RoleButtons.ContainsKey(team))
            {
                RoleButtons.Add(team, new());
            }
            RoleButtons[team].Add(button);
            buttons.Add(button);
            int row = i[(int)team] / 5;
            int col = i[(int)team] % 5;
            buttonParent.localPosition = new Vector3(-3.47f + 1.75f * col, 1.5f - 0.45f * row, -200f);
            buttonParent.localScale = new Vector3(0.55f, 0.55f, 1f);
            label.text = cs(roleInfo.color, roleInfo.Name);
            label.alignment = TextAlignmentOptions.Center;
            label.transform.localPosition = new Vector3(0, 0, label.transform.localPosition.z);
            label.transform.localScale *= 1.6f;
            label.autoSizeTextContainer = true;
            int copiedIndex = i[(int)team];

            button.GetComponent<PassiveButton>().OnClick.RemoveAllListeners();
            if (!PlayerControl.LocalPlayer.Data.IsDead) button.GetComponent<PassiveButton>().OnClick.AddListener((Action)(() =>
            {
                if (selectedButton != button)
                {
                    selectedButton = button;
                    buttons.ForEach(x => x.GetComponent<SpriteRenderer>().color = x == selectedButton ? Color.red : Color.white);
                }
                else
                {
                    var dyingTarget = CachedPlayer.LocalPlayer.PlayerControl;
                    var focusedTarget = playerById(__instance.playerStates[buttonTarget].TargetPlayerId);
                    var mainRoleInfo = RoleInfo.getRoleInfoForPlayer(focusedTarget, true);

                    if (__instance.state is not (MeetingHud.VoteStates.Voted or MeetingHud.VoteStates.NotVoted)
                        || focusedTarget == null
                        || (HandleGuesser.remainingShots(CachedPlayer.LocalPlayer.PlayerId) <= 0
                            && HandleGuesser.isGuesser(CachedPlayer.LocalPlayer.PlayerId))
                        || (CachedPlayer.LocalPlayer.PlayerControl == Doomsayer.doomsayer && !Doomsayer.CanShoot))
                        return;

                    if (!HandleGuesser.killsThroughShield && focusedTarget == Medic.shielded)
                    {
                        // Depending on the options, shooting the shielded player will not allow the guess, notifiy everyone about the kill attempt and close the window
                        __instance.playerStates.ForEach(x => x.gameObject.SetActive(true));
                        Object.Destroy(container.gameObject);

                        var murderAttemptWriter = AmongUsClient.Instance.StartRpcImmediately(CachedPlayer.LocalPlayer.PlayerControl.NetId,
                            (byte)CustomRPC.ShieldedMurderAttempt, SendOption.Reliable);
                        AmongUsClient.Instance.FinishRpcImmediately(murderAttemptWriter);
                        RPCProcedure.shieldedMurderAttempt(0);
                        SoundEffectsManager.play("fail");
                        return;
                    }
                    if (focusedTarget == Indomitable.indomitable)
                    {
                        showFlash(new Color32(255, 197, 97, byte.MinValue));
                        __instance.playerStates.ForEach(x => x.gameObject.SetActive(true));
                        Object.Destroy(container.gameObject);

                        var murderAttemptWriter = AmongUsClient.Instance.StartRpcImmediately(CachedPlayer.LocalPlayer.PlayerControl.NetId,
                            (byte)CustomRPC.ShieldedMurderAttempt, SendOption.Reliable);
                        AmongUsClient.Instance.FinishRpcImmediately(murderAttemptWriter);
                        RPCProcedure.shieldedMurderAttempt(0);
                        SoundEffectsManager.play("fail");
                        seedGuessChat(CachedPlayer.LocalPlayer.PlayerControl, dyingTarget, (byte)roleInfo.roleId);
                        return;
                    }

                    if (mainRoleInfo == null) return;

                    foreach (var role in mainRoleInfo)
                    {
                        if (role.roleId == roleInfo.roleId)
                        {
                            dyingTarget = focusedTarget;
                            continue;
                        }
                    }

                    if (Specoality.specoality != null && CachedPlayer.LocalPlayer.PlayerControl == Specoality.specoality && Specoality.linearfunction > 0)
                    {
                        if (Specoality.specoality.IsAlive() && focusedTarget != dyingTarget)
                        {
                            if (guesserUI != null) guesserUIExitButton.OnClick.Invoke();

                            Coroutines.Start(showFlashCoroutine(Color.red, 1f, 0.3f));
                            Specoality.linearfunction--;
                            SoundEffectsManager.play("fail");
                            //RPCProcedure.seedGuessChat(CachedPlayer.LocalPlayer.PlayerControl, dyingTarget, (byte)roleInfo.roleId);
                            __instance.playerStates.ForEach(x =>
                            {
                                if (x.TargetPlayerId == focusedTarget.PlayerId && x.transform.FindChild("ShootButton") != null)
                                {
                                    Object.Destroy(x.transform.FindChild("ShootButton").gameObject);
                                }
                            });
                            return;
                        }
                    }

                    // Shoot player and send chat info if activated
                    var writer = AmongUsClient.Instance.StartRpcImmediately(CachedPlayer.LocalPlayer.PlayerControl.NetId,
                        (byte)CustomRPC.GuesserShoot, SendOption.Reliable);
                    writer.Write(CachedPlayer.LocalPlayer.PlayerId);
                    writer.Write(dyingTarget.PlayerId);
                    writer.Write(focusedTarget.PlayerId);
                    writer.Write((byte)roleInfo.roleId);
                    AmongUsClient.Instance.FinishRpcImmediately(writer);
                    guesserShoot(CachedPlayer.LocalPlayer.PlayerId, dyingTarget.PlayerId, focusedTarget.PlayerId, (byte)roleInfo.roleId);

                    // Reset the GUI
                    __instance.playerStates.ForEach(x => x.gameObject.SetActive(true));
                    Object.Destroy(container.gameObject);
                    if (CanMultipleShots(dyingTarget))
                    {
                        __instance.playerStates.ForEach(x =>
                        {
                            if (x.TargetPlayerId == dyingTarget.PlayerId && x.transform.FindChild("ShootButton") != null)
                                Object.Destroy(x.transform.FindChild("ShootButton").gameObject);
                        });
                    }
                    else
                    {
                        __instance.playerStates.ForEach(x =>
                        {
                            if (x.transform.FindChild("ShootButton") != null)
                                Object.Destroy(x.transform.FindChild("ShootButton").gameObject);
                        });
                    }
                }
            }));
            i[(int)team]++;
            ind++;
        }
        guesserSelectRole(RoleType.Crewmate);
        ReloadPage();
    }

    public static void guesserShoot(byte killerId, byte dyingTargetId, byte guessedTargetId, byte guessedRoleId)
    {
        var dyingTarget = playerById(dyingTargetId);
        var guessedTarget = playerById(guessedTargetId);
        var guesser = playerById(killerId);
        if (dyingTarget == null) return;

        var dyingPartner = dyingTarget.getPartner();

        // Lawyer shouldn't be exiled with the client for guesses
        if (Lawyer.target != null && (dyingTarget == Lawyer.target || dyingPartner == Lawyer.target))
            Lawyer.targetWasGuessed = true;

        if (Executioner.target != null && (dyingTarget == Executioner.target || dyingPartner == Executioner.target))
            Executioner.targetWasGuessed = true;

        if (Witch.witch != null && (dyingTarget == Witch.witch || dyingPartner == Witch.witch))
            Witch.witchWasGuessed = true;

        if (Thief.thief != null && Thief.thief.PlayerId == killerId && Thief.canStealWithGuess)
        {
            var roleInfo = RoleInfo.allRoleInfos.FirstOrDefault(x => (byte)x.roleId == guessedRoleId);
            if (Thief.thief.IsAlive() && Thief.tiefCanKill(dyingTarget, guesser))
                Thief.StealsRole(dyingTarget.PlayerId);
        }

        if (Doomsayer.doomsayer != null && Doomsayer.doomsayer == guesser && Doomsayer.canGuess)
        {
            var roleInfo = RoleInfo.allRoleInfos.FirstOrDefault(x => (byte)x.roleId == guessedRoleId);
            if (!Doomsayer.doomsayer.Data.IsDead && guessedTargetId == dyingTargetId)
            {
                Doomsayer.killedToWin++;
                if (Doomsayer.killedToWin >= Doomsayer.killToWin) Doomsayer.triggerDoomsayerrWin = true;
                if (guesserUI != null) guesserUIExitButton.OnClick.Invoke();
            }
            else
            {
                seedGuessChat(guesser, guessedTarget, guessedRoleId);
                return;
            }
        }

        bool lawyerDiedAdditionally = false;
        if (Lawyer.lawyer != null && Lawyer.lawyer.PlayerId == killerId && Lawyer.target != null && Lawyer.target.PlayerId == dyingTargetId)
        {
            // Lawyer guessed client.
            if (CachedPlayer.LocalPlayer.PlayerControl == Lawyer.lawyer)
            {
                FastDestroyableSingleton<HudManager>.Instance.KillOverlay.ShowKillAnimation(Lawyer.lawyer.Data, Lawyer.lawyer.Data);
            }

            Lawyer.lawyer.Exiled();
            lawyerDiedAdditionally = true;
            Message("辩护失败", "GuesserShoot");
            OverrideDeathReasonAndKiller(Lawyer.lawyer, CustomDeathReason.LawyerSuicide, guesser);
        }

        byte partnerId = dyingPartner != null ? dyingPartner.PlayerId : dyingTargetId;

        dyingTarget.Exiled();
        OverrideDeathReasonAndKiller(dyingTarget, CustomDeathReason.Guess, guesser);
        if (Constants.ShouldPlaySfx()) SoundManager.Instance.PlaySound(dyingTarget.KillSfx, false, 0.8f);

        if (MeetingHud.Instance)
        {
            MeetingHud.Instance.discussionTimer -= CustomOptionHolder.guessExtendmeetingTime.GetFloat();
            MeetingHudPatch.swapperCheckAndReturnSwap(MeetingHud.Instance, dyingTargetId);

            foreach (var pva in MeetingHud.Instance.playerStates)
            {
                bool shouldClearVote = CustomOptionHolder.guessReVote.GetBool()
                    || pva.VotedFor == dyingTargetId || pva.VotedFor == partnerId
                    || (lawyerDiedAdditionally && Lawyer.lawyer?.PlayerId == pva.TargetPlayerId);

                if (shouldClearVote)
                {
                    pva.UnsetVote();
                    var voteAreaPlayer = playerById(pva.TargetPlayerId);
                    if (voteAreaPlayer?.AmOwner == false) continue;
                    MeetingHud.Instance.ClearVote();
                    MeetingHudPatch.swapperCheckAndReturnSwap(MeetingHud.Instance, partnerId);
                }
            }
            if (AmongUsClient.Instance.AmHost) MeetingHud.Instance.CheckForEndVoting();
        }

        HandleGuesser.remainingShots(killerId, true);

        if (FastDestroyableSingleton<HudManager>.Instance != null && guesser != null)
        {
            if (CachedPlayer.LocalPlayer.PlayerControl == dyingTarget)
            {
                FastDestroyableSingleton<HudManager>.Instance.KillOverlay.ShowKillAnimation(guesser.Data, dyingTarget.Data);
            }
            else if (dyingPartner != null && CachedPlayer.LocalPlayer.PlayerControl == dyingPartner)
            {
                FastDestroyableSingleton<HudManager>.Instance.KillOverlay.ShowKillAnimation(dyingPartner.Data, dyingPartner.Data);
            }
        }

        // remove shoot button from targets for all guessers and close their guesserUI
        if (GuesserGM.isGuesser(PlayerControl.LocalPlayer.PlayerId) && PlayerControl.LocalPlayer != guesser && !PlayerControl.LocalPlayer.Data.IsDead &&
            GuesserGM.remainingShots(PlayerControl.LocalPlayer.PlayerId) > 0 && MeetingHud.Instance)
        {
            MeetingHud.Instance.playerStates.ToList().ForEach(x =>
            {
                if (x.TargetPlayerId == dyingTarget.PlayerId && x.transform.FindChild("ShootButton") != null)
                    Object.Destroy(x.transform.FindChild("ShootButton")?.gameObject);
            });

            if (dyingPartner != null)
            {
                MeetingHud.Instance.playerStates.ToList().ForEach(x =>
                {
                    if (x.TargetPlayerId == dyingPartner.PlayerId && x.transform.FindChild("ShootButton") != null)
                        Object.Destroy(x.transform.FindChild("ShootButton")?.gameObject);
                });
            }

            if (lawyerDiedAdditionally)
            {
                MeetingHud.Instance.playerStates.ToList().ForEach(x =>
                {
                    if (x.TargetPlayerId == Lawyer.lawyer?.PlayerId && x.transform.FindChild("ShootButton") != null)
                        Object.Destroy(x.transform.FindChild("ShootButton")?.gameObject);
                });
            }
        }

        if (guesserUI != null && guesserUIExitButton != null) guesserUIExitButton.OnClick.Invoke();
        if (guesser != null && guessedTarget != null) seedGuessChat(guesser, guessedTarget, guessedRoleId);
        if (WolfLord.Player == guesser && !WolfLord.Revealed && PlayerControl.LocalPlayer == guesser) WolfLord.WolfLord_Patch.ClearButton();
    }

    public static void seedGuessChat(PlayerControl guesser, PlayerControl guessedTarget, byte guessedRoleId)
    {
        if (PlayerControl.LocalPlayer.IsDead() && PlayerControl.LocalPlayer != Specter.Player)
        {
            var roleInfo = RoleInfo.allRoleInfos.FirstOrDefault(x => (byte)x.roleId == guessedRoleId);
            var msg = $"{guesser.Data.PlayerName} 赌怪猜测 {guessedTarget.Data.PlayerName} 是 {roleInfo?.Name ?? ""}!";
            if (AmongUsClient.Instance.AmClient && FastDestroyableSingleton<HudManager>.Instance)
            {
                _ = new LateTask(() => { FastDestroyableSingleton<HudManager>.Instance!.Chat.AddChat(guesser, msg); }, 0.1f, "Guess Chat");
            }
        }
    }
}