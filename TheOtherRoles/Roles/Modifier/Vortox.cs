﻿using UnityEngine;

namespace TheOtherRoles.Roles.Modifier;

public class Vortox
{
    public static PlayerControl Player;
    public static Color color = Palette.ImpostorRed;

    public static bool skipMeeting;
    public static int skipMeetingNum;
    public static int skipCount;
    public static bool triggerImpWin;

    public static bool Reversal => Player.IsAlive() && PlayerControl.LocalPlayer != Medic.futureShielded;

    public static void ClearAndReload()
    {
        Player = null;
        triggerImpWin = false;
        skipMeeting = CustomOptionHolder.modifierVortoxSkipMeeting.GetBool();
        skipMeetingNum = CustomOptionHolder.modifierVortoxSkipNum.GetInt();
        skipCount = 0;
    }
}
