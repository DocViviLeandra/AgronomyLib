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

        public override void OnBlockRemoved() => base.OnBlockRemoved();

        public static double stateUpdateIntervalDays = 1 / 3.0;

        public IFarmlandBlockEntity farmland => Blockentity as IFarmlandBlockEntity;

        public BlockEntityBehaviorAgronomyFarmland(BlockEntity be) : base(be) { }

        public bool OnCropPlaced(Block block) {
            // OBSOLETE

            return true;
        }

        public bool OnCropRemoved(Block block) {
            // OBSOLETE

            return true;
        }

        public override void FromTreeAttributes(ITreeAttribute tree, IWorldAccessor worldAccessForResolve) {
            base.FromTreeAttributes(tree, worldAccessForResolve);
        }

        public override void ToTreeAttributes(ITreeAttribute tree) {
            base.ToTreeAttributes(tree);
        }

        public override void GetBlockInfo(IPlayer forPlayer, StringBuilder dsc) {
            base.GetBlockInfo(forPlayer, dsc);

            // Get the block info that the crop wants to append!
            if (Api.World.BlockAccessor.GetBlockEntity(farmland.UpPos) is BlockEntity cropBlockEntity) {
                if (cropBlockEntity is IProvidesFarmlandInfo infoEntity) infoEntity.GetFarmlandInfo(forPlayer, dsc);

                foreach (BlockEntityBehavior beh in cropBlockEntity.Behaviors) {
                    if (beh is IProvidesFarmlandInfo infoBehavior) infoBehavior.GetFarmlandInfo(forPlayer, dsc);
                }
            }

            dsc.ToString();

            
        }

    }
}
