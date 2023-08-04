using AutoMapper;
using nordelta.cobra.webapi.Models;
using nordelta.cobra.webapi.Models.ValueObject.BankFiles.Constants;
using nordelta.cobra.webapi.Repositories.Contracts;
using nordelta.cobra.webapi.Services.Contracts;
using nordelta.cobra.webapi.Services.DTOs;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;

namespace nordelta.cobra.webapi.Services
{
    public class PaymentDetailService : IPaymentDetailService
    {
        private readonly IPaymentDetailRepository _paymentDetailRepository;
        private readonly IMapper _mapper;

        public PaymentDetailService(
            IPaymentDetailRepository paymentDetailRepository,
            IMapper mapper
            )
        {
            _paymentDetailRepository = paymentDetailRepository;
            _mapper = mapper;
        }

        public bool HasPaymentDetail(int paymentMethodId)
        {
            return _paymentDetailRepository.HasPaymentDetail(paymentMethodId);
        }

        public List<PaymentDetailDto> GetAllByPaymentMethodId(int paymentMethodId)
        {
            var paymentDetailDtos = new List<PaymentDetailDto>();
            try
            {
                var paymentDetails = _paymentDetailRepository.GetAll(x => x.PaymentMethodId == paymentMethodId);
                if (paymentDetails.Any())
                {
                    paymentDetailDtos = _mapper.Map<IEnumerable<PaymentDetail>, IEnumerable<PaymentDetailDto>>(paymentDetails).ToList();
                }
                return paymentDetailDtos;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "GetAllByPaymentMethodId: Ocurrió un error al intentar obtener payment details con paymentMethodId {id}", paymentMethodId);
            }
            return paymentDetailDtos;
        }

        public bool CreateAll(List<PaymentDetail> paymentDetails)
        {
            try
            {
                return _paymentDetailRepository.CreateAll(paymentDetails);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "CreateAll: Ocurrió un error al intentar crear payment details");
            }
            return false;
        }

        public bool UpdateAll(List<PaymentDetail> paymentDetails)
        {
            try
            {
                var listPaymentDetails = new List<PaymentDetail>();

                foreach (var paymentDetail in paymentDetails)
                {
                    var res = _paymentDetailRepository.GetAll(x => x.PaymentMethodId == paymentDetail.PaymentMethodId
                                                                && x.SubOperationId == paymentDetail.SubOperationId).FirstOrDefault();
                    if (res != null)
                    {
                        res.ErrorDetail = paymentDetail.ErrorDetail;
                        res.Status = paymentDetail.Status;
                        res.CreditingDate = paymentDetail.CreditingDate;

                        listPaymentDetails.Add(res);
                    }
                }

                return _paymentDetailRepository.UpdateAll(listPaymentDetails);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "UpdateAll: Ocurrió un error al intentar actualizar payment details");
            }

            return true;
        }

        
    }
}