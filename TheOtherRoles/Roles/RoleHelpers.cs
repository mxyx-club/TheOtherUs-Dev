using System.Collections.Generic;
using System.Linq;
using MonoMod.Utils;
using TheOtherRoles.Utilities;

namespace TheOtherRoles.Roles;

public static class RoleHelpers
{
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

    public static Dictionary<byte, byte[]> blockedRolePairings = new();

    public static void blockRole()
    {
        blockedRolePairings.Clear();

        blockedRolePairings.Add((byte)RoleId.Vampire, [(byte)RoleId.Warlock]);
        blockedRolePairings.Add((byte)RoleId.Witch, [(byte)RoleId.Warlock]);
        blockedRolePairings.Add((byte)RoleId.Warlock, [(byte)RoleId.Vampire]);

        if (CustomOptionHolder.pavlovsownerAndJackalAsWell.getBool())
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

    public static Dictionary<RoleId, int> RoleIsEnable = new();

    public static void ResetRoleSelection()
    {
        RoleIsEnable.Clear();
        RoleIsEnable.AddRange(new()
        {
            { RoleId.Sheriff, CustomOptionHolder.sheriffSpawnRate.getSelection() },
            { RoleId.Deputy, CustomOptionHolder.deputySpawnRate.getSelection() },
            { RoleId.BodyGuard, CustomOptionHolder.bodyGuardSpawnRate.getSelection() },
            { RoleId.Balancer, CustomOptionHolder.balancerSpawnRate.getSelection() },
            { RoleId.Detective, CustomOptionHolder.detectiveSpawnRate.getSelection() },
            { RoleId.Engineer, CustomOptionHolder.engineerSpawnRate.getSelection() },
            { RoleId.Hacker, CustomOptionHolder.hackerSpawnRate.getSelection() },
            { RoleId.InfoSleuth, CustomOptionHolder.infoSleuthSpawnRate.getSelection() },
            { RoleId.Jumper, CustomOptionHolder.jumperSpawnRate.getSelection() },
            { RoleId.Mayor, CustomOptionHolder.mayorSpawnRate.getSelection() },
            { RoleId.Medic, CustomOptionHolder.medicSpawnRate.getSelection() },
            { RoleId.Medium, CustomOptionHolder.mediumSpawnRate.getSelection() },
            { RoleId.Portalmaker, CustomOptionHolder.portalmakerSpawnRate.getSelection() },
            { RoleId.Prophet, CustomOptionHolder.prophetSpawnRate.getSelection() },
            { RoleId.Prosecutor, CustomOptionHolder.prosecutorSpawnRate.getSelection() },
            { RoleId.SecurityGuard, CustomOptionHolder.securityGuardSpawnRate.getSelection() },
            { RoleId.Seer, CustomOptionHolder.seerSpawnRate.getSelection() },
            { RoleId.Snitch, CustomOptionHolder.snitchSpawnRate.getSelection() },
            { RoleId.Spy, CustomOptionHolder.spySpawnRate.getSelection() },
            { RoleId.Swapper, CustomOptionHolder.swapperSpawnRate.getSelection() },
            { RoleId.TimeMaster, CustomOptionHolder.timeMasterSpawnRate.getSelection() },
            { RoleId.Tracker, CustomOptionHolder.trackerSpawnRate.getSelection() },
            { RoleId.Trapper, CustomOptionHolder.trapperSpawnRate.getSelection() },
            { RoleId.Veteran, CustomOptionHolder.veteranSpawnRate.getSelection() },
            { RoleId.Vigilante, CustomOptionHolder.guesserSpawnRate.getSelection() },

            { RoleId.Blackmailer, CustomOptionHolder.blackmailerSpawnRate.getSelection() },
            { RoleId.Bomber, CustomOptionHolder.bomberSpawnRate.getSelection() },
            { RoleId.BountyHunter, CustomOptionHolder.bountyHunterSpawnRate.getSelection() },
            { RoleId.Butcher, CustomOptionHolder.butcherSpawnRate.getSelection() },
            { RoleId.Camouflager, CustomOptionHolder.camouflagerSpawnRate.getSelection() },
            { RoleId.Cleaner, CustomOptionHolder.cleanerSpawnRate.getSelection() },
            { RoleId.Eraser, CustomOptionHolder.eraserSpawnRate.getSelection() },
            { RoleId.Escapist, CustomOptionHolder.escapistSpawnRate.getSelection() },
            { RoleId.EvilTrapper, CustomOptionHolder.evilTrapperSpawnRate.getSelection() },
            { RoleId.Gambler, CustomOptionHolder.gamblerSpawnRate.getSelection() },
            { RoleId.Mimic, CustomOptionHolder.mimicSpawnRate.getSelection() },
            { RoleId.Miner, CustomOptionHolder.minerSpawnRate.getSelection() },
            { RoleId.Morphling, CustomOptionHolder.morphlingSpawnRate.getSelection() },
            { RoleId.Ninja, CustomOptionHolder.ninjaSpawnRate.getSelection() },
            { RoleId.Poucher, CustomOptionHolder.poucherSpawnRate.getSelection() },
            { RoleId.Terrorist, CustomOptionHolder.terroristSpawnRate.getSelection() },
            { RoleId.Trickster, CustomOptionHolder.tricksterSpawnRate.getSelection() },
            { RoleId.Undertaker, CustomOptionHolder.undertakerSpawnRate.getSelection() },
            { RoleId.Vampire, CustomOptionHolder.vampireSpawnRate.getSelection() },
            { RoleId.Warlock, CustomOptionHolder.warlockSpawnRate.getSelection() },
            { RoleId.Witch, CustomOptionHolder.witchSpawnRate.getSelection() },
            { RoleId.Yoyo, CustomOptionHolder.yoyoSpawnRate.getSelection() },
            { RoleId.Grenadier, CustomOptionHolder.grenadierSpawnRate.getSelection() },

            { RoleId.Akujo, CustomOptionHolder.akujoSpawnRate.getSelection() },
            { RoleId.Amnisiac, CustomOptionHolder.amnisiacSpawnRate.getSelection() },
            { RoleId.Arsonist, CustomOptionHolder.arsonistSpawnRate.getSelection() },
            { RoleId.Doomsayer, CustomOptionHolder.doomsayerSpawnRate.getSelection() },
            { RoleId.Executioner, CustomOptionHolder.executionerSpawnRate.getSelection() },
            { RoleId.Jackal, CustomOptionHolder.jackalSpawnRate.getSelection() },
            { RoleId.Sidekick, CustomOptionHolder.jackalSpawnRate.getSelection() },
            { RoleId.Jester, CustomOptionHolder.jesterSpawnRate.getSelection() },
            { RoleId.Juggernaut, CustomOptionHolder.juggernautSpawnRate.getSelection() },
            { RoleId.Lawyer, CustomOptionHolder.lawyerSpawnRate.getSelection() },
            { RoleId.PartTimer, CustomOptionHolder.partTimerSpawnRate.getSelection() },
            { RoleId.Pavlovsowner, CustomOptionHolder.pavlovsownerSpawnRate.getSelection() },
            { RoleId.Pavlovsdogs, CustomOptionHolder.pavlovsownerSpawnRate.getSelection() },
            { RoleId.Survivor, CustomOptionHolder.survivorSpawnRate.getSelection() },
            { RoleId.Swooper, CustomOptionHolder.swooperSpawnRate.getSelection() },
            { RoleId.Thief, CustomOptionHolder.thiefSpawnRate.getSelection() },
            { RoleId.Vulture, CustomOptionHolder.vultureSpawnRate.getSelection() },
            { RoleId.Werewolf, CustomOptionHolder.werewolfSpawnRate.getSelection() },
            { RoleId.Pursuer, CustomOptionHolder.lawyerSpawnRate.getSelection() + CustomOptionHolder.executionerSpawnRate.getSelection() },

            { RoleId.Lover, CustomOptionHolder.modifierLover.getSelection() },
            { RoleId.Aftermath, CustomOptionHolder.modifierAftermath.getSelection() },
            { RoleId.AntiTeleport, CustomOptionHolder.modifierAntiTeleport.getSelection() },
            { RoleId.Assassin, CustomOptionHolder.modifierAssassin.getSelection() },
            { RoleId.Bait, CustomOptionHolder.modifierBait.getSelection() },
            { RoleId.Blind, CustomOptionHolder.modifierBlind.getSelection() },
            { RoleId.Bloody, CustomOptionHolder.modifierBloody.getSelection() },
            { RoleId.ButtonBarry, CustomOptionHolder.modifierButtonBarry.getSelection() },
            { RoleId.Chameleon, CustomOptionHolder.modifierChameleon.getSelection() },
            { RoleId.Cursed, CustomOptionHolder.modifierCursed.getSelection() },
            { RoleId.Disperser, CustomOptionHolder.modifierDisperser.getSelection() },
            { RoleId.Flash, CustomOptionHolder.modifierFlash.getSelection() },
            { RoleId.Giant, CustomOptionHolder.modifierGiant.getSelection() },
            { RoleId.Indomitable, CustomOptionHolder.modifierIndomitable.getSelection() },
            { RoleId.Invert, CustomOptionHolder.modifierInvert.getSelection() },
            { RoleId.LastImpostor, CustomOptionHolder.modifierLastImpostor.getSelection() },
            { RoleId.Mini, CustomOptionHolder.modifierMini.getSelection() },
            { RoleId.Multitasker, CustomOptionHolder.modifierMultitasker.getSelection() },
            { RoleId.Radar, CustomOptionHolder.modifierRadar.getSelection() },
            { RoleId.Shifter, CustomOptionHolder.modifierShifter.getSelection() },
            { RoleId.Slueth, CustomOptionHolder.modifierSlueth.getSelection() },
            { RoleId.Specoality, CustomOptionHolder.modifierSpecoality.getSelection() },
            { RoleId.Tiebreaker, CustomOptionHolder.modifierTieBreaker.getSelection() },
            { RoleId.Torch, CustomOptionHolder.modifierTorch.getSelection() },
            { RoleId.Tunneler, CustomOptionHolder.modifierTunneler.getSelection() },
            { RoleId.Vip, CustomOptionHolder.modifierVip.getSelection() },
            { RoleId.Watcher, CustomOptionHolder.modifierWatcher.getSelection()}
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
        Balancer.clearAndReload();
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

        blockRole();
        ResetRoleSelection();
    }
}