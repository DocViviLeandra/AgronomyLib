using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vintagestory.API.Common;
using Vintagestory.API.MathTools;
using Vintagestory.GameContent;

namespace AgronomyLib {

    /// <summary>
    /// An interface for a <see cref="BlockBehavior"/> to affect the <see cref="BlockEntityFarmland.TryGrowCrop(double)"/> method.
    /// </summary>
    public interface ICropGrowthBehavior {
        /// <summary>
        /// Executes when TryGrowCrop() is called on a crop, before any growth logic is executed.
        /// </summary>
        /// <param name="world"></param>
        /// <param name="pos">The position of the crop</param>
        /// <param name="currentTotalHours"></param>
        /// <param name="handling"></param>
        /// <returns>false if the crop should not attempt to grow.</returns>
        public abstract bool BeforeTryGrowCrop(IWorldAccessor world, BlockPos pos, double currentTotalHours, ref EnumHandling handling);

        /// <summary>
        /// Invoked when TryGrowCrop() is called on a crop, after all growth logic is executed, regardless of the outcome.
        /// </summary>
        /// <param name="world"></param>
        /// <param name="pos">The position of the crop</param>
        /// <param name="currentTotalHours"></param>
        /// <param name="wasGrown">Whether or not the crop successfully grew</param>
        /// <param name="handling"></param>
        public abstract void OnGrowth(IWorldAccessor world, BlockPos pos, double currentTotalHours, bool wasGrown, ref EnumHandling handling);
    }

    /// <summary>
    /// An interface for a <see cref="BlockEntityBehavior"/> to affect the <see cref="BlockEntityFarmland.TryGrowCrop(double)"/> method 
    /// </summary>
    public interface ICropGrowthBEBehavior {

        /// <summary>
        /// Invoked prior to the execution of the crop's TryGrowCrop() method.
        /// </summary>
        /// <param name="currentTotalHours"></param>
        /// <param name="handling"></param>
        /// <returns></returns>
        public virtual bool BeforeTryGrowCrop(double currentTotalHours, ref EnumHandling handling) {
            return true;
        }

        /// <summary>
        /// Invoked after a crop grows, regardless of whether the growth was successful or not.
        /// </summary>
        /// <param name="currentTotalHours"></param>
        /// <param name="handling"></param>
        /// <param name="wasGrown">Whether the crop's growth was successful or not</param>
        public virtual void OnGrowth(double currentTotalHours, bool wasGrown, ref EnumHandling handling) {
            return;
        }
    }
}
