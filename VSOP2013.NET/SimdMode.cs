// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace VSOP2013
{
    /// <summary>
    /// Selects the vector width used by <see cref="Calculator"/> to evaluate the series.
    /// </summary>
    public enum SimdMode
    {
        /// <summary>Pick the widest hardware-accelerated width available.</summary>
        Auto = 0,

        /// <summary>Scalar evaluation only.</summary>
        Scalar = 1,

        /// <summary>128-bit vectors (2 doubles).</summary>
        Vector128 = 2,

        /// <summary>256-bit vectors (4 doubles).</summary>
        Vector256 = 3,

        /// <summary>512-bit vectors (8 doubles).</summary>
        Vector512 = 4,
    }
}
