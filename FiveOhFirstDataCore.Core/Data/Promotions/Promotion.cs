﻿using FiveOhFirstDataCore.Core.Account;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FiveOhFirstDataCore.Core.Data.Promotions
{
    public class Promotion
    {
        public Trooper PromotionFor { get; set; }
        public int PromotionForId { get; set; }

        public Trooper RequestedBy { get; set; }
        public int? RequestedById { get; set; }

        public List<Trooper> ApprovedBy { get; set; }

        public PromotionBoardLevel NeededBoard { get; set; }
        public PromotionBoardLevel CurrentBoard { get; set; }

        public int PromoteFrom { get; set; }
        public int PromoteTo { get; set; }

        public string Reason { get; set; }
    }
}
