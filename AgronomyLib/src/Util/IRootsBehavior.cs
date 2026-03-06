using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vintagestory.API.Common;
using Vintagestory.API.MathTools;

namespace AgronomyLib {
    public interface IRootsBehavior {

        public virtual bool OnRootsUpdate(IWorldAccessor world, BlockPos pos, ref RootState rootState, ref string currentCropRootsKey, float temp, ref EnumHandling handling) {
            return true;
        }
    }
}
