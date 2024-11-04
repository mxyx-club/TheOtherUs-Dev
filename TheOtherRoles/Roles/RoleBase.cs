using System;
using System.Collections.Generic;
using AmongUs.GameOptions;
using Il2CppSystem.Xml;
using UnityEngine;

namespace TheOtherRoles.Roles;

public abstract class RoleBase : IDisposable
{
    public PlayerControl Player { get; set; }
    public string RoleName { get; set; }
    public RoleId RoleId { get; set; }
    public int ConfigId { get; set; }
    public virtual bool CanUseVent { get; set; }
    public virtual bool HasImpVision { get; set; }
    public virtual bool IsKiller { get; set; }
    public virtual bool IsEvil { get; set; }
    public virtual bool CanAssign { get; set; }

    protected bool hasTasks;

    public RoleBase(PlayerControl player, RoleInfos RoleInfo, bool? hasTasks = null)
    {
        Player = player;
        RoleName = RoleInfo.Name;
        RoleId = RoleInfo.RoleId;
        ConfigId = RoleInfo.ConfigId;
        this.hasTasks = hasTasks ?? (RoleInfo.RoleTeam == RoleType.Crewmate);
        RoleBaseManager.AllActiveRoles.Add(Player.PlayerId, this);
    }

    /// <summary>
    /// 销毁玩家实例
    /// </summary>
    public void Dispose()
    {
        OnDestroy();
        RoleBaseManager.AllActiveRoles.Remove(Player.PlayerId);
        Player = null;
    }
    /// <summary>
    /// 按钮
    /// </summary>
    /// <param name="_hudManager"></param>
    public virtual void ButtonCreate(HudManager _hudManager) { }

    /// <summary>
    /// 重置按钮
    /// </summary>
    public virtual void ResetCustomButton() { }

    /// <summary>
    /// 销毁职业实例时调用
    /// </summary>
    public virtual void OnDestroy() { }

    /// <summary>
    /// 游戏开始后会立刻调用
    /// </summary>
    public virtual void OnGameStart() { }

    /// <summary>
    /// 每帧调用的函数
    /// </summary>
    public virtual void FixedUpdate() { }

    /// <summary>
    /// 报告时调用的函数
    /// </summary>
    public virtual void OnReportDeadBody(PlayerControl reporter, GameData.PlayerInfo target) { }
    /// <summary>
    /// 会议开始时调用的函数
    /// </summary>
    public virtual void OnMeetingStart(MeetingHud __instance) { }

    /// <summary>
    /// 会议结束驱逐玩家时调用的函数
    /// </summary>
    public virtual void OnExileBegin(GameData.PlayerInfo exiled) { }

    /// <summary>
    /// 驱逐结束后调用的函数
    /// </summary>
    /// <param name="exiled"></param>
    public virtual void OnExileWrapUp(GameData.PlayerInfo exiled) { }

    /// <summary>
    /// 击杀时调用的函数
    /// </summary>
    /// <param name="target"></param>
    public virtual void OnKill(PlayerControl target) { }

    /// <summary>
    /// 被击杀时调用的函数
    /// </summary>
    /// <param name="killer"></param>
    public virtual void OnDeath(PlayerControl killer = null) { }

    /// <summary>
    /// 重置职业
    /// </summary>
    public abstract void ResetRoles();
}

public class RoleInfos
{
    private static readonly List<RoleInfos> _AllRoleInfo = [];

    public RoleInfos(
        string NameKey,
        Color Color,
        RoleId RoleId,
        RoleType roleTeam,
        Func<PlayerControl, RoleBase> createInstance,
        OptionCreatorDelegate optionCreator,
        int ConfigId
        )
    {
        this.NameKey = NameKey;
        this.Color = Color;
        this.RoleId = RoleId;
        this.ConfigId = ConfigId;
        RoleTeam = roleTeam;
        CreateRoleInstance = createInstance;
        OptionCreator = optionCreator;
        _AllRoleInfo.Add(this);
    }

    public static IReadOnlyList<RoleInfos> AllRoleInfo => _AllRoleInfo;

    public Color Color { get; set; }
    public string NameKey { get; set; }
    public RoleId RoleId { get; set; }
    public string DescriptionText { get; set; } = string.Empty;
    public string IntroInfo { get; set; } = string.Empty;
    public RoleType RoleTeam { get; set; }
    public int ConfigId;
    public OptionCreatorDelegate OptionCreator;
    public Func<PlayerControl, RoleBase> CreateRoleInstance;

    public string Name => NameKey.Translate();

    public string IntroDescription => getString(NameKey + "IntroDesc");
    public string ShortDescription => getString(NameKey + "ShortDesc");
    public string FullDescription => getString(NameKey + "FullDesc");

    public static RoleInfos Create(
        string NameKey,
        RoleId roleId,
        RoleType roleTeam,
        Color color,
        Func<PlayerControl, RoleBase> createInstance,
        OptionCreatorDelegate optionCreator,
        int configId
        )
    {
        var roleInfo = new RoleInfos(NameKey, color, roleId, roleTeam, createInstance, optionCreator, configId);
        return roleInfo;
    }

    public delegate void OptionCreatorDelegate();
}
