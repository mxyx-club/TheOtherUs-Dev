using Object = UnityEngine.Object;

namespace TheOtherRoles.Patches.CursedTasks
{
    public class CursedVentCleaningTask
    {
        [HarmonyPatch(typeof(VentCleaningMinigame))]
        public static class VentCleaningMinigamePatch
        {
            [HarmonyPatch(nameof(VentCleaningMinigame.Begin)), HarmonyPostfix]
            public static void BeginPostfix(VentCleaningMinigame __instance)
            {

                if (!ModOption.CursedTasks) return;
                var TaskParent = __instance.transform.parent;
                for (var i = 0; i < TaskParent.childCount; i++)
                {
                    var child = TaskParent.GetChild(i);
                    if (child.name == "VentDirt(Clone)") Object.Destroy(child);
                }
                var Rd = new System.Random();
                __instance.numberOfDirts = Rd.Next(500, 800);
                for (var i = 0; i < __instance.numberOfDirts; i++) __instance.SpawnDirt();
            }
        }
    }
}