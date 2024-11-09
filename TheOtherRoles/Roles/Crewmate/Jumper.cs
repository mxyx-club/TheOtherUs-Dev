using UnityEngine;

namespace TheOtherRoles.Roles.Crewmate;

public static class Jumper
{
    public static PlayerControl jumper;
    public static Color color = new Color32(204, 155, 20, byte.MaxValue); // mint

    public static float JumpTime = 30f;
    public static float ChargesOnPlace = 1f;

    public static bool resetPlaceAfterMeeting;

    public static float ChargesGainOnMeeting = 2f;
    public static float MaxCharges = 3f;
    public static float Charges = 1f;

    public static Vector3 jumpLocation;

    public static ResourceSprite jumpMarkButtonSprite = new("JumperMarkButton.png");
    public static ResourceSprite jumpJumpButtonSprite = new("JumperJumpButton.png");
    public static bool usedPlace;

    public static void resetPlaces()
    {
        Charges = Mathf.RoundToInt(ChargesOnPlace);
        jumpLocation = Vector3.zero;
        usedPlace = false;
    }

    public static void clearAndReload()
    {
        resetPlaces();
        jumpLocation = Vector3.zero;
        jumper = null;
        resetPlaceAfterMeeting = CustomOptionHolder.jumperResetPlaceAfterMeeting.GetBool();
        Charges = CustomOptionHolder.jumperMaxCharges.GetFloat();
        JumpTime = CustomOptionHolder.jumperJumpTime.GetFloat();
        ChargesOnPlace = 1f;
        ChargesGainOnMeeting = CustomOptionHolder.jumperChargesGainOnMeeting.GetFloat();
        MaxCharges = CustomOptionHolder.jumperMaxCharges.GetFloat();
        usedPlace = false;
    }
}
