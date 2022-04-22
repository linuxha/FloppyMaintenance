using System;

namespace ActiproSoftware.SyntaxEditor {

	/// <summary>
	/// Represents the method that will handle find/replace status change events.
	/// </summary>
	/// <param name="sender">Sender of the event.</param>
	/// <param name="e">A <see cref="FindReplaceStatusChangeEventArgs"/> containing event data.</param>
	/// <remarks>
	/// When you create a <c>FindReplaceStatusChangeEventHandler</c> delegate, you identify the method that will handle the event. 
	/// To associate the event with your event handler, add an instance of the delegate to the event. 
	/// The event handler is called whenever the event occurs, unless you remove the delegate.
	/// </remarks>
	/// <seealso cref="FindReplaceStatusChangeEventArgs" />
	public delegate void FindReplaceStatusChangeEventHandler(object sender, FindReplaceStatusChangeEventArgs e);
}
