using System;
using System.Collections.Generic;
using AmongUs.GameOptions;
using Hazel;
using MonoMod.Utils;
using TheOtherRoles.CustomGameModes;
using TheOtherRoles.Utilities;
using UnityEngine;

namespace TheOtherRoles.Roles;

public static class RolesHelper
{

    public static readonly RoleId[] AllRoles = EnumHelper.GetAllValues<RoleId>();
    public static Dictionary<RoleId, Type> DuplicatedRoles;

    public static Dictionary<byte, RoleId> DevRole = new();
    public static bool IsAlive(this GameData.PlayerInfo player)
    {
        return player != null && !player.Disconnected && !player.IsDead;
    }

    public static bool IsDead(this GameData.PlayerInfo player)
    {
        return player == null || player.Disconnected || player.IsDead;
    }

    public static bool IsAlive(this PlayerControl player)
    {
        return player != null && !player.Data.Disconnected && !player.Data.IsDead;
    }

    public static bool IsDead(this PlayerControl player)
    {
        return player == null || player.Data.Disconnected || player.Data.IsDead;
    }

    public static bool CanAssign(this RoleId role)
    {
        var roleInfo = role.GetRoleInfo();

        return roleInfo?.CanAssign ?? true;
    }

    public static int GetRandomIndex<T>(List<T> list)
    {
        var indexData = UnityEngine.Random.Range(0, list.Count);
        return indexData;
    }

    public static RoleTypes GetRoleTypes(this RoleId role)
    {
        return role switch
        {
            RoleId.GM => RoleTypes.GuardianAngel,
            _ => role.IsImpostor() ? RoleTypes.Impostor : RoleTypes.Crewmate,
        };
    }

    public static int GetRoleCount(this RoleId role)
    {
        var info = role.GetRoleInfo();
        if (info == null) return 0;
        if (info.MaxPlayer == 1) return 1;
        Message($"{role} {info.PlayerCountOption?.GetInt() ?? 0}", "GetRoleCount");
        return info.PlayerCountOption?.GetInt() ?? 0;
    }

    public static bool IsImpostor(this RoleId role)
    {
        var roleInfo = role.GetRoleInfo();
        if (roleInfo != null)
            return roleInfo.RoleType == RoleType.Impostor;
        return false;
    }

    public static bool IsNeutral(this RoleId role)
    {
        var roleInfo = role.GetRoleInfo();
        if (roleInfo != null)
            return roleInfo.RoleType == RoleType.Neutral;
        return false;
    }

    public static bool IsKillerNeutral(this RoleId role)
    {
        var info = role.GetRoleInfo();
        return info != null && (int)info.RoleId is >= 75 and < 100;
    }

    public static bool IsCrewmate(this RoleId role)
    {
        var roleInfo = role.GetRoleInfo();
        if (roleInfo != null)
            return roleInfo.RoleType == RoleType.Crewmate;
        return false;
    }

    public static RoleInfos GetRoleInfo(RoleId role)
    {
        return CustomRoleManager.AllRolesInfo.TryGetValue(role, out RoleInfos result) ? result : null;
    }

    public static void SetRoleRPC(this PlayerControl Player, RoleId selectRoleData)
    {
        MessageWriter killWriter = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.SetRole, SendOption.Reliable, -1);
        killWriter.Write(Player.PlayerId);
        killWriter.Write((byte)selectRoleData);
        AmongUsClient.Instance.FinishRpcImmediately(killWriter);
        RPCProcedure.SetRole(Player.PlayerId, (byte)selectRoleData);
    }

    public static void SetRole(this PlayerControl player, RoleId role)
    {
        bool flag = player.GetRole() != role && player.PlayerId == CachedPlayer.LocalPlayer.PlayerId;
        if (flag)
        {
            CustomRoleManager.SetRole(player, role);
            //PlayerControlHelper.RefreshRoleDescription(PlayerControl.LocalPlayer);
        }
        Info(player.Data.PlayerName + " >= " + role);
    }

    public static Color GetRoleColor(RoleId role, PlayerControl player = null, bool IsImpostorReturn = false)
    {
        RoleInfos roleInfo = GetRoleInfo(role);
        if (roleInfo != null)
            return roleInfo.Color;
        return Color.black;
        //return IntroData.GetIntrodata(role, player, IsImpostorReturn)?.color ?? new();
    }

    public static bool IsHidden(this RoleId role)
    {
        var roleInfo = role.GetRoleInfo();

        return roleInfo?.IsHidden ?? false;
    }

    public static bool IsCanNotOpen(this RoleId role)
    {
        var roleInfo = role.GetRoleInfo();
        return roleInfo?.CanAssign ?? true;
    }

    public static RoleType GetRoleTeam(RoleId role, PlayerControl player = null, bool IsImpostorReturn = false)
    {
        RoleInfos roleInfo = GetRoleInfo(role);
        if (roleInfo != null)
            return roleInfo.RoleType;
        return RoleType.Special;
        //return IntroData.GetIntrodata(role, player, IsImpostorReturn)?.Team ?? RoleType.Crewmate;
    }

    /*
    public static string GetRoleIntro(RoleId role, PlayerControl player = null)
    {
        IntroInfo introInfo = IntroInfo.GetIntroInfo(role);
        if (introInfo != null)
            return introInfo.IntroDesc;
        return "";
        //return IntroData.GetIntrodata(role, player)?.TitleDesc;
    }*/





    #region æ…÷∞“µHelpers
    public static Dictionary<byte, byte[]> blockedRolePairings = new();
    public static Dictionary<RoleId, int> RoleIsEnable = new();
    public static void blockRole()
    {
        blockedRolePairings.Clear();

        blockedRolePairings.Add((byte)RoleId.Vampire, [(byte)RoleId.Warlock]);
        blockedRolePairings.Add((byte)RoleId.Witch, [(byte)RoleId.Warlock]);
        blockedRolePairings.Add((byte)RoleId.Warlock, [(byte)RoleId.Vampire]);

        if (CustomOptionHolder.pavlovsownerAndJackalAsWell.GetBool())
        {
            blockedRolePairings.Add((byte)RoleId.Jackal, [(byte)RoleId.Pavlovsowner]);
            blockedRolePairings.Add((byte)RoleId.Pavlovsowner, [(byte)RoleId.Jackal]);
        }
        if (Executioner.promotesToLawyer)
        {
            blockedRolePairings.Add((byte)RoleId.Executioner, [(byte)RoleId.Lawyer]);
            blockedRolePairings.Add((byte)RoleId.Lawyer, [(byte)RoleId.Executioner]);
        }

        blockedRolePairings.Add((byte)RoleId.Vulture, [(byte)RoleId.Cleaner]);
        blockedRolePairings.Add((byte)RoleId.Cleaner, [(byte)RoleId.Vulture]);

        blockedRolePairings.Add((byte)RoleId.Ninja, [(byte)RoleId.Swooper]);
        blockedRolePairings.Add((byte)RoleId.Swooper, [(byte)RoleId.Ninja]);
    }

    public static bool CanMultipleShots(PlayerControl dyingTarget)
    {
        if (dyingTarget == CachedPlayer.LocalPlayer.PlayerControl)
            return false;

        if (ModOption.gameMode != CustomGamemodes.Guesser)
        {
            if (PlayerControl.LocalPlayer == Vigilante.vigilante
                && HandleGuesser.remainingShots(CachedPlayer.LocalPlayer.PlayerId) > 0
                && Vigilante.hasMultipleShotsPerMeeting) return true;
            else if (Assassin.assassin.Any(x => x == PlayerControl.LocalPlayer)
                && HandleGuesser.remainingShots(CachedPlayer.LocalPlayer.PlayerId) > 0
                && Assassin.assassinMultipleShotsPerMeeting) return true;
        }

        else if (HandleGuesser.isGuesser(CachedPlayer.LocalPlayer.PlayerId)
            && HandleGuesser.remainingShots(CachedPlayer.LocalPlayer.PlayerId) > 0
            && HandleGuesser.hasMultipleShotsPerMeeting) return true;

        return CachedPlayer.LocalPlayer.PlayerControl == Doomsayer.doomsayer && Doomsayer.hasMultipleShotsPerMeeting &&
               Doomsayer.CanShoot;
    }

    public static void ResetRoleSelection()
    {
        RoleIsEnable.Clear();
        RoleIsEnable.AddRange(new()
        {
            { RoleId.Sheriff, CustomOptionHolder.sheriffSpawnRate.GetSelection() },
            { RoleId.Deputy, CustomOptionHolder.deputySpawnRate.GetSelection() },
            { RoleId.BodyGuard, CustomOptionHolder.bodyGuardSpawnRate.GetSelection() },
            //{ RoleId.Balancer, Balancer.balancerSpawnRate.getSelection() },
            { RoleId.Detective, CustomOptionHolder.detectiveSpawnRate.GetSelection() },
            { RoleId.Engineer, CustomOptionHolder.engineerSpawnRate.GetSelection() },
            { RoleId.Hacker, CustomOptionHolder.hackerSpawnRate.GetSelection() },
            { RoleId.InfoSleuth, CustomOptionHolder.infoSleuthSpawnRate.GetSelection() },
            { RoleId.Jumper, CustomOptionHolder.jumperSpawnRate.GetSelection() },
            { RoleId.Mayor, CustomOptionHolder.mayorSpawnRate.GetSelection() },
            { RoleId.Medic, CustomOptionHolder.medicSpawnRate.GetSelection() },
            { RoleId.Medium, CustomOptionHolder.mediumSpawnRate.GetSelection() },
            { RoleId.Portalmaker, CustomOptionHolder.portalmakerSpawnRate.GetSelection() },
            { RoleId.Prophet, CustomOptionHolder.prophetSpawnRate.GetSelection() },
            { RoleId.Prosecutor, CustomOptionHolder.prosecutorSpawnRate.GetSelection() },
            { RoleId.SecurityGuard, CustomOptionHolder.securityGuardSpawnRate.GetSelection() },
            { RoleId.Seer, CustomOptionHolder.seerSpawnRate.GetSelection() },
            { RoleId.Snitch, CustomOptionHolder.snitchSpawnRate.GetSelection() },
            { RoleId.Spy, CustomOptionHolder.spySpawnRate.GetSelection() },
            { RoleId.Swapper, CustomOptionHolder.swapperSpawnRate.GetSelection() },
            { RoleId.TimeMaster, CustomOptionHolder.timeMasterSpawnRate.GetSelection() },
            { RoleId.Tracker, CustomOptionHolder.trackerSpawnRate.GetSelection() },
            { RoleId.Trapper, CustomOptionHolder.trapperSpawnRate.GetSelection() },
            { RoleId.Veteran, CustomOptionHolder.veteranSpawnRate.GetSelection() },
            { RoleId.Vigilante, CustomOptionHolder.guesserSpawnRate.GetSelection() },

            { RoleId.Blackmailer, CustomOptionHolder.blackmailerSpawnRate.GetSelection() },
            { RoleId.Bomber, CustomOptionHolder.bomberSpawnRate.GetSelection() },
            { RoleId.BountyHunter, CustomOptionHolder.bountyHunterSpawnRate.GetSelection() },
            { RoleId.Butcher, CustomOptionHolder.butcherSpawnRate.GetSelection() },
            { RoleId.Camouflager, CustomOptionHolder.camouflagerSpawnRate.GetSelection() },
            { RoleId.Cleaner, CustomOptionHolder.cleanerSpawnRate.GetSelection() },
            { RoleId.Eraser, CustomOptionHolder.eraserSpawnRate.GetSelection() },
            { RoleId.Escapist, CustomOptionHolder.escapistSpawnRate.GetSelection() },
            { RoleId.EvilTrapper, CustomOptionHolder.evilTrapperSpawnRate.GetSelection() },
            { RoleId.Gambler, CustomOptionHolder.gamblerSpawnRate.GetSelection() },
            { RoleId.Mimic, CustomOptionHolder.mimicSpawnRate.GetSelection() },
            { RoleId.Miner, CustomOptionHolder.minerSpawnRate.GetSelection() },
            { RoleId.Morphling, CustomOptionHolder.morphlingSpawnRate.GetSelection() },
            { RoleId.Ninja, CustomOptionHolder.ninjaSpawnRate.GetSelection() },
            { RoleId.Poucher, CustomOptionHolder.poucherSpawnRate.GetSelection() },
            { RoleId.Terrorist, CustomOptionHolder.terroristSpawnRate.GetSelection() },
            { RoleId.Trickster, CustomOptionHolder.tricksterSpawnRate.GetSelection() },
            { RoleId.Undertaker, CustomOptionHolder.undertakerSpawnRate.GetSelection() },
            { RoleId.Vampire, CustomOptionHolder.vampireSpawnRate.GetSelection() },
            { RoleId.Warlock, CustomOptionHolder.warlockSpawnRate.GetSelection() },
            { RoleId.Witch, CustomOptionHolder.witchSpawnRate.GetSelection() },
            { RoleId.Yoyo, CustomOptionHolder.yoyoSpawnRate.GetSelection() },
            { RoleId.Grenadier, CustomOptionHolder.grenadierSpawnRate.GetSelection() },

            { RoleId.Akujo, CustomOptionHolder.akujoSpawnRate.GetSelection() },
            { RoleId.Amnisiac, CustomOptionHolder.amnisiacSpawnRate.GetSelection() },
            { RoleId.Arsonist, CustomOptionHolder.arsonistSpawnRate.GetSelection() },
            { RoleId.Doomsayer, CustomOptionHolder.doomsayerSpawnRate.GetSelection() },
            { RoleId.Executioner, CustomOptionHolder.executionerSpawnRate.GetSelection() },
            { RoleId.Jackal, CustomOptionHolder.jackalSpawnRate.GetSelection() },
            { RoleId.Sidekick, CustomOptionHolder.jackalSpawnRate.GetSelection() },
            { RoleId.Jester, CustomOptionHolder.jesterSpawnRate.GetSelection() },
            { RoleId.Juggernaut, CustomOptionHolder.juggernautSpawnRate.GetSelection() },
            { RoleId.Lawyer, CustomOptionHolder.lawyerSpawnRate.GetSelection() },
            { RoleId.PartTimer, CustomOptionHolder.partTimerSpawnRate.GetSelection() },
            { RoleId.Pavlovsowner, CustomOptionHolder.pavlovsownerSpawnRate.GetSelection() },
            { RoleId.Pavlovsdogs, CustomOptionHolder.pavlovsownerSpawnRate.GetSelection() },
            { RoleId.Survivor, CustomOptionHolder.survivorSpawnRate.GetSelection() },
            { RoleId.Swooper, CustomOptionHolder.swooperSpawnRate.GetSelection() },
            { RoleId.Thief, CustomOptionHolder.thiefSpawnRate.GetSelection() },
            { RoleId.Vulture, CustomOptionHolder.vultureSpawnRate.GetSelection() },
            { RoleId.Werewolf, CustomOptionHolder.werewolfSpawnRate.GetSelection() },
            { RoleId.Pursuer, CustomOptionHolder.lawyerSpawnRate.GetSelection() + CustomOptionHolder.executionerSpawnRate.GetSelection() },

            { RoleId.Lover, CustomOptionHolder.modifierLover.GetSelection() },
            { RoleId.Aftermath, CustomOptionHolder.modifierAftermath.GetSelection() },
            { RoleId.AntiTeleport, CustomOptionHolder.modifierAntiTeleport.GetSelection() },
            { RoleId.Assassin, CustomOptionHolder.modifierAssassin.GetSelection() },
            { RoleId.Bait, CustomOptionHolder.modifierBait.GetSelection() },
            { RoleId.Blind, CustomOptionHolder.modifierBlind.GetSelection() },
            { RoleId.Bloody, CustomOptionHolder.modifierBloody.GetSelection() },
            { RoleId.ButtonBarry, CustomOptionHolder.modifierButtonBarry.GetSelection() },
            { RoleId.Chameleon, CustomOptionHolder.modifierChameleon.GetSelection() },
            { RoleId.Cursed, CustomOptionHolder.modifierCursed.GetSelection() },
            { RoleId.Disperser, CustomOptionHolder.modifierDisperser.GetSelection() },
            { RoleId.Flash, CustomOptionHolder.modifierFlash.GetSelection() },
            { RoleId.Giant, CustomOptionHolder.modifierGiant.GetSelection() },
            { RoleId.Indomitable, CustomOptionHolder.modifierIndomitable.GetSelection() },
            { RoleId.Invert, CustomOptionHolder.modifierInvert.GetSelection() },
            { RoleId.LastImpostor, CustomOptionHolder.modifierLastImpostor.GetSelection() },
            { RoleId.Mini, CustomOptionHolder.modifierMini.GetSelection() },
            { RoleId.Multitasker, CustomOptionHolder.modifierMultitasker.GetSelection() },
            { RoleId.Radar, CustomOptionHolder.modifierRadar.GetSelection() },
            { RoleId.Shifter, CustomOptionHolder.modifierShifter.GetSelection() },
            { RoleId.Slueth, CustomOptionHolder.modifierSlueth.GetSelection() },
            { RoleId.Specoality, CustomOptionHolder.modifierSpecoality.GetSelection() },
            { RoleId.Tiebreaker, CustomOptionHolder.modifierTieBreaker.GetSelection() },
            { RoleId.Torch, CustomOptionHolder.modifierTorch.GetSelection() },
            { RoleId.Tunneler, CustomOptionHolder.modifierTunneler.GetSelection() },
            { RoleId.Vip, CustomOptionHolder.modifierVip.GetSelection() },
            { RoleId.Watcher, CustomOptionHolder.modifierWatcher.GetSelection()}
        });
    }

    public static void clearAndReloadRoles()
    {
        Vigilante.clearAndReload();
        Jester.clearAndReload();
        Mayor.clearAndReload();
        Prosecutor.clearAndReload();
        Portalmaker.clearAndReload();
        Poucher.clearAndReload();
        Mimic.clearAndReload();
        Engineer.clearAndReload();
        Sheriff.clearAndReload();
        InfoSleuth.clearAndReload();
        Gambler.clearAndReload();
        Butcher.clearAndReload();
        Deputy.clearAndReload();
        Amnisiac.clearAndReload();
        Detective.clearAndReload();
        Werewolf.clearAndReload();
        TimeMaster.clearAndReload();
        BodyGuard.clearAndReload();
        Veteran.clearAndReload();
        Medic.clearAndReload();
        Shifter.clearAndReload();
        Swapper.clearAndReload();
        Lovers.clearAndReload();
        Seer.clearAndReload();
        Morphling.clearAndReload();
        Camouflager.clearAndReload();
        Hacker.clearAndReload();
        Tracker.clearAndReload();
        Vampire.clearAndReload();
        Snitch.clearAndReload();
        Jackal.clearAndReload();
        Sidekick.clearAndReload();
        Pavlovsdogs.clearAndReload();
        Eraser.clearAndReload();
        Spy.clearAndReload();
        Trickster.clearAndReload();
        Cleaner.clearAndReload();
        Undertaker.clearAndReload();
        Warlock.clearAndReload();
        SecurityGuard.clearAndReload();
        Arsonist.clearAndReload();
        BountyHunter.clearAndReload();
        Vulture.clearAndReload();
        Medium.clearAndReload();
        Bomber.clearAndReload();
        Lawyer.clearAndReload();
        Executioner.clearAndReload();
        Pursuer.clearAndReload();
        Witch.clearAndReload();
        Jumper.clearAndReload();
        Prophet.clearAndReload();
        Escapist.clearAndReload();
        Ninja.clearAndReload();
        Blackmailer.clearAndReload();
        Thief.clearAndReload();
        Miner.clearAndReload();
        Trapper.clearAndReload();
        Terrorist.clearAndReload();
        Juggernaut.clearAndReload();
        Doomsayer.clearAndReload();
        Swooper.clearAndReload();
        Akujo.clearAndReload();
        Yoyo.clearAndReload();
        EvilTrapper.clearAndReload();
        Survivor.clearAndReload();
        PartTimer.clearAndReload();
        Grenadier.clearAndReload();

        // Modifier
        Assassin.clearAndReload();
        Aftermath.clearAndReload();
        Bait.clearAndReload();
        Bloody.clearAndReload();
        AntiTeleport.clearAndReload();
        Tiebreaker.clearAndReload();
        Sunglasses.clearAndReload();
        Torch.clearAndReload();
        Flash.clearAndReload();
        Blind.clearAndReload();
        Watcher.clearAndReload();
        Radar.clearAndReload();
        Tunneler.clearAndReload();
        Multitasker.clearAndReload();
        Disperser.clearAndReload();
        Mini.clearAndReload();
        Giant.clearAndReload();
        Indomitable.clearAndReload();
        Slueth.clearAndReload();
        Cursed.clearAndReload();
        Vip.clearAndReload();
        Invert.clearAndReload();
        Chameleon.clearAndReload();
        ButtonBarry.clearAndReload();
        LastImpostor.clearAndReload();
        Specoality.clearAndReload();

        // Gamemodes
        HandleGuesser.clearAndReload();
        HideNSeek.clearAndReload();
        PropHunt.clearAndReload();

        blockRole();
        ResetRoleSelection();
    }
    #endregion
}

public enum RoleType
{
    Crewmate,
    Impostor,
    Neutral,
    Modifier,
    Special,
}
