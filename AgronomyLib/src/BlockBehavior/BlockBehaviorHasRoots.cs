using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vintagestory.API;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Config;
using Vintagestory.API.Datastructures;
using Vintagestory.API.MathTools;
using Vintagestory.GameContent;

// NOTE: This is obsolete, it is retained only for reference, and only for now

namespace AgronomyLib {
    /// <summary>
    /// Specifies the properties of the roots for the crop.
    /// Uses the code "AgronomyLib.HasRoots"
    /// </summary>
    /// <remarks>
    /// NOTE: When tracking RootStates, it will use the behavior from the most mature version of the crop, not the current one!
    /// </remarks>
    /// <example>
    /// <code lang = "json">
    /// "behaviors": [
    ///     {
    ///         "name": "AgronomyLib.HasRoots",
    ///         "properties": {
    ///             "survivesCrop": true,
    ///             "minStageToSurvive": 2,
    ///             
    ///             "coldDamageBelow": -12,
    ///             "heatDamageAbove": 30,
    ///             
    ///             "monthsToDecompose": 1,
    ///             
    ///             "rootShape": { "base": "agronomylib:rootdefault" },
    ///             "rootTexture": { "base": "agronomylib:rootdefault" },
    ///             "alwaysShowRoots": false,
    ///         }
    ///     }
    /// ]
    /// </code>
    /// </example>
    public class BlockBehaviorHasRoots : BlockBehavior {
        #region keys
        internal static readonly string className = "HasRoots";
        #endregion

        public static readonly string defaultRootModelString = "agronomylib:rootdefault";
        public static readonly int defaultMonthsToDecompose = 1;

        /// <summary>
        /// Whether or not the roots will survive past the parent crop's death.
        /// </summary>
        [DocumentAsJson("Optional", "true")]
        public bool SurvivesCrop = false;

        /// <summary>
        /// The minimum stage the crop must reach for the roots to survive even if the crop dies. Meaningless if SurvivesCrop is false.
        /// </summary>
        [DocumentAsJson("Optional", "1")]
        public int MinStageToSurvive = 1;

        /// <summary>
        /// The temperature below which the roots will die of cold damage, if different from the parent crop. If left unspecified, it will use the parent crop's ColdDamageBelow. Meaningless if SurvivesCrop is false.
        /// </summary>
        [DocumentAsJson("Optional")]
        public float DieBelowTemp;

        /// <summary>
        /// The temperature above which the roots will die of heat damage, if different from the parent crop. If left unspecified, it will use the parent crop's HeatDamageAbove. Meaningless if SurvivesCrop is false.
        /// </summary>
        [DocumentAsJson("Optional")]
        public float DieAboveTemp;

        /// <summary>
        /// The months it will take for the roots to decompose when the roots die.
        /// </summary>
        [DocumentAsJson("Optional", "1")]
        public int MonthsToDecompose = defaultMonthsToDecompose;

        /// <summary>
        /// The shape to use to render the roots. If left unspecified, the default shape will be used instead.
        /// </summary>
        [DocumentAsJson("Optional")]
        public CompositeShape RootShape;

        /// <summary>
        /// The textures to use when rendering the roots. If left unspecified, the textures in the shape will be used instead.
        /// </summary>
        [DocumentAsJson("Optional")]
        public CompositeTexture? RootTexture;

        /// <summary>
        /// Normally, roots are not rendered when there is a crop growing on the block. If this is true, the roots will always be rendered.
        /// </summary>
        [DocumentAsJson("Optional", "false")]
        public bool AlwaysShowRoots = false;

        public BlockBehaviorHasRoots(Block block) : base(block) { }

        public override void Initialize(JsonObject properties) {
            base.Initialize(properties);

            SurvivesCrop = properties["survivesCrop"].AsBool(false);

            DieBelowTemp = properties["coldDamageBelow"].AsFloat(float.MinValue);
            DieAboveTemp = properties["heatDamageAbove"].AsFloat(float.MaxValue);
            MonthsToDecompose = properties["monthsToDecompose"].AsInt(defaultMonthsToDecompose);
            
            RootShape = properties["rootShape"].AsObject<CompositeShape>(new CompositeShape() { Base = AssetLocation.Create(defaultRootModelString) });
            RootTexture = properties["rootTexture"].AsObject<CompositeTexture>();
            AlwaysShowRoots = properties["alwaysShowRoots"].AsBool(false);
        }

        public override void OnLoaded(ICoreAPI api) {
            base.OnLoaded(api);

            float defaultDieBelow = block.CropProps.ColdDamageBelow;
            float defaultDieAbove = block.CropProps.HeatDamageAbove;
            if (DieBelowTemp == float.MinValue) {
                DieBelowTemp = defaultDieBelow;
            }
            if (DieAboveTemp == float.MaxValue) {
                DieAboveTemp = defaultDieAbove;
            }
        }

        // TODO: Perhaps replace this with the CropBehavior OnPlanted(), since we don't actually want to call OnCropPlaced() if it isn't newly planted.
        public override void OnBlockPlaced(IWorldAccessor world, BlockPos blockPos, ref EnumHandling handling) {
            base.OnBlockPlaced(world, blockPos, ref handling);

            BlockEntity be = world.BlockAccessor.GetBlockEntity(blockPos.DownCopy());
            if (be?.GetBehavior<BlockEntityBehaviorAgronomyFarmland>() is BlockEntityBehaviorAgronomyFarmland beh) {
                beh.OnCropPlaced(block);
            }
        }

        public override void OnBlockRemoved(IWorldAccessor world, BlockPos pos, ref EnumHandling handling) {
            base.OnBlockRemoved(world, pos, ref handling);

            BlockEntity be = world.BlockAccessor.GetBlockEntity(pos.DownCopy());
            if (be?.GetBehavior<BlockEntityBehaviorAgronomyFarmland>() is BlockEntityBehaviorAgronomyFarmland beh) {
                beh.OnCropRemoved(block);
            }
        }
    }
}