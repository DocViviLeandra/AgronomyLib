using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vintagestory.API;
using Vintagestory.API.Common;
using Vintagestory.API.Datastructures;
using Vintagestory.API.MathTools;

namespace AgronomyLib {

    /// <summary>
    /// Container for tracking properties of a plant related to vernalization
    /// </summary>
    public class VernalizationProperties {
        /// <summary>
        /// The stage at which a crop will stop growing and wait to become vernalized, if it is not yet vernalized.
        /// </summary>
        [DocumentAsJson("Required", "1")]
        public int MaxStageBeforeVernalization = 1;

        /// <summary>
        /// The earliest stage at which the crop is able to become vernalized. Vernalizing conditions prior to this stage will not contribute to vernalization.
        /// </summary>
        [DocumentAsJson("Optional", "0")]
        public int MinStageBeforeVernalization = 0;

        /// <summary>
        /// The number of hours the crop must be exposed to cold temperatures in order to become vernalized
        /// </summary>
        [DocumentAsJson("Recommended", "100")]
        public NatFloat VernalizationHours = NatFloat.createUniform(100, 10);

        /// <summary>
        /// The temperature below which the crop must be exposed to for an extended time in order to become vernalized
        /// </summary>
        [DocumentAsJson("Recommended", "4")]
        public NatFloat VernalizationTemp = NatFloat.createUniform(4, 1);
    }

    /// <summary>
    /// Container for tracking the state of vernalization for an individual crop.
    /// </summary>
    public class VernalizationState {
        #region keys
        internal static readonly string AttributeKey = $"{AgronomyLibModSystem.attributePrefix}.vernalizationState";
        #endregion

        #region fixed variables
        public int MaxStageBeforeVernalization;
        public int MinStageBeforeVernalization;
        public float VernalizationHours;
        public float VernalizationTemp;
        #endregion

        #region dynamic variables
        public double lastTickTotalDays;
        public double vernalizedHours;
        #endregion

        #region accessors
        public bool IsVernalized => vernalizedHours >= VernalizationHours;
        #endregion

        public void FromTreeAttributes(ITreeAttribute tree) {
            MaxStageBeforeVernalization = tree.GetInt("maxStageBeforeVernalization", 1);
            MinStageBeforeVernalization = tree.GetInt("minStageBeforeVernalization", 0);
            VernalizationHours = tree.GetFloat("vernalizationHours", 100);
            VernalizationTemp = tree.GetFloat("vernalizationTemp", 4);

            lastTickTotalDays = tree.GetDouble("lastTickTotalDays", 0);
            vernalizedHours = tree.GetDouble("vernalizedHours", 0);
        }

        public void ToTreeAttributes(ITreeAttribute tree) {
            tree.SetInt("maxStageBeforeVernalization", MaxStageBeforeVernalization);
            tree.SetInt("minStageBeforeVernalization", MinStageBeforeVernalization);
            tree.SetFloat("vernalizationHours", VernalizationHours);
            tree.SetFloat("vernalizationTemp", VernalizationTemp);

            tree.SetDouble("lastTickTotalDays", lastTickTotalDays);
            tree.SetDouble("vernalizedHours", vernalizedHours);
        }
    }
}
