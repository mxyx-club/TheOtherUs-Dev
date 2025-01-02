namespace TheOtherRoles.Roles.Modifier;

public static class Giant
{
    public static PlayerControl giant;
    public static float speed = 0.72f;
    public static readonly float size = 1.05f;

    public static void clearAndReload()
    {
        giant = null;
        speed = CustomOptionHolder.modifierGiantSpped.GetFloat();
    }
}
