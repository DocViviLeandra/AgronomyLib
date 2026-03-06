using AgronomyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vintagestory.API;
using Vintagestory.API.Common;
using Vintagestory.API.Datastructures;
using Vintagestory.API.MathTools;
using Vintagestory.GameContent;

namespace AgronomyLib {
    /// <summary>
    /// When a crop with this behavior dies, it leaves behind a 'Roots' block that can sprout into a new instance of the crop.
    /// Uses code 'AgronomyLib.RegrowsFromRoots`.
    /// </summary>
    /// <example>
    /// <code lang = "json">
    /// "behaviors": [
    ///     {
    ///         "name": "AgronomyLib.RegrowsFromRoots",
    ///         "properties": {
    ///             "monthsToRegrow": 1,
    ///             "regrowAboveTemp": 1,
    ///             "regrowBelowTemp": 50,
    ///             "yieldMul": 0.75
    ///             "speedMul": 1.5
    ///         }
    ///     }
    /// ]
    /// </code>
    /// </example>
    public class BlockBehaviorRegrowsFromRoots : BlockBehaviorAgronomyCropBase, IRootsBehavior {
        #region keys
        internal static readonly string className = "RegrowsFromRoots";
        #endregion

        public static readonly float DefaultMonthsToRegrow = 1.0f;
        public static readonly float DefaultYieldMul = 1.0f;
        public static readonly float DefaultSpeedMul = 2f;

        /// <summary>
        /// The number of months it will take for the crop to regrow to the first stage
        /// </summary>
        [DocumentAsJson("Recommended", "1")]
        public float MonthsToRegrow = DefaultMonthsToRegrow;

        /// <summary>
        /// Minimum temperature for the crop to regrow. If unspecified, it's the crop's cold damage threshold (as long as that threshold is greater than or equal to 1).
        /// </summary>
        [DocumentAsJson("Recommended", "1")]
        public float RegrowAboveTemp = float.MinValue;

        /// <summary>
        /// Maximum temperature for the crop to regrow. By default, it's the same as the crop's heat damage threshold.
        /// </summary>
        [DocumentAsJson("Recommended", "50")]
        public float RegrowBelowTemp = float.MaxValue;

        /// <summary>
        /// Modifier to apply to crops that have regrown.
        /// </summary>
        [DocumentAsJson("Optional", "1")]
        public float YieldMul = DefaultYieldMul;

        /// <summary>
        /// Modifier to the rate at which the plant grows while it is regrowing (up to the highest stage it reached the first time)
        /// </summary>
        [DocumentAsJson("Recommended", "2")]
        public float SpeedMul = DefaultSpeedMul;

        

        public BlockBehaviorRegrowsFromRoots(Block block) : base(block) { }

        public override void Initialize(JsonObject properties) {
            base.Initialize(properties);

            MonthsToRegrow = properties["monthsToRegrow"].AsFloat(DefaultMonthsToRegrow);
            RegrowAboveTemp = properties["regrowAboveTemp"].AsFloat(float.MinValue);
            RegrowBelowTemp = properties["regrowBelowTemp"].AsFloat(float.MaxValue);
            YieldMul = properties["yieldMul"].AsFloat(DefaultYieldMul);
            SpeedMul = properties["speedMul"].AsFloat(DefaultSpeedMul);
            
        }

        public override void OnLoaded(ICoreAPI api) {
            base.OnLoaded(api);

            if (RegrowAboveTemp == float.MinValue) {
                RegrowAboveTemp = block?.CropProps.ColdDamageBelow ?? 1;
            }

            // Enforce a minimum regrow temp of 1
            if (RegrowAboveTemp < 1) {
                RegrowAboveTemp = 1;
            }

            if (RegrowBelowTemp == float.MaxValue) {
                RegrowBelowTemp = block?.CropProps.HeatDamageAbove ?? 50;
            }
        }

        public virtual bool OnRootsUpdate(IWorldAccessor world, BlockPos pos, ref RootState rootState, ref string currentCropRootsKey, float temp, ref EnumHandling handling) {
            if (rootState.crownDeadHours > MonthsToRegrow * world.Calendar.HoursPerDay * world.Calendar.DaysPerMonth) {
                if (temp > RegrowAboveTemp && temp < RegrowBelowTemp) {
                    BlockEntityFarmland? farmland = world.BlockAccessor.GetBlockEntity(pos) as BlockEntityFarmland;
                    if (farmland == null) return false;

                    Block cropToPlant = world.GetBlock(rootState.CropBlock.CodeWithVariant("stage", "1"));

                    // TODO: More robust checking/handling
                    if (farmland.CanPlant()) {
                        // Avoid triggering OnBlockPlaced because we're not actually creating a new crop, just regrowing it
                        world.BlockAccessor.ExchangeBlock(cropToPlant.BlockId, pos);
                        rootState.IsCurrentCrop = true;
                        currentCropRootsKey = rootState.GenerateID(world.Api);
                    }

                    return true;
                }
            }

            return false;
        }
    }
}
