using System;
using System.Collections.Generic;
using System.Linq;
using AmongUs.GameOptions;
using Hazel;
using MonoMod.Utils;
using Reactor.Utilities.Extensions;
using TheOtherRoles.CustomGameModes;
using TheOtherRoles.Utilities;
using UnityEngine;

namespace TheOtherRoles.Patches;

[HarmonyPatch(typeof(RoleOptionsCollectionV07), nameof(RoleOptionsCollectionV07.GetNumPerGame))]
internal class RoleOptionsDataGetNumPerGamePatch
{
    public static void Postfix(ref int __result)
    {
        // Deactivate Vanilla Roles if the mod roles are active
        if (GameOptionsManager.Instance.CurrentGameOptions.GameMode == GameModes.Normal) __result = 0;
    }
}

[HarmonyPatch(typeof(IGameOptionsExtensions), nameof(IGameOptionsExtensions.GetAdjustedNumImpostors))]
internal class GameOptionsDataGetAdjustedNumImpostorsPatch
{
    public static void Postfix(ref int __result)
    {
        if (ModOption.gameMode is CustomGamemodes.HideNSeek or CustomGamemodes.PropHunt)
        {
            var impCount = ModOption.gameMode == CustomGamemodes.HideNSeek
                ? Mathf.RoundToInt(CustomOptionHolder.hideNSeekHunterCount.getFloat())
                : CustomOptionHolder.propHuntNumberOfHunters.getQuantity();
            __result = impCount;
            ; // Set Imp Num
        }
        else if (GameOptionsManager.Instance.CurrentGameOptions.GameMode == GameModes.Normal)
        {
            // Ignore Vanilla impostor limits in TOR Games.
            __result = Mathf.Clamp(GameOptionsManager.Instance.CurrentGameOptions.NumImpostors, 0, 15);
        }
    }
}

[HarmonyPatch(typeof(GameOptionsData), nameof(GameOptionsData.Validate))]
internal class GameOptionsDataValidatePatch
{
    public static void Postfix(GameOptionsData __instance)
    {
        if (ModOption.gameMode == CustomGamemodes.HideNSeek ||
            GameOptionsManager.Instance.CurrentGameOptions.GameMode != GameModes.Normal) return;
        if (ModOption.gameMode == CustomGamemodes.PropHunt)
            __instance.NumImpostors = CustomOptionHolder.propHuntNumberOfHunters.getQuantity();
        __instance.NumImpostors = GameOptionsManager.Instance.CurrentGameOptions.NumImpostors;
    }
}

[HarmonyPatch(typeof(RoleManager), nameof(RoleManager.SelectRoles))]
internal class RoleManagerSelectRolesPatch
{
    private static int crewValues;

    private static int impValues;

    //private static bool isEvilGuesser;
    private static readonly List<Tuple<byte, byte>> playerRoleMap = new();
    public static bool isGuesserGamemode => ModOption.gameMode == CustomGamemodes.Guesser;

    public static void Postfix()
    {
        var writer = AmongUsClient.Instance.StartRpcImmediately(CachedPlayer.LocalPlayer.PlayerControl.NetId,
            (byte)CustomRPC.ResetVaribles, SendOption.Reliable);
        AmongUsClient.Instance.FinishRpcImmediately(writer);
        RPCProcedure.resetVariables();
        // Don't assign Roles in Hide N Seek
        if (ModOption.gameMode == CustomGamemodes.HideNSeek || ModOption.gameMode == CustomGamemodes.PropHunt ||
            GameOptionsManager.Instance.currentGameOptions.GameMode == GameModes.HideNSeek) return;
        assignRoles();
    }

    private static void assignRoles()
    {
        var data = getRoleAssignmentData();
        selectFactionForFactionIndependentRoles(data);
        assignEnsuredRoles(data); // Assign roles that should always be in the game next
        assignDependentRoles(data); // Assign roles that may have a dependent role
        assignChanceRoles(data); // Assign roles that may or may not be in the game last
        assignRoleTargets(data); // Assign targets for Lawyer & Prosecutor
        if (isGuesserGamemode) assignGuesserGamemode();
        assignModifiers(); // Assign modifier
        setRolesAgain(); //brb
        if (Jackal.jackal != null) Jackal.setSwoop();
    }

    public static RoleAssignmentData getRoleAssignmentData()
    {
        // Get the players that we want to assign the roles to. Crewmate and Neutral roles are assigned to natural crewmates. Impostor roles to impostors.
        var crewmates = PlayerControl.AllPlayerControls.ToList().OrderBy(x => Guid.NewGuid()).ToList();
        crewmates.RemoveAll(x => x.Data.Role.IsImpostor);
        var impostors = PlayerControl.AllPlayerControls.ToList().OrderBy(x => Guid.NewGuid()).ToList();
        impostors.RemoveAll(x => !x.Data.Role.IsImpostor);

        var neutralMin = CustomOptionHolder.neutralRolesCountMin.getSelection();
        var neutralMax = CustomOptionHolder.neutralRolesCountMax.getSelection();
        var killerNeutralMin = CustomOptionHolder.killerNeutralRolesCountMin.getSelection();
        var killerNeutralMax = CustomOptionHolder.killerNeutralRolesCountMax.getSelection();
        var impostorNum = ModOption.NumImpostors;

        // Make sure min is less or equal to max
        neutralMin = Math.Min(neutralMin, neutralMax);
        killerNeutralMin = Math.Min(killerNeutralMin, killerNeutralMax);

        // Get the maximum allowed count of each role type based on the minimum and maximum option
        var neutralCountSettings = rnd.Next(neutralMin, neutralMax + 1);
        var killerNeutralCount = rnd.Next(killerNeutralMin, killerNeutralMax + 1);
        var crewCountSettings = PlayerControl.AllPlayerControls.Count - neutralCountSettings - impostorNum;

        killerNeutralCount = Math.Min(killerNeutralCount, neutralCountSettings);

        // Potentially lower the actual maximum to the assignable players
        var maxCrewmateRoles = Mathf.Min(crewmates.Count, crewCountSettings);
        var maxNeutralRoles = Mathf.Min(crewmates.Count, neutralCountSettings);
        var maxKillerNeutralRoles = Mathf.Min(crewmates.Count, killerNeutralCount);
        var maxImpostorRoles = Mathf.Min(impostors.Count, impostorNum);

        // Fill in the lists with the roles that should be assigned to players. Note that the special roles (like Mafia or Lovers) are NOT included in these lists
        var impSettings = new Dictionary<byte, int>();
        var neutralSettings = new Dictionary<byte, int>();
        var killerNeutralSettings = new Dictionary<byte, int>();
        var crewSettings = new Dictionary<byte, int>();

        impSettings.Add((byte)RoleId.Morphling, CustomOptionHolder.morphlingSpawnRate.getSelection());
        impSettings.Add((byte)RoleId.Camouflager, CustomOptionHolder.camouflagerSpawnRate.getSelection());
        impSettings.Add((byte)RoleId.Vampire, CustomOptionHolder.vampireSpawnRate.getSelection());
        impSettings.Add((byte)RoleId.Eraser, CustomOptionHolder.eraserSpawnRate.getSelection());
        impSettings.Add((byte)RoleId.Trickster, CustomOptionHolder.tricksterSpawnRate.getSelection());
        impSettings.Add((byte)RoleId.Cleaner, CustomOptionHolder.cleanerSpawnRate.getSelection());
        impSettings.Add((byte)RoleId.Undertaker, CustomOptionHolder.undertakerSpawnRate.getSelection());
        impSettings.Add((byte)RoleId.Miner, CustomOptionHolder.minerSpawnRate.getSelection());
        impSettings.Add((byte)RoleId.Butcher, CustomOptionHolder.butcherSpawnRate.getSelection());
        impSettings.Add((byte)RoleId.Warlock, CustomOptionHolder.warlockSpawnRate.getSelection());
        impSettings.Add((byte)RoleId.BountyHunter, CustomOptionHolder.bountyHunterSpawnRate.getSelection());
        impSettings.Add((byte)RoleId.Witch, CustomOptionHolder.witchSpawnRate.getSelection());
        impSettings.Add((byte)RoleId.Escapist, CustomOptionHolder.escapistSpawnRate.getSelection());
        impSettings.Add((byte)RoleId.Ninja, CustomOptionHolder.ninjaSpawnRate.getSelection());
        if (!Poucher.spawnModifier) impSettings.Add((byte)RoleId.Poucher, CustomOptionHolder.poucherSpawnRate.getSelection());
        impSettings.Add((byte)RoleId.Mimic, CustomOptionHolder.mimicSpawnRate.getSelection());
        impSettings.Add((byte)RoleId.Terrorist, CustomOptionHolder.terroristSpawnRate.getSelection());
        impSettings.Add((byte)RoleId.Bomber, CustomOptionHolder.bomberSpawnRate.getSelection());
        impSettings.Add((byte)RoleId.Blackmailer, CustomOptionHolder.blackmailerSpawnRate.getSelection());
        impSettings.Add((byte)RoleId.Yoyo, CustomOptionHolder.yoyoSpawnRate.getSelection());
        impSettings.Add((byte)RoleId.EvilTrapper, CustomOptionHolder.evilTrapperSpawnRate.getSelection());
        impSettings.Add((byte)RoleId.Gambler, CustomOptionHolder.gamblerSpawnRate.getSelection());
        impSettings.Add((byte)RoleId.Grenadier, CustomOptionHolder.grenadierSpawnRate.getSelection());

        neutralSettings.Add((byte)RoleId.Survivor, CustomOptionHolder.survivorSpawnRate.getSelection());
        //neutralSettings.Add((byte)RoleId.Pursuer, CustomOptionHolder.pursuerSpawnRate.getSelection());
        neutralSettings.Add((byte)RoleId.Amnisiac, CustomOptionHolder.amnisiacSpawnRate.getSelection());
        neutralSettings.Add((byte)RoleId.PartTimer, CustomOptionHolder.partTimerSpawnRate.getSelection());
        neutralSettings.Add((byte)RoleId.Jester, CustomOptionHolder.jesterSpawnRate.getSelection());
        neutralSettings.Add((byte)RoleId.Lawyer, CustomOptionHolder.lawyerSpawnRate.getSelection());
        neutralSettings.Add((byte)RoleId.Executioner, CustomOptionHolder.executionerSpawnRate.getSelection());
        neutralSettings.Add((byte)RoleId.Arsonist, CustomOptionHolder.arsonistSpawnRate.getSelection());
        neutralSettings.Add((byte)RoleId.Vulture, CustomOptionHolder.vultureSpawnRate.getSelection());
        neutralSettings.Add((byte)RoleId.Doomsayer, CustomOptionHolder.doomsayerSpawnRate.getSelection());
        neutralSettings.Add((byte)RoleId.Akujo, CustomOptionHolder.akujoSpawnRate.getSelection());
        neutralSettings.Add((byte)RoleId.Thief, CustomOptionHolder.thiefSpawnRate.getSelection());
        killerNeutralSettings.Add((byte)RoleId.Jackal, CustomOptionHolder.jackalSpawnRate.getSelection());
        killerNeutralSettings.Add((byte)RoleId.Pavlovsowner, CustomOptionHolder.pavlovsownerSpawnRate.getSelection());
        killerNeutralSettings.Add((byte)RoleId.Werewolf, CustomOptionHolder.werewolfSpawnRate.getSelection());
        killerNeutralSettings.Add((byte)RoleId.Juggernaut, CustomOptionHolder.juggernautSpawnRate.getSelection());
        killerNeutralSettings.Add((byte)RoleId.Swooper, CustomOptionHolder.swooperSpawnRate.getSelection());

        // Check if killerNeutralMin and killerNeutralMax are 0
        if (killerNeutralMin + killerNeutralMax == 0)
        {
            // If both are 0, treat all killer neutrals as regular neutrals
            neutralSettings.AddRange(killerNeutralSettings);
            killerNeutralCount = 0;
        }
        else
        {
            // Adjust maxNeutralRoles by allocating killerNeutral roles
            maxNeutralRoles = Math.Min(maxNeutralRoles - killerNeutralCount, neutralMax);
        }

        crewSettings.Add((byte)RoleId.Mayor, CustomOptionHolder.mayorSpawnRate.getSelection());
        crewSettings.Add((byte)RoleId.Prosecutor, CustomOptionHolder.prosecutorSpawnRate.getSelection());
        crewSettings.Add((byte)RoleId.Portalmaker, CustomOptionHolder.portalmakerSpawnRate.getSelection());
        crewSettings.Add((byte)RoleId.Engineer, CustomOptionHolder.engineerSpawnRate.getSelection());
        crewSettings.Add((byte)RoleId.BodyGuard, CustomOptionHolder.bodyGuardSpawnRate.getSelection());
        crewSettings.Add((byte)RoleId.Detective, CustomOptionHolder.detectiveSpawnRate.getSelection());
        crewSettings.Add((byte)RoleId.TimeMaster, CustomOptionHolder.timeMasterSpawnRate.getSelection());
        crewSettings.Add((byte)RoleId.Veteran, CustomOptionHolder.veteranSpawnRate.getSelection());
        crewSettings.Add((byte)RoleId.Medic, CustomOptionHolder.medicSpawnRate.getSelection());
        crewSettings.Add((byte)RoleId.Swapper, CustomOptionHolder.swapperSpawnRate.getSelection());
        crewSettings.Add((byte)RoleId.Seer, CustomOptionHolder.seerSpawnRate.getSelection());
        crewSettings.Add((byte)RoleId.Hacker, CustomOptionHolder.hackerSpawnRate.getSelection());
        crewSettings.Add((byte)RoleId.InfoSleuth, CustomOptionHolder.infoSleuthSpawnRate.getSelection());
        crewSettings.Add((byte)RoleId.Balancer, CustomOptionHolder.balancerSpawnRate.getSelection());
        crewSettings.Add((byte)RoleId.Tracker, CustomOptionHolder.trackerSpawnRate.getSelection());
        crewSettings.Add((byte)RoleId.Snitch, CustomOptionHolder.snitchSpawnRate.getSelection());
        crewSettings.Add((byte)RoleId.Medium, CustomOptionHolder.mediumSpawnRate.getSelection());
        crewSettings.Add((byte)RoleId.Prophet, CustomOptionHolder.prophetSpawnRate.getSelection());
        if (!isGuesserGamemode)
            crewSettings.Add((byte)RoleId.Vigilante, CustomOptionHolder.guesserSpawnRate.getSelection());
        crewSettings.Add((byte)RoleId.Trapper, CustomOptionHolder.trapperSpawnRate.getSelection());
        if (impostors.Count > 1)
            // Only add Spy if more than 1 impostor as the spy role is otherwise useless
            crewSettings.Add((byte)RoleId.Spy, CustomOptionHolder.spySpawnRate.getSelection());
        crewSettings.Add((byte)RoleId.SecurityGuard, CustomOptionHolder.securityGuardSpawnRate.getSelection());
        crewSettings.Add((byte)RoleId.Jumper, CustomOptionHolder.jumperSpawnRate.getSelection());
        return new RoleAssignmentData
        {
            crewmates = crewmates,
            impostors = impostors,
            crewSettings = crewSettings,
            neutralSettings = neutralSettings,
            killerNeutralSettings = killerNeutralSettings,
            impSettings = impSettings,
            maxCrewmateRoles = maxCrewmateRoles,
            maxNeutralRoles = maxNeutralRoles,
            maxKillerNeutralRoles = maxKillerNeutralRoles,
            maxImpostorRoles = maxImpostorRoles
        };
    }

    private static void selectFactionForFactionIndependentRoles(RoleAssignmentData data)
    {
        // Assign Sheriff
        if ((CustomOptionHolder.deputySpawnRate.getSelection() > 0 &&
             CustomOptionHolder.sheriffSpawnRate.getSelection() == 10) ||
            CustomOptionHolder.deputySpawnRate.getSelection() == 0)
            data.crewSettings.Add((byte)RoleId.Sheriff, CustomOptionHolder.sheriffSpawnRate.getSelection());

        crewValues = data.crewSettings.Values.ToList().Sum();
        impValues = data.impSettings.Values.ToList().Sum();
    }

    private static void assignEnsuredRoles(RoleAssignmentData data)
    {
        static List<byte> GetEnsuredRoles(Dictionary<byte, int> settings) => settings.Where(x => x.Value == 10).Select(x => x.Key).ToList();

        // Get all roles where the chance to occur is set to 100%
        var ensuredCrewmateRoles = GetEnsuredRoles(data.crewSettings);
        var ensuredNeutralRoles = GetEnsuredRoles(data.neutralSettings);
        var ensuredKillerNeutralRoles = GetEnsuredRoles(data.killerNeutralSettings);
        var ensuredImpostorRoles = GetEnsuredRoles(data.impSettings);

        // Assign roles until we run out of either players we can assign roles to or run out of roles we can assign to players
        while ((data.impostors.Count > 0 && data.maxImpostorRoles > 0 && ensuredImpostorRoles.Count > 0) ||
            (data.crewmates.Count > 0 && (
                (data.maxCrewmateRoles > 0 && ensuredCrewmateRoles.Count > 0)
                || (data.maxNeutralRoles > 0 && ensuredNeutralRoles.Count > 0)
                || (data.maxKillerNeutralRoles > 0 && ensuredKillerNeutralRoles.Count > 0)
            )))
        {
            var rolesToAssign = new Dictionary<RoleType, List<byte>>();
            if (data.crewmates.Count > 0 && data.maxCrewmateRoles > 0 && ensuredCrewmateRoles.Count > 0)
                rolesToAssign.Add(RoleType.Crewmate, ensuredCrewmateRoles);
            if (data.crewmates.Count > 0 && data.maxNeutralRoles > 0 && ensuredNeutralRoles.Count > 0)
                rolesToAssign.Add(RoleType.Neutral, ensuredNeutralRoles);
            if (data.crewmates.Count > 0 && data.maxKillerNeutralRoles > 0 && ensuredKillerNeutralRoles.Count > 0)
                rolesToAssign.Add(RoleType.KillerNeutral, ensuredKillerNeutralRoles);
            if (data.impostors.Count > 0 && data.maxImpostorRoles > 0 && ensuredImpostorRoles.Count > 0)
                rolesToAssign.Add(RoleType.Impostor, ensuredImpostorRoles);

            // Randomly select a pool of roles to assign a role from next (Crewmate role, Neutral role or Impostor role) 
            // then select one of the roles from the selected pool to a player 
            // and remove the role (and any potentially blocked role pairings) from the pool(s)
            var roleType = rolesToAssign.Keys.ElementAt(rnd.Next(0, rolesToAssign.Keys.Count));
            var players = roleType is RoleType.Crewmate or RoleType.Neutral or RoleType.KillerNeutral ? data.crewmates : data.impostors;

            var index = rnd.Next(0, rolesToAssign[roleType].Count);
            var roleId = rolesToAssign[roleType][index];
            setRoleToRandomPlayer(roleId, players);
            rolesToAssign[roleType].RemoveAt(index);

            if (RoleClass.blockedRolePairings.ContainsKey(roleId))
            {
                foreach (var blockedRoleId in RoleClass.blockedRolePairings[roleId])
                {
                    // Set chance for the blocked roles to 0 for chances less than 100%
                    if (data.impSettings.ContainsKey(blockedRoleId)) data.impSettings[blockedRoleId] = 0;
                    if (data.neutralSettings.ContainsKey(blockedRoleId)) data.neutralSettings[blockedRoleId] = 0;
                    if (data.killerNeutralSettings.ContainsKey(blockedRoleId)) data.killerNeutralSettings[blockedRoleId] = 0;
                    if (data.crewSettings.ContainsKey(blockedRoleId)) data.crewSettings[blockedRoleId] = 0;
                    // Remove blocked roles even if the chance was 100%
                    foreach (var ensuredRolesList in rolesToAssign.Values)
                        ensuredRolesList.RemoveAll(x => x == blockedRoleId);
                }
            }

            // Adjust the role limit
            switch (roleType)
            {
                case RoleType.Crewmate:
                    data.maxCrewmateRoles--;
                    crewValues -= 10;
                    break;
                case RoleType.Neutral:
                    data.maxNeutralRoles--;
                    break;
                case RoleType.KillerNeutral:
                    data.maxKillerNeutralRoles--;
                    break;
                case RoleType.Impostor:
                    data.maxImpostorRoles--;
                    impValues -= 10;
                    break;
            }
        }
    }

    private static void assignDependentRoles(RoleAssignmentData data)
    {
        // Roles that prob have a dependent role
        //bool guesserFlag = CustomOptionHolder.guesserSpawnBothRate.getSelection() > 0 
        //     && CustomOptionHolder.guesserSpawnRate.getSelection() > 0;
        var sheriffFlag = CustomOptionHolder.deputySpawnRate.getSelection() > 0
                          && CustomOptionHolder.sheriffSpawnRate.getSelection() > 0;

        //if (isGuesserGamemode) guesserFlag = false;
        // if (!guesserFlag && !sheriffFlag) return; // assignDependentRoles is not needed

        var crew = data.crewmates.Count < data.maxCrewmateRoles
            ? data.crewmates.Count
            : data.maxCrewmateRoles; // Max number of crew loops
        var imp = data.impostors.Count < data.maxImpostorRoles
            ? data.impostors.Count
            : data.maxImpostorRoles; // Max number of imp loops
        var crewSteps = crew / data.crewSettings.Keys.Count; // Avarage crewvalues deducted after each loop 
        var impSteps = imp / data.impSettings.Keys.Count; // Avarage impvalues deducted after each loop

        // set to false if needed, otherwise we can skip the loop
        var isSheriff = !sheriffFlag;

        // --- Simulate Crew & Imp ticket system ---
        while (crew > 0 && !isSheriff /* || (!isEvilGuesser && !isGuesser)*/)
        {
            if (!isSheriff && rnd.Next(crewValues) < CustomOptionHolder.sheriffSpawnRate.getSelection())
                isSheriff = true;
            crew--;
            crewValues -= crewSteps;
        }

        // --- Assign Main Roles if they won the lottery ---
        if (isSheriff && Sheriff.sheriff == null && data.crewmates.Count > 0 && data.maxCrewmateRoles > 0 &&
            sheriffFlag)
        {
            // Set Sheriff cause he won the lottery
            var sheriff = setRoleToRandomPlayer((byte)RoleId.Sheriff, data.crewmates);
            data.crewmates.ToList().RemoveAll(x => x.PlayerId == sheriff);
            data.maxCrewmateRoles--;
        }

        // --- Assign Dependent Roles if main role exists ---
        if (Sheriff.sheriff != null)
        {
            // Deputy
            if (CustomOptionHolder.deputySpawnRate.getSelection() == 10 && data.crewmates.Count > 0 &&
                data.maxCrewmateRoles > 0)
            {
                // Force Deputy
                var deputy = setRoleToRandomPlayer((byte)RoleId.Deputy, data.crewmates);
                data.crewmates.ToList().RemoveAll(x => x.PlayerId == deputy);
                data.maxCrewmateRoles--;
            }
            else if (CustomOptionHolder.deputySpawnRate.getSelection() <
                     10) // Dont force, add Deputy to the ticket system
            {
                data.crewSettings.Add((byte)RoleId.Deputy, CustomOptionHolder.deputySpawnRate.getSelection());
            }
        }

        if (!data.crewSettings.ContainsKey((byte)RoleId.Sheriff)) data.crewSettings.Add((byte)RoleId.Sheriff, 0);
    }

    private static void assignChanceRoles(RoleAssignmentData data)
    {
        static List<byte> GetEnsuredRoles(Dictionary<byte, int> settings) =>
            settings.Where(x => x.Value is > 0 and < 10).Select(x => x.Key).ToList();

        // Get all roles where the chance to occur is set grater than 0% but not 100% and build a ticket pool based on their weight
        var crewmateTickets = GetEnsuredRoles(data.crewSettings);
        var neutralTickets = GetEnsuredRoles(data.neutralSettings);
        var killerNeutralTickets = GetEnsuredRoles(data.killerNeutralSettings);
        var impostorTickets = GetEnsuredRoles(data.impSettings);

        // Assign roles until we run out of either players we can assign roles to or run out of roles we can assign to players
        while ((data.impostors.Count > 0 && data.maxImpostorRoles > 0 && impostorTickets.Count > 0) ||
            (data.crewmates.Count > 0 && (
                (data.maxCrewmateRoles > 0 && crewmateTickets.Count > 0)
                || (data.maxNeutralRoles > 0 && neutralTickets.Count > 0)
                || (data.maxKillerNeutralRoles > 0 && killerNeutralTickets.Count > 0))))
        {
            var rolesToAssign = new Dictionary<RoleType, List<byte>>();
            if (data.crewmates.Count > 0 && data.maxCrewmateRoles > 0 && crewmateTickets.Count > 0)
                rolesToAssign.Add(RoleType.Crewmate, crewmateTickets);
            if (data.crewmates.Count > 0 && data.maxNeutralRoles > 0 && neutralTickets.Count > 0)
                rolesToAssign.Add(RoleType.Neutral, neutralTickets);
            if (data.crewmates.Count > 0 && data.maxKillerNeutralRoles > 0 && killerNeutralTickets.Count > 0)
                rolesToAssign.Add(RoleType.KillerNeutral, killerNeutralTickets);
            if (data.impostors.Count > 0 && data.maxImpostorRoles > 0 && impostorTickets.Count > 0)
                rolesToAssign.Add(RoleType.Impostor, impostorTickets);

            // Randomly select a pool of role tickets to assign a role from next (Crewmate role, Neutral role or Impostor role) 
            // then select one of the roles from the selected pool to a player 
            // and remove all tickets of this role (and any potentially blocked role pairings) from the pool(s)
            var roleType = rolesToAssign.Keys.ElementAt(rnd.Next(0, rolesToAssign.Keys.Count));
            var players = roleType is RoleType.Crewmate or RoleType.Neutral or RoleType.KillerNeutral ? data.crewmates : data.impostors;
            var index = rnd.Next(0, rolesToAssign[roleType].Count);
            var roleId = rolesToAssign[roleType][index];
            setRoleToRandomPlayer(roleId, players);
            rolesToAssign[roleType].RemoveAll(x => x == roleId);

            if (RoleClass.blockedRolePairings.ContainsKey(roleId))
                foreach (var blockedRoleId in RoleClass.blockedRolePairings[roleId])
                {
                    // Remove tickets of blocked roles from all pools
                    crewmateTickets.RemoveAll(x => x == blockedRoleId);
                    neutralTickets.RemoveAll(x => x == blockedRoleId);
                    killerNeutralTickets.RemoveAll(x => x == blockedRoleId);
                    impostorTickets.RemoveAll(x => x == blockedRoleId);
                }

            // Adjust the role limit
            switch (roleType)
            {
                case RoleType.Crewmate:
                    data.maxCrewmateRoles--;
                    break;
                case RoleType.Neutral:
                    data.maxNeutralRoles--;
                    break;
                case RoleType.KillerNeutral:
                    data.maxKillerNeutralRoles--;
                    break;
                case RoleType.Impostor:
                    data.maxImpostorRoles--;
                    break;
            }
        }
    }

    private static void assignRoleTargets(RoleAssignmentData data)
    {
        // Set Lawyer or Prosecutor Target
        if (Lawyer.lawyer != null)
        {
            var possibleTargets = new List<PlayerControl>();
            // Lawyer
            foreach (PlayerControl p in CachedPlayer.AllPlayers)
                if (!p.Data.IsDead && !p.Data.Disconnected && p != Lovers.lover1 && p != Lovers.lover2 &&
                    (p.Data.Role.IsImpostor || p == Swooper.swooper || p == Jackal.jackal || p == Juggernaut.juggernaut ||
                     p == Werewolf.werewolf || (Lawyer.targetCanBeJester && p == Jester.jester)))
                    possibleTargets.Add(p);

            if (possibleTargets.Count == 0)
            {
                var w = AmongUsClient.Instance.StartRpcImmediately(CachedPlayer.LocalPlayer.PlayerControl.NetId,
                    (byte)CustomRPC.LawyerPromotesToPursuer, SendOption.Reliable);
                AmongUsClient.Instance.FinishRpcImmediately(w);
                RPCProcedure.lawyerPromotesToPursuer();
            }
            else
            {
                var target = possibleTargets[rnd.Next(0, possibleTargets.Count)];
                var writer = AmongUsClient.Instance.StartRpcImmediately(CachedPlayer.LocalPlayer.PlayerControl.NetId,
                    (byte)CustomRPC.LawyerSetTarget, SendOption.Reliable);
                writer.Write(target.PlayerId);
                AmongUsClient.Instance.FinishRpcImmediately(writer);
                RPCProcedure.lawyerSetTarget(target.PlayerId);
            }
        }

        // Executioner
        if (Executioner.executioner != null)
        {
            var possibleTargets = new List<PlayerControl>();
            // Executioner
            foreach (PlayerControl p in CachedPlayer.AllPlayers)
                if (!p.Data.IsDead && !p.Data.Disconnected && p != Lovers.lover1 && p != Lovers.lover2 &&
                    p != Mini.mini && !p.Data.Role.IsImpostor && !isNeutral(p) && p != Swapper.swapper)
                    possibleTargets.Add(p);

            if (possibleTargets.Count == 0)
            {
                var w = AmongUsClient.Instance.StartRpcImmediately(CachedPlayer.LocalPlayer.PlayerControl.NetId,
                    (byte)CustomRPC.ExecutionerPromotesRole, SendOption.Reliable);
                AmongUsClient.Instance.FinishRpcImmediately(w);
                RPCProcedure.executionerPromotesRole();
            }
            else
            {
                var target = possibleTargets[rnd.Next(0, possibleTargets.Count)];
                var writer = AmongUsClient.Instance.StartRpcImmediately(CachedPlayer.LocalPlayer.PlayerControl.NetId,
                    (byte)CustomRPC.ExecutionerSetTarget, SendOption.Reliable);
                writer.Write(target.PlayerId);
                AmongUsClient.Instance.FinishRpcImmediately(writer);
                RPCProcedure.executionerSetTarget(target.PlayerId);
            }
        }
    }

    private static void assignModifiers()
    {
        var modifierMin = CustomOptionHolder.modifiersCountMin.getSelection();
        var modifierMax = CustomOptionHolder.modifiersCountMax.getSelection();
        if (modifierMin > modifierMax) modifierMin = modifierMax;
        var modifierCountSettings = rnd.Next(modifierMin, modifierMax + 1);
        var players = PlayerControl.AllPlayerControls.ToArray().ToList();
        if (isGuesserGamemode && !CustomOptionHolder.guesserGamemodeHaveModifier.getBool())
            players.RemoveAll(x => GuesserGM.isGuesser(x.PlayerId));

        var impPlayer = new List<PlayerControl>(players);
        var neutralPlayer = new List<PlayerControl>(players);
        var impPlayerL = new List<PlayerControl>(players);
        var crewPlayer = new List<PlayerControl>(players);
        impPlayer.RemoveAll(x => !x.Data.Role.IsImpostor);
        neutralPlayer.RemoveAll(x => !isNeutral(x));
        impPlayerL.RemoveAll(x => !x.Data.Role.IsImpostor);
        crewPlayer.RemoveAll(x => x.Data.Role.IsImpostor || isNeutral(x));

        var modifierCount = Mathf.Min(players.Count, modifierCountSettings);

        if (modifierCount == 0) return;

        var allModifiers = new List<RoleId>();
        var ensuredModifiers = new List<RoleId>();
        var chanceModifiers = new List<RoleId>();

        var impModifiers = new List<RoleId>();
        var ensuredImpModifiers = new List<RoleId>();
        var chanceImpModifiers = new List<RoleId>();
        allModifiers.AddRange(
        [
            RoleId.Aftermath,
            RoleId.Tiebreaker,
            RoleId.Mini,
            RoleId.Giant,
            RoleId.Bait,
            RoleId.Bloody,
            RoleId.AntiTeleport,
            RoleId.Sunglasses,
            RoleId.Torch,
            RoleId.Flash,
            RoleId.Multitasker,
            RoleId.ButtonBarry,
            RoleId.Vip,
            RoleId.Invert,
            RoleId.Indomitable,
            RoleId.Tunneler,
            RoleId.Slueth,
            RoleId.Blind,
            RoleId.Watcher,
            RoleId.Radar,
            RoleId.Disperser,
            RoleId.Specoality,
            RoleId.PoucherModifier,
            RoleId.Cursed,
            RoleId.Chameleon,
            RoleId.Shifter,
        ]);

        impModifiers.AddRange(
        [
            RoleId.Assassin
        ]);

        if (rnd.Next(1, 101) <= CustomOptionHolder.modifierLover.getSelection() * 10)
        {
            // Assign lover
            var isEvilLover = rnd.Next(1, 101) <= CustomOptionHolder.modifierLoverImpLoverRate.getSelection() * 10;
            byte firstLoverId;

            if (isEvilLover) firstLoverId = setModifierToRandomPlayer((byte)RoleId.Lover, impPlayerL);
            else firstLoverId = setModifierToRandomPlayer((byte)RoleId.Lover, crewPlayer);
            var secondLoverId = setModifierToRandomPlayer((byte)RoleId.Lover, crewPlayer, 1);

            players.RemoveAll(x => x.PlayerId == firstLoverId || x.PlayerId == secondLoverId);
            modifierCount--;
        }

        foreach (var m in allModifiers)
            if (getSelectionForRoleId(m) == 10)
                ensuredModifiers.AddRange(Enumerable.Repeat(m, getSelectionForRoleId(m, true) / 10));
            else chanceModifiers.AddRange(Enumerable.Repeat(m, getSelectionForRoleId(m, true)));
        foreach (var m in impModifiers)
            if (getSelectionForRoleId(m) == 10)
                ensuredImpModifiers.AddRange(Enumerable.Repeat(m, getSelectionForRoleId(m, true) / 10));
            else chanceImpModifiers.AddRange(Enumerable.Repeat(m, getSelectionForRoleId(m, true)));

        assignModifiersToPlayers(ensuredImpModifiers, impPlayer, modifierCount); // Assign ensured imp modifier
        assignModifiersToPlayers(ensuredModifiers, players, modifierCount); // Assign ensured modifier

        modifierCount -= ensuredImpModifiers.Count + ensuredModifiers.Count;
        if (modifierCount <= 0) return;
        var chanceModifierCount = Mathf.Min(modifierCount, chanceModifiers.Count);
        var chanceModifierToAssign = new List<RoleId>();
        while (chanceModifierCount > 0 && chanceModifiers.Count > 0)
        {
            var index = rnd.Next(0, chanceModifiers.Count);
            var modifierId = chanceModifiers[index];
            chanceModifierToAssign.Add(modifierId);

            var modifierSelection = getSelectionForRoleId(modifierId);
            while (modifierSelection > 0)
            {
                chanceModifiers.Remove(modifierId);
                modifierSelection--;
            }

            chanceModifierCount--;
        }

        assignModifiersToPlayers(chanceModifierToAssign, players, modifierCount); // Assign chance modifier

        var chanceImpModifierCount = Mathf.Min(modifierCount, chanceImpModifiers.Count);
        var chanceImpModifierToAssign = new List<RoleId>();
        while (chanceImpModifierCount > 0 && chanceImpModifiers.Count > 0)
        {
            var index = rnd.Next(0, chanceImpModifiers.Count);
            var modifierId = chanceImpModifiers[index];
            chanceImpModifierToAssign.Add(modifierId);

            var modifierSelection = getSelectionForRoleId(modifierId);
            while (modifierSelection > 0)
            {
                chanceImpModifiers.Remove(modifierId);
                modifierSelection--;
            }

            chanceImpModifierCount--;
        }

        assignModifiersToPlayers(chanceImpModifierToAssign, impPlayer, modifierCount); // Assign chance Imp modifier
    }

    private static void assignGuesserGamemode()
    {
        var impPlayer = PlayerControl.AllPlayerControls.ToArray().ToList().OrderBy(x => Guid.NewGuid()).ToList();
        var neutralPlayer = PlayerControl.AllPlayerControls.ToArray().ToList().OrderBy(x => Guid.NewGuid()).ToList();
        var crewPlayer = PlayerControl.AllPlayerControls.ToArray().ToList().OrderBy(x => Guid.NewGuid()).ToList();
        impPlayer.RemoveAll(x => !x.Data.Role.IsImpostor);
        neutralPlayer.RemoveAll(x => !isNeutral(x) || x == Doomsayer.doomsayer);
        crewPlayer.RemoveAll(x => x.Data.Role.IsImpostor || isNeutral(x));
        assignGuesserGamemodeToPlayers(crewPlayer,
            Mathf.RoundToInt(CustomOptionHolder.guesserGamemodeCrewNumber.getFloat()));
        assignGuesserGamemodeToPlayers(neutralPlayer,
            Mathf.RoundToInt(CustomOptionHolder.guesserGamemodeNeutralNumber.getFloat()),
            CustomOptionHolder.guesserForceJackalGuesser.getBool(),
            CustomOptionHolder.guesserForceThiefGuesser.getBool(),
            CustomOptionHolder.guesserForcePavlovsGuesser.getBool());
        assignGuesserGamemodeToPlayers(impPlayer,
            Mathf.RoundToInt(CustomOptionHolder.guesserGamemodeImpNumber.getFloat()));
    }

    private static void assignGuesserGamemodeToPlayers(List<PlayerControl> playerList, int count,
        bool forceJackal = false, bool forceThief = false, bool forcePavlovsowner = false)
    {
        var IndexList = new Queue<PlayerControl>();

        if (Jackal.jackal != null && forceJackal)
            IndexList.Enqueue(Jackal.jackal);

        if (Pavlovsdogs.pavlovsowner != null && forcePavlovsowner)
            IndexList.Enqueue(Pavlovsdogs.pavlovsowner);

        if (Thief.thief != null && forceThief)
            IndexList.Enqueue(Thief.thief);

        for (var i = 0; i < count && playerList.Count > 0; i++)
        {
            byte playerId;

            if (IndexList.Count > 0 && IndexList.TryDequeue(out var player))
            {
                playerId = player.PlayerId;
                playerList.Remove(player);
            }
            else
            {
                var player2 = playerList.Random();
                playerId = player2.PlayerId;
                playerList.Remove(player2);
            }

            var writer = AmongUsClient.Instance.StartRpcImmediately(CachedPlayer.LocalPlayer.PlayerControl.NetId,
                (byte)CustomRPC.SetGuesserGm, SendOption.Reliable);
            writer.Write(playerId);
            AmongUsClient.Instance.FinishRpcImmediately(writer);
            RPCProcedure.setGuesserGm(playerId);
        }
    }

    private static byte setRoleToRandomPlayer(byte roleId, List<PlayerControl> playerList, bool removePlayer = true)
    {
        var index = rnd.Next(0, playerList.Count);
        var playerId = playerList[index].PlayerId;
        if (removePlayer) playerList.RemoveAt(index);

        playerRoleMap.Add(new Tuple<byte, byte>(playerId, roleId));

        var writer = AmongUsClient.Instance.StartRpcImmediately(CachedPlayer.LocalPlayer.PlayerControl.NetId,
            (byte)CustomRPC.SetRole, SendOption.Reliable);
        writer.Write(roleId);
        writer.Write(playerId);
        AmongUsClient.Instance.FinishRpcImmediately(writer);
        RPCProcedure.setRole(roleId, playerId);
        return playerId;
    }

    private static byte setModifierToRandomPlayer(byte modifierId, List<PlayerControl> playerList, byte flag = 0)
    {
        if (playerList.Count == 0) return byte.MaxValue;
        var index = rnd.Next(0, playerList.Count);
        var playerId = playerList[index].PlayerId;
        playerList.RemoveAt(index);

        var writer = AmongUsClient.Instance.StartRpcImmediately(CachedPlayer.LocalPlayer.PlayerControl.NetId,
            (byte)CustomRPC.SetModifier, SendOption.Reliable);
        writer.Write(modifierId);
        writer.Write(playerId);
        writer.Write(flag);
        AmongUsClient.Instance.FinishRpcImmediately(writer);
        RPCProcedure.setModifier(modifierId, playerId, flag);
        return playerId;
    }

    private static void assignModifiersToPlayers(List<RoleId> modifiers, List<PlayerControl> playerList, int modifierCount)
    {
        modifiers = modifiers.OrderBy(x => rnd.Next()).ToList(); // randomize list

        while (modifierCount < modifiers.Count)
        {
            var index = rnd.Next(0, modifiers.Count);
            modifiers.RemoveAt(index);
        }

        byte playerId;

        var impPlayer = new List<PlayerControl>(playerList);
        impPlayer.RemoveAll(x => !x.Data.Role.IsImpostor);

        var crewPlayer = new List<PlayerControl>(playerList);
        crewPlayer.RemoveAll(x => x.Data.Role.IsImpostor || isNeutral(x));

        var noImpPlayer = new List<PlayerControl>(playerList);
        noImpPlayer.RemoveAll(x => x.Data.Role.IsImpostor);


        if (modifiers.Contains(RoleId.Assassin))
        {
            var assassinCount = 0;
            while (assassinCount < modifiers.FindAll(x => x == RoleId.Assassin).Count)
            {
                playerId = setModifierToRandomPlayer((byte)RoleId.Assassin, impPlayer);
                playerList.RemoveAll(x => x.PlayerId == playerId);
                assassinCount++;
            }
            modifiers.RemoveAll(x => x == RoleId.Assassin);
        }

        if (modifiers.Contains(RoleId.Disperser))
        {
            playerId = setModifierToRandomPlayer((byte)RoleId.Disperser, impPlayer);
            impPlayer.RemoveAll(x => x.PlayerId == playerId);
            playerList.RemoveAll(x => x.PlayerId == playerId);
            modifiers.RemoveAll(x => x == RoleId.Disperser);
        }

        if (modifiers.Contains(RoleId.Specoality))
        {
            List<PlayerControl> GuesserList = [];

            if (isGuesserGamemode)
            {
                foreach (var player in GuesserGM.guessers)
                {
                    GuesserList.Add(player.guesser);
                    GuesserList.RemoveAll(x => !x.Data.Role.IsImpostor);
                }
            }
            else
            {
                foreach (var player in Assassin.assassin)
                {
                    GuesserList.Add(player);
                }

            }

            playerId = setModifierToRandomPlayer((byte)RoleId.Specoality, GuesserList);
            impPlayer.RemoveAll(x => x.PlayerId == playerId);
            playerList.RemoveAll(x => x.PlayerId == playerId);
            modifiers.RemoveAll(x => x == RoleId.Specoality);
        }

        if (modifiers.Contains(RoleId.PoucherModifier))
        {
            playerId = setModifierToRandomPlayer((byte)RoleId.PoucherModifier, impPlayer);
            impPlayer.RemoveAll(x => x.PlayerId == playerId);
            playerList.RemoveAll(x => x.PlayerId == playerId);
            modifiers.RemoveAll(x => x == RoleId.PoucherModifier);
        }

        if (modifiers.Contains(RoleId.Cursed))
        {
            playerId = setModifierToRandomPlayer((byte)RoleId.Cursed, crewPlayer);
            if (!Cursed.hideModifier) crewPlayer.RemoveAll(x => x.PlayerId == playerId);
            playerList.RemoveAll(x => x.PlayerId == playerId);
            modifiers.RemoveAll(x => x == RoleId.Cursed);
        }

        if (modifiers.Contains(RoleId.Tunneler))
        {
            var TunnelerPlayer = new List<PlayerControl>(crewPlayer);
            TunnelerPlayer.RemoveAll(x => x == Engineer.engineer);
            playerId = setModifierToRandomPlayer((byte)RoleId.Tunneler, TunnelerPlayer);
            crewPlayer.RemoveAll(x => x.PlayerId == playerId);
            playerList.RemoveAll(x => x.PlayerId == playerId);
            modifiers.RemoveAll(x => x == RoleId.Tunneler);
        }

        if (modifiers.Contains(RoleId.Watcher))
        {
            var WatcherPlayer = new List<PlayerControl>(playerList);
            WatcherPlayer.RemoveAll(x => x.Data.Role.IsImpostor || x == Prosecutor.prosecutor);
            playerId = setModifierToRandomPlayer((byte)RoleId.Watcher, WatcherPlayer);
            crewPlayer.RemoveAll(x => x.PlayerId == playerId);
            playerList.RemoveAll(x => x.PlayerId == playerId);
            modifiers.RemoveAll(x => x == RoleId.Watcher);
        }

        if (modifiers.Contains(RoleId.Shifter))
        {
            var shifterCrewPlayer = new List<PlayerControl>(playerList);
            if (Shifter.shiftALLNeutra)
            {
                shifterCrewPlayer.RemoveAll(x => x.Data.Role.IsImpostor
                    || x == Jackal.jackal
                    || x == Sidekick.sidekick
                    || x == Lawyer.lawyer
                    || x == Pavlovsdogs.pavlovsowner);
            }
            else
            {
                shifterCrewPlayer.RemoveAll(x => x.Data.Role.IsImpostor || isNeutral(x));
            }
            playerId = setModifierToRandomPlayer((byte)RoleId.Shifter, shifterCrewPlayer);
            crewPlayer.RemoveAll(x => x.PlayerId == playerId);
            playerList.RemoveAll(x => x.PlayerId == playerId);
            modifiers.RemoveAll(x => x == RoleId.Shifter);
        }

        if (modifiers.Contains(RoleId.Sunglasses))
        {
            var sunglassesCount = 0;
            var sunglassesCrewPlayer = new List<PlayerControl>(crewPlayer);
            sunglassesCrewPlayer.RemoveAll(x => x == Mayor.mayor);
            while (sunglassesCount < modifiers.FindAll(x => x == RoleId.Sunglasses).Count)
            {
                playerId = setModifierToRandomPlayer((byte)RoleId.Sunglasses, sunglassesCrewPlayer);
                crewPlayer.RemoveAll(x => x.PlayerId == playerId);
                sunglassesCrewPlayer.RemoveAll(x => x.PlayerId == playerId);
                playerList.RemoveAll(x => x.PlayerId == playerId);
                sunglassesCount++;
            }

            modifiers.RemoveAll(x => x == RoleId.Sunglasses);
        }

        if (modifiers.Contains(RoleId.Aftermath))
        {
            playerId = setModifierToRandomPlayer((byte)RoleId.Aftermath, noImpPlayer);
            noImpPlayer.RemoveAll(x => x.PlayerId == playerId);
            crewPlayer.RemoveAll(x => x.PlayerId == playerId);
            playerList.RemoveAll(x => x.PlayerId == playerId);
            modifiers.RemoveAll(x => x == RoleId.Aftermath);
        }

        if (Bait.SwapCrewmate && modifiers.Contains(RoleId.Bait))
        {
            playerId = setModifierToRandomPlayer((byte)RoleId.Bait, crewPlayer);
            crewPlayer.RemoveAll(x => x.PlayerId == playerId);
            playerList.RemoveAll(x => x.PlayerId == playerId);
            modifiers.RemoveAll(x => x == RoleId.Bait);
        }

        if (modifiers.Contains(RoleId.Torch))
        {
            var torchCount = 0;
            while (torchCount < modifiers.FindAll(x => x == RoleId.Torch).Count)
            {
                playerId = setModifierToRandomPlayer((byte)RoleId.Torch, crewPlayer);
                crewPlayer.RemoveAll(x => x.PlayerId == playerId);
                playerList.RemoveAll(x => x.PlayerId == playerId);
                torchCount++;
            }

            modifiers.RemoveAll(x => x == RoleId.Torch);
        }

        if (modifiers.Contains(RoleId.ButtonBarry))
        {
            var buttonPlayer = new List<PlayerControl>(playerList);
            buttonPlayer.RemoveAll(x => x == Mayor.mayor);

            playerId = setModifierToRandomPlayer((byte)RoleId.ButtonBarry, buttonPlayer);
            buttonPlayer.RemoveAll(x => x.PlayerId == playerId);
            playerList.RemoveAll(x => x.PlayerId == playerId);
            modifiers.RemoveAll(x => x == RoleId.ButtonBarry);
        }

        if (modifiers.Contains(RoleId.Multitasker))
        {
            var multitaskerCount = 0;
            while (multitaskerCount < modifiers.FindAll(x => x == RoleId.Multitasker).Count)
            {
                playerId = setModifierToRandomPlayer((byte)RoleId.Multitasker, crewPlayer);
                crewPlayer.RemoveAll(x => x.PlayerId == playerId);
                playerList.RemoveAll(x => x.PlayerId == playerId);
                multitaskerCount++;
            }
            modifiers.RemoveAll(x => x == RoleId.Multitasker);
        }

        if (modifiers.Contains(RoleId.Chameleon))
        {
            var chameleonPlayer = new List<PlayerControl>(playerList);
            chameleonPlayer.RemoveAll(x => x == Ninja.ninja);
            int chameleonCount = 0;
            while (chameleonCount < modifiers.FindAll(x => x == RoleId.Chameleon).Count)
            {
                playerId = setModifierToRandomPlayer((byte)RoleId.Chameleon, chameleonPlayer);
                chameleonPlayer.RemoveAll(x => x.PlayerId == playerId);
                crewPlayer.RemoveAll(x => x.PlayerId == playerId);
                playerList.RemoveAll(x => x.PlayerId == playerId);
                chameleonCount++;
            }
            modifiers.RemoveAll(x => x == RoleId.Chameleon);
        }

        foreach (var modifier in modifiers)
        {
            if (playerList.Count == 0) break;
            playerId = setModifierToRandomPlayer((byte)modifier, playerList);
            playerList.RemoveAll(x => x.PlayerId == playerId);
        }
    }

    private static int getSelectionForRoleId(RoleId roleId, bool multiplyQuantity = false)
    {
        var selection = 0;
        switch (roleId)
        {
            case RoleId.Lover:
                selection = CustomOptionHolder.modifierLover.getSelection();
                break;
            case RoleId.Tiebreaker:
                selection = CustomOptionHolder.modifierTieBreaker.getSelection();
                break;
            case RoleId.Indomitable:
                selection = CustomOptionHolder.modifierIndomitable.getSelection();
                break;
            case RoleId.Cursed:
                selection = CustomOptionHolder.modifierCursed.getSelection();
                break;
            case RoleId.Slueth:
                selection = CustomOptionHolder.modifierSlueth.getSelection();
                break;
            case RoleId.Blind:
                selection = CustomOptionHolder.modifierBlind.getSelection();
                break;
            case RoleId.Watcher:
                selection = CustomOptionHolder.modifierWatcher.getSelection();
                break;
            case RoleId.Radar:
                selection = CustomOptionHolder.modifierRadar.getSelection();
                break;
            case RoleId.Disperser:
                selection = CustomOptionHolder.modifierDisperser.getSelection();
                break;
            case RoleId.Specoality:
                selection = CustomOptionHolder.modifierSpecoality.getSelection();
                break;
            case RoleId.PoucherModifier:
                if (Poucher.spawnModifier) selection = CustomOptionHolder.poucherSpawnRate.getSelection();
                break;
            case RoleId.Mini:
                selection = CustomOptionHolder.modifierMini.getSelection();
                break;
            case RoleId.Giant:
                selection = CustomOptionHolder.modifierGiant.getSelection();
                break;
            case RoleId.Aftermath:
                selection = CustomOptionHolder.modifierAftermath.getSelection();
                break;
            case RoleId.Bait:
                selection = CustomOptionHolder.modifierBait.getSelection();
                break;
            case RoleId.Bloody:
                selection = CustomOptionHolder.modifierBloody.getSelection();
                if (multiplyQuantity) selection *= CustomOptionHolder.modifierBloodyQuantity.getQuantity();
                break;
            case RoleId.AntiTeleport:
                if (isFungle) break;
                selection = CustomOptionHolder.modifierAntiTeleport.getSelection();
                if (multiplyQuantity) selection *= CustomOptionHolder.modifierAntiTeleportQuantity.getQuantity();
                break;
            case RoleId.Tunneler:
                selection = CustomOptionHolder.modifierTunneler.getSelection();
                break;
            case RoleId.ButtonBarry:
                selection = CustomOptionHolder.modifierButtonBarry.getSelection();
                break;
            case RoleId.Sunglasses:
                selection = CustomOptionHolder.modifierSunglasses.getSelection();
                if (multiplyQuantity) selection *= CustomOptionHolder.modifierSunglassesQuantity.getQuantity();
                break;
            case RoleId.Torch:
                selection = CustomOptionHolder.modifierTorch.getSelection();
                if (multiplyQuantity) selection *= CustomOptionHolder.modifierTorchQuantity.getQuantity();
                break;
            case RoleId.Flash:
                selection = CustomOptionHolder.modifierFlash.getSelection();
                if (multiplyQuantity) selection *= CustomOptionHolder.modifierFlashQuantity.getQuantity();
                break;
            case RoleId.Multitasker:
                selection = CustomOptionHolder.modifierMultitasker.getSelection();
                if (multiplyQuantity) selection *= CustomOptionHolder.modifierMultitaskerQuantity.getQuantity();
                break;
            case RoleId.Vip:
                selection = CustomOptionHolder.modifierVip.getSelection();
                if (multiplyQuantity) selection *= CustomOptionHolder.modifierVipQuantity.getQuantity();
                break;
            case RoleId.Invert:
                selection = CustomOptionHolder.modifierInvert.getSelection();
                if (multiplyQuantity) selection *= CustomOptionHolder.modifierInvertQuantity.getQuantity();
                break;
            case RoleId.Chameleon:
                selection = CustomOptionHolder.modifierChameleon.getSelection();
                if (multiplyQuantity) selection *= CustomOptionHolder.modifierChameleonQuantity.getQuantity();
                break;
            case RoleId.Shifter:
                selection = CustomOptionHolder.modifierShifter.getSelection();
                break;
            case RoleId.Assassin:
                if (!isGuesserGamemode)
                {
                    selection = CustomOptionHolder.modifierAssassin.getSelection();
                    if (multiplyQuantity)
                        selection *= CustomOptionHolder.modifierAssassinQuantity.getQuantity();
                }
                break;
        }

        return selection;
    }

    private static void setRolesAgain()
    {
        while (playerRoleMap.Any())
        {
            var amount = (byte)Math.Min(playerRoleMap.Count, 20);
            var writer = AmongUsClient.Instance!.StartRpcImmediately(CachedPlayer.LocalPlayer.PlayerControl.NetId,
                (byte)CustomRPC.WorkaroundSetRoles, SendOption.Reliable);
            writer.Write(amount);
            for (var i = 0; i < amount; i++)
            {
                var option = playerRoleMap[0];
                playerRoleMap.RemoveAt(0);
                writer.WritePacked((uint)option.Item1);
                writer.WritePacked((uint)option.Item2);
            }

            AmongUsClient.Instance.FinishRpcImmediately(writer);
        }
    }

    public class RoleAssignmentData
    {
        public Dictionary<byte, int> crewSettings = new();
        public Dictionary<byte, int> impSettings = new();
        public Dictionary<byte, int> neutralSettings = new();
        public Dictionary<byte, int> killerNeutralSettings = new();
        public List<PlayerControl> crewmates { get; set; }
        public List<PlayerControl> impostors { get; set; }
        public int maxCrewmateRoles { get; set; }
        public int maxNeutralRoles { get; set; }
        public int maxKillerNeutralRoles { get; set; }
        public int maxImpostorRoles { get; set; }
    }

    private enum RoleType
    {
        Crewmate = 0,
        Neutral = 1,
        KillerNeutral = 2,
        Impostor = 3
    }
}