﻿using AutoMapper;
using Infrastructure.SqlServer.Repositories;
using Infrastructure.SqlServer.Repositories.SqlServer.DataContext;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UseCases.Repositories;
using UseCases.UnitOfWork;

namespace Infrastructure.SqlServer.UnitOfWork
{
    public class SqlServerPublisherUnitOfWork : IPublisherUnitOfWork
    {
        private readonly BookShopDbContext _context;

        public SqlServerPublisherUnitOfWork(BookShopDbContext context, IMapper mapper)
        {
            _context = context;

            PublisherRepository = new SqlServerPublisherRepository(context, mapper);
            BookRepository = new SqlServerBookRepository(context, mapper);
        }

        public IPublisherRepository PublisherRepository { get; }

        public IBookRepository BookRepository { get; }

        public Task BeginTransactionAsync()
        {
            return _context.Database.BeginTransactionAsync();
        }

        public Task CancelTransactionAsync()
        {
            return _context.Database.RollbackTransactionAsync();
        }

        public Task SaveChangesAsync()
        {
            return _context.Database.CommitTransactionAsync();
        }
    }
}
