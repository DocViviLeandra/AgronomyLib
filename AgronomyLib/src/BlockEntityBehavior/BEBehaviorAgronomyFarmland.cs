using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vintagestory.API.Common;
using Vintagestory.API.Datastructures;
using Vintagestory.API.MathTools;
using Vintagestory.GameContent;

namespace AgronomyLib {
    /// <summary>
    /// A behavior added to farmland by AgronomyLib to track data important to other functions of the library. Shouldn't be added to anything else.
    /// Uses code "AgronomyLib.AgronomyFarmland".
    /// </summary>
    public class BlockEntityBehaviorAgronomyFarmland : BlockEntityBehavior {
        #region keys
        internal static readonly string className = "AgronomyFarmland";
        #endregion

        public static double stateUpdateIntervalDays = 1 / 3.0;

        /// <summary>
        /// Indexes rootStates by the time they were created and their original blockId
        /// </summary>
        protected Dictionary<string, RootState> rootStates = new Dictionary<string, RootState>();
        protected string? currentCropRootStateKey;
        public double lastRootTickTotalDays;

        public IFarmlandBlockEntity farmland => Blockentity as IFarmlandBlockEntity;

        public BlockEntityBehaviorAgronomyFarmland(BlockEntity be) : base(be) { }

        public bool OnCropPlaced(Block block) {
            // Initialize a root state for the current crop
            RootState newState = RootState.Create(Api, Pos, block, true);
            string key = newState.GenerateID(Api);
            rootStates.Add(key, newState);
            currentCropRootStateKey = key;

            return true;
        }

        public bool OnCropRemoved(Block block) {
            // Mark the root state for the current crop as having lost its crown, then delink it
            if (currentCropRootStateKey != null) {
                rootStates[currentCropRootStateKey].CrownAlive = false;
                currentCropRootStateKey = null;
            }

            return true;
        }

        public Dictionary<string, RootState> UpdateAndGetRootStates() {
            double totalDays = Api.World.Calendar.TotalDays;

            // We don't do any updating if not enough time has passed to justify it
            if (totalDays - lastRootTickTotalDays < stateUpdateIntervalDays) return rootStates;

            var baseClimate = Api.World.BlockAccessor.GetClimateAt(Pos, EnumGetClimateMode.WorldGenValues);
            if (baseClimate == null) return rootStates; // Region not yet loaded, we cannot continue

            // TODO: Get greenhouse temp bonus

            int prevIntDays = -99;
            float temp = 0;
            bool markDirty = false;

            while (totalDays - lastRootTickTotalDays >= stateUpdateIntervalDays) {
                int intDays = (int)lastRootTickTotalDays;

                // Avoid reading the same temp over and over again
                if (prevIntDays != intDays) {
                    // For roughly daily average temps

                    double midday = intDays + 0.5;
                    temp = Api.World.BlockAccessor.GetClimateAt(Pos, baseClimate, EnumGetClimateMode.ForSuppliedDate_TemperatureOnly, midday).Temperature;
                    // TODO: Apply greenhouse temp bonus

                    prevIntDays = intDays;
                }

                List<string> toRemove = new List<string>();
                
                foreach (string key in rootStates.Select(x => x.Key) ) {
                    RootState state = rootStates[key];
                    Block block = state.CropBlock;
                    EnumHandling handling = EnumHandling.PassThrough;
                    foreach (BlockBehavior bh in block.BlockBehaviors) {
                        if (bh is IRootsBehavior rbh) {
                            rbh.OnRootsUpdate(Api.World, Pos, ref state, temp, ref handling);
                            if (handling == EnumHandling.PreventSubsequent) break;
                        }
                    }

                    if (handling != EnumHandling.PreventSubsequent && handling != EnumHandling.PreventDefault) {
                        if (!state.CrownAlive) {
                            // Only die from cold damage if the crop crown isn't alive
                            if (state.DieBelowTemp > temp) {
                                // TODO: Handle root death
                                markDirty = true;
                                break;
                            }

                            // Only die from heat damage if the crop crown isn't alive
                            if (state.DieAboveTemp < temp) {
                                // TODO: Handle root death
                                markDirty = true;
                                break;
                            }
                        }
                        

                        // Timing updates
                        if (!state.CrownAlive) {
                            // Update the number of hours the crown has been dead
                            state.crownDeadHours += stateUpdateIntervalDays * Api.World.Calendar.HoursPerDay;
                        }
                    }
                }

                foreach (string stateForRemoval in toRemove) {
                    rootStates.Remove(stateForRemoval);
                }

                lastRootTickTotalDays += stateUpdateIntervalDays;
            }

            return rootStates;
        }

        protected bool TryRegrowFromRoots(RootState state) {
            // TODO: This method!

            return false;
        }

        protected bool TryDecomposeRoot(RootState state) {
            // TODO: This method!
            
            return false;
        }

        public override void FromTreeAttributes(ITreeAttribute tree, IWorldAccessor worldAccessForResolve) {
            base.FromTreeAttributes(tree, worldAccessForResolve);

            ITreeAttribute rootStatesTree = tree.GetOrAddTreeAttribute("rootStates");
            foreach (string key in rootStatesTree.Select(x => x.Key)) {
                // TODO: RootState needs serious refactoring to make this work.
                ITreeAttribute rootStateAttribute = tree.GetTreeAttribute(key);
                RootState? state;
                if (rootStates.TryGetValue(key, out state)) {
                    state.FromTreeAttributes(rootStateAttribute, Api.World);
                } else {
                    rootStates.Add(key, RootState.CreateFromTreeAttributes(rootStateAttribute, Api.World));
                }
            }
        }

        public override void ToTreeAttributes(ITreeAttribute tree) {
            base.ToTreeAttributes(tree);
        }

        public override void GetBlockInfo(IPlayer forPlayer, StringBuilder dsc) {
            base.GetBlockInfo(forPlayer, dsc);

            var states = UpdateAndGetRootStates();
            dsc.AppendLine("Roots:");
            foreach (var state in states) {
                dsc.AppendLine(state.ToString());
            }

            // Get the block info that the crop wants to append!
            if (Api.World.BlockAccessor.GetBlockEntity(farmland.UpPos) is BlockEntity cropBlockEntity) {
                if (cropBlockEntity is IFarmlandInfo infoEntity) infoEntity.GetFarmlandInfo(forPlayer, dsc);

                foreach (BlockEntityBehavior beh in cropBlockEntity.Behaviors) {
                    if (beh is IFarmlandInfo infoBehavior) infoBehavior.GetFarmlandInfo(forPlayer, dsc);
                }
            }

            dsc.ToString();
        }

    }
}
