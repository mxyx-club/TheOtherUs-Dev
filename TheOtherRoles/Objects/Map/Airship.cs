﻿using System;
using System.Linq;
using TheOtherRoles.Utilities;
using UnityEngine;

namespace TheOtherRoles.Objects.Map;

[HarmonyPatch(typeof(ShipStatus), nameof(ShipStatus.Awake))]
internal class OptimizeMapPatch
{
    private static Sprite ladderSprite;
    private static Sprite ladderBgSprite;

    public static void Postfix(ShipStatus __instance)
    {
        addLadder(GameOptionsManager.Instance.currentNormalGameOptions.MapId);
        optimizeMap(GameOptionsManager.Instance.currentNormalGameOptions.MapId);
    }

    public static void optimizeMap(int mapId)
    {
        if (!CustomOptionHolder.airshipOptimize.GetBool()) return;
        if (mapId == 4)
        {
            var obj = ShipStatus.Instance.FastRooms[SystemTypes.GapRoom].gameObject;
            //昇降機右に影を追加
            OneWayShadows oneWayShadow = obj.transform.FindChild("Shadow").FindChild("LedgeShadow").GetComponent<OneWayShadows>();
            oneWayShadow.enabled = false;
            if (CachedPlayer.LocalPlayer.PlayerControl.Data.Role.IsImpostor) oneWayShadow.gameObject.SetActive(false);

            SpriteRenderer renderer;

            GameObject fance = new("ModFance")
            {
                layer = LayerMask.NameToLayer("Ship")
            };
            fance.transform.SetParent(obj.transform);
            fance.transform.localPosition = new Vector3(4.2f, 0.15f, 0.5f);
            fance.transform.localScale = new Vector3(1f, 1f, 1f);
            fance.SetActive(true);
            var Collider = fance.AddComponent<EdgeCollider2D>();
            Collider.points = new Vector2[] { new(1.5f, -0.2f), new(-1.5f, -0.2f), new(-1.5f, 1.5f) };
            Collider.enabled = true;
            renderer = fance.AddComponent<SpriteRenderer>();
            renderer.sprite = UnityHelper.loadSpriteFromResources("TheOtherRoles.Resources.AirshipFence.png", 100f);

            var panel = obj.transform.FindChild("panel_data");
            panel.localPosition = new Vector3(4.52f, -3.95f, 0.1f);
        }

    }

    public static void addLadder(int mapId)
    {
        if (mapId == 4)
        {
            GameObject meetingRoom = ShipStatus.Instance.FastRooms[SystemTypes.MeetingRoom].gameObject;
            GameObject gapRoom = ShipStatus.Instance.FastRooms[SystemTypes.GapRoom].gameObject;
            if (CustomOptionHolder.airshipLadder.GetBool())
            {
                // 梯子追加
                GameObject ladder = meetingRoom.GetComponentsInChildren<SpriteRenderer>().FirstOrDefault(x => x.name == "ladder_meeting").gameObject;
                GameObject newLadder = UnityEngine.Object.Instantiate(ladder, ladder.transform.parent);
                Il2CppArrayBase<Ladder> ladders = newLadder.GetComponentsInChildren<Ladder>();
                int id = 100;
                foreach (var l in ladders)
                {
                    if (l.name == "LadderBottom") l.gameObject.SetActive(false);
                    l.Id = (byte)id;
                    FastDestroyableSingleton<AirshipStatus>.Instance.Ladders.AddItem(l);
                    id++;
                }
                newLadder.transform.position = new Vector3(15.442f, 12.18f, 0.1f);
                if (!ladderSprite) ladderSprite = UnityHelper.loadSpriteFromResources("TheOtherRoles.Resources.ladder.png", 100f);
                newLadder.GetComponentInChildren<SpriteRenderer>().sprite = ladderSprite;

                // 梯子の周りの影を消す
                UnityEngine.Object.Destroy(gapRoom.GetComponentsInChildren<EdgeCollider2D>().FirstOrDefault(x => Math.Abs(x.points[0].x + 6.2984f) < 0.1));
                EdgeCollider2D collider = meetingRoom.GetComponentsInChildren<EdgeCollider2D>().FirstOrDefault(x => x.pointCount == 46);
                Il2CppSystem.Collections.Generic.List<Vector2> points = new();
                EdgeCollider2D newCollider = collider.gameObject.AddComponent<EdgeCollider2D>();
                EdgeCollider2D newCollider2 = collider.gameObject.AddComponent<EdgeCollider2D>();
                points.Add(collider.points[45]);
                points.Add(collider.points[44]);
                points.Add(collider.points[43]);
                points.Add(collider.points[42]);
                points.Add(collider.points[41]);
                newCollider.SetPoints(points);
                points.Clear();
                foreach (int i in Enumerable.Range(0, 41))
                {
                    points.Add(collider.points[i]);
                }
                newCollider2.SetPoints(points);
                UnityEngine.Object.DestroyObject(collider);

                // 梯子の背景を変更
                SpriteRenderer side = meetingRoom.GetComponentsInChildren<SpriteRenderer>().FirstOrDefault(x => x.name == "meeting_side");
                SpriteRenderer bg = UnityEngine.Object.Instantiate(side, side.transform.parent);
                if (!ladderBgSprite) ladderBgSprite = UnityHelper.loadSpriteFromResources("TheOtherRoles.Resources.ladder_bg.png", 100f);
                bg.sprite = ladderBgSprite;
                bg.transform.localPosition = new Vector3(9.57f, -3.355f, 4.9f);
            }
        }
    }
}