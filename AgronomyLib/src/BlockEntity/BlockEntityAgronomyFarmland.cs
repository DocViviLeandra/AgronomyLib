using System;
using System.Collections.Generic;
using System.Text;
using Vintagestory.API.Common;
using Vintagestory.API.Datastructures;
using Vintagestory.API.Server;
using Vintagestory.GameContent;


namespace AgronomyLib {
    public class BlockEntityAgronomyFarmland : BlockEntityFarmland {

        public override void Initialize(ICoreAPI api) {
            base.Initialize(api);

            deadCropBlock = Api.World.GetBlock(new AssetLocation("deadcrop")); // TODO: More robust handling for this!
        }

        /// <remarks>
        /// Unchanged from vanilla aside from using local version of updateCropDamage()
        /// </remarks>
        protected override void beginIntervalledUpdate(out FarmlandFastForwardUpdate onInterval, out FarmlandUpdateEnd onEnd) {
            Block cropBlock = GetCrop();
            bool hasCrop = cropBlock != null;
            bool hasRipeCrop = HasRipeCrop();
            double hoursNextStage = GetHoursForNextStage();

            base.beginIntervalledUpdate(out onInterval, out onEnd);

            var prevOnInterval = onInterval;

            onInterval = (hourInterval1, conds, lightGrowthSpeedFactor, growthPaused) => {
                prevOnInterval.Invoke(hourInterval1, conds, lightGrowthSpeedFactor, growthPaused);

                // Adjust for light level, ie 10% growth speed needs 90% of hourIntervall added back on to total growth time
                totalHoursForNextStage += hourInterval1 * (1 - lightGrowthSpeedFactor);

                hasCrop = updateCropDamage(hourInterval1, cropBlock, hasCrop, hasRipeCrop, conds);

                if (growthPaused) {
                    totalHoursForNextStage += previousHourInterval; // Postpone crop growth for the same amount of time that has been suspended
                    return;
                }

                if (moistureLevel < 0.1) {
                    // Too dry to grow. Todo: Make it dependent on crop
                    return;
                }

                if (totalHoursLastUpdate >= totalHoursForNextStage) {
                    TryGrowCrop(totalHoursForNextStage);
                    hasRipeCrop = HasRipeCrop();

                    hoursNextStage = GetHoursForNextStage();
                }
            };
        }

        protected override bool RecoverFertility => GetCrop() == null || HasRipeCrop(); // TODO: Allow fertility recovery for certain crops when ripe?

        private bool updateCropDamage(double hourIntervall, Block cropBlock, bool hasCrop, bool hasRipeCrop, ClimateCondition conds) {
            if (!hasCrop) {
                ripeCropColdDamaged = false;
                unripeCropColdDamaged = false;
                unripeHeatDamaged = false;
                for (int i = 0; i < damageAccum.Length; i++) damageAccum[i] = 0;
            } else {
                BlockCropProperties cProps = cropBlock.GetCropProps();
                if (cropBlock?.CropProps != null && conds.Temperature < cProps.ColdDamageBelow) {
                    if (hasRipeCrop) {
                        ripeCropColdDamaged = true;
                    } else {
                        unripeCropColdDamaged = true;
                        damageAccum[(int)EnumCropStressType.TooCold] += (float)hourIntervall;
                    }
                } else {
                    damageAccum[(int)EnumCropStressType.TooCold] = Math.Max(0, damageAccum[(int)EnumCropStressType.TooCold] - (float)hourIntervall / 10);
                }

                if (cProps != null && conds.Temperature > cProps.HeatDamageAbove && hasCrop) {
                    unripeHeatDamaged = true;
                    damageAccum[(int)EnumCropStressType.TooHot] += (float)hourIntervall;
                } else {
                    damageAccum[(int)EnumCropStressType.TooHot] = Math.Max(0, damageAccum[(int)EnumCropStressType.TooHot] - (float)hourIntervall / 10);
                }

                for (int i = 0; i < damageAccum.Length; i++) {
                    float dmg = damageAccum[i];
                    if (!allowcropDeath) dmg = damageAccum[i] = 0;

                    if (dmg > 48) { // TODO: Make damage threshold configurable
                        // TODO: More versatile dead handling
                        Api.World.BlockAccessor.SetBlock(deadCropBlock.Id, upPos); 
                        var be = Api.World.BlockAccessor.GetBlockEntity(upPos) as BlockEntityDeadCrop;
                        be.Inventory[0].Itemstack = new ItemStack(cropBlock);
                        be.deathReason = (EnumCropStressType)i;
                        hasCrop = false;
                        break;
                    }
                }
            }

            return hasCrop;
        }

        public double GetHoursForNextStage() {
            Block block = GetCrop();
            if (block == null) return int.MaxValue;

            BlockCropProperties cProps = block.GetCropProps();
            var totalDays = cProps.TotalGrowthDays;
            // Backwards compatibility, if days are provided we convert it to months using the default configuration timescale
            // After, we convert it to the currently configured timescale
            // For example, if something is set to grow in 6 days and the amount of days per month has been changed to 30, the new growth time will be 15 days.
            if (totalDays > 0) {
                var defaultTimeInMonths = totalDays / 12;
                totalDays = defaultTimeInMonths * Api.World.Calendar.DaysPerMonth;
            } else {
                totalDays = cProps.TotalGrowthMonths * Api.World.Calendar.DaysPerMonth;
            }

            // There's one fewer update than growth stages, since we start on the first one already
            float stageHours = Api.World.Calendar.HoursPerDay * totalDays / Math.Max(1, block.CropProps.GrowthStages - 1);

            stageHours *= 1 / GetGrowthRate(cProps.RequiredNutrient); // TODO: Allow crops to modify?

            // Add a bit random to it (+/- 10%)
            stageHours *= (float)(0.9 + 0.2 * rand.NextDouble());

            return stageHours / growthRateMul;
        }

        /// <remarks>
        /// Unchanged from vanilla
        /// </remarks>
        public bool HasRipeCrop() {
            Block block = GetCrop();
            return block != null && GetCropStage(block) >= block.CropProps.GrowthStages;
        }


        public bool TryGrowCrop(double currentTotalHours) {
            Block block = GetCrop();
            if (block == null) return false;

            int currentGrowthStage = GetCropStage(block);

            BlockCropProperties cProps = block.GetCropProps();
            if (currentGrowthStage < cProps.GrowthStages) {
                int newGrowthStage = currentGrowthStage + 1; // TODO: Directly ask the crop what its next stage should be?

                Block? nextBlock = Api.World.GetBlock(block.CodeWithParts($"{newGrowthStage}"));
                if (nextBlock == null) return false;

                if (cProps.Behaviors != null) {
                    EnumHandling handled = EnumHandling.PassThrough;
                    bool result = false;
                    foreach (CropBehavior behavior in cProps.Behaviors) {
                        result = behavior.TryGrowCrop(Api, this, currentTotalHours, newGrowthStage, ref handled);
                        if (handled == EnumHandling.PreventSubsequent) return result;
                    }

                    if (handled == EnumHandling.PreventDefault) return result;
                }

                if (Api.World.BlockAccessor.GetBlockEntity(upPos) == null) {
                    Api.World.BlockAccessor.SetBlock(nextBlock.BlockId, upPos); //create any blockEntity if necessary (e.g. Bell Pepper and other fruiting crops)
                } else {
                    Api.World.BlockAccessor.ExchangeBlock(nextBlock.BlockId, upPos);    //do not destroy existing blockEntity (e.g. Bell Pepper and other fruiting crops)
                }

                ConsumeNutrients(block);
                return true;
            }

            return false;
        }
    }
}
