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

    public class RootState {
        private static readonly CompositeShape defaultRootShape = new CompositeShape() { Base = AssetLocation.Create(BlockBehaviorHasRoots.defaultRootModelString)};

        protected string? id = null;

        protected BlockPos pos; // This should never change after the root is created.

        protected bool isCurrentCrop;
        protected bool crownAlive;

        protected Block cropBlock; // Should never be accessed directly, use CropBlock instead to make sure behaviors update as well
        protected BlockBehaviorHasRoots? baseBehavior;

        public double crownDeadHours;

        /// <summary>
        /// For tracking miscellaneous extra data anything might like to attach
        /// </summary>
        public ITreeAttribute extraAttributes;

        public BlockPos Pos {
            get => pos;
        }

        /// <summary>
        /// True if the roots belong to the crop currently growing on the farmland. Currently, identical to CrownAlive.
        /// </summary>
        public bool IsCurrentCrop {
            get => isCurrentCrop;
            set => isCurrentCrop = CrownAlive = value;
        }

        /// <summary>
        /// True if aboveground portion of the crop is alive
        /// </summary>
        public bool CrownAlive {
            get => crownAlive;
            set {
                if (value) {
                    crownAlive = isCurrentCrop = true;
                    crownDeadHours = 0;
                } else {
                    crownAlive = isCurrentCrop = false;
                }
            }
        }

        public Block CropBlock {
            get => cropBlock;
            set {
                cropBlock = value;
                baseBehavior = value.GetBehavior<BlockBehaviorHasRoots>();
            }
        }

        

        public int RootStage {
            get {
                if (cropBlock is BlockCrop crop) {
                    return crop.CurrentCropStage;

                } else if (cropBlock.CropProps != null) {
                    // if the block isn't a BlockCrop, but does have CropProps, we try to manually get the stage
                    int rootStage;
                    if (!int.TryParse(cropBlock.LastCodePart(), out rootStage)) {
                        throw new ArgumentException($"Cannot determine the stage of Block {cropBlock}!");
                    }

                    return rootStage;

                } else {
                    throw new ArgumentException($"Cannot determine the stage of {cropBlock} as it lacks CropProps!");
                }
            }
        }

        

        #region BaseBehavior Accessors
        public BlockBehaviorHasRoots? BaseBehavior { get => baseBehavior; }
        public bool SurvivesCrop { get => baseBehavior?.SurvivesCrop ?? false; }
        public int MinStageToSurvive { get => baseBehavior?.MinStageToSurvive ?? 99; }
        public float DieBelowTemp { get => baseBehavior?.DieBelowTemp ?? cropBlock.CropProps.ColdDamageBelow; }
        public float DieAboveTemp { get => baseBehavior?.DieAboveTemp ?? cropBlock.CropProps.HeatDamageAbove; }
        public float MonthsToDecompose { get => baseBehavior?.MonthsToDecompose ?? BlockBehaviorHasRoots.defaultMonthsToDecompose; }
        public CompositeShape? RootShape { get => baseBehavior?.RootShape ?? defaultRootShape; }
        public bool AlwaysShowRoots { get =>  baseBehavior?.AlwaysShowRoots ?? false; }
        #endregion

        protected RootState() { }

        /// <summary>
        /// Generate an ID by which to identify this RootState. Subsequent invocations will return the same ID.
        /// </summary>
        /// <param name="api"></param>
        /// <returns></returns>
        public string GenerateID(ICoreAPI api) {
            if (id == null) {
                return id = $"{CropBlock.BlockId}:{api.World.Calendar.TotalHours}";
            } else return id;
        }

        /// <summary>
        /// Factory method to create a new RootState
        /// </summary>
        /// <param name="pos">The position of the farmland in which the roots are embedded</param>
        /// <param name="cropBlock">The crop the roots belong to</param>
        /// <param name="isCurrentCrop">Whether the roots belong to the crop currently on the farmland</param>
        /// <returns></returns>
        public static RootState Create(ICoreAPI api, BlockPos pos, Block cropBlock, bool isCurrentCrop) {
            RootState state = new RootState() {
                pos = pos,
                CropBlock = cropBlock,
                crownAlive = isCurrentCrop,
                crownDeadHours = 0,
            };

            state.GenerateID(api);

            return state;
        }

        /// <summary>
        /// Factory method to generate a RootState from an ITreeAtttribute (typically, when receiving a newly-created one from the server)
        /// </summary>
        /// <param name="api"></param>
        /// <param name="tree"></param>
        /// <returns></returns>
        public static RootState CreateFromTreeAttributes(ITreeAttribute tree, IWorldAccessor worldForResolve) {
            return new RootState() {
                id = tree.GetString("id"),
                CropBlock = worldForResolve.GetBlock(tree.GetInt("cropBlockId")),
                pos = tree.GetBlockPos("pos"),

                crownAlive = tree.GetBool("crownAlive"),
                crownDeadHours = tree.GetDouble("crownDeadHours"),

                extraAttributes = tree.GetTreeAttribute("extraAttributes"),
            };
        }

        public bool UpdateCropStage(ICoreAPI api, int newStage) {
            if (newStage >= RootStage) {
                Block newBlock = api.World.GetBlock(cropBlock.CodeWithVariant("stage", newStage.ToString()));
                if (newBlock != null) {
                    CropBlock = newBlock;
                    return true;
                }
            }

            return false;
        }
        

        public void FromTreeAttributes(ITreeAttribute tree, IWorldAccessor worldForResolve) {
            id = tree.GetString("id");
            CropBlock = worldForResolve.GetBlock(tree.GetInt("cropBlockId"));
            pos = tree.GetBlockPos("pos");

            crownAlive = tree.GetBool("crownAlive");
            crownDeadHours = tree.GetDouble("crownDeadHours");

            extraAttributes = tree.GetTreeAttribute("extraAttributes");
        }

        public void ToTreeAttributes(ITreeAttribute tree) {
            tree.SetString("id", id);

            tree.SetInt("cropBlockId", CropBlock.BlockId);

            tree.SetBool("crownAlive", crownAlive);

            tree.SetDouble("crownDeadHours", crownDeadHours);

            tree.GetOrAddTreeAttribute("extraAttributes");
            tree["extraAttributes"] = extraAttributes;
        }

        public override string ToString() {
            return Lang.Get("{0} roots, stage {1}", Lang.Get(cropBlock.Code), RootStage);
        }
    }
}
