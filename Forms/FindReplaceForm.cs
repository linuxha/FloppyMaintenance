using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using ActiproSoftware.Products.SyntaxEditor;
using ActiproSoftware.SyntaxEditor;
using ActiproSoftware.WinUICore;

namespace FloppyMaintenance
{

	/// <summary>
	/// Represents a built-in Find and Replace form.
	/// </summary>
	public class FindReplaceForm : System.Windows.Forms.Form {

		private FindReplaceOptions	options;

		private ActiproSoftware.SyntaxEditor.SyntaxEditor		syntaxEditor;

		//

		private System.Windows.Forms.TextBox findTextBox;
		private System.Windows.Forms.Button findButton;
		private System.Windows.Forms.Label findWhatLabel;
		private System.Windows.Forms.CheckBox searchTypeCheckBox;
		private System.Windows.Forms.ComboBox searchTypeDropDownList;
		private System.Windows.Forms.CheckBox matchCaseCheckBox;
		private System.Windows.Forms.CheckBox matchWholeWordCheckBox;
		private System.Windows.Forms.CheckBox searchUpCheckBox;
		private System.Windows.Forms.Button replaceButton;
		private System.Windows.Forms.Button replaceAllButton;
		private System.Windows.Forms.Button markAllButton;
		private System.Windows.Forms.Button closeButton;
		private System.Windows.Forms.Label replaceWithLabel;
		private System.Windows.Forms.TextBox replaceTextBox;
		private System.Windows.Forms.CheckBox markWithCheckBox;
		private System.Windows.Forms.CheckBox searchInSelectionCheckBox;
		private System.Windows.Forms.CheckBox searchHiddenTextCheckBox;
		private System.Windows.Forms.Button findInsertButton;
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		/////////////////////////////////////////////////////////////////////////////////////////////////////
		// EVENTS
		/////////////////////////////////////////////////////////////////////////////////////////////////////

		/// <summary>
		/// Occurs when there is a status change.
		/// </summary>
		/// <eventdata>
		/// The event handler receives an argument of type <c>FindReplaceStatusChangeEventArgs</c> containing data related to this event.
		/// </eventdata>
		[
			Category("Action"),
			Description("Occurs when there is a status change.")
		]
		public event FindReplaceStatusChangeEventHandler StatusChanged;
		
		/////////////////////////////////////////////////////////////////////////////////////////////////////
		// OBJECT
		/////////////////////////////////////////////////////////////////////////////////////////////////////

		/// <summary>
		/// Initializes a new instance of the <c>FindReplaceForm</c> class.
		/// </summary>
		/// <param name="syntaxEditor">The <see cref="SyntaxEditor"/> for which to display the form.</param>
		/// <param name="options">The <see cref="FindReplaceOptions"/> to use.</param>
		public FindReplaceForm(ActiproSoftware.SyntaxEditor.SyntaxEditor syntaxEditor, FindReplaceOptions options) {
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();

			// Ensure there are options
			if (options == null)
				throw new ArgumentNullException("options");

			// Initalize parameters
			this.syntaxEditor	= syntaxEditor;
			this.options		= options;

			// Load text from resources
			this.Text = SR.GetString("FindReplaceForm_Text");
			closeButton.Text = SR.GetString("General_CloseButton_Text");
			findButton.Text = SR.GetString("FindReplaceForm_FindButton_Text");
			findWhatLabel.Text = SR.GetString("FindReplaceForm_FindWhatLabel_Text");
			markAllButton.Text = SR.GetString("FindReplaceForm_MarkAllButton_Text");
			markWithCheckBox.Text = SR.GetString("FindReplaceForm_MarkWithCheckBox_Text");
			matchCaseCheckBox.Text = SR.GetString("FindReplaceForm_MatchCaseCheckBox_Text");
			matchWholeWordCheckBox.Text = SR.GetString("FindReplaceForm_MatchWholeWordCheckBox_Text");
			replaceButton.Text = SR.GetString("FindReplaceForm_ReplaceButton_Text");
			replaceAllButton.Text = SR.GetString("FindReplaceForm_ReplaceAllButton_Text");
			replaceWithLabel.Text = SR.GetString("FindReplaceForm_ReplaceWithLabel_Text");
			searchHiddenTextCheckBox.Text = SR.GetString("FindReplaceForm_SearchHiddenTextCheckBox_Text");
			searchInSelectionCheckBox.Text = SR.GetString("FindReplaceForm_SearchInSelectionCheckBox_Text");
			searchTypeCheckBox.Text = SR.GetString("FindReplaceForm_SearchTypeCheckBox_Text");
			this.searchTypeDropDownList.Items.AddRange(new object[] {
				SR.GetString("FindReplaceForm_SearchTypeDropDownList_Item_RegularExpressions"),
				SR.GetString("FindReplaceForm_SearchTypeDropDownList_Item_Wildcards")
				});
			searchUpCheckBox.Text = SR.GetString("FindReplaceForm_SearchUpCheckBox_Text");

	        // Select the first search type
		    searchTypeDropDownList.SelectedIndex = 0;

			// Update options
			this.UpdateUIFromFindReplaceOptions();
		}

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose( bool disposing )
		{
			if( disposing )
			{
				if(components != null)
				{
					components.Dispose();
				}
			}
			base.Dispose( disposing );
		}

		#region Windows Form Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
            this.findTextBox = new System.Windows.Forms.TextBox();
            this.findButton = new System.Windows.Forms.Button();
            this.findWhatLabel = new System.Windows.Forms.Label();
            this.searchTypeCheckBox = new System.Windows.Forms.CheckBox();
            this.searchTypeDropDownList = new System.Windows.Forms.ComboBox();
            this.matchCaseCheckBox = new System.Windows.Forms.CheckBox();
            this.matchWholeWordCheckBox = new System.Windows.Forms.CheckBox();
            this.searchUpCheckBox = new System.Windows.Forms.CheckBox();
            this.replaceButton = new System.Windows.Forms.Button();
            this.replaceAllButton = new System.Windows.Forms.Button();
            this.markAllButton = new System.Windows.Forms.Button();
            this.closeButton = new System.Windows.Forms.Button();
            this.replaceWithLabel = new System.Windows.Forms.Label();
            this.replaceTextBox = new System.Windows.Forms.TextBox();
            this.markWithCheckBox = new System.Windows.Forms.CheckBox();
            this.searchInSelectionCheckBox = new System.Windows.Forms.CheckBox();
            this.searchHiddenTextCheckBox = new System.Windows.Forms.CheckBox();
            this.findInsertButton = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // findTextBox
            // 
            this.findTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.findTextBox.Location = new System.Drawing.Point(101, 8);
            this.findTextBox.Name = "findTextBox";
            this.findTextBox.Size = new System.Drawing.Size(243, 20);
            this.findTextBox.TabIndex = 1;
            this.findTextBox.TextChanged += new System.EventHandler(this.findTextBox_TextChanged);
            // 
            // findButton
            // 
            this.findButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.findButton.Enabled = false;
            this.findButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.findButton.Location = new System.Drawing.Point(374, 8);
            this.findButton.Name = "findButton";
            this.findButton.Size = new System.Drawing.Size(75, 23);
            this.findButton.TabIndex = 13;
            this.findButton.Text = "&Find Next";
            this.findButton.Click += new System.EventHandler(this.findButton_Click);
            // 
            // findWhatLabel
            // 
            this.findWhatLabel.AutoSize = true;
            this.findWhatLabel.Location = new System.Drawing.Point(8, 10);
            this.findWhatLabel.Name = "findWhatLabel";
            this.findWhatLabel.Size = new System.Drawing.Size(56, 13);
            this.findWhatLabel.TabIndex = 0;
            this.findWhatLabel.Text = "Find what:";
            // 
            // searchTypeCheckBox
            // 
            this.searchTypeCheckBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.searchTypeCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.searchTypeCheckBox.Location = new System.Drawing.Point(11, 126);
            this.searchTypeCheckBox.Name = "searchTypeCheckBox";
            this.searchTypeCheckBox.Size = new System.Drawing.Size(58, 24);
            this.searchTypeCheckBox.TabIndex = 8;
            this.searchTypeCheckBox.Text = "Use";
            this.searchTypeCheckBox.CheckedChanged += new System.EventHandler(this.searchTypeCheckBox_CheckedChanged);
            // 
            // searchTypeDropDownList
            // 
            this.searchTypeDropDownList.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.searchTypeDropDownList.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.searchTypeDropDownList.Enabled = false;
            this.searchTypeDropDownList.Location = new System.Drawing.Point(58, 127);
            this.searchTypeDropDownList.Name = "searchTypeDropDownList";
            this.searchTypeDropDownList.Size = new System.Drawing.Size(267, 21);
            this.searchTypeDropDownList.TabIndex = 9;
            // 
            // matchCaseCheckBox
            // 
            this.matchCaseCheckBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.matchCaseCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.matchCaseCheckBox.Location = new System.Drawing.Point(11, 60);
            this.matchCaseCheckBox.Name = "matchCaseCheckBox";
            this.matchCaseCheckBox.Size = new System.Drawing.Size(260, 24);
            this.matchCaseCheckBox.TabIndex = 5;
            this.matchCaseCheckBox.Text = "Match case";
            // 
            // matchWholeWordCheckBox
            // 
            this.matchWholeWordCheckBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.matchWholeWordCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.matchWholeWordCheckBox.Location = new System.Drawing.Point(11, 81);
            this.matchWholeWordCheckBox.Name = "matchWholeWordCheckBox";
            this.matchWholeWordCheckBox.Size = new System.Drawing.Size(260, 24);
            this.matchWholeWordCheckBox.TabIndex = 6;
            this.matchWholeWordCheckBox.Text = "Match whole word";
            // 
            // searchUpCheckBox
            // 
            this.searchUpCheckBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.searchUpCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.searchUpCheckBox.Location = new System.Drawing.Point(11, 102);
            this.searchUpCheckBox.Name = "searchUpCheckBox";
            this.searchUpCheckBox.Size = new System.Drawing.Size(260, 24);
            this.searchUpCheckBox.TabIndex = 7;
            this.searchUpCheckBox.Text = "Search up";
            // 
            // replaceButton
            // 
            this.replaceButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.replaceButton.Enabled = false;
            this.replaceButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.replaceButton.Location = new System.Drawing.Point(374, 34);
            this.replaceButton.Name = "replaceButton";
            this.replaceButton.Size = new System.Drawing.Size(75, 23);
            this.replaceButton.TabIndex = 14;
            this.replaceButton.Text = "&Replace";
            this.replaceButton.Click += new System.EventHandler(this.replaceButton_Click);
            // 
            // replaceAllButton
            // 
            this.replaceAllButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.replaceAllButton.Enabled = false;
            this.replaceAllButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.replaceAllButton.Location = new System.Drawing.Point(374, 61);
            this.replaceAllButton.Name = "replaceAllButton";
            this.replaceAllButton.Size = new System.Drawing.Size(75, 23);
            this.replaceAllButton.TabIndex = 15;
            this.replaceAllButton.Text = "Replace &All";
            this.replaceAllButton.Click += new System.EventHandler(this.replaceAllButton_Click);
            // 
            // markAllButton
            // 
            this.markAllButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.markAllButton.Enabled = false;
            this.markAllButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.markAllButton.Location = new System.Drawing.Point(374, 88);
            this.markAllButton.Name = "markAllButton";
            this.markAllButton.Size = new System.Drawing.Size(75, 23);
            this.markAllButton.TabIndex = 16;
            this.markAllButton.Text = "&Mark All";
            this.markAllButton.Click += new System.EventHandler(this.markAllButton_Click);
            // 
            // closeButton
            // 
            this.closeButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.closeButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.closeButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.closeButton.Location = new System.Drawing.Point(374, 115);
            this.closeButton.Name = "closeButton";
            this.closeButton.Size = new System.Drawing.Size(75, 23);
            this.closeButton.TabIndex = 17;
            this.closeButton.Text = "Close";
            this.closeButton.Click += new System.EventHandler(this.closeButton_Click);
            // 
            // replaceWithLabel
            // 
            this.replaceWithLabel.AutoSize = true;
            this.replaceWithLabel.Location = new System.Drawing.Point(8, 36);
            this.replaceWithLabel.Name = "replaceWithLabel";
            this.replaceWithLabel.Size = new System.Drawing.Size(72, 13);
            this.replaceWithLabel.TabIndex = 3;
            this.replaceWithLabel.Text = "Replace with:";
            // 
            // replaceTextBox
            // 
            this.replaceTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.replaceTextBox.Location = new System.Drawing.Point(101, 33);
            this.replaceTextBox.Name = "replaceTextBox";
            this.replaceTextBox.Size = new System.Drawing.Size(265, 20);
            this.replaceTextBox.TabIndex = 4;
            // 
            // markWithCheckBox
            // 
            this.markWithCheckBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.markWithCheckBox.Checked = true;
            this.markWithCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.markWithCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.markWithCheckBox.Location = new System.Drawing.Point(203, 102);
            this.markWithCheckBox.Name = "markWithCheckBox";
            this.markWithCheckBox.Size = new System.Drawing.Size(156, 24);
            this.markWithCheckBox.TabIndex = 12;
            this.markWithCheckBox.Text = "Mark with bookmarks";
            this.markWithCheckBox.Visible = false;
            // 
            // searchInSelectionCheckBox
            // 
            this.searchInSelectionCheckBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.searchInSelectionCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.searchInSelectionCheckBox.Location = new System.Drawing.Point(203, 81);
            this.searchInSelectionCheckBox.Name = "searchInSelectionCheckBox";
            this.searchInSelectionCheckBox.Size = new System.Drawing.Size(156, 24);
            this.searchInSelectionCheckBox.TabIndex = 11;
            this.searchInSelectionCheckBox.Text = "Search in selection";
            // 
            // searchHiddenTextCheckBox
            // 
            this.searchHiddenTextCheckBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.searchHiddenTextCheckBox.Checked = true;
            this.searchHiddenTextCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.searchHiddenTextCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.searchHiddenTextCheckBox.Location = new System.Drawing.Point(203, 60);
            this.searchHiddenTextCheckBox.Name = "searchHiddenTextCheckBox";
            this.searchHiddenTextCheckBox.Size = new System.Drawing.Size(156, 24);
            this.searchHiddenTextCheckBox.TabIndex = 10;
            this.searchHiddenTextCheckBox.Text = "Search hidden text";
            // 
            // findInsertButton
            // 
            this.findInsertButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.findInsertButton.Enabled = false;
            this.findInsertButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.findInsertButton.Location = new System.Drawing.Point(347, 7);
            this.findInsertButton.Name = "findInsertButton";
            this.findInsertButton.Size = new System.Drawing.Size(20, 22);
            this.findInsertButton.TabIndex = 2;
            this.findInsertButton.Text = ">";
            this.findInsertButton.Click += new System.EventHandler(this.findInsertButton_Click);
            // 
            // FindReplaceForm
            // 
            this.AcceptButton = this.findButton;
            this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
            this.CancelButton = this.closeButton;
            this.ClientSize = new System.Drawing.Size(457, 156);
            this.Controls.Add(this.findInsertButton);
            this.Controls.Add(this.searchHiddenTextCheckBox);
            this.Controls.Add(this.searchInSelectionCheckBox);
            this.Controls.Add(this.markWithCheckBox);
            this.Controls.Add(this.replaceWithLabel);
            this.Controls.Add(this.findWhatLabel);
            this.Controls.Add(this.replaceTextBox);
            this.Controls.Add(this.closeButton);
            this.Controls.Add(this.markAllButton);
            this.Controls.Add(this.replaceAllButton);
            this.Controls.Add(this.replaceButton);
            this.Controls.Add(this.searchUpCheckBox);
            this.Controls.Add(this.matchWholeWordCheckBox);
            this.Controls.Add(this.matchCaseCheckBox);
            this.Controls.Add(this.searchTypeDropDownList);
            this.Controls.Add(this.searchTypeCheckBox);
            this.Controls.Add(this.findButton);
            this.Controls.Add(this.findTextBox);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FindReplaceForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Find/Replace";
            this.ResumeLayout(false);
            this.PerformLayout();

		}
		#endregion
		
		/////////////////////////////////////////////////////////////////////////////////////////////////////
		// EVENT HANDLERS
		/////////////////////////////////////////////////////////////////////////////////////////////////////

		/// <summary>
		/// Occurs when the button is clicked.
		/// </summary>
		/// <param name="sender">Sender of the event.</param>
		/// <param name="e">Event arguments.</param>
		private void closeButton_Click(object sender, System.EventArgs e) {
			this.Close();
		}

		/// <summary>
		/// Occurs when the button is clicked.
		/// </summary>
		/// <param name="sender">Sender of the event.</param>
		/// <param name="e">Event arguments.</param>
		private void findButton_Click(object sender, System.EventArgs e) {
			// Update find/replace options
			this.UpdateFindReplaceOptionsFromUI();

			if (syntaxEditor == null)
				return;

			// Raise an event
			this.OnStatusChanged(new FindReplaceStatusChangeEventArgs(FindReplaceStatusChangeType.Find, options));

			// Perform a find operation
			FindReplaceResultSet resultSet;
			try {
				resultSet = syntaxEditor.SelectedView.FindReplace.Find(options);
			}
			catch (Exception ex) {
				MessageBox.Show(this, SR.GetString("FindReplaceForm_Error_Message") + "\r\n" + ex.Message, Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Error);
				return;
			}

			// Set the status
			if (resultSet.PastDocumentEnd) {
				// Raise an event
				this.OnStatusChanged(new FindReplaceStatusChangeEventArgs(FindReplaceStatusChangeType.PastDocumentEnd, options));
			}

			// Find if the search went past the starting point
			if (resultSet.PastSearchStartOffset) {
				MessageBox.Show(this, SR.GetString("FindReplaceForm_PastSearchStartOffset_Message"), Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Information);
				return;
			}

			// If no matches were found...			
			if (resultSet.Count == 0)
				MessageBox.Show(this, SR.GetString("FindReplaceForm_NotFound_Message"), Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Information);
		}

		/// <summary>
		/// Occurs when the Text property is changed.
		/// </summary>
		/// <param name="sender">Sender of the event.</param>
		/// <param name="e">Event arguments.</param>
		private void findTextBox_TextChanged(object sender, System.EventArgs e) {
            findButton.Enabled = (findTextBox.Text.Length > 0);
            replaceButton.Enabled = findButton.Enabled;
            replaceAllButton.Enabled = (findTextBox.Text.Length > 0);
            markAllButton.Enabled = replaceAllButton.Enabled;
		}

		/// <summary>
		/// Occurs when the button is clicked.
		/// </summary>
		/// <param name="sender">Sender of the event.</param>
		/// <param name="e">Event arguments.</param>
		private void findInsertButton_Click(object sender, System.EventArgs e) {
            OwnerDrawContextMenu menu = new OwnerDrawContextMenu();
			if (searchTypeDropDownList.SelectedIndex == 0) {
				menu.MenuItems.Add(new OwnerDrawMenuItem(SR.GetString("FindReplaceForm_RegularExpression_SingleCharacter"), -1, new EventHandler(menuItem_Click), Shortcut.None, "."));
				menu.MenuItems.Add(new OwnerDrawMenuItem(SR.GetString("FindReplaceForm_RegularExpression_ZeroOrMore"), -1, new EventHandler(menuItem_Click), Shortcut.None, "*"));
				menu.MenuItems.Add(new OwnerDrawMenuItem(SR.GetString("FindReplaceForm_RegularExpression_OneOrMore"), -1, new EventHandler(menuItem_Click), Shortcut.None, "+"));
				menu.MenuItems.Add(new OwnerDrawMenuItem("-"));
				menu.MenuItems.Add(new OwnerDrawMenuItem(SR.GetString("FindReplaceForm_RegularExpression_BeginningOfLine"), -1, new EventHandler(menuItem_Click), Shortcut.None, "^"));
				menu.MenuItems.Add(new OwnerDrawMenuItem(SR.GetString("FindReplaceForm_RegularExpression_EndOfLine"), -1, new EventHandler(menuItem_Click), Shortcut.None, "$"));
				menu.MenuItems.Add(new OwnerDrawMenuItem(SR.GetString("FindReplaceForm_RegularExpression_WordBoundary"), -1, new EventHandler(menuItem_Click), Shortcut.None, "\\b"));
				menu.MenuItems.Add(new OwnerDrawMenuItem(SR.GetString("FindReplaceForm_RegularExpression_Whitespace"), -1, new EventHandler(menuItem_Click), Shortcut.None, "\\s"));
				menu.MenuItems.Add(new OwnerDrawMenuItem(SR.GetString("FindReplaceForm_RegularExpression_LineBreak"), -1, new EventHandler(menuItem_Click), Shortcut.None, "\\n"));
				menu.MenuItems.Add(new OwnerDrawMenuItem("-"));
				menu.MenuItems.Add(new OwnerDrawMenuItem(SR.GetString("FindReplaceForm_RegularExpression_OneCharacterInSet"), -1, new EventHandler(menuItem_Click), Shortcut.None, "[ ]"));
				menu.MenuItems.Add(new OwnerDrawMenuItem(SR.GetString("FindReplaceForm_RegularExpression_OneCharacterNotInSet"), -1, new EventHandler(menuItem_Click), Shortcut.None, "[^ ]"));
				menu.MenuItems.Add(new OwnerDrawMenuItem(SR.GetString("FindReplaceForm_RegularExpression_Or"), -1, new EventHandler(menuItem_Click), Shortcut.None, "|"));
				menu.MenuItems.Add(new OwnerDrawMenuItem(SR.GetString("FindReplaceForm_RegularExpression_CharacterEscape"), -1, new EventHandler(menuItem_Click), Shortcut.None, "\\"));
				menu.MenuItems.Add(new OwnerDrawMenuItem(SR.GetString("FindReplaceForm_RegularExpression_Group"), -1, new EventHandler(menuItem_Click), Shortcut.None, "( )"));
			}
			else {
				menu.MenuItems.Add(new OwnerDrawMenuItem(SR.GetString("FindReplaceForm_Wildcard_ZeroOrMore"), -1, new EventHandler(menuItem_Click), Shortcut.None, "*"));
				menu.MenuItems.Add(new OwnerDrawMenuItem(SR.GetString("FindReplaceForm_Wildcard_AnySingleCharacter"), -1, new EventHandler(menuItem_Click), Shortcut.None, "?"));
				menu.MenuItems.Add(new OwnerDrawMenuItem(SR.GetString("FindReplaceForm_Wildcard_AnySingleDigit"), -1, new EventHandler(menuItem_Click), Shortcut.None, "#"));
				menu.MenuItems.Add(new OwnerDrawMenuItem(SR.GetString("FindReplaceForm_Wildcard_OneCharacterInSet"), -1, new EventHandler(menuItem_Click), Shortcut.None, "[ ]"));
				menu.MenuItems.Add(new OwnerDrawMenuItem(SR.GetString("FindReplaceForm_Wildcard_OneCharacterNotInSet"), -1, new EventHandler(menuItem_Click), Shortcut.None, "[! ]"));
			}
			menu.Show(findInsertButton, new Point(findInsertButton.ClientRectangle.Right, findInsertButton.ClientRectangle.Top));
		}
		
		/// <summary>
		/// Occurs when the button is clicked.
		/// </summary>
		/// <param name="sender">Sender of the event.</param>
		/// <param name="e">Event arguments.</param>
		private void markAllButton_Click(object sender, System.EventArgs e) {
			// Update find/replace options
			this.UpdateFindReplaceOptionsFromUI();

			if (syntaxEditor == null)
				return;
		
			// Perform a mark all operation
			FindReplaceResultSet resultSet;
			try {
				if (markWithCheckBox.Checked)
					resultSet = syntaxEditor.SelectedView.FindReplace.MarkAll(options);
				else
					resultSet = syntaxEditor.SelectedView.FindReplace.MarkAll(options, typeof(GrammarErrorSpanIndicator));
			}
			catch (Exception ex) {
				MessageBox.Show(this, SR.GetString("FindReplaceForm_Error_Message") + "\r\n" + ex.Message, Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Error);
				return;
			}

			// If no matches were found
			if (resultSet.Count == 0)
				MessageBox.Show(this, SR.GetString("FindReplaceForm_NotFound_Message"), Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Information);
		}

		/// <summary>
		/// Occurs when the menu item is clicked.
		/// </summary>
		/// <param name="sender">Sender of the event.</param>
		/// <param name="e">Event arguments.</param>
		private void menuItem_Click(object sender, System.EventArgs e) {
			findTextBox.SelectedText = ((OwnerDrawMenuItem)sender).Tag.ToString();
		}

		/// <summary>
		/// Occurs when the button is clicked.
		/// </summary>
		/// <param name="sender">Sender of the event.</param>
		/// <param name="e">Event arguments.</param>
		private void replaceButton_Click(object sender, System.EventArgs e) {
			// Update find/replace options
			this.UpdateFindReplaceOptionsFromUI();

			if (syntaxEditor == null)
				return;

			// Raise an event
			this.OnStatusChanged(new FindReplaceStatusChangeEventArgs(FindReplaceStatusChangeType.Replace, options));

			// Perform a find operation
			FindReplaceResultSet resultSet;
			try {
				resultSet = syntaxEditor.SelectedView.FindReplace.Replace(options);
			}
			catch (Exception ex) {
				MessageBox.Show(this, SR.GetString("FindReplaceForm_Error_Message") + "\r\n" + ex.Message, Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Error);
				return;
			}

			// Set the status
			if (resultSet.PastDocumentEnd) {
				// Raise an event
				this.OnStatusChanged(new FindReplaceStatusChangeEventArgs(FindReplaceStatusChangeType.PastDocumentEnd, options));
			}

			// If no matches were found...			
			if (resultSet.Count == 0) {
				if ((resultSet.PastDocumentEnd) && (resultSet.ReplaceOccurred))
					MessageBox.Show(this, SR.GetString("FindReplaceForm_PastSearchStartOffset_Message"), Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Information);
				else
					MessageBox.Show(this, SR.GetString("FindReplaceForm_NotFound_Message"), Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Information);
			}
		}

		/// <summary>
		/// Occurs when the button is clicked.
		/// </summary>
		/// <param name="sender">Sender of the event.</param>
		/// <param name="e">Event arguments.</param>
		private void replaceAllButton_Click(object sender, System.EventArgs e) {
			// Update find/replace options
			this.UpdateFindReplaceOptionsFromUI();

			if (syntaxEditor == null)
				return;
		
			// Perform a replace all operation
			FindReplaceResultSet resultSet;
			try {
				resultSet = syntaxEditor.SelectedView.FindReplace.ReplaceAll(options);
			}
			catch (Exception ex) {
				MessageBox.Show(this, SR.GetString("FindReplaceForm_Error_Message") + "\r\n" + ex.Message, Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Error);
				return;
			}

			// If no matches were found...
			if (resultSet.Count == 0) {
				MessageBox.Show(this, SR.GetString("FindReplaceForm_NotFound_Message"), Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Information);
				return;
			}

			// Display the number of replacements
			MessageBox.Show(this, resultSet.Count + " " + SR.GetString("FindReplaceForm_OccurrencesReplaced_Message"), Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Information);
		}

		/// <summary>
		/// Occurs when the Checked property is changed.
		/// </summary>
		/// <param name="sender">Sender of the event.</param>
		/// <param name="e">Event arguments.</param>
		private void searchTypeCheckBox_CheckedChanged(object sender, System.EventArgs e) {
			findInsertButton.Enabled = searchTypeCheckBox.Checked;
			searchTypeDropDownList.Enabled = searchTypeCheckBox.Checked;
		}
		
		/////////////////////////////////////////////////////////////////////////////////////////////////////
		// NON-PUBLIC PROCEDURES
		/////////////////////////////////////////////////////////////////////////////////////////////////////
		
		/// <summary>
		/// Updates the find/replace options from the UI.
		/// </summary>
		private void UpdateFindReplaceOptionsFromUI() {
			options.FindText			= findTextBox.Text;
			options.ReplaceText			= replaceTextBox.Text;
			options.MatchCase			= matchCaseCheckBox.Checked;
			options.MatchWholeWord		= matchWholeWordCheckBox.Checked;
			options.SearchHiddenText	= searchHiddenTextCheckBox.Checked;
			options.SearchInSelection	= searchInSelectionCheckBox.Checked;
			options.SearchUp			= searchUpCheckBox.Checked;
			options.SearchType			= (!searchTypeCheckBox.Checked ? FindReplaceSearchType.Normal :
											(searchTypeDropDownList.SelectedIndex == 0 ? FindReplaceSearchType.RegularExpression : FindReplaceSearchType.Wildcard));
		}

		/// <summary>
		/// Updates the UI from the find/replace options.
		/// </summary>
		private void UpdateUIFromFindReplaceOptions() {
			findTextBox.Text						= options.FindText;
			replaceTextBox.Text						= options.ReplaceText;
			matchCaseCheckBox.Checked				= options.MatchCase;
			matchWholeWordCheckBox.Checked			= options.MatchWholeWord;
			searchUpCheckBox.Checked				= options.SearchUp;
			searchHiddenTextCheckBox.Checked		= options.SearchHiddenText;
			searchInSelectionCheckBox.Checked		= options.SearchInSelection;
			searchTypeCheckBox.Checked				= (options.SearchType != FindReplaceSearchType.Normal);
			searchTypeDropDownList.SelectedIndex	= (options.SearchType != FindReplaceSearchType.Wildcard ? 0 : 1); 
		}
		
		/////////////////////////////////////////////////////////////////////////////////////////////////////
		// PUBLIC PROCEDURES
		/////////////////////////////////////////////////////////////////////////////////////////////////////
		
		/// <summary>
		/// Occurs before the form is activated.
		/// </summary>
		/// <param name="e">An <c>EventArgs</c> that contains the event data.</param>
		protected override void OnActivated(EventArgs e) {
			// Call the base method
			base.OnActivated(e);

			// Select the find text
			findTextBox.SelectAll();
			findTextBox.Focus();
		}

		/// <summary>
		/// Occurs before the form is closed.
		/// </summary>
		/// <param name="e">A <c>CancelEventArgs</c> that contains the event data.</param>
		protected override void OnClosing(CancelEventArgs e) {
			// Raise an event
			this.OnStatusChanged(new FindReplaceStatusChangeEventArgs(FindReplaceStatusChangeType.Ready, options));

			// Call the base method
			base.OnClosing(e);

			// Cancel the close and hide the form instead (for reuse)
			e.Cancel = true;
			if (this.Owner != null)
				this.Owner.Activate();
			this.Hide();
		}
		
		/// <summary>
		/// Raises the <c>StatusChanged</c> event.
		/// </summary>
		/// <param name="e">A <c>FindReplaceStatusChangeEventArgs</c> that contains the event data.</param>
		protected virtual void OnStatusChanged(FindReplaceStatusChangeEventArgs e) {
			if (this.StatusChanged != null) 
				this.StatusChanged(this, e);
		}
		
		/// <summary>
		/// Occurs when the <c>Visible</c> property of the control is changed.
		/// </summary>
		/// <param name="e">An <c>EventArgs</c> that contains the event data.</param>
		protected override void OnVisibleChanged(EventArgs e) {
			// Call the base method
			base.OnVisibleChanged(e);

			if (this.Visible) {
				// Update options
				this.UpdateUIFromFindReplaceOptions();
			}
		}

		/// <summary>
		/// Gets or sets the <see cref="FindReplaceOptions"/> to use.
		/// </summary>
		/// <value>The <see cref="FindReplaceOptions"/> to use.</value>
		public FindReplaceOptions Options {
			get {
				return options;
			}
			set {
				if (value == null)
					throw new ArgumentNullException("Options");

				options = value;

				this.UpdateUIFromFindReplaceOptions();
			}
		}

		/// <summary>
		/// Gets or sets the <see cref="ActiproSoftware.SyntaxEditor.SyntaxEditor"/> that is currently the focus of the find/replace form.
		/// </summary>
		/// <value>The <see cref="ActiproSoftware.SyntaxEditor.SyntaxEditor"/> that is currently the focus of the find/replace form.</value>
		public ActiproSoftware.SyntaxEditor.SyntaxEditor SyntaxEditor {
			get {
				return syntaxEditor;
			}
			set {
				syntaxEditor = value;
			}
		}

	}
}
