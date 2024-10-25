namespace TheOtherRoles.Patches.CursedTasks;

internal class CollectShells
{
    [HarmonyPatch(typeof(CollectShellsMinigame))]
    private static class CollectShellsMinigameMinigamePatch
    {
        [HarmonyPatch(nameof(CollectShellsMinigame.Begin)), HarmonyPrefix]

        private static void BeginPrefix(CollectShellsMinigame __instance)
        {
            if (!ModOption.CursedTasks) return;
            var Rd = new System.Random();

            __instance.numShellsRange = (IntRange)Rd.Next(4, 20);

        }
    }


}
