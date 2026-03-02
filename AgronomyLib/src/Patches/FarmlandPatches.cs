using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vintagestory.API.Common;
using Vintagestory.GameContent;

namespace AgronomyLib {

    [HarmonyPatch(typeof(BlockEntityFarmland))]
    public static class FarmlandPatches {

        [HarmonyPostfix]
        [HarmonyPatch(nameof(BlockEntityFarmland.GetBlockInfo))]
        public static void GetBlockInfoPostfix(ref BlockEntityFarmland __instance, IPlayer forPlayer, StringBuilder dsc) {
            // Make it actually get BlockInfo from each of its behaviors like every other BlockEntity in the game. Why doesn't it already do this? Beats me!
            foreach (BlockEntityBehavior behavior in __instance.Behaviors) {
                behavior.GetBlockInfo(forPlayer, dsc);
            }
        }

        [HarmonyPrefix]
        [HarmonyPatch(nameof(BlockEntityFarmland.TryGrowCrop))]
        public static bool TryGrowCropPrefix(ref BlockEntityFarmland __instance, bool __result, double currentTotalHours) {
            // Allows BlockEntityBehaviors attached to the crop entity to influence TryGrowCrop without requiring a separate CropBehavior.
            // Also allows such overrides to work even if the crop is "fully grown"!
            ICoreAPI api = __instance.Api;

            bool result = true;
            EnumHandling handling = EnumHandling.PassThrough;
            if (api.World.BlockAccessor.GetBlockEntity(__instance.UpPos) is BlockEntity be) {
                foreach (BlockEntityBehavior behavior in be.Behaviors) {
                    if (behavior is BlockEntityBehaviorAgronomyCropBase cropBehavior) {
                        result = cropBehavior.TryGrowCrop(api, __instance, currentTotalHours, ref handling);
                        if (handling == EnumHandling.PreventSubsequent) {
                            __result = result;
                            return false;
                        }
                    }
                }

                if (handling == EnumHandling.PreventDefault) {
                    __result = result;
                    return false;
                }
            }

            return true;
        }
    }
}
