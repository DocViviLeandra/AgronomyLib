using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vintagestory.API.Common;

namespace AgronomyLib {

    /// <summary>
    /// Allows something to specify additional info that should be displayed in the farmland it is attached to.
    /// </summary>
    public interface IFarmlandInfoProvider  {
        /// <summary>
        /// Get any info the behavior wants to add to the farmland.
        /// </summary>
        /// <param name="forPlayer"></param>
        /// <param name="dsc"></param>
        public abstract void GetFarmlandInfo(IPlayer forPlayer, StringBuilder dsc);
    }
}
