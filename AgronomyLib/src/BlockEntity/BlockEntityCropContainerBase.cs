using System;
using System.Collections.Generic;
using System.Text;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.GameContent;

namespace AgronomyLib.src.BlockEntity {
    public class BlockEntityCropContainerBase : BlockEntityContainer {
        protected InventoryGeneric inv;
        public override InventoryBase Inventory => inv;
        public override string InventoryClassName => "agronomylib:cropcontainer";

        public ItemSlot cropSlot => inv[0];
        public Block? containedCrop => inv[0].Itemstack?.Block;

        public virtual BlockCropProperties? CropProps() => containedCrop?.GetCropProps();
        public virtual int CurrentStage() => containedCrop?.GetCurrentCropStage(Api.World, Pos) ?? 0;

        public BlockEntityCropContainerBase() {
            inv = new InventoryGeneric(1, "agronomylib:cropcontainer-0", null, null);
        }

        public override void Initialize(ICoreAPI api) {
            base.Initialize(api);
        }

        protected override void OnTick(float dt) {
            // Don't tick inventory contents
        }

        public override void OnBlockBroken(IPlayer byPlayer = null) {
            // base.OnBlockBroken(byPlayer); - We don't want to simply drop inventory contents, we want to treat it like the plant broke, or has been uprooted
            // TODO: Crop breaking logic
        }

        public virtual bool TryGrowCrop(BlockEntityFarmland farmland, double currentTotalHours) {
            Block? crop = containedCrop;
            if (crop == null) return false;

            int currentGrowthStage = crop.GetCurrentCropStage();
            BlockCropProperties cropProps = crop.GetCropProps();
            if (currentGrowthStage < cropProps.GrowthStages) {
                int newGrowthStage = currentGrowthStage + 1;

                Block? nextBlock = Api.World.GetBlock(crop.CodeWithParts($"{newGrowthStage}"));
                if (nextBlock == null) return false;

                if (cropProps.Behaviors != null) {
                    EnumHandling handling = EnumHandling.PassThrough;
                    bool result = false;
                    foreach (CropBehavior behavior in cropProps.Behaviors) {
                        result = behavior.TryGrowCrop(Api, farmland, currentTotalHours, newGrowthStage, ref handling);
                        if (handling == EnumHandling.PreventSubsequent) return result;
                    }

                    if (handling == EnumHandling.PreventDefault) return result;
                }

                cropSlot.Itemstack = new ItemStack(nextBlock, 1);

                // There's one fewer update than growth stages, since we start on the first one already
                farmland.ConsumeNutrients(cropProps.RequiredNutrient, cropProps.NutrientConsumption / Math.Max(1, cropProps.GrowthStages - 1));
                return true;
            }

            return false;            
        }
    }
}
