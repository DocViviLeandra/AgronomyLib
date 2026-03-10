using System;
using System.Collections.Generic;
using System.Text;
using Vintagestory.API.Common;
using Vintagestory.API.MathTools;
using Vintagestory.GameContent;

namespace AgronomyLib {
    /// <summary>
    /// An interface which allows a <see cref="Block"/> that represents a crop to provide its own behavior that should be executed on crop death instead of the vanilla behavior. Vanilla behavior will not execute for Blocks implementing this interface!
    /// </summary>
    public interface IBlockCropDeath {
        protected static AssetLocation defaultDeadCrop = new AssetLocation("deadcrop");

        /// <summary>
        /// Called when something attempts to kill the crop represented by this block; returns false if it should be prevented from do so. MUST call <see cref="DoKillCrop"/>, if the crop is killed here.
        /// </summary>
        /// <remarks>
        /// Default implementation simply calls <see cref="DoKillCrop(IWorldAccessor, BlockPos, EnumCropStressType, BlockEntityFarmland?)"/> and then returns true.
        /// </remarks>
        /// <param name="world"></param>
        /// <param name="pos">The position of the crop.</param>
        /// <param name="deathReason">The cause of the crop's death.</param>
        /// <param name="farmland">The farmland on which the crop is growing, if it is growing on farmland.</param>
        /// <returns>false if something prevents the crop from dying</returns>
        public virtual bool TryKillCrop(IWorldAccessor world, BlockPos pos, EnumCropStressType deathReason, BlockEntityFarmland? farmland = null) {
            DoKillCrop(world, pos, deathReason, farmland);

            return true;
        }

        /// <summary>
        /// Called when something DOES kill the crop represented by this block; should handle all crop death logic.
        /// </summary>
        /// <remarks>
        /// Default implementation is identical to vanilla crop death behavior.
        /// </remarks>
        /// <param name="world"></param>
        /// <param name="pos">The position of the crop.</param>
        /// <param name="deathReason">The cause of the crop's death.</param>
        /// <param name="farmland">The farmland on which the crop is growing, if it is growing on farmland.</param>
        /// <returns>false if there remains a living crop here after this method executes.</returns>
        public virtual bool DoKillCrop(IWorldAccessor world, BlockPos pos, EnumCropStressType deathReason, BlockEntityFarmland? farmland = null) {
            Block cropBlock = farmland != null ? farmland.GetCrop() : world.BlockAccessor.GetBlock(pos);

            Block deadCropBlock = world.BlockAccessor.GetBlock(defaultDeadCrop);
            world.BlockAccessor.SetBlock(deadCropBlock.Id, pos);
            var be = world.BlockAccessor.GetBlockEntity(pos) as BlockEntityDeadCrop;
            be.Inventory[0].Itemstack = new ItemStack(cropBlock);
            be.deathReason = deathReason;
            return true;
        }

        /// <summary>
        /// Called after the crop has died. If, for some reason, this places a new crop in the old crop's space, it should return true! Otherwise, it should return false.
        /// </summary>
        /// <param name="world"></param>
        /// <param name="pos">The position of the crop.</param>
        /// <param name="deathReason">The cause of the crop's death.</param>
        /// <param name="farmland">The farmland on which the crop is growing, if it is growing on farmland.</param>
        /// <returns>true if there remains a living crop here after this method executes.</returns>
        public abstract bool OnCropDeath(IWorldAccessor world, BlockPos pos, EnumCropStressType deathReason, BlockEntityFarmland? farmland = null);
    }

    /// <summary>
    /// An interface which allows a <see cref="BlockBehavior"/> to specify behavior on the death of a crop, either instead of or in addition to default behavior.
    /// </summary>
    public interface IBehaviorCropDeath {
        /// <summary>
        /// 
        /// </summary>
        /// <remarks>
        /// This method may implement crop death logic of its own to replace the default logic. If so, make sure to return true and set <paramref name="handling"/> to <see cref="EnumHandling.PreventSubsequent"/>! AgronomyLib will assume novel death logic was implemented here with that combination.
        /// </remarks>
        /// <param name="world"></param>
        /// <param name="pos">The position of the crop</param>
        /// <param name="deathReason">The cause of the crop's death.</param>
        /// <param name="handling"></param>
        /// <param name="farmland">The farmland on which the crop is growing, if it is growing on farmland.</param>
        /// <returns>false if this behavior prevents the crop from being killed</returns>
        public abstract bool TryKillCrop(IWorldAccessor world, BlockPos pos, EnumCropStressType deathReason, ref EnumHandling handling, BlockEntityFarmland? farmland = null);

        /// <summary>
        /// Called after the crop has died. If, for some reason, this places a new crop in the old crop's space, it should return true! Otherwise, it should return false.
        /// </summary>
        /// <param name="world"></param>
        /// <param name="pos">The position of the crop.</param>
        /// <param name="deathReason">The cause of the crop's death.</param>
        /// <param name="farmland">The farmland on which the crop is growing, if it is growing on farmland.</param>
        /// <returns>true if there remains a living crop here after this method executes.</returns>
        public abstract bool OnCropDeath(IWorldAccessor world, BlockPos pos, EnumCropStressType deathReason, ref EnumHandling handling, BlockEntityFarmland? farmland = null);
    }
}
