using UnityEngine;
namespace TheOtherRoles.Patches.CursedTasks
{
    internal class CaliDistributar
    {
        [HarmonyPatch(typeof(SweepMinigame))]
        private static class SweepMinigamePatch
        {
            [HarmonyPatch(nameof(SweepMinigame.FixedUpdate)), HarmonyPrefix]
            private static void FixedUpdatePrefix(SweepMinigame __instance)
            {
                if (!ModOption.CursedTasks) return;
                var numer = new System.Random();
                float num = numer.Next(10, 50);
                __instance.timer += Time.fixedDeltaTime * num;
            }
        }
    }
}
