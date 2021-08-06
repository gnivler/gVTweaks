using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using UnityEngine;
using static gVTweaks.Mod;
using Object = UnityEngine.Object;
using BepInEx;
using static gVTweaks.Settings;

namespace gVTweaks
{
    public static class Patches
    {
        [HarmonyPatch(typeof(Game), "Update")]
        public static class GameUpdatePatch
        {
            private static bool big;

            public static void Postfix()
            {
                if (bigHead.Value
                    && Input.GetKey(KeyCode.LeftShift)
                    && Input.GetKeyDown(KeyCode.H))
                {
                    var head = Player.m_localPlayer.GetComponentsInChildren<Transform>().First(t => t.name == "Head");
                    if (big)
                    {
                        head.localScale /= 3;
                    }
                    else
                    {
                        head.localScale *= 3;
                    }

                    big = !big;
                }

                //if (Input.GetKeyDown(KeyCode.F4))
                //{
                //    var iron = Resources.FindObjectsOfTypeAll<ItemDrop>().First(o => o.name == "Iron");
                //    iron.Awake();
                //    for (var i = 0; i < 30; i++)
                //    {
                //        Player.m_localPlayer.m_inventory.AddItem(iron.m_itemData.Clone());
                //    }
                //}
            }
        }

        [HarmonyPatch(typeof(ItemDrop), "CanPickup")]
        public static class ItemDropCanPickupPatch
        {
            public static bool Prefix(ZNetView ___m_nview, ref bool __result)
            {
                if (!lootDelay.Value)
                {
                    return true;
                }

                if (___m_nview == null || !___m_nview.IsValid())
                {
                    __result = true;
                    return false;
                }

                __result = ___m_nview.IsOwner();
                return false;
            }
        }

        [HarmonyPatch(typeof(ZoneSystem), "Awake")]
        public static class ZoneSystemAwakePatch
        {
            public static void Postfix(ref int ___m_activeArea, ref int ___m_activeDistantArea)
            {
                if (viewDistance.Value)
                {
                    ___m_activeArea = 4;
                    ___m_activeDistantArea = 4;
                }
            }
        }

        [HarmonyPatch(typeof(ZDOMan), "FindSectorObjects", typeof(Vector2i), typeof(int), typeof(int), typeof(List<ZDO>), typeof(List<ZDO>))]
        public static class ZDOManFindSectorObjectsPatch
        {
            public static void Prefix(ref int area, ref int distantArea)
            {
                if (viewDistance.Value)
                {
                    area = 4;
                    distantArea = 4;
                }
            }
        }

        //[HarmonyPatch(typeof(ZDO), "SetDistant")]
        //public static class ZDOSetDistantPatch
        //{
        //    public static void Prefix(ref bool distant)
        //    {
        //        distant = false;
        //    }
        //}

        [HarmonyPatch(typeof(GameCamera), "Awake")]
        public static class GameCameraAwakePatch
        {
            public static void Postfix(Camera ___m_camera, ref float ___m_maxDistance, ref float ___m_maxDistanceBoat)
            {
                if (camera.Value)
                {
                    ___m_maxDistance = 16;
                    ___m_maxDistanceBoat = 24;
                }
            }
        }

        [HarmonyPatch(typeof(Smelter), "Awake")]
        public static class SmelterAwakePatch
        {
            public static void Postfix(Smelter __instance, ref int ___m_maxFuel, ref int ___m_maxOre)
            {
                if (smelterSpeed.Value != -1)
                {
                    __instance.m_secPerProduct = smelterSpeed.Value;
                }
                
                if (smelterSize.Value != -1)
                {
                    if (__instance.m_fuelItem is null)
                    {
                        // is a kiln
                        ___m_maxOre = smelterSize.Value;
                    }
                    else
                    {
                        // is a smelter
                        ___m_maxOre = smelterSize.Value;
                        ___m_maxFuel = smelterSize.Value;
                    }
                }
            }
        }

        [HarmonyPatch(typeof(Smelter), "OnAddOre")]
        public static class SmelterOnAddOrePatch
        {
            public static void Postfix(Smelter __instance, Switch sw, Humanoid user)
            {
                if (deposits.Value)
                {
                    var item = AccessTools.Method(typeof(Smelter), "FindCookableItem").Invoke(__instance, new object[] {user.GetInventory()});
                    if (item is not null)
                    {
                        AccessTools.Method(typeof(Smelter), "OnAddOre").Invoke(__instance, new[] {sw, user, item});
                    }
                }
            }
        }

        [HarmonyPatch(typeof(Smelter), "OnAddFuel")]
        public static class SmelterOnAddFuelPatch
        {
            public static void Postfix(Smelter __instance, Switch sw, Humanoid user, ItemDrop ___m_fuelItem)
            {
                if (deposits.Value)
                {
                    if (user.GetInventory().HaveItem(___m_fuelItem.m_itemData.m_shared.m_name))
                    {
                        AccessTools.Method(typeof(Smelter), "OnAddFuel").Invoke(__instance, new object[] {sw, user, null});
                    }
                }
            }
        }

        [HarmonyPatch(typeof(Ship), "GetSailForce")]
        public static class ShipGetSailForcePatch
        {
            public static void Prefix(ref float sailSize)
            {
                if (shipSpeed.Value)
                {
                    sailSize *= 3;
                }
            }
        }

        [HarmonyPatch(typeof(Ship), "Awake")]
        public static class ShipAwakePatch
        {
            public static void Postfix(ref float ___m_backwardForce)
            {
                if (shipSpeed.Value)
                {
                    ___m_backwardForce = 0.6f;
                }
            }
        }

        // thanks to Valheim Plus
        //[HarmonyPatch(typeof(Bed), "Interact")]
        //public static class BedInteractPatch
        //{
        //    private static void Postfix(Bed __instance, Humanoid human, ZNetView ___m_nview)
        //    {
        //        if (Input.GetKey(KeyCode.LeftShift)
        //            && Input.GetKeyDown(KeyCode.E))
        //        {
        //            bool flag = Traverse.Create(__instance).Method("IsCurrent").GetValue<bool>() && ___m_nview.GetZDO().GetLong("owner") != 0L;
        //            if (!flag)
        //            {
        //                Player humanPlayer = human as Player;
        //                if (!EnvMan.instance.IsAfternoon() && !EnvMan.instance.IsNight())
        //                {
        //                    human.Message(MessageHud.MessageType.Center, "$msg_cantsleep", 0, null);
        //                    return;
        //                }
        //
        //                if (!Traverse.Create(__instance).Method("CheckEnemies", humanPlayer).GetValue<bool>())
        //                {
        //                    return;
        //                }
        //
        //                if (!Traverse.Create(__instance).Method("CheckExposure", humanPlayer).GetValue<bool>())
        //                {
        //                    return;
        //                }
        //
        //                if (!Traverse.Create(__instance).Method("CheckFire", humanPlayer).GetValue<bool>())
        //                {
        //                    return;
        //                }
        //
        //                if (!Traverse.Create(__instance).Method("CheckWet", humanPlayer).GetValue<bool>())
        //                {
        //                    return;
        //                }
        //
        //                human.AttachStart(__instance.m_spawnPoint, true, true, "attach_bed", new Vector3(0f, 0.5f, 0f));
        //            }
        //        }
        //    }
        //}
        //
        //[HarmonyPatch(typeof(Bed), "Interact")]
        //public static class Bed_InteractIgnoreStandaloneE_Patch
        //{
        //    private static bool Prefix()
        //    {
        //        if (Input.GetKey(KeyCode.E))
        //        {
        //            if (Input.GetKey(KeyCode.LeftShift))
        //            {
        //                return false;
        //            }
        //        }
        //
        //        return true;
        //    }
        //}
        //
    }
}
