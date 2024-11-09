using Il2CppSystem.Text;
using Reactor.Utilities.Extensions;
using UnityEngine;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

namespace TheOtherRoles.Patches.CursedTasks;

internal class DivertPower
{
    private static bool _isIntermission;
    private static readonly int Outline = Shader.PropertyToID("_Outline");
    private static PlayerTask[] PlayerTasksArray => PlayerControl.LocalPlayer.myTasks?.ToArray();

    [HarmonyPatch(typeof(ShipStatus))]
    private class ShipStatusPatch
    {
        [HarmonyPatch(nameof(ShipStatus.Start))]
        [HarmonyPrefix]
        private static void StartPrefix()
        {
            _isIntermission = false;
        }
    }

    [HarmonyPatch(typeof(NormalPlayerTask))]
    private static class NormalPlayerTaskPatch
    {
        [HarmonyPatch(nameof(NormalPlayerTask.NextStep))]
        [HarmonyPostfix]
        private static void NextStepPostfix(NormalPlayerTask __instance)
        {
            if (!ModOption.CursedTasks) return;
            if (__instance.TaskType != TaskTypes.DivertPower) return;

            if (PlayerTasksArray.Count(x => x.TaskType ==
                TaskTypes.DivertPower && !x.IsComplete) == 0)
            {
                _isIntermission = false;
            }

            if (__instance.taskStep == __instance.MaxStep)
            {
                var arrowParent = __instance.Arrow.transform.parent;
                for (var i = 0; i < arrowParent.childCount; i++)
                {
                    if (arrowParent.GetChild(i))
                        Object.Destroy(arrowParent.GetChild(i).gameObject);
                }

                return;
            }

            for (var i = 0; i < 500; i++)
            {
                _isIntermission = true;
                var arrowObject =
                    Object.Instantiate(__instance.Arrow.gameObject, __instance.Arrow.transform.parent);
                var arrowBehavior = arrowObject.GetComponent<ArrowBehaviour>();
                arrowObject.GetComponent<SpriteRenderer>().color = RandomColor();
                arrowBehavior.target = new Vector2(Random.RandomRange(-30f, 30f), Random.RandomRange(-30f, 30f));
            }
        }
    }
    public static Color RandomColor()
    {
        return new Color(Random.value, Random.value, Random.value, 1.0f);
    }


    [HarmonyPatch(typeof(DivertPowerTask))]
    private static class DivertPowerTaskPatch
    {
        [HarmonyPatch(nameof(DivertPowerTask.AppendTaskText))]
        [HarmonyPrefix]
        private static bool AppendTaskTextPrefix(DivertPowerTask __instance, StringBuilder sb)
        {
            if (!ModOption.CursedTasks) return true;
            var divertLocation = DestroyableSingleton<TranslationController>.Instance
                .GetString(__instance.StartAt);
            switch (__instance.TaskStep)
            {
                case 0:
                    _ = sb.AppendLine($"{divertLocation}: {DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.DivertPower)} (0/2)");
                    break;
                case 1:
                    _ = sb.AppendLine($"<color=yellow>???????: {DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.AcceptDivertedPower)} (1/2)</color>");
                    break;
                case 2:
                    return true;
                default:
                    _ = sb.AppendLine($"{divertLocation}: {DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.DivertPower)} (0/2)");
                    break;
            }

            return false;
        }
    }

    [HarmonyPatch(typeof(DivertPowerMinigame))]
    private static class DivertPowerMinigamePatch
    {
        [HarmonyPatch(nameof(DivertPowerMinigame.Begin))]
        [HarmonyPrefix]
        private static void BeginPrefix(DivertPowerMinigame __instance)
        {
            if (!ModOption.CursedTasks) return;
            System.Random random = new();
            __instance.SliderOrder = __instance.SliderOrder
                .OrderBy(x => random.Next()).ToArray();
        }
    }

    [HarmonyPatch(typeof(MapTaskOverlay))]
    private static class MapBehaviourPatch
    {
        [HarmonyPatch(nameof(MapTaskOverlay.Show))]
        [HarmonyPostfix]
        private static void ShowPostfix(MapTaskOverlay __instance)
        {
            if (!ModOption.CursedTasks) return;
            var divertTasks = PlayerTasksArray.Count(x => x.TaskType is TaskTypes.DivertPower);
            if (!_isIntermission)
            {
                Debug("Checking child count");
                if (__instance.transform.childCount <= 100) return;
                Debug("Attempting to destroy children");

                for (var i = 0; i < __instance.transform.childCount; i++)
                {
                    var child = __instance.transform.GetChild(i);
                    if (!child || !child.name.StartsWith("Divert") || !child.name.Contains("Power")) continue;
                    child.gameObject.Destroy();
                }

                Debug("Children destroyed successfully (and legally)");

                return;
            }

            GameObject powerIndicator = default;
            var mapIcons = __instance.transform;

            if (mapIcons.childCount > 100)
            {
                for (var i = 0; i < mapIcons.childCount; i++)
                {
                    var child = mapIcons.GetChild(i);
                    if (!child || !child.name.StartsWith("Divert") || !child.name.Contains("Power")) continue;
                    child.GetComponent<SpriteRenderer>().material.SetFloat(Outline, 1f);
                }
                return;
            }

            for (var i = 0; i < mapIcons.childCount; i++)
            {
                var child = mapIcons.GetChild(i);
                if (!child || !child.name.StartsWith("Divert") || !child.name.Contains("Power")) continue;
                powerIndicator = child.gameObject;
                break;
            }

            if (powerIndicator && powerIndicator.Equals(default)) return;
            (float x1, float x2, float y1, float y2)[] mapBounds =
            {
                (-6.1f, 5f, -4.5f, 2f), (-4f, 7.4f, -1.3f, 5.5f), (0f, 0f, 0f, 0f), (-6.1f, 5f, -4.5f, 2f),
                (-4.5f, 6.5f, -3.5f, 3.2f)
            };

            var mapBound = mapBounds[
                AmongUsClient.Instance.InOnlineScene
                    ? GameOptionsManager.Instance.currentNormalGameOptions.MapId
                    : AmongUsClient.Instance.TutorialMapId];

            for (var i = 0; i < 250 * divertTasks; i++)
            {
                var iconObject = Object.Instantiate(powerIndicator, mapIcons);
                iconObject.active = true;
                iconObject.transform.localPosition = new Vector3(Random.RandomRange(mapBound.x1, mapBound.x2),
                    Random.RandomRange(mapBound.y1, mapBound.y2), iconObject.transform.localPosition.z);
            }
        }
    }
}