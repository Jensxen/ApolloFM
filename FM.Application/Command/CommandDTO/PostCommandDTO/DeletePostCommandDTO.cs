﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FM.Application.Command.CommandDTO.PostCommandDTO
{
    public class DeletePostCommandDTO
    {
        public int Id { get; set; }
        public byte[] RowVersion { get; set; }
    }
}
