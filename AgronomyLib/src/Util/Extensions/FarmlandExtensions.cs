using System;
using System.Collections.Generic;
using System.Text;
using Vintagestory.API.Common;
using Vintagestory.API.Config;
using Vintagestory.GameContent;

namespace AgronomyLib {
    /// <summary>
    /// Class containing extension methods that AgronomyLib adds to <see cref="BlockEntityFarmland"/>.
    /// </summary>
    public static class FarmlandExtensions {
        private static AssetLocation defaultDeadCrop = new AssetLocation("deadcrop");

        /// <summary>
        /// Attempts to kill the crop on the farmland, applying all crop death logic.
        /// </summary>
        /// <param name="farmland"></param>
        /// <param name="deathReason">The cause of the crop's death.</param>
        /// <returns>Whether the crop on the farmland died.</returns>
        public static bool TryKillCrop(this BlockEntityFarmland farmland, EnumCropStressType deathReason) {
            ICoreAPI Api = farmland.Api;
            Block cropBlock = farmland.GetCrop();
            if (cropBlock == null) return false;

            bool shouldKill = true;
            bool didKill = false;
            bool preventDefault = false;
            foreach (BlockBehavior behavior in cropBlock.BlockBehaviors) {
                EnumHandling handling = EnumHandling.PassThrough;
                if (behavior is IBehaviorCropDeath behaviorCD) {
                    bool tempResult = behaviorCD.TryKillCrop(Api.World, farmland.UpPos, deathReason, ref handling, farmland);
                    if (handling != EnumHandling.PassThrough) shouldKill = tempResult;
                    if (handling == EnumHandling.PreventDefault) preventDefault = true;
                    if (handling == EnumHandling.PreventSubsequent) {
                        if (shouldKill) {
                            // If the behavior returned true but is preventing subsequent, we assume it implemented its own death logic. That means we still need to call OnCropDeath()!
                            preventDefault = true;
                            didKill = true;
                            break;
                        } else return false;
                    }
                }
            }

            if (preventDefault) {
                if (!didKill) return false; // Once again, if we still killed the crop but the default behavior is prevented, we still need to call OnCropDeath()
            } else {
                if (cropBlock is IBlockCropDeath blockCD) {
                    didKill = blockCD.TryKillCrop(Api.World, farmland.UpPos, deathReason);
                } else {
                    // Default crop death behavior
                    Block deadCropBlock = Api.World.BlockAccessor.GetBlock(defaultDeadCrop);
                    Api.World.BlockAccessor.SetBlock(deadCropBlock.Id, farmland.UpPos);
                    var be = Api.World.BlockAccessor.GetBlockEntity(farmland.UpPos) as BlockEntityDeadCrop;
                    be.Inventory[0].Itemstack = new ItemStack(cropBlock);
                    be.deathReason = deathReason;
                    didKill = true;
                }
            }

            if (didKill) {
                bool livingCrop = false;
                // reuse the same variable for OnCropDeath()
                preventDefault = false;
                foreach (BlockBehavior behavior in cropBlock.BlockBehaviors) {
                    if (behavior is IBehaviorCropDeath behaviorCD) {
                        EnumHandling handling = EnumHandling.PassThrough;
                        bool tempResult = behaviorCD.OnCropDeath(Api.World, farmland.UpPos, deathReason, ref handling, farmland);
                        if (handling != EnumHandling.PassThrough) livingCrop = tempResult;
                        if (handling == EnumHandling.PreventDefault) preventDefault = true;
                        if (handling == EnumHandling.PreventSubsequent) return true;
                    }
                }

                if (preventDefault) return true;

                if (cropBlock is IBlockCropDeath cropDeath) {
                    livingCrop = cropDeath.OnCropDeath(Api.World, farmland.UpPos, deathReason, farmland);
                }

                return true;
            } else return false;

        }
    }
}
