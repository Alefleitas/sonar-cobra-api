using System.Collections.Generic;
using AutoMapper;
using nordelta.cobra.webapi.Models;
using nordelta.cobra.webapi.Repositories.Contracts;
using nordelta.cobra.webapi.Services.Contracts;
using nordelta.cobra.webapi.Services.DTOs;

namespace nordelta.cobra.webapi.Services
{
    public class PaymentHistoryService : IPaymentHistoryService
    {
        private readonly IMapper _mapper;
        private readonly IDebinRepository _debinRepository;
        public PaymentHistoryService(IMapper mapper, IDebinRepository debinRepository)
        {
            this._mapper = mapper;
            this._debinRepository = debinRepository;
        }

        public List<PaymentHistoryDto> GetAllApprovedDebin(string clientCuit)
        {
            var result = _debinRepository.GetAllPayed(clientCuit);
            var map = _mapper.Map<List<Debin>, List<PaymentHistoryDto>>(result);
            return map;
        }
    }
}
