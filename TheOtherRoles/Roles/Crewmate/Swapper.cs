using UnityEngine;

namespace TheOtherRoles.Roles.Crewmate;

public static class Swapper
{
    public static PlayerControl swapper;
    public static Color color = new Color32(134, 55, 86, byte.MaxValue);
    public static bool canCallEmergency;
    public static bool canOnlySwapOthers;
    public static int charges;
    public static float rechargeTasksNumber;
    public static bool canFixSabotages;
    public static float rechargedTasks;

    public static byte playerId1 = byte.MaxValue;
    public static byte playerId2 = byte.MaxValue;

    public static ResourceSprite spriteCheck = new("SwapperCheck.png", 150f);

    public static void clearAndReload()
    {
        swapper = null;
        playerId1 = byte.MaxValue;
        playerId2 = byte.MaxValue;
        canCallEmergency = CustomOptionHolder.swapperCanCallEmergency.GetBool();
        canOnlySwapOthers = CustomOptionHolder.swapperCanOnlySwapOthers.GetBool();
        canFixSabotages = CustomOptionHolder.swapperCanFixSabotages.GetBool();
        charges = Mathf.RoundToInt(CustomOptionHolder.swapperSwapsNumber.GetFloat());
        rechargeTasksNumber = Mathf.RoundToInt(CustomOptionHolder.swapperRechargeTasksNumber.GetFloat());
        rechargedTasks = Mathf.RoundToInt(CustomOptionHolder.swapperRechargeTasksNumber.GetFloat());
    }
}
