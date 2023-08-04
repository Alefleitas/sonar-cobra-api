using nordelta.cobra.webapi.Repositories.Contexts;
using System.Linq;
using nordelta.cobra.webapi.Models;
using nordelta.cobra.webapi.Repositories.Contracts;
using System;
using System.Reflection;
using Microsoft.EntityFrameworkCore;
using System.Linq.Dynamic.Core;
using System.Collections.Generic;
using nordelta.cobra.webapi.Controllers.ViewModels;
using nordelta.cobra.webapi.Services.Contracts;
using nordelta.cobra.webapi.Utils;
using Serilog;

namespace nordelta.cobra.webapi.Repositories
{
    public class ExchangeRateFileRepository : IExchangeRateFileRepository
    {
        private readonly RelationalDbContext _context;
        private readonly IHolidaysService _holidaysService;
        private readonly IUserChangesLogRepository _userChangesLogRepository;
        private readonly IUserRepository _userRepository;
        public IUnitOfWork UnitOfWork => _context;

        public ExchangeRateFileRepository(RelationalDbContext context,
                                          IHolidaysService holidaysService,
                                          IUserChangesLogRepository userChangesLogRepository,
                                          IUserRepository userRepository)
        {
            _context = context;
            _holidaysService = holidaysService;
            _userChangesLogRepository = userChangesLogRepository;
            _userRepository = userRepository;
        }

        public void Add(ExchangeRateFile exchangeRateFile)
        {
            _context.ExchangeRateFile.Add(exchangeRateFile);
            _context.SaveChanges();
        }

        public void AddBonoConfig(Bono bono)
        {
            _context.Bono.Add(bono);
            _context.SaveChanges();
        }

        public Bono GetLastBonosConfig()
        {
            return _context.Bono.OrderByDescending(x => x.Id).FirstOrDefault();
        }

        public bool CheckDolarMepJobWasExecuted()
        {
            var datetimeNow = LocalDateTime.GetDateTimeNow();

            var todayQuotation = _context.Quotations
                .Where(x => x.UploadDate.Date == datetimeNow.Date && x.RateType == RateTypes.UsdMEP)
                .FirstOrDefault();

            return todayQuotation != null;
        }

        public bool CheckQuotationExists(Quotation quotation)
        {
            var result = _context.Quotations.Any(x =>
            x.FromCurrency == quotation.FromCurrency &&
            x.RateType == quotation.RateType &&
            x.EffectiveDateFrom.Date == quotation.EffectiveDateFrom.Date &&
            x.EffectiveDateTo.Date == quotation.EffectiveDateTo.Date);
            return result;
        }
        public ExchangeRateFile GetByFileName(string fileName)
        {
            return _context.ExchangeRateFile.FirstOrDefault(e => e.FileName.Equals(fileName));
        }

        public ExchangeRateFile GetLastExchangeRateFileAvailable()
        {
            if (!_context.ExchangeRateFile.Any())
            {
                return null;
            }
            int fileId = _context.ExchangeRateFile.Max(e => e.Id);
            ExchangeRateFile file = _context.ExchangeRateFile.Find(fileId);

            return file;
        }

        public List<QuotationViewModel> GetQuotationTypes()
        {
            Type baseType = typeof(Quotation);
            Assembly assembly = baseType.Assembly;
            var quotationTypes = assembly.GetTypes().Where(t => typeof(Quotation).IsAssignableFrom(t) && t != typeof(Quotation)).ToList();

            try
            {
                List<QuotationViewModel> quotations = new List<QuotationViewModel>();
                foreach (var quotationType in quotationTypes)
                {
                    var data = _context.Set(quotationType).OrderBy("EffectiveDateFrom desc, UploadDate desc").FirstOrDefault();
                    quotations.Add(new QuotationViewModel
                    {
                        Code = quotationType.Name,
                        Data = data != null ? data : Activator.CreateInstance(quotationType)
                    });
                }

                return quotations;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return null;
            }
        }

        public dynamic GetLastQuotation(string type)
        {
            var quotation = _context.Set(type).OrderBy("UploadDate desc, Id desc").FirstOrDefault();
            if (quotation == null)
            {
                var quotationType = _context.GetType().Assembly.GetExportedTypes().FirstOrDefault(t => t.Name == type);
                return Activator.CreateInstance(quotationType);
            }
            else
                return quotation;
        }

        public dynamic GetCurrentQuotation(string type, bool lastQuote = false)
        {
            var quotationQueryable = _context.Set(type);
            if (!lastQuote)
            {
                var query = $"\"{LocalDateTime.GetDateTimeNow().Date:yyyy-MM-dd}\" >= EffectiveDateFrom and \"{LocalDateTime.GetDateTimeNow().Date:yyyy-MM-dd}\" < EffectiveDateTo";
                quotationQueryable = quotationQueryable.Where(query);
            }

            var quotation = quotationQueryable.OrderBy("UploadDate desc").FirstOrDefault();
            if (quotation == null)
            {
                var quotationType = _context.GetType().Assembly.GetExportedTypes().FirstOrDefault(t => t.Name == type);
                return Activator.CreateInstance(quotationType);
            }
            else
                return quotation;
        }

        public List<object> GetAllQuotations(string type)
        {
            return _context.Set(type).OrderBy("UploadDate desc").ToList();
        }

        public dynamic GetQuotationBetweenDate(string type, DateTime date)
        {
            var query = $"\"{date.Date:yyyy-MM-dd}\" >= EffectiveDateFrom and \"{date.Date:yyyy-MM-dd}\" < EffectiveDateTo";
            var quotation = _context.Set(type).Where(query).FirstOrDefault();
            if (quotation == null)
            {
                var quotationType = _context.GetType().Assembly.GetExportedTypes().FirstOrDefault(t => t.Name == type);
                return Activator.CreateInstance(quotationType);
            }
            else
                return quotation;
        }

        public dynamic GetQuotationByDate(string type, DateTime date)
        {
            var query = $"\"{date.ToString("yyyy-MM-dd")}\" = EffectiveDateFrom";
            var quotation = _context.Set(type).Where(query).FirstOrDefault();
            if (quotation == null)
            {
                var quotationType = _context.GetType().Assembly.GetExportedTypes().FirstOrDefault(t => t.Name == type);
                return Activator.CreateInstance(quotationType);
            }
            else
                return quotation;
        }

        public Quotation GetQuotationById(int quotationId)
        {
            return _context.Quotations.AsNoTracking().SingleOrDefault(x => x.Id == quotationId);
        }

        public List<Quotation> GetQuotationsByIds(List<int> quotationIds)
        {
            return _context.Quotations.Where(x => quotationIds.Contains(x.Id)).ToList();
        }

        public void CancelQuotation(Quotation quotation)
        {
            _context.Quotations.Remove(quotation);
            _context.SaveChanges();
        }

        public List<Quotation> AddQuotations(List<Quotation> quotations)
        {
            _context.Quotations.AddRange(quotations);
            _context.SaveChanges();
            return quotations;
        }

        public dynamic AddQuotation(string quotationType, dynamic quotation, User user, EQuotationSource loadType)
        {
            try
            {

                if (loadType == EQuotationSource.MANUAL)
                {
                    var objectType = _context.GetType().Assembly.GetExportedTypes().FirstOrDefault(t => t.Name == quotationType);
                    var instance = Activator.CreateInstance(objectType);
                    var dateFrom = DateTime.Parse(quotation["effectiveDateFrom"].ToString());
                    if (quotationType == "CAC")
                    {
                        var dateFromEffective = new DateTime(dateFrom.Year, dateFrom.Month, dateFrom.Day, 0, 0, 0, 000);
                        quotation["effectiveDateFrom"] = dateFromEffective;
                        var dateToEffective = new DateTime(
                            dateFromEffective.AddMonths(1).Year, //year of the date
                            dateFromEffective.AddMonths(1).Month, //month of the date
                            DateTime.DaysInMonth(dateFromEffective.AddMonths(1).Year, dateFromEffective.AddMonths(1).Month), //last day of the resultant month
                            23, 59, 59, 999); //last second of the day
                        quotation["effectiveDateTo"] = dateToEffective;
                    }
                    else
                    {
                        quotation["effectiveDateFrom"] = new DateTime(dateFrom.Year, dateFrom.Month, dateFrom.Day, 0, 0, 0, 000);
                        quotation["effectiveDateTo"] = new DateTime(dateFrom.Year, dateFrom.Month, dateFrom.Day, 23, 59, 59, 999);
                    }

                    foreach (var prop in objectType.GetProperties())
                    {
                        var propFirstToLower = char.ToLower(prop.Name[0]) + prop.Name.Substring(1);
                        if (quotation[propFirstToLower] != null)
                        {
                            PropertyInfo targetProp = instance.GetType().GetProperty(prop.Name, BindingFlags.Public | BindingFlags.Instance);
                            if (targetProp != null && targetProp.CanWrite)
                            {
                                targetProp.SetValue(instance, Convert.ChangeType(quotation[propFirstToLower], prop.PropertyType), null);
                            }
                        }
                    }

                    (instance as Quotation).UserId = user.Id;
                    (instance as Quotation).UploadDate = LocalDateTime.GetDateTimeNow();
                    (instance as Quotation).Source = loadType;
                    _context.Add(instance);
                    _context.SaveChanges();
                    var quoationId = (instance as Quotation).Id;
                    if (user == null)
                    {
                        Log.Error($"Error: No se encontro usuario. Id: {user.Id}.");
                    }
                    else
                    {
                        AddLog(quoationId, user);

                    }

                    return instance;
                }
                UnitOfWork.RunWithExecutionStrategy(() =>
                {
                    (quotation as Quotation).UserId = user.Id;
                    (quotation as Quotation).UploadDate = LocalDateTime.GetDateTimeNow();
                    (quotation as Quotation).Source = loadType;
                    _context.Quotations.Add(quotation);
                    UnitOfWork.SaveChanges();

                    AddLog(quotation.Id,
                        new User
                        {
                            Id = string.IsNullOrEmpty(Convert.ToString(quotation.UserId)) ? "SYSTEM" : Convert.ToString(quotation.UserId),
                            Email = (user is null || string.IsNullOrEmpty(user.Email)) ? "SYSTEM" : user.Email
                        });
                });

                return quotation;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error persisting Quotation {@quotation} of type {type}", quotation, quotationType);
                return null;
            }
        }

        public bool CheckCacExists(string cacDate)
        {
            return _context.Quotations.Any(q => q.Description.Contains(cacDate));
        }

        public List<Quotation> GetAllQuotationsToday()
        {
            return _context.Quotations.Where(x => x.UploadDate.Date == DateTime.Now.Date).ToList();
        }


        private void AddLog(int id, User user)
        {
            _userChangesLogRepository.Add(new UserChangesLog
            {
                EntityId = id,
                ModifiedEntity = CobraEntity.Quotations,
                ModifiedField = "Id",
                UserEmail = user.Email,
                UserId = user.Id,
                ModifyDate = LocalDateTime.GetDateTimeNow()
            });
        }

    }
}
