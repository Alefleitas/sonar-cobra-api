using nordelta.cobra.webapi.Repositories.Contracts;
using nordelta.cobra.webapi.Services.Contracts;
using System;
using System.Collections.Generic;
using nordelta.cobra.webapi.Models;

namespace nordelta.cobra.webapi.Services
{
    public class RestrictionsListService : IRestrictionsListService
    {
        private readonly IRestrictionsListRepository _restrictionsListRepository;

        public RestrictionsListService(IRestrictionsListRepository restrictionsListRepository)
        {
            _restrictionsListRepository = restrictionsListRepository;
        }

        public bool SetLockAdvancePayments(bool action, User user)
        {
            return _restrictionsListRepository.SetLockAdvancePayments(action, user);
        }

        public LockAdvancePayments GetLockAdvancePayments()
        {
            return _restrictionsListRepository.GetLockAdvancePayments();
        }

        public List<Restriction> GetCompleteRestrictionsList()
        {
            return _restrictionsListRepository.GetCompleteRestrictionsList();
        }

        public List<Restriction> GetRestrictionsListByUserId(string userId)
        {
            return _restrictionsListRepository.GetRestrictionsListByUserId(userId);
        }

        public bool PostRestrictionList(List<Restriction> newRestrictions)
        {
            string userId = newRestrictions[0].UserId;
            try
            {
                bool deleteOk = _restrictionsListRepository.DeleteRestrictionsByUserId(userId);
                if (deleteOk)
                {
                    try
                    {
                        _restrictionsListRepository.AddRestrictions(newRestrictions);
                        Serilog.Log.Information("Se han agregado restricciones exitosamente");
                        return true;
                    }
                    catch (Exception ex)
                    {
                        Serilog.Log.Error("Ocurrió un error agregando restricciones: {@message}", ex.Message);
                        return false;
                    }
                }
                return false;
            } catch (Exception ex)
            {
                Serilog.Log.Error("Ocurrió un error actualizando la lista de restricciones: {@message}", ex.Message);
                return false;
            }
        }

        public bool DeleteRestrictionsByUserId(string userId)
        {
            return _restrictionsListRepository.DeleteRestrictionsByUserId(userId);
        }
    }
}