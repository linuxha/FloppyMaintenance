using System;
using System.Collections;
using System.Diagnostics;
using ActiproSoftware.Drawing;

namespace ActiproSoftware.SyntaxEditor {

	/// <summary>
	/// Represents a group captured by a regular expression match.
	/// </summary>
	public class FindReplaceGroup {

		private int		length;
		private string	name;
		private int		offset;
		private string	text;

		/////////////////////////////////////////////////////////////////////////////////////////////////////
		// OBJECT
		/////////////////////////////////////////////////////////////////////////////////////////////////////

		/// <summary>
		/// Initializes a new instance of the <c>FindReplaceGroup</c> class. 
		/// </summary>
		/// <param name="name">The name of the group.</param>
		/// <param name="offset">The offset of the group.</param>
		/// <param name="length">The length of the group.</param>
		/// <param name="text">The text of the group.</param>
		internal FindReplaceGroup(string name, int offset, int length, string text) {
			this.name = name;
			this.offset = offset;
			this.length = length;
			this.text = text;
		}

		/////////////////////////////////////////////////////////////////////////////////////////////////////
		// PUBLIC PROCEDURES
		/////////////////////////////////////////////////////////////////////////////////////////////////////
		
		/// <summary>
		/// Gets the offset length of the group.
		/// </summary>
		/// <value>The offset length of the group.</value>
		public int Length {
			get {
				return length;
			}
		}

		/// <summary>
		/// Gets name of the group.
		/// </summary>
		/// <value>The name of the group.</value>
		public string Name {
			get {
				return name;
			}
		}

		/// <summary>
		/// Gets the offset of the group.
		/// </summary>
		/// <value>The offset of the group.</value>
		public int Offset {
			get {
				return offset;
			}
		}

		/// <summary>
		/// Gets the text that was in the group.
		/// </summary>
		/// <value>The text that was in the group.</value>
		public string Text {
			get {
				return text;
			}
		}

	}

}
