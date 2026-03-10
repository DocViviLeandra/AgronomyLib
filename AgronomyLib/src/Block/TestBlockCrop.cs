using System;
using System.Collections.Generic;
using System.Text;
using Vintagestory.API.Common;
using Vintagestory.API.Config;
using Vintagestory.API.MathTools;
using Vintagestory.GameContent;

namespace AgronomyLib {
    public class TestBlockCrop : BlockCrop, IBlockProvidesCropProps {

        public BlockCropProperties CropProperties(IWorldAccessor world, BlockPos pos) {
            BlockCropProperties defaultProps = world.BlockAccessor.GetBlock(pos).CropProps;
            return new BlockCropProperties() {
                RequiredNutrient = EnumSoilNutrient.P,
                NutrientConsumption = defaultProps.NutrientConsumption,
                GrowthStages = defaultProps.GrowthStages,
                TotalGrowthDays = defaultProps.TotalGrowthDays,
                TotalGrowthMonths = defaultProps.TotalGrowthMonths,
                MultipleHarvests = defaultProps.MultipleHarvests,
                HarvestGrowthStageLoss = defaultProps.HarvestGrowthStageLoss,
                ColdDamageBelow = defaultProps.ColdDamageBelow,
                DamageGrowthStuntMul = defaultProps.DamageGrowthStuntMul,
                ColdDamageRipeMul = defaultProps.ColdDamageRipeMul,
                HeatDamageAbove = defaultProps.HeatDamageAbove,
                Behaviors = defaultProps.Behaviors,
            };
        }

        public int CropStage() {
            return CurrentCropStage;
        }

        public int CropStage(IWorldAccessor world, BlockPos pos) {
            BlockCrop block = world.BlockAccessor.GetBlock(pos) as BlockCrop;
            return block.CurrentCropStage;
        }

        public BlockCropProperties CropProperties() {
            BlockCropProperties defaultProps = CropProps;
            return new BlockCropProperties() {
                RequiredNutrient = EnumSoilNutrient.P,
                NutrientConsumption = defaultProps.NutrientConsumption,
                GrowthStages = defaultProps.GrowthStages,
                TotalGrowthDays = defaultProps.TotalGrowthDays,
                TotalGrowthMonths = defaultProps.TotalGrowthMonths,
                MultipleHarvests = defaultProps.MultipleHarvests,
                HarvestGrowthStageLoss = defaultProps.HarvestGrowthStageLoss,
                ColdDamageBelow = defaultProps.ColdDamageBelow,
                DamageGrowthStuntMul = defaultProps.DamageGrowthStuntMul,
                ColdDamageRipeMul = defaultProps.ColdDamageRipeMul,
                HeatDamageAbove = defaultProps.HeatDamageAbove,
                Behaviors = defaultProps.Behaviors,
            };
        }
    }
}
