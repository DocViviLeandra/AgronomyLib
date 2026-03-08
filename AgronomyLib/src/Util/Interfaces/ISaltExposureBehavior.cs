using System;
using System.Collections.Generic;
using System.Text;
using Vintagestory.API.Common;
using Vintagestory.API.MathTools;

namespace AgronomyLib {

    /// <summary>
    /// Interface to indicate a <see cref="BlockBehavior"/> (for crops) has some special interaction with being exposed to salt, instead of or in addition to taking damage from it.
    /// </summary>
    public interface ISaltExposureBehavior {

        public abstract void OnSaltExposure(IWorldAccessor world, BlockPos pos, ref EnumHandling handled);
    }
}
