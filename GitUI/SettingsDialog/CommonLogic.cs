﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GitCommands;
using System.ComponentModel;

namespace GitUI.SettingsDialog
{
    class CommonLogic
    {
        [Browsable(false)]
        public GitModule Module { get { return null; /* TODO: see GitModuleForm */ } }

        public string GetMergeTool()
        {
            return Module.GetGlobalSetting("merge.tool");
        }

        public bool IsMergeTool(string toolName)
        {
            return GetMergeTool().Equals(toolName,
                StringComparison.CurrentCultureIgnoreCase);
        }
    }
}
