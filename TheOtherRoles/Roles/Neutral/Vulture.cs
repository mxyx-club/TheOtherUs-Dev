using System.Collections.Generic;
using TheOtherRoles.Objects;
using UnityEngine;
using Object = UnityEngine.Object;

namespace TheOtherRoles.Roles.Neutral;

public static class Vulture
{
    public static PlayerControl vulture;
    public static Color color = new Color32(139, 69, 19, byte.MaxValue);
    public static List<Arrow> localArrows = new();
    public static float cooldown = 30f;
    public static int vultureNumberToWin = 4;
    public static int eatenBodies;
    public static bool triggerVultureWin;
    public static bool canUseVents = true;
    public static bool showArrows = true;
    public static ResourceSprite buttonSprite = new("VultureButton.png");

    public static void clearAndReload()
    {
        vulture = null;
        vultureNumberToWin = Mathf.RoundToInt(CustomOptionHolder.vultureNumberToWin.GetFloat());
        eatenBodies = 0;
        cooldown = CustomOptionHolder.vultureCooldown.GetFloat();
        triggerVultureWin = false;
        canUseVents = CustomOptionHolder.vultureCanUseVents.GetBool();
        showArrows = CustomOptionHolder.vultureShowArrows.GetBool();
        if (localArrows != null)
            foreach (var arrow in localArrows)
                if (arrow?.arrow != null)
                    Object.Destroy(arrow.arrow);
        localArrows.Clear();
    }
}
