using System.Collections.Generic;
using TheOtherRoles.Objects;
using TheOtherRoles.Utilities;
using TMPro;
using UnityEngine;
using Object = UnityEngine.Object;
using static TheOtherRoles.Options.ModOption;

namespace TheOtherRoles.Roles.Neutral;

public class Amnisiac
{
    public static List<PlayerControl> player = new();
    public static List<Arrow> localArrows = new();
    public static Color color = new(0.5f, 0.7f, 1f, 1f);
    public static List<PoolablePlayer> poolIcons = new();

    public static bool showArrows = true;
    public static bool resetRole;

    public static ResourceSprite buttonSprite = new("Remember.png");

    public static void clearAndReload()
    {
        player.Clear();
        showArrows = CustomOptionHolder.amnisiacShowArrows.GetBool();
        resetRole = CustomOptionHolder.amnisiacResetRole.GetBool();
        if (localArrows != null)
            foreach (var arrow in localArrows)
                if (arrow?.arrow != null)
                    Object.Destroy(arrow.arrow);
        localArrows.Clear();
    }

    public static void TakeRole(byte targetId, byte playerId)
    {
        var target = playerById(targetId);
        var local = playerById(playerId);
        if (target == null || local == null) return;
        var targetInfo = RoleInfo.getRoleInfoForPlayer(target, false, false);
        var roleInfo = targetInfo.FirstOrDefault();
        if (target.isImpostor()) turnToImpostor(local);
        switch (roleInfo!.roleId)
        {
            case RoleId.Crewmate:
                player.RemoveAll(x => x.PlayerId == local.PlayerId);
                break;
            case RoleId.Impostor:
                player.RemoveAll(x => x.PlayerId == local.PlayerId);
                break;
            case RoleId.Jester:
                if (resetRole) Jester.clearAndReload();
                player.RemoveAll(x => x.PlayerId == local.PlayerId);
                Jester.jester = local;
                player.Add(target);
                break;
            case RoleId.Juggernaut:
                if (resetRole) Juggernaut.clearAndReload();
                player.RemoveAll(x => x.PlayerId == local.PlayerId);
                Juggernaut.juggernaut = local;
                player.Add(target);
                break;
            case RoleId.Doomsayer:
                if (resetRole) Doomsayer.clearAndReload();
                player.RemoveAll(x => x.PlayerId == local.PlayerId);
                Doomsayer.doomsayer = local;
                player.Add(target);
                break;
            case RoleId.Swooper:
                if (resetRole) Swooper.clearAndReload();
                player.RemoveAll(x => x.PlayerId == local.PlayerId);
                Swooper.swooper = local;
                player.Add(target);
                break;
            case RoleId.Vulture:
                player.RemoveAll(x => x.PlayerId == local.PlayerId);
                if (resetRole) Vulture.clearAndReload();
                Vulture.vulture = local;
                player.Add(target);
                break;

            case RoleId.Executioner:
                player.RemoveAll(x => x.PlayerId == local.PlayerId);
                Executioner.executioner = local;
                player.Add(target);
                break;

            case RoleId.Lawyer:
                player.RemoveAll(x => x.PlayerId == local.PlayerId);
                Lawyer.lawyer = local;
                player.Add(target);
                break;

            case RoleId.Akujo:
                player.RemoveAll(x => x.PlayerId == local.PlayerId);
                Akujo.akujo = local;
                player.Add(target);
                break;

            case RoleId.Pursuer:
                player.RemoveAll(x => x.PlayerId == local.PlayerId);
                if (resetRole) Pursuer.clearAndReload();
                Pursuer.pursuer.Add(local);
                player.Add(target);
                break;

            case RoleId.Jackal:
                player.RemoveAll(x => x.PlayerId == local.PlayerId);
                Jackal.jackal.Add(local);
                break;

            case RoleId.Sidekick:
                player.RemoveAll(x => x.PlayerId == local.PlayerId);
                Jackal.jackal.Add(target);
                Jackal.sidekick = local;
                break;

            case RoleId.Survivor:
                player.RemoveAll(x => x.PlayerId == local.PlayerId);
                Survivor.survivor.Add(local);
                break;

            case RoleId.Thief:
                player.RemoveAll(x => x.PlayerId == local.PlayerId);
                if (resetRole) Thief.clearAndReload();
                Thief.thief = local;
                player.Add(target);
                break;

            case RoleId.Pavlovsowner:
                player.RemoveAll(x => x.PlayerId == local.PlayerId);
                Pavlovsdogs.pavlovsdogs.Add(Pavlovsdogs.pavlovsowner);
                Pavlovsdogs.pavlovsowner = local;
                break;

            case RoleId.Pavlovsdogs:
                player.RemoveAll(x => x.PlayerId == local.PlayerId);
                Pavlovsdogs.pavlovsdogs.Add(local);
                break;

            case RoleId.Werewolf:
                if (resetRole) Werewolf.clearAndReload();
                Werewolf.werewolf = local;
                player.RemoveAll(x => x.PlayerId == local.PlayerId);
                player.Add(target);
                break;


            case RoleId.Witness:
                player.RemoveAll(x => x.PlayerId == local.PlayerId);
                if (resetRole) Witness.ClearAndReload();
                Witness.player = local;
                player.Add(target);
                break;

            case RoleId.Arsonist:
                player.RemoveAll(x => x.PlayerId == local.PlayerId);
                if (resetRole) Arsonist.clearAndReload();
                Arsonist.arsonist = local;
                player.Add(target);

                if (CachedPlayer.LocalPlayer.PlayerControl == Arsonist.arsonist)
                {
                    var playerCounter = 0;
                    var bottomLeft = new Vector3(
                        -FastDestroyableSingleton<HudManager>.Instance.UseButton.transform.localPosition.x,
                        FastDestroyableSingleton<HudManager>.Instance.UseButton.transform.localPosition.y,
                        FastDestroyableSingleton<HudManager>.Instance.UseButton.transform.localPosition.z);
                    foreach (PlayerControl p in CachedPlayer.AllPlayers)
                        if (playerIcons.ContainsKey(p.PlayerId) && p != Arsonist.arsonist)
                        {
                            //Arsonist.poolIcons.Add(p);
                            if (Arsonist.dousedPlayers.Contains(p))
                                playerIcons[p.PlayerId].setSemiTransparent(false);
                            else
                                playerIcons[p.PlayerId].setSemiTransparent(true);

                            playerIcons[p.PlayerId].transform.localPosition = bottomLeft +
                                                                              new Vector3(-0.25f, -0.25f, 0) +
                                                                              (Vector3.right * playerCounter++ * 0.35f);
                            playerIcons[p.PlayerId].transform.localScale = Vector3.one * 0.2f;
                            playerIcons[p.PlayerId].gameObject.SetActive(true);
                        }
                }
                break;

            case RoleId.Mimic:
                player.RemoveAll(x => x.PlayerId == local.PlayerId);
                if (resetRole) Mimic.clearAndReload(false);
                Mimic.mimic = local;
                break;

            case RoleId.BodyGuard:
                player.RemoveAll(x => x.PlayerId == local.PlayerId);
                if (resetRole) BodyGuard.clearAndReload();
                BodyGuard.bodyguard = local;
                break;

            case RoleId.Prosecutor:
                player.RemoveAll(x => x.PlayerId == local.PlayerId);
                if (resetRole) Prosecutor.clearAndReload();
                Prosecutor.prosecutor = target;
                break;

            case RoleId.Mayor:
                player.RemoveAll(x => x.PlayerId == local.PlayerId);
                if (resetRole) Mayor.clearAndReload();
                Mayor.mayor = local;
                break;

            case RoleId.Portalmaker:
                player.RemoveAll(x => x.PlayerId == local.PlayerId);
                if (resetRole) Portalmaker.clearAndReload();
                Portalmaker.portalmaker = local;
                break;

            case RoleId.Engineer:
                player.RemoveAll(x => x.PlayerId == local.PlayerId);
                if (resetRole) Engineer.clearAndReload();
                Engineer.engineer = local;
                break;

            case RoleId.Sheriff:
                player.RemoveAll(x => x.PlayerId == local.PlayerId);
                if (Sheriff.formerDeputy != null && Sheriff.formerDeputy == Sheriff.sheriff)
                {
                    Sheriff.formerDeputy = null;
                    Deputy.deputy = local;
                }
                else Sheriff.sheriff = local;
                break;

            case RoleId.Deputy:
                player.RemoveAll(x => x.PlayerId == local.PlayerId);
                if (resetRole) Deputy.clearAndReload(false);
                Deputy.deputy = local;
                break;

            case RoleId.Butcher:
                player.RemoveAll(x => x.PlayerId == local.PlayerId);
                Butcher.butcher = local;
                break;

            case RoleId.Detective:
                player.RemoveAll(x => x.PlayerId == local.PlayerId);
                if (resetRole) Detective.clearAndReload();
                Detective.detective = local;
                break;

            case RoleId.Balancer:
                player.RemoveAll(x => x.PlayerId == local.PlayerId);
                if (resetRole) Balancer.clearAndReload();
                Balancer.balancer = local;
                break;

            case RoleId.InfoSleuth:
                player.RemoveAll(x => x.PlayerId == local.PlayerId);
                InfoSleuth.infoSleuth = local;
                break;

            case RoleId.TimeMaster:
                player.RemoveAll(x => x.PlayerId == local.PlayerId);
                if (resetRole) TimeMaster.clearAndReload();
                TimeMaster.timeMaster = local;
                break;

            case RoleId.Veteran:
                player.RemoveAll(x => x.PlayerId == local.PlayerId);
                if (resetRole) Veteran.clearAndReload();
                Veteran.veteran = local;
                break;

            case RoleId.Medic:
                player.RemoveAll(x => x.PlayerId == local.PlayerId);
                if (resetRole) Medic.clearAndReload();
                Medic.medic = local;
                break;

            case RoleId.Swapper:
                player.RemoveAll(x => x.PlayerId == local.PlayerId);
                if (resetRole) Swapper.clearAndReload();
                Swapper.swapper = local;
                break;

            case RoleId.PartTimer:
                player.RemoveAll(x => x.PlayerId == local.PlayerId);
                if (resetRole) PartTimer.clearAndReload();
                PartTimer.partTimer = local;
                break;

            case RoleId.Seer:
                player.RemoveAll(x => x.PlayerId == local.PlayerId);
                if (resetRole) Seer.clearAndReload();
                Seer.seer = local;
                break;

            case RoleId.Morphling:
                player.RemoveAll(x => x.PlayerId == local.PlayerId);
                if (resetRole) Morphling.clearAndReload();
                Morphling.morphling = local;
                break;
            case RoleId.Bomber:
                player.RemoveAll(x => x.PlayerId == local.PlayerId);
                if (resetRole) Bomber.clearAndReload();
                Bomber.bomber = local;
                break;

            case RoleId.Yoyo:
                player.RemoveAll(x => x.PlayerId == local.PlayerId);
                if (resetRole) Yoyo.clearAndReload();
                Yoyo.yoyo = local;
                break;

            case RoleId.Terrorist:
                player.RemoveAll(x => x.PlayerId == local.PlayerId);
                if (resetRole) Terrorist.clearAndReload();
                Terrorist.terrorist = local;
                break;

            case RoleId.Camouflager:
                player.RemoveAll(x => x.PlayerId == local.PlayerId);
                if (resetRole) Camouflager.clearAndReload();
                Camouflager.camouflager = local;
                break;

            case RoleId.Hacker:
                player.RemoveAll(x => x.PlayerId == local.PlayerId);
                if (resetRole) Hacker.clearAndReload();
                Hacker.hacker = local;
                break;

            case RoleId.Tracker:
                player.RemoveAll(x => x.PlayerId == local.PlayerId);
                if (resetRole) Tracker.clearAndReload();
                Tracker.tracker = local;
                break;

            case RoleId.Vampire:
                player.RemoveAll(x => x.PlayerId == local.PlayerId);
                if (resetRole) Vampire.clearAndReload();
                Vampire.vampire = local;
                break;

            case RoleId.Snitch:
                player.RemoveAll(x => x.PlayerId == local.PlayerId);
                if (resetRole) Snitch.clearAndReload();
                Snitch.snitch = local;
                break;

            case RoleId.Eraser:
                player.RemoveAll(x => x.PlayerId == local.PlayerId);
                if (resetRole) Eraser.clearAndReload();
                Eraser.eraser = local;
                break;

            case RoleId.Spy:
                player.RemoveAll(x => x.PlayerId == local.PlayerId);
                if (resetRole) Spy.clearAndReload();
                Spy.spy = local;
                break;

            case RoleId.Trickster:
                player.RemoveAll(x => x.PlayerId == local.PlayerId);
                if (resetRole) Trickster.clearAndReload();
                Trickster.trickster = local;
                break;

            case RoleId.Cleaner:
                player.RemoveAll(x => x.PlayerId == local.PlayerId);
                if (resetRole) Cleaner.clearAndReload();
                Cleaner.cleaner = local;
                break;

            case RoleId.Warlock:
                player.RemoveAll(x => x.PlayerId == local.PlayerId);
                if (resetRole) Warlock.clearAndReload();
                Warlock.warlock = local;
                break;

            case RoleId.Grenadier:
                player.RemoveAll(x => x.PlayerId == local.PlayerId);
                if (resetRole) Grenadier.clearAndReload();
                Grenadier.grenadier = local;
                break;

            case RoleId.SecurityGuard:
                player.RemoveAll(x => x.PlayerId == local.PlayerId);
                if (resetRole) SecurityGuard.clearAndReload();
                SecurityGuard.securityGuard = local;
                break;

            case RoleId.Assassin:
                player.RemoveAll(x => x.PlayerId == local.PlayerId);
                Assassin.assassin.Add(local);
                break;

            case RoleId.Vigilante:
                player.RemoveAll(x => x.PlayerId == local.PlayerId);
                if (resetRole) Vigilante.clearAndReload();
                Vigilante.vigilante = local;
                break;

            case RoleId.BountyHunter:
                player.RemoveAll(x => x.PlayerId == local.PlayerId);
                if (resetRole) BountyHunter.clearAndReload();
                BountyHunter.bountyHunter = local;

                BountyHunter.bountyUpdateTimer = 0f;
                if (CachedPlayer.LocalPlayer.PlayerControl == BountyHunter.bountyHunter)
                {
                    var bottomLeft =
                        new Vector3(-FastDestroyableSingleton<HudManager>.Instance.UseButton.transform.localPosition.x,
                            FastDestroyableSingleton<HudManager>.Instance.UseButton.transform.localPosition.y,
                            FastDestroyableSingleton<HudManager>.Instance.UseButton.transform.localPosition.z) +
                        new Vector3(-0.25f, 1f, 0);
                    BountyHunter.cooldownText =
                        Object.Instantiate(FastDestroyableSingleton<HudManager>.Instance.KillButton.cooldownTimerText,
                            FastDestroyableSingleton<HudManager>.Instance.transform);
                    BountyHunter.cooldownText.alignment = TextAlignmentOptions.Center;
                    BountyHunter.cooldownText.transform.localPosition = bottomLeft + new Vector3(0f, -1f, -1f);
                    BountyHunter.cooldownText.gameObject.SetActive(true);

                    foreach (PlayerControl p in CachedPlayer.AllPlayers)
                        if (playerIcons.ContainsKey(p.PlayerId))
                        {
                            playerIcons[p.PlayerId].setSemiTransparent(false);
                            playerIcons[p.PlayerId].transform.localPosition = bottomLeft + new Vector3(0f, -1f, 0);
                            playerIcons[p.PlayerId].transform.localScale = Vector3.one * 0.4f;
                            playerIcons[p.PlayerId].gameObject.SetActive(false);
                        }
                }
                break;

            case RoleId.Medium:
                player.RemoveAll(x => x.PlayerId == local.PlayerId);
                if (resetRole) Medium.clearAndReload();
                Medium.medium = local;
                break;

            case RoleId.Witch:
                player.RemoveAll(x => x.PlayerId == local.PlayerId);
                if (resetRole) Witch.clearAndReload();
                Witch.witch = local;
                break;

            case RoleId.Jumper:
                player.RemoveAll(x => x.PlayerId == local.PlayerId);
                if (resetRole) Jumper.clearAndReload();
                Jumper.jumper = local;
                break;

            case RoleId.Escapist:
                player.RemoveAll(x => x.PlayerId == local.PlayerId);
                if (resetRole) Escapist.clearAndReload();
                Escapist.escapist = local;
                break;

            case RoleId.Trapper:
                player.RemoveAll(x => x.PlayerId == local.PlayerId);
                if (resetRole) Trapper.clearAndReload();
                Trapper.trapper = local;
                break;
            case RoleId.Ninja:
                player.RemoveAll(x => x.PlayerId == local.PlayerId);
                if (resetRole) Ninja.clearAndReload();
                Ninja.ninja = local;
                break;

            case RoleId.Blackmailer:
                player.RemoveAll(x => x.PlayerId == local.PlayerId);
                if (resetRole) Blackmailer.clearAndReload();
                Blackmailer.blackmailer = local;
                break;

            case RoleId.Miner:
                player.RemoveAll(x => x.PlayerId == local.PlayerId);
                if (resetRole) Miner.clearAndReload();
                Miner.miner = local;
                break;
            case RoleId.Undertaker:
                player.RemoveAll(x => x.PlayerId == local.PlayerId);
                if (resetRole) Undertaker.clearAndReload();
                Undertaker.undertaker = local;
                break;
            case RoleId.Prophet:
                player.RemoveAll(x => x.PlayerId == local.PlayerId);
                if (resetRole) Prophet.clearAndReload();
                Prophet.prophet = local;
                break;
            case RoleId.EvilTrapper:
                player.RemoveAll(x => x.PlayerId == local.PlayerId);
                if (resetRole) EvilTrapper.clearAndReload();
                EvilTrapper.evilTrapper = local;
                break;
            case RoleId.Gambler:
                player.RemoveAll(x => x.PlayerId == local.PlayerId);
                Gambler.gambler = local;
                break;
            case RoleId.Poucher:
                player.RemoveAll(x => x.PlayerId == local.PlayerId);
                if (resetRole) Poucher.clearAndReload();
                Poucher.poucher = local;
                break;
        }
    }

}
