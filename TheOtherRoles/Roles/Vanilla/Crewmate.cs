using UnityEngine;

namespace TheOtherRoles.Roles.Vanilla;

public class Crewmate : RoleBase
{
    public static Color color = Palette.CrewmateBlue;

    public static readonly RoleInfos RoleInfo = new(
        typeof(Balancer),
        (p) => new Crewmate(p),
        RoleId.Crewmate,
        RoleType.Crewmate,
        "Crewmate",
        color,
        10000,
        null,
        false
    );

    public Crewmate(PlayerControl p) : base(p, RoleInfo) { }
}
