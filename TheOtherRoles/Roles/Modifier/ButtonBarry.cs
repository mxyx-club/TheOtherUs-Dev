namespace TheOtherRoles.Roles.Modifier;

public static class ButtonBarry
{
    public static PlayerControl buttonBarry;
    public static int remoteMeetingsLeft = 1;
    public static bool SabotageRemoteMeetings;

    public static ResourceSprite buttonSprite = new("EmergencyButton.png", 550);

    public static void clearAndReload()
    {
        buttonBarry = null;
        remoteMeetingsLeft = 1;

        //SabotageRemoteMeetings = false;
        SabotageRemoteMeetings = CustomOptionHolder.modifierButtonSabotageRemoteMeetings.GetBool();
    }
}
