﻿using FiveOhFirstDataCore.Core.Data;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FiveOhFirstDataCore.Core.Structures
{
    public class NewTrooperData
    {
        public int Id { get; set; }
        public string NickName { get; set; } = "";
        public TrooperRank StartingRank { get; set; } = TrooperRank.Recruit;
        public bool Sixteen { get; set; } = false;
        public bool ModsDownloaded { get; set; } = false;
        public bool PossibleTroll { get; set; } = false;
        public int Age { get; set; }
        public PrefferedRole PreferredRole { get; set; } = PrefferedRole.Trooper;
    }
}
