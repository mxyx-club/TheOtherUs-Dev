namespace TheOtherRoles.Roles.Modifier;

public static class Giant
{
    public static PlayerControl giant;
    public static float speed = 0.72f;
    public static float size = 1.08f;

    public static void clearAndReload()
    {
        giant = null;
        speed = CustomOptionHolder.modifierGiantSpped.GetFloat();
    }
}
