using System.Collections.Generic;
using UnityEngine;

namespace TheOtherRoles.Roles.Crewmate;

public static class Trapper
{
    public static PlayerControl trapper;
    public static Color color = new Color32(110, 57, 105, byte.MaxValue);

    public static float cooldown = 30f;
    public static int maxCharges = 5;
    public static int rechargeTasksNumber = 3;
    public static int rechargedTasks = 3;
    public static int charges = 1;
    public static int trapCountToReveal = 2;
    public static List<PlayerControl> playersOnMap = new();
    public static bool anonymousMap;
    public static int infoType; // 0 = Role, 1 = Good/Evil, 2 = Name
    public static float trapDuration = 5f;

    public static ResourceSprite trapButtonSprite = new("Trapper_Place_Button.png");

    public static void clearAndReload()
    {
        trapper = null;
        cooldown = CustomOptionHolder.trapperCooldown.GetFloat();
        maxCharges = Mathf.RoundToInt(CustomOptionHolder.trapperMaxCharges.GetFloat());
        rechargeTasksNumber = Mathf.RoundToInt(CustomOptionHolder.trapperRechargeTasksNumber.GetFloat());
        rechargedTasks = Mathf.RoundToInt(CustomOptionHolder.trapperRechargeTasksNumber.GetFloat());
        charges = Mathf.RoundToInt(CustomOptionHolder.trapperMaxCharges.GetFloat()) / 2;
        trapCountToReveal = Mathf.RoundToInt(CustomOptionHolder.trapperTrapNeededTriggerToReveal.GetFloat());
        playersOnMap = new List<PlayerControl>();
        anonymousMap = CustomOptionHolder.trapperAnonymousMap.GetBool();
        infoType = CustomOptionHolder.trapperInfoType.GetSelection();
        trapDuration = CustomOptionHolder.trapperTrapDuration.GetFloat();
    }
}
