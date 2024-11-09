using System.Collections.Generic;
using Hazel;
using TheOtherRoles.Utilities;
using UnityEngine;

namespace TheOtherRoles.Roles.Neutral;

public class Jackal
{
    public static PlayerControl jackal;

    public static Color color = new Color32(0, 180, 235, byte.MaxValue);
    public static PlayerControl currentTarget;
    public static List<PlayerControl> formerJackals = new();

    public static float cooldown = 30f;
    public static float createSidekickCooldown = 30f;
    public static bool canUseVents = true;
    public static bool canCreateSidekick = true;
    public static bool jackalPromotedFromSidekickCanCreateSidekick = true;
    public static bool hasImpostorVision;
    public static bool CanImpostorFindSidekick;
    public static bool canSabotage;
    public static bool killFakeImpostor;
    public static bool wasTeamRed;
    public static bool wasImpostor;
    public static bool wasSpy;

    public static float chanceSwoop;
    public static bool canSwoop;
    public static float swoopTimer;
    public static float swoopCooldown = 30f;
    public static float duration = 30f;
    public static bool isInvisable;

    public static ResourceSprite SidekickButton = new("SidekickButton.png");

    public static void setSwoop()
    {
        var chance = canSwoop = rnd.NextDouble() < chanceSwoop;
        var writer = AmongUsClient.Instance.StartRpcImmediately(CachedPlayer.LocalPlayer.PlayerControl.NetId,
                    (byte)CustomRPC.JackalCanSwooper, SendOption.Reliable);
        writer.Write(chance ? byte.MaxValue : 0);
        AmongUsClient.Instance.FinishRpcImmediately(writer);
        RPCProcedure.jackalCanSwooper(chance);
    }

    public static void clearAndReload()
    {
        jackal = null;
        formerJackals.Clear();
        currentTarget = null;
        isInvisable = false;
        cooldown = CustomOptionHolder.jackalKillCooldown.GetFloat();
        swoopCooldown = CustomOptionHolder.jackalSwooperCooldown.GetFloat();
        duration = CustomOptionHolder.jackalSwooperDuration.GetFloat();
        createSidekickCooldown = CustomOptionHolder.jackalCreateSidekickCooldown.GetFloat();
        canUseVents = CustomOptionHolder.jackalCanUseVents.GetBool();
        canSabotage = CustomOptionHolder.jackalCanUseSabo.GetBool();
        CanImpostorFindSidekick = CustomOptionHolder.jackalCanImpostorFindSidekick.GetBool();
        canCreateSidekick = CustomOptionHolder.jackalCanCreateSidekick.GetBool();
        jackalPromotedFromSidekickCanCreateSidekick = CustomOptionHolder.jackalPromotedFromSidekickCanCreateSidekick.GetBool();
        hasImpostorVision = CustomOptionHolder.jackalAndSidekickHaveImpostorVision.GetBool();
        killFakeImpostor = CustomOptionHolder.jackalkillFakeImpostor.GetBool();
        wasTeamRed = wasImpostor = wasSpy = false;
        chanceSwoop = CustomOptionHolder.jackalChanceSwoop.GetSelection() / 10f;
    }
}
