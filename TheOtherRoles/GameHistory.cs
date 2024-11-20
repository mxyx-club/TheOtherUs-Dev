using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace TheOtherRoles;

public enum CustomDeathReason
{
    HostCmdKill,
    Exile,
    Kill,
    Disconnect,
    Guess,
    Shift,
    LawyerSuicide,
    LoverSuicide, // not necessary
    WitchExile,
    Bomb,
    LoveStolen,
    Loneliness,
    Arson,
    FakeSK,
    SheriffKill,
    SheriffMisfire,
    SheriffMisadventure,
    Suicide,
    BombVictim,
}
public class DeadPlayer
{
    public CustomDeathReason DeathReason { get; set; }
    public PlayerControl KillerIfExisting { get; set; }
    public PlayerControl Player { get; }
    public DateTime TimeOfDeath { get; }
    public bool wasCleaned { get; set; }

    public DeadPlayer(PlayerControl player, DateTime timeOfDeath, CustomDeathReason deathReason, PlayerControl killerIfExisting)
    {
        this.Player = player;
        this.TimeOfDeath = timeOfDeath;
        DeathReason = deathReason;
        KillerIfExisting = killerIfExisting;
        wasCleaned = false;
    }
}

public static class GameHistory
{
    private static readonly Dictionary<byte, DeadPlayer> allDeadPlayers = new();

    public static List<Tuple<Vector3, bool>> localPlayerPositions = new();

    public static void Clear()
    {
        localPlayerPositions.Clear();
        allDeadPlayers.Clear();
    }

    public static IReadOnlyDictionary<byte, DeadPlayer> AllDeadPlayers => allDeadPlayers;

    public static DeadPlayer GetByPlayerId(byte playerId)
    {
        return allDeadPlayers.TryGetValue(playerId, out var deadPlayer) ? deadPlayer : null;
    }

    public static void OverrideDeathReasonAndKiller(PlayerControl player, CustomDeathReason deathReason, PlayerControl killer = null)
    {
        if (player == null) return;

        byte playerId = player.PlayerId;
        if (allDeadPlayers.TryGetValue(playerId, out var existingDeadPlayer))
        {
            existingDeadPlayer.DeathReason = deathReason;
            if (killer != null) existingDeadPlayer.KillerIfExisting = killer;
        }
        else
        {
            var newDeadPlayer = new DeadPlayer(player, DateTime.UtcNow, deathReason, killer);
            allDeadPlayers[playerId] = newDeadPlayer;
        }
    }

    public static int GetKillCount(PlayerControl killer)
    {
        if (killer == null) return 0;

        return allDeadPlayers.Values.Count(info => info.KillerIfExisting != null && info.KillerIfExisting.PlayerId == killer.PlayerId);
    }
}