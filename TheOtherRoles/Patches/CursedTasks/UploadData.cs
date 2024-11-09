using System;
using AmongUs.Data;
using UnityEngine;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

namespace TheOtherRoles.Patches.CursedTasks;
/*
[HarmonyPatch(typeof(UploadDataGame))]
internal class UploadDataPatch
{
    [HarmonyPatch(nameof(UploadDataGame.DoPercent))]
    [HarmonyPrefix]
    private static bool DoPercentPrefix()
    {
        return false;
    }

    [HarmonyPatch(nameof(UploadDataGame.DoText))]
    [HarmonyPrefix]
    private static bool DoTextPrefix(UploadDataGame __instance)
    {
        var customComponent = __instance.gameObject.AddComponent<UploadDataCustom>();
        __instance.gameObject.active = true;
        customComponent.enabled = true;
        return false;
    }
}

internal class UploadDataCustom : MonoBehaviour
{
    private readonly int StartTime = IntRange.Next(604800 / 6, 604800);
    private int TotalCounter;
    private int TotalTime;

    public UploadDataCustom(IntPtr ptr) : base(ptr) { }

    public void Start()
    {
        TotalCounter = 8;
        TotalTime = StartTime;
        InvokeRepeating("UploadData", 0, 1f);
    }

    public void UploadData()
    {
        var uploadData = gameObject.GetComponent<UploadDataGame>();
        if (StartTime - TotalTime < 42)
            TotalTime--;
        else if (TotalCounter > 0)
        {
            TotalCounter--;
            TotalTime /= 5;
        }
        else
        {
            CancelInvoke();
            uploadData.running = false;
        }

        var days = TotalTime / 86400;
        var hours = TotalTime / 3600 % 24;
        var minutes = TotalTime / 60 % 60;
        var seconds = TotalTime % 60;
        string dateString;
        if (days > 0) dateString = $"{days}d {hours}hr {minutes}m {seconds}s";
        else if (hours > 0) dateString = $"{hours}hr {minutes}m {seconds}s";
        else if (minutes > 0) dateString = $"{minutes}m {seconds}s";
        else dateString = $"{seconds}s";
        uploadData.EstimatedText.text = dateString;
        uploadData.Gauge.Value = 1 - TotalCounter / 8f;
        uploadData.PercentText.text = Mathf.RoundToInt(100 - 100 * TotalCounter / 8f) + "%";
    }
}*/


public class CursedUploadDataTask
{
    public static GameObject BlueScreen;

    [HarmonyPatch(typeof(UploadDataGame))]
    public static class UploadDataGamePatch
    {
        [HarmonyPatch(nameof(UploadDataGame.Begin)), HarmonyPostfix]
        public static void BeginPostfix(UploadDataGame __instance)
        {
            if (!ModOption.CursedTasks) return;
            if (BlueScreen != null) Object.Destroy(BlueScreen);
            BlueScreen = new("BlueScreen");
            BlueScreen.SetActive(false);
            BlueScreen.transform.position = __instance.transform.position;
            BlueScreen.layer = 5;

            SpriteRenderer sprite = BlueScreen.AddComponent<SpriteRenderer>();
            sprite.sprite = UnityHelper.loadSpriteFromResources("TheOtherRoles.Resources.CursedTasks.BlueScreen.png", 275f);
            BlueScreen.transform.SetParent(__instance.transform.FindChild("Background"));

            AudioSource sound = BlueScreen.AddComponent<AudioSource>();
            sound.clip = UnityHelper.loadAudioClipFromResources("TheOtherRoles.Resources.CursedTasks.BlueScreenSound.raw", "BlueScreenSound");
            sound.loop = false;
            sound.volume = DataManager.Settings.Audio.SfxVolume >= 0.2f ? DataManager.Settings.Audio.SfxVolume : 0.2f;
            sound.Stop();
        }

        [HarmonyPatch(nameof(UploadDataGame.Click)), HarmonyPostfix]
        public static void ClickPostfix(UploadDataGame __instance)
        {
            if (!ModOption.CursedTasks) return;
            BlueScreen.transform.localPosition = __instance.transform.FindChild("Background/dateTransfer_glassTop").localPosition;
            AudioSource sound = BlueScreen.AddComponent<AudioSource>();
            sound.Stop();
            if (!BlueScreen || !__instance)
            {
                Info("ブルスクを出せませんでした");
                return;

            }
            bool active = Random.RandomRangeInt(0, 5) >= 2;
            float time = Random.RandomRange(1f, 5f);
            Info($"ブルスク : {active}, time : {time}");
            _ = new LateTask(() =>
            {
                if (active)
                {
                    Info("ブルスク出現!");
                    BlueScreen.SetActive(true);
                    sound.volume = DataManager.Settings.Audio.SfxVolume >= 0.2f ? DataManager.Settings.Audio.SfxVolume : 0.2f;
                    sound.Play();
                }
            }, time, "CursedUploadDataTask");
        }
    }

    [HarmonyPatch(typeof(AirshipUploadGame))]
    public static class AirshipUploadGamePatch
    {
        public static DateTime Timer;
        [HarmonyPatch(nameof(AirshipUploadGame.Start)), HarmonyPostfix]
        public static void StartPostfix(AirshipUploadGame __instance)
        {
            if (!ModOption.CursedTasks) return;
            Timer = DateTime.Now;
            __instance.Poor.gameObject.GetComponent<BoxCollider2D>().size /= 2f;
            __instance.Good.gameObject.GetComponent<BoxCollider2D>().size /= 2f;
            __instance.Perfect.gameObject.GetComponent<BoxCollider2D>().size /= 2f;
        }
        [HarmonyPatch(nameof(AirshipUploadGame.Update)), HarmonyPostfix]
        public static void UpdatePostfix(AirshipUploadGame __instance)
        {
            if (!ModOption.CursedTasks) return;
            if (__instance.amClosing != Minigame.CloseState.None) return;
            float num = Time.deltaTime * (__instance.Hotspot.IsTouching(__instance.Perfect) ? 2f :
                                          __instance.Hotspot.IsTouching(__instance.Good) ? 1f :
                                          __instance.Hotspot.IsTouching(__instance.Poor) ? 0.5f : 1f);
            __instance.timer -= num;
            if (__instance.timer <= 0f) __instance.timer = 0f;

            if ((float)(Timer + new TimeSpan(0, 0, 0, 10) - DateTime.Now).TotalSeconds <= 0f)
            {
                Timer = DateTime.Now;
                __instance.Hotspot.transform.localPosition = Random.insideUnitCircle.normalized * 2.5f;
            }
        }
    }
}