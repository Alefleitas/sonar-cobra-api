using Microsoft.EntityFrameworkCore;
using nordelta.cobra.webapi.Models;
using nordelta.cobra.webapi.Repositories.Contexts;
using nordelta.cobra.webapi.Repositories.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace nordelta.cobra.webapi.Repositories
{
    public class PaymentDetailRepository : IPaymentDetailRepository
    {
        private readonly RelationalDbContext _context;

        public PaymentDetailRepository(RelationalDbContext context)
        {
            _context = context;
        }

        public bool UpdateAll(List<PaymentDetail> paymentDetails)
        {
            _context.PaymentDetail.UpdateRange(paymentDetails);
            return _context.SaveChanges() > 0;
        }

        public bool CreateAll(List<PaymentDetail> paymentDetails)
        {
            _context.PaymentDetail.AddRange(paymentDetails);
            return _context.SaveChanges() > 0;
        }

        public List<PaymentDetail> GetAll(Expression<Func<PaymentDetail, bool>> predicate = null) 
            => _context.PaymentDetail.Where(predicate).Include(x => x.PaymentMethod).ToList();

        public bool HasPaymentDetail(int paymentMethodId)
        {
            return _context.PaymentDetail.AsNoTracking().Any(x => x.PaymentMethodId == paymentMethodId);
        }
    }
}
