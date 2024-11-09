using System.Collections.Generic;
using UnityEngine;

namespace TheOtherRoles.Roles.Modifier;
public class Assassin
{
    public static List<PlayerControl> assassin = [];
    public static Color color = Palette.ImpostorRed;

    public static int remainingShotsEvilGuesser = 2;
    public static bool assassinMultipleShotsPerMeeting;
    public static bool assassinKillsThroughShield = true;
    public static bool evilGuesserCanGuessCrewmate = true;
    public static bool evilGuesserCanGuessSpy = true;
    public static bool guesserCantGuessSnitch;

    public static void clearAndReload()
    {
        assassin.Clear();
        remainingShotsEvilGuesser = Mathf.RoundToInt(CustomOptionHolder.modifierAssassinNumberOfShots.GetFloat());
        assassinMultipleShotsPerMeeting = CustomOptionHolder.modifierAssassinMultipleShotsPerMeeting.GetBool();
        assassinKillsThroughShield = CustomOptionHolder.modifierAssassinKillsThroughShield.GetBool();
        evilGuesserCanGuessCrewmate = CustomOptionHolder.guesserEvilCanKillCrewmate.GetBool();
        evilGuesserCanGuessSpy = CustomOptionHolder.guesserEvilCanKillSpy.GetBool();
        guesserCantGuessSnitch = CustomOptionHolder.guesserCantGuessSnitchIfTaksDone.GetBool();
    }
}