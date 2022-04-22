using System;

namespace ActiproSoftware.SyntaxEditor {

	/// <summary>
	/// Represents the method that will handle <see cref="IncrementalSearch"/> events.
	/// </summary>
	/// <param name="sender">Sender of the event.</param>
	/// <param name="e">A <see cref="IncrementalSearchEventArgs"/> containing event data.</param>
	/// <remarks>
	/// When you create a <c>IncrementalSearchEventHandler</c> delegate, you identify the method that will handle the event. 
	/// To associate the event with your event handler, add an instance of the delegate to the event. 
	/// The event handler is called whenever the event occurs, unless you remove the delegate.
	/// </remarks>
	/// <seealso cref="IncrementalSearchEventArgs" />
	public delegate void IncrementalSearchEventHandler(object sender, IncrementalSearchEventArgs e);
}
