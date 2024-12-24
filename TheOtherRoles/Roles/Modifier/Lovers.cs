using UnityEngine;

namespace TheOtherRoles.Roles.Modifier;

public static class Lovers
{
    public static PlayerControl lover1;
    public static PlayerControl lover2;
    public static Color color = new Color32(232, 57, 185, byte.MaxValue);

    public static bool bothDie = true;

    public static bool enableChat = true;

    // Lovers save if next to be exiled is a lover, because RPC of ending game comes before RPC of exiled
    public static bool notAckedExiledIsLover;

    public static bool isLover(this PlayerControl player)
    {
        return player != null && (player == lover1 || player == lover2);
    }

    public static bool existing()
    {
        return lover1 != null && lover2 != null && !lover1.Data.Disconnected && !lover2.Data.Disconnected;
    }

    public static bool existingAndAlive()
    {
        return existing() && !lover1.Data.IsDead && !lover2.Data.IsDead &&
               !notAckedExiledIsLover; // ADD NOT ACKED IS LOVER
    }

    public static PlayerControl otherLover(PlayerControl player)
    {
        if (!existing() || player == null) return null;
        if (player == lover1) return lover2;
        if (player == lover2) return lover1;
        return null;
    }

    public static bool existingWithKiller()
    {
        return existing() && (lover1.isKiller() || lover2.isKiller());
    }

    public static bool hasAliveKillingLover(this PlayerControl player)
    {
        if (!existingAndAlive() || !existingWithKiller())
            return false;
        return player != null && (player == lover1 || player == lover2);
    }

    public static void clearAndReload()
    {
        lover1 = null;
        lover2 = null;
        notAckedExiledIsLover = false;
        bothDie = CustomOptionHolder.modifierLoverBothDie.GetBool();
        enableChat = CustomOptionHolder.modifierLoverEnableChat.GetBool();
    }
}
