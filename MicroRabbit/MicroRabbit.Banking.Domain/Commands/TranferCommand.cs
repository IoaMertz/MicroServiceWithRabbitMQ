﻿using MicroRabbit.Domain.Core.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MicroRabbit.Banking.Domain.Commands
{
    public abstract class TranferCommand :Command
    {
        //only those who extend it will be able to set it
        public int From { get; protected set; }
        public int To { get; protected set; }
        public decimal Amount { get; protected set; }
    }
}
