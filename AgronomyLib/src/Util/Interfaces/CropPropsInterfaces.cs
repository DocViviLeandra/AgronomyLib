using System;
using System.Collections.Generic;
using System.Text;
using Vintagestory.API.Common;

namespace AgronomyLib {
    /// <summary>
    /// An interface which provides a method for providing <see cref="BlockCropProperties"/> through a method rather than by direct field access. Should only be added to a <see cref="Block"/>.
    /// </summary>
    public interface ICropPropsProviderBlock {
        public abstract BlockCropProperties GetProvidedCropProps();
    }
}
