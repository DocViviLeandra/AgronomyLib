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
    /// Base class for AgronomyLib's Block classes for crops (and things that pretend to be crops to fool the vanilla code)
    /// </summary>
    public abstract class BlockAgronomyCropBase : BlockCrop, ICropGrowing {

        public virtual bool TryGrowCrop(ICoreAPI api, IFarmlandBlockEntity farmland, double currentTotalHours, ref EnumHandling handling) {
            return true;
        }
    }
}
