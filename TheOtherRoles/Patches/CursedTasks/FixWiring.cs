using UnityEngine;
using Object = UnityEngine.Object;

namespace TheOtherRoles.Patches.CursedTasks;

internal class CursedFixWiring
{
    public static int WiresNum;

    public static int NumWires = 4;

    public static float ScalarY = 1f;

    [HarmonyPatch(typeof(ShipStatus))]
    private class ShipStatusPatch
    {
        [HarmonyPatch(nameof(ShipStatus.Start))]
        [HarmonyPrefix]
        private static void StartPrefix()
        {
            WiresNum = 0;
        }
    }

    [HarmonyPatch(typeof(WireMinigame))]
    internal class WireMinigamePatch
    {
        [HarmonyPatch(nameof(WireMinigame.Begin))]
        [HarmonyPrefix]
        private static void BeginPrefix(WireMinigame __instance)
        {
            if (!ModOption.CursedTasks) return;
            var WiresOrder = new int[3] { 8, 16, 70 };
            NumWires = WiresOrder[WiresNum];
            ScalarY = NumWires < 12 ? 1f : 8f / NumWires + 0.3f;
            var ParentAll = GameObject.Find("Main Camera/WireMinigame(Clone)").transform;
            __instance.ExpectedWires = new sbyte[NumWires];
            WireMinigame.colors = new Color[NumWires];
            __instance.Symbols = new Sprite[NumWires];
            __instance.ActualWires = new sbyte[NumWires];
            __instance.LeftLights = new SpriteRenderer[NumWires];
            __instance.RightLights = new SpriteRenderer[NumWires];
            __instance.LeftNodes = new Wire[NumWires];
            __instance.RightNodes = new WireNode[NumWires];
            var ParentLeftNode = ParentAll.FindChild("LeftWires").transform;
            Helpers.DestroyObjects(ParentLeftNode);
            var positionY = 2.25f;
            for (var i = 0; i < NumWires; i++)
            {
                var newGameObject = Helpers.BuildWire(ParentLeftNode.FindChild("LeftWireNode").gameObject,
                    ref positionY);
                for (var j = 0; j < newGameObject.transform.childCount; j++)
                    newGameObject.transform.GetChild(j).localScale = new Vector3(1f, ScalarY, 1f);
                var headTransform = newGameObject.transform.FindChild("Head");
                headTransform.localPosition = new Vector3(0.235f, headTransform.localPosition.y,
                    headTransform.localPosition.z);
                headTransform.GetComponent<CircleCollider2D>().enabled = true;
                headTransform.GetComponent<CircleCollider2D>().radius = 1.5f / NumWires + 0.1f;
                var wireComponent = newGameObject.GetComponent<Wire>();
                wireComponent.enabled = true;
                __instance.LeftNodes[i] = wireComponent;
            }

            var ParentRightNode = ParentAll.FindChild("RightWires").transform;
            Helpers.DestroyObjects(ParentRightNode);
            positionY = 2.25f;
            for (var i = 0; i < NumWires; i++)
            {
                var newGameObject = Helpers.BuildWire(ParentRightNode.FindChild("RightWireNode").gameObject,
                    ref positionY);
                var headTransform = newGameObject.transform.FindChild("electricity_wiresBase1");
                newGameObject.transform.localScale = new Vector3(1f, ScalarY, 1f);
                headTransform.localPosition = new Vector3(0.145f, 0f, headTransform.localPosition.z);
                newGameObject.transform.GetComponent<CircleCollider2D>().enabled = true;
                newGameObject.GetComponent<CircleCollider2D>().radius = 0.45f;
                var wireComponent = newGameObject.GetComponent<WireNode>();
                wireComponent.enabled = true;
                __instance.RightNodes[i] = wireComponent;
            }

            ParentAll.FindChild("LeftLights").gameObject.active = false;
            ParentAll.FindChild("RightLights").gameObject.active = false;
            for (var i = 0; i < NumWires; i++)
            {
                __instance.ExpectedWires[i] = (sbyte)i;
                WireMinigame.colors[i] = Color.HSVToRGB((float)i / NumWires, 1f, 1f);
                __instance.ActualWires[i] = -1;
                __instance.Symbols[i] = new Sprite();
            }
        }

        [HarmonyPatch(nameof(WireMinigame.UpdateLights))]
        [HarmonyPrefix]
        private static bool SetColorPrefix()
        {
            return !ModOption.CursedTasks;
        }

        [HarmonyPatch(nameof(WireMinigame.CheckTask))]
        [HarmonyPrefix]
        private static void CheckTaskPrefix(WireMinigame __instance)
        {
            if (!ModOption.CursedTasks) return;
            var flag = true;
            for (var i = 0; i < __instance.ActualWires.Length; i++)
                if (__instance.ActualWires[i] != __instance.ExpectedWires[i])
                {
                    flag = false;
                    break;
                }

            if (flag) WiresNum++;
        }

        [HarmonyPatch(nameof(WireMinigame.CheckRightSide))]
        [HarmonyPrefix]
        private static bool CheckRightSidePrefix(WireMinigame __instance, ref WireNode __result, Vector2 pos)
        {
            if (!ModOption.CursedTasks) return true;
            var leftNode = __instance.myController.amTouching;
            int leftId = leftNode.transform.parent.GetComponent<Wire>().WireId;
            for (var i = 0; i < __instance.RightNodes.Length; i++)
            {
                var wireNode = __instance.RightNodes[i];
                if (wireNode.hitbox.OverlapPoint(pos) && __instance.ExpectedWires[leftId] == wireNode.WireId)
                    __result = wireNode;
            }

            if (!__result) __result = null;
            return false;
        }
    }

    [HarmonyPatch(typeof(Wire))]
    internal static class WirePatch
    {
        [HarmonyPatch(nameof(Wire.ResetLine))]
        [HarmonyPostfix]
        private static void ResetLinePostfix(Wire __instance, [HarmonyArgument(1)] bool reset)
        {
            if (!ModOption.CursedTasks) return;
            if (reset)
            {
                __instance.ColorBase.transform.localScale = new Vector3(5f, ScalarY, 1f);
                __instance.ColorBase.transform.localPosition = new Vector3(-0.3f, 0f, 1f);
                return;
            }

            __instance.ColorBase.transform.localScale = new Vector3(7.8f, ScalarY, 1f);
            __instance.ColorBase.transform.localPosition = new Vector3(-0.22f, 0f, 1f);
            __instance.Liner.transform.localScale =
                new Vector3(__instance.Liner.transform.localScale.x, ScalarY, 1f);
        }
    }

    internal class Helpers
    {
        public static GameObject BuildWire(GameObject prefab, ref float positionY)
        {
            positionY -= 4.6f / (NumWires + 1);
            var newGameObject = Object.Instantiate(prefab, prefab.transform.parent);
            newGameObject.transform.localPosition = new Vector3(newGameObject.transform.localPosition.x, positionY,
                newGameObject.transform.localPosition.z);
            newGameObject.transform.FindChild("BaseSymbol").gameObject.active = false;
            return newGameObject;
        }

        public static void DestroyObjects(Transform parent)
        {
            for (var i = 0; i < parent.childCount; i++)
            {
                var childNode = parent.GetChild(i).gameObject;
                if (!parent.GetChild(i).gameObject.name.Contains("WireNode")) continue;
                Object.Destroy(childNode);
            }
        }
    }
}