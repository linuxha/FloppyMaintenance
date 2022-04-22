using System;
using System.Collections;

namespace ActiproSoftware.SyntaxEditor {

	/// <summary>
	/// Represents the results of a find and replace operation.
	/// </summary>
	public class FindReplaceResultSet : ReadOnlyCollectionBase {

		private bool		pastDocumentEnd				= false;
		private bool		pastSearchStartOffset		= false;
		private bool		replaceOccurred				= false;

		/////////////////////////////////////////////////////////////////////////////////////////////////////
		// OBJECT
		/////////////////////////////////////////////////////////////////////////////////////////////////////

		/// <summary>
		/// Initializes a new instance of the <c>FindReplaceResultSet</c> class.
		/// </summary>
		public FindReplaceResultSet() {
		}

		/// <summary>
		/// Initializes a new instance of the <c>FindReplaceResultSet</c> class.
		/// </summary>
		/// <param name="pastSearchStartOffset">Whether the search went past the start search offset.</param>
		public FindReplaceResultSet(bool pastSearchStartOffset) {
			// Initialize parameters
			this.pastSearchStartOffset = pastSearchStartOffset;
		}

		/////////////////////////////////////////////////////////////////////////////////////////////////////
		// NON-PUBLIC PROCEDURES
		/////////////////////////////////////////////////////////////////////////////////////////////////////

		/// <summary>
		/// Adds the specified item to the collection. 
		/// </summary>
		/// <param name="value">
		/// The <see cref="FindReplaceResult"/> to add to the collection. 
		/// </param>
		/// <returns>
		/// The position into which the new item was inserted.
		/// </returns>
		internal int Add(FindReplaceResult value) {
			return this.InnerList.Add(value);
		}

		/// <summary>
		/// Sets the <see cref="PastDocumentEnd"/> property.
		/// </summary>
		/// <param name="value">The value to set.</param>
		internal void SetPastDocumentEnd(bool value) {
			pastDocumentEnd = value;
		}

		/// <summary>
		/// Sets the <see cref="PastSearchStartOffset"/> property.
		/// </summary>
		/// <param name="value">The value to set.</param>
		internal void SetPastSearchStartOffset(bool value) {
			pastSearchStartOffset = value;
		}

		/// <summary>
		/// Sets the <see cref="ReplaceOccurred"/> property.
		/// </summary>
		/// <param name="value">The value to set.</param>
		internal void SetReplaceOccurred(bool value) {
			replaceOccurred = value;
		}

		/////////////////////////////////////////////////////////////////////////////////////////////////////
		// PUBLIC PROCEDURES
		/////////////////////////////////////////////////////////////////////////////////////////////////////

		/// <summary>
		/// Gets whether the search went past the end of the document and wrapped.
		/// </summary>
		/// <value>
		/// <c>true</c> if the search went past the end of the document and wrapped; otherwise, <c>false</c>.
		/// </value>
		public bool PastDocumentEnd {
			get {
				return pastDocumentEnd;
			}
		}

		/// <summary>
		/// Gets whether the search went past the search start offset.
		/// </summary>
		/// <value>
		/// <c>true</c> if the search went past the search start offset; otherwise, <c>false</c>.
		/// </value>
		public bool PastSearchStartOffset {
			get {
				return pastSearchStartOffset;
			}
		}

		/// <summary>
		/// Gets whether a replace occurred.
		/// </summary>
		/// <value>
		/// <c>true</c> if a replace occurred; otherwise, <c>false</c>.
		/// </value>
		public bool ReplaceOccurred {
			get {
				return replaceOccurred;
			}
		}

		/// <summary>
		/// Gets the <see cref="FindReplaceResult"/> at the specified index. 
		/// <para>
		/// [C#] In C#, this property is the indexer for the <c>FindReplaceResultSet</c> class. 
		/// </para>
		/// </summary>
		/// <param name="index">The index of the <see cref="FindReplaceResult"/> to return.</param>
		/// <value>
		/// The <see cref="FindReplaceResult"/> at the specified index. 
		/// </value>
		public FindReplaceResult this[int index] {
			get {
				return (FindReplaceResult)this.InnerList[index];
			}
		}
	}
}
