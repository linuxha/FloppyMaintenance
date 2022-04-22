using System;

namespace ActiproSoftware.SyntaxEditor {

	/// <summary>
	/// Specifies a find/replace search type.
	/// </summary>
	public enum FindReplaceSearchType {

		/// <summary>
		/// Search text normally.
		/// </summary>
		Normal,

		/// <summary>
		/// Search text using regular expressions.
		/// </summary>
		RegularExpression,

		/// <summary>
		/// Search text using wildcards.
		/// </summary>
		Wildcard
	
	}
}
