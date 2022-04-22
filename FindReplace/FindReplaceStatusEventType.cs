using System;

namespace ActiproSoftware.SyntaxEditor {

	/// <summary>
	/// Specifies the type of event that occurred for a find/replace status event.
	/// </summary>
	public enum FindReplaceStatusChangeType {

		/// <summary>
		/// The find/replace operation has completed, generally by a find/replace form close.
		/// </summary>
		Ready,

		/// <summary>
		/// A find operation is in progress.
		/// </summary>
		Find,

		/// <summary>
		/// A replace operation is in progress.
		/// </summary>
		Replace,

		/// <summary>
		/// The find or replace operation went past the document end.
		/// </summary>
		PastDocumentEnd,

	}
}
