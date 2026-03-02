using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vintagestory.API.Common;

namespace AgronomyLib {
    /// <summary>
    /// Base class for AgronomyLib's BlockEntityBehaviors for crops.
    /// </summary>
    public abstract class BlockEntityBehaviorAgronomyCropBase : BlockEntityBehavior {

        public BlockEntityBehaviorAgronomyCropBase(BlockEntity be) : base(be) { }

        public virtual bool TryGrowCrop(ICoreAPI api, IFarmlandBlockEntity farmland, double currentTotalHours, ref EnumHandling handling) {
            return true;
        }

        /// <summary>
        /// Get any info the behavior wants to add to the farmland.
        /// </summary>
        /// <param name="forPlayer"></param>
        /// <param name="dsc"></param>
        public virtual void GetFarmlandInfo(IPlayer forPlayer, StringBuilder dsc) {

        }
    }
}
