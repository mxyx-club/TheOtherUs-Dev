using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TheOtherRoles.CustomGameModes;
using TheOtherRoles.Utilities;
using TMPro;
using UnityEngine;
using Object = UnityEngine.Object;

namespace TheOtherRoles.Patches;

internal enum CustomGameOverReason
{
    Draw = 10,
    LoversWin,
    TeamJackalWin,
    TeamPavlovsWin,
    MiniLose,
    JesterWin,
    ArsonistWin,
    VultureWin,
    LawyerSoloWin,
    ExecutionerWin,
    SwooperWin,
    WerewolfWin,
    JuggernautWin,
    DoomsayerWin,
    AkujoWin,
}

internal enum WinCondition
{
    Draw = -1,
    Default,
    LoversTeamWin,
    LoversSoloWin,
    JesterWin,
    JackalWin,
    PavlovsWin,
    SwooperWin,
    MiniLose,
    ArsonistWin,
    VultureWin,
    LawyerSoloWin,
    AdditionalLawyerBonusWin,
    AdditionalLawyerStolenWin,
    AdditionalAlivePursuerWin,
    AdditionalAliveSurvivorWin,
    ExecutionerWin,
    WerewolfWin,
    JuggernautWin,
    DoomsayerWin,
    AkujoSoloWin,
    EveryoneDied,
    AkujoTeamWin,
}

internal static class AdditionalTempData
{
    // Should be implemented using a proper GameOverReason in the future
    public static WinCondition winCondition = WinCondition.Default;
    public static List<WinCondition> additionalWinConditions = new();
    public static List<PlayerRoleInfo> playerRoles = new();
    public static float timer;

    public static void clear()
    {
        playerRoles.Clear();
        additionalWinConditions.Clear();
        winCondition = WinCondition.Default;
        timer = 0;
    }

    internal class PlayerRoleInfo
    {
        public string PlayerName { get; set; }
        public List<RoleInfo> Roles { get; set; }
        public string RoleNames { get; set; }
        public int TasksCompleted { get; set; }
        public int TasksTotal { get; set; }
        public bool IsGuesser { get; set; }
        public int? Kills { get; set; }
        public bool IsAlive { get; set; }
    }
}

[HarmonyPatch(typeof(AmongUsClient), nameof(AmongUsClient.OnGameEnd))]
public class OnGameEndPatch
{
    private static GameOverReason gameOverReason;

    public static void Prefix(AmongUsClient __instance, [HarmonyArgument(0)] ref EndGameResult endGameResult)
    {
        gameOverReason = endGameResult.GameOverReason;
        if ((int)endGameResult.GameOverReason >= 10) endGameResult.GameOverReason = GameOverReason.ImpostorByKill;

        // Reset zoomed out ghosts
        toggleZoom(true);
    }

    public static void Postfix(AmongUsClient __instance, [HarmonyArgument(0)] ref EndGameResult endGameResult)
    {
        AdditionalTempData.clear();

        foreach (var playerControl in CachedPlayer.AllPlayers)
        {
            var roles = RoleInfo.getRoleInfoForPlayer(playerControl);
            var (tasksCompleted, tasksTotal) = TasksHandler.taskInfo(playerControl.Data);
            var isGuesser = HandleGuesser.isGuesserGm && HandleGuesser.isGuesser(playerControl.PlayerId);
            int? killCount = GameHistory.deadPlayers.FindAll(x => x.killerIfExisting != null && x.killerIfExisting.PlayerId == playerControl.PlayerId).Count;
            if (killCount == 0 && !(new List<RoleInfo>
                          { RoleInfo.sheriff, RoleInfo.jackal, RoleInfo.sidekick, RoleInfo.thief, RoleInfo.juggernaut }
                      .Contains(RoleInfo.getRoleInfoForPlayer(playerControl, false).FirstOrDefault()) ||
                  playerControl.Data.Role.IsImpostor)) killCount = null;
            var roleString = RoleInfo.GetRolesString(playerControl, true);
            AdditionalTempData.playerRoles.Add(new AdditionalTempData.PlayerRoleInfo
            {
                PlayerName = playerControl.Data.PlayerName,
                Roles = roles,
                RoleNames = roleString,
                TasksTotal = tasksTotal,
                TasksCompleted = tasksCompleted,
                IsGuesser = isGuesser,
                Kills = killCount,
                IsAlive = !playerControl.Data.IsDead
            });

            if (Cultist.isCultistGame) GameOptionsManager.Instance.currentNormalGameOptions.NumImpostors = 2;
        }

        // Remove Jester, Arsonist, Vulture, Jackal, former Jackals and Sidekick from winners (if they win, they'll be readded)
        var notWinners = new List<PlayerControl>();
        if (Jester.jester != null) notWinners.Add(Jester.jester);
        if (Sidekick.sidekick != null) notWinners.Add(Sidekick.sidekick);
        if (Amnisiac.amnisiac != null) notWinners.Add(Amnisiac.amnisiac);
        if (Jackal.jackal != null) notWinners.Add(Jackal.jackal);
        if (Arsonist.arsonist != null) notWinners.Add(Arsonist.arsonist);
        if (Swooper.swooper != null) notWinners.Add(Swooper.swooper);
        if (Vulture.vulture != null) notWinners.Add(Vulture.vulture);
        if (Werewolf.werewolf != null) notWinners.Add(Werewolf.werewolf);
        if (Lawyer.lawyer != null) notWinners.Add(Lawyer.lawyer);
        if (Executioner.executioner != null) notWinners.Add(Executioner.executioner);
        if (Thief.thief != null) notWinners.Add(Thief.thief);
        if (Juggernaut.juggernaut != null) notWinners.Add(Juggernaut.juggernaut);
        if (Doomsayer.doomsayer != null) notWinners.Add(Doomsayer.doomsayer);
        if (Akujo.akujo != null) notWinners.Add(Akujo.akujo);
        if (Pavlovsdogs.pavlovsowner != null) notWinners.Add(Pavlovsdogs.pavlovsowner);
        if (Pavlovsdogs.pavlovsdogs != null) notWinners.AddRange(Pavlovsdogs.pavlovsdogs);
        if (Akujo.honmei != null && Akujo.honmeiCannotFollowWin) notWinners.Add(Akujo.honmei);
        if (Pursuer.pursuer != null) notWinners.AddRange(Pursuer.pursuer);
        if (Survivor.survivor != null) notWinners.AddRange(Survivor.survivor);
        notWinners.AddRange(Jackal.formerJackals);

        var winnersToRemove = new List<CachedPlayerData>();
        foreach (var winner in EndGameResult.CachedWinners.GetFastEnumerator())
            if (notWinners.Any(x => x.Data.PlayerName == winner.PlayerName))
                winnersToRemove.Add(winner);
        foreach (var winner in winnersToRemove) EndGameResult.CachedWinners.Remove(winner);
        bool isCanceled = gameOverReason == (GameOverReason)CustomGameOverReason.Draw;
        bool everyoneDead = AdditionalTempData.playerRoles.All(x => !x.IsAlive);
        bool jesterWin = Jester.jester != null && gameOverReason == (GameOverReason)CustomGameOverReason.JesterWin;
        bool werewolfWin = gameOverReason == (GameOverReason)CustomGameOverReason.WerewolfWin &&
                          Werewolf.werewolf != null && !Werewolf.werewolf.Data.IsDead;
        bool juggernautWin = gameOverReason == (GameOverReason)CustomGameOverReason.JuggernautWin &&
                            Juggernaut.juggernaut != null && !Juggernaut.juggernaut.Data.IsDead;
        bool swooperWin = gameOverReason == (GameOverReason)CustomGameOverReason.SwooperWin &&
                            Swooper.swooper != null && !Swooper.swooper.Data.IsDead;
        bool arsonistWin = Arsonist.arsonist != null &&
                          gameOverReason == (GameOverReason)CustomGameOverReason.ArsonistWin;
        bool miniLose = Mini.mini != null && gameOverReason == (GameOverReason)CustomGameOverReason.MiniLose;
        bool doomsayerWin = Doomsayer.doomsayer != null &&
                           gameOverReason == (GameOverReason)CustomGameOverReason.DoomsayerWin;
        // Either they win if they are among the last 3 players, or they win if they are both Crewmates and both alive and the Crew wins (Team Imp/Jackal Lovers can only win solo wins)
        bool loversWin = Lovers.existingAndAlive() &&
                        (gameOverReason == (GameOverReason)CustomGameOverReason.LoversWin ||
                         (GameManager.Instance.DidHumansWin(gameOverReason) &&
                          !Lovers.existingWithKiller()));
        bool teamJackalWin = gameOverReason == (GameOverReason)CustomGameOverReason.TeamJackalWin &&
                            ((Jackal.jackal != null && !Jackal.jackal.Data.IsDead) ||
                            (Sidekick.sidekick != null && !Sidekick.sidekick.Data.IsDead));
        bool teamPavlovsWin = gameOverReason == (GameOverReason)CustomGameOverReason.TeamPavlovsWin &&
                            ((Pavlovsdogs.pavlovsowner != null && !Pavlovsdogs.pavlovsowner.Data.IsDead) ||
                            (Pavlovsdogs.pavlovsdogs != null && Pavlovsdogs.pavlovsdogs.Any(p => !p.Data.IsDead)));
        bool vultureWin = Vulture.vulture != null && gameOverReason == (GameOverReason)CustomGameOverReason.VultureWin;
        bool executionerWin = Executioner.executioner != null && gameOverReason == (GameOverReason)CustomGameOverReason.ExecutionerWin;
        bool lawyerSoloWin = Lawyer.lawyer != null && gameOverReason == (GameOverReason)CustomGameOverReason.LawyerSoloWin;
        bool akujoWin = false;
        if (Akujo.honmeiOptimizeWin)
        {
            akujoWin = (Akujo.akujo != null
                && gameOverReason == (GameOverReason)CustomGameOverReason.AkujoWin
                    && Akujo.honmei != null && !Akujo.honmei.Data.IsDead && !Akujo.akujo.Data.IsDead)
                || (GameManager.Instance.DidHumansWin(gameOverReason)
                && !Akujo.existingWithKiller() && Akujo.honmei != null && !Akujo.honmei.Data.IsDead && !Akujo.akujo.Data.IsDead);
        }
        else
        {
            akujoWin = Akujo.akujo != null && gameOverReason == (GameOverReason)CustomGameOverReason.AkujoWin && Akujo.honmei != null && !Akujo.honmei.Data.IsDead && !Akujo.akujo.Data.IsDead;
        }

        bool isPursurerLose = jesterWin || arsonistWin || miniLose || isCanceled;

        // Mini lose
        if (miniLose)
        {
            EndGameResult.CachedWinners = new Il2CppSystem.Collections.Generic.List<CachedPlayerData>();
            var wpd = new CachedPlayerData(Mini.mini.Data);
            wpd.IsYou = false; // If "no one is the Mini", it will display the Mini, but also show defeat to everyone
            EndGameResult.CachedWinners.Add(wpd);
            AdditionalTempData.winCondition = WinCondition.MiniLose;
        }

        else if (isCanceled)
        {
            EndGameResult.CachedWinners = new Il2CppSystem.Collections.Generic.List<CachedPlayerData>();
            AdditionalTempData.winCondition = WinCondition.Draw;
        }

        // Jester win
        else if (jesterWin)
        {
            EndGameResult.CachedWinners = new Il2CppSystem.Collections.Generic.List<CachedPlayerData>();
            var wpd = new CachedPlayerData(Jester.jester.Data);
            EndGameResult.CachedWinners.Add(wpd);
            AdditionalTempData.winCondition = WinCondition.JesterWin;
        }

        // Arsonist win
        else if (arsonistWin)
        {
            EndGameResult.CachedWinners = new Il2CppSystem.Collections.Generic.List<CachedPlayerData>();
            var wpd = new CachedPlayerData(Arsonist.arsonist.Data);
            EndGameResult.CachedWinners.Add(wpd);
            AdditionalTempData.winCondition = WinCondition.ArsonistWin;
        }

        // Everyone Died
        else if (everyoneDead)
        {
            EndGameResult.CachedWinners = new Il2CppSystem.Collections.Generic.List<CachedPlayerData>();
            AdditionalTempData.winCondition = WinCondition.EveryoneDied;
        }

        // Vulture win
        else if (vultureWin)
        {
            EndGameResult.CachedWinners = new Il2CppSystem.Collections.Generic.List<CachedPlayerData>();
            var wpd = new CachedPlayerData(Vulture.vulture.Data);
            EndGameResult.CachedWinners.Add(wpd);
            AdditionalTempData.winCondition = WinCondition.VultureWin;
        }

        // Jester win
        else if (executionerWin)
        {
            EndGameResult.CachedWinners = new Il2CppSystem.Collections.Generic.List<CachedPlayerData>();
            var wpd = new CachedPlayerData(Executioner.executioner.Data);
            EndGameResult.CachedWinners.Add(wpd);
            AdditionalTempData.winCondition = WinCondition.ExecutionerWin;
        }

        // Lovers win conditions
        else if (loversWin)
        {
            // Double win for lovers, crewmates also win
            if (!Lovers.existingWithKiller())
            {
                AdditionalTempData.winCondition = WinCondition.LoversTeamWin;
                EndGameResult.CachedWinners = new Il2CppSystem.Collections.Generic.List<CachedPlayerData>();
                foreach (PlayerControl p in CachedPlayer.AllPlayers)
                {
                    if (p == null) continue;
                    if (p == Lovers.lover1 || p == Lovers.lover2)
                        EndGameResult.CachedWinners.Add(new CachedPlayerData(p.Data));
                    else if (Pursuer.pursuer.Any(pc => pc == p) && !Pursuer.pursuer.Any(pc => pc.Data.IsDead))
                        EndGameResult.CachedWinners.Add(new CachedPlayerData(p.Data));
                    else if (Survivor.survivor.Any(pc => pc == p) && !Survivor.survivor.Any(pc => pc.Data.IsDead))
                        EndGameResult.CachedWinners.Add(new CachedPlayerData(p.Data));
                    else if (p != Jester.jester && p != Jackal.jackal && p != Werewolf.werewolf &&
                             p != Juggernaut.juggernaut && p != Doomsayer.doomsayer &&
                             p != Sidekick.sidekick && p != Arsonist.arsonist && p != Vulture.vulture &&
                             p != Swooper.swooper &&
                             p != Pavlovsdogs.pavlovsowner &&
                             !Pavlovsdogs.pavlovsdogs.Contains(p) &&
                             p != Akujo.akujo &&
                             !Jackal.formerJackals.Contains(p) && !p.Data.Role.IsImpostor)
                        EndGameResult.CachedWinners.Add(new CachedPlayerData(p.Data));
                }
            }
            // Lovers solo win
            else
            {
                AdditionalTempData.winCondition = WinCondition.LoversSoloWin;
                EndGameResult.CachedWinners = new Il2CppSystem.Collections.Generic.List<CachedPlayerData>();
                EndGameResult.CachedWinners.Add(new CachedPlayerData(Lovers.lover1.Data));
                EndGameResult.CachedWinners.Add(new CachedPlayerData(Lovers.lover2.Data));
            }
        }

        // Jackal win condition (should be implemented using a proper GameOverReason in the future)
        else if (teamJackalWin)
        {
            // Jackal wins if nobody except jackal is alive
            AdditionalTempData.winCondition = WinCondition.JackalWin;
            EndGameResult.CachedWinners = new Il2CppSystem.Collections.Generic.List<CachedPlayerData>();
            var wpd = new CachedPlayerData(Jackal.jackal.Data);
            wpd.IsImpostor = false;
            EndGameResult.CachedWinners.Add(wpd);
            // If there is a sidekick. The sidekick also wins
            if (Sidekick.sidekick != null)
            {
                var wpdSidekick = new CachedPlayerData(Sidekick.sidekick.Data);
                wpdSidekick.IsImpostor = false;
                EndGameResult.CachedWinners.Add(wpdSidekick);
            }

            foreach (var player in Jackal.formerJackals)
            {
                var wpdFormerJackal = new CachedPlayerData(player.Data);
                wpdFormerJackal.IsImpostor = false;
                EndGameResult.CachedWinners.Add(wpdFormerJackal);
            }
        }
        else if (teamPavlovsWin)
        {
            // Jackal wins if nobody except jackal is alive
            AdditionalTempData.winCondition = WinCondition.PavlovsWin;
            EndGameResult.CachedWinners = new Il2CppSystem.Collections.Generic.List<CachedPlayerData>();
            var wpd = new CachedPlayerData(Pavlovsdogs.pavlovsowner.Data);
            wpd.IsImpostor = false;
            EndGameResult.CachedWinners.Add(wpd);

            foreach (var player in Pavlovsdogs.pavlovsdogs)
            {
                var wpdFormerPavlovs = new CachedPlayerData(player.Data);
                wpdFormerPavlovs.IsImpostor = false;
                EndGameResult.CachedWinners.Add(wpdFormerPavlovs);
            }
        }
        else if (werewolfWin)
        {
            // Werewolf wins if nobody except jackal is alive
            AdditionalTempData.winCondition = WinCondition.WerewolfWin;
            EndGameResult.CachedWinners = new Il2CppSystem.Collections.Generic.List<CachedPlayerData>();
            var wpd = new CachedPlayerData(Werewolf.werewolf.Data);
            wpd.IsImpostor = false;
            EndGameResult.CachedWinners.Add(wpd);
        }

        else if (juggernautWin)
        {
            // JuggernautWin wins if nobody except jackal is alive
            AdditionalTempData.winCondition = WinCondition.JuggernautWin;
            EndGameResult.CachedWinners = new Il2CppSystem.Collections.Generic.List<CachedPlayerData>();
            var wpd = new CachedPlayerData(Juggernaut.juggernaut.Data);
            wpd.IsImpostor = false;
            EndGameResult.CachedWinners.Add(wpd);
        }

        else if (doomsayerWin)
        {
            // DoomsayerWin wins if nobody except jackal is alive
            AdditionalTempData.winCondition = WinCondition.DoomsayerWin;
            EndGameResult.CachedWinners = new Il2CppSystem.Collections.Generic.List<CachedPlayerData>();
            var wpd = new CachedPlayerData(Doomsayer.doomsayer.Data);
            EndGameResult.CachedWinners.Add(wpd);
            AdditionalTempData.winCondition = WinCondition.DoomsayerWin;
        }

        //Swooper
        else if (swooperWin)
        {
            // Swooper wins if nobody except jackal is alive
            AdditionalTempData.winCondition = WinCondition.SwooperWin;
            EndGameResult.CachedWinners = new Il2CppSystem.Collections.Generic.List<CachedPlayerData>();
            var wpd = new CachedPlayerData(Swooper.swooper.Data);
            wpd.IsImpostor = false;
            EndGameResult.CachedWinners.Add(wpd);
        }

        // Akujo win
        else if (akujoWin)
        {
            if (Akujo.honmeiOptimizeWin)
            {
                if (!Akujo.existingWithKiller())
                {
                    AdditionalTempData.winCondition = WinCondition.AkujoTeamWin;
                    EndGameResult.CachedWinners = new Il2CppSystem.Collections.Generic.List<CachedPlayerData>();
                    foreach (PlayerControl p in CachedPlayer.AllPlayers)
                    {
                        if (p == null) continue;
                        if (p == Akujo.akujo && p == Akujo.honmei)
                            EndGameResult.CachedWinners.Add(new CachedPlayerData(p.Data));
                        else if (Pursuer.pursuer.Contains(p) && !p.Data.IsDead)
                            EndGameResult.CachedWinners.Add(new CachedPlayerData(p.Data));
                        else if (Survivor.survivor.Contains(p) && !p.Data.IsDead)
                            EndGameResult.CachedWinners.Add(new CachedPlayerData(p.Data));
                        else if (p != Jester.jester && p != Jackal.jackal && p != Werewolf.werewolf &&
                            p != Juggernaut.juggernaut && p != Doomsayer.doomsayer && p != Lawyer.lawyer && !Pursuer.pursuer.Contains(p) &&
                            p != Sidekick.sidekick && p != Arsonist.arsonist && p != Vulture.vulture && p != Amnisiac.amnisiac && p != Thief.thief &&
                            p != Pavlovsdogs.pavlovsowner && !Pavlovsdogs.pavlovsdogs.Contains(p) && !Jackal.formerJackals.Contains(p) && !p.Data.Role.IsImpostor)
                            EndGameResult.CachedWinners.Add(new CachedPlayerData(p.Data));

                    }
                }
                else
                {
                    AdditionalTempData.winCondition = WinCondition.AkujoSoloWin;
                    EndGameResult.CachedWinners = new Il2CppSystem.Collections.Generic.List<CachedPlayerData>();
                    EndGameResult.CachedWinners.Add(new CachedPlayerData(Akujo.akujo.Data));
                    EndGameResult.CachedWinners.Add(new CachedPlayerData(Akujo.honmei.Data));
                }
            }
            else
            {
                AdditionalTempData.winCondition = WinCondition.AkujoSoloWin;
                EndGameResult.CachedWinners = new Il2CppSystem.Collections.Generic.List<CachedPlayerData>();
                EndGameResult.CachedWinners.Add(new CachedPlayerData(Akujo.akujo.Data));
                EndGameResult.CachedWinners.Add(new CachedPlayerData(Akujo.honmei.Data));
            }
        }

        // Lawyer solo win 
        else if (lawyerSoloWin)
        {
            EndGameResult.CachedWinners = new Il2CppSystem.Collections.Generic.List<CachedPlayerData>();
            CachedPlayerData wpd = new(Lawyer.lawyer.Data);
            EndGameResult.CachedWinners.Add(wpd);
            AdditionalTempData.winCondition = WinCondition.LawyerSoloWin;
        }

        // Possible Additional winner: Lawyer
        if (!lawyerSoloWin && Lawyer.lawyer != null && Lawyer.target != null &&
            (!Lawyer.target.Data.IsDead || Lawyer.target == Jester.jester) && !Lawyer.notAckedExiled)
        {
            CachedPlayerData winningClient = null;
            foreach (var winner in EndGameResult.CachedWinners.GetFastEnumerator())
                if (winner.PlayerName == Lawyer.target.Data.PlayerName)
                    winningClient = winner;
            if (winningClient != null)
            {
                if (!EndGameResult.CachedWinners.ToArray().Any(x => x.PlayerName == Lawyer.lawyer.Data.PlayerName))
                {
                    if (!Lawyer.lawyer.Data.IsDead && Lawyer.stolenWin)
                    {
                        EndGameResult.CachedWinners.Remove(winningClient);
                        EndGameResult.CachedWinners.Add(new CachedPlayerData(Lawyer.lawyer.Data));
                        AdditionalTempData.additionalWinConditions.Add(WinCondition.AdditionalLawyerStolenWin); // The Lawyer replaces the client's victory
                    }
                    else
                    {
                        EndGameResult.CachedWinners.Add(new CachedPlayerData(Lawyer.lawyer.Data));
                        AdditionalTempData.additionalWinConditions.Add(WinCondition.AdditionalLawyerBonusWin); // The Lawyer wins with the client
                    }
                }
            }
        }

        // Possible Additional winner: Pursuer
        if (Pursuer.pursuer != null && Pursuer.pursuer.Any(p => !p.Data.IsDead) && !Lawyer.notAckedExiled && !isPursurerLose)
        {
            foreach (var player in Pursuer.pursuer.Where(p => !p.Data.IsDead))
            {
                if (!EndGameResult.CachedWinners.ToArray().Any(x => x.PlayerName == player.Data.PlayerName))
                    EndGameResult.CachedWinners.Add(new CachedPlayerData(player.Data));
            }
            AdditionalTempData.additionalWinConditions.Add(WinCondition.AdditionalAlivePursuerWin);
            /*
            if (!EndGameResult.CachedWinners.ToArray().Any(x => x.PlayerName == Pursuer.pursuer.Data.PlayerName))
                EndGameResult.CachedWinners.Add(new CachedPlayerData(Pursuer.pursuer.Data));*/
        }

        // Possible Additional winner: Survivor
        if (Survivor.survivor != null && Survivor.survivor.Any(p => !p.Data.IsDead) && !isPursurerLose)
        {
            foreach (var player in Survivor.survivor.Where(p => !p.Data.IsDead))
            {
                if (!EndGameResult.CachedWinners.ToArray().Any(x => x.PlayerName == player.Data.PlayerName))
                    EndGameResult.CachedWinners.Add(new CachedPlayerData(player.Data));
            }
            AdditionalTempData.additionalWinConditions.Add(WinCondition.AdditionalAliveSurvivorWin);
            /*
            if (!EndGameResult.CachedWinners.ToArray().Any(x => x.PlayerName == Survivor.survivor.Data.PlayerName))
                EndGameResult.CachedWinners.Add(new CachedPlayerData(Survivor.survivor.Data));*/
        }

        AdditionalTempData.timer = (float)(DateTime.UtcNow -
            (HideNSeek.isHideNSeekGM ? HideNSeek.startTime : PropHunt.startTime)).TotalMilliseconds / 1000;

        // Reset Settings
        if (MapOption.gameMode == CustomGamemodes.HideNSeek) ShipStatusPatch.resetVanillaSettings();
        RPCProcedure.resetVariables();
    }
}

[HarmonyPatch(typeof(EndGameManager), nameof(EndGameManager.SetEverythingUp))]
public class EndGameManagerSetUpPatch
{
    public static void Postfix(EndGameManager __instance)
    {
        // Delete and readd PoolablePlayers always showing the name and role of the player
        foreach (var pb in __instance.transform.GetComponentsInChildren<PoolablePlayer>())
            Object.Destroy(pb.gameObject);
        var num = Mathf.CeilToInt(7.5f);
        var list = EndGameResult.CachedWinners.ToArray().ToList().OrderBy(delegate (CachedPlayerData b)
        {
            return !b.IsYou ? 0 : -1;
        }).ToList();
        for (var i = 0; i < list.Count; i++)
        {
            var CachedPlayerData2 = list[i];
            var num2 = i % 2 == 0 ? -1 : 1;
            var num3 = (i + 1) / 2;
            var num4 = num3 / (float)num;
            var num5 = Mathf.Lerp(1f, 0.75f, num4);
            float num6 = i == 0 ? -8 : -1;
            var poolablePlayer = Object.Instantiate(__instance.PlayerPrefab, __instance.transform);
            poolablePlayer.transform.localPosition = new Vector3(1f * num2 * num3 * num5,
                FloatRange.SpreadToEdges(-1.125f, 0f, num3, num), num6 + (num3 * 0.01f)) * 0.9f;
            var num7 = Mathf.Lerp(1f, 0.65f, num4) * 0.9f;
            var vector = new Vector3(num7, num7, 1f);
            poolablePlayer.transform.localScale = vector;
            if (CachedPlayerData2.IsDead)
            {
                poolablePlayer.SetBodyAsGhost();
                poolablePlayer.SetDeadFlipX(i % 2 == 0);
            }
            else
            {
                poolablePlayer.SetFlipX(i % 2 == 0);
            }

            poolablePlayer.UpdateFromPlayerOutfit(CachedPlayerData2.Outfit, PlayerMaterial.MaskType.None, CachedPlayerData2.IsDead, true);

            poolablePlayer.cosmetics.nameText.color = Color.white;
            poolablePlayer.cosmetics.nameText.transform.localScale =
                new Vector3(1f / vector.x, 1f / vector.y, 1f / vector.z);
            var localPosition = poolablePlayer.cosmetics.nameText.transform.localPosition;
            localPosition = new Vector3(
                localPosition.x,
                localPosition.y, -15f);
            poolablePlayer.cosmetics.nameText.transform.localPosition = localPosition;
            poolablePlayer.cosmetics.nameText.text = CachedPlayerData2.PlayerName;

            foreach (var roles in from data in AdditionalTempData.playerRoles
                                  where data.PlayerName == CachedPlayerData2.PlayerName
                                  select poolablePlayer.cosmetics.nameText.text +=
                         $"\n{string.Join("\n", data.Roles.Select(x => cs(x.color, x.name)))}")
            {
            }
        }

        // Additional code
        var bonusText = Object.Instantiate(__instance.WinText.gameObject);
        var position1 = __instance.WinText.transform.position;
        bonusText.transform.position = new Vector3(position1.x,
            position1.y - 0.5f, position1.z);
        bonusText.transform.localScale = new Vector3(0.7f, 0.7f, 1f);
        var textRenderer = bonusText.GetComponent<TMP_Text>();
        textRenderer.text = "";
        Message("游戏结束");
        switch (AdditionalTempData.winCondition)
        {
            case WinCondition.Draw:
                textRenderer.text = "房主强制结束游戏";
                textRenderer.color = Color.gray;
                __instance.BackgroundBar.material.SetColor("_Color", Color.gray);
                break;
            case WinCondition.EveryoneDied:
                textRenderer.text = "无人生还";
                textRenderer.color = Palette.DisabledGrey;
                __instance.BackgroundBar.material.SetColor("_Color", Palette.DisabledGrey);
                break;
            case WinCondition.JesterWin:
                textRenderer.text = "听我说谢谢你";
                textRenderer.color = Jester.color;
                __instance.BackgroundBar.material.SetColor("_Color", Jester.color);
                break;
            case WinCondition.DoomsayerWin:
                textRenderer.text = "末日预言家获胜";
                textRenderer.color = Doomsayer.color;
                __instance.BackgroundBar.material.SetColor("_Color", Doomsayer.color);
                break;
            case WinCondition.ArsonistWin:
                textRenderer.text = "用火焰净化一切";
                textRenderer.color = Arsonist.color;
                __instance.BackgroundBar.material.SetColor("_Color", Arsonist.color);
                break;
            case WinCondition.VultureWin:
                textRenderer.text = "吃饱饱！";
                textRenderer.color = Vulture.color;
                __instance.BackgroundBar.material.SetColor("_Color", Vulture.color);
                break;
            case WinCondition.LawyerSoloWin:
                textRenderer.text = "律师获胜";
                textRenderer.color = Lawyer.color;
                __instance.BackgroundBar.material.SetColor("_Color", Lawyer.color);
                break;
            case WinCondition.WerewolfWin:
                textRenderer.text = "月下狼人获胜！";
                textRenderer.color = Werewolf.color;
                __instance.BackgroundBar.material.SetColor("_Color", Werewolf.color);
                break;
            case WinCondition.JuggernautWin:
                textRenderer.text = "天启获胜";
                textRenderer.color = Juggernaut.color;
                __instance.BackgroundBar.material.SetColor("_Color", Juggernaut.color);
                break;
            case WinCondition.SwooperWin:
                textRenderer.text = "隐身人获胜!";
                textRenderer.color = Swooper.color;
                __instance.BackgroundBar.material.SetColor("_Color", Swooper.color);
                break;
            case WinCondition.ExecutionerWin:
                textRenderer.text = "小嘴叭叭!";
                textRenderer.color = Executioner.color;
                __instance.BackgroundBar.material.SetColor("_Color", Executioner.color);
                break;
            case WinCondition.LoversTeamWin:
                textRenderer.text = "船员和恋人获胜";
                textRenderer.color = Lovers.color;
                __instance.BackgroundBar.material.SetColor("_Color", Lovers.color);
                break;
            case WinCondition.LoversSoloWin:
                textRenderer.text = "与你的爱恋心意合一~";
                textRenderer.color = Lovers.color;
                __instance.BackgroundBar.material.SetColor("_Color", Lovers.color);
                break;
            case WinCondition.JackalWin:
                textRenderer.text = "豺狼的全家福.jpg";
                textRenderer.color = Jackal.color;
                __instance.BackgroundBar.material.SetColor("_Color", Jackal.color);
                break;
            case WinCondition.PavlovsWin:
                textRenderer.text = "这是乖狗狗的奖励哦";
                textRenderer.color = Pavlovsdogs.color;
                __instance.BackgroundBar.material.SetColor("_Color", Pavlovsdogs.color);
                break;
            case WinCondition.AkujoSoloWin:
                textRenderer.text = "请给我扭曲你人生的权利！";
                textRenderer.color = Akujo.color;
                __instance.BackgroundBar.material.SetColor("_Color", Akujo.color);
                break;
            case WinCondition.AkujoTeamWin:
                textRenderer.text = "我只是加入你们不是拆散你们！";
                textRenderer.color = Akujo.color;
                __instance.BackgroundBar.material.SetColor("_Color", Akujo.color);
                break;
            case WinCondition.MiniLose:
                textRenderer.text = "他就只是个孩子啊！";
                textRenderer.color = Mini.color;
                break;
        }

        var winConditionsTexts = new List<string>();
        var pursuerAlive = false;
        var survivorAlive = false;
        foreach (var cond in AdditionalTempData.additionalWinConditions)
        {
            switch (cond)
            {
                case WinCondition.AdditionalLawyerStolenWin:
                    winConditionsTexts.Add(cs(Lawyer.color, "律师代替客户胜利"));
                    break;
                case WinCondition.AdditionalLawyerBonusWin:
                    winConditionsTexts.Add(cs(Lawyer.color, "律师和客户胜利"));
                    break;
                case WinCondition.AdditionalAlivePursuerWin:
                    pursuerAlive = true;
                    break;
                case WinCondition.AdditionalAliveSurvivorWin:
                    survivorAlive = true;
                    break;
            }
        }

        if (pursuerAlive && survivorAlive)
        {
            winConditionsTexts.Add($"{cs(Pursuer.color, "起诉人")} & {cs(Survivor.color, "幸存者存活")}");
        }
        else
        {
            if (pursuerAlive) winConditionsTexts.Add(cs(Pursuer.color, "起诉人存活"));
            if (survivorAlive) winConditionsTexts.Add(cs(Survivor.color, "幸存者存活"));
        }

        if (winConditionsTexts.Count == 1)
        {
            textRenderer.text += $"\n{winConditionsTexts[0]}";
        }
        else if (winConditionsTexts.Count > 1)
        {
            var combinedText = string.Join(" & ", winConditionsTexts);
            textRenderer.text += $"\n{combinedText}";
        }

        if (MapOption.showRoleSummary || HideNSeek.isHideNSeekGM || PropHunt.isPropHuntGM)
        {
            if (Camera.main != null)
            {
                var position = Camera.main.ViewportToWorldPoint(new Vector3(0f, 1f, Camera.main.nearClipPlane));
                var roleSummary = Object.Instantiate(__instance.WinText.gameObject);
                roleSummary.transform.position = new Vector3(__instance.Navigation.ExitButton.transform.position.x + 0.1f,
                    position.y - 0.1f, -214f);
                roleSummary.transform.localScale = new Vector3(1f, 1f, 1f);

                var roleSummaryText = new StringBuilder();
                if (HideNSeek.isHideNSeekGM || PropHunt.isPropHuntGM)
                {
                    var minutes = (int)AdditionalTempData.timer / 60;
                    var seconds = (int)AdditionalTempData.timer % 60;
                    roleSummaryText.AppendLine($"<color=#FAD934FF>剩余时间: {minutes:00}:{seconds:00}</color> \n");
                }

                roleSummaryText.AppendLine("游戏总结:");
                foreach (var data in AdditionalTempData.playerRoles)
                {
                    //var roles = string.Join(" ", data.Roles.Select(x => Helpers.cs(x.color, x.name)));
                    var roles = data.RoleNames;
                    //if (data.IsGuesser) roles += " (Guesser)";
                    var taskInfo = data.TasksTotal > 0
                        ? $" - <color=#FAD934FF>({data.TasksCompleted}/{data.TasksTotal})</color>"
                        : "";
                    if (data.Kills != null) taskInfo += $" - <color=#FF0000FF>(击杀: {data.Kills})</color>";
                    roleSummaryText.AppendLine(
                        $"{cs(data.IsAlive ? Color.white : new Color(.7f, .7f, .7f), data.PlayerName)} - {roles}{taskInfo}");
                }

                var roleSummaryTextMesh = roleSummary.GetComponent<TMP_Text>();
                roleSummaryTextMesh.alignment = TextAlignmentOptions.TopLeft;
                roleSummaryTextMesh.color = Color.white;
                roleSummaryTextMesh.fontSizeMin = 1.5f;
                roleSummaryTextMesh.fontSizeMax = 1.5f;
                roleSummaryTextMesh.fontSize = 1.5f;

                var roleSummaryTextMeshRectTransform = roleSummaryTextMesh.GetComponent<RectTransform>();
                roleSummaryTextMeshRectTransform.anchoredPosition = new Vector2(position.x + 3.5f, position.y - 0.1f);
                roleSummaryTextMesh.text = roleSummaryText.ToString();
            }
        }

        AdditionalTempData.clear();
    }
}

[HarmonyPatch(typeof(LogicGameFlowNormal), nameof(LogicGameFlowNormal.CheckEndCriteria))]
internal class CheckEndCriteriaPatch
{
    public static bool Prefix(ShipStatus __instance)
    {
        if (!GameData.Instance) return false;
        // InstanceExists | Don't check Custom Criteria when in Tutorial
        if (DestroyableSingleton<TutorialManager>.InstanceExists) return true;
        var statistics = new PlayerStatistics(__instance);
        if (CheckAndEndGameForHost(__instance)) return false;
        if (MapOption.DebugMode) return false;
        if (CheckAndEndGameForTaskWin(__instance)) return false;
        if (CheckAndEndGameForMiniLose(__instance)) return false;
        if (CheckAndEndGameForJesterWin(__instance)) return false;
        if (CheckAndEndGameForDoomsayerWin(__instance)) return false;
        if (CheckAndEndGameForArsonistWin(__instance)) return false;
        if (CheckAndEndGameForVultureWin(__instance)) return false;
        if (CheckAndEndGameForSabotageWin(__instance)) return false;
        if (CheckAndEndGameForExecutionerWin(__instance)) return false;
        if (CheckAndEndGameForAkujoWin(__instance, statistics)) return false;
        if (CheckAndEndGameForWerewolfWin(__instance, statistics)) return false;
        if (CheckAndEndGameForLoverWin(__instance, statistics)) return false;
        if (CheckAndEndGameForJackalWin(__instance, statistics)) return false;
        if (CheckAndEndGameForPavlovsWin(__instance, statistics)) return false;
        if (CheckAndEndGameForSwooperWin(__instance, statistics)) return false;
        if (CheckAndEndGameForJuggernautWin(__instance, statistics)) return false;
        if (CheckAndEndGameForImpostorWin(__instance, statistics)) return false;
        if (CheckAndEndGameForCrewmateWin(__instance, statistics)) return false;
        return false;
    }

    private static bool CheckAndEndGameForHost(ShipStatus __instance)
    {
        if (MapOption.isCanceled)
        {
            GameManager.Instance.RpcEndGame((GameOverReason)CustomGameOverReason.Draw, false);
            return true;
        }
        return false;
    }

    private static bool CheckAndEndGameForMiniLose(ShipStatus __instance)
    {
        if (!Mini.triggerMiniLose) return false;
        //__instance.enabled = false;
        GameManager.Instance.RpcEndGame((GameOverReason)CustomGameOverReason.MiniLose, false);
        return true;
    }

    private static bool CheckAndEndGameForJesterWin(ShipStatus __instance)
    {
        if (Jester.triggerJesterWin)
        {
            //__instance.enabled = false;
            GameManager.Instance.RpcEndGame((GameOverReason)CustomGameOverReason.JesterWin, false);
            return true;
        }
        return false;
    }

    private static bool CheckAndEndGameForDoomsayerWin(ShipStatus __instance)
    {
        if (Doomsayer.triggerDoomsayerrWin)
        {
            //__instance.enabled = false;
            GameManager.Instance.RpcEndGame((GameOverReason)CustomGameOverReason.DoomsayerWin, false);
            return true;
        }
        return false;
    }

    private static bool CheckAndEndGameForArsonistWin(ShipStatus __instance)
    {
        if (Arsonist.triggerArsonistWin)
        {
            //__instance.enabled = false;
            GameManager.Instance.RpcEndGame((GameOverReason)CustomGameOverReason.ArsonistWin, false);
            return true;
        }
        return false;
    }

    private static bool CheckAndEndGameForVultureWin(ShipStatus __instance)
    {
        if (Vulture.triggerVultureWin)
        {
            //__instance.enabled = false;
            GameManager.Instance.RpcEndGame((GameOverReason)CustomGameOverReason.VultureWin, false);
            return true;
        }
        return false;
    }

    private static bool CheckAndEndGameForSabotageWin(ShipStatus __instance)
    {
        if (MapUtilities.Systems == null) return false;
        var systemType = MapUtilities.Systems.ContainsKey(SystemTypes.LifeSupp)
            ? MapUtilities.Systems[SystemTypes.LifeSupp]
            : null;
        if (systemType != null)
        {
            var lifeSuppSystemType = systemType.TryCast<LifeSuppSystemType>();
            if (lifeSuppSystemType != null && lifeSuppSystemType.Countdown < 0f)
            {
                EndGameForSabotage(__instance);
                lifeSuppSystemType.Countdown = 10000f;
                return true;
            }
        }

        var systemType2 = MapUtilities.Systems.ContainsKey(SystemTypes.Reactor)
            ? MapUtilities.Systems[SystemTypes.Reactor]
            : null;
        systemType2 ??= MapUtilities.Systems.ContainsKey(SystemTypes.Laboratory)
                ? MapUtilities.Systems[SystemTypes.Laboratory]
                : null;
        if (systemType2 != null)
        {
            var criticalSystem = systemType2.TryCast<ICriticalSabotage>();
            if (criticalSystem != null && criticalSystem.Countdown < 0f)
            {
                EndGameForSabotage(__instance);
                criticalSystem.ClearSabotage();
                return true;
            }
        }

        return false;
    }

    private static bool CheckAndEndGameForTaskWin(ShipStatus __instance)
    {
        if (MapOption.PreventTaskEnd || (HideNSeek.isHideNSeekGM && !HideNSeek.taskWinPossible) || PropHunt.isPropHuntGM) return false;
        if (GameData.Instance.TotalTasks > 0 && GameData.Instance.TotalTasks <= GameData.Instance.CompletedTasks)
        {
            //__instance.enabled = false;
            GameManager.Instance.RpcEndGame(GameOverReason.HumansByTask, false);
            return true;
        }

        return false;
    }

    private static bool CheckAndEndGameForExecutionerWin(ShipStatus __instance)
    {
        if (Executioner.triggerExecutionerWin)
        {
            //__instance.enabled = false;
            GameManager.Instance.RpcEndGame((GameOverReason)CustomGameOverReason.ExecutionerWin, false);
            return true;
        }

        return false;
    }

    private static bool CheckAndEndGameForLoverWin(ShipStatus __instance, PlayerStatistics statistics)
    {
        if (statistics.TeamLoversAlive == 2 && statistics.TotalAlive <= 3)
        {
            //__instance.enabled = false;
            GameManager.Instance.RpcEndGame((GameOverReason)CustomGameOverReason.LoversWin, false);
            return true;
        }

        return false;
    }

    private static bool CheckAndEndGameForAkujoWin(ShipStatus __instance, PlayerStatistics statistics)
    {
        if (statistics.TeamAkujoAlive == 2 && statistics.TotalAlive <= 3)
        {
            GameManager.Instance.RpcEndGame((GameOverReason)CustomGameOverReason.AkujoWin, false);
            return true;
        }
        return false;
    }
    private static bool CheckAndEndGameForJackalWin(ShipStatus __instance, PlayerStatistics statistics)
    {
        if (statistics.TeamJackalAlive >= statistics.TotalAlive - statistics.TeamJackalAlive &&
            statistics.TeamImpostorsAlive == 0 &&
            statistics.TeamJuggernautAlive == 0 &&
            statistics.TeamPavlovsAlive == 0 &&
            statistics.TeamWerewolfAlive == 0 &&
            statistics.TeamAkujoAlive == 0 &&
            statistics.TeamSwooperAlive == 0 &&
            !(statistics.TeamJackalHasAliveLover &&
              statistics.TeamLoversAlive == 2) && !killingCrewAlive())
        {
            //__instance.enabled = false;
            GameManager.Instance.RpcEndGame((GameOverReason)CustomGameOverReason.TeamJackalWin, false);
            return true;
        }

        return false;
    }
    private static bool CheckAndEndGameForPavlovsWin(ShipStatus __instance, PlayerStatistics statistics)
    {
        if (statistics.TeamPavlovsAlive >= statistics.TotalAlive - statistics.TeamPavlovsAlive &&
            statistics.TeamImpostorsAlive == 0 &&
            statistics.TeamJackalAlive == 0 &&
            statistics.TeamJuggernautAlive == 0 &&
            statistics.TeamWerewolfAlive == 0 &&
            statistics.TeamAkujoAlive == 0 &&
            statistics.TeamSwooperAlive == 0 &&
            !(statistics.TeamPavlovsHasAliveLover && statistics.TeamLoversAlive == 2) && !killingCrewAlive())
        {
            //__instance.enabled = false;
            GameManager.Instance.RpcEndGame((GameOverReason)CustomGameOverReason.TeamPavlovsWin, false);
            return true;
        }

        return false;
    }
    private static bool CheckAndEndGameForSwooperWin(ShipStatus __instance, PlayerStatistics statistics)
    {
        if (statistics.TeamSwooperAlive >= statistics.TotalAlive - statistics.TeamSwooperAlive &&
            statistics.TeamImpostorsAlive == 0 &&
            statistics.TeamJuggernautAlive == 0 &&
            statistics.TeamJackalAlive == 0 &&
            statistics.TeamPavlovsAlive == 0 &&
            statistics.TeamWerewolfAlive == 0 &&
            !(statistics.TeamSwooperHasAliveLover &&
            statistics.TeamLoversAlive == 2) &&
            !killingCrewAlive())
        {
            //__instance.enabled = false;
            GameManager.Instance.RpcEndGame((GameOverReason)CustomGameOverReason.SwooperWin, false);
            return true;
        }
        return false;
    }
    private static bool CheckAndEndGameForWerewolfWin(ShipStatus __instance, PlayerStatistics statistics)
    {
        if (
            statistics.TeamWerewolfAlive >= statistics.TotalAlive - statistics.TeamWerewolfAlive &&
            statistics.TeamImpostorsAlive == 0 &&
            statistics.TeamJuggernautAlive == 0 &&
            statistics.TeamJackalAlive == 0 &&
            statistics.TeamPavlovsAlive == 0 &&
            statistics.TeamSwooperAlive == 0 &&
            !(statistics.TeamWerewolfHasAliveLover &&
              statistics.TeamLoversAlive == 2) &&
            !killingCrewAlive()
        )
        {
            //__instance.enabled = false;
            GameManager.Instance.RpcEndGame((GameOverReason)CustomGameOverReason.WerewolfWin, false);
            return true;
        }

        return false;
    }

    private static bool CheckAndEndGameForJuggernautWin(ShipStatus __instance, PlayerStatistics statistics)
    {
        if (
            statistics.TeamJuggernautAlive >= statistics.TotalAlive - statistics.TeamJuggernautAlive &&
            statistics.TeamImpostorsAlive == 0 &&
            statistics.TeamJackalAlive == 0 &&
            statistics.TeamPavlovsAlive == 0 &&
            statistics.TeamWerewolfAlive == 0 &&
            statistics.TeamSwooperAlive == 0 &&
            !(statistics.TeamJuggernautHasAliveLover &&
              statistics.TeamLoversAlive == 2) &&
            !killingCrewAlive()
        )
        {
            //__instance.enabled = false;
            GameManager.Instance.RpcEndGame((GameOverReason)CustomGameOverReason.JuggernautWin, false);
            return true;
        }

        return false;
    }

    private static bool CheckAndEndGameForImpostorWin(ShipStatus __instance, PlayerStatistics statistics)
    {
        if (HideNSeek.isHideNSeekGM || PropHunt.isPropHuntGM)
            if (0 != statistics.TotalAlive - statistics.TeamImpostorsAlive)
                return false;

        if (statistics.TeamImpostorsAlive >= statistics.TotalAlive - statistics.TeamImpostorsAlive &&
            statistics.TeamJackalAlive == 0 &&
            statistics.TeamPavlovsAlive == 0 &&
            statistics.TeamWerewolfAlive == 0 &&
            statistics.TeamSwooperAlive == 0 &&
            statistics.TeamAkujoAlive == 0 &&
            statistics.TeamJuggernautAlive == 0 &&
            !(statistics.TeamImpostorHasAliveLover && statistics.TeamLoversAlive == 2) && !killingCrewAlive())
        {
            //__instance.enabled = false;
            GameOverReason endReason;
            switch (GameData.LastDeathReason)
            {
                case DeathReason.Exile:
                    endReason = GameOverReason.ImpostorByVote;
                    break;
                case DeathReason.Kill:
                    endReason = GameOverReason.ImpostorByKill;
                    break;
                default:
                    endReason = GameOverReason.ImpostorByVote;
                    break;
            }

            GameManager.Instance.RpcEndGame(endReason, false);
            return true;
        }

        return false;
    }

    private static bool CheckAndEndGameForCrewmateWin(ShipStatus __instance, PlayerStatistics statistics)
    {
        if (HideNSeek.isHideNSeekGM && HideNSeek.timer <= 0 && !HideNSeek.isWaitingTimer)
        {
            //__instance.enabled = false;
            GameManager.Instance.RpcEndGame(GameOverReason.HumansByVote, false);
            return true;
        }
        if (PropHunt.isPropHuntGM && PropHunt.timer <= 0 && PropHunt.timerRunning)
        {
            GameManager.Instance.RpcEndGame(GameOverReason.HumansByVote, false);
            return true;
        }
        if (statistics.TeamImpostorsAlive == 0 &&
            statistics.TeamJackalAlive == 0 &&
            statistics.TeamPavlovsAlive == 0 &&
            statistics.TeamWerewolfAlive == 0 &&
            statistics.TeamSwooperAlive == 0 &&
            statistics.TeamJuggernautAlive == 0)
        {
            if (Akujo.honmeiOptimizeWin)
            {
                GameManager.Instance.RpcEndGame(GameOverReason.HumansByVote, false);
                return true;
            }
            else if (statistics.TeamAkujoAlive <= 1)
            {
                GameManager.Instance.RpcEndGame(GameOverReason.HumansByVote, false);
                return true;
            }
            return true;
        }
        return false;
    }

    private static void EndGameForSabotage(ShipStatus __instance)
    {
        //__instance.enabled = false;
        GameManager.Instance.RpcEndGame(GameOverReason.ImpostorBySabotage, false);
    }
}

internal class PlayerStatistics
{
    public PlayerStatistics(ShipStatus __instance)
    {
        GetPlayerCounts();
    }

    public int TeamImpostorsAlive { get; set; }
    public int TeamJackalAlive { get; set; }
    public int TeamPavlovsAlive { get; set; }
    public int TeamLoversAlive { get; set; }
    public int TotalAlive { get; set; }
    public int TeamSwooperAlive { get; set; }
    public bool TeamImpostorHasAliveLover { get; set; }
    public bool TeamJackalHasAliveLover { get; set; }
    public bool TeamPavlovsHasAliveLover { get; set; }
    public int TeamWerewolfAlive { get; set; }
    public int TeamAkujoAlive { get; set; }
    public bool TeamSwooperHasAliveLover { get; set; }
    public bool TeamWerewolfHasAliveLover { get; set; }
    public int TeamJuggernautAlive { get; set; }
    public bool TeamJuggernautHasAliveLover { get; set; }

    private static bool isLover(NetworkedPlayerInfo p)
    {
        return (Lovers.lover1 != null && Lovers.lover1.PlayerId == p.PlayerId) ||
               (Lovers.lover2 != null && Lovers.lover2.PlayerId == p.PlayerId);
    }

    private void GetPlayerCounts()
    {
        var numJackalAlive = 0;
        var numPavlovsAlive = 0;
        var numImpostorsAlive = 0;
        var numLoversAlive = 0;
        var numTotalAlive = 0;
        var numSwooperAlive = 0;
        var numWerewolfAlive = 0;
        var numJuggernautAlive = 0;
        var numAkujoAlive = 0;
        var impLover = false;
        var jackalLover = false;
        var pavlovsLover = false;
        var swooperLover = false;
        var werewolfLover = false;
        var juggernautLover = false;

        foreach (var playerInfo in GameData.Instance.AllPlayers.GetFastEnumerator())
            if (!playerInfo.Disconnected)
                if (!playerInfo.IsDead)
                {
                    numTotalAlive++;

                    var lover = isLover(playerInfo);
                    if (lover) numLoversAlive++;

                    if (playerInfo.Role.IsImpostor)
                    {
                        numImpostorsAlive++;
                        if (lover) impLover = true;
                    }

                    if (Jackal.jackal != null && Jackal.jackal.PlayerId == playerInfo.PlayerId)
                    {
                        numJackalAlive++;
                        if (lover) jackalLover = true;
                    }

                    if (Sidekick.sidekick != null && Sidekick.sidekick.PlayerId == playerInfo.PlayerId)
                    {
                        numJackalAlive++;
                        if (lover) jackalLover = true;
                    }

                    if (Pavlovsdogs.pavlovsowner != null && Pavlovsdogs.pavlovsowner.PlayerId == playerInfo.PlayerId)
                    {
                        numPavlovsAlive++;
                        if (lover) pavlovsLover = true;
                    }

                    if (Pavlovsdogs.pavlovsdogs != null && Pavlovsdogs.pavlovsdogs.Any(p => p.PlayerId == playerInfo.PlayerId))
                    {
                        numPavlovsAlive++;
                        if (lover) pavlovsLover = true;
                    }

                    if (Werewolf.werewolf != null && Werewolf.werewolf.PlayerId == playerInfo.PlayerId)
                    {
                        numWerewolfAlive++;
                        if (lover) werewolfLover = true;
                    }
                    if (Swooper.swooper != null && Swooper.swooper.PlayerId == playerInfo.PlayerId)
                    {
                        numSwooperAlive++;
                        if (lover) swooperLover = true;
                    }
                    if (Juggernaut.juggernaut != null && Juggernaut.juggernaut.PlayerId == playerInfo.PlayerId)
                    {
                        numJuggernautAlive++;
                        if (lover) juggernautLover = true;
                    }
                    if (Akujo.akujo != null && Akujo.akujo.PlayerId == playerInfo.PlayerId)
                    {
                        numAkujoAlive++;
                    }
                    if (Akujo.honmei != null && Akujo.honmei.PlayerId == playerInfo.PlayerId)
                    {
                        numAkujoAlive++;
                    }
                }

        TeamJackalAlive = numJackalAlive;
        TeamImpostorsAlive = numImpostorsAlive;
        TeamLoversAlive = numLoversAlive;
        TeamPavlovsAlive = Pavlovsdogs.loser ? 0 : numPavlovsAlive;
        TotalAlive = numTotalAlive;
        TeamAkujoAlive = numAkujoAlive;
        TeamImpostorHasAliveLover = impLover;
        TeamJackalHasAliveLover = jackalLover;
        TeamPavlovsHasAliveLover = pavlovsLover;
        TeamWerewolfHasAliveLover = werewolfLover;
        TeamWerewolfAlive = numWerewolfAlive;
        TeamSwooperHasAliveLover = swooperLover;
        TeamSwooperAlive = numSwooperAlive;
        TeamJuggernautAlive = numJuggernautAlive;
        TeamJuggernautHasAliveLover = juggernautLover;
    }
}