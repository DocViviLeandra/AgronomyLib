using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using Vintagestory.API.Common;
using Vintagestory.API.Config;
using Vintagestory.API.Datastructures;
using Vintagestory.API.MathTools;
using Vintagestory.GameContent;

namespace AgronomyLib {

    [HarmonyPatch(typeof(BlockEntitySoilNutrition))]
    public static class BlockEntitySoilNutritionPatches {
        #region helpers
        private class BESoilNutritionHelper : BlockEntitySoilNutrition {
            /// <remarks>I know this is cursed, tell Anego Studios to make EnumWaterSearchResult public!</remarks>
            public static dynamic? EnumWaterSearchResultGetter(int val) { 
                switch (val) {
                    case 0: return EnumWaterSearchResult.Found; break;
                    case 1: return EnumWaterSearchResult.NotFound; break;
                    case 2: return EnumWaterSearchResult.Deferred; break;
                }

                return null;
            }
        }
        #endregion

        private static dynamic? EWSRFound => BESoilNutritionHelper.EnumWaterSearchResultGetter(0);
        private static dynamic? EWSRNotFound => BESoilNutritionHelper.EnumWaterSearchResultGetter(1);
        private static dynamic? EWSRDeferred => BESoilNutritionHelper.EnumWaterSearchResultGetter(2);


        [HarmonyPostfix]
        [HarmonyPatch(nameof(BlockEntitySoilNutrition.OnCropBlockBroken))]
        public static void OnCropBlockBrokenPostfix(ref bool ___saltExposed) {
            ___saltExposed = false;
        }

        static FastInvokeHandler GetNearbyWaterDistanceHandler = HarmonyLib.MethodInvoker.GetHandler(AccessTools.Method("GetNearbyWaterDistance"), true);

        [HarmonyPrefix]
        [HarmonyPatch("GetNearbyWaterDistance")]
        public static bool GetNearbyWaterDistancePrefix(ref BlockEntitySoilNutrition __instance, ref float __result, ref double ___lastWaterSearchedTotalHours, ref bool ___farmlandIsAtChunkEdge, ref bool ___saltExposed, ref float[] ___damageAccum, out object result, float hoursPassed) {
            ICoreAPI Api = __instance.Api;
            BlockPos Pos = __instance.Pos;
            bool farmlandIsAtChunkEdge = ___farmlandIsAtChunkEdge;

            // 1. Watered check
            float waterDistance = 99;
            farmlandIsAtChunkEdge = false;

            bool saltWater = false;

            Api.World.BlockAccessor.SearchFluidBlocks(
                new BlockPos(Pos.X - 4, Pos.Y, Pos.Z - 4),
                new BlockPos(Pos.X + 4, Pos.Y, Pos.Z + 4),
                (block, pos) => {
                    if (block.LiquidCode == "saltwater") {
                        saltWater = true;
                    }

                    if (block.LiquidCode == "water" || saltWater) {
                        waterDistance = Math.Min(waterDistance, Math.Max(Math.Abs(pos.X - Pos.X), Math.Abs(pos.Z - Pos.Z)));
                    }

                    return true;
                },
                (cx, cy, cz) => farmlandIsAtChunkEdge = true
            );

            ___farmlandIsAtChunkEdge = farmlandIsAtChunkEdge;

            if (saltWater) {
                ___saltExposed = true;
                Block upblock = Api.World.BlockAccessor.GetBlock(__instance.UpPos);
                // Perform salt exposure reactions
                EnumHandling handled = EnumHandling.PassThrough;
                foreach (BlockBehavior blockBehavior in upblock.BlockBehaviors) {
                    if (blockBehavior is IBehaviorSaltExposure seBlockBehavior) {
                        seBlockBehavior.OnSaltExposure(Api.World, __instance.UpPos, ref handled);
                        if (handled == EnumHandling.PreventSubsequent) break;
                    }
                }

                if (handled != EnumHandling.PreventSubsequent && handled != EnumHandling.PreventDefault) {
                    ___damageAccum[(int)(EnumCropStressType.Salt)] += hoursPassed;
                }
            }

            result = EWSRDeferred;
            if (farmlandIsAtChunkEdge) {
                __result = 99;
                return false;
            }

            ___lastWaterSearchedTotalHours = Api.World.Calendar.TotalHours;

            if (waterDistance < 4f) {
                result = EWSRFound;
                __result = waterDistance;
                return false;
            }

            result = EWSRNotFound;
            __result = 99;
            return false;
        }

        [HarmonyPostfix]
        [HarmonyPatch(nameof(BlockEntitySoilNutrition.ToTreeAttributes))]
        public static void ToTreeAttributesPostfix(bool ___saltExposed, ITreeAttribute tree) {
            tree.SetBool("saltExposed", ___saltExposed);
        }


    }
}

