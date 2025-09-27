﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DebugMod.InfoPanels
{
    /// <summary>
    /// Creates an input panel which is a single column of info.
    /// </summary>
    public class SimpleInfoPanel : CustomInfoPanel
    {
        private float sep;
        private float y = -10f;
        public SimpleInfoPanel(string Name, float sep) : base(Name, false) { this.sep = sep; }

        public void AddInfo(string label, Func<string> textFunc)
        {
            if ((label, textFunc) == (null, null))
            {
                y += 20;
            }
            else
            {
                AddInfo(10, 10 + sep, y += 20, label, textFunc);
            }
        }
    }
}
