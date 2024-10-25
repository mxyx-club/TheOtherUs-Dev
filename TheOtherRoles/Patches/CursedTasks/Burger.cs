using System.Linq;
using UnityEngine;
namespace TheOtherRoles.Patches.CursedTasks;

internal static class Burger
{
    [HarmonyPatch(typeof(BurgerMinigame))]
    private static class BurgerMinigamePatch
    {
        [HarmonyPatch(nameof(BurgerMinigame.Begin)), HarmonyPostfix]
        private static void BeginPostfix(BurgerMinigame __instance)
        {
            if (!ModOption.CursedTasks) return;
            switch (Random.RandomRange(0f, 1f))
            {
                case <= 0.50f: // 50%
                    __instance.ExpectedToppings = new(6);
                    __instance.ExpectedToppings[0] = BurgerToppingTypes.Plate;
                    for (var i = 1; i < __instance.ExpectedToppings.Count; i++)
                    {
                        var topping = (BurgerToppingTypes)IntRange.Next(0, 6);
                        var set = __instance.ExpectedToppings.Count(t => t == topping) < topping switch
                        {
                            BurgerToppingTypes.TopBun => 1,
                            BurgerToppingTypes.BottomBun => 1,
                            BurgerToppingTypes.Lettuce => 3,
                            _ => 2
                        };
                        if (set) __instance.ExpectedToppings[i] = topping;
                        else i--;
                    }
                    break;
                case <= 0.70f: // 20%
                    __instance.ExpectedToppings = new(6);
                    __instance.ExpectedToppings[0] = BurgerToppingTypes.Plate;
                    var bun = (new BurgerToppingTypes[] { BurgerToppingTypes.Meat, BurgerToppingTypes.Onion, BurgerToppingTypes.Tomato }).GetRandom();
                    __instance.ExpectedToppings[1] = bun;
                    __instance.ExpectedToppings[5] = bun;
                    for (var i = 2; i < __instance.ExpectedToppings.Count - 1; i++)
                    {
                        var topping = (BurgerToppingTypes)IntRange.Next(2, 6);
                        var set = __instance.ExpectedToppings.Count(t => t == topping) < topping switch
                        {
                            BurgerToppingTypes.TopBun => 1,
                            BurgerToppingTypes.BottomBun => 1,
                            BurgerToppingTypes.Lettuce => 3,
                            _ => 2
                        };
                        if (set) __instance.ExpectedToppings[i] = topping;
                        else i--;
                    }
                    break;
                case <= 0.90f: // 20%
                    __instance.ExpectedToppings = new(6);
                    __instance.ExpectedToppings[0] = BurgerToppingTypes.Plate;
                    __instance.ExpectedToppings[1] = BurgerToppingTypes.Lettuce;
                    __instance.ExpectedToppings[5] = BurgerToppingTypes.Lettuce;
                    for (var i = 2; i < __instance.ExpectedToppings.Count - 1; i++)
                    {
                        var topping = (new BurgerToppingTypes[] { BurgerToppingTypes.Lettuce, BurgerToppingTypes.Onion, BurgerToppingTypes.Tomato }).GetRandom();
                        var set = __instance.ExpectedToppings.Count(t => t == topping) < topping switch
                        {
                            BurgerToppingTypes.TopBun => 1,
                            BurgerToppingTypes.BottomBun => 1,
                            BurgerToppingTypes.Lettuce => 3,
                            _ => 2
                        };
                        if (set) __instance.ExpectedToppings[i] = topping;
                        else i--;
                    }
                    break;
                case <= 0.95f: // 5%
                    __instance.ExpectedToppings = new(3);
                    __instance.ExpectedToppings[0] = BurgerToppingTypes.Plate;
                    __instance.ExpectedToppings[1] = BurgerToppingTypes.BottomBun;
                    __instance.ExpectedToppings[2] = BurgerToppingTypes.TopBun;
                    break;
                case <= 1.00f: // 5%
                    __instance.ExpectedToppings = new(6);
                    __instance.ExpectedToppings[0] = BurgerToppingTypes.Plate;
                    if (BoolRange.Next(0.1f))
                    {
                        __instance.ExpectedToppings[1] = BurgerToppingTypes.Lettuce;
                        __instance.ExpectedToppings[5] = BurgerToppingTypes.Lettuce;
                    }
                    else
                    {
                        __instance.ExpectedToppings[1] = BurgerToppingTypes.BottomBun;
                        __instance.ExpectedToppings[5] = BurgerToppingTypes.TopBun;
                    }
                    for (var i = 2; i < __instance.ExpectedToppings.Count - 1; i++)
                    {
                        var topping = (BurgerToppingTypes)IntRange.Next(2, 6);
                        if (__instance.ExpectedToppings.Count(t => t == topping) >= 2) i--;
                        else __instance.ExpectedToppings[i] = topping;
                    }
                    if (BoolRange.Next(0.01f))
                    {
                        var burgerToppingTypes = __instance.ExpectedToppings[5];
                        __instance.ExpectedToppings[5] = __instance.ExpectedToppings[4];
                        __instance.ExpectedToppings[4] = burgerToppingTypes;
                    }
                    break;
            }

            for (var i = 0; i < __instance.PaperSlots.Length; i++)
            {
                if (i < __instance.ExpectedToppings.Count - 1) __instance.PaperSlots[i].sprite = __instance.PaperToppings[(int)__instance.ExpectedToppings[i + 1]];
                else __instance.PaperSlots[i].enabled = false;
            }
        }
    }
}
