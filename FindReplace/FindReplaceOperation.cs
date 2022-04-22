using System;

namespace ActiproSoftware.SyntaxEditor {

	/// <summary>
	/// Specifies the type of find and replace operation.
	/// </summary>
	public enum FindReplaceOperation {

		/// <summary>
		/// There is no operation specified.
		/// </summary>
		None,

		/// <summary>
		/// Finds the next occurrence of the find text.
		/// </summary>
		Find,

		/// <summary>
		/// Replaces the next occurrence of the find text with the replace text.
		/// </summary>
		Replace,

		/// <summary>
		/// Replaces all occurrences of the find text with the replace text.
		/// </summary>
		ReplaceAll,

		/// <summary>
		/// Marks all the occurrences of the find text with a line indicator.
		/// </summary>
		MarkAll
	}
}
