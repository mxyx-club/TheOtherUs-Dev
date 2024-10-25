using System;
using Il2CppSystem.Text;

namespace TheOtherRoles.Patches.CursedTasks;

internal class CursedSample
{
    [HarmonyPatch(typeof(SampleMinigame))]
    private static class SampleMinigamePatch
    {
        [HarmonyPostfix]
        [HarmonyPatch(nameof(SampleMinigame.Begin))]
        private static void BeginPostfix(SampleMinigame __instance)
        {
            if (!ModOption.CursedTasks) return;
            SampleMinigame.ProcessingStrings = new StringNames[]
            {
                   StringNames.DoSomethingElse, StringNames.DoSomethingElse,
            };

            __instance.TimePerStep = 86400f;
        }
    }

    [HarmonyPatch(typeof(NormalPlayerTask))]
    private static class NormalPlayerTaskPatch
    {
        [HarmonyPrefix]
        [HarmonyPatch(nameof(NormalPlayerTask.AppendTaskText))]
        private static bool BeginPrefix(NormalPlayerTask __instance, [HarmonyArgument(0)] StringBuilder sb)
        {
            if (!ModOption.CursedTasks) return true;
            if (__instance.TaskType != TaskTypes.InspectSample) return true;
            if (!__instance.ShowTaskTimer || __instance.TimerStarted != NormalPlayerTask.TimerState.Started)
                return true;

            var startAt = DestroyableSingleton<TranslationController>.Instance.GetString(__instance.StartAt);
            var taskType = DestroyableSingleton<TranslationController>.Instance.GetString(__instance.TaskType);

            var time = TimeSpan.FromSeconds((int)__instance.TaskTimer);

            var painfulCounter = (int)__instance.TaskTimer switch
            {
                >= 3600 => $"{time.Hours}h {time.Seconds}s",
                >= 60 => $"{time.Minutes}m {time.Seconds}s",
                _ => $"{time.Seconds}s"
            };

            _ = sb.AppendLine($"<color=yellow>{startAt}: {taskType} " +
                              $"({painfulCounter})</color>");

            return false;
        }

        [HarmonyPostfix]
        [HarmonyPatch(nameof(NormalPlayerTask.FixedUpdate))]
        private static void FixedUpdatePostfix(NormalPlayerTask __instance)
        {
            if (!ModOption.CursedTasks) return;
            if (__instance.TaskType != TaskTypes.InspectSample) return;

            __instance.TaskTimer -= (int)__instance.TaskTimer switch
            {
                >= 3455 => 0,
                >= 2600 => 1.8f,
                >= 2400 => 2.2f,
                >= 1700 => 2.7f,
                >= 1000 => 3.4f,
                >= 15 => 3.7f,
                _ => 0
            };
        }
    }
}