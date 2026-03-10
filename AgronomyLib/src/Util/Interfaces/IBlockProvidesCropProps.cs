using System;
using System.Collections.Generic;
using System.Text;
using Vintagestory.API.Common;
using Vintagestory.API.MathTools;

namespace AgronomyLib {
    /// <summary>
    /// An interface which provides a method for providing <see cref="BlockCropProperties"/> through a method rather than by direct field access. Should only be added to a <see cref="Block"/>.
    /// </summary>
    public interface IBlockProvidesCropProps {
        public abstract BlockCropProperties CropProperties();

        public abstract BlockCropProperties CropProperties(IWorldAccessor world, BlockPos pos);

        public abstract int CropStage();

        public abstract int CropStage(IWorldAccessor world, BlockPos pos);
    }
}
