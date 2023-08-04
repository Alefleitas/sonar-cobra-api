using nordelta.cobra.webapi.Models;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace nordelta.cobra.webapi.Repositories.Contracts
{
    public interface IPaymentDetailRepository
    {
        public List<PaymentDetail> GetAll(Expression<Func<PaymentDetail, bool>> predicate = null);
        public bool UpdateAll(List<PaymentDetail> paymentDetails);
        public bool CreateAll(List<PaymentDetail> paymentDetails);
        public bool HasPaymentDetail(int paymentMethodId);
    }
}
