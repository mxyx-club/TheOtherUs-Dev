using UnityEngine;

namespace TheOtherRoles.Roles.Impostor;

public static class Undertaker
{
    public static PlayerControl undertaker;
    public static Color color = Palette.ImpostorRed;

    public static float dragingDelaiAfterKill;

    public static bool isDraging;
    public static DeadBody deadBodyDraged;
    public static bool canDragAndVent;

    public static float velocity = 1;

    public static ResourceSprite buttonSprite = new("UndertakerDragButton.png");

    public static void clearAndReload()
    {
        undertaker = null;
        isDraging = false;
        canDragAndVent = CustomOptionHolder.undertakerCanDragAndVent.GetBool();
        deadBodyDraged = null;
        velocity = CustomOptionHolder.undertakerDragingAfterVelocity.GetFloat();
        dragingDelaiAfterKill = CustomOptionHolder.undertakerDragingDelaiAfterKill.GetFloat();
    }
}
