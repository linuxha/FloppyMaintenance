using System;
using System.Collections;
using System.Diagnostics;
using System.Text;
using ActiproSoftware.ComponentModel;
using ActiproSoftware.Drawing;
using ActiproSoftware.SyntaxEditor;

namespace ActiproSoftware.SyntaxEditor {

	/// <summary>
	/// Provides access to find and replace functionality within an <see cref="EditorView"/> and uses the current selection
	/// for determining find/replace start offsets.
	/// </summary>
	public class EditorViewFindReplace {

		private bool					blockSearchStartOffsetReset		= false;
		private IncrementalSearch		incrementalSearch;
		private string					lastFindText;
		private FindReplaceOperation	lastOperation					= FindReplaceOperation.None;
		private FindReplaceOptions		options;
		private bool					pastDocumentEnd					= false;
		private int						searchStartOffset;
		private EditorView				view;

		#if TRACE
		private static TraceSwitch traceSwitch = new TraceSwitch("FindReplace", "ActiproSoftware.SyntaxEditor.FindReplace");
		#endif

		/////////////////////////////////////////////////////////////////////////////////////////////////////
		// OBJECT
		/////////////////////////////////////////////////////////////////////////////////////////////////////

		/// <summary>
		/// Initializes a new instance of the <c>EditorViewFindReplace</c> class.
		/// </summary>
		/// <param name="view">The <see cref="EditorView"/> that owns the object.</param>
		internal EditorViewFindReplace(EditorView view) {
            // Initialize parameters
			this.view = view;

			// Create an incremental search object
			incrementalSearch = new IncrementalSearch(this);

			// Initial variables
			// traceSwitch.Level = TraceLevel.Info;
		}

		/////////////////////////////////////////////////////////////////////////////////////////////////////
		// NON-PUBLIC PROCEDURES
		/////////////////////////////////////////////////////////////////////////////////////////////////////

		/// <summary>
		/// Gets the <see cref="ActiproSoftware.SyntaxEditor.EditorView"/> that owns the object.
		/// </summary>
		/// <value>The <see cref="ActiproSoftware.SyntaxEditor.EditorView"/> that owns the object.</value>
		internal EditorView EditorView {
			get {
				return view;
			}
		}

		/// <summary>
		/// Returns a <see cref="FindReplaceResultSet"/> that indicates the search went past the start offset.
		/// </summary>
		/// <returns>A <see cref="FindReplaceResultSet"/> that indicates the search went past the start offset.</returns>
		internal FindReplaceResultSet GetPastSearchStartOffsetResultSet() {
			#if TRACE
			// Write trace messages
			switch (traceSwitch.Level) {
				case TraceLevel.Info:
					Trace.WriteLine("Search went past the search start offset: " + searchStartOffset + ".");
					break;
			}
			#endif

			// Reset the search start offset
			this.ResetSearchStartOffset();

			return new FindReplaceResultSet(true);
		}

		/// <summary>
		/// Gets or sets the last text to find.
		/// </summary>
		/// <value>The last text to find.</value>
		internal string LastFindText {
			get {
				return lastFindText;
			}
		}

		/// <summary>
		/// Perform a find or replace operation.
		/// </summary>
		/// <param name="options">The find/replace options.</param>
		/// <param name="operation">The operation to perform.</param>
		/// <param name="startAtCurrentOffset">Whether the start at the current offset or the next one.</param>
		/// <returns>
		/// A <see cref="FindReplaceResultSet"/> that specifies the result of the operation.
		/// </returns>
		internal FindReplaceResultSet PerformFindReplace(FindReplaceOptions options, FindReplaceOperation operation, bool startAtCurrentOffset) {
			int matchOffset, matchLength;
			FindReplaceGroupCollection groups;

			// Validate
			this.Validate(options);

			// Create a result set
			FindReplaceResultSet resultSet = new FindReplaceResultSet();

			// If the options have changed...
			if ((this.options != options) || (options.Modified) || (lastOperation != operation)) {
				// Store the operation
				lastFindText	= options.FindText;
				lastOperation	= operation;

				// Save the options
				this.options = options;

				if ((this.options != options) || (options.Modified) || (!(
					((lastOperation == FindReplaceOperation.Find) || (lastOperation == FindReplaceOperation.Replace)) &&
					((operation == FindReplaceOperation.Find) || (operation == FindReplaceOperation.Replace))
					))) {
					// Reset the search start offset
					this.ResetSearchStartOffset();
				}
			}

			// If this is a replace, check the selection to see if it matches the find text
			if (operation == FindReplaceOperation.Replace) {
				// If a match is found...
				if (view.SyntaxEditor.Document.SearchForward(options.FindRegexCode, view.Selection.FirstOffset, view.SyntaxEditor.Document.Length, options.SearchHiddenText, true, out matchOffset, out matchLength, out groups)) {
					// If the find is the same as the current selection...
					if ((matchOffset == view.Selection.FirstOffset) && ((matchOffset + matchLength == view.Selection.LastOffset) || ((matchLength == 0) && (view.Selection.Length == 1)))) {
						// Perform a replace 

						// Get the replace text
						string replaceText = DocumentFindReplace.GetReplaceText(options, groups);

						#if TRACE
						// Write trace messages
						switch (traceSwitch.Level) {
							case TraceLevel.Info:
								Trace.WriteLine("Found match: '" + view.SyntaxEditor.Document.GetSubstring(matchOffset, matchLength) + "', ranging from " + 
									matchOffset + " to " + (matchOffset + matchLength) + ", and replacing with '" + replaceText + "'.");
								break;
						}
						#endif

						// Flag that a replace occurred
						resultSet.SetReplaceOccurred(true);

						// Replace the text that was found
						blockSearchStartOffsetReset = true;
						view.Selection.SuspendEvents();
						view.Selection.IsFindReplaceSelection = true;
						try {
							if (matchLength == 0)
								view.Selection.StartOffset = view.Selection.FirstOffset;

							view.ReplaceSelectedText(DocumentModificationType.Replace, replaceText, DocumentModificationOptions.CheckReadOnly | DocumentModificationOptions.SelectInsertedText);

							// If the replaced text was zero-length (like start/end of line)...
							if (matchLength == 0) {
								if (options.SearchUp) {
									// Move caret before the replaced text
									view.Selection.StartOffset -= replaceText.Length; 
								}
								else {
									// Move caret after the replaced text
									if (view.Selection.EndOffset == view.SyntaxEditor.Document.Length)
										view.Selection.StartOffset = 0;
									else
										view.Selection.StartOffset++;
								}
							}
						}
						finally {
							view.Selection.IsFindReplaceSelection = false;
							view.Selection.ResumeEvents();
						}
						blockSearchStartOffsetReset = false;

						// Reset the search start offset
						// this.ResetSearchStartOffset();
						searchStartOffset += (replaceText.Length - matchLength);
					}
				}
			}

			// Get the offset range
			TextRange offsetTextRange;
			if (options.SearchInSelection) {
				if (!view.Selection.FindReplaceSelectionTextRangeIsSet)
					startAtCurrentOffset = true;
				offsetTextRange = view.Selection.FindReplaceSelectionTextRange;
			}
			else
				offsetTextRange = new TextRange(0, view.SyntaxEditor.Document.Length);

			// Search 
			if (options.SearchUp) {
				// Set the start offset
				int startOffset = (startAtCurrentOffset ? view.Selection.LastOffset : view.Selection.FirstOffset);

				// Search backward
				if (view.SyntaxEditor.Document.SearchSemiBackward(options.FindRegexCode, startOffset, offsetTextRange.StartOffset, options.SearchHiddenText, false, out matchOffset, out matchLength, out groups)) {

					// Flag that the search went past the start search offset
					if ((pastDocumentEnd) && (matchOffset < searchStartOffset)) 
						return this.GetPastSearchStartOffsetResultSet();

					#if TRACE
					// Write trace messages
					switch (traceSwitch.Level) {
						case TraceLevel.Info:
							Trace.WriteLine("Found match: '" + view.SyntaxEditor.Document.GetSubstring(matchOffset, matchLength) + "', ranging from " + 
								matchOffset + " to " + (matchOffset + matchLength) + ".");
							break;
					}
					#endif

					// If not performing a Find operation where changing of selection is not wanted...
					if (!((operation == FindReplaceOperation.Find) && (!options.ChangeSelection))) {
						// Get the text range to select
						TextRange selectionTextRange;
						if ((matchLength == 0) && (matchOffset < offsetTextRange.EndOffset))
							selectionTextRange = new TextRange(matchOffset, matchOffset + 1);
						else
							selectionTextRange = new TextRange(matchOffset, matchOffset + matchLength);

						// Highlight the text that was found, but keep the search start offset the same
						view.SyntaxEditor.Document.EnsureTextRangeExpanded(selectionTextRange);
						blockSearchStartOffsetReset = true;
						view.Selection.IsFindReplaceSelection = true;
						view.Selection.SelectRange(selectionTextRange, SelectionModes.ContinuousStream);
						view.Selection.IsFindReplaceSelection = false;
						blockSearchStartOffsetReset = false;
					}

					// Add a result
					resultSet.Add(new FindReplaceResult(matchOffset, matchLength, groups));

					return resultSet;
				}

				// No match was found from start offset to end... 
				// Flag that the search went past the start search offset if the start offset was after the start search offset
				if (view.Selection.FirstOffset > searchStartOffset)
					return this.GetPastSearchStartOffsetResultSet();
				else if ((pastDocumentEnd) && (view.Selection.FirstOffset == searchStartOffset))
					return this.GetPastSearchStartOffsetResultSet();

				if (view.SyntaxEditor.Document.SearchSemiBackward(options.FindRegexCode, offsetTextRange.EndOffset, startOffset, options.SearchHiddenText, true, out matchOffset, out matchLength, out groups)) {
					// Flag that the end of the document was reached
					pastDocumentEnd = true;

					// Flag that the search went past the start search offset
					if ((pastDocumentEnd) && (matchOffset < searchStartOffset))
						return this.GetPastSearchStartOffsetResultSet();

					#if TRACE
					// Write trace messages
					switch (traceSwitch.Level) {
						case TraceLevel.Info:
							Trace.WriteLine("Found match after wrapping: '" + view.SyntaxEditor.Document.GetSubstring(matchOffset, matchLength) + "', ranging from " + 
								matchOffset + " to " + (matchOffset + matchLength) + ".");
							break;
					}
					#endif

					// If not performing a Find operation where changing of selection is not wanted...
					if (!((operation == FindReplaceOperation.Find) && (!options.ChangeSelection))) {
						// Get the text range to select
						TextRange selectionTextRange;
						if ((matchLength == 0) && (matchOffset < offsetTextRange.EndOffset))
							selectionTextRange = new TextRange(matchOffset, matchOffset + 1);
						else
							selectionTextRange = new TextRange(matchOffset, matchOffset + matchLength);

						// Highlight the text that was found, but keep the search start offset the same
						view.SyntaxEditor.Document.EnsureTextRangeExpanded(selectionTextRange);
						blockSearchStartOffsetReset = true;
						view.Selection.IsFindReplaceSelection = true;
						view.Selection.SelectRange(selectionTextRange, SelectionModes.ContinuousStream);
						view.Selection.IsFindReplaceSelection = false;
						blockSearchStartOffsetReset = false;
					}

					// Add a result
					resultSet.Add(new FindReplaceResult(matchOffset, matchLength, groups));

					resultSet.SetPastDocumentEnd(true);
					return resultSet;
				}
			}
			else {
				// Set the start offset
				int startOffset = (startAtCurrentOffset ? view.Selection.FirstOffset : view.Selection.LastOffset);

				// Search forward
				if (view.SyntaxEditor.Document.SearchForward(options.FindRegexCode, startOffset, offsetTextRange.EndOffset, options.SearchHiddenText, false, out matchOffset, out matchLength, out groups)) {
					// Flag that the search went past the start search offset
					if ((pastDocumentEnd) && (matchOffset > searchStartOffset)) 
						return this.GetPastSearchStartOffsetResultSet();

					#if TRACE
					// Write trace messages
					switch (traceSwitch.Level) {
						case TraceLevel.Info:
							Trace.WriteLine("Found match: '" + view.SyntaxEditor.Document.GetSubstring(matchOffset, matchLength) + "', ranging from " + 
								matchOffset + " to " + (matchOffset + matchLength) + ".");
							break;
					}
					#endif

					// If not performing a Find operation where changing of selection is not wanted...
					if (!((operation == FindReplaceOperation.Find) && (!options.ChangeSelection))) {
						// Get the text range to select
						TextRange selectionTextRange;
						if ((matchLength == 0) && (matchOffset < offsetTextRange.EndOffset))
							selectionTextRange = new TextRange(matchOffset, matchOffset + 1);
						else
							selectionTextRange = new TextRange(matchOffset, matchOffset + matchLength);

						// Highlight the text that was found, but keep the search start offset the same
						view.SyntaxEditor.Document.EnsureTextRangeExpanded(selectionTextRange);
						blockSearchStartOffsetReset = true;
						view.Selection.IsFindReplaceSelection = true;
						view.Selection.SelectRange(selectionTextRange, SelectionModes.ContinuousStream);
						view.Selection.IsFindReplaceSelection = false;
						blockSearchStartOffsetReset = false;
					}

					// Add a result
					resultSet.Add(new FindReplaceResult(matchOffset, matchLength, groups));

					return resultSet;
				}

				// No match was found from start offset to end... 
				// Flag that the search went past the start search offset if the start offset was before the start search offset
				if (view.Selection.FirstOffset < searchStartOffset)
					return this.GetPastSearchStartOffsetResultSet();
				else if ((pastDocumentEnd) && (view.Selection.FirstOffset == searchStartOffset))
					return this.GetPastSearchStartOffsetResultSet();
				
				if (view.SyntaxEditor.Document.SearchForward(options.FindRegexCode, offsetTextRange.StartOffset, startOffset, options.SearchHiddenText, false, out matchOffset, out matchLength, out groups)) {
					// Flag that the end of the document was reached
					pastDocumentEnd = true;

					// Flag that the search went past the start search offset
					if ((pastDocumentEnd) && (matchOffset >= searchStartOffset))  // 2/16/2010 - Changed to >= (http://www.actiprosoftware.com/Support/Forums/ViewForumTopic.aspx?ForumTopicID=4559#17025)
						return this.GetPastSearchStartOffsetResultSet();

					#if TRACE
					// Write trace messages
					switch (traceSwitch.Level) {
						case TraceLevel.Info:
							Trace.WriteLine("Found match after wrapping: '" + view.SyntaxEditor.Document.GetSubstring(matchOffset, matchLength) + "', ranging from " + 
								matchOffset + " to " + (matchOffset + matchLength) + ".");
							break;
					}
					#endif

					// If not performing a Find operation where changing of selection is not wanted...
					if (!((operation == FindReplaceOperation.Find) && (!options.ChangeSelection))) {
						// Get the text range to select
						TextRange selectionTextRange;
						if ((matchLength == 0) && (matchOffset < offsetTextRange.EndOffset))
							selectionTextRange = new TextRange(matchOffset, matchOffset + 1);
						else
							selectionTextRange = new TextRange(matchOffset, matchOffset + matchLength);

						// Highlight the text that was found, but keep the search start offset the same
						view.SyntaxEditor.Document.EnsureTextRangeExpanded(selectionTextRange);
						blockSearchStartOffsetReset = true;
						view.Selection.IsFindReplaceSelection = true;
						view.Selection.SelectRange(selectionTextRange, SelectionModes.ContinuousStream);
						view.Selection.IsFindReplaceSelection = false;
						blockSearchStartOffsetReset = false;
					}

					// Add a result
					resultSet.Add(new FindReplaceResult(matchOffset, matchLength, groups));

					resultSet.SetPastDocumentEnd(true);
					return resultSet;
				}
			}

			#if TRACE
			// Write trace messages
			switch (traceSwitch.Level) {
				case TraceLevel.Info:
					Trace.WriteLine("No match was found.");
					break;
			}
			#endif

			resultSet.SetPastDocumentEnd(true);
			return resultSet;
		}

		/// <summary>
		/// Resets the search start offset to the current location in the document.
		/// </summary>
		internal void ResetSearchStartOffset() {
			// Quit if blocked
			if (blockSearchStartOffsetReset)
				return;

			// Reset the offset
			searchStartOffset = view.Selection.FirstOffset;
			pastDocumentEnd = false;

			#if TRACE
			// Write trace messages
			switch (traceSwitch.Level) {
				case TraceLevel.Info:
					Trace.WriteLine("Resetting search start offset to: " + searchStartOffset + ".");
					break;
			}
			#endif
		}

		/// <summary>
		/// Validates the options.
		/// </summary>
		/// <param name="options">The <see cref="FindReplaceOptions"/> to validate.</param>
		private void Validate(FindReplaceOptions options) {
			// Call the document find/replace's validate method
			view.SyntaxEditor.Document.FindReplace.Validate(options);

			// If there is no DFA in the options...
			if (options.Modified) {
				#if TRACE
				// Write trace messages
				switch (traceSwitch.Level) {
					case TraceLevel.Info:
						Trace.WriteLine("Creating a new DFA for search.");
						break;
				}
				#endif

				// Update the incremental search match case
				incrementalSearch.SetMatchCase(options.MatchCase);
			}
		}

		/////////////////////////////////////////////////////////////////////////////////////////////////////
		// PUBLIC PROCEDURES
		/////////////////////////////////////////////////////////////////////////////////////////////////////

		/// <summary>
		/// Performs a find operation.
		/// </summary>
		/// <param name="options">The find/replace options.</param>
		/// <returns>
		/// A <see cref="FindReplaceResultSet"/> that specifies the result of the operation.
		/// </returns>
		public FindReplaceResultSet Find(FindReplaceOptions options) {
			FindReplaceResultSet resultSet = this.PerformFindReplace(options, FindReplaceOperation.Find, false);

			// If doing a search in selection and are past the start offset, reselect the range
			if ((options.SearchInSelection) && (resultSet.PastSearchStartOffset)) {
				TextRange selectionTextRange = view.Selection.FindReplaceSelectionTextRange;
				view.Selection.SelectRange(new TextRange(selectionTextRange.EndOffset, selectionTextRange.EndOffset));  // NOTE: For some reason the selection needs to be changed twice for things to work properly
				view.Selection.SelectRange(selectionTextRange);
			}

			return resultSet;
		}

		/// <summary>
		/// Gets the <see cref="ActiproSoftware.SyntaxEditor.IncrementalSearch"/> that allows incremental searching.
		/// </summary>
		/// <value>The <see cref="ActiproSoftware.SyntaxEditor.IncrementalSearch"/> that allows incremental searching.</value>
		public IncrementalSearch IncrementalSearch {
			get {
				return incrementalSearch;
			}
		}

		/// <summary>
		/// Gets the last operation that was performed.
		/// </summary>
		/// <value>
		/// A <see cref="FindReplaceOperation"/> specifying the last operation that was performed.
		/// </value>
		public FindReplaceOperation LastOperation {
			get {
				return lastOperation;
			}
		}

		/// <summary>
		/// Performs a mark all operation that marks any line that has the text to find with a <see cref="BookmarkLineIndicator"/>.
		/// </summary>
		/// <param name="options">The find/replace options.</param>
		/// <returns>
		/// A <see cref="FindReplaceResultSet"/> that specifies the result of the operation.
		/// </returns>
		public FindReplaceResultSet MarkAll(FindReplaceOptions options) {
			return this.MarkAll(options, typeof(BookmarkLineIndicator));
		}

		/// <summary>
		/// Performs a mark all operation that marks any line that has the text to find with an indicator.
		/// </summary>
		/// <param name="options">The find/replace options.</param>
		/// <param name="indicatorType">The <see cref="Type"/> to of line indicator use for marking the lines.</param>
		/// <returns>
		/// A <see cref="FindReplaceResultSet"/> that specifies the result of the operation.
		/// </returns>
		public FindReplaceResultSet MarkAll(FindReplaceOptions options, Type indicatorType) {
			// Validate
			this.Validate(options);

			// If the options have changed...
			if ((this.options != options) || (options.Modified) || (lastOperation != FindReplaceOperation.MarkAll)) {
				// Store the operation
				lastFindText	= options.FindText;
				lastOperation	= FindReplaceOperation.MarkAll;

				// Save the options
				this.options = options;

				// Reset the search start offset
				this.ResetSearchStartOffset();
			}

			// Get the offset range
			TextRange offsetTextRange;
			if (options.SearchInSelection)
				offsetTextRange = view.Selection.FindReplaceSelectionTextRange;
			else
				offsetTextRange = new TextRange(0, view.SyntaxEditor.Document.Length);

			// Perform the mark all
			FindReplaceResultSet resultSet = view.SyntaxEditor.Document.FindReplace.MarkAll(options, offsetTextRange, indicatorType);

			return resultSet;
		}

		/// <summary>
		/// Performs a replace operation.
		/// </summary>
		/// <param name="options">The find/replace options.</param>
		/// <returns>
		/// A <see cref="FindReplaceResultSet"/> that specifies the result of the operation.
		/// </returns>
		public FindReplaceResultSet Replace(FindReplaceOptions options) {
			FindReplaceResultSet resultSet = this.PerformFindReplace(options, FindReplaceOperation.Replace, false);
			
			// If doing a search in selection and are past the start offset, reselect the range
			if ((options.SearchInSelection) && (resultSet.PastDocumentEnd) && (resultSet.ReplaceOccurred)) {
				TextRange selectionTextRange = view.Selection.FindReplaceSelectionTextRange;
				view.Selection.SelectRange(new TextRange(selectionTextRange.EndOffset, selectionTextRange.EndOffset));  // NOTE: For some reason the selection needs to be changed twice for things to work properly
				view.Selection.SelectRange(selectionTextRange);
			}

			return resultSet;
		}

		/// <summary>
		/// Performs a replace all operation.
		/// </summary>
		/// <param name="options">The find/replace options.</param>
		/// <returns>
		/// A <see cref="FindReplaceResultSet"/> that specifies the result of the operation.
		/// </returns>
		public FindReplaceResultSet ReplaceAll(FindReplaceOptions options) {
			FindReplaceResultSet resultSet;

			// Validate
			this.Validate(options);

			// If the options have changed...
			if ((this.options != options) || (options.Modified) || (lastOperation != FindReplaceOperation.ReplaceAll)) {
				// Store the operation
				lastFindText	= options.FindText;
				lastOperation	= FindReplaceOperation.ReplaceAll;

				// Save the options
				this.options = options;

				// Reset the search start offset
				this.ResetSearchStartOffset();
			}

			// Get the selection
			Selection selection = view.Selection;

			try {
				// Block search start offset from resetting
				blockSearchStartOffsetReset = true;

				// Suspend selection events
				selection.SuspendEvents();
				view.SyntaxEditor.SuspendPainting();

				// Get the offset range
				TextRange adjustedTextRange;
				TextRange[] offsetTextRanges;
				if (options.SearchInSelection) {
					adjustedTextRange = view.Selection.FindReplaceSelectionTextRange;
					if ((!view.Selection.IsZeroLength) && (view.Selection.Mode == SelectionModes.Block))
						offsetTextRanges = view.Selection.GetBlockSelectionTextRanges(true);
					else
						offsetTextRanges = new TextRange[] { adjustedTextRange };
				}
				else {
					adjustedTextRange = new TextRange(0, view.SyntaxEditor.Document.Length);
					offsetTextRanges = new TextRange[] { adjustedTextRange };
				}

				// Perform the replace all
				resultSet = view.SyntaxEditor.Document.FindReplace.ReplaceAll(options, offsetTextRanges, ref adjustedTextRange);

				if (options.SearchInSelection) {
					// Select the adjusted range
					view.Selection.SelectRange(adjustedTextRange);
				}
			}
			finally {
				// Un-block search start offset from resetting
				blockSearchStartOffsetReset = false;

				// Resume selection events
				view.SyntaxEditor.ResumePainting();
				selection.ResumeEvents();
			}

			return resultSet;
		}

		/// <summary>
		/// Gets the offset at which the search starts in the document.
		/// </summary>
		/// <value>The offset at which the search starts in the document.</value>
		public int SearchStartOffset {
			get {
				return searchStartOffset;
			}
		}	
	}
}
