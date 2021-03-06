﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace nsGUI
{
    /// <summary>
    /// Form to add sentence terminator.
    /// </summary>
    public partial class FormAddSentenceTerminator : Form
    {
        #region Variables

        private const String _memberName = "sentenceTerminator";
        private String _selectedSentenceTerminator;

        #endregion Variables

        #region Constructors

        public FormAddSentenceTerminator()
        {
            InitializeComponent();
            this.Icon = Properties.Resources.AppIcon;

            DataTable dt = new DataTable();
            dt.Columns.Add(new DataColumn(_memberName));

            DataRow newRow = dt.NewRow();
            newRow[_memberName] = ".";
            dt.Rows.Add(newRow);

            newRow = dt.NewRow();
            newRow[_memberName] = "!";
            dt.Rows.Add(newRow);

            newRow = dt.NewRow();
            newRow[_memberName] = "?";
            dt.Rows.Add(newRow);

            this._comboBoxSentenceTerminator.DataSource = dt;
            this._comboBoxSentenceTerminator.DisplayMember = _memberName;
            this._comboBoxSentenceTerminator.ValueMember = _memberName;
        }

        #endregion Constuctors

        #region Properties

        /// <summary>
        /// Selected sentence terminator.
        /// </summary>
        public String SelectedSentenceTerminator
        {
            get { return _selectedSentenceTerminator; }
        }

        #endregion Properties

        #region Event Handlers

        /// <summary>
        /// Event handler for event of clicking Accept button.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AcceptButton_Click(Object sender, EventArgs e)
        {
            this._selectedSentenceTerminator = this._comboBoxSentenceTerminator.SelectedValue.ToString();

            this.DialogResult = DialogResult.OK;
        }

        /// <summary>
        /// Cancle button click event handler.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CancelButton_Click(Object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
        }

        #endregion Event Handlers

        #region Methods

        #endregion Methods
    }
}
