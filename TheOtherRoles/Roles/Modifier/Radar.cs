﻿using System.Collections.Generic;
using TheOtherRoles.Objects;
using UnityEngine;
using Object = UnityEngine.Object;

namespace TheOtherRoles.Roles.Modifier;

public static class Radar
{
    public static PlayerControl radar;
    public static List<Arrow> localArrows = new();
    public static PlayerControl ClosestPlayer;
    public static Color color = new Color32(255, 0, 128, byte.MaxValue);

    public static void clearAndReload()
    {
        radar = null;
        if (localArrows != null)
            foreach (var arrow in localArrows)
                if (arrow?.arrow != null)
                    Object.Destroy(arrow.arrow);
        localArrows.Clear();
    }
}
