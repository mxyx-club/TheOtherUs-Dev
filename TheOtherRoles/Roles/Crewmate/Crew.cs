using RewiredConsts;
using UnityEngine;

namespace TheOtherRoles.Roles.Crewmate;

public class Crew : RoleBase
{
    public static PlayerControl crew;
    public static Color color = Palette.White;

    public static readonly RoleInfos RoleInfo = RoleInfos.Create(
        "Crewmate",
        RoleId.Crew,
        RoleType.Crewmate,
        Palette.CrewmateBlue,
        player => new Crew(player),
        AddOptions,
        10000
    );

    public Crew(PlayerControl crew) : base(crew, RoleInfo) { }

    public static void clearAndReload()
    {
        crew = null;
    }

    private static void AddOptions()
    {
    }

    public override void ResetRoles()
    {
    }
}
