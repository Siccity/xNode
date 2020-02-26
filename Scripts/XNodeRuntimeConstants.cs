namespace XNode
{
    /// <summary>
    /// A helper class containing shared constants.
    /// </summary>
	public static class XNodeRuntimeConstants
	{
        // Exceptions
		public const string MISMATCHED_KEYS_TO_VALUES_EXCEPTION_MESSAGE =
			"There are {0} keys and {1} values after deserialization. " +
			"Make sure that both key and value types are serializable.";

        // Reflection

        /// <summary>
        /// A collection of assembly prefixes that should not be reflected for derived node types.
        /// </summary>
        public static string[] IGNORE_ASSEMBLY_PREFIXES =
        {
            "ExCSS",
            "Microsoft",
            "Mono",
            "netstandard",
            "mscorlib",
            "nunit",
            "SyntaxTree",
            "System",
            "Unity",
            "UnityEditor",
            "UnityEngine"
        };
	}
}
