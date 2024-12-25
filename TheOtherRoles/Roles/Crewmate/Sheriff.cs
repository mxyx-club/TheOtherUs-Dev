using System.Linq;
using UnityEngine;

namespace TheOtherRoles.Roles.Crewmate;

public static class Sheriff
{
    public static PlayerControl sheriff;
    public static Color color = new Color32(248, 205, 70, byte.MaxValue);

    public static float cooldown = 30f;
    public static bool canKillNeutrals;
    public static bool canKillLawyer;
    public static bool canKillSurvivor;
    public static bool canKillJester;
    public static bool canKillPursuer;
    public static bool canKillPartTimer;
    public static bool canKillVulture;
    public static bool canKillThief;
    public static bool canKillAmnesiac;
    public static bool canKillExecutioner;
    public static bool canKillDoomsayer;
    public static bool spyCanDieToSheriff;
    public static int misfireKills; // Self: 0, Target: 1, Both: 2

    public static PlayerControl currentTarget;

    public static PlayerControl formerDeputy; // Needed for keeping handcuffs + shifting
    public static PlayerControl formerSheriff; // When deputy gets promoted...

    public static void replaceCurrentSheriff(PlayerControl deputy)
    {
        if (!formerSheriff) formerSheriff = sheriff;
        sheriff = deputy;
        currentTarget = null;
        cooldown = CustomOptionHolder.sheriffCooldown.GetFloat();
    }

    public static bool sheriffCanKillNeutral(PlayerControl target)
    {
        return (target != Mini.mini || Mini.isGrownUp()) &&
               (target.Data.Role.IsImpostor ||
                Jackal.jackal.Any(x => x == target) ||
                Jackal.sidekick == target ||
                Juggernaut.juggernaut == target ||
                Werewolf.werewolf == target ||
                Swooper.swooper == target ||
                Pavlovsdogs.pavlovsowner == target ||
                Pavlovsdogs.pavlovsdogs.Any(p => p == target) ||
                (spyCanDieToSheriff && Spy.spy == target) ||
                (canKillNeutrals &&
                    (Akujo.akujo == target || isKillerNeutral(target) ||
                        (Survivor.Player.Any(p => p == target) && canKillSurvivor) ||
                        (Jester.jester == target && canKillJester) ||
                        (Vulture.vulture == target && canKillVulture) ||
                        (Thief.thief == target && canKillThief) || Witness.Player == target ||
                        (Amnisiac.Player.Any(p => p == target) && canKillAmnesiac) ||
                        (PartTimer.partTimer == target && canKillPartTimer) ||
                        (Lawyer.lawyer == target && canKillLawyer) ||
                        (Executioner.executioner == target && canKillExecutioner) ||
                        (Pursuer.Player.Any(p => p == target) && canKillPursuer) ||
                        (Doomsayer.doomsayer == target && canKillDoomsayer))));
    }

    public static void clearAndReload()
    {
        sheriff = null;
        currentTarget = null;
        formerDeputy = null;
        formerSheriff = null;
        misfireKills = CustomOptionHolder.sheriffMisfireKills.GetSelection();
        cooldown = CustomOptionHolder.sheriffCooldown.GetFloat();
        canKillNeutrals = CustomOptionHolder.sheriffCanKillNeutrals.GetBool();
        canKillSurvivor = CustomOptionHolder.sheriffCanKillSurvivor.GetBool();
        canKillLawyer = CustomOptionHolder.sheriffCanKillLawyer.GetBool();
        canKillJester = CustomOptionHolder.sheriffCanKillJester.GetBool();
        canKillPursuer = CustomOptionHolder.sheriffCanKillPursuer.GetBool();
        canKillPartTimer = CustomOptionHolder.sheriffCanKillPartTimer.GetBool();
        canKillVulture = CustomOptionHolder.sheriffCanKillVulture.GetBool();
        canKillThief = CustomOptionHolder.sheriffCanKillThief.GetBool();
        canKillAmnesiac = CustomOptionHolder.sheriffCanKillAmnesiac.GetBool();
        canKillExecutioner = CustomOptionHolder.sheriffCanKillExecutioner.GetBool();
        spyCanDieToSheriff = CustomOptionHolder.spyCanDieToSheriff.GetBool();
        canKillDoomsayer = CustomOptionHolder.sheriffCanKillDoomsayer.GetBool();
    }
}
