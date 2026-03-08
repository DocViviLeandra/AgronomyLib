using System;
using System.Collections.Generic;
using System.Text;
using Vintagestory.API.Common;

namespace AgronomyLib {
    public static class BlockExtensions {
        public static BlockCropProperties GetCropProps(this Block block) {
            if (block is ICropPropsProviderBlock cropPropsProvider) {
                return cropPropsProvider.GetProvidedCropProps();
            } else {
                return block.CropProps;
            }
        }
    }
}
