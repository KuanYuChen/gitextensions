using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using GitCommands;

namespace GitUI
{
    public class FilterBranchHelper : IDisposable
    {
        private bool _applyingFilter;
        private ToolStripComboBox _NO_TRANSLATE_toolStripBranches;
        private ToolStripDropDownButton _NO_TRANSLATE_toolStripDropDownButton2;
        private RevisionGrid _NO_TRANSLATE_RevisionGrid;
        private ToolStripMenuItem localToolStripMenuItem;
        private ToolStripMenuItem tagsToolStripMenuItem;
        private ToolStripMenuItem remoteToolStripMenuItem;
        private GitModule Module => _NO_TRANSLATE_RevisionGrid.Module;

        public FilterBranchHelper()
        {
            this.localToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.tagsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.remoteToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            //
            // localToolStripMenuItem
            //
            this.localToolStripMenuItem.Checked = true;
            this.localToolStripMenuItem.CheckOnClick = true;
            this.localToolStripMenuItem.Name = "localToolStripMenuItem";
            this.localToolStripMenuItem.Text = "Local";
            //
            // tagsToolStripMenuItem
            //
            this.tagsToolStripMenuItem.CheckOnClick = true;
            this.tagsToolStripMenuItem.Name = "tagToolStripMenuItem";
            this.tagsToolStripMenuItem.Text = "Tag";
            //
            // remoteToolStripMenuItem
            //
            this.remoteToolStripMenuItem.CheckOnClick = true;
            this.remoteToolStripMenuItem.Name = "remoteToolStripMenuItem";
            this.remoteToolStripMenuItem.Size = new System.Drawing.Size(115, 22);
            this.remoteToolStripMenuItem.Text = "Remote";
        }

        public FilterBranchHelper(ToolStripComboBox toolStripBranches, ToolStripDropDownButton toolStripDropDownButton2, RevisionGrid revisionGrid)
            : this()
        {
            this._NO_TRANSLATE_toolStripBranches = toolStripBranches;
            this._NO_TRANSLATE_toolStripDropDownButton2 = toolStripDropDownButton2;
            this._NO_TRANSLATE_RevisionGrid = revisionGrid;

            this._NO_TRANSLATE_toolStripDropDownButton2.DropDownItems.AddRange(new ToolStripItem[] {
                this.localToolStripMenuItem,
                this.tagsToolStripMenuItem,
                this.remoteToolStripMenuItem });

            this._NO_TRANSLATE_toolStripBranches.DropDown += this.toolStripBranches_DropDown;
            this._NO_TRANSLATE_toolStripBranches.TextUpdate += this.toolStripBranches_TextUpdate;
            this._NO_TRANSLATE_toolStripBranches.Leave += this.toolStripBranches_Leave;
            this._NO_TRANSLATE_toolStripBranches.KeyUp += this.toolStripBranches_KeyUp;
        }

        public void InitToolStripBranchFilter()
        {
            bool local = localToolStripMenuItem.Checked;
            bool tag = tagsToolStripMenuItem.Checked;
            bool remote = remoteToolStripMenuItem.Checked;

            _NO_TRANSLATE_toolStripBranches.Items.Clear();

            if (Module.IsValidGitWorkingDir())
            {
                AsyncLoader.DoAsync(() => GetBranchAndTagRefs(local, tag, remote),
                    branches =>
                    {
                        foreach (var branch in branches)
                            _NO_TRANSLATE_toolStripBranches.Items.Add(branch);

                        var autoCompleteList = _NO_TRANSLATE_toolStripBranches.AutoCompleteCustomSource.Cast<string>();
                        if (!autoCompleteList.SequenceEqual(branches))
                        {
                            _NO_TRANSLATE_toolStripBranches.AutoCompleteCustomSource.Clear();
                            _NO_TRANSLATE_toolStripBranches.AutoCompleteCustomSource.AddRange(branches.ToArray());
                        }
                    });
            }

            _NO_TRANSLATE_toolStripBranches.Enabled = Module.IsValidGitWorkingDir();
        }

        private List<string> GetBranchHeads(bool local, bool remote)
        {
            var list = new List<string>();
            if (local && remote)
            {
                var branches = Module.GetRefs(true, true);
                list.AddRange(branches.Where(branch => !branch.IsTag).Select(branch => branch.Name));
            }
            else if (local)
            {
                var branches = Module.GetRefs(false);
                list.AddRange(branches.Select(branch => branch.Name));
            }
            else if (remote)
            {
                var branches = Module.GetRefs(true, true);
                list.AddRange(branches.Where(branch => branch.IsRemote && !branch.IsTag).Select(branch => branch.Name));
            }
            return list;
        }

        private IEnumerable<string> GetTagsRefs()
        {
            return Module.GetRefs(true, false).Select(tag => tag.Name);
        }

        private List<string> GetBranchAndTagRefs(bool local, bool tag, bool remote)
        {
            var list = GetBranchHeads(local, remote);
            if (tag)
                list.AddRange(GetTagsRefs());
            return list;
        }

        private void toolStripBranches_TextUpdate(object sender, EventArgs e)
        {
            UpdateBranchFilterItems();
        }

        private void toolStripBranches_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyValue == (char)Keys.Enter)
            {
                ApplyBranchFilter(true);
            }
        }

        private void toolStripBranches_DropDown(object sender, EventArgs e)
        {
            UpdateBranchFilterItems();
        }

        private void ApplyBranchFilter(bool refresh)
        {
            if (_applyingFilter)
            {
                return;
            }
            _applyingFilter = true;
            try
            {
                string filter = _NO_TRANSLATE_toolStripBranches.Items.Count > 0 ? _NO_TRANSLATE_toolStripBranches.Text : string.Empty;
                bool success = _NO_TRANSLATE_RevisionGrid.SetAndApplyBranchFilter(filter);
                if (success && refresh)
                {
                    _NO_TRANSLATE_RevisionGrid.ForceRefreshRevisions();
                }
            }
            finally
            {
                _applyingFilter = false;
            }
        }

        private void UpdateBranchFilterItems()
        {
            string filter = _NO_TRANSLATE_toolStripBranches.Items.Count > 0 ? _NO_TRANSLATE_toolStripBranches.Text : string.Empty;
            var branches = GetBranchAndTagRefs(localToolStripMenuItem.Checked, tagsToolStripMenuItem.Checked, remoteToolStripMenuItem.Checked);
            var matches = branches.Where(branch => branch.IndexOf(filter, StringComparison.InvariantCultureIgnoreCase) >= 0).ToArray();

            var index = _NO_TRANSLATE_toolStripBranches.SelectionStart;
            _NO_TRANSLATE_toolStripBranches.Items.Clear();
            _NO_TRANSLATE_toolStripBranches.Items.AddRange(matches);
            _NO_TRANSLATE_toolStripBranches.SelectionStart = index;
        }

        public void SetBranchFilter(string filter, bool refresh)
        {
            _NO_TRANSLATE_toolStripBranches.Text = filter;
            ApplyBranchFilter(refresh);
        }

        private void toolStripBranches_Leave(object sender, EventArgs e)
        {
            ApplyBranchFilter(true);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                localToolStripMenuItem.Dispose();
                remoteToolStripMenuItem.Dispose();
                tagsToolStripMenuItem.Dispose();
            }
        }
    }
}
