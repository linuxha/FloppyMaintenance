using System;
using System.Text;
using System.Text.RegularExpressions;
using ActiproSoftware.ComponentModel;
using ActiproSoftware.SyntaxEditor;

namespace ActiproSoftware.SyntaxEditor {

	/// <summary>
	/// Provides an object to store find and replace operation options.
	/// </summary>
	public class FindReplaceOptions : DisposableObject {

		private bool					changeSelection		= true;
		private RegexCode				findRegexCode;
		private string					findText;
		private bool					matchCase;
		private bool					matchWholeWord;
		private RegexNode				replaceRegexNode;
		private string					replaceText;
		private bool					searchHiddenText	= true;
		private bool					searchInSelection;
		private FindReplaceSearchType	searchType			= FindReplaceSearchType.Normal;
		private bool					searchUp;

		/////////////////////////////////////////////////////////////////////////////////////////////////////
		// OBJECT
		/////////////////////////////////////////////////////////////////////////////////////////////////////

		/// <summary>
		/// Initializes a new instance of the <c>FindReplaceOptions</c> class.
		/// </summary>
		/// <remarks>
		/// The default constructor initializes all fields to their default values.
		/// </remarks>
		public FindReplaceOptions() {
		}

		/////////////////////////////////////////////////////////////////////////////////////////////////////
		// NON-PUBLIC PROCEDURES
		/////////////////////////////////////////////////////////////////////////////////////////////////////

		/// <summary>
		/// Gets the <see cref="RegexCode"/> used for searching.
		/// </summary>
		/// <value>The <see cref="RegexCode"/> used for searching.</value>
		internal RegexCode FindRegexCode {
			get {
				if (findRegexCode == null) {
					// Build the pattern
					string pattern;
					switch (searchType) {
						case FindReplaceSearchType.Normal:
							pattern = "\"" + RegexHelper.Escape(findText) + "\"";
							break;
						case FindReplaceSearchType.Wildcard:
							pattern = RegexParser.WildcardToRegexPattern(findText);
							break;
						default:
							pattern = findText;
							break;
					}

					// Handle whole words
                    if ((matchWholeWord) && (findText != null) && (findText.Length > 0)) {
						// 3/10/2011 - Added Char.IsLetter check since CharClass.Word doesn't yet handle Unicode character classes like in WPF/Silverlight
						pattern = ((searchType != FindReplaceSearchType.Normal) || (Char.IsLetter(findText[0])) || (CharClass.Word.Contains(findText[0])) ? @"\b " : String.Empty) + pattern +
							((searchType != FindReplaceSearchType.Normal) || (Char.IsLetter(findText[findText.Length - 1])) || (CharClass.Word.Contains(findText[findText.Length - 1])) ? @" \b" : String.Empty);
					}

					// Capture the whole match
					pattern = String.Format(@"( {0} )", pattern);

					// Compile the regex
					RegexNode node = new RegexParser().ParsePattern(pattern, true, false, 
						(matchCase ? CaseSensitivity.Sensitive : CaseSensitivity.Insensitive));
					findRegexCode = new RegexCompiler().Compile(node, false);
				}
				
				return findRegexCode;			
			}
		}

		/// <summary>
		/// Gets the <see cref="RegexNode"/> used for replacing.
		/// </summary>
		/// <value>The <see cref="RegexNode"/> used for replacing.</value>
		internal RegexNode ReplaceRegexNode {
			get {
				if (replaceRegexNode == null) {
					// Build the pattern
					string pattern;
					switch (searchType) {
						case FindReplaceSearchType.Normal:
						case FindReplaceSearchType.Wildcard:
							// 9/25/2010 - Updated so that regex replace patterns allow escape characters
							pattern = RegexHelper.Escape(replaceText, true);
							break;
						default:
							pattern = replaceText;
							break;
					}

					// Return null if there is no pattern
					if ((pattern == null) || (pattern.Length == 0))
						return null;

					// Compile the regex
					replaceRegexNode = new RegexParser().ParsePattern(pattern, true, true, CaseSensitivity.Sensitive);
				}
				
				return replaceRegexNode;			
			}
		}

		/// <summary>
		/// Resets the regexes.
		/// </summary>
		private void ResetRegexes() {
			findRegexCode = null;
			replaceRegexNode = null;
		}

		/////////////////////////////////////////////////////////////////////////////////////////////////////
		// PUBLIC PROCEDURES
		/////////////////////////////////////////////////////////////////////////////////////////////////////

		/// <summary>
		/// Gets or sets whether to change the selection when a match is found in a <see cref="EditorViewFindReplace.Find"/> operation.
		/// </summary>
		/// <value>
		/// <c>true</c> if the selection should be changed when a match is found in a <see cref="EditorViewFindReplace.Find"/> operation; otherwise, <c>false</c>.
		/// The default value is <c>true</c>.
		/// </value>
		/// <remarks>
		/// This option is only valid for a simple <see cref="EditorViewFindReplace.Find"/> operation.
		/// </remarks>
		public bool ChangeSelection {
			get {
				return changeSelection;
			}
			set {
				changeSelection = value;
			}
		}

		/// <summary>
		/// Gets or sets the text to find.
		/// </summary>
		/// <value>The text to find.</value>
		public string FindText {
			get {
				return findText;
			}
			set {
				// Quit if the value is already set
				if (findText == value)
					return;

				// Set the new value
				findText = value;

				// Reset the regexes
				this.ResetRegexes();
			}
		}

		/// <summary>
		/// Gets or sets whether searches should be case sensitive.
		/// </summary>
		/// <value>
		/// <c>true</c> if searches should be case sensitive; otherwise, <c>false</c>.
		/// </value>
		public bool MatchCase {
			get {
				return matchCase;
			}
			set {
				// Quit if the value is already set
				if (matchCase == value)
					return;

				// Set the new value
				matchCase = value;
				
				// Reset the regexes
				this.ResetRegexes();
			}
		}

		/// <summary>
		/// Gets or sets whether searches should only match whole words.
		/// </summary>
		/// <value>
		/// <c>true</c> if searches should only match whole words; otherwise, <c>false</c>.
		/// </value>
		public bool MatchWholeWord {
			get {
				return matchWholeWord;
			}
			set {
				// Quit if the value is already set
				if (matchWholeWord == value)
					return;

				// Set the new value
				matchWholeWord = value;

				// Reset the regexes
				this.ResetRegexes();
			}
		}

		/// <summary>
		/// Gets whether the options have been modified since the last search.
		/// </summary>
		/// <value>
		/// <c>true</c> if the options have been modified since the last search; otherwise, <c>false</c>.
		/// </value>
		public bool Modified {
			get {
				return (findRegexCode == null);
			}
		}

		/// <summary>
		/// Gets or sets the text with which to replace.
		/// </summary>
		/// <value>The text with which to replace.</value>
		public string ReplaceText {
			get {
				return replaceText;
			}
			set {
				// Quit if the value is already set
				if (replaceText == value)
					return;

				// Set the new value
				replaceText = value;

				// Reset the regexes
				this.ResetRegexes();
			}
		}

		/// <summary>
		/// Gets or sets whether to search text that is hidden within a collapsed node.
		/// </summary>
		/// <value>
		/// <c>true</c> if hidden text should be searched; otherwise, <c>false</c>.  The default value is <c>true</c>.
		/// </value>
		public bool SearchHiddenText {
			get {
				return searchHiddenText;
			}
			set {
				searchHiddenText = value;
			}
		}

		/// <summary>
		/// Gets or sets whether to restrict results to those within the current selection for find/replace operations.
		/// </summary>
		/// <value>
		/// <c>true</c> if results should be restricted to those within the current selection for find/replace operations; otherwise, <c>false</c>.
		/// The default value is <c>false</c>.
		/// </value>
		/// <remarks>
		/// This option is only valid for find/replace operations from the <see cref="EditorView"/> find/replace object model.
		/// </remarks>
		public bool SearchInSelection {
			get {
				return searchInSelection;
			}
			set {
				searchInSelection = value;
			}
		}

		/// <summary>
		/// Gets or sets the search type.
		/// </summary>
		/// <value>
		/// A <see cref="FindReplaceSearchType"/> specifying the search type.
		/// </value>
		public FindReplaceSearchType SearchType {
			get {
				return searchType;
			}
			set {
				// Quit if the value is already set
				if (searchType == value)
					return;

				// Set the new value
				searchType = value;

				// Reset the regexes
				this.ResetRegexes();
			}
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

				// Reset the regexes
				this.ResetRegexes();
			}
		}

	}
}
