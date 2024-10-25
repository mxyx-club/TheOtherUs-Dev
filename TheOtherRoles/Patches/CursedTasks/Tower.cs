using UnityEngine;
namespace TheOtherRoles.Patches.CursedTasks;

internal class Towel
{
    [HarmonyPatch(typeof(TowelMinigame))]
    private static class TowelMinigamePatch
    {
        [HarmonyPatch(nameof(TowelMinigame.Begin)), HarmonyPostfix]
        private static void BeginPostfix(TowelMinigame __instance)
        {
            if (!ModOption.CursedTasks) return;
            var copytowel = Object.Instantiate(__instance.Towels[0]);
            copytowel.gameObject.SetActive(false);
            var scale = copytowel.transform.localScale;
            scale.x -= 0.1f;
            scale.y -= 0.1f;
            copytowel.transform.localScale = scale;
            foreach (var towel in __instance.Towels)
                Object.Destroy(towel.gameObject);
            __instance.Towels = new Collider2D[25];
            for (var i = 0; i < 25; i++)
            {
                var towel = Object.Instantiate(copytowel);
                towel.transform.SetParent(__instance.transform);
                towel.transform.position = __instance.transform.position + new Vector3(Random.Range(-0.5f, 0.5f), Random.Range(-0.5f, 0.5f));
                towel.name = $"towel_towel(Clone) {i}";
                __instance.Towels[i] = towel;
                towel.gameObject.SetActive(true);
            }
            Object.Destroy(copytowel.gameObject);

        }
    }


}
