using System;
using System.Collections.Generic;
using System.Text;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.GameContent;

namespace AgronomyLib.src.BlockEntity {
    public abstract class BlockEntityCropContainerBase : BlockEntityContainer {
        // TODO: Support multiple crops of the same type growing in a container, to support nurseries and stuff

        protected InventoryGeneric inv;
        public override InventoryBase Inventory => inv;
        public override string InventoryClassName => "agronomylib:cropcontainer";

        protected bool cropAlive;

        public virtual BlockCropContainerBase? ownBlock => Block as BlockCropContainerBase;
        public virtual ItemSlot CropSlot => inv[0];
        public virtual Block? GetContainedCrop() => CropSlot.Itemstack?.Block != null && CropSlot.Itemstack?.Block.GetCropProps() != null ? CropSlot.Itemstack?.Block : null;

        public virtual bool HasCrop => GetContainedCrop() != null;
        public virtual bool HasLivingCrop => HasCrop && cropAlive;

        public virtual BlockCropProperties? CropProps() => GetContainedCrop()?.GetCropProps();
        public virtual int CurrentStage() => GetContainedCrop()?.GetCurrentCropStage(Api.World, Pos) ?? 0;

        public BlockEntityCropContainerBase() {
            inv = new InventoryGeneric(1, "agronomylib:cropcontainer-0", null, null);
        }

        public override void Initialize(ICoreAPI api) {
            base.Initialize(api);
        }

        protected override void OnTick(float dt) {
            // Don't tick inventory contents
            // TODO: This might be necessary for uprooted crops?
        }

        public override void OnBlockBroken(IPlayer byPlayer = null) {
            // base.OnBlockBroken(byPlayer); - We don't want to simply drop inventory contents, we want to treat it like the plant broke, or has been uprooted
            // TODO: Crop breaking logic
        }

        public virtual bool TryGrowCrop(BlockEntityFarmland farmland, double currentTotalHours) {
            if (!cropAlive) return false;

            Block? crop = GetContainedCrop();
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
                        if (handling == EnumHandling.PreventSubsequent) {
                            MarkDirty(true);
                            return result;
                        }
                    }

                    if (handling == EnumHandling.PreventDefault) {
                        MarkDirty(true);
                        return result;
                    }
                }

                CropSlot.Itemstack = new ItemStack(nextBlock, 1);

                // There's one fewer update than growth stages, since we start on the first one already
                farmland.ConsumeNutrients(cropProps.RequiredNutrient, cropProps.NutrientConsumption / Math.Max(1, cropProps.GrowthStages - 1));
                MarkDirty(true);
                return true;
            }

            return false;            
        }

        public virtual bool DoKillCrop(EnumCropStressType deathReason, BlockEntityFarmland farmland = null) {
            if (!cropAlive) return false;

            cropAlive = false;
            if (farmland != null) {
                // Since the block isn't actually being broken, we need to manually call OnCropBlockBroken()
                farmland.OnCropBlockBroken();
            }

            MarkDirty(true);

            return true;
        }
    }
}
