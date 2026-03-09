using System;
using System.Collections.Generic;
using System.Text;
using Vintagestory.API.Common;
using Vintagestory.API.MathTools;
using Vintagestory.GameContent;

namespace AgronomyLib {
    /// <summary>
    /// An interface which allows a <see cref="BlockBehavior"/> to specify behavior on the death of a crop, either instead of or in addition to default behavior
    /// </summary>
    public interface IBehaviorCropDeath {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="world"></param>
        /// <param name="pos">The position of the crop</param>
        /// <param name="deathReason"></param>
        /// <param name="handling"></param>
        /// <returns><see cref="true"/> if there is still a living crop on this farmland after this.</returns>
        public abstract bool OnCropDeath(IWorldAccessor world, BlockPos pos, EnumCropStressType deathReason, ref EnumHandling handling);
    }
}
