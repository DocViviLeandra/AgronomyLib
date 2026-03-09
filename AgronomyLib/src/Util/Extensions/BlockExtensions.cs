using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using Vintagestory.API.Common;
using Vintagestory.API.Config;
using Vintagestory.API.MathTools;
using Vintagestory.GameContent;

namespace AgronomyLib {
    public static class BlockExtensions {
        /// <summary>
        /// An extension method allowing a <see cref="Block"/>'s <see cref="BlockCropProperties"/> to be retrieved in a way that can be overridden, rather than via direct access to <see cref="Block.CropProps"/>. This one retrieves it for the general type of block.
        /// </summary>
        /// <param name="block">The <see cref="Block"/> this extension method is being invoked on</param>
        /// <returns>The <see cref="BlockCropProperties"/> of the <see cref="Block"/>. If the <see cref="Block"/> implements <see cref="IBlockProvidesCropProps"/>, it will retrieve the <see cref="BlockCropProperties"/> via the method it provides, instead of direct access.</returns>
        public static BlockCropProperties GetCropProps(this Block block) {
            if (block is IBlockProvidesCropProps cropPropsProvider) {
                return cropPropsProvider.CropProperties();
            } else {
                return block.CropProps;
            }
        }

        /// <summary>
        /// An extension method allowing a <see cref="Block"/>'s <see cref="BlockCropProperties"/> to be retrieved in a way that can be overridden, rather than via direct access to <see cref="Block.CropProps"/>. This one retrieves it for the specific Block at the specified position.
        /// </summary>
        /// <remarks>
        /// AgronomyLib's <see cref="FarmlandTranspilerPatch"/> replaces all references in <see cref="BlockEntitySoilNutrition"/> and <see cref="BlockEntityFarmland"/> to <see cref="Block.CropProps"/> with calls to this method.
        /// </remarks>
        /// <param name="block">The <see cref="Block"/> this extension method is being invoked on</param>
        /// <param name="Api"></param>
        /// <param name="pos">The position of the instance of the <see cref="Block"/></param>
        /// <returns>The <see cref="BlockCropProperties"/> of the <see cref="Block"/>. If the <see cref="Block"/> implements <see cref="IBlockProvidesCropProps"/>, it will retrieve the <see cref="BlockCropProperties"/> via the method it provides, instead of direct access.</returns>
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static BlockCropProperties GetCropProps(this Block block, ICoreAPI Api, BlockPos pos) {
            if (block is IBlockProvidesCropProps cropPropsProvider) {
                return cropPropsProvider.CropProperties(Api.World, pos);
            } else {
                return block.CropProps;
            }
        }

        public static int GetCurrentCropStage(this Block block, IWorldAccessor world, BlockPos pos) {
            if (block is IBlockProvidesCropProps cropPropsProvider) {
                return cropPropsProvider.CurrentStage(world, pos);
            } else if (block is BlockCrop cropBlock) {
                return cropBlock.CurrentStage();
            } else {
                int.TryParse(block.LastCodePart(), out int stage);
                return stage;
            }
        }
    }
}
