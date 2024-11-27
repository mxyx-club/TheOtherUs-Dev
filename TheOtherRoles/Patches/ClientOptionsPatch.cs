using System;
using System.Collections;
using System.Collections.Generic;
using BepInEx.Unity.IL2CPP.Utils.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using static UnityEngine.UI.Button;
using Object = UnityEngine.Object;

namespace TheOtherRoles.Patches;

[HarmonyPatch(typeof(OptionsMenuBehaviour), nameof(OptionsMenuBehaviour.Start))]
public static class StartOptionMenuPatch
{
    public static void UpdateCustomText(this ToggleButtonBehaviour button, Color color, string text = null)
    {
        button.onState = false;
        button.Background.color = color;
        if (text != null)
        {
            button.Text.text = text;
        }

        if (button.Rollover)
        {
            button.Rollover.ChangeOutColor(color);
        }
    }

    public static void UpdateToggleText(this ToggleButtonBehaviour button, bool on, string text)
    {
        button.onState = on;
        Color color = on ? new Color(0f, 1f, 0.16470589f, 1f) : Color.white;
        button.Background.color = color;
        button.Text.text = text + ": " + DestroyableSingleton<TranslationController>.Instance.GetString(button.onState
            ? StringNames.SettingsOn : StringNames.SettingsOff, new Il2CppReferenceArray<Il2CppSystem.Object>(0));
        if (button.Rollover)
        {
            button.Rollover.ChangeOutColor(color);
        }
    }

    public static void UpdateButtonText(this ToggleButtonBehaviour button, string text, string state)
    {
        button.onState = false;
        Color color = Color.white;
        button.Background.color = color;
        button.Text.text = text + ": " + state;
        if (button.Rollover)
        {
            button.Rollover.ChangeOutColor(color);
        }
    }

    private static ToggleButtonBehaviour AddButton(Vector2 pos, string name, Action onClicked, GameObject nebulaTab, GameObject toggleButtonTemplate)
    {
        GameObject button = Object.Instantiate(toggleButtonTemplate, null);
        button.transform.SetParent(nebulaTab.transform);
        button.transform.localScale = new Vector3(1f, 1f, 1f);
        button.transform.localPosition = new Vector3(1.3f * ((pos.x * 2f) - 1f), 1.6f - (0.5f * pos.y), 0f);
        button.name = name;
        ToggleButtonBehaviour result = button.GetComponent<ToggleButtonBehaviour>();
        PassiveButton passiveButton = button.GetComponent<PassiveButton>();
        passiveButton.OnClick = new ButtonClickedEvent();
        passiveButton.OnClick.AddListener((UnityAction)onClicked);
        return result;
    }

    private static ToggleButtonBehaviour toggleCursor;
    private static ToggleButtonBehaviour enableSoundEffects;
    private static ToggleButtonBehaviour showKeyReminder;
    private static ToggleButtonBehaviour showFPS;
    private static ToggleButtonBehaviour localHats;

    public static void Postfix(OptionsMenuBehaviour __instance)
    {
        List<TabGroup> tabs = new(__instance.Tabs.ToArray());

        PassiveButton passiveButton;

        //�O���Ŀ��׷�Ӥ���

        GameObject nebulaTab = new("NebulaTab");
        nebulaTab.transform.SetParent(__instance.transform);
        nebulaTab.transform.localScale = new Vector3(1f, 1f, 1f);
        nebulaTab.SetActive(false);

        GameObject keyBindingTab = new("KeyBindingTab");
        keyBindingTab.transform.SetParent(__instance.transform);
        keyBindingTab.transform.localScale = new Vector3(1f, 1f, 1f);
        keyBindingTab.SetActive(false);

        GameObject applyButtonTemplate = tabs[1].Content.transform.GetChild(0).FindChild("ApplyButton").gameObject;
        GameObject toggleButtonTemplate = tabs[0].Content.transform.FindChild("MiscGroup").FindChild("StreamerModeButton").gameObject;

        //EnableSoundEffects
        enableSoundEffects = AddButton(new Vector2(0, 0), "EnableSoundEffects", () =>
        {
            enableSoundEffects.UpdateToggleText(!enableSoundEffects.onState, GetString("EnableSoundEffectsText"));
            ModOption.enableSoundEffects = Main.EnableSoundEffects.Value = enableSoundEffects.onState;
        }, nebulaTab, toggleButtonTemplate);

        //ToggleCursor
        toggleCursor = AddButton(new Vector2(0, 1), "ToggleCursor", () =>
        {
            enableCursor(false);
            toggleCursor.UpdateToggleText(!toggleCursor.onState, GetString("ToggleCursorText"));
            ModOption.toggleCursor = Main.ToggleCursor.Value = toggleCursor.onState;
            Message($"toggleCursor: {toggleCursor.onState}");
        }, nebulaTab, toggleButtonTemplate);

        //ShowFPS
        showKeyReminder = AddButton(new Vector2(1, 0), "ShowKeyReminder", () =>
        {
            showKeyReminder.UpdateToggleText(!showKeyReminder.onState, GetString("ShowKeyReminder"));
            ModOption.showKeyReminder = Main.ShowKeyReminder.Value = showKeyReminder.onState;
        }, nebulaTab, toggleButtonTemplate);

        //ShowFPS
        showFPS = AddButton(new Vector2(1, 1), "ShowFPS", () =>
        {
            showFPS.UpdateToggleText(!showFPS.onState, GetString("ShowFPS"));
            ModOption.showFPS = Main.ShowFPS.Value = showFPS.onState;
        }, nebulaTab, toggleButtonTemplate);

        //LocalHats
        localHats = AddButton(new Vector2(0, 2), "LocalHats", () =>
        {
            localHats.UpdateToggleText(!localHats.onState, GetString("LocalHatsText"));
            ModOption.localHats = Main.LocalHats.Value = localHats.onState;
        }, nebulaTab, toggleButtonTemplate);

        //���`��굱�ƥܥ���
        GameObject TextObject;

        List<ToggleButtonBehaviour> allKeyBindingButtons = [];
        int selectedKeyBinding = -1;

        GameObject defaultButton = Object.Instantiate(applyButtonTemplate, null);
        defaultButton.transform.SetParent(keyBindingTab.transform);
        defaultButton.transform.localScale = new Vector3(1f, 1f, 1f);
        defaultButton.transform.localPosition = new Vector3(0f, -2.5f, 0f);
        defaultButton.name = "RestoreDefaultsButton";
        defaultButton.transform.GetChild(0).GetComponent<SpriteRenderer>().size = new Vector2(2.25f, 0.4f);
        TextObject = defaultButton.transform.FindChild("Text_TMP").gameObject;
        TextObject.GetComponent<TextMeshPro>().text = GetString("keyBinding.restoreDefaults");
        TextObject.GetComponent<TextMeshPro>().rectTransform.sizeDelta *= 2;
        TextObject.GetComponent<TextTranslatorTMP>().enabled = false;
        passiveButton = defaultButton.GetComponent<PassiveButton>();
        passiveButton.OnClick = new ButtonClickedEvent();
        passiveButton.OnClick.AddListener((UnityAction)(() =>
        {
            selectedKeyBinding = -1;
            //_ = SoundManager.Instance.PlaySound(Module.MetaScreen.getSelectClip(), false, 0.8f);

            for (int i = 0; i < ModInputManager.allInputs.Count; i++)
            {
                ModInputManager.ModInput input = ModInputManager.allInputs[i];
                input.resetToDefault();
                allKeyBindingButtons[i].UpdateCustomText(Color.white, GetString("keyBinding." + input.identifier) + ": " + ModInputManager.allKeyCodes[input.keyCode].displayKey);
            }
        }
        ));

        foreach (var input in ModInputManager.allInputs)
        {
            int index = allKeyBindingButtons.Count;

            GameObject inputButton = Object.Instantiate(toggleButtonTemplate, null);
            inputButton.transform.SetParent(keyBindingTab.transform);
            inputButton.transform.localScale = new Vector3(1f, 1f, 1f);
            inputButton.transform.localPosition = new Vector3(1.3f * ((index % 2 * 2) - 1), 1.5f - (0.5f * (index / 2)), 0f);
            inputButton.name = input.identifier;
            ToggleButtonBehaviour inputToggleButton = inputButton.GetComponent<ToggleButtonBehaviour>();
            inputToggleButton.BaseText = 0;
            inputToggleButton.Text.text = GetString("keyBinding." + input.identifier) + ": " + ModInputManager.allKeyCodes[input.keyCode].displayKey;
            passiveButton = inputButton.GetComponent<PassiveButton>();
            passiveButton.OnClick = new ButtonClickedEvent();
            passiveButton.OnClick.AddListener((UnityAction)(() =>
            {
                if (selectedKeyBinding == index)
                {
                    selectedKeyBinding = -1;
                    inputToggleButton.UpdateCustomText(Color.white, null);
                }
                else
                {
                    selectedKeyBinding = index;
                    allKeyBindingButtons[selectedKeyBinding].UpdateCustomText(Color.yellow,
                        GetString($"{GetString($"keyBinding.{input.identifier}")}: {GetString("keyBinding.recording")}"));
                    inputToggleButton.UpdateCustomText(Color.yellow, null);
                }
            }));

            allKeyBindingButtons.Add(inputToggleButton);
        }

        GameObject keyBindingButton = Object.Instantiate(applyButtonTemplate, null);
        keyBindingButton.transform.SetParent(nebulaTab.transform);
        keyBindingButton.transform.localScale = new Vector3(1f, 1f, 1f);
        keyBindingButton.transform.localPosition = new Vector3(0f, -1.5f, 0f);
        keyBindingButton.name = "KeyBindingButton";
        keyBindingButton.transform.GetChild(0).GetComponent<SpriteRenderer>().size = new Vector2(2.25f, 0.4f);
        TextObject = keyBindingButton.transform.FindChild("Text_TMP").gameObject;
        TextObject.GetComponent<TextMeshPro>().text = GetString("keyBinding");
        TextObject.GetComponent<TextMeshPro>().rectTransform.sizeDelta *= 2;
        TextObject.GetComponent<TextTranslatorTMP>().enabled = false;
        passiveButton = keyBindingButton.GetComponent<PassiveButton>();
        passiveButton.OnClick = new ButtonClickedEvent();
        passiveButton.OnClick.AddListener((UnityAction)(() =>
        {
            //_ = SoundManager.Instance.PlaySound(Module.MetaScreen.getSelectClip(), false, 0.8f);
            __instance.OpenTabGroup(tabs.Count - 1);
        }
        ));

        IEnumerator getEnumerator()
        {
            while (true)
            {
                /*
                if (HudManager.InstanceExists && !GameStartManager.InstanceExists)
                {
                    keyBindingButton.gameObject.SetActive(false);
                */

                if (keyBindingTab.gameObject.active && Input.anyKeyDown && selectedKeyBinding != -1)
                {
                    foreach (KeyValuePair<KeyCode, ModInputManager.KeyCodeData> entry in ModInputManager.allKeyCodes)
                    {
                        if (!Input.GetKeyDown(entry.Key))
                        {
                            continue;
                        }

                        ModInputManager.ModInput input = ModInputManager.allInputs[selectedKeyBinding];
                        input.changeKeyCode(entry.Key);
                        allKeyBindingButtons[selectedKeyBinding].UpdateCustomText(Color.white, GetString("keyBinding." + input.identifier) + ": " + ModInputManager.allKeyCodes[input.keyCode].displayKey);
                        selectedKeyBinding = -1;
                        break;
                    }
                }
                else if (!keyBindingTab.gameObject.active && selectedKeyBinding != -1)
                {
                    allKeyBindingButtons[selectedKeyBinding].UpdateCustomText(Color.white, null);
                    selectedKeyBinding = -1;
                }
                yield return null;
            }
        }

        _ = HudManager.InstanceExists
            ? HudManager.Instance.StartCoroutine(getEnumerator().WrapToIl2Cpp())
            : __instance.StartCoroutine(getEnumerator().WrapToIl2Cpp());


        //���֤�׷�Ӥ���

        tabs[^1] = Object.Instantiate(tabs[1], null);
        TabGroup nebulaButton = tabs[^1];
        nebulaButton.gameObject.name = "NebulaButton";
        nebulaButton.transform.SetParent(tabs[0].transform.parent);
        nebulaButton.transform.localScale = new Vector3(1f, 1f, 1f);
        nebulaButton.Content = nebulaTab;
        GameObject textObj = nebulaButton.transform.FindChild("Text_TMP").gameObject;
        textObj.GetComponent<TextTranslatorTMP>().enabled = false;
        textObj.GetComponent<TMP_Text>().text = "modOptionsTitle".Translate();

        tabs.Add(Object.Instantiate(tabs[1], null));
        TabGroup keyBindingTabButton = tabs[^1];
        keyBindingTabButton.gameObject.name = "KeyBindingButton";
        keyBindingTabButton.transform.SetParent(tabs[0].transform.parent);
        keyBindingTabButton.transform.localScale = new Vector3(1f, 1f, 1f);
        keyBindingTabButton.Content = keyBindingTab;
        keyBindingTabButton.gameObject.SetActive(false);

        passiveButton = nebulaButton.gameObject.GetComponent<PassiveButton>();
        passiveButton.OnClick = new ButtonClickedEvent();
        passiveButton.OnClick.AddListener((UnityAction)(() =>
        {
            __instance.OpenTabGroup(tabs.Count - 2);

            showFPS.UpdateToggleText(Main.ShowFPS.Value, GetString("ShowFPS"));
            enableSoundEffects.UpdateToggleText(Main.EnableSoundEffects.Value, GetString("EnableSoundEffectsText"));
            showKeyReminder.UpdateToggleText(Main.ShowKeyReminder.Value, GetString("ShowKeyReminder"));
            toggleCursor.UpdateToggleText(Main.ToggleCursor.Value, GetString("ToggleCursorText"));
            localHats.UpdateToggleText(Main.LocalHats.Value, GetString("LocalHatsText"));

            passiveButton.OnMouseOver.Invoke();
        }
        ));

        float y = tabs[0].transform.localPosition.y, z = tabs[0].transform.localPosition.z;
        if (tabs.Count == 4)
        {
            for (int i = 0; i < 3; i++)
            {
                tabs[i].transform.localPosition = new Vector3(1.7f * (i - 1), y, z);
            }
        }
        else if (tabs.Count == 5)
        {
            for (int i = 0; i < 4; i++)
            {
                tabs[i].transform.localPosition = new Vector3(1.62f * (i - 1.5f), y, z);
            }
        }

        __instance.Tabs = new Il2CppReferenceArray<TabGroup>(tabs.ToArray());
    }
}