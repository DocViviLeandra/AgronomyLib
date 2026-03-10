using AgronomyLib;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vintagestory.API.Common;
using Vintagestory.API.Common.Entities;
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
                BlockCropProperties? cProps = cropBlock?.GetCropProps(Api, __instance.UpPos);
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
                        hasCrop = !__instance.TryKillCrop((EnumCropStressType)i);

                        // Certain death behaviors may leave a living crop behind, so we need to check for that.
                        if (__instance.GetCrop() != null) {
                            hasCrop = true;
                        }
                    }
                }
            }
            
            return true;
        }

        [HarmonyPrefix]
        [HarmonyPatch(nameof(BlockEntityFarmland.TryGrowCrop))]
        public static bool TryGrowCropPrefix(ref BlockEntityFarmland __instance, ref bool __result, double currentTotalHours) {
            Block cropBlock = __instance.GetCrop();

            // Execute behaviors that run before TryGrowCrop
            __result = FarmlandMethods.BeforeTryGrowCropCaller(ref __instance,  cropBlock, currentTotalHours, out bool preventDefault);
            if (preventDefault) return false;
            
            if (cropBlock is IBlockCropGrowth blockCG) {
                // If the crop block implements IBlockCropGrowth, it provides its own logic for TryGrowCrop
                __result = blockCG.TryGrowCrop(__instance, currentTotalHours);
                return false;
            }

            return true;
        }

        [HarmonyPostfix]
        [HarmonyPatch(nameof(BlockEntityFarmland.TryGrowCrop))]
        public static void TryGrowCropPostfix(ref BlockEntityFarmland __instance, ref bool __result, double currentTotalHours) {
            Block cropBlock = __instance.GetCrop();

            // Execute behaviors that run after TryGrowCrop
            FarmlandMethods.OnGrowthCaller(ref __instance, cropBlock, __result, currentTotalHours);
        }
        

        [HarmonyPrefix]
        [HarmonyPatch(nameof(BlockEntityFarmland.GetCropStage))]
        public static bool GetCropStagePrefix(ref BlockEntityFarmland __instance, ref int __result, Block block) {
            __result = block.GetCurrentCropStage(__instance.Api.World, __instance.UpPos);
            return false;
        }

        /// <summary>
        /// Reimplementation which uses <see cref="FarmlandExtensions.TryKillCrop(BlockEntityFarmland, EnumCropStressType)"/>
        /// </summary>
        [HarmonyPrefix]
        [HarmonyPatch(nameof(BlockEntityFarmland.ConsumeOnePortion))]
        public static bool ConsumeOnePortionPrefix(ref BlockEntityFarmland __instance, ref float __result, Entity entity) {
            ICoreAPI Api = __instance.Api;

            Block cropBlock = __instance.GetCrop();
            if (cropBlock == null) {
                __result = 0;
                return false;
            }

            __instance.TryKillCrop(EnumCropStressType.Eaten);
            __result = 1f;
            return false;
        }
    }
}
