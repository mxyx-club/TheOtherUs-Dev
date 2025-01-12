using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AmongUs.GameOptions;
using TheOtherRoles.Utilities;
using TMPro;
using UnityEngine;
using Object = UnityEngine.Object;

namespace TheOtherRoles.Patches;

internal enum CustomGameOverReason
{
    Canceled = 10,
    ImpostorWin,
    LoversWin,
    TeamJackalWin,
    TeamPavlovsWin,
    MiniLose,
    JesterWin,
    WitnessWin,
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
    Canceled = -1,
    Default,
    MiniLose,
    EveryoneDied,
    TaskerWin,
    LoversTeamWin,
    LoversSoloWin,
    JesterWin,
    JackalWin,
    WitnessWin,
    PavlovsWin,
    SwooperWin,
    ArsonistWin,
    VultureWin,
    LawyerSoloWin,
    AdditionalLawyerBonusWin,
    AdditionalLawyerStolenWin,
    AdditionalAlivePursuerWin,
    AdditionalAliveSurvivorWin,
    AdditionalPartTimerWin,
    ExecutionerWin,
    WerewolfWin,
    JuggernautWin,
    DoomsayerWin,
    AkujoSoloWin,
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
        List<RoleInfo> killRole =
        [
            RoleInfo.sheriff,
            RoleInfo.jackal,
            RoleInfo.sidekick,
            RoleInfo.swooper,
            RoleInfo.thief,
            RoleInfo.werewolf,
            RoleInfo.juggernaut,
            RoleInfo.pavlovsdogs
        ];

        foreach (var playerControl in CachedPlayer.AllPlayers)
        {
            var roles = RoleInfo.getRoleInfoForPlayer(playerControl);
            var (tasksCompleted, tasksTotal) = TasksHandler.taskInfo(playerControl.Data);
            var isGuesser = HandleGuesser.isGuesserGm && HandleGuesser.isGuesser(playerControl.PlayerId);
            int? killCount = GameHistory.GetKillCount(playerControl);
            if (killCount == 0 &&
                !(killRole.Contains(RoleInfo.getRoleInfoForPlayer(playerControl, false).FirstOrDefault())
                 || playerControl.Data.Role.IsImpostor)) killCount = null;
            var roleString = RoleInfo.GetRolesString(playerControl, true, true, true, false);
            AdditionalTempData.playerRoles.Add(new AdditionalTempData.PlayerRoleInfo
            {
                PlayerName = playerControl.PlayerName,
                Roles = roles,
                RoleNames = roleString,
                TasksTotal = tasksTotal,
                TasksCompleted = tasksCompleted,
                IsGuesser = isGuesser,
                Kills = killCount,
                IsAlive = playerControl.IsAlive
            });
        }

        // Remove Jester, Arsonist, Vulture, Jackal, former Jackals and Sidekick from winners (if they win, they'll be readded)
        var notWinners = new List<PlayerControl>();

        notWinners.AddRange(new[]
        {
            Jester.jester,
            Jackal.sidekick,
            Arsonist.arsonist,
            Swooper.swooper,
            Vulture.vulture,
            Werewolf.werewolf,
            Lawyer.lawyer,
            Executioner.executioner,
            Witness.Player,
            Specter.Player,
            Thief.thief,
            Juggernaut.juggernaut,
            Doomsayer.doomsayer,
            PartTimer.partTimer,
            Akujo.akujo,
            Pavlovsdogs.pavlovsowner,
        }.Where(p => p != null));

        notWinners.AddRange(Amnisiac.Player.Where(p => p != null));
        notWinners.AddRange(Pavlovsdogs.pavlovsdogs.Where(p => p != null));
        notWinners.AddRange(Jackal.jackal.Where(p => p != null));
        notWinners.AddRange(Pursuer.Player.Where(p => p != null));
        notWinners.AddRange(Survivor.Player.Where(p => p != null));
        if (Akujo.honmeiCannotFollowWin && Akujo.honmei != null) notWinners.Add(Akujo.honmei);

        var winnersToRemove = new List<WinningPlayerData>();
        foreach (var winner in TempData.winners.GetFastEnumerator())
            if (notWinners.Any(x => x != null && x.Data.PlayerName == winner.PlayerName))
                winnersToRemove.Add(winner);

        foreach (var winner in winnersToRemove) TempData.winners.Remove(winner);
        var isCanceled = gameOverReason == (GameOverReason)CustomGameOverReason.Canceled;
        var everyoneDead = AdditionalTempData.playerRoles.All(x => !x.IsAlive);
        var miniLose = Mini.mini != null && gameOverReason == (GameOverReason)CustomGameOverReason.MiniLose;
        var jesterWin = Jester.jester != null && gameOverReason == (GameOverReason)CustomGameOverReason.JesterWin;
        var witnessWin = Witness.Player != null && gameOverReason == (GameOverReason)CustomGameOverReason.WitnessWin;
        var impostorWin = (gameOverReason is GameOverReason.ImpostorByKill or GameOverReason.ImpostorBySabotage or GameOverReason.ImpostorByVote)
            || Vortox.triggerImpWin;
        var werewolfWin = gameOverReason == (GameOverReason)CustomGameOverReason.WerewolfWin && Werewolf.werewolf.IsAlive();
        var juggernautWin = gameOverReason == (GameOverReason)CustomGameOverReason.JuggernautWin && Juggernaut.juggernaut.IsAlive();
        var swooperWin = gameOverReason == (GameOverReason)CustomGameOverReason.SwooperWin && Swooper.swooper.IsAlive();
        var arsonistWin = Arsonist.arsonist != null && gameOverReason == (GameOverReason)CustomGameOverReason.ArsonistWin;
        var doomsayerWin = Doomsayer.doomsayer != null && gameOverReason == (GameOverReason)CustomGameOverReason.DoomsayerWin;
        var loversWin = Lovers.existingAndAlive() && (gameOverReason == (GameOverReason)CustomGameOverReason.LoversWin ||
                         (GameManager.Instance.DidHumansWin(gameOverReason) && !Lovers.existingWithKiller()));
        var teamJackalWin = gameOverReason == (GameOverReason)CustomGameOverReason.TeamJackalWin &&
                            (Jackal.jackal.Any(x => x.IsAlive()) || Jackal.sidekick.IsAlive());
        var teamPavlovsWin = gameOverReason == (GameOverReason)CustomGameOverReason.TeamPavlovsWin &&
                            (Pavlovsdogs.pavlovsowner.IsAlive() || Pavlovsdogs.pavlovsdogs.Any(p => p.IsAlive()));
        var vultureWin = Vulture.vulture != null && gameOverReason == (GameOverReason)CustomGameOverReason.VultureWin;
        var executionerWin = Executioner.executioner != null && gameOverReason == (GameOverReason)CustomGameOverReason.ExecutionerWin;
        var lawyerSoloWin = Lawyer.lawyer != null && gameOverReason == (GameOverReason)CustomGameOverReason.LawyerSoloWin;
        var akujoWin = Akujo.akujo.IsAlive() && Akujo.honmei.IsAlive() &&
            (Akujo.honmeiOptimizeWin
                ? !Akujo.existingWithKiller() &&
                  (gameOverReason == (GameOverReason)CustomGameOverReason.AkujoWin || GameManager.Instance.DidHumansWin(gameOverReason))
                : gameOverReason == (GameOverReason)CustomGameOverReason.AkujoWin);
        bool isPursurerLose = jesterWin || arsonistWin || miniLose || isCanceled || executionerWin;

        // Mini lose
        if (miniLose)
        {
            // If "no one is the Mini", it will display the Mini, but also show defeat to everyone
            TempData.winners = new Il2CppSystem.Collections.Generic.List<WinningPlayerData>();
            var wpd = new WinningPlayerData(Mini.mini.Data) { IsYou = false };
            TempData.winners.Add(wpd);
            AdditionalTempData.winCondition = WinCondition.MiniLose;
        }
        else if (isCanceled)
        {
            TempData.winners = new Il2CppSystem.Collections.Generic.List<WinningPlayerData>();
            AdditionalTempData.winCondition = WinCondition.Canceled;
        }

        // Jester win
        else if (jesterWin)
        {
            TempData.winners = new Il2CppSystem.Collections.Generic.List<WinningPlayerData>();
            var wpd = new WinningPlayerData(Jester.jester.Data);
            TempData.winners.Add(wpd);
            AdditionalTempData.winCondition = WinCondition.JesterWin;
        }

        // Witness win
        else if (witnessWin)
        {
            TempData.winners = new Il2CppSystem.Collections.Generic.List<WinningPlayerData>();
            var wpd = new WinningPlayerData(Witness.Player.Data);
            TempData.winners.Add(wpd);
            AdditionalTempData.winCondition = WinCondition.WitnessWin;
        }

        // Arsonist win
        else if (arsonistWin)
        {
            TempData.winners = new Il2CppSystem.Collections.Generic.List<WinningPlayerData>();
            var wpd = new WinningPlayerData(Arsonist.arsonist.Data);
            TempData.winners.Add(wpd);
            AdditionalTempData.winCondition = WinCondition.ArsonistWin;
        }

        // Everyone Died
        else if (everyoneDead)
        {
            TempData.winners = new Il2CppSystem.Collections.Generic.List<WinningPlayerData>();
            AdditionalTempData.winCondition = WinCondition.EveryoneDied;
        }

        // Vulture win
        else if (vultureWin)
        {
            TempData.winners = new Il2CppSystem.Collections.Generic.List<WinningPlayerData>();
            var wpd = new WinningPlayerData(Vulture.vulture.Data);
            TempData.winners.Add(wpd);
            AdditionalTempData.winCondition = WinCondition.VultureWin;
        }

        // Jester win
        else if (executionerWin)
        {
            TempData.winners = new Il2CppSystem.Collections.Generic.List<WinningPlayerData>();
            var wpd = new WinningPlayerData(Executioner.executioner.Data);
            TempData.winners.Add(wpd);
            AdditionalTempData.winCondition = WinCondition.ExecutionerWin;
        }

        // Lovers win conditions
        else if (loversWin)
        {
            // Double win for lovers, crewmates also win
            if (!Lovers.existingWithKiller())
            {
                AdditionalTempData.winCondition = WinCondition.LoversTeamWin;
                TempData.winners = new Il2CppSystem.Collections.Generic.List<WinningPlayerData>();
                foreach (PlayerControl p in CachedPlayer.AllPlayers)
                {
                    if (p == null) continue;
                    if (p == Lovers.lover1 || p == Lovers.lover2)
                        TempData.winners.Add(new WinningPlayerData(p.Data));
                    else if (Pursuer.Player.Any(pc => pc == p) && Pursuer.Player.Any(pc => pc.IsAlive()))
                        TempData.winners.Add(new WinningPlayerData(p.Data));
                    else if (Survivor.Player.Any(pc => pc == p) && Survivor.Player.Any(pc => pc.IsAlive()))
                        TempData.winners.Add(new WinningPlayerData(p.Data));
                    else if (!notWinners.Contains(p) && !p.Data.Role.IsImpostor)
                        TempData.winners.Add(new WinningPlayerData(p.Data));
                }
            }
            // Lovers solo win
            else
            {
                AdditionalTempData.winCondition = WinCondition.LoversSoloWin;
                TempData.winners = new Il2CppSystem.Collections.Generic.List<WinningPlayerData>();
                TempData.winners.Add(new WinningPlayerData(Lovers.lover1.Data));
                TempData.winners.Add(new WinningPlayerData(Lovers.lover2.Data));
            }
        }

        // Jackal win condition (should be implemented using a proper GameOverReason in the future)
        else if (teamJackalWin)
        {
            // Jackal wins if nobody except jackal is alive
            AdditionalTempData.winCondition = WinCondition.JackalWin;
            TempData.winners = new Il2CppSystem.Collections.Generic.List<WinningPlayerData>();
            foreach (var player in Jackal.jackal)
            {
                var wpdFormerJackal = new WinningPlayerData(player.Data);
                wpdFormerJackal.IsImpostor = false;
                TempData.winners.Add(wpdFormerJackal);
            }
            // If there is a sidekick. The sidekick also wins
            if (Jackal.sidekick != null)
            {
                var wpdSidekick = new WinningPlayerData(Jackal.sidekick.Data);
                wpdSidekick.IsImpostor = false;
                TempData.winners.Add(wpdSidekick);
            }
        }
        else if (teamPavlovsWin)
        {
            // Jackal wins if nobody except jackal is alive
            AdditionalTempData.winCondition = WinCondition.PavlovsWin;
            TempData.winners = new Il2CppSystem.Collections.Generic.List<WinningPlayerData>();
            var wpd = new WinningPlayerData(Pavlovsdogs.pavlovsowner.Data);
            wpd.IsImpostor = false;
            TempData.winners.Add(wpd);

            foreach (var player in Pavlovsdogs.pavlovsdogs)
            {
                var wpdFormerPavlovs = new WinningPlayerData(player.Data);
                wpdFormerPavlovs.IsImpostor = false;
                TempData.winners.Add(wpdFormerPavlovs);
            }
        }
        else if (werewolfWin)
        {
            // Werewolf wins if nobody except jackal is alive
            AdditionalTempData.winCondition = WinCondition.WerewolfWin;
            TempData.winners = new Il2CppSystem.Collections.Generic.List<WinningPlayerData>();
            var wpd = new WinningPlayerData(Werewolf.werewolf.Data);
            wpd.IsImpostor = false;
            TempData.winners.Add(wpd);
        }

        else if (juggernautWin)
        {
            // JuggernautWin wins if nobody except jackal is alive
            AdditionalTempData.winCondition = WinCondition.JuggernautWin;
            TempData.winners = new Il2CppSystem.Collections.Generic.List<WinningPlayerData>();
            var wpd = new WinningPlayerData(Juggernaut.juggernaut.Data);
            wpd.IsImpostor = false;
            TempData.winners.Add(wpd);
        }

        else if (impostorWin)
        {
            TempData.winners = new Il2CppSystem.Collections.Generic.List<WinningPlayerData>();
            foreach (GameData.PlayerInfo player in GameData.Instance.AllPlayers)
            {
                var wpd = new WinningPlayerData(player) { IsImpostor = true };
                if (player.Role.IsImpostor)
                {
                    TempData.winners.Add(wpd);
                }
            }
        }

        else if (doomsayerWin)
        {
            // DoomsayerWin wins if nobody except jackal is alive
            AdditionalTempData.winCondition = WinCondition.DoomsayerWin;
            TempData.winners = new Il2CppSystem.Collections.Generic.List<WinningPlayerData>();
            var wpd = new WinningPlayerData(Doomsayer.doomsayer.Data);
            TempData.winners.Add(wpd);
            AdditionalTempData.winCondition = WinCondition.DoomsayerWin;
        }

        //Swooper
        else if (swooperWin)
        {
            // Swooper wins if nobody except jackal is alive
            AdditionalTempData.winCondition = WinCondition.SwooperWin;
            TempData.winners = new Il2CppSystem.Collections.Generic.List<WinningPlayerData>();
            var wpd = new WinningPlayerData(Swooper.swooper.Data);
            wpd.IsImpostor = false;
            TempData.winners.Add(wpd);
        }

        // Akujo win
        else if (akujoWin)
        {
            if (Akujo.honmeiOptimizeWin && !Akujo.existingWithKiller())
            {
                AdditionalTempData.winCondition = WinCondition.AkujoTeamWin;
                TempData.winners = new Il2CppSystem.Collections.Generic.List<WinningPlayerData>();
                foreach (PlayerControl p in CachedPlayer.AllPlayers)
                {
                    if (p == null) continue;
                    if (p == Akujo.akujo || p == Akujo.honmei)
                        TempData.winners.Add(new WinningPlayerData(p.Data));
                    else if (Pursuer.Player.Contains(p) && !p.Data.IsDead)
                        TempData.winners.Add(new WinningPlayerData(p.Data));
                    else if (Survivor.Player.Contains(p) && !p.Data.IsDead)
                        TempData.winners.Add(new WinningPlayerData(p.Data));
                    else if (!notWinners.Contains(p) && !p.Data.Role.IsImpostor)
                        TempData.winners.Add(new WinningPlayerData(p.Data));
                }
            }
            else
            {
                AdditionalTempData.winCondition = WinCondition.AkujoSoloWin;
                TempData.winners = new Il2CppSystem.Collections.Generic.List<WinningPlayerData>();
                TempData.winners.Add(new WinningPlayerData(Akujo.akujo.Data));
                TempData.winners.Add(new WinningPlayerData(Akujo.honmei.Data));
            }
        }

        // Lawyer solo win 
        else if (lawyerSoloWin)
        {
            TempData.winners = new Il2CppSystem.Collections.Generic.List<WinningPlayerData>();
            WinningPlayerData wpd = new(Lawyer.lawyer.Data);
            TempData.winners.Add(wpd);
            AdditionalTempData.winCondition = WinCondition.LawyerSoloWin;
        }

        // Possible Additional winner: Lawyer
        if (!lawyerSoloWin && Lawyer.lawyer != null && Lawyer.target != null &&
            (!Lawyer.target.Data.IsDead || Lawyer.target == Jester.jester) && !Lawyer.notAckedExiled)
        {
            WinningPlayerData winningClient = null;
            foreach (var winner in TempData.winners.GetFastEnumerator())
                if (winner.PlayerName == Lawyer.target.Data.PlayerName)
                    winningClient = winner;
            if (winningClient != null)
            {
                if (!TempData.winners.ToArray().Any(x => x.PlayerName == Lawyer.lawyer.Data.PlayerName))
                {
                    if (!Lawyer.lawyer.Data.IsDead && Lawyer.stolenWin)
                    {
                        TempData.winners.Remove(winningClient);
                        TempData.winners.Add(new WinningPlayerData(Lawyer.lawyer.Data));
                        AdditionalTempData.additionalWinConditions.Add(WinCondition.AdditionalLawyerStolenWin); // The Lawyer replaces the client's victory
                    }
                    else
                    {
                        TempData.winners.Add(new WinningPlayerData(Lawyer.lawyer.Data));
                        AdditionalTempData.additionalWinConditions.Add(WinCondition.AdditionalLawyerBonusWin); // The Lawyer wins with the client
                    }
                }
            }
        }

        // Possible Additional winner: Pursuer
        if (Pursuer.Player != null && Pursuer.Player.Any(p => !p.Data.IsDead) && !Lawyer.notAckedExiled && !isPursurerLose)
        {
            foreach (var player in Pursuer.Player.Where(p => !p.Data.IsDead))
            {
                if (!TempData.winners.ToArray().Any(x => x.PlayerName == player.Data.PlayerName))
                    TempData.winners.Add(new WinningPlayerData(player.Data));
            }
            AdditionalTempData.additionalWinConditions.Add(WinCondition.AdditionalAlivePursuerWin);
        }

        // Possible Additional winner: Survivor
        if (Survivor.Player != null && Survivor.Player.Any(p => !p.Data.IsDead) && !isPursurerLose)
        {
            foreach (var player in Survivor.Player.Where(p => !p.Data.IsDead))
            {
                if (!TempData.winners.ToArray().Any(x => x.PlayerName == player.Data.PlayerName))
                    TempData.winners.Add(new WinningPlayerData(player.Data));
            }
            AdditionalTempData.additionalWinConditions.Add(WinCondition.AdditionalAliveSurvivorWin);
        }

        if (PartTimer.partTimer != null && PartTimer.target != null && TempData.winners.ToArray().Any(x => x.PlayerName == PartTimer.target.Data.PlayerName))
        {
            TempData.winners.Add(new WinningPlayerData(PartTimer.partTimer.Data));
            AdditionalTempData.additionalWinConditions.Add(WinCondition.AdditionalPartTimerWin);
        }
        Message($"游戏结束 {AdditionalTempData.winCondition}", "OnGameEnd");
        // Reset Settings
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
        var list = TempData.winners.ToList().OrderBy(delegate (WinningPlayerData b) { return !b.IsYou ? 0 : -1; }).ToList();
        for (var i = 0; i < list.Count; i++)
        {
            var winningPlayerData2 = list[i];
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
            if (winningPlayerData2.IsDead)
            {
                poolablePlayer.SetBodyAsGhost();
                poolablePlayer.SetDeadFlipX(i % 2 == 0);
            }
            else
            {
                poolablePlayer.SetFlipX(i % 2 == 0);
            }

            poolablePlayer.UpdateFromPlayerOutfit(winningPlayerData2, PlayerMaterial.MaskType.None, winningPlayerData2.IsDead, true);

            poolablePlayer.cosmetics.nameText.color = Color.white;
            poolablePlayer.cosmetics.nameText.transform.localScale = new Vector3(1f / vector.x, 1f / vector.y, 1f / vector.z);
            var localPosition = poolablePlayer.cosmetics.nameText.transform.localPosition;
            localPosition = new Vector3(localPosition.x, localPosition.y, -15f);
            poolablePlayer.cosmetics.nameText.transform.localPosition = localPosition;
            poolablePlayer.cosmetics.nameText.text = winningPlayerData2.PlayerName;

            foreach (var roles in from data in AdditionalTempData.playerRoles
                                  where data.PlayerName == winningPlayerData2.PlayerName
                                  select poolablePlayer.cosmetics.nameText.text +=
                         $"\n{string.Join("\n", data.Roles.Select(x => cs(x.color, x.Name)))}")
            {
            }
        }

        // Create a dictionary for win conditions
        var winConditionTexts = new Dictionary<WinCondition, (Color, string)>
        {
            { WinCondition.Canceled, (Color.gray, "CanceledEnd") },
            { WinCondition.EveryoneDied, (Palette.DisabledGrey, "EveryoneDied") },
            { WinCondition.JesterWin, (Jester.color, "JesterWin") },
            { WinCondition.DoomsayerWin, (Doomsayer.color, "DoomsayerWin") },
            { WinCondition.ArsonistWin, (Arsonist.color, "ArsonistWin") },
            { WinCondition.VultureWin, (Vulture.color, "VultureWin") },
            { WinCondition.LawyerSoloWin, (Lawyer.color, "LawyerSoloWin") },
            { WinCondition.WerewolfWin, (Werewolf.color, "WerewolfWin") },
            { WinCondition.WitnessWin, (Witness.color, "WitnessWin") },
            { WinCondition.JuggernautWin, (Juggernaut.color, "JuggernautWin") },
            { WinCondition.SwooperWin, (Swooper.color, "SwooperWin") },
            { WinCondition.ExecutionerWin, (Executioner.color, "ExecutionerWin") },
            { WinCondition.LoversTeamWin, (Lovers.color, "LoversTeamWin") },
            { WinCondition.LoversSoloWin, (Lovers.color, "LoversSoloWin") },
            { WinCondition.JackalWin, (Jackal.color, "JackalWin") },
            { WinCondition.PavlovsWin, (Pavlovsdogs.color, "PavlovsWin") },
            { WinCondition.AkujoSoloWin, (Akujo.color, "AkujoSoloWin") },
            { WinCondition.AkujoTeamWin, (Akujo.color, "AkujoTeamWin") },
            { WinCondition.MiniLose, (Mini.color, "MiniLose") },
        };

        var winConditionMappings = new Dictionary<WinCondition, (Color, string)>
        {
            { WinCondition.AdditionalLawyerStolenWin, (Lawyer.color, "LawyerStolenWin") },
            { WinCondition.AdditionalLawyerBonusWin, (Lawyer.color, "LawyerBonusWin") },
            { WinCondition.AdditionalPartTimerWin, (PartTimer.color, "PartTimerWin") },
            { WinCondition.AdditionalAlivePursuerWin, (Pursuer.color, "起诉人存活") },
            { WinCondition.AdditionalAliveSurvivorWin, (Survivor.color, "幸存者存活") }
        };

        var bonusText = Object.Instantiate(__instance.WinText.gameObject);
        var position1 = __instance.WinText.transform.position;
        bonusText.transform.position = new Vector3(position1.x, position1.y - 0.5f, position1.z);
        bonusText.transform.localScale = new Vector3(0.7f, 0.7f, 1f);
        var textRenderer = bonusText.GetComponent<TMP_Text>();
        textRenderer.text = "";

        if (winConditionTexts.TryGetValue(AdditionalTempData.winCondition, out var winText))
        {
            textRenderer.text = winText.Item2.Translate();
            textRenderer.color = winText.Item1;
            __instance.BackgroundBar.material.SetColor("_Color", winText.Item1);
        }

        var winConditionsTexts = new List<string>();
        foreach (var cond in AdditionalTempData.additionalWinConditions)
        {
            if (winConditionMappings.TryGetValue(cond, out var mapping))
            {
                winConditionsTexts.Add(cs(mapping.Item1, mapping.Item2.Translate()));
            }
        }

        if (winConditionsTexts.Count == 1)
        {
            textRenderer.text += $"<size=50%>\n{winConditionsTexts[0]}</size>";
        }
        else if (winConditionsTexts.Count > 1)
        {
            var combinedText = string.Join(" & ", winConditionsTexts);
            textRenderer.text += $"<size=50%>\n{combinedText}</size>";
        }

        if (GameOptionsManager.Instance.currentGameOptions.GameMode == GameModes.Normal)
        {
            if (Camera.main != null)
            {
                var position = Camera.main.ViewportToWorldPoint(new Vector3(0f, 1f, Camera.main.nearClipPlane));
                var roleSummary = Object.Instantiate(__instance.WinText.gameObject);
                roleSummary.transform.position = new Vector3(__instance.Navigation.ExitButton.transform.position.x + 0.1f,
                    position.y - 0.1f, -214f);
                roleSummary.transform.localScale = new Vector3(1f, 1f, 1f);

                var roleSummaryText = new StringBuilder();

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
        var statistics = new PlayerStatistics(__instance);
        if (ModOption.DisableGameEnd) return false;
        // InstanceExists | Don't check Custom Criteria when in Tutorial
        if (DestroyableSingleton<TutorialManager>.InstanceExists) return true;
        if (CheckAndEndGameForTaskWin(__instance)) return false;
        if (CheckAndEndGameForMiniLose(__instance)) return false;
        if (CheckAndEndGameForJesterWin(__instance)) return false;
        if (CheckAndEndGameForDoomsayerWin(__instance)) return false;
        if (CheckAndEndGameForWitnessWin(__instance)) return false;
        if (CheckAndEndGameForVultureWin(__instance)) return false;
        if (CheckAndEndGameForSabotageWin(__instance)) return false;
        if (CheckAndEndGameForExecutionerWin(__instance)) return false;
        if (CheckAndEndGameForAkujoWin(__instance, statistics)) return false;
        if (CheckAndEndGameForArsonistWin(__instance, statistics)) return false;
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

    private static bool CheckAndEndGameForWitnessWin(ShipStatus __instance)
    {
        if (Witness.triggerWitnessWin)
        {
            //__instance.enabled = false;
            GameManager.Instance.RpcEndGame((GameOverReason)CustomGameOverReason.WitnessWin, false);
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
        if (ModOption.PreventTaskEnd) return false;
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

    private static bool CheckAndEndGameForArsonistWin(ShipStatus __instance, PlayerStatistics statistics)
    {
        if (statistics.TeamArsonistAlive >= statistics.TotalAlive - statistics.TeamArsonistAlive &&
            statistics.TeamImpostorsAlive == 0 &&
            statistics.TeamJuggernautAlive == 0 &&
            statistics.TeamPavlovsAlive == 0 &&
            statistics.TeamJackalAlive == 0 &&
            statistics.TeamWerewolfAlive == 0 &&
            statistics.TeamAkujoAlive == 0 &&
            statistics.TeamSwooperAlive == 0 &&
            !(statistics.TeamArsonisHasAliveLover && statistics.TeamLoversAlive == 2)
            && !killingCrewAlive())
        {
            //__instance.enabled = false;
            GameManager.Instance.RpcEndGame((GameOverReason)CustomGameOverReason.ArsonistWin, false);
            return true;
        }
        return false;
    }

    private static bool CheckAndEndGameForAkujoWin(ShipStatus __instance, PlayerStatistics statistics)
    {
        if ((statistics.TeamAkujoAlive == 2 && statistics.TotalAlive <= 3)
            || (statistics.TeamAkujoAlive == 2 &&
                statistics.TeamImpostorsAlive == 0 &&
                statistics.TeamArsonistAlive == 0 &&
                statistics.TeamJuggernautAlive == 0 &&
                statistics.TeamPavlovsAlive == 0 &&
                statistics.TeamWerewolfAlive == 0 &&
                statistics.TeamJackalAlive == 0 &&
                statistics.TeamSwooperAlive == 0 &&
                !(statistics.TeamLoversAlive != 0 && Lovers.existingWithKiller())))
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
            statistics.TeamArsonistAlive == 0 &&
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
            statistics.TeamArsonistAlive == 0 &&
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
            statistics.TeamArsonistAlive == 0 &&
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
            statistics.TeamArsonistAlive == 0 &&
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
            statistics.TeamArsonistAlive == 0 &&
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
        if (Vortox.triggerImpWin || (statistics.TeamImpostorsAlive >= statistics.TotalAlive - statistics.TeamImpostorsAlive &&
            statistics.TeamJackalAlive == 0 &&
            statistics.TeamPavlovsAlive == 0 &&
            statistics.TeamWerewolfAlive == 0 &&
            statistics.TeamSwooperAlive == 0 &&
            statistics.TeamArsonistAlive == 0 &&
            statistics.TeamAkujoAlive == 0 &&
            statistics.TeamJuggernautAlive == 0 &&
            !(statistics.TeamImpostorHasAliveLover && statistics.TeamLoversAlive == 2) && !killingCrewAlive()))
        {
            //__instance.enabled = false;
            GameOverReason endReason;
            switch (TempData.LastDeathReason)
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
        if (statistics.TeamImpostorsAlive == 0 &&
            statistics.TeamJackalAlive == 0 &&
            statistics.TeamPavlovsAlive == 0 &&
            statistics.TeamArsonistAlive == 0 &&
            statistics.TeamWerewolfAlive == 0 &&
            statistics.TeamSwooperAlive == 0 &&
            statistics.TeamJuggernautAlive == 0)
        {
            if (Akujo.honmeiOptimizeWin || statistics.TeamAkujoAlive <= 1)
            {
                GameManager.Instance.RpcEndGame(GameOverReason.HumansByVote, false);
                return true;
            }
        }
        return false;
    }

    private static void EndGameForSabotage(ShipStatus __instance)
    {
        //__instance.enabled = false;
        GameManager.Instance.RpcEndGame(GameOverReason.ImpostorBySabotage, false);
    }
}

[HarmonyPatch(typeof(GameManager), nameof(GameManager.RpcEndGame))]
internal class RPCEndGamePatch
{
    public static void Postfix(ref GameOverReason endReason)
    {
        Message($"游戏结束 {(CustomGameOverReason)endReason} {endReason}", "RpcEndGame");
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
    public int TeamArsonistAlive { get; set; }
    public int TeamAkujoAlive { get; set; }
    public bool TeamSwooperHasAliveLover { get; set; }
    public bool TeamWerewolfHasAliveLover { get; set; }
    public bool TeamArsonisHasAliveLover { get; set; }
    public int TeamJuggernautAlive { get; set; }
    public bool TeamJuggernautHasAliveLover { get; set; }

    private static bool isLover(GameData.PlayerInfo p)
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
        var numArsonistAlive = 0;
        var numWerewolfAlive = 0;
        var numJuggernautAlive = 0;
        var numAkujoAlive = 0;
        var impLover = false;
        var jackalLover = false;
        var pavlovsLover = false;
        var arsonistLover = false;
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

                    if (Jackal.jackal != null && Jackal.jackal.Any(x => x.PlayerId == playerInfo.PlayerId))
                    {
                        numJackalAlive++;
                        if (lover) jackalLover = true;
                    }

                    if (Jackal.sidekick != null && Jackal.sidekick.PlayerId == playerInfo.PlayerId)
                    {
                        numJackalAlive++;
                        if (lover) jackalLover = true;
                    }

                    if (Arsonist.arsonist != null && Arsonist.arsonist.PlayerId == playerInfo.PlayerId)
                    {
                        numArsonistAlive++;
                        if (lover) arsonistLover = true;
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
        TeamArsonistAlive = numArsonistAlive;
        TeamSwooperHasAliveLover = swooperLover;
        TeamSwooperAlive = numSwooperAlive;
        TeamJuggernautAlive = numJuggernautAlive;
        TeamArsonisHasAliveLover = arsonistLover;
        TeamJuggernautHasAliveLover = juggernautLover;
    }
}