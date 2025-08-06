namespace PolygonTriangulation
{
    using System.Collections.Generic;

#if UNITY_EDITOR || UNITY_STANDALONE
    using Vertex = UnityEngine.Vector2;
#elif STRIDE_PLATFORM_WINDOWS || STRIDE_PLATFORM_WINDOWS_DESKTOP || STRIDE_PLATFORM_MONO_MOBILE || STRIDE_PLATFORM_ANDROID || STRIDE_PLATFORM_IOS
    using Vertex = Stride.Core.Mathematics.Vector2;
#else
    using Vertex = System.Numerics.Vector2;

#endif

    /// <summary>
    /// Extension methods for polygon
    /// </summary>
    public static class PolygonExtensions
    {
        /// <summary>
        /// Adds the vertices.
        /// </summary>
        /// <param name="builder">The builder.</param>
        /// <param name="vertices">The vertices.</param>
        /// <returns>the same builder</returns>
        public static IPolygonBuilder AddVertices(this IPolygonBuilder builder, params int[] vertices)
        {
            return builder.AddVertices((IEnumerable<int>)vertices);
        }
    }
}
