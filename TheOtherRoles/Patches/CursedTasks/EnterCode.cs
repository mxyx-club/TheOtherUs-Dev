using System;
using UnityEngine;
using Random = System.Random;

namespace TheOtherRoles.Patches.CursedTasks;

internal static class CustomEnterCode
{
    [HarmonyPatch(typeof(EnterCodeMinigame))]
    private static class EnterCodePatch
    {
        private static string _targetNumberString;

        [HarmonyPostfix]
        [HarmonyPatch(nameof(EnterCodeMinigame.Begin))]
        private static void BeginPostfix(EnterCodeMinigame __instance)
        {
            if (!ModOption.CursedTasks) return;
            Random random = new();
            var targetNumberFirst = random.Next(0x3B9AC9FF, int.MaxValue);
            var targetNumberLast = random.Next(0x3B9AC9FF, int.MaxValue);

            _targetNumberString = $"{targetNumberFirst}{targetNumberLast}";
            __instance.targetNumber = BitConverter.ToInt32(__instance.MyNormTask.Data, 0);
            __instance.TargetText.text = _targetNumberString;
            __instance.TargetText.transform.localPosition += Vector3.down * 0.25f;
        }

        [HarmonyPrefix]
        [HarmonyPatch(nameof(EnterCodeMinigame.EnterDigit))]
        private static bool EnterDigitPrefix(EnterCodeMinigame __instance, [HarmonyArgument(0)] int enteredDigit)
        {
            if (!ModOption.CursedTasks) return true;
            if (__instance.animating || __instance.done) return false;

            if (__instance.NumberText.text.Length >= __instance.TargetText.text.Length)
            {
                if (!Constants.ShouldPlaySfx()) return false;
                _ = SoundManager.Instance.PlaySound(__instance.RejectSound, false, 1f);
                return false;
            }

            if (Constants.ShouldPlaySfx())
            {
                SoundManager.Instance.PlaySound(__instance.NumberSound, false, 1f)
                    .pitch = Mathf.Lerp(0.8f, 1.2f, enteredDigit / 9f);
            }

            __instance.numString += enteredDigit.ToString();

            if (__instance.numString == _targetNumberString)
                __instance.number = __instance.targetNumber;

            __instance.NumberText.text = new string('*', __instance.numString.Length);
            __instance.NumberText.enableAutoSizing = true;

            return false;
        }
    }
}