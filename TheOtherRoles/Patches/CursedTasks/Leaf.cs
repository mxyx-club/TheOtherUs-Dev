using UnityEngine;
using Object = UnityEngine.Object;

namespace TheOtherRoles.Patches.CursedTasks;

internal class CursedLeaf
{
    [HarmonyPatch(typeof(LeafMinigame))]
    private static class LeafMinigamePatch
    {
        [HarmonyPatch(nameof(LeafMinigame.Begin))]
        [HarmonyPostfix]
        private static void BeginPostfix(LeafMinigame __instance)
        {
            if (!ModOption.CursedTasks) return;
            var leavesNum = 550;
            __instance.MyNormTask.taskStep = 0;
            __instance.MyNormTask.MaxStep = leavesNum;
            var TaskParent = __instance.transform.parent;
            for (var i = 0; i < TaskParent.childCount; i++)
            {
                var child = TaskParent.GetChild(i);
                if (child.name == "o2_leaf1(Clone)") Object.Destroy(child);
            }

            __instance.Leaves = new Collider2D[leavesNum];
            for (var i = 0; i < leavesNum; i++)
            {
                var leafBehaviour = Object.Instantiate(__instance.LeafPrefab);
                leafBehaviour.transform.SetParent(__instance.transform);
                leafBehaviour.Parent = __instance;
                var localPosition = __instance.ValidArea.Next();
                leafBehaviour.transform.localPosition = new Vector3(localPosition.x, localPosition.y, -1);
                __instance.Leaves[i] = leafBehaviour.GetComponent<Collider2D>();
            }

            GameObject pointer = new("cursor");
            pointer.transform.SetParent(__instance.transform);
            pointer.layer = 4;
            var collider2D = pointer.AddComponent<CircleCollider2D>();
            collider2D.radius = 1f;
        }

        [HarmonyPatch(nameof(LeafMinigame.FixedUpdate))]
        [HarmonyPostfix]
        private static void FixedUpdatePostfix(LeafMinigame __instance)
        {
            if (!ModOption.CursedTasks) return;
            __instance.transform.FindChild("cursor").position = __instance.myController.HoverPosition;
        }
    }
}