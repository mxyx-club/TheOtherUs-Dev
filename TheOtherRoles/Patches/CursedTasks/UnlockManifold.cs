namespace TheOtherRoles.Patches.CursedTasks;

[HarmonyPatch(typeof(UnlockManifoldsMinigame))]
internal static class UnlockManifold
{
    [HarmonyPatch(nameof(UnlockManifoldsMinigame.Begin))]
    [HarmonyPrefix]
    private static void BeginPrefix(UnlockManifoldsMinigame __instance)
    {
        if (!ModOption.CursedTasks) return;
        foreach (var button in __instance.Buttons)
            button.sprite = UnityHelper.loadSpriteFromResources("TheOtherRoles.Resources.CursedTasks.UnlockManifold.png", 100);
    }
}