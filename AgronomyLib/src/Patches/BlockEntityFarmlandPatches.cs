using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vintagestory.API.Common;
using Vintagestory.API.Util;
using Vintagestory.GameContent;

namespace AgronomyLib {

    [HarmonyPatch(typeof(BlockEntityFarmland))]
    public static class BlockEntityFarmlandPatches {

        [HarmonyPostfix]
        [HarmonyPatch(nameof(BlockEntityFarmland.GetBlockInfo))]
        public static void GetBlockInfoPostfix(ref BlockEntityFarmland __instance, IPlayer forPlayer, StringBuilder dsc) {
            // Makes it actually get BlockInfo from each of its behaviors like every other BlockEntity in the game. Why doesn't it already do this? Beats me!
            foreach (BlockEntityBehavior behavior in __instance.Behaviors) {
                behavior.GetBlockInfo(forPlayer, dsc);
            }
        }

        /// <remarks>
        /// Complete reimplementation that uses GetCropProps() and has more versatile handling for crop death
        /// </remarks>
        [HarmonyPrefix]
        [HarmonyPatch("updateCropDamage")]
        public static bool updateCropDamagePrefix(ref BlockEntityFarmland __instance, ref bool __result, ref bool ___ripeCropColdDamaged, ref bool ___unripeCropColdDamaged, ref bool ___unripeHeatDamaged, ref bool ___allowcropDeath, ref float[] ___damageAccum, ref Block ___deadCropBlock, double hourIntervall, Block cropBlock, bool hasCrop, bool hasRipeCrop, ClimateCondition conds) {
            ICoreAPI Api = __instance.Api;
            
            if (!hasCrop) {
                ___ripeCropColdDamaged = false;
                ___unripeCropColdDamaged = false;
                ___unripeHeatDamaged = false;
                for (int i = 0; i < ___damageAccum.Length; i++) ___damageAccum[i] = 0;
            } else {
                BlockCropProperties? cProps = cropBlock?.GetCropProps();
                if (cProps != null && conds.Temperature < cProps.ColdDamageBelow) {
                    if (hasRipeCrop) {
                        ___ripeCropColdDamaged = true;
                    } else {
                        ___unripeCropColdDamaged = true;
                        ___damageAccum[(int)EnumCropStressType.TooCold] += (float)hourIntervall;
                    }
                } else {
                    ___damageAccum[(int)EnumCropStressType.TooCold] = Math.Max(0, ___damageAccum[(int)EnumCropStressType.TooCold] - (float)hourIntervall / 10);
                }

                if (cProps != null && conds.Temperature > cProps.HeatDamageAbove && hasCrop) {
                    ___unripeHeatDamaged = true;
                    ___damageAccum[(int)EnumCropStressType.TooHot] += (float)hourIntervall;
                } else {
                    ___damageAccum[(int)EnumCropStressType.TooHot] = Math.Max(0, ___damageAccum[(int)EnumCropStressType.TooHot] - (float)hourIntervall / 10);
                }

                for (int i = 0; i < ___damageAccum.Length; i++) {
                    float dmg = ___damageAccum[i];
                    if (!___allowcropDeath) dmg = ___damageAccum[i] = 0;

                    if (dmg > 48) {
                        hasCrop = FarmlandMethods.KillCrop(Api.World, __instance.UpPos, (EnumCropStressType)i);
                    }
                }
            }
            
            return true;
        }

        [HarmonyPrefix]
        [HarmonyPatch(nameof(BlockEntityFarmland.TryGrowCrop))]
        public static bool TryGrowCropPrefix(ref BlockEntityFarmland __instance, ref bool __result, double currentTotalHours) {
            ICoreAPI Api = __instance.Api;

            if (!FarmlandMethods.BeforeTryGrowCropCaller(ref __instance, ref __result, currentTotalHours)) {
                return false;
            }

            return true;
        }

        [HarmonyPostfix]
        [HarmonyPatch(nameof(BlockEntityFarmland.TryGrowCrop))]
        public static void TryGrowCropPostfix(ref BlockEntityFarmland __instance, ref bool __result, double currentTotalHours) {
            FarmlandMethods.OnGrowthCaller(ref __instance, __result, currentTotalHours);
        }
    }
}
