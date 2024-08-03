﻿using System;
using System.Collections.Generic;
using System.Linq;
using Hazel;
using TheOtherRoles.Objects;
using TheOtherRoles.Utilities;
using UnityEngine;
using Random = System.Random;

namespace TheOtherRoles.Modules;

[HarmonyPatch(typeof(KeyboardJoystick), nameof(KeyboardJoystick.Update))]
public class CommandHandler
{
    //private static readonly string passwordHash = "d1f51dfdfd8d38027fd2ca9dfeb299399b5bdee58e6c0b3b5e9a45cd4e502848";
    private static readonly Random random = new((int)DateTime.Now.Ticks);
    private static readonly List<PlayerControl> bots = new();
    private static void Postfix(KeyboardJoystick __instance)
    {
        if (AmongUsClient.Instance && (AmongUsClient.Instance.AmHost || MapOption.DebugMode))
        {
            /*
            // 生成假人
            if (Input.GetKey(KeyCode.LeftControl) && Input.GetKey(KeyCode.F) && Input.GetKeyDown(KeyCode.Return))
            {
                var playerControl = UnityEngine.Object.Instantiate(AmongUsClient.Instance.PlayerPrefab);
                _ = playerControl.PlayerId = (byte)GameData.Instance.GetAvailableId();

                bots.Add(playerControl);
                GameData.Instance.AddPlayer(playerControl, new InnerNet.ClientData(0));
                AmongUsClient.Instance.Spawn(playerControl);

                playerControl.transform.position = CachedPlayer.LocalPlayer.transform.position;
                playerControl.GetComponent<DummyBehaviour>().enabled = true;
                playerControl.NetTransform.enabled = false;
                playerControl.SetName(RandomString(6));
                playerControl.SetColor((byte)random.Next(Palette.PlayerColors.Length));
                playerControl.Data.RpcSetTasks(Array.Empty<byte>());
            }*/
            // 结束游戏
            if (InGame && Input.GetKey(KeyCode.L) && Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.Return))
            {
                var writer = AmongUsClient.Instance.StartRpcImmediately(CachedPlayer.LocalPlayer.PlayerControl.NetId,
                    (byte)CustomRPC.ForceEnd, SendOption.Reliable);
                AmongUsClient.Instance.FinishRpcImmediately(writer);
                RPCProcedure.forceEnd();
            }
            // 强制开始会议或结束会议
            if (Input.GetKey(KeyCode.M) && Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.Return))
            {
                if (IsMeeting) MeetingHud.Instance.RpcClose();
                else CachedPlayer.LocalPlayer.PlayerControl.NoCheckStartMeeting(null, true);
            }
            // 强制结束游戏
            if (Input.GetKey(KeyCode.LeftControl) && Input.GetKey(KeyCode.T) && Input.GetKeyDown(KeyCode.Return) && InGame)
            {
                MapOption.isCanceled = true;
            }
            if (Input.GetKey(KeyCode.LeftControl) && Input.GetKey(KeyCode.V) && Input.GetKeyDown(KeyCode.Return) && InGame)
            {
                CustomButton.resetKillButton(CachedPlayer.LocalPlayer.PlayerControl, 0.5f);
            }
            // 快速开始游戏
            if (Input.GetKeyDown(KeyCode.LeftShift) && IsCountDown)
            {
                GameStartManager.Instance.countDownTimer = 0;
            }
        }
    }
    public static string RandomString(int length)
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        return new string(Enumerable.Repeat(chars, length).Select(s => s[random.Next(s.Length)]).ToArray());
    }
}
