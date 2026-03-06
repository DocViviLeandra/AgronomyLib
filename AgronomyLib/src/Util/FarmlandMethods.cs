using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vintagestory.API.Common;
using Vintagestory.GameContent;

namespace AgronomyLib {
    public static class FarmlandMethods {

        public static bool TryGrowCropOverrides(ref BlockEntityFarmland __instance, ref bool __result, double currentTotalHours) {
            // Allows BlockEntityBehaviors attached to the crop entity to influence TryGrowCrop without requiring a separate CropBehavior.
            // Also allows such overrides to work even if the crop is "fully grown"!
            ICoreAPI api = __instance.Api;

            bool result = true;
            EnumHandling handling = EnumHandling.PassThrough;

            Block block = api.World.BlockAccessor.GetBlock(__instance.UpPos);

            // Iterate through BlockBehaviors and apply any that influence TryGrowCrop
            foreach (BlockBehavior bh in block.BlockBehaviors) {
                if (bh is ICropGrowing bhOverriding) {
                    result = bhOverriding.TryGrowCrop(api, __instance, currentTotalHours, ref handling);
                    if (handling == EnumHandling.PreventSubsequent) {
                        __result = result;
                        return false;
                    }
                }
            }

            // Apply anything done by the Block class itself
            if (block is ICropGrowing blockOverriding) {
                result = blockOverriding.TryGrowCrop(api, __instance, currentTotalHours, ref handling);
                if (handling == EnumHandling.PreventSubsequent) {
                    __result = result;
                    return false;
                }
            }

            // Apply anything done by the crop's entity's BlockEntityBehaviors
            if (api.World.BlockAccessor.GetBlockEntity(__instance.UpPos) is BlockEntity be) {
                foreach (BlockEntityBehavior behavior in be.Behaviors) {
                    if (behavior is ICropGrowing behOverriding) {
                        result = behOverriding.TryGrowCrop(api, __instance, currentTotalHours, ref handling);
                        if (handling == EnumHandling.PreventSubsequent) {
                            __result = result;
                            return false;
                        }
                    }
                }

                // Apply anything done by the crop's BlockEntity class
                if (be is ICropGrowing beOverriding) {
                    result = beOverriding.TryGrowCrop(api, __instance, currentTotalHours, ref handling);
                    if (handling == EnumHandling.PreventSubsequent) {
                        __result = result;
                        return false;
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
