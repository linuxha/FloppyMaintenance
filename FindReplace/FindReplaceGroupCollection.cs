using System;
using System.Collections;
using ActiproSoftware.ComponentModel;

namespace ActiproSoftware.SyntaxEditor {

	/// <summary>
	/// Provides access to a collection of <see cref="ActiproSoftware.SyntaxEditor.FindReplaceGroup"/> objects.
	/// </summary>
	public class FindReplaceGroupCollection : ReadOnlyCollectionBase {

		/////////////////////////////////////////////////////////////////////////////////////////////////////
		// NON-PUBLIC PROCEDURES
		/////////////////////////////////////////////////////////////////////////////////////////////////////

		/// <summary>
		/// Adds a <see cref="FindReplaceGroup"/> to the collection.
		/// </summary>
		/// <param name="group">The <see cref="FindReplaceGroup"/> to add to the member list.</param>
		/// <returns>The index at which the <see cref="FindReplaceGroup"/> was added.</returns>
		internal int Add(FindReplaceGroup group) {
			return this.InnerList.Add(group);
		}

		/// <summary>
		/// Copies a range of elements from the <see cref="ArrayList"/> to a compatible one-dimensional <see cref="Array"/>, starting at the specified index of the target array.
		/// </summary>
		/// <param name="index">The zero-based index in the source ArrayList at which copying begins.</param>
		/// <param name="array">The one-dimensional <see cref="Array"/> that is the destination of the elements copied from <see cref="ArrayList"/>. The <see cref="Array"/> must have zero-based indexing.</param>
		/// <param name="arrayIndex">The zero-based index in array at which copying begins.</param>
		/// <param name="count">The number of elements to copy.</param>
		internal void CopyTo(int index, Array array, int arrayIndex, int count) {
			this.InnerList.CopyTo(index, array, arrayIndex, count);
		}

		/// <summary>
		/// Removes a range of elements from the collection.
		/// </summary>
		/// <param name="index">The index at which to start removing.</param>
		/// <param name="count">The number of elements to remove.</param>
		internal void RemoveRange(int index, int count) {
			this.InnerList.RemoveRange(index, count);
		}

		/// <summary>
		/// Sets the value at the specified index.
		/// </summary>
		/// <param name="index">The index at which to set.</param>
		/// <param name="value">The value to set.</param>
		internal void SetValue(int index, FindReplaceGroup value) {
			this.InnerList[index] = value;
		}

		/////////////////////////////////////////////////////////////////////////////////////////////////////
		// PUBLIC PROCEDURES
		/////////////////////////////////////////////////////////////////////////////////////////////////////

		/// <summary>
		/// Gets the <see cref="FindReplaceGroup"/> at the specified index. 
		/// <para>
		/// [C#] In C#, this property is the indexer for the <c>FindReplaceGroupCollection</c> class. 
		/// </para>
		/// </summary>
		/// <param name="index">The index of the <see cref="FindReplaceGroup"/> to return.</param>
		/// <value>
		/// The <see cref="FindReplaceGroup"/> at the specified index. 
		/// </value>
		public FindReplaceGroup this[int index] {
			get {
				return (FindReplaceGroup)this.InnerList[index];
			}
		}

		/// <summary>
		/// Gets the <see cref="FindReplaceGroup"/> with the specified name. 
		/// <para>
		/// [C#] In C#, this property is the indexer for the <c>FindReplaceGroupCollection</c> class. 
		/// </para>
		/// </summary>
		/// <param name="name">The name of the <see cref="FindReplaceGroup"/> to return.</param>
		/// <value>
		/// The <see cref="FindReplaceGroup"/> with the specified name. 
		/// </value>
		public FindReplaceGroup this[string name] {
			get {
				foreach (FindReplaceGroup group in this.InnerList) {
					if (group.Name == name)
						return group;
				}
				return null;
			}
		}


	}
}
