using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Config;
using Vintagestory.API.Datastructures;
using Vintagestory.API.MathTools;
using Vintagestory.GameContent;

namespace AgronomyLib {
    /// <summary>
    /// Crops with this behavior will need to vernalize (as fruit trees do) in order to finish their development - when the crop reaches the stage specified with "maxStageBeforeVernalization", it will stop growing until it becomes vernalized.
    /// Uses the code "AgronomyLib.RequiresVernalization"
    /// </summary>
    /// <example>
    /// <code lang="json">
    /// "entityBehaviors": [
    ///     {
    ///         "name": "AgronomyLib.RequiresVernalization",
    ///         "properties": {
    ///             "maxStageBeforeVernalization": 4,
    ///             "minStageBeforeVernalization": 2,
    ///             "vernalizationHours": { "avg": 100, "var": 10 },
    ///             "vernalizationTemp": { "avg": 4, "var": 4 }
    ///         }
    ///     }
    /// ]
    /// </code>
    /// </example>
    public class BlockEntityBehaviorRequiresVernalization : BlockEntityBehavior, ICropGrowthBEBehavior, IFarmlandInfoProvider  {
        #region keys
        internal static readonly string className = "RequiresVernalization";
        #endregion

        public static double stateUpdateIntervalDays = 1 / 3.0;

        protected VernalizationState vState;

        public BlockEntity be => Blockentity;
        public BlockCrop crop => Block as BlockCrop;

        public BlockEntityBehaviorRequiresVernalization(BlockEntity be) : base(be) { }

        public override void Initialize(ICoreAPI api, JsonObject properties) {
            base.Initialize(api, properties);

            RegisterCropType(api, properties);

            BlockEntityFarmland bef;
        }

        public void RegisterCropType(ICoreAPI api, JsonObject properties) {
            var rnd = api.World.Rand;

            VernalizationProperties vProps = new VernalizationProperties() {
                MaxStageBeforeVernalization = properties["maxStageBeforeVernalization"].AsInt(1),
                MinStageBeforeVernalization = properties["minStageBeforeVernalization"].AsInt(0),
                VernalizationHours = properties["vernalizationHours"].AsObject<NatFloat>(NatFloat.createUniform(100, 10)),
                VernalizationTemp = properties["vernalizationTemp"].AsObject<NatFloat>(NatFloat.createUniform(4, 1))
            };

            vState = new VernalizationState() {
                MaxStageBeforeVernalization = vProps.MaxStageBeforeVernalization,
                MinStageBeforeVernalization = vProps.MinStageBeforeVernalization,
                VernalizationHours = vProps.VernalizationHours.nextFloat(1, rnd),
                VernalizationTemp = vProps.VernalizationTemp.nextFloat(1, rnd),

                lastTickTotalDays = api.World.Calendar.TotalDays,
                vernalizedHours = 0
            };

        }

        public VernalizationState UpdateAndGetVernalizationState() {
            // Code here is based primarily on updates for fruit trees!

            double totalDays = Api.World.Calendar.TotalDays;

            // If the crop isn't developed enough to vernalize anyways, we just reset vernalizedHours and move on
            if (crop.CurrentCropStage < vState.MinStageBeforeVernalization) {
                vState.vernalizedHours = 0;
                vState.lastTickTotalDays = totalDays;
                return vState;
            }

            // We don't do any updating if not enough time has passed to justify it
            if (totalDays - vState.lastTickTotalDays < stateUpdateIntervalDays) return vState;

            var baseClimate = Api.World.BlockAccessor.GetClimateAt(Pos, EnumGetClimateMode.WorldGenValues);
            if (baseClimate == null) return vState; // Region not yet loaded, we cannot continue

            int prevIntDays = -99;
            float middayTemp = 0;

            // Apply all updates to catch up to present
            while (totalDays - vState.lastTickTotalDays >= stateUpdateIntervalDays) {
                int intDays = (int)vState.lastTickTotalDays;

                // We don't need to check the midday temperature multiple times for the same day
                if (prevIntDays != intDays) {
                    double midday = intDays + 0.5;
                    middayTemp = Api.World.BlockAccessor.GetClimateAt(Pos, baseClimate, EnumGetClimateMode.ForSuppliedDate_TemperatureOnly, midday).Temperature;

                    // TODO: Apply greenhouse bonuses
                    // TODO: Account for cellar growth whenever I get around to adding nurseries and stuff for root crops
                }

                if (middayTemp <= vState.VernalizationTemp) {
                    vState.vernalizedHours += stateUpdateIntervalDays * Api.World.Calendar.HoursPerDay;
                }

                vState.lastTickTotalDays += stateUpdateIntervalDays;
            }

            return vState;
        }

        public virtual bool BeforeTryGrowCrop(ICoreAPI api, IFarmlandBlockEntity farmland, double currentTotalHours, ref EnumHandling handling) {
            VernalizationState vState = UpdateAndGetVernalizationState();

            if (crop.CurrentCropStage < vState.MaxStageBeforeVernalization) return true;
            else if (vState.IsVernalized) return true;
            else {
                handling = EnumHandling.PreventSubsequent;
                return false;
            }
        }

        public override void GetBlockInfo(IPlayer forPlayer, StringBuilder dsc) {
            base.GetBlockInfo(forPlayer, dsc);

            VernalizationState vState = UpdateAndGetVernalizationState();

            if (vState.IsVernalized) {
                dsc.AppendLine(string.Format("<font color={0}>Vernalized!</font>", ColorUtil.Int2Hex(GuiStyle.DamageColorGradient[99])));
            } else if (crop.CurrentCropStage >= vState.MaxStageBeforeVernalization) {
                dsc.AppendLine(string.Format("<font color={0}>Not Vernalized! Will not grow.</font>", ColorUtil.Int2Hex(GuiStyle.DamageColorGradient[1])));
            }
        }

        public virtual void GetFarmlandInfo(IPlayer forPlayer, StringBuilder dsc) {

            VernalizationState vState = UpdateAndGetVernalizationState();

            if (vState.IsVernalized) {
                dsc.AppendLine(string.Format("<font color={0}>Vernalized!</font>", ColorUtil.Int2Hex(GuiStyle.DamageColorGradient[99])));
            } else if (crop.CurrentCropStage >= vState.MaxStageBeforeVernalization) {
                dsc.AppendLine(string.Format("<font color={0}>Not Vernalized! Will not grow.</font>", ColorUtil.Int2Hex(GuiStyle.DamageColorGradient[1])));
            } else {
                dsc.AppendLine(string.Format("Will require vernalization to grow beyond stage {0}.", vState.MaxStageBeforeVernalization));
            }

            if (crop.CurrentCropStage < vState.MinStageBeforeVernalization) {
                dsc.AppendLine(string.Format("Too young to vernalize! Must grow to stage {0} first.", vState.MinStageBeforeVernalization));
            }
        }

        public override void FromTreeAttributes(ITreeAttribute tree, IWorldAccessor worldAccessForResolve) {
            base.FromTreeAttributes(tree, worldAccessForResolve);

            if (vState is null) {
                vState = new VernalizationState();
            }

            vState.FromTreeAttributes(tree.GetOrAddTreeAttribute(VernalizationState.AttributeKey));
        }

        public override void ToTreeAttributes(ITreeAttribute tree) {
            base.ToTreeAttributes(tree);

            var vStateTree = new TreeAttribute();
            vState.ToTreeAttributes(vStateTree);
            tree[VernalizationState.AttributeKey] = vStateTree;
        }
                
    }
}
