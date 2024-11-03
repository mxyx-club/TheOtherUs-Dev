using System;
using TheOtherRoles.Utilities;
using Random = System.Random;

namespace TheOtherRoles.Roles;

public static class RoleHelpers
{
    public static bool CanMultipleShots(PlayerControl dyingTarget)
    {
        if (dyingTarget == CachedPlayer.LocalPlayer.PlayerControl)
            return false;

        if (ModOption.gameMode != CustomGamemodes.Guesser)
        {
            if (PlayerControl.LocalPlayer == Vigilante.vigilante
                && HandleGuesser.remainingShots(CachedPlayer.LocalPlayer.PlayerId) > 0
                && Vigilante.hasMultipleShotsPerMeeting) return true;
            else if (Assassin.assassin.Any(x => x == PlayerControl.LocalPlayer)
                && HandleGuesser.remainingShots(CachedPlayer.LocalPlayer.PlayerId) > 0
                && Assassin.assassinMultipleShotsPerMeeting) return true;
        }

        else if (HandleGuesser.isGuesser(CachedPlayer.LocalPlayer.PlayerId)
            && HandleGuesser.remainingShots(CachedPlayer.LocalPlayer.PlayerId) > 0
            && HandleGuesser.hasMultipleShotsPerMeeting) return true;

        return CachedPlayer.LocalPlayer.PlayerControl == Doomsayer.doomsayer && Doomsayer.hasMultipleShotsPerMeeting &&
               Doomsayer.CanShoot;
    }

}