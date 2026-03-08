using System;
using System.Collections.Generic;
using System.Text;
using Vintagestory.API.Common;
using Vintagestory.GameContent;

namespace AgronomyLib {
    public class TestBlockCrop : BlockCrop, ICropPropsProviderBlock {
        public BlockCropProperties GetProvidedCropProps() {
            BlockCropProperties props = CropProps;
            props.RequiredNutrient = EnumSoilNutrient.P;
            return props;
        }
    }
}
