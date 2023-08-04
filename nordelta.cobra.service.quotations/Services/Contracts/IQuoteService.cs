using nordelta.cobra.service.quotations.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nordelta.cobra.service.quotations.Services.Contracts
{
    public interface IQuoteService
    {
        public Task<List<Quotes>> GetQuotesAsync();
        public Task<bool> InformQuotesAsync(List<Quotes> quotes, string endpoint);
    }
}
