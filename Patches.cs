using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using UnityEngine;
using static gVTweaks.Mod;
using UnityEngine.PostProcessing;
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
                {
                    //var ArmorWolfLegs = Resources.FindObjectsOfTypeAll<ItemDrop>().First(o => o.name == "ArmorWolfLegs");
                    //ArmorWolfLegs.Awake();
                    //for (var i = 0; i < 25; i++)
                    //{
                    //    Player.m_localPlayer.m_inventory.AddItem(ArmorWolfLegs.m_itemData.Clone());
                    //}

                    //var ArmorWolfChest = Resources.FindObjectsOfTypeAll<ItemDrop>().First(o => o.name == "ArmorWolfLegs");
                    //ArmorWolfChest.Awake();
                    //Player.m_localPlayer.m_inventory.AddItem(Player.m_localPlayer.m_inventory.AddItem("DragonEgg", 1, 0, 0,  0, "");
                    //var ship = new GameObject("CustomLongship");
                    //ship.AddComponent<Ship>();
                    //ship.transform.position = Player.m_localPlayer.transform.position + Vector3.one;
                    //Object.Instantiate(ship);
                }
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
                if (viewDistanceIncrease.Value > 0)
                {
                    ___m_activeArea = 2 + viewDistanceIncrease.Value;
                    ___m_activeDistantArea = 2 + viewDistanceIncrease.Value;
                }
            }
        }

        [HarmonyPatch(typeof(ZDOMan), "FindSectorObjects", typeof(Vector2i), typeof(int), typeof(int), typeof(List<ZDO>), typeof(List<ZDO>))]
        public static class ZDOManFindSectorObjectsPatch
        {
            public static void Prefix(ref int area, ref int distantArea)
            {
                if (viewDistanceIncrease.Value > 0)
                {
                    // 2 is the current default (Aug 2021)
                    area = 2 + viewDistanceIncrease.Value;
                    distantArea = 2 + viewDistanceIncrease.Value;
                }
            }
        }

        [HarmonyPatch(typeof(GameCamera), "Awake")]
        public static class GameCameraAwakePatch
        {
            public static void Postfix(ref float ___m_maxDistance, ref float ___m_maxDistanceBoat)
            {
                if (camera.Value)
                {
                    ___m_maxDistance = 16;
                    ___m_maxDistanceBoat = 32;
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
                    var item = AccessTools.Method(typeof(Smelter), "FindCookableItem").Invoke(__instance, new object[] { user.GetInventory() });
                    if (item is not null
                        && __instance.GetQueueSize() < __instance.m_maxOre)
                    {
                        AccessTools.Method(typeof(Smelter), "OnAddOre").Invoke(__instance, new[] { sw, user, item });
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
                    if (user.GetInventory().HaveItem(___m_fuelItem.m_itemData.m_shared.m_name)
                        && __instance.GetFuel() < __instance.m_maxFuel)
                    {
                        AccessTools.Method(typeof(Smelter), "OnAddFuel").Invoke(__instance, new object[] { sw, user, null });
                    }
                }
            }
        }

        [HarmonyPatch(typeof(Ship), "GetSailForce")]
        public static class ShipGetSailForcePatch
        {
            public static void Prefix(ref float sailSize)
            {
                if (sailEffectPercent.Value > 100)
                {
                    // ReSharper disable once PossibleLossOfFraction
                    sailSize *= sailEffectPercent.Value / 100;
                }
            }
        }

        [HarmonyPatch(typeof(Ship), "Awake")]
        public static class ShipAwakePatch
        {
            public static void Postfix(Ship __instance, ref float ___m_backwardForce)
            {
                if (shipMassPercent.Value > 100)
                {
                    // ReSharper disable once PossibleLossOfFraction
                    __instance.GetComponentInChildren<Rigidbody>().mass /= shipMassPercent.Value / 100;
                }

                if (oarSpeedPercent.Value > 100)
                {
                    // ReSharper disable once PossibleLossOfFraction
                    ___m_backwardForce *= oarSpeedPercent.Value / 100;
                }
            }
        }

        [HarmonyPatch(typeof(CameraEffects), "SetAntiAliasing")]
        public static class CameraEffectsSetAntiAliasingPatch
        {
            public static void Prefix(PostProcessingBehaviour ___m_postProcessing)
            {
                if (taa.Value)
                {
                    var antialiasingSettings = ___m_postProcessing.profile.antialiasing.settings;
                    antialiasingSettings.method = AntialiasingModel.Method.Taa;
                    ___m_postProcessing.profile.antialiasing.settings = antialiasingSettings;
                }
            }
        }

        //[HarmonyPatch(typeof(GeometryUtility), "CalculateFrustumPlanes", typeof(Matrix4x4), typeof(Plane[]))]
        //public static class adfasdfd
        //{
        //    public static void Prefix(ref Plane[] planes)
        //    {
        //        foreach (var p in planes)
        //        {
        //            
        //        }
        //    }
        //}

        //[HarmonyPatch(typeof(Utils), "InsideMainCamera", typeof(BoundingSphere))]
        //public static class UtilsInsideMainCameraPatch
        //{
        //    public static void Prefix(ref BoundingSphere bounds)
        //    {
        //        if (boundFactor.Value != -1)
        //        {
        //            bounds.radius *= boundFactor.Value;
        //        }
        //    }
        //}
        //
        //[HarmonyPatch(typeof(Utils), "InsideMainCamera", typeof(Bounds))]
        //public static class UtilsInsideMainCameraPatch2
        //{
        //    public static void Prefix(ref Bounds bounds)
        //    {
        //        if (boundFactor.Value != -1)
        //        {
        //            bounds.size *= boundFactor.Value;
        //        }
        //    }
        //}

        //thanks to Valheim Plus
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
    }
}
