using System;

namespace TheOtherRoles.Roles;

public abstract class RoleBase : IDisposable
{
    public PlayerControl Player { get; set; }
    public string RoleName { get; set; }
    public RoleId RoleId { get; set; }
    public virtual bool CanUseVent { get; set; }
    public virtual bool HasImpVision { get; set; }
    public virtual bool IsKiller { get; set; }
    public virtual bool IsEvil { get; set; }
    public RoleInfos Roleinfo { get; }
    public RoleType RoleType { get; }

    protected bool hasTasks;

    public static int MaxInstanceId = int.MinValue;
    private int InstanceId { get; }
    public RoleBase(PlayerControl player, RoleInfos roleInfo, bool? hasTasks = null)
    {
        InstanceId = MaxInstanceId;
        MaxInstanceId++;
        Player = player;
        RoleType = roleInfo.RoleType;
        Roleinfo = roleInfo;
        RoleName = roleInfo.NameKey;
        RoleId = roleInfo.RoleId;
        this.hasTasks = hasTasks ?? (roleInfo.RoleType == RoleType.Crewmate);
        CustomRoleManager.AllActiveRoles.Add(player.PlayerId, this);
    }

    /// <summary>
    /// 销毁玩家实例
    /// </summary>
    public void Dispose()
    {
        OnDestroy();
        Player = null;
    }

    public void SetPlayer(PlayerControl player)
    {
        Player = player;
    }

    public override bool Equals(object obj)
    {
        if (obj is not RoleBase)
            return false;
        return this.GetHashCode() == obj.GetHashCode();
    }

    public override int GetHashCode()
    {
        return InstanceId;
    }

    /// <summary>
    /// 重置职业
    /// </summary>
    public virtual void ResetRoles() { }

    /// <summary>
    /// 按钮
    /// </summary>
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
    /// 帧 Task 处理函数，仅当前玩家调用
    /// 需要所有玩家调用时注册CustomRoleManager.OnFixedUpdateOthers
    /// </summary>
    /// <param name="player">目标玩家</param>
    public virtual void OnFixedUpdate(PlayerControl player) { }

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
    /// 玩家死亡时调用的函数
    /// 无论死亡玩家是谁，所有玩家都会调用，所以需要判断死亡玩家的身份
    /// </summary>
    /// <param name="player">死亡玩家</param>
    /// <param name="deathReason">死亡原因</param>
    /// <param name="isOnMeeting">是否在会议中死亡</param>
    public virtual void OnPlayerDeath(PlayerControl player, CustomDeathReason deathReason, bool isOnMeeting = false) { }
}
