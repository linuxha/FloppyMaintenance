using System;
using System.Text;

namespace ActiproSoftware.SyntaxEditor {

	/// <summary>
	/// Represents a single result from a find and replace operation.
	/// </summary>
	public class FindReplaceResult : ITextRange {

		private FindReplaceGroupCollection	groups;
		private TextRange					textRange;

		/////////////////////////////////////////////////////////////////////////////////////////////////////
		// OBJECT
		/////////////////////////////////////////////////////////////////////////////////////////////////////

		/// <summary>
		/// Initializes a new instance of the <c>FindReplaceResult</c> class.
		/// </summary>
		/// <param name="offset">The offset at which the match was found.</param>
		/// <param name="length">The length of the match.</param>
		/// <param name="groups">The collection of captured groups.</param>
		public FindReplaceResult(int offset, int length, FindReplaceGroupCollection groups) : this(new TextRange(offset, offset + length), groups) {}
		
		/// <summary>
		/// Initializes a new instance of the <c>FindReplaceResult</c> class.
		/// </summary>
		/// <param name="textRange">The <see cref="TextRange"/> of the match.</param>
		/// <param name="groups">The collection of captured groups.</param>
		public FindReplaceResult(TextRange textRange, FindReplaceGroupCollection groups) {
			// Intialize parameters
			this.textRange	= textRange;
			this.groups		= groups;
		}
		
		/////////////////////////////////////////////////////////////////////////////////////////////////////
		// INTERFACE IMPLEMENTATION
		/////////////////////////////////////////////////////////////////////////////////////////////////////

		/// <summary>
		/// Gets or sets a <see cref="TextRange"/> that specifies the text range of the object.
		/// </summary>
		/// <value>A <see cref="TextRange"/> that specifies the text range of the object.</value>
		TextRange ITextRange.TextRange { 
			get {
				return textRange;
			}
			set {
				textRange = value;
			}
		}

		/////////////////////////////////////////////////////////////////////////////////////////////////////
		// PUBLIC PROCEDURES
		/////////////////////////////////////////////////////////////////////////////////////////////////////
		
		/// <summary>
		/// Gets the end offset at which the match was found.
		/// </summary>
		/// <value>The end offset at which the match was found.</value>
		public int EndOffset {
			get {
				return textRange.EndOffset;
			}
		}
	
		/// <summary>
		/// Returns a preview of the replacement text by using the specified find/replace options.
		/// </summary>
		/// <param name="options">The <see cref="FindReplaceOptions"/> to use.</param>
		/// <returns>A preview of the replacement text by using the specified find/replace options.</returns>
		public string GetPreviewReplaceText(FindReplaceOptions options) {
			return DocumentFindReplace.GetReplaceText(options, groups);
		}

		/// <summary>
		/// Gets the collection of captured groups.
		/// </summary>
		/// <value>The collection of captured groups.</value>
		public FindReplaceGroupCollection Groups {
			get {
				return groups;
			}
		}
	
		/// <summary>
		/// Gets the length of the match.
		/// </summary>
		/// <value>The length of the match.</value>
		public int Length {
			get {
				return textRange.Length;
			}
		}

		/// <summary>
		/// Gets the start offset at which the match was found.
		/// </summary>
		/// <value>The start offset at which the match was found.</value>
		public int StartOffset {
			get {
				return textRange.StartOffset;
			}
		}
	
		/// <summary>
		/// Gets the text that was matched.
		/// </summary>
		/// <value>The text that was matched.</value>
		public string Text {
			get {
				FindReplaceGroup group = groups["&"];
				if (group != null)
					return group.Text;
				else
					return null;
			}
		}

		/// <summary>
		/// Gets a <see cref="TextRange"/> that contains the text that was found.
		/// </summary>
		/// <value>A <see cref="TextRange"/> that contains the text that was found.</value>
		public TextRange TextRange {
			get {
				return textRange;
			}
		}

	}
}
