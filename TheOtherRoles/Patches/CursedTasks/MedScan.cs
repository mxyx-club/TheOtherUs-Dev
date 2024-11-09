/*using System;

namespace TheOtherRoles.Patches.CursedTasks;

internal class CursedMedScan
{
    public static (string id, int bloodType) PlayerData;

    [HarmonyPatch(typeof(MedScanMinigame))]
    private static class MedScanMinigamePatch
    {
        [HarmonyPatch(nameof(MedScanMinigame.Begin))]
        [HarmonyPostfix]
        private static void BeginPostfix(MedScanMinigame __instance)
        {
            if (!ModOption.CursedTasks) return;
            if (PlayerData == default)
            {
                for (var i = 0; i < 6; i++)
                {
                    var id = new Random().Next(0, int.MaxValue);
                    PlayerData.id += id.ToString("X").PadLeft(8, '0');
                }

                PlayerData.bloodType = new Random().Next(0, 8);
            }

            __instance.completeString =
               "Player Identity: " + PlayerControl.LocalPlayer.Data.ColorName + " Player " + PlayerData.id + "\nIdentification Number: " + PlayerData.id + "\nPlayer Name: " + PlayerControl.LocalPlayer.cosmetics.nameText.text + "\nHeight: 3 feet, 6 inches" + "\nWeight: 92 pounds" + "\nColor: " + $"{PlayerControl.LocalPlayer.Data.ColorName} " + "\nBlood Type: " + MedScanMinigame.BloodTypes[PlayerData.bloodType];
            __instance.ScanDuration = 20f;
        }
    }

    [HarmonyPatch(typeof(ShipStatus))]
    private static class ShipStatusPatch
    {
        [HarmonyPatch(nameof(ShipStatus.Start))]
        [HarmonyPrefix]
        private static void StartPrefix()
        {
            if (!ModOption.CursedTasks) return;
            PlayerData = default;
        }
    }
}*/