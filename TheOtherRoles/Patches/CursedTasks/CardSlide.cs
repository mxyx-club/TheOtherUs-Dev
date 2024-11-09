using UnityEngine;
using Random = UnityEngine.Random;

namespace TheOtherRoles.Patches.CursedTasks
{
    internal static class CustomCardSwipe
    {
        private static readonly bool PrevState = false;

        [HarmonyPatch(typeof(CardSlideGame))]
        private static class CardSlidePatch
        {
            [HarmonyPatch(nameof(CardSlideGame.Begin))]
            [HarmonyPrefix]
            private static void BeginPrefix(CardSlideGame __instance)
            {
                if (!ModOption.CursedTasks) return;
                __instance.AcceptedTime = new FloatRange(0.5f, 0.5f);
            }

            [HarmonyPatch(nameof(CardSlideGame.Update))]
            [HarmonyPrefix]
            private static void PutCardBackPrefix(CardSlideGame __instance)
            {
                if (!ModOption.CursedTasks) return;
                var CurrentState = __instance.redLight.color == Color.red;
                if (PrevState == CurrentState || !CurrentState) return;
                var randomNumber = Random.RandomRangeInt(0, 40);
                if (randomNumber == 0) __instance.AcceptedTime = new FloatRange(0.25f, 2f);
                else __instance.AcceptedTime = new FloatRange(0.495f, 0.505f);
            }
        }
    }
}