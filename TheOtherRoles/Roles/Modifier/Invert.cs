using System.Collections.Generic;

namespace TheOtherRoles.Roles.Modifier;

public static class Invert
{
    public static List<PlayerControl> invert = new();
    public static int meetings = 3;

    public static void clearAndReload()
    {
        invert.Clear();
        meetings = (int)CustomOptionHolder.modifierInvertDuration.GetFloat();
    }
}
