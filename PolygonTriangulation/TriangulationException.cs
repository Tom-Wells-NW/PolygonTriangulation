namespace PolygonTriangulation
{
    using System;
    using System.Linq;
    using System.Runtime.Serialization;

    /// <summary>
    /// polygon data for exceptions during polygon triangulation
    /// </summary>
    [Serializable]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1032:Standardausnahmekonstruktoren implementieren", Justification = "Exception must have a polygon")]
    public class TriangulationException : InvalidOperationException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TriangulationException"/> class.
        /// </summary>
        /// <param name="polygon">The polygon.</param>
        /// <param name="edgeCreateCode">The edge create code.</param>
        /// <param name="innerException">The inner exception.</param>
        public TriangulationException(Polygon polygon, string edgeCreateCode, Exception innerException)
            : this(polygon, edgeCreateCode, innerException.Message, innerException)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TriangulationException"/> class.
        /// </summary>
        /// <param name="polygon">The polygon.</param>
        /// <param name="edgeCreateCode">The edge create code.</param>
        /// <param name="message">The message.</param>
        /// <param name="innerException">The inner exception.</param>
        public TriangulationException(Polygon polygon, string edgeCreateCode, string message, Exception innerException)
            : base(message, innerException)
        {
            this.PolygonCreateCode = BuildPolygonCode(polygon);
            this.EdgeCreateCode = edgeCreateCode;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TriangulationException"/> class. Used during deserialization.
        /// </summary>
        /// <param name="info">The object that holds the serialized object data.</param>
        /// <param name="context">The contextual information about the source or destination.</param>
        protected TriangulationException(SerializationInfo info, StreamingContext context)
#pragma warning disable SYSLIB0051 // Type or member is obsolete
            : base(info, context)
#pragma warning restore SYSLIB0051 // Type or member is obsolete
        {
            this.EdgeCreateCode = info.GetString(nameof(this.EdgeCreateCode));
            this.PolygonCreateCode = info.GetString(nameof(this.PolygonCreateCode));
        }

        /// <summary>
        /// Gets the code to feed the edges to a polygon builder.
        /// </summary>
        public string EdgeCreateCode { get; }

        /// <summary>
        /// Gets the code to create the polygon in a unittest
        /// </summary>
        public string PolygonCreateCode { get; }

        /// <inheritdoc/>
        [Obsolete("to be removed in .NET 9.0")]
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue(nameof(this.EdgeCreateCode), this.EdgeCreateCode);
            info.AddValue(nameof(this.PolygonCreateCode), this.PolygonCreateCode);
        }

        /// <summary>
        /// Create a polygon string that can be used to create the polygon
        /// </summary>
        /// <param name="polygon">The polygon to convert to code.</param>
        /// <returns>polygon as code</returns>
        internal static string BuildPolygonCode(Polygon polygon)
        {
            if (polygon == null)
            {
                return string.Empty;
            }

            var sb = new System.Text.StringBuilder();
            var culture = System.Globalization.CultureInfo.InvariantCulture;

            sb.AppendLine("var vertices = new[]");
            sb.AppendLine("{");
#if UNITY_EDITOR || UNITY_STANDALONE
            var vertexStrings = polygon.Vertices.Select(
                x => string.Format(culture, "    new Vertex({0:0.0000000}f, {1:0.0000000}f),", x.x, x.y));
#else
            var vertexStrings = polygon.Vertices.Select(
                x => string.Format(culture, "    new Vertex({0:0.0000000}f, {1:0.0000000}f),", x.X, x.Y));
#endif
            sb.AppendLine(string.Join(Environment.NewLine, vertexStrings));
            sb.AppendLine("};");
            sb.AppendLine(string.Empty);

            sb.AppendLine("var polygon = Polygon.Build(vertices)");
            foreach (var subPolygonId in polygon.SubPolygonIds)
            {
#pragma warning disable CA1305 // Specify IFormatProvider
                sb.AppendLine($"    .AddVertices({string.Join(", ", polygon.SubPolygonVertices(subPolygonId))})");
#pragma warning restore CA1305 // Specify IFormatProvider
                sb.AppendLine($"    .ClosePartialPolygon()");
            }

            var fusionVerticex = polygon.SubPolygonIds
                .SelectMany(x => polygon.SubPolygonVertices(x))
                .GroupBy(x => x)
                .Where(x => x.Count() > 1)
                .Select(x => x.Key);
#pragma warning disable CA1305 // Specify IFormatProvider
            sb.AppendLine($"    .Close({string.Join(", ", fusionVerticex)});");
#pragma warning restore CA1305 // Specify IFormatProvider

            return sb.ToString();
        }
    }
}
