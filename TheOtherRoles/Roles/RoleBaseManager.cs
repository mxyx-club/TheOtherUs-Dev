
using System.Collections.Generic;
using System.Linq;

namespace TheOtherRoles.Roles;

public class RoleBaseManager
{

    public static Dictionary<byte, RoleBase> AllActiveRoles = new();

    public static void AddRole(byte playerId, RoleBase role)
    {
        if (!AllActiveRoles.ContainsKey(playerId))
        {
            AllActiveRoles.Add(playerId, role);
        }
    }

    public static void RemoveRole(byte playerId)
    {
        if (AllActiveRoles.ContainsKey(playerId))
        {
            AllActiveRoles[playerId].Dispose();
            AllActiveRoles.Remove(playerId);
        }
    }

    public static bool SwapRoles(byte playerIdA, byte playerIdB)
    {
        if (!AllActiveRoles.ContainsKey(playerIdA) || !AllActiveRoles.ContainsKey(playerIdB))
        {
            return false; // 如果任意一个玩家没有角色，无法交换
        }

        // 获取两个玩家的角色
        var roleA = AllActiveRoles[playerIdA];
        var roleB = AllActiveRoles[playerIdB];

        // 交换角色的Player属性
        (roleB.Player, roleA.Player) = (roleA.Player, roleB.Player);

        // 更新字典中的引用
        AllActiveRoles[playerIdA] = roleB;
        AllActiveRoles[playerIdB] = roleA;

        return true;
    }

    public static RoleBase GetRole(byte playerId)
    {
        return AllActiveRoles.TryGetValue(playerId, out var role) ? role : null;
    }

    public static void ClearRoles()
    {
        foreach (var role in AllActiveRoles.Values)
        {
            role.Dispose();
        }
        AllActiveRoles.Clear();
    }

}