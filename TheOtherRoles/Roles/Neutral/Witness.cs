using UnityEngine;

namespace TheOtherRoles.Roles.Neutral;

public class Witness
{
    public static PlayerControl player;
    public static Color color = new Color32(123, 170, 255, byte.MaxValue);
    public static PlayerControl target;

    public static int markTimer;
    public static int winCount;

    public static void ClearAndReload()
    {
        player = null;
        target = null;
        markTimer = 30;
        winCount = 3;
    }
}
