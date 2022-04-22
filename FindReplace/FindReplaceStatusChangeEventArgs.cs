using System;

namespace ActiproSoftware.SyntaxEditor {

	/// <summary>
	/// Provides data for find/replace status change events.
	/// </summary>
	/// <remarks>
	/// This class is used with the <see cref="FindReplaceStatusChangeEventHandler" /> delegate.
	/// </remarks>
	/// <seealso cref="FindReplaceStatusChangeEventHandler" />
	public class FindReplaceStatusChangeEventArgs : EventArgs {

		private FindReplaceStatusChangeType	changeType;
		private FindReplaceOptions			options;

		/////////////////////////////////////////////////////////////////////////////////////////////////////
		// OBJECT
		/////////////////////////////////////////////////////////////////////////////////////////////////////

		/// <summary>
		/// Initializes a new instance of the <c>FindReplaceStatusChangeEventArgs</c> class.
		/// </summary>
		/// <param name="changeType">A <see cref="FindReplaceStatusChangeType"/> specifying the type of status change that occurred.</param>
		/// <param name="options">A <see cref="FindReplaceOptions"/> that contains the find/replace options in use.</param>
		public FindReplaceStatusChangeEventArgs(FindReplaceStatusChangeType changeType, FindReplaceOptions options) {
			// Initialize parameters
			this.changeType	= changeType;
			this.options	= options;
		}

		/////////////////////////////////////////////////////////////////////////////////////////////////////
		// PUBLIC PROCEDURES
		/////////////////////////////////////////////////////////////////////////////////////////////////////

		/// <summary>
		/// Gets the type of status change that occurred.
		/// </summary>
		/// <value>A <see cref="FindReplaceStatusChangeType"/> specifying the type of event that occurred.</value>
		public FindReplaceStatusChangeType ChangeType {
			get {
				return changeType;
			}
		}	

		/// <summary>
		/// Gets a <see cref="FindReplaceOptions"/> that contains the find/replace options in use.
		/// </summary>
		/// <value>A <see cref="FindReplaceOptions"/> that contains the find/replace options in use.</value>
		public FindReplaceOptions Options {
			get {
				return options;
			}
		}
	}
}
