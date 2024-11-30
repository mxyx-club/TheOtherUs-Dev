using System;
using System.Collections.Generic;
using TheOtherRoles.Buttons;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace TheOtherRoles.Patches;

public static class LobbyRoleInfo
{
    public static GameObject RolesSummaryUI { get; set; }
    public static readonly List<string> Teams = ["Impostors", "Neutrals", "Crewmates", "Modifiers", "GhostRole"];
    private static TextMeshPro infoButtonText;
    private static TextMeshPro infoTitleText;

    [HarmonyPatch(typeof(HudManager), nameof(HudManager.Update))]
    public static class RoleSummaryButtonHudUpdate
    {
        public static void Postfix(HudManager __instance)
        {
            if (!LobbyBehaviour.Instance || AmongUsClient.Instance.GameState == InnerNet.InnerNetClient.GameStates.Started) return;
            try
            {
                //if (HudManagerStartPatch.roleSummaryButton == null) HudManagerStartPatch.createRoleSummaryButton(__instance);
                if (HudManagerStartPatch.roleSummaryButton.Timer > 0f) HudManagerStartPatch.roleSummaryButton.Timer = 0f;
                HudManagerStartPatch.roleSummaryButton.Update();
            }
            catch { }
        }
    }

    public static void RoleSummaryOnClick()
    {
        if (RolesSummaryUI != null) return;

        SpriteRenderer container = new GameObject("RoleSummaryMenuContainer").AddComponent<SpriteRenderer>();
        container.sprite = new ResourceSprite("LobbyRoleInfo.TeamScreen.png", 110f);
        container.transform.SetParent(HudManager.Instance.transform);
        container.gameObject.transform.SetLocalZ(-200);
        container.transform.localPosition = new Vector3(0, -0.2f, -50f);
        container.transform.localScale = new Vector3(.75f, .7f, 1f);
        container.gameObject.layer = 5;

        RolesSummaryUI = container.gameObject;

        Transform buttonTemplate = HudManager.Instance.SettingsButton.transform;
        TextMeshPro textTemplate = HudManager.Instance.TaskPanel.taskText;

        TextMeshPro newtitle = Object.Instantiate(textTemplate, container.transform);
        newtitle.text = GetString("lobbyInfoSummary");
        newtitle.color = Color.white;
        newtitle.outlineWidth = 0.01f;
        newtitle.transform.localPosition = new Vector3(1f, 0.17f, -2f);
        newtitle.transform.localScale = Vector3.one * 2.5f;

        List<Transform> buttons = new();

        for (int i = 0; i < Teams.Count; i++)
        {
            string team = "";
            RoleType teamid = RoleType.Crewmate;
            switch (Teams[i])
            {
                case "Impostors":
                    team = cs(Palette.ImpostorRed, GetString("ImpostorRolesText"));
                    teamid = RoleType.Impostor;
                    break;
                case "Neutrals":
                    team = cs(new Color32(76, 84, 78, 255), GetString("NeutralRolesText"));
                    teamid = RoleType.Neutral;
                    break;
                case "Crewmates":
                    team = cs(Palette.CrewmateBlue, GetString("CrewmateRolesText"));
                    teamid = RoleType.Crewmate;
                    break;
                case "Modifiers":
                    team = cs(Color.yellow, GetString("ModifierRolesText"));
                    teamid = RoleType.Modifier;
                    break;
                case "GhostRole":
                    team = cs(new Color32(25, 68, 142, byte.MaxValue), GetString("GhostRoleText"));
                    teamid = RoleType.GhostRole;
                    break;
            }

            Transform buttonTransform = Object.Instantiate(buttonTemplate, container.transform);
            buttonTransform.name = team + " Button";
            buttonTransform.GetComponent<BoxCollider2D>().size = new Vector2(2.5f, 0.55f);
            buttonTransform.GetComponent<SpriteRenderer>().sprite = new ResourceSprite("TheOtherRoles.Resources.LobbyRoleInfo.RolePlate.png", 215f);
            buttons.Add(buttonTransform);
            buttonTransform.localPosition = new Vector3(0, 2.2f - (i * 1f), -5);
            buttonTransform.localScale = new Vector3(2f, 1.5f, 1f);

            TextMeshPro label = Object.Instantiate(textTemplate, buttonTransform);
            label.text = team;
            label.alignment = TextAlignmentOptions.Center;
            label.transform.localPosition = new Vector3(0, 0, label.transform.localPosition.z);
            label.transform.localScale = new Vector3(1.4f, 2.2f, 1f);

            PassiveButton button = buttonTransform.GetComponent<PassiveButton>();
            button.OnClick.RemoveAllListeners();
            Button.ButtonClickedEvent onClick = button.OnClick = new Button.ButtonClickedEvent();
            onClick.AddListener((Action)(() =>
            {
                Object.Destroy(container.gameObject);
                roleInfosOnclick(team, teamid);
            }));

            button.OnMouseOver.RemoveAllListeners();
            button.OnMouseOver.AddListener((Action)(() =>
            {
                buttonTransform.GetComponent<SpriteRenderer>().color = Color.yellow;
            }));

            button.OnMouseOut.RemoveAllListeners();
            button.OnMouseOut.AddListener((Action)(() =>
            {
                buttonTransform.GetComponent<SpriteRenderer>().color = Color.white;
            }));
        }
    }

    public static void roleInfosOnclick(string team, RoleType teamId)
    {
        SpriteRenderer container = new GameObject("RoleListMenuContainer").AddComponent<SpriteRenderer>();
        container.sprite = new ResourceSprite("LobbyRoleInfo.RoleListScreen.png", 110f);
        container.transform.SetParent(HudManager.Instance.transform);
        container.transform.localPosition = new Vector3(0, 0.12f, -75f);
        container.transform.localScale = new Vector3(.7f, .7f, 1f);
        container.gameObject.layer = 5;
        RolesSummaryUI = container.gameObject;

        Transform buttonTemplate = HudManager.Instance.SettingsButton.transform;
        TextMeshPro textTemplate = HudManager.Instance.TaskPanel.taskText;

        TextMeshPro newtitle = Object.Instantiate(textTemplate, container.transform);
        newtitle.text = team;
        newtitle.outlineWidth = 0.1f;
        newtitle.transform.localPosition = new Vector3(0f, 2.8f, -2f);
        newtitle.transform.localScale = Vector3.one * 2.5f;

        List<Transform> buttons = new();
        int count = 0;
        bool gameStarted = AmongUsClient.Instance.GameState == InnerNet.InnerNetClient.GameStates.Started;
        foreach (RoleInfo roleInfo in RoleInfo.allRoleInfos)
        {
            if (roleInfo.roleTeam == RoleType.Modifier && teamId != RoleType.Modifier) continue;
            else if (roleInfo.roleTeam == RoleType.Neutral && teamId != RoleType.Neutral) continue;
            else if (roleInfo.roleTeam == RoleType.Impostor && teamId != RoleType.Impostor) continue;
            else if (roleInfo.roleTeam == RoleType.Crewmate && teamId != RoleType.Crewmate) continue;
            else if (roleInfo.roleTeam == RoleType.GhostRole && teamId != RoleType.GhostRole) continue;

            Transform buttonTransform = Object.Instantiate(buttonTemplate, container.transform);
            buttonTransform.name = cs(roleInfo.color, roleInfo.Name) + " Button";
            buttonTransform.GetComponent<BoxCollider2D>().size = new Vector2(2.5f, 0.55f);
            TextMeshPro label = Object.Instantiate(textTemplate, buttonTransform);
            buttonTransform.GetComponent<SpriteRenderer>().sprite = UnityHelper.loadSpriteFromResources("TheOtherRoles.Resources.LobbyRoleInfo.RolePlate2.png", 215f);
            buttons.Add(buttonTransform);
            int row = count / 3, col = count % 3;
            buttonTransform.localPosition = new Vector3(-3.205f + (col * 3.2f), 2.9f - (row * 0.75f), -5);
            buttonTransform.localScale = new Vector3(1.125f, 1.125f, 1f);
            label.text = cs(roleInfo.color, roleInfo.Name);
            label.alignment = TextAlignmentOptions.Center;
            label.transform.localPosition = new Vector3(0, 0, label.transform.localPosition.z);
            label.transform.localScale *= 1.5f;
            PassiveButton button = buttonTransform.GetComponent<PassiveButton>();
            button.OnClick.RemoveAllListeners();
            Button.ButtonClickedEvent onClick = button.OnClick = new Button.ButtonClickedEvent();
            onClick.AddListener((Action)(() =>
            {
                Object.Destroy(container.gameObject);
                AddInfoCard(roleInfo);
            }));
            button.OnMouseOut.RemoveAllListeners();
            button.OnMouseOver.AddListener((Action)(() =>
            {
                buttonTransform.GetComponent<SpriteRenderer>().color = Color.yellow;
            }));
            button.OnMouseOut.RemoveAllListeners();
            button.OnMouseOut.AddListener((Action)(() =>
            {
                buttonTransform.GetComponent<SpriteRenderer>().color = Color.white;
            }));
            count++;
        }
    }

    private static void AddInfoCard(RoleInfo roleInfo)
    {
        string roleSettingDescription = roleInfo.FullDescription != "" ? roleInfo.FullDescription : roleInfo.ShortDescription;
        string coloredHelp = cs(Color.white, roleSettingDescription);

        GameObject roleCard = Object.Instantiate(new GameObject("RoleCard"), HudManager.Instance.transform);
        SpriteRenderer roleCardRend = roleCard.AddComponent<SpriteRenderer>();
        roleCard.layer = 5;
        roleCard.transform.localPosition = new Vector3(0f, 0f, -150f);
        roleCard.transform.localScale = new Vector3(0.68f, 0.68f, 1f);
        RolesSummaryUI = roleCard.gameObject;

        roleCardRend.sprite = UnityHelper.loadSpriteFromResources("TheOtherRoles.Resources.LobbyRoleInfo.SummaryScreen.png", 110f);

        infoButtonText = Object.Instantiate(HudManager.Instance.TaskPanel.taskText, roleCard.transform);
        infoButtonText.color = Color.white;
        infoButtonText.text = coloredHelp;
        infoButtonText.enableWordWrapping = false;
        infoButtonText.transform.localScale = Vector3.one * 1.25f;
        infoButtonText.transform.localPosition = new Vector3(-2.9f, 0f, -50f);
        infoButtonText.alignment = TextAlignmentOptions.TopLeft;
        infoButtonText.fontStyle = FontStyles.Bold;

        infoTitleText = Object.Instantiate(HudManager.Instance.TaskPanel.taskText, roleCard.transform);
        infoTitleText.color = Color.white;
        infoTitleText.text = cs(roleInfo.color, roleInfo.Name);
        infoTitleText.enableWordWrapping = false;
        infoTitleText.transform.localScale = Vector3.one * 3f;
        infoTitleText.transform.localPosition = new Vector3(0f, 2.4f, -50f);
        infoTitleText.alignment = TextAlignmentOptions.Center;
        infoTitleText.fontStyle = FontStyles.Bold;
    }
}

[HarmonyPatch(typeof(ShipStatus), nameof(ShipStatus.Start))]
internal class GameStartPatch
{
    public static void Prefix(ShipStatus __instance)
    {
        if (LobbyRoleInfo.RolesSummaryUI != null) LobbyRoleInfo.RolesSummaryUI.SetActive(false);
    }
}