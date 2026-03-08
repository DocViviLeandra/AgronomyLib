using System;
using System.Collections.Generic;
using System.Text;
using Vintagestory.API;
using Vintagestory.API.Common;
using Vintagestory.API.Datastructures;
using Vintagestory.API.MathTools;
using Vintagestory.GameContent;

namespace AgronomyLib {
    public class BlockBehaviorSaltTolerant : BlockBehavior, ISaltExposureBehavior, ICropYieldBehavior {
        #region keys
        internal static readonly string className = "SaltTolerant";
        #endregion

        /// <summary>
        /// Multiplier to yield from being exposed to salt. Not all SaltTolerant crops LIKE salt!
        /// </summary>
        /// <remarks>
        /// Currently unimplemented.
        /// </remarks>
        [DocumentAsJson("Optional", "1.0f")]
        public float SaltExposureYieldMul = 1.0f;

        public BlockBehaviorSaltTolerant(Block block) : base(block) { }

        

        public override void Initialize(JsonObject properties) {
            base.Initialize(properties);

            SaltExposureYieldMul = properties["saltExposureYieldMul"].AsFloat(1.0f);
        }

        public virtual void OnSaltExposure(IWorldAccessor world, BlockPos pos, ref EnumHandling handled) {
            handled = EnumHandling.PreventDefault;

        }

        public float GetYieldMul(IWorldAccessor world, BlockPos pos) {
            return 1.0f;
        }
    }
}
