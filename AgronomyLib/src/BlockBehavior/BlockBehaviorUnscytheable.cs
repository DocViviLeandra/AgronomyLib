using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vintagestory.API.Common;

namespace AgronomyLib {

    /// <summary>
    /// Prevents a block with this behavior from being destroyed by the scythe, even if it would normally be able to.
    /// Uses Code "AgronomyLib.Unscytheable"
    /// </summary>
    /// <example>
    /// <code lang = "json">
    /// "behaviors": [
    ///     {
    ///         "name": "AgronomyLib.Unscytheable"
    ///     }
    /// ]
    /// </code>
    /// </example>
    public class BlockBehaviorUnscytheable : BlockBehavior {
        #region keys
        public static readonly string className = "Unscytheable";
        #endregion

        public BlockBehaviorUnscytheable(Block block) : base(block) { }
    }
}
