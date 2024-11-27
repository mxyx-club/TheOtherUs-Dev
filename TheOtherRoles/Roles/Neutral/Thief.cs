using System.Linq;
using UnityEngine;

namespace TheOtherRoles.Roles.Neutral;

public static class Thief
{
    public static PlayerControl thief;
    public static Color color = new Color32(71, 99, 45, byte.MaxValue);
    public static PlayerControl currentTarget;
    public static PlayerControl formerThief;

    public static float cooldown = 30f;

    public static bool suicideFlag; // Used as a flag for suicide

    public static bool hasImpostorVision;
    public static bool canUseVents;
    public static bool canKillSheriff;
    public static bool canKillDeputy;
    public static bool canKillVeteran;
    public static bool canStealWithGuess;

    public static void clearAndReload()
    {
        thief = null;
        suicideFlag = false;
        currentTarget = null;
        formerThief = null;
        hasImpostorVision = CustomOptionHolder.thiefHasImpVision.GetBool();
        cooldown = CustomOptionHolder.thiefCooldown.GetFloat();
        canUseVents = CustomOptionHolder.thiefCanUseVents.GetBool();
        canKillSheriff = CustomOptionHolder.thiefCanKillSheriff.GetBool();
        canKillDeputy = CustomOptionHolder.thiefCanKillDeputy.GetBool();
        canKillVeteran = CustomOptionHolder.thiefCanKillVeteran.GetBool();
        canStealWithGuess = CustomOptionHolder.thiefCanStealWithGuess.GetBool();
    }

    public static bool tiefCanKill(PlayerControl target, PlayerControl killer)
    {
        return killer == thief && (target.Data.Role.IsImpostor ||
            target == Jackal.jackal ||
            target == Sidekick.sidekick ||
            target == Werewolf.werewolf ||
            target == Juggernaut.juggernaut ||
            target == Swooper.swooper ||
            Pavlovsdogs.pavlovsdogs.Any(p => p == target) ||
            target == Pavlovsdogs.pavlovsowner ||
            (canKillSheriff && target == Sheriff.sheriff) ||
            (canKillDeputy && target == Deputy.deputy) ||
            (canKillVeteran && target == Veteran.veteran));
    }
}
