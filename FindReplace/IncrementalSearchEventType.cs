using System;

namespace ActiproSoftware.SyntaxEditor {

	/// <summary>
	/// Specifies the type of event that occurred for an <see cref="IncrementalSearch"/> event.
	/// </summary>
	public enum IncrementalSearchEventType {

		/// <summary>
		/// Incremental search was activated.
		/// </summary>
		Activated,

        /// <summary>
		/// Incremental search was activated.
        /// </summary>
		Deactivated,	

		/// <summary>
		/// A search was performed.
		/// </summary>
		Search,

		/// <summary>
		/// A character was typed and was attempted to be added to incremental search however was disallowed since the previous 
		/// find text was not found in the document.
		/// </summary>
		CharacterIgnored
	}
}
