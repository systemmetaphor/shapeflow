namespace ShapeFlow.Shapes
{
    /// <summary>
    /// Identifies what to expect when fetching the shape instance.
    /// </summary>
    public enum ShapeFormat
    {
        /// <summary>
        /// When a shape is not defined.
        /// </summary>
        None,

        /// <summary>
        /// Expect to find a JObject when fetching the shape instance.
        /// </summary>
        Json,

        /// <summary>
        /// Expect to find a XDocument when fetching the shape instance.
        /// </summary>
        Xml,

        /// <summary>
        /// Expect to find a YamlNode when fetching the shape instance.
        /// </summary>
        Yaml,

        /// <summary>
        /// Expect to find an object (representing an arbitrary object shape) when fetching the shape instance.
        /// </summary>
        Clr,

        /// <summary>
        /// Expect to find a collection of files with path and text.
        /// </summary>
        FileSet
    }
}
