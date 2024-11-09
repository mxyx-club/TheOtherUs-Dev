
namespace TheOtherRoles.Patches;

[HarmonyPatch(typeof(AmongUsClient), nameof(AmongUsClient.CoStartGame))]
public class GameStartedPatch
{
    public static void Postfix(AmongUsClient __instance)
    {
        GameHistory.Clear();
        CustomRoleManager.OnFixedUpdateOthers.Clear();
        CustomOption.CustomRoleSpawnChances.TryGetValue(RoleId.Balancer, out var value);
        Message(value?.GetSelection().ToString() ?? "0", "GameStarted");
        //Message(RoleManagerSelectRolesPatch.GetPlayerCount(RoleId.Balancer).ToString() ?? "0", "GameStarted");
    }
}
