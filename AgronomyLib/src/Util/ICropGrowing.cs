using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vintagestory.API.Common;
using Vintagestory.API.MathTools;

namespace AgronomyLib {

    /// <summary>
    /// An interface containing a method to override TryGrowCrop, which can be added to any Block, BlockEntity, or BlockBehavior to enable it to do so
    /// </summary>
    public interface ICropGrowing {
        public abstract bool TryGrowCrop(ICoreAPI api, IFarmlandBlockEntity farmland, double currentTotalHours, ref EnumHandling handling);


    }
}
