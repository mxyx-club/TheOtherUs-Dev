using System;
using System.Collections.Generic;
using UnityEngine;
using static TheOtherRoles.Options.CustomOption;

namespace TheOtherRoles.Roles;

public class RoleInfos
{
    public static IReadOnlyList<RoleInfos> AllRoleInfo => _AllRoleInfo;
    private static readonly List<RoleInfos> _AllRoleInfo = new();

    public Type RoleObjectType { get; }
    public string RoleObjectTypeName { get; }
    public string NameKey { get; }
    public Color Color { get; }
    public RoleId RoleId { get; }
    public RoleType RoleType { get; }
    public int ConfigId { get; }
    public bool CanAssign { get; }
    public int MaxPlayer { get; }
    public bool IsHidden { get; }
    public int AssignSelection => RoleOption?.GetSelection() ?? 0;
    public int PlayerCount => PlayerCountOption?.GetInt() ?? 0;
    public Func<PlayerControl, RoleBase> CreateRoleInstance { get; }
    public CustomOptionType OptionType { get; }
    public string Name => NameKey.Translate();

    public CustomOption RoleOption { get; private set; }
    public CustomOption PlayerCountOption { get; private set; }
    public Action OptionCreater;


    public string IntroDescription => GetString(NameKey + "IntroDesc");
    public string ShortDescription => GetString(NameKey + "ShortDesc");
    public string FullDescription => GetString(NameKey + "FullDesc");

    public RoleInfos(
        Type RoleObjectType,
        Func<PlayerControl, RoleBase> CreateRoleInstance,
        RoleId RoleId,
        RoleType Type,
        string NameKey,
        Color32 Color,
        int ConfigId,
        Action OptionCreator = null,
        bool CanAssign = true,
        bool Hidden = false,
        int MaxPlayer = 15
        )
    {
        this.RoleObjectType = RoleObjectType;
        this.Color = Color;
        this.RoleId = RoleId;
        this.NameKey = NameKey;
        this.ConfigId = ConfigId;
        this.RoleType = this.RoleType;
        this.CreateRoleInstance = CreateRoleInstance;
        this.OptionCreater = OptionCreator;
        this.CanAssign = CanAssign;
        this.IsHidden = Hidden;
        this.MaxPlayer = MaxPlayer;
        OptionType = this.RoleType switch
        {
            RoleType.Impostor => CustomOptionType.Impostor,
            RoleType.Crewmate => CustomOptionType.Crewmate,
            RoleType.Neutral => CustomOptionType.Neutral,
            RoleType.Modifier => CustomOptionType.Modifier,
            _ => CustomOptionType.General
        };
        _AllRoleInfo.Add(this);
        CustomRoleManager.AllRolesInfo.Add(RoleId, this);
    }

    public RoleBase CreateInstance(PlayerControl player)
    {
        if (CreateRoleInstance != null)
            return CreateRoleInstance(player);
        return Activator.CreateInstance(RoleObjectType, player as object) as RoleBase;
    }

    public void CreateOption()
    {
        if (RoleOption != null || !CanAssign) return;

        RoleOption = Create(ConfigId, OptionType, cs(Color, NameKey), CustomOptionHolder.rates, null, true);
        if (MaxPlayer > 1) PlayerCountOption = Create(ConfigId + 1, OptionType, "RolesCount", 1, 1, MaxPlayer, 1, RoleOption);

        OptionCreater?.Invoke();

        CustomRoleSpawnChances.Add(RoleOption.RoleId, RoleOption);
        if (MaxPlayer > 1) CustomRoleCounts.Add(RoleOption.RoleId, PlayerCountOption);
    }
}
