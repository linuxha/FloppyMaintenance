using System;
using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;
using ActiproSoftware.ComponentModel;
using ActiproSoftware.Drawing;
using ActiproSoftware.SyntaxEditor;

namespace ActiproSoftware.SyntaxEditor {

	/// <summary>
	/// Provides access to find and replace functionality within a <see cref="Document"/>, which is a non-user interface
	/// alternative for the <see cref="EditorView.FindReplace"/> object model for an <see cref="EditorView"/>.
	/// </summary>
	public class DocumentFindReplace {

		private Document	document;

		#if TRACE
		private static TraceSwitch traceSwitch = new TraceSwitch("FindReplace", "ActiproSoftware.SyntaxEditor.FindReplace");
		#endif

		/////////////////////////////////////////////////////////////////////////////////////////////////////
		// OBJECT
		/////////////////////////////////////////////////////////////////////////////////////////////////////

		/// <summary>
		/// Initializes a new instance of the <c>DocumentFindReplace</c> class.
		/// </summary>
		/// <param name="document">The <see cref="Document"/> that owns the object.</param>
		internal DocumentFindReplace(Document document) {
            // Initialize parameters
			this.document = document;

			// Initial variables
			// traceSwitch.Level = TraceLevel.Info;
		}

		/////////////////////////////////////////////////////////////////////////////////////////////////////
		// NON-PUBLIC PROCEDURES
		/////////////////////////////////////////////////////////////////////////////////////////////////////

		/// <summary>
		/// Returns a <see cref="FindReplaceResultSet"/> that indicates the search went past the start offset.
		/// </summary>
		/// <param name="searchStartOffset">The start offset of a search.</param>
		/// <returns>A <see cref="FindReplaceResultSet"/> that indicates the search went past the start offset.</returns>
		internal FindReplaceResultSet GetPastSearchStartOffsetResultSet(int searchStartOffset) {
			#if TRACE
			// Write trace messages
			switch (traceSwitch.Level) {
				case TraceLevel.Info:
					Trace.WriteLine("Search went past the search start offset: " + searchStartOffset + ".");
					break;
			}
			#endif

			return new FindReplaceResultSet(true);
		}

		/// <summary>
		/// Returns the replace text.
		/// </summary>
		/// <param name="options">The find/replace options.</param>
		/// <param name="groups">The list of captured groups.</param>
		/// <returns>The replace text.</returns>
		internal static string GetReplaceText(FindReplaceOptions options, FindReplaceGroupCollection groups) {
			// Normal search so return the exact replace text
			if (options.SearchType == FindReplaceSearchType.RegularExpression) {
				// Regular expression search so look for substitutions
				StringBuilder replaceText = new StringBuilder();
				DocumentFindReplace.GetReplaceTextRecursive(replaceText, options.ReplaceRegexNode, groups);
				return replaceText.ToString();
			}

			return options.ReplaceText;
		}

		/// <summary>
		/// Recurses through the replace text regex node tree and build replace text.
		/// </summary>
		/// <param name="replaceText">The <see cref="StringBuilder"/> containing replace text.</param>
		/// <param name="node">The current <see cref="RegexNode"/>.</param>
		/// <param name="groups">The list of captured groups.</param>
		/// <remarks>The regex tree will only consist of RegexConcatenationNode, RegexStringNode, and RegexSubstitutionNode nodes.</remarks>
		private static void GetReplaceTextRecursive(StringBuilder replaceText, RegexNode node, FindReplaceGroupCollection groups) {
			if (node == null)
				return;

			switch (node.NodeType) {
				case RegexNode.Concatenation:
					foreach (RegexNode childNode in node.Children)
						DocumentFindReplace.GetReplaceTextRecursive(replaceText, childNode, groups);
					break;
				case RegexNode.Char:
				case RegexNode.String:
					replaceText.Append(node.StringData);
					break;
				case RegexNode.Substitution:
					FindReplaceGroup group = groups[node.StringData];
					if (group != null)
						replaceText.Append(group.Text);
					break;
			}
		}

		/// <summary>
		/// Perform a find or replace operation.
		/// </summary>
		/// <param name="options">The find/replace options.</param>
		/// <param name="startOffset">The offset at which to start the search</param>
		/// <returns>
		/// A <see cref="FindReplaceResultSet"/> that specifies the result of the operation.
		/// </returns>
		internal FindReplaceResultSet PerformFindReplace(FindReplaceOptions options, int startOffset) {
			int matchOffset, matchLength;
			FindReplaceGroupCollection groups;

			// Validate
			this.Validate(options);

			// Create a result set
			FindReplaceResultSet resultSet = new FindReplaceResultSet();

			// Search 
			if (options.SearchUp) {
				// Search backward
				if (document.SearchSemiBackward(options.FindRegexCode, startOffset, 0, options.SearchHiddenText, false, out matchOffset, out matchLength, out groups)) {
					#if TRACE
					// Write trace messages
					switch (traceSwitch.Level) {
						case TraceLevel.Info:
							Trace.WriteLine("Found match: '" + document.GetSubstring(matchOffset, matchLength) + "', ranging from " + 
								matchOffset + " to " + (matchOffset + matchLength) + ".");
							break;
					}
					#endif

					// Add a result
					resultSet.Add(new FindReplaceResult(matchOffset, matchLength, groups));

					return resultSet;
				}

				if (document.SearchSemiBackward(options.FindRegexCode, document.Length, startOffset, options.SearchHiddenText, true, out matchOffset, out matchLength, out groups)) {
					// Flag that the search went past the start search offset
					if (matchOffset < startOffset)
						return this.GetPastSearchStartOffsetResultSet(startOffset);

					#if TRACE
					// Write trace messages
					switch (traceSwitch.Level) {
						case TraceLevel.Info:
							Trace.WriteLine("Found match after wrapping: '" + document.GetSubstring(matchOffset, matchLength) + "', ranging from " + 
								matchOffset + " to " + (matchOffset + matchLength) + ".");
							break;
					}
					#endif

					// Add a result
					resultSet.Add(new FindReplaceResult(matchOffset, matchLength, groups));

					resultSet.SetPastDocumentEnd(true);
					return resultSet;
				}
			}
			else {
				// Search forward
				if (document.SearchForward(options.FindRegexCode, startOffset, document.Length, options.SearchHiddenText, false, out matchOffset, out matchLength, out groups)) {
					#if TRACE
					// Write trace messages
					switch (traceSwitch.Level) {
						case TraceLevel.Info:
							Trace.WriteLine("Found match: '" + document.GetSubstring(matchOffset, matchLength) + "', ranging from " + 
								matchOffset + " to " + (matchOffset + matchLength) + ".");
							break;
					}
					#endif

					// Add a result
					resultSet.Add(new FindReplaceResult(matchOffset, matchLength, groups));

					return resultSet;
				}

				if (document.SearchForward(options.FindRegexCode, 0, startOffset, options.SearchHiddenText, false, out matchOffset, out matchLength, out groups)) {
					// Flag that the search went past the start search offset
					if (matchOffset > startOffset)
						return this.GetPastSearchStartOffsetResultSet(startOffset);

					#if TRACE
					// Write trace messages
					switch (traceSwitch.Level) {
						case TraceLevel.Info:
							Trace.WriteLine("Found match after wrapping: '" + document.GetSubstring(matchOffset, matchLength) + "', ranging from " + 
								matchOffset + " to " + (matchOffset + matchLength) + ".");
							break;
					}
					#endif

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
		/// Performs a replace all operation.
		/// </summary>
		/// <param name="options">The find/replace options.</param>
		/// <param name="offsetTextRanges">The <see cref="TextRange"/> array of offsets to search.  If everything should be searched, specify the range <c>0</c> to the length of the <see cref="ActiproSoftware.SyntaxEditor.Document"/>.</param>
		/// <param name="adjustedTextRange">Returns the adjusted <see cref="TextRange"/> that was searched based on replacements.</param>
		/// <returns>
		/// A <see cref="FindReplaceResultSet"/> that specifies the result of the operation.
		/// </returns>
		internal FindReplaceResultSet ReplaceAll(FindReplaceOptions options, TextRange[] offsetTextRanges, ref TextRange adjustedTextRange) {
			if ((offsetTextRanges == null) || (offsetTextRanges.Length == 0))
				throw new ArgumentNullException("offsetTextRanges");

			// Validate
			this.Validate(options);

			// Create a result set
			FindReplaceResultSet resultSet = new FindReplaceResultSet();

			// Search 
			int currentOffset;
			int matchOffset, matchLength;
			FindReplaceGroupCollection groups;
			bool forceSkipToNextCharacter = (options.FindText != null) && (options.FindText.Trim().StartsWith("^"));

			try {
				// Suspend selection events and start an undo group
				document.OnUserInterfaceCommand(new UserInterfaceCommandEventArgs(UserInterfaceCommands.SuspendSelectionEvents));
				document.SuspendParsing();
				document.UndoRedo.StartGroup(DocumentModificationType.ReplaceAll);
				
				for (int index = offsetTextRanges.Length - 1; index >= 0; index--) {
					// Normalize the range
					var offsetTextRange = offsetTextRanges[index];
					offsetTextRange.Normalize();

					// Search forward
					currentOffset = offsetTextRange.StartOffset;
					while (document.SearchForward(options.FindRegexCode, currentOffset, document.Length, options.SearchHiddenText, false, out matchOffset, out matchLength, out groups)) {
						// Quit if the match occurs outside the offset range
						if (matchOffset + matchLength > offsetTextRange.EndOffset)
							break;

						// Get the replace text
						string replaceText = DocumentFindReplace.GetReplaceText(options, groups);

						#if TRACE
						// Write trace messages
						switch (traceSwitch.Level) {
							case TraceLevel.Info:
								Trace.WriteLine("Found match: '" + document.GetSubstring(matchOffset, matchLength) + "', ranging from " + 
									matchOffset + " to " + (matchOffset + matchLength) + ", and replacing with '" + replaceText + "'.");
								break;
						}
						#endif

						// Flag that a replace occurred
						resultSet.SetReplaceOccurred(true);

						// Replace the text
						document.EnsureTextRangeExpanded(new TextRange(matchOffset, matchOffset + matchLength));
						document.ReplaceText(DocumentModificationType.Replace, matchOffset, matchLength, replaceText);

						// Adjust the end offset range based on the delta between find/replace text lengths
						offsetTextRange = new TextRange(offsetTextRange.StartOffset, offsetTextRange.EndOffset + (replaceText.Length - matchLength));
						adjustedTextRange = new TextRange(adjustedTextRange.StartOffset, adjustedTextRange.EndOffset + (replaceText.Length - matchLength));

						// Add a result
						resultSet.Add(new FindReplaceResult(matchOffset, matchLength, groups));

						// Set the new current offset
						currentOffset = matchOffset + replaceText.Length + ((forceSkipToNextCharacter) || (matchLength == 0) ? 1 : 0);
					}
				}
			}
			finally {
				// End the undo group and resume selection events
				document.UndoRedo.EndGroup();
				document.ResumeParsing();
				document.OnUserInterfaceCommand(new UserInterfaceCommandEventArgs(UserInterfaceCommands.ResumeSelectionEvents));
			}

			// If no matches were found...
			if (resultSet.Count == 0) {
				#if TRACE
				// Write trace messages
				switch (traceSwitch.Level) {
					case TraceLevel.Info:
						Trace.WriteLine("No match was found.");
						break;
				}
				#endif
			}

			return resultSet;
		}

		/// <summary>
		/// Validates the options.
		/// </summary>
		/// <param name="options">The <see cref="FindReplaceOptions"/> to validate.</param>
		internal void Validate(FindReplaceOptions options) {
			// If there is no find text, throw an exception
			if ((options.FindText == null) || (options.FindText.Length == 0))
				throw new ApplicationException("No find text was specified.");
		}

		/////////////////////////////////////////////////////////////////////////////////////////////////////
		// PUBLIC PROCEDURES
		/////////////////////////////////////////////////////////////////////////////////////////////////////

		/// <summary>
		/// Clears the <see cref="SpanIndicator"/> find result marks that have been applied to the document via a call to <see cref="MarkAll"/>.
		/// </summary>
		public void ClearSpanIndicatorMarks() {
			lock (document.SpanIndicatorLayers.SyncRoot) {
				SpanIndicatorLayer layer = document.SpanIndicatorLayers[SpanIndicatorLayer.FindResultKey];
				if (layer != null)
					document.SpanIndicatorLayers.Remove(layer);
			}
		}

		/// <summary>
		/// Performs a find operation.
		/// </summary>
		/// <param name="options">The find/replace options.</param>
		/// <param name="startOffset">The offset at which to start searching.</param>
		/// <returns>
		/// A <see cref="FindReplaceResultSet"/> that specifies the result of the operation.
		/// </returns>
		public FindReplaceResultSet Find(FindReplaceOptions options, int startOffset) {
			return this.PerformFindReplace(options, startOffset);
		}

		/// <summary>
		/// Performs a find all operation.
		/// </summary>
		/// <param name="options">The find/replace options.</param>
		/// <returns>
		/// A <see cref="FindReplaceResultSet"/> that specifies the result of the operation.
		/// </returns>
		public FindReplaceResultSet FindAll(FindReplaceOptions options) {
			return this.FindAll(options, new TextRange(0, document.Length));
		}

		/// <summary>
		/// Performs a find all operation.
		/// </summary>
		/// <param name="options">The find/replace options.</param>
		/// <param name="offsetTextRange">The <see cref="TextRange"/> of offsets to search.  If all should be searched, specify the range <c>0</c> to the length of the <see cref="ActiproSoftware.SyntaxEditor.Document"/>.</param>
		/// <returns>
		/// A <see cref="FindReplaceResultSet"/> that specifies the result of the operation.
		/// </returns>
		public FindReplaceResultSet FindAll(FindReplaceOptions options, TextRange offsetTextRange) {
			// Validate
			this.Validate(options);

			// Normalize the range
			offsetTextRange.Normalize();

			// Create a result set
			FindReplaceResultSet resultSet = new FindReplaceResultSet();

			// Search 
			int currentOffset;
			int matchOffset, matchLength;
			FindReplaceGroupCollection groups;

			// Search forward
			currentOffset = offsetTextRange.StartOffset;
			while (document.SearchForward(options.FindRegexCode, currentOffset, document.Length, options.SearchHiddenText, false, out matchOffset, out matchLength, out groups)) {
				// Quit if the match occurs outside the offset range
				if (matchOffset + matchLength > offsetTextRange.EndOffset)
					break;

				#if TRACE
				// Write trace messages
				switch (traceSwitch.Level) {
					case TraceLevel.Info:
						Trace.WriteLine("Found match: '" + document.GetSubstring(matchOffset, matchLength) + "', ranging from " + 
							matchOffset + " to " + (matchOffset + matchLength) + ".");
						break;
				}
				#endif

				// Add a result
				resultSet.Add(new FindReplaceResult(matchOffset, matchLength, groups));

				// Set the new current offset
				currentOffset = matchOffset + (matchLength > 0 ? matchLength : 1);
			}

			// If no matches were found...
			if (resultSet.Count == 0) {
				#if TRACE
				// Write trace messages
				switch (traceSwitch.Level) {
					case TraceLevel.Info:
						Trace.WriteLine("No match was found.");
						break;
				}
				#endif
			}

			return resultSet;
		}

		/// <summary>
		/// Performs a mark all operation that marks any line that has the text to find with a <see cref="BookmarkLineIndicator"/>.
		/// </summary>
		/// <param name="options">The find/replace options.</param>
		/// <returns>
		/// A <see cref="FindReplaceResultSet"/> that specifies the result of the operation.
		/// </returns>
		public FindReplaceResultSet MarkAll(FindReplaceOptions options) {
			return this.MarkAll(options, new TextRange(0, document.Length), typeof(BookmarkLineIndicator));
		}

		/// <summary>
		/// Performs a mark all operation that marks any line that has the text to find with a <see cref="BookmarkLineIndicator"/>.
		/// </summary>
		/// <param name="options">The find/replace options.</param>
		/// <param name="offsetTextRange">The <see cref="TextRange"/> of offsets to search.  If all should be searched, specify the range <c>0</c> to the length of the <see cref="ActiproSoftware.SyntaxEditor.Document"/>.</param>
		/// <returns>
		/// A <see cref="FindReplaceResultSet"/> that specifies the result of the operation.
		/// </returns>
		public FindReplaceResultSet MarkAll(FindReplaceOptions options, TextRange offsetTextRange) {
			return this.MarkAll(options, offsetTextRange, typeof(BookmarkLineIndicator));
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
			return this.MarkAll(options, new TextRange(0, document.Length), indicatorType);
		}

		/// <summary>
		/// Performs a mark all operation that marks any line that has the text to find with an indicator.
		/// </summary>
		/// <param name="options">The find/replace options.</param>
		/// <param name="offsetTextRange">The <see cref="TextRange"/> of offsets to search.  If all should be searched, specify the range <c>0</c> to the length of the <see cref="ActiproSoftware.SyntaxEditor.Document"/>.</param>
		/// <param name="indicatorType">The <see cref="Type"/> to of line indicator use for marking the lines.</param>
		/// <returns>
		/// A <see cref="FindReplaceResultSet"/> that specifies the result of the operation.
		/// </returns>
		public FindReplaceResultSet MarkAll(FindReplaceOptions options, TextRange offsetTextRange, Type indicatorType) {
			bool isLineIndicator = true;

			// Validate
			this.Validate(options);

			// Normalize the range
			offsetTextRange.Normalize();

			// Create a result set
			FindReplaceResultSet resultSet = new FindReplaceResultSet();

			// Ensure indicator type is valid
			if (indicatorType.IsSubclassOf(typeof(LineIndicator)))
				isLineIndicator = true;
			else if (indicatorType.IsSubclassOf(typeof(SpanIndicator)))
				isLineIndicator = false;
			else
				throw new ApplicationException("The specified indicator type for the Mark All operation does not inherit from LineIndicator or SpanIndicator.");

			// Search 
			int currentOffset;
			int matchOffset, matchLength;
			FindReplaceGroupCollection groups;
		
			lock (document.SpanIndicatorLayers.SyncRoot) {
				if (!isLineIndicator) {
					// Remove the find result layer if there is one
					this.ClearSpanIndicatorMarks();

					// Add the find result layer 
					document.SpanIndicatorLayers.Add(new SpanIndicatorLayer(SpanIndicatorLayer.FindResultKey, SpanIndicatorLayer.FindResultDisplayPriority));
				}

				// Search forward
				currentOffset = offsetTextRange.StartOffset;
				while (document.SearchForward(options.FindRegexCode, currentOffset, document.Length, options.SearchHiddenText, false, out matchOffset, out matchLength, out groups)) {
					// Quit if the match occurs outside the offset range
					if (matchOffset + matchLength > offsetTextRange.EndOffset)
						break;

					#if TRACE
					// Write trace messages
					switch (traceSwitch.Level) {
						case TraceLevel.Info:
							Trace.WriteLine("Found match: '" + document.GetSubstring(matchOffset, matchLength) + "', ranging from " + 
								matchOffset + " to " + (matchOffset + matchLength) + ".");
							break;
					}
					#endif

					// Add the indicator
					if (isLineIndicator)
						document.LineIndicators.Add(Activator.CreateInstance(indicatorType) as LineIndicator, document.Lines.IndexOf(matchOffset));
					else if ((matchLength > 0) || (matchOffset < document.Length))
						document.SpanIndicatorLayers[SpanIndicatorLayer.FindResultKey].Add(Activator.CreateInstance(indicatorType) as SpanIndicator, 
							matchOffset, (matchLength > 0 ? matchLength : 1));

					// Add a result
					resultSet.Add(new FindReplaceResult(matchOffset, matchLength, groups));

					// Set the new current offset
					currentOffset = matchOffset + (matchLength > 0 ? matchLength : 1);
				}
			}

			// If no matches were found...
			if (resultSet.Count == 0) {
				if (!isLineIndicator) {
					// Remove the find result layer if there is one
					this.ClearSpanIndicatorMarks();
				}

				#if TRACE
				// Write trace messages
				switch (traceSwitch.Level) {
					case TraceLevel.Info:
						Trace.WriteLine("No match was found.");
						break;
				}
				#endif
			}

			return resultSet;
		}

		/// <summary>
		/// Performs a replace operation on a result from the previous call to <see cref="Find"/>.
		/// </summary>
		/// <param name="options">The find/replace options.</param>
		/// <param name="result">A <see cref="FindReplaceResult"/> indicating the return value of the last call to <see cref="Find"/> to replace.</param>
		/// <returns>
		/// A <see cref="FindReplaceResultSet"/> that specifies the result of the operation.
		/// </returns>
		public FindReplaceResultSet Replace(FindReplaceOptions options, FindReplaceResult result) {
			// Get the replace text
			string replaceText = DocumentFindReplace.GetReplaceText(options, result.Groups);

			// Replace the text
			document.ReplaceText(DocumentModificationType.Replace, result.TextRange, replaceText);

			// Create a result set
			FindReplaceResultSet resultSet = new FindReplaceResultSet();
			resultSet.Add(result);
			resultSet.SetReplaceOccurred(true);
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
			return this.ReplaceAll(options, new TextRange(0, document.Length));
		}
		
		/// <summary>
		/// Performs a replace all operation.
		/// </summary>
		/// <param name="options">The find/replace options.</param>
		/// <param name="offsetTextRange">The <see cref="TextRange"/> offsets to search.  If everything should be searched, specify the range <c>0</c> to the length of the <see cref="ActiproSoftware.SyntaxEditor.Document"/>.</param>
		/// <returns>
		/// A <see cref="FindReplaceResultSet"/> that specifies the result of the operation.
		/// </returns>
		public FindReplaceResultSet ReplaceAll(FindReplaceOptions options, TextRange offsetTextRange) {
			TextRange adjustedTextRange = offsetTextRange;
			return this.ReplaceAll(options, new TextRange[] { offsetTextRange }, ref adjustedTextRange);
		}

	}
}
