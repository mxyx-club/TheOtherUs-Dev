using UnityEngine;

namespace TheOtherRoles.Roles.Vanilla;
public class Impostor : RoleBase
{
    public static Color color = Palette.ImpostorRed;

    public static readonly RoleInfos RoleInfo = new(
        typeof(Balancer),
        (p) => new Impostor(p),
        RoleId.Impostor,
        RoleType.Impostor,
        "Impostor",
        color,
        60000,
        null,
        false
    );

    public Impostor(PlayerControl p) : base(p, RoleInfo) { }

    public override void ResetRoles()
    {
    }
}
