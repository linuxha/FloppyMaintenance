using System;
using System.Windows.Forms;

namespace ActiproSoftware.SyntaxEditor {

	/// <summary>
	/// Provides functionality for performing incremental searches.
	/// </summary>
	public class IncrementalSearch : ISyntaxEditorEditModeHandler {

		private bool					active;
		private EditorViewFindReplace	findReplace;
		private string					findText				= String.Empty;
		private bool					ignoreDeactivate		= false;
		private bool					ignoreFurtherFindText	= false;
		private bool					matchCase;
		private bool					searchUp;

		/////////////////////////////////////////////////////////////////////////////////////////////////////
		// OBJECT
		/////////////////////////////////////////////////////////////////////////////////////////////////////

		/// <summary>
		/// Initializes a new instance of the <c>IncrementalSearch</c> class.
		/// </summary>
		/// <param name="findReplace">The <see cref="EditorViewFindReplace"/> that owns the object.</param>
		internal IncrementalSearch(EditorViewFindReplace findReplace) {
			// Initialize parameters
			this.findReplace = findReplace;
		}

		/////////////////////////////////////////////////////////////////////////////////////////////////////
		// INTERFACE IMPLEMENTATION
		/////////////////////////////////////////////////////////////////////////////////////////////////////
		
		/// <summary>
		/// Raises the <c>EditorViewMouseHover</c> event.
		/// </summary>
		/// <param name="e">An <c>EditorViewMouseEventArgs</c> that contains the event data.</param>
		/// <returns>
		/// <c>true</c> if the event was processed by the control; otherwise, <c>false</c>.
		/// </returns>
		bool ISyntaxEditorEditModeHandler.OnEditorViewMouseHover(EditorViewMouseEventArgs e) {
			return false;
		}

		/// <summary>
		/// Raises the <c>KeyTyping</c> event.
		/// </summary>
		/// <param name="e">A <c>KeyTypingEventArgs</c> that contains the event data.</param>
		void ISyntaxEditorEditModeHandler.OnKeyTyping(KeyTypingEventArgs e) {
			// Hide if visible
			if (active) {
				switch (e.KeyData) {
					case Keys.Escape:
						// Stop incremental search
						this.Active = false;
						e.Cancel = true;
						return;
					case Keys.Back:
						// Remove a character
						this.RemoveCharacterFromFindText();
						this.PerformSearch(true, false);
						e.Cancel = true;
						return;
				}

				if (e.KeyChar != '\0') {
					// Add the character to the incremental search
					findReplace.IncrementalSearch.AddCharacterToFindText(e.KeyChar);
					findReplace.IncrementalSearch.PerformSearch(true, false);
					e.Cancel = true;
				}
			}

		}

		/// <summary>
		/// Raises the <c>LostFocus</c> event.
		/// </summary>
		/// <param name="e">An <c>EventArgs</c> that contains the event data.</param>
		void ISyntaxEditorEditModeHandler.OnLostFocus(EventArgs e) {
			// Stop if active
			if (active)
				this.Active = false;
		}

		/// <summary>
		/// Raises the <c>MouseDown</c> event.
		/// </summary>
		/// <param name="e">A <c>MouseEventArgs</c> that contains the event data.</param>
		/// <returns>
		/// <c>true</c> if the event was processed by the control; otherwise, <c>false</c>.
		/// </returns>
		bool ISyntaxEditorEditModeHandler.OnMouseDown(MouseEventArgs e) {
			return false;
		}

		/// <summary>
		/// Raises the <c>MouseWheel</c> event.
		/// </summary>
		/// <param name="e">A <c>MouseEventArgs</c> that contains the event data.</param>
		/// <returns>
		/// <c>true</c> if the event was processed by the control; otherwise, <c>false</c>.
		/// </returns>
		bool ISyntaxEditorEditModeHandler.OnMouseWheel(MouseEventArgs e) {
			return false;
		}

		/// <summary>
		/// Raises the <c>SelectedViewChanged</c> event.
		/// </summary>
		/// <param name="e">An <c>EventArgs</c> that contains the event data.</param>
		void ISyntaxEditorEditModeHandler.OnSelectedViewChanged(EventArgs e) {
			// Stop if active
			if (active)
				this.Active = false;
		}

		/// <summary>
		/// Raises the <c>SelectionChanged</c> event.
		/// </summary>
		/// <param name="e">A <c>SelectionEventArgs</c> that contains the event data.</param>
		void ISyntaxEditorEditModeHandler.OnSelectionChanged(SelectionEventArgs e) {
			// Stop if active
			if (active)
				this.Active = false;
		}

		/// <summary>
		/// Raises the <c>TextChanging</c> event.
		/// </summary>
		/// <param name="e">A <c>DocumentModificationEventArgs</c> that contains the event data.</param>
		void ISyntaxEditorEditModeHandler.OnTextChanging(DocumentModificationEventArgs e) {
			// Stop if active
			if (active)
				this.Active = false;
		}

		/// <summary>
		/// Called when the parent form is deactivated.
		/// </summary>
		void ISyntaxEditorEditModeHandler.ParentFormDeactivate() {}

		/// <summary>
		/// Gets whether the handler requires notification when the parent <see cref="Form"/> is deactivated.
		/// </summary>
		/// <value>
		/// <c>true</c> if the handler requires notification when the parent <see cref="Form"/> is deactivated; otherwise, <c>false</c>.
		/// </value>
		bool ISyntaxEditorEditModeHandler.RequiresParentFormDeactivateNotification { 
			get { 
				return false;
			}
		}

		/////////////////////////////////////////////////////////////////////////////////////////////////////
		// NON-PUBLIC PROCEDURES
		/////////////////////////////////////////////////////////////////////////////////////////////////////

		/// <summary>
		/// Adds a character to the find text.
		/// </summary>
		/// <param name="ch">The character to add.</param>
		internal void AddCharacterToFindText(char ch) {
			// If ignoring, quit
			if (ignoreFurtherFindText) {
				// Raise an event
				findReplace.EditorView.SyntaxEditor.OnIncrementalSearch(new IncrementalSearchEventArgs(IncrementalSearchEventType.CharacterIgnored, null));
				return;
			}

			// Add the character
			findText += ch;
		}

		/// <summary>
		/// Performs an incremental search.
		/// </summary>
		/// <param name="startAtCurrentOffset">Whether to start at the current offset.</param>
		/// <param name="reverseSearch">Whether to reverse the <see cref="SearchUp"/> flag.</param>
		internal void PerformSearch(bool startAtCurrentOffset, bool reverseSearch) {
			// Quit if there is no find text
			if (findText.Length == 0) {
				// Raise an event
				findReplace.EditorView.SyntaxEditor.OnIncrementalSearch(
					new IncrementalSearchEventArgs(IncrementalSearchEventType.Search, new FindReplaceResultSet(true)));
				return;
			}

			// Create options
			FindReplaceOptions options = new FindReplaceOptions();
			options.MatchCase = matchCase;
			options.FindText = findText;
			options.SearchUp = searchUp;

			// If the search direction should be reversed...
			if (reverseSearch)
				options.SearchUp = !options.SearchUp;

			// Perform the find
			ignoreDeactivate = true;
			FindReplaceResultSet resultSet = findReplace.PerformFindReplace(options, FindReplaceOperation.Find, startAtCurrentOffset);
			ignoreDeactivate = false;

			// See whether further text should be ignored
			ignoreFurtherFindText = (resultSet.Count == 0);

			// Raise an event
			findReplace.EditorView.SyntaxEditor.OnIncrementalSearch(new IncrementalSearchEventArgs(IncrementalSearchEventType.Search, resultSet));
		}

		/// <summary>
		/// Removes a character from the find text.
		/// </summary>
		internal void RemoveCharacterFromFindText() {
			// Quit if there is no find text
			if (findText.Length == 0)
				return;

			// Trim the find text by one character
			if (findText.Length == 1)
				findText = String.Empty;
			else
				findText = findText.Substring(0, findText.Length - 1);
		}

		/// <summary>
		/// Sets the <see cref="MatchCase"/> property value.
		/// </summary>
		/// <param name="value">
		/// <c>true</c> if searches should be case sensitive; otherwise, <c>false</c>.
		/// </param>
		internal void SetMatchCase(bool value) {
			matchCase = value;
		}

		/// <summary>
		/// Starts incremental searching mode.
		/// </summary>
		private void Start() {
			// Init variables
			findText = String.Empty;

			// Raise an event
			findReplace.EditorView.SyntaxEditor.OnIncrementalSearch(new IncrementalSearchEventArgs(IncrementalSearchEventType.Activated, null));

			// Update the cursor
			findReplace.EditorView.SyntaxEditor.UpdateIncrementalSearchCursor();

			// Add the handler
			findReplace.EditorView.SyntaxEditor.EditModeHandlerAdd(this);
		}

		/// <summary>
		/// Stops incremental searching mode.
		/// </summary>
		private void Stop() {
			// Remove the handler
			findReplace.EditorView.SyntaxEditor.EditModeHandlerRemove(this);

			// Clear variables
			findText = String.Empty;
			ignoreFurtherFindText = false;

			// Raise an event
			findReplace.EditorView.SyntaxEditor.OnIncrementalSearch(new IncrementalSearchEventArgs(IncrementalSearchEventType.Deactivated, null));

			// Update the cursor
			findReplace.EditorView.SyntaxEditor.UpdateIncrementalSearchCursor();
		}

		/// <summary>
		/// Updates the find text from the last main find/replace operation.
		/// </summary>
		internal void UpdateFindTextFromLastSearch() {
			findText = findReplace.LastFindText;
			if (findText == null)
				findText = String.Empty;
		}

		/////////////////////////////////////////////////////////////////////////////////////////////////////
		// PUBLIC PROCEDURES
		/////////////////////////////////////////////////////////////////////////////////////////////////////

		/// <summary>
		/// Gets or sets whether incremental searching is active.
		/// </summary>
		/// <value>
		/// <c>true</c> if incremental searching is active; otherwise, <c>false</c>.
		/// </value>
		public bool Active {
			get {
				return active;
			}
			set {
				// Quit if the value is already set
				if ((ignoreDeactivate) || (active == value))
					return;

				// Set the new value
				active = value;

				// Start or stop
				if (active)
					this.Start();
				else
					this.Stop();
			}
		}

		/// <summary>
		/// Gets the text to find.
		/// </summary>
		/// <value>The text to find.</value>
		public string FindText {
			get {
				return findText;
			}
		}

		/// <summary>
		/// Gets whether searches should be case sensitive.
		/// </summary>
		/// <value>
		/// <c>true</c> if searches should be case sensitive; otherwise, <c>false</c>.
		/// </value>
		public bool MatchCase {
			get {
				return matchCase;
			}
		}

		/// <summary>
		/// Performs an incremental search.
		/// </summary>
		public void PerformSearch() {
			if (!active) {
				this.Active = true;
				return;
			}

			if (findText.Length == 0) {
				// Try to get text from the last search
				this.UpdateFindTextFromLastSearch();
			}

			this.PerformSearch(false, false);
		}

		/// <summary>
		/// Performs an incremental search.
		/// </summary>
		/// <param name="searchUp">Whether to search backwards.</param>
		public void PerformSearch(bool searchUp) {
			this.SearchUp = searchUp;
			this.PerformSearch();
		}

		/// <summary>
		/// Gets or sets whether to search backwards.
		/// </summary>
		/// <value>
		/// <c>true</c> if searches should be performed backwards; otherwise, <c>false</c>.
		/// </value>
		public bool SearchUp {
			get {
				return searchUp;
			}
			set {
				// Quit if the value is already set
				if (searchUp == value)
					return;

				// Set the new value
				searchUp = value;

				// Update the cursor
				findReplace.EditorView.SyntaxEditor.UpdateIncrementalSearchCursor();
			}
		}
	}
}
