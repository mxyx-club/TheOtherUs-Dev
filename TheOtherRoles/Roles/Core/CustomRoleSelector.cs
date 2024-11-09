
using System;
using System.Collections.Generic;
using AmongUs.GameOptions;

namespace TheOtherRoles.Roles.Core;



internal static class CustomRoleSelector
{
    public static Dictionary<PlayerControl, RoleId> RoleResult;
    public static IReadOnlyList<RoleId> AllRoles => RoleResult.Values.ToList();

    private static void SelectHiddenImpRoles()
    { }
    public static void SelectCustomRoles()
    {
        int optImpNum = ModOption.NumImpostors;
        int optNeutralNum;
        int optNKNum;
        int optCrewNum;

        int playerCount = Main.AllAlivePlayerControls.Count();

        int neutralMin = CustomOptionHolder.neutralRolesCountMin.GetSelection();
        int neutralMax = CustomOptionHolder.neutralRolesCountMax.GetSelection();
        int killerNeutralMin = CustomOptionHolder.killerNeutralRolesCountMin.GetSelection();
        int killerNeutralMax = CustomOptionHolder.killerNeutralRolesCountMax.GetSelection();

        neutralMin = Math.Max(0, Math.Min(neutralMin, neutralMax));
        killerNeutralMin = Math.Max(0, Math.Min(killerNeutralMin, killerNeutralMax));

        optNeutralNum = (neutralMin < neutralMax) ? rnd.Next(neutralMin, neutralMax + 1) : neutralMin;
        optNKNum = (killerNeutralMin < killerNeutralMax) ? rnd.Next(killerNeutralMin, killerNeutralMax + 1) : killerNeutralMin;

        optNKNum = Math.Min(optNKNum, optNeutralNum);

        optCrewNum = playerCount - optNeutralNum - optImpNum;

        if (killerNeutralMin + killerNeutralMax > 0) optNeutralNum = Math.Min(optNeutralNum - optNKNum, neutralMax);

        int readyRoleTotalNum = 0;
        List<RoleId> rolesToAssign = new();
        List<RoleId> roleList = new();

        List<RoleId> crewOnList = new();
        List<RoleId> crewRateList = new();

        List<RoleId> ImpOnList = new();
        List<RoleId> ImpRateList = new();

        int readyNKNum = 0;
        List<RoleId> NKOnList = new();
        List<RoleId> NKRateList = new();

        int readyNeutralNum = 0;
        List<RoleId> NeutralOnList = new();
        List<RoleId> NeutralRateList = new();

        RoleResult = new();

        // 在职业列表中搜索职业
        foreach (var cr in Enum.GetValues(typeof(RoleId)))
        {
            RoleId role = (RoleId)Enum.Parse(typeof(RoleId), cr.ToString());
            if (role is RoleId.GM) continue;
            for (int i = 0; i < role.GetAssignCount(); i++)
                roleList.Add(role);
        }

        // 职业设置为：优先
        foreach (var role in roleList.Where(x => x.GetRoleCount() == 10))
        {
            if (role.IsImpostor()) ImpOnList.Add(role);
            else if (role.IsKillerNeutral()) NKOnList.Add(role);
            else if (role.IsNeutral()) NeutralOnList.Add(role);
            else crewOnList.Add(role);
        }
        // 职业设置为：启用
        foreach (var role in roleList.Where(x => x.GetRoleCount() is > 0 and < 10))
        {
            if (role.IsImpostor()) ImpRateList.Add(role);
            else if (role.IsKillerNeutral()) NKRateList.Add(role);
            else if (role.IsNeutral()) NeutralRateList.Add(role);
            else crewRateList.Add(role);
        }

        // 抽取优先职业（内鬼）
        while (ImpOnList.Count > 0)
        {
            if (readyRoleTotalNum >= optImpNum) break;
            var select = ImpOnList[rnd.Next(0, ImpOnList.Count)];
            ImpOnList.Remove(select);

            rolesToAssign.Add(select);

            readyRoleTotalNum++;
            Info(select.ToString() + " 加入内鬼职业待选列表（优先）", "CustomRoleSelector");
            if (readyRoleTotalNum >= playerCount) goto EndOfAssign;
            if (readyRoleTotalNum >= optImpNum) break;
        }
        // 优先职业不足以分配，开始分配启用的职业（内鬼）
        if (readyRoleTotalNum < playerCount && readyRoleTotalNum < optImpNum)
        {
            while (ImpRateList.Count > 0)
            {
                if (readyRoleTotalNum >= optImpNum) break;
                var select = ImpRateList[rnd.Next(0, ImpRateList.Count)];
                ImpRateList.Remove(select);
                rolesToAssign.Add(select);

                readyRoleTotalNum++;
                Info(select.ToString() + " 加入内鬼职业待选列表", "CustomRoleSelector");
                if (readyRoleTotalNum >= playerCount) goto EndOfAssign;
                if (readyRoleTotalNum >= optImpNum) break;
            }
        }
        // 抽取优先职业（中立杀手）
        while (NKOnList.Count > 0 && optNKNum > 0)
        {
            var select = NKOnList[rnd.Next(0, NKOnList.Count)];

            NKOnList.Remove(select);

            rolesToAssign.Add(select);
            readyRoleTotalNum++;
            readyNKNum += select.GetAssignCount();
            Info(select.ToString() + " 加入中立职业待选列表（优先）", "CustomRoleSelector");
            if (readyRoleTotalNum >= playerCount) goto EndOfAssign;
            if (readyNKNum >= optNKNum) break;
        }
        // 优先职业不足以分配，开始分配启用的职业（中立杀手）
        if (readyRoleTotalNum < playerCount && readyNKNum < optNKNum)
        {
            while (NKRateList.Count > 0 && optNKNum > 0)
            {
                var select = NKRateList[rnd.Next(0, NKRateList.Count)];

                NKRateList.Remove(select);

                rolesToAssign.Add(select);
                readyRoleTotalNum++;
                readyNKNum += select.GetAssignCount();
                Info(select.ToString() + " 加入中立职业待选列表", "CustomRoleSelector");
                if (readyRoleTotalNum >= playerCount) goto EndOfAssign;
                if (readyNKNum >= optNKNum) break;
            }
        }
        // 抽取优先职业（中立）
        while (NeutralOnList.Count > 0 && optNeutralNum > 0)
        {
            var select = NeutralOnList[rnd.Next(0, NeutralOnList.Count)];
            NeutralOnList.Remove(select);
            rolesToAssign.Add(select);
            readyRoleTotalNum++;
            readyNeutralNum += select.GetAssignCount();
            Info(select.ToString() + " 加入中立职业待选列表（优先）", "CustomRoleSelector");
            if (readyRoleTotalNum >= playerCount) goto EndOfAssign;
            if (readyNeutralNum >= optNeutralNum) break;
        }

        // 优先职业不足以分配，开始分配启用的职业（中立）
        if (readyRoleTotalNum < playerCount && readyNeutralNum < optNeutralNum)
        {
            while (NeutralRateList.Count > 0 && optNeutralNum > 0)
            {
                var select = NeutralRateList[rnd.Next(0, NeutralRateList.Count)];
                NeutralRateList.Remove(select);
                rolesToAssign.Add(select);
                readyRoleTotalNum++;
                readyNeutralNum += select.GetAssignCount();
                Info(select.ToString() + " 加入中立职业待选列表", "CustomRoleSelector");
                if (readyRoleTotalNum >= playerCount) goto EndOfAssign;
                if (readyNeutralNum >= optNeutralNum) break;
            }
        }

        // 抽取优先职业
        while (crewOnList.Count > 0)
        {
            var select = crewOnList[rnd.Next(0, crewOnList.Count)];
            crewOnList.Remove(select);
            rolesToAssign.Add(select);
            if (select == RoleId.Sheriff && CustomOptionHolder.deputySpawnRate.GetBool() && readyRoleTotalNum < playerCount)
            {
                rolesToAssign.Add(RoleId.Deputy);
            }
            readyRoleTotalNum++;
            Info(select.ToString() + " 加入船员职业待选列表（优先）", "CustomRoleSelector");
            if (readyRoleTotalNum >= playerCount) goto EndOfAssign;
        }
        // 优先职业不足以分配，开始分配启用的职业
        if (readyRoleTotalNum < playerCount)
        {
            while (crewRateList.Count > 0)
            {
                var select = crewRateList[rnd.Next(0, crewRateList.Count)];
                crewRateList.Remove(select);
                rolesToAssign.Add(select);
                readyRoleTotalNum++;
                if (select == RoleId.Sheriff && CustomOptionHolder.deputySpawnRate.GetBool() && readyRoleTotalNum < playerCount)
                {

                    rolesToAssign.Add(RoleId.Deputy);
                }

                Info(select.ToString() + " 加入船员职业待选列表", "CustomRoleSelector");
                if (readyRoleTotalNum >= playerCount) goto EndOfAssign;
            }
        }

    // 职业抽取结束
    EndOfAssign:


        // Dev Roles List Edit
        foreach (var dr in DevRole)
        {
            if (dr.Key == PlayerControl.LocalPlayer.PlayerId && CustomOptionHolder.EnableGM.GetBool()) continue;
            if (rolesToAssign.Contains(dr.Value))
            {
                rolesToAssign.Remove(dr.Value);
                rolesToAssign.Insert(0, dr.Value);
                Info("职业列表提高优先：" + dr.Value, "Dev Role");
                continue;
            }
            for (int i = 0; i < rolesToAssign.Count; i++)
            {
                var role = rolesToAssign[i];
                if (dr.Value.GetRoleInfo().RoleOption.GetSelection() != role.GetRoleInfo().RoleOption.GetSelection()) continue;
                if (
                    (dr.Value.IsImpostor() && role.IsImpostor()) ||
                    (dr.Value.IsNeutral() && role.IsNeutral()) ||
                    (dr.Value.IsCrewmate() & role.IsCrewmate())
                    )
                {
                    rolesToAssign.RemoveAt(i);
                    rolesToAssign.Insert(0, dr.Value);
                    Info("覆盖职业列表：" + i + " " + role.ToString() + " => " + dr.Value, "Dev Role");
                    break;
                }
            }
        }

        var AllPlayer = Main.AllAlivePlayerControls.ToList();

        while (AllPlayer.Count > 0 && rolesToAssign.Count > 0)
        {
            PlayerControl delPc = null;
            foreach (var pc in AllPlayer)
                foreach (var dr in DevRole.Where(x => pc.PlayerId == x.Key))
                {
                    if (dr.Key == PlayerControl.LocalPlayer.PlayerId && CustomOptionHolder.EnableGM.GetBool()) continue;
                    var id = rolesToAssign.IndexOf(dr.Value);
                    if (id == -1) continue;
                    RoleResult.Add(pc, rolesToAssign[id]);
                    Info($"职业优先分配：{AllPlayer[0].GetRealName()} => {rolesToAssign[id]}", "CustomRoleSelector");
                    delPc = pc;
                    rolesToAssign.RemoveAt(id);
                    goto EndOfWhile;
                }

            var roleId = rnd.Next(0, rolesToAssign.Count);
            RoleResult.Add(AllPlayer[0], rolesToAssign[roleId]);
            Info($"职业分配：{AllPlayer[0].GetRealName()} => {rolesToAssign[roleId]}", "CustomRoleSelector");
            AllPlayer.RemoveAt(0);
            rolesToAssign.RemoveAt(roleId);

        EndOfWhile:;
            if (delPc != null)
            {
                AllPlayer.Remove(delPc);
                DevRole.Remove(delPc.PlayerId);
            }
        }

        if (AllPlayer.Count > 0)
            Warn("职业分配错误：存在未被分配职业的玩家", "CustomRoleSelector");
        if (rolesToAssign.Count > 0)
            Error("职业分配错误：存在未被分配的职业", "CustomRoleSelector");

    }

    public static int addScientistNum = 0;
    public static int addEngineerNum = 0;
    public static int addShapeshifterNum = 0;
    public static int addGuardianAngelNum = 0;

    public static int GetRoleTypesCount(RoleTypes type)
    {
        return type switch
        {
            RoleTypes.Engineer => addEngineerNum,
            RoleTypes.Scientist => addScientistNum,
            RoleTypes.Shapeshifter => addShapeshifterNum,
            RoleTypes.GuardianAngel => addGuardianAngelNum,
            _ => 0
        };
    }

    public static int GetAssignCount(this RoleId role)
    {
        int maximumCount = role.GetRoleCount();
        int assignUnitCount = 1;
        return maximumCount / assignUnitCount;
    }

    /*
    public static List<RoleId> AddonRolesList = new();
    public static void SelectAddonRoles()
    {
        AddonRolesList = new();
        foreach (var cr in Enum.GetValues(typeof(RoleId)))
        {
            RoleId role = (RoleId)Enum.Parse(typeof(RoleId), cr.ToString());
            //if (!role.IsAddon()) continue;
            //if (role is RoleId.Madmate && Options.MadmateSpawnMode.GetInt() != 0) continue;
            if (role is RoleId.Lover) continue;
            AddonRolesList.Add(role);
        }
    }*/
}