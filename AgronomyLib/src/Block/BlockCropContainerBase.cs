using AgronomyLib.src.BlockEntity;
using System;
using System.Collections.Generic;
using System.Text;
using Vintagestory.API.Common;
using Vintagestory.API.Config;
using Vintagestory.API.MathTools;
using Vintagestory.GameContent;

namespace AgronomyLib {
    /// <summary>
    /// A block which, along with its associated block entity, contains a crop to which farmland updates are applied instead of the block. Allows a variety of trickery, such as beanpoles containing bean plants.
    /// </summary>
    public class BlockCropContainerBase : Block, IBlockProvidesCropProps, IBlockCropGrowth {
        public virtual BlockCropProperties CropProperties() => null; // This isn't a crop if there isn't a crop in it!

        public virtual BlockCropProperties CropProperties(IWorldAccessor world, BlockPos pos) {
            BlockEntityCropContainerBase? be = world.BlockAccessor.GetBlockEntity(pos) as BlockEntityCropContainerBase;
            if (be == null) {
                world.Logger.Error("BlockCropContainerBase at position {0} cannot find associated BlockEntityCropContainerBase!", pos);
                return null;
            }

            return be.CropProps();
        }

        public virtual int CropStage() => 0; // This isn't a crop if there isn't a crop in it!

        public virtual int CropStage(IWorldAccessor world, BlockPos pos) {
            BlockEntityCropContainerBase? be = world.BlockAccessor.GetBlockEntity(pos) as BlockEntityCropContainerBase;
            if (be == null) {
                world.Logger.Error("BlockCropContainerBase at position {0} cannot find associated BlockEntityCropContainerBase!", pos);
                return 0;
            }

            return be.CurrentStage();
        }

        public bool TryGrowCrop(BlockEntityFarmland farmland, double currentTotalHours) {
            BlockEntityCropContainerBase? be = farmland.Api.World.BlockAccessor.GetBlockEntity(farmland.UpPos) as BlockEntityCropContainerBase;
            if (be == null) {
                farmland.Api.Logger.Error("BlockCropContainerBase at position {0} cannot find associated BlockEntityCropContainerBase!", farmland.UpPos);
                return false;
            }

            return be.TryGrowCrop(farmland, currentTotalHours);
        }
    }
}
