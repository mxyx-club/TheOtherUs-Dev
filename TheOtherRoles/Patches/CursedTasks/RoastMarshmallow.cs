namespace TheOtherRoles.Patches.CursedTasks;

internal class RoastMarshmallowFire
{
    [HarmonyPatch(typeof(RoastMarshmallowFireMinigame))]
    private static class RoastMarshmallowFireMinigamePatch
    {
        [HarmonyPatch(nameof(RoastMarshmallowFireMinigame.Begin)), HarmonyPrefix]

        private static void BeginPrefix(RoastMarshmallowFireMinigame __instance)
        {
            if (!ModOption.CursedTasks) return;
            var rd = new System.Random();
            var timetoast = (float)rd.Next(120, 300);
            __instance.timeToToasted = timetoast;
        }
    }


}
