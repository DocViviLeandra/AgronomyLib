using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vintagestory.API.Common;
using Vintagestory.API.MathTools;
using Vintagestory.GameContent;

namespace AgronomyLib {
    /// <summary>
    /// Base class for AgronomyLib's BlockEntityBehaviors for crops.
    /// </summary>
    public abstract class BlockBehaviorAgronomyCropBase : BlockBehavior, ICropGrowing {

        public BlockBehaviorAgronomyCropBase(Block block) : base(block) { }

        public virtual bool TryGrowCrop(ICoreAPI api, IFarmlandBlockEntity farmland, double currentTotalHours, ref EnumHandling handling) {
            return true;
        }

        public virtual void OnCropDeath(IWorldAccessor world, BlockPos pos, EnumCropStressType deathReason, ref EnumHandling handling) {

        }
    }
}
