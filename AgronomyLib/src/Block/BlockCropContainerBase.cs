using AgronomyLib.src.BlockEntity;
using System;
using System.Collections.Generic;
using System.Text;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Config;
using Vintagestory.API.MathTools;
using Vintagestory.API.Util;
using Vintagestory.GameContent;

namespace AgronomyLib {
    /// <summary>
    /// A block which, along with its associated block entity, contains a crop to which farmland updates are applied instead of the block. Allows a variety of trickery, such as beanpoles containing bean plants.
    /// </summary>
    public abstract class BlockCropContainerBase : BlockContainer, IBlockProvidesCropProps, IBlockCropGrowth, IBlockCropDeath {
        // TODO: Support multiple crops of the same type growing in a container, to support nurseries and stuff

        public virtual string meshRefsCacheKey => Code.ToShortString() + "meshRefs";

        public virtual int ContainerSlotId => 0;

        public virtual int GetContainerSlotId(BlockPos pos) {
            return ContainerSlotId;
        }

        public virtual int GetContainerSlotId(ItemStack containerStack) {
            return ContainerSlotId;
        }

        public override void OnLoaded(ICoreAPI api) {
            base.OnLoaded(api);

            // TODO: load props?

            if (api.Side != EnumAppSide.Client) return;
            ICoreClientAPI capi = api as ICoreClientAPI;

            interactions = ObjectCacheUtil.GetOrCreate(api, "agronomylib:cropContainerBase", () => {
                List<ItemStack> cropContainerStacks = new List<ItemStack>();

                
                foreach(CollectibleObject obj in api.World.Collectibles) {
                    if (obj is BlockCropContainerBase blc) { // TODO: Additional checks?
                        cropContainerStacks.Add(new ItemStack(obj));
                    }
                }

                var ccStacks = cropContainerStacks.ToArray();

                return new WorldInteraction[] {
                    // TODO: standard world interactions
                };
            });
        }

        public override WorldInteraction[] GetPlacedBlockInteractionHelp(IWorldAccessor world, BlockSelection selection, IPlayer forPlayer) {
            return base.GetPlacedBlockInteractionHelp(world, selection, forPlayer).Append(interactions);
        }

        public override WorldInteraction[] GetHeldInteractionHelp(ItemSlot inSlot) {
            // TODO: Base held interaction
            return base.GetHeldInteractionHelp(inSlot);
        }

        public WorldInteraction[] interactions { get; protected set; }



        public ItemStack? GetCropStack(ItemStack containerStack) {
            ItemStack[] stacks = GetContents(api.World, containerStack);
            int id = GetContainerSlotId(containerStack);
            return (stacks != null && stacks.Length > 0) ? stacks[Math.Min(stacks.Length - 1, id)] : null;
        }

        public virtual BlockCropProperties CropProperties() => null; // This isn't a crop if there isn't a crop in it!

        public virtual BlockCropProperties CropProperties(IWorldAccessor world, BlockPos pos) {
            BlockEntityCropContainerBase? be = world.BlockAccessor.GetBlockEntity(pos) as BlockEntityCropContainerBase;
            if (be == null) {
                world.Logger.Error("BlockCropContainerBase at position {0} cannot find associated BlockEntityCropContainerBase!", pos);
                return null;
            }

            return be.CropProps();
        }

        public virtual int CropStage() => 0; // This isn't a crop if there isn't a crop in it!

        public virtual int CropStage(IWorldAccessor world, BlockPos pos) {
            BlockEntityCropContainerBase? be = world.BlockAccessor.GetBlockEntity(pos) as BlockEntityCropContainerBase;
            if (be == null) {
                world.Logger.Error("BlockCropContainerBase at position {0} cannot find associated BlockEntityCropContainerBase!", pos);
                return 0;
            }

            return be.CurrentStage();
        }

        public bool TryGrowCrop(BlockEntityFarmland farmland, double currentTotalHours) {
            BlockEntityCropContainerBase? be = farmland.Api.World.BlockAccessor.GetBlockEntity(farmland.UpPos) as BlockEntityCropContainerBase;
            if (be == null) {
                farmland.Api.Logger.Error("BlockCropContainerBase at position {0} cannot find associated BlockEntityCropContainerBase!", farmland.UpPos);
                return false;
            }

            return be.TryGrowCrop(farmland, currentTotalHours);
        }

        public virtual bool TryKillCrop(IWorldAccessor world, BlockPos pos, EnumCropStressType deathReason, BlockEntityFarmland? farmland = null) {
            DoKillCrop(world, pos, deathReason, farmland);
            return true;
        }

        public virtual bool DoKillCrop(IWorldAccessor world, BlockPos pos, EnumCropStressType deathReason, BlockEntityFarmland? farmland = null) {
            BlockEntityCropContainerBase? be = farmland.Api.World.BlockAccessor.GetBlockEntity(farmland.UpPos) as BlockEntityCropContainerBase;
            if (be == null) {
                farmland.Api.Logger.Error("BlockCropContainerBase at position {0} cannot find associated BlockEntityCropContainerBase!", farmland.UpPos);
                return false;
            }

            return be.DoKillCrop(deathReason, farmland);
        }

        public virtual bool OnCropDeath(IWorldAccessor world, BlockPos pos, EnumCropStressType deathReason, BlockEntityFarmland? farmland) {
            return true;
        }
    }
}
