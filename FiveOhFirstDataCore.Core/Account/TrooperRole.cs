﻿using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FiveOhFirstDataCore.Core.Account
{
    public class TrooperRole : IdentityRole<int>
    {
        public TrooperRole() : base() { }
        public TrooperRole(string name) : base(name) { }
    }
}