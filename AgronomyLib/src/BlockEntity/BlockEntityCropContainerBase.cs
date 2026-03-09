using System;
using System.Collections.Generic;
using System.Text;
using Vintagestory.API.Common;
using Vintagestory.GameContent;

namespace AgronomyLib.src.BlockEntity {
    public class BlockEntityCropContainerBase : BlockEntityContainer {
        protected InventoryGeneric inv;
        public override InventoryBase Inventory => inv;

        public override string InventoryClassName => "agronomylib:cropcontainer";

        public ItemSlot cropSlot => inv[0];

        public virtual BlockCropProperties CropProps() => cropSlot.Itemstack.Block.GetCropProps(Api, Pos);
        public virtual int CurrentStage() => cropSlot.Itemstack.Block.GetCurrentCropStage(Api.World, Pos);

        public BlockEntityCropContainerBase() {
            inv = new InventoryGeneric(1, InventoryClassName, null, null);
        }
    }
}
