﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UseCases.UnitOfWork
{
    public interface IUnitOfWork
    {
        Task BeginTransactionAsync();
        Task SaveChangesAsync();
        Task CancelTransactionAsync();
    }
}
