﻿using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace TheOtherRoles.Roles.Neutral;

public static class Doomsayer
{
    public static PlayerControl doomsayer;
    public static Color color = new Color32(0, 255, 128, byte.MaxValue);

    public static PlayerControl currentTarget;
    public static float cooldown = 30f;
    public static int formationNum = 1;
    public static bool hasMultipleShotsPerMeeting;
    public static bool canGuessNeutral;
    public static bool canGuessImpostor;
    public static bool triggerDoomsayerrWin;
    public static bool canGuess = true;
    public static bool onlineTarger;
    public static float killToWin = 3;
    public static float killedToWin;
    public static bool CanShoot = true;

    public static ResourceSprite buttonSprite = new("SeerButton.png");

    public static string GetInfo(PlayerControl target)
    {
        try
        {
            var allRoleInfo = (onlineTarger ? onlineRoleInfos() : allRoleInfos()).OrderBy(_ => rnd.Next()).ToList();
            var roleInfoTarget = RoleInfo.getRoleInfoForPlayer(target, false).FirstOrDefault();
            var AllMessage = new List<string>();
            allRoleInfo.Remove(RoleInfo.doomsayer);
            allRoleInfo.Remove(roleInfoTarget);

            if (allRoleInfo.Count < formationNum + 2)
                return $"There are fewer than {formationNum + 2} players.\n玩家人数不足 {formationNum + 2} 无法揭示。";

            var formation = formationNum;
            var x = rnd.Next(0, formation);
            var message = new StringBuilder();
            var tempNumList = Enumerable.Range(0, allRoleInfo.Count).ToList();
            var temp = (tempNumList.Count > formation ? tempNumList.Take(formation) : tempNumList).OrderBy(_ => rnd.Next()).ToList();

            message.AppendLine($"{target.Data.PlayerName} 的职业可能是：\n");

            for (int num = 0, tempNum = 0; num < formation; num++, tempNum++)
            {
                var info = allRoleInfo[temp[tempNum]];

                message.Append(num == x ? roleInfoTarget.Name : info.Name);
                message.Append(num < formation - 1 ? ", " : ';');
            }

            AllMessage.Add(message.ToString());

            return $"{message}";
        }
        catch
        {
            return "Doomsayer Error\n末日预言家揭示出错";
        }
    }

    public static void clearAndReload()
    {
        doomsayer = null;
        currentTarget = null;
        killedToWin = 0;
        canGuess = true;
        triggerDoomsayerrWin = false;
        cooldown = CustomOptionHolder.doomsayerCooldown.GetFloat();
        hasMultipleShotsPerMeeting = CustomOptionHolder.doomsayerHasMultipleShotsPerMeeting.GetBool();
        canGuessNeutral = CustomOptionHolder.doomsayerCanGuessNeutral.GetBool();
        canGuessImpostor = CustomOptionHolder.doomsayerCanGuessImpostor.GetBool();
        formationNum = CustomOptionHolder.doomsayerDormationNum.GetInt();
        killToWin = CustomOptionHolder.doomsayerKillToWin.GetFloat();
        onlineTarger = CustomOptionHolder.doomsayerOnlineTarger.GetBool();
    }
}
