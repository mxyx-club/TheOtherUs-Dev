using System;
using System.Collections.Generic;

namespace TheOtherRoles.Roles;

public static class CustomRoleManager
{
    public static PlayerData<RoleBase> PlayerRoles { get; private set; } = new();
    //public static Dictionary<string, HashSet<RoleBase>> RoleBaseTypes { get; private set; } = new();
    //public static readonly Dictionary<RoleId, RoleBase> RoleClass = [];
    public static Dictionary<byte, RoleBase> AllActiveRoles = new();
    public static readonly Dictionary<RoleId, RoleInfos> AllRolesInfo = new();

    public static RoleInfos GetRoleInfo(this RoleId role) => AllRolesInfo.ContainsKey(role) ? AllRolesInfo[role] : null;
    public static RoleBase GetRoleClass(this PlayerControl player) => GetRoleBaseByPlayerId(player.PlayerId);
    public static RoleBase GetRoleBaseByPlayerId(byte playerId) => AllActiveRoles.TryGetValue(playerId, out var roleBase) ? roleBase : null;

    /// <summary>
    /// 其他玩家视角下的帧 Task 处理事件
    /// 用于干涉其他职业
    /// </summary>
    public static HashSet<Action<PlayerControl>> OnFixedUpdateOthers = new();

    public static void OnFixedUpdate(PlayerControl player)
    {
        if (IsInTask)
        {
            var roleclass = player.GetRoleClass();

            player.GetRoleClass()?.OnFixedUpdate(player);

            //その他視点処理があれば実行
            foreach (var onFixedUpdate in OnFixedUpdateOthers)
            {
                onFixedUpdate(player);
            }
        }
    }

    public static RoleBase SetRole(PlayerControl player, RoleId role)
    {
        RoleInfos roleInfo = GetRoleInfo(role);
        if (roleInfo == null) return null;
        return roleInfo.CreateInstance(player);
    }
    public static bool IsRole(this PlayerControl p, RoleId role)
    {
        return p.GetRole() == role;
    }

    public static RoleId GetRole(this PlayerControl player)
    {
        //ロルベの場合はロルベのロールを返す
        RoleBase roleBase = player.GetRoleBase();
        return roleBase?.RoleId ?? RoleId.Default;
    }

    public static RoleBase GetRoleBase(this PlayerControl player)
    {
        return PlayerRoles[player];
    }

    /// <summary>
    /// 全部对象的销毁事件
    /// </summary>
    public static void Dispose()
    {
        Info($"Dispose ActiveRoles");
        OnFixedUpdateOthers.Clear();

        AllActiveRoles.Values.ToArray().Do(roleClass => roleClass.Dispose());
    }
}