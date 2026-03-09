using AgronomyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vintagestory.API.Common;
using Vintagestory.API.MathTools;
using Vintagestory.GameContent;

namespace AgronomyLib {
    internal static class FarmlandMethods {

        internal static bool BeforeTryGrowCropCaller(ref BlockEntityFarmland farmland, Block block, double currentTotalHours, out bool preventDefault) {
            // Allows BlockEntityBehaviors attached to the crop entity to influence TryGrowCrop without requiring a separate CropBehavior.
            // Also allows such overrides to work even if the crop is "fully grown"!
            ICoreAPI api = farmland.Api;

            bool result = true;
            preventDefault = false;

            // Iterate through BlockBehaviors and apply any that influence TryGrowCrop
            foreach (BlockBehavior behavior in block.BlockBehaviors) {
                if (behavior is IBehaviorCropGrowth behaviorCG) {
                    EnumHandling handling = EnumHandling.PassThrough;
                    bool tempResult = behaviorCG.BeforeTryGrowCrop(farmland, currentTotalHours, ref handling);
                    if (handling != EnumHandling.PassThrough) result = tempResult;
                    if (handling == EnumHandling.PreventDefault || handling == EnumHandling.PreventSubsequent) preventDefault = true;
                    if (handling == EnumHandling.PreventSubsequent) return result;
                }
            }

            // Apply anything done by the crop's entity's BlockEntityBehaviors
            if (api.World.BlockAccessor.GetBlockEntity(farmland.UpPos) is BlockEntity be) {
                foreach (BlockEntityBehavior behavior in be.Behaviors) {
                    if (behavior is IBlockEntityBehaviorCropGrowth bebehaviorCG) {
                        EnumHandling handling = EnumHandling.PassThrough;
                        bool tempResult = bebehaviorCG.BeforeTryGrowCrop(farmland, currentTotalHours, ref handling);
                        if (handling != EnumHandling.PassThrough) result = tempResult;
                        if (handling == EnumHandling.PreventDefault || handling == EnumHandling.PreventSubsequent) preventDefault = true;
                        if (handling == EnumHandling.PreventSubsequent) return result;
                    }
                }
            }

            return result;
        }

        internal static void OnGrowthCaller(ref BlockEntityFarmland farmland, Block block, bool wasGrown, double currentTotalHours) {
            ICoreAPI api = farmland.Api;

            bool preventDefault = false;

            // Iterate through BlockBehaviors and apply any that influence TryGrowCrop
            foreach (BlockBehavior behavior in block.BlockBehaviors) {
                if (behavior is IBehaviorCropGrowth behaviorCG) {
                    EnumHandling handling = EnumHandling.PassThrough;
                    behaviorCG.OnGrowth(farmland, currentTotalHours, wasGrown, ref handling);
                    if (handling == EnumHandling.PreventDefault || handling == EnumHandling.PreventSubsequent) preventDefault = true;
                    if (handling == EnumHandling.PreventSubsequent) return;
                }
            }

            // Apply anything done by the crop's entity's BlockEntityBehaviors
            if (api.World.BlockAccessor.GetBlockEntity(farmland.UpPos) is BlockEntity be) {
                foreach (BlockEntityBehavior behavior in be.Behaviors) {
                    if (behavior is IBlockEntityBehaviorCropGrowth bebehaviorCG) {
                        EnumHandling handling = EnumHandling.PassThrough;
                        bool tempResult = bebehaviorCG.BeforeTryGrowCrop(farmland, currentTotalHours, ref handling);
                        if (handling == EnumHandling.PreventDefault || handling == EnumHandling.PreventSubsequent) preventDefault = true;
                        if (handling == EnumHandling.PreventSubsequent) return;
                    }
                }
            }
        }

        private static AssetLocation defaultDeadCrop = new AssetLocation("deadcrop");
        internal static bool KillCrop(IWorldAccessor world, BlockPos pos, EnumCropStressType deathReason) {
            Block cropBlock = world.BlockAccessor.GetBlock(pos);

            EnumHandling handling = EnumHandling.PassThrough;
            bool result = false;
            foreach(BlockBehavior behavior in cropBlock.BlockBehaviors) {
                if (behavior is IBehaviorCropDeath cropDeathBehavior) {
                    result = cropDeathBehavior.OnCropDeath(world, pos, deathReason, ref handling);
                    if (handling == EnumHandling.PreventSubsequent) {
                        return result;
                    }
                }
            }

            if (handling == EnumHandling.PreventDefault) return result;

            // Default Behavior
            Block deadCropBlock = world.BlockAccessor.GetBlock(defaultDeadCrop);
            world.BlockAccessor.SetBlock(deadCropBlock.Id, pos);
            var be = world.BlockAccessor.GetBlockEntity(pos) as BlockEntityDeadCrop;
            be.Inventory[0].Itemstack = new ItemStack(cropBlock);
            be.deathReason = deathReason;
            return false;
        }
    }
}
