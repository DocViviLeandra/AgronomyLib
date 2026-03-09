using AgronomyLib.src.BlockEntity;
using System;
using System.Collections.Generic;
using System.Text;
using Vintagestory.API.Common;
using Vintagestory.API.MathTools;
using Vintagestory.GameContent;

namespace AgronomyLib {
    /// <summary>
    /// A block which, along with its associated block entity, contains a crop to which farmland updates are applied instead of the block. Allows a variety of trickery, such as beanpoles containing bean plants.
    /// </summary>
    public class BlockCropContainerBase : Block, IBlockProvidesCropProps {
        public virtual BlockCropProperties CropProperties(IWorldAccessor world, BlockPos pos) {
            BlockEntityCropContainerBase? be = world.BlockAccessor.GetBlockEntity(pos) as BlockEntityCropContainerBase;
            if (be == null) {
                world.Logger.Error("BlockCropContainerBase at position {0} cannot find associated BlockEntityCropContainerBase!", pos);
                return null;
            }

            return be.CropProps();
        }

        public BlockCropProperties CropProperties() => throw new NotImplementedException();

        public virtual int CurrentStage(IWorldAccessor world, BlockPos pos) {
            BlockEntityCropContainerBase? be = world.BlockAccessor.GetBlockEntity(pos) as BlockEntityCropContainerBase;
            if (be == null) {
                world.Logger.Error("BlockCropContainerBase at position {0} cannot find associated BlockEntityCropContainerBase!", pos);
                return 0;
            }

            return be.CurrentStage();
        }
    }
}
