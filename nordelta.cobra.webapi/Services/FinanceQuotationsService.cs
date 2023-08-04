using AutoMapper;
using Microsoft.Extensions.DependencyInjection;
using nordelta.cobra.webapi.Models;
using nordelta.cobra.webapi.Repositories.Contracts;
using nordelta.cobra.webapi.Services.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using Serilog;

namespace nordelta.cobra.webapi.Services
{
    public class FinanceQuotationsService : IFinanceQuotationsService
    {
        private QuotationPackage _financeQuotations;
        private static IServiceProvider _services;
        private readonly IMapper _mapper;

        public FinanceQuotationsService(IServiceProvider services, IMapper mapper)
        {
            _services = services;
            _mapper = mapper;
        }

        public void UpdateQuotations(IEnumerable<FinanceQuotation> quotes)
        {

            var validatedQuotes = ValidateQuotes(quotes);

            _financeQuotations = new QuotationPackage()
            {
                Date = DateTime.Now,
                Quotations = GetVariations(validatedQuotes)
            };
        }

        public QuotationPackage GetQuotations()
        {
            if (_financeQuotations == null)
            {
                UpdateWithLastQuotations();
            }
            return _financeQuotations;
        }

        public void SaveQuotations()
        {
            using (var scope = _services.CreateScope())
            {
                var _historicQuotationsRepository = scope.ServiceProvider.GetRequiredService<IHistoricQuotationsRepository>();
                _historicQuotationsRepository.SaveQuotations(_financeQuotations.Quotations);
            }
        }

        private IEnumerable<FinanceQuotation> GetVariations(IEnumerable<FinanceQuotation> quotes)
        {
            using (var scope = _services.CreateScope())
            {
                var _historicQuotationsRepository = scope.ServiceProvider.GetRequiredService<IHistoricQuotationsRepository>();
                var _holidaysService = scope.ServiceProvider.GetRequiredService<IHolidaysService>();

                foreach (FinanceQuotation quote in quotes)
                {
                    List<HistoricQuotations> data = new List<HistoricQuotations>();
                    List<Variacion> variations = new List<Variacion>();

                    if (quote.Tipo == ETipoQuote.CAUCION)
                    {
                        data = _historicQuotationsRepository.GetHistoricQuotationsBySubtipo(quote.Subtipo);
                    }
                    else
                    {
                        data = _historicQuotationsRepository.GetHistoricQuotations(quote.Tipo, quote.Titulo);
                    }

                    try
                    {
                        DateTime lastDay = DateTime.Now.AddDays(-1);
                        DateTime lastMonth = DateTime.Now.AddMonths(-1);
                        lastMonth = new DateTime(lastMonth.Year, lastMonth.Month, DateTime.DaysInMonth(lastMonth.Year, lastMonth.Month));
                        DateTime lastYear = new DateTime(lastDay.Year - 1, 12, 31);
                    
                        lastDay = _holidaysService.GetPreviousWorkDayFromDate(lastDay);
                        lastMonth = _holidaysService.GetPreviousWorkDayFromDate(lastMonth);
                        lastYear = _holidaysService.GetPreviousWorkDayFromDate(lastYear);

                        double varDiaria = 0;
                        double varMensual = 0;
                        double varAnual = 0;

                        var lastDayData = data.Find(x => x.Fecha.Date == lastDay.Date);
                        var lastMonthData = data.Find(x => x.Fecha.Date == lastMonth.Date);
                        var lastAnualData = data.Find(x => x.Fecha.Date == lastYear.Date);

                        if (lastDayData != null)
                            varDiaria = GetVariacionCalculo(quote, lastDayData.Valor);
                        if (lastMonthData != null)
                            varMensual = GetVariacionCalculo(quote, lastMonthData.Valor);
                        if (lastAnualData != null)
                            varAnual = GetVariacionCalculo(quote, lastAnualData.Valor);

                        variations.Add(new Variacion() { Tipo = ETipoVariacion.DIARIA, Valor = varDiaria, HistoricAvailable = lastDayData != null });
                        variations.Add(new Variacion() { Tipo = ETipoVariacion.MENSUAL, Valor = varMensual, HistoricAvailable = lastMonthData != null });
                        variations.Add(new Variacion() { Tipo = ETipoVariacion.ANUAL, Valor = varAnual, HistoricAvailable = lastAnualData != null });

                    }
                    catch (Exception ex)
                    {
                        Log.Error("GetVariations(): Error al obtener variaciones de cotización. Exception detail: {@ex}", ex);
                    }

                    quote.Variacion = variations;
                }
            }

            return quotes;
        }

        private double GetVariacionCalculo(FinanceQuotation quote, double valorInicial)
        {
            if (quote.Tipo == ETipoQuote.CANJE || quote.Tipo == ETipoQuote.CAUCION)
            {
                return quote.Valor - valorInicial;
            }

            return (quote.Valor - valorInicial) / valorInicial * 100;
        }

        protected IEnumerable<FinanceQuotation> ValidateQuotes(IEnumerable<FinanceQuotation> quotes)
        {
            try
            {
                using (var scope = _services.CreateScope())
                {
                    var _historicQuotationsRepository = scope.ServiceProvider.GetRequiredService<IHistoricQuotationsRepository>();

                    foreach (var q in quotes)
                    {
                        if (q.Tooltip != null)
                        {
                            HistoricQuotations getHistoricQuote = new HistoricQuotations();
                            if (q.Tipo == ETipoQuote.CAUCION && q.Subtipo == ESubtipoQuote.PLAZO_CERCANO)
                                getHistoricQuote = _historicQuotationsRepository.GetMostRecentHistoricQuotation(q.Tipo, q.Subtipo);
                            else getHistoricQuote = _historicQuotationsRepository.GetMostRecentHistoricQuotation(q.Titulo);

                            if (getHistoricQuote != null)
                            {
                                q.Valor = getHistoricQuote.Valor;
                                q.Tooltip.Mensaje = q.Tooltip.Mensaje + " - se muestra valor cargado el " + getHistoricQuote.Fecha;
                                if (q.Tipo == ETipoQuote.CAUCION && q.Subtipo == ESubtipoQuote.PLAZO_CERCANO)
                                    q.Titulo = getHistoricQuote.Titulo;
                            }
                        }
                    }

                    return quotes;
                }
            }
            catch (Exception ex)
            {
                Log.Error("ValidateQuotes(): Error validando cotizaciones. Exception detail: {@ex}", ex);
            }

            return quotes;
        }

        private void UpdateWithLastQuotations()
        {
            List<HistoricQuotations> listaCotizaciones = new List<HistoricQuotations>();

            try
            {
                using (var scope = _services.CreateScope())
                {
                    var _historicQuotationsRepository = scope.ServiceProvider.GetRequiredService<IHistoricQuotationsRepository>();

                    if (!_historicQuotationsRepository.HasHistoricQuotations())
                    {
                        _financeQuotations = new QuotationPackage()
                        {
                            Date = DateTime.Now,
                            Quotations = new List<FinanceQuotation>()
                        };
                        return;
                    }

                    var _holidaysService = scope.ServiceProvider.GetRequiredService<IHolidaysService>();

                    DateTime date = DateTime.Now;
                    while (listaCotizaciones.Count() == 0)
                    {
                        listaCotizaciones = _historicQuotationsRepository.GetHistoricQuotationsByDate(date.Date);
                        if (listaCotizaciones.Count() == 0) date = _holidaysService.GetPreviousWorkDayFromDate(date.AddDays(-1));
                    }
                }

                List<FinanceQuotation> historicData = new List<FinanceQuotation>();
                listaCotizaciones.ForEach(x =>
                {
                    historicData.Add(new FinanceQuotation
                    {
                        Tipo = x.Tipo,
                        Subtipo = x.Subtipo,
                        Titulo = x.Titulo,
                        Valor = x.Valor,
                        Tooltip = new TooltipMessage
                        {
                            Tipo = "outdated",
                            Mensaje = "Cotización desactualizada"
                        }
                    });
                });

                _financeQuotations = new QuotationPackage()
                {
                    Date = listaCotizaciones.FirstOrDefault().Fecha,
                    Quotations = GetVariations(historicData)
                };

            } catch (Exception ex)
            {
                Log.Error("UpdateWithLastQuotations(): Error obteniendo cotizaciones. Exception detail: {@ex}", ex);
            }
        }
    }

    public class QuotationPackage
    {
        public DateTime Date { get; set; }
        public IEnumerable<FinanceQuotation> Quotations { get; set; }
    }

}