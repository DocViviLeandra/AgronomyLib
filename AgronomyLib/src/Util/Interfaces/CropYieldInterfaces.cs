using System;
using System.Collections.Generic;
using System.Text;
using Vintagestory.API.Common;
using Vintagestory.API.MathTools;

namespace AgronomyLib {

    /// <summary>
    /// Interface that allows a <see cref="BlockBehavior"/> to modify the yield of a crop.
    /// </summary>
    public interface IBehaviorCropYield {
        /// <summary>
        /// Returns a multiplier to be applied to the crop's yield
        /// </summary>
        /// <param name="world"></param>
        /// <param name="pos">The position of the crop</param>
        /// <returns></returns>
        public abstract float GetYieldMul(IWorldAccessor world, BlockPos pos);
    }

    /// <summary>
    /// Interface that allows a <see cref="BlockEntityBehavior"/> to modify the yield of a crop.
    /// </summary>
    public interface IBEBehaviorCropYield {
        /// <summary>
        /// Returns a multiplier to be applied to the crop's yield
        /// </summary>
        /// <param name="currentTotalHours"></param>
        /// <returns></returns>
        public abstract float GetYieldMul();
    }
}
