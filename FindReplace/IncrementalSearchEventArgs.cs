using System;

namespace ActiproSoftware.SyntaxEditor {

	/// <summary>
	/// Event arguments for <see cref="IncrementalSearch"/> events.
	/// </summary>
	/// <remarks>
	/// This class is used with the <see cref="IncrementalSearchEventHandler" /> delegate.
	/// </remarks>
	/// <seealso cref="IncrementalSearchEventHandler" />
	public class IncrementalSearchEventArgs : EventArgs {

		private IncrementalSearchEventType		eventType;
		private FindReplaceResultSet			resultSet;

		/////////////////////////////////////////////////////////////////////////////////////////////////////
		// OBJECT
		/////////////////////////////////////////////////////////////////////////////////////////////////////

		/// <summary>
		/// Initializes a new instance of the <c>IncrementalSearchEventArgs</c> class.
		/// </summary>
		/// <param name="eventType">A <see cref="IncrementalSearchEventType"/> specifying the type of event that occurred.</param>
		/// <param name="resultSet">A <see cref="FindReplaceResultSet"/> that contains the result of the search operation.</param>
		public IncrementalSearchEventArgs(IncrementalSearchEventType eventType, FindReplaceResultSet resultSet) {
			// Initialize parameters
			this.eventType = eventType;
			this.resultSet = resultSet;
		}

		/////////////////////////////////////////////////////////////////////////////////////////////////////
		// PUBLIC PROCEDURES
		/////////////////////////////////////////////////////////////////////////////////////////////////////

		/// <summary>
		/// Gets the type of event that occurred.
		/// </summary>
		/// <value>A <see cref="IncrementalSearchEventType"/> specifying the type of event that occurred.</value>
		public IncrementalSearchEventType EventType {
			get {
				return eventType;
			}
		}	

		/// <summary>
		/// Gets the <see cref="FindReplaceResultSet"/> that contains the result of the search operation, if any.
		/// </summary>
		/// <value>A <see cref="FindReplaceResultSet"/> that contains the result of the search operation; otherwise, <see langword="null"/>.</value>
		public FindReplaceResultSet ResultSet {
			get {
				return resultSet;
			}
		}
	}
}
