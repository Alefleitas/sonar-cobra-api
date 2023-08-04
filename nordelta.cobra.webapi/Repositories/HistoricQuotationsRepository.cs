using nordelta.cobra.webapi.Models;
using nordelta.cobra.webapi.Repositories.Contexts;
using nordelta.cobra.webapi.Repositories.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;

namespace nordelta.cobra.webapi.Repositories
{
    public class HistoricQuotationsRepository: IHistoricQuotationsRepository
    {
        private readonly RelationalDbContext _context;
        private readonly IUserChangesLogRepository _userChangesLogRepository;

        public HistoricQuotationsRepository(RelationalDbContext context, IUserChangesLogRepository userChangesLogRepository)
        {
            _context = context;
            _userChangesLogRepository = userChangesLogRepository;
        }

        public List<HistoricQuotations> GetAllHistoricQuotations()
        {
            return _context.HistoricQuotations.ToList();
        }

        public List<HistoricQuotations>  GetHistoricQuotations(ETipoQuote tipo, string titulo)
        {
            return _context.HistoricQuotations.Where(x => x.Tipo == tipo && x.Titulo == titulo).ToList();
        }

        public List<HistoricQuotations> GetHistoricQuotationsBySubtipo(ESubtipoQuote subtipo)
        {
            return _context.HistoricQuotations.Where(x => x.Subtipo == subtipo).ToList();
        }

        public HistoricQuotations GetMostRecentHistoricQuotation(string titulo)
        {
            return _context.HistoricQuotations.Where(x => x.Titulo == titulo)
                                              .OrderByDescending(x => x.Fecha)
                                              .FirstOrDefault();
        }

        public HistoricQuotations GetMostRecentHistoricQuotation(ETipoQuote tipo, ESubtipoQuote subtipo)
        {
            return _context.HistoricQuotations.Where(x => x.Tipo == tipo && x.Subtipo == subtipo)
                                              .OrderByDescending(x => x.Fecha)
                                              .FirstOrDefault();
        }

        public List<HistoricQuotations> GetHistoricQuotationsByDate(DateTime date)
        {
            return _context.HistoricQuotations.Where(x => x.Fecha.Date == date.Date).ToList(); ;
        }

        public void SaveQuotations(IEnumerable<FinanceQuotation> quotes)
        {
            DateTime now = DateTime.Now;

            if (_context.HistoricQuotations.Any(x => x.Fecha.Date == now.Date))
            {
                List<HistoricQuotations> updateQuotations = new List<HistoricQuotations>();
                List<HistoricQuotations> newQuotations = new List<HistoricQuotations>();

                foreach (FinanceQuotation quote in quotes)
                {
                    HistoricQuotations toUpdate = quote.Tipo == ETipoQuote.CAUCION ? 
                        _context.HistoricQuotations.FirstOrDefault(x => x.Subtipo == quote.Subtipo && x.Fecha.Date == now.Date) : 
                        _context.HistoricQuotations.FirstOrDefault(x => x.Tipo == quote.Tipo && x.Titulo == quote.Titulo && x.Fecha.Date == now.Date);

                    if (toUpdate == null)
                    {
                        createQuotation(quote, newQuotations, now);
                        continue;
                    }

                    if (quote.Tooltip == null)
                    {
                        toUpdate.Fecha = now;
                    }

                    if (quote.Tooltip == null) toUpdate.Fecha = now;
                    if (quote.Valor != 0)
                    {
                        toUpdate.Valor = quote.Valor;
                    }

                    updateQuotations.Add(toUpdate);
                }
                _context.HistoricQuotations.UpdateRange(updateQuotations);
                _context.HistoricQuotations.AddRange(newQuotations);
                _context.SaveChanges();
            }
            else
            {
                List<HistoricQuotations> newQuotations = new List<HistoricQuotations>();

                foreach(FinanceQuotation quote in quotes)
                {
                    createQuotation(quote, newQuotations, now);
                }
                _context.HistoricQuotations.AddRange(newQuotations);
                _context.SaveChanges();
            }
        }

        public bool HasHistoricQuotations()
        {
            return _context.HistoricQuotations.FirstOrDefault() != null;
        }

        private void createQuotation(FinanceQuotation quote, List<HistoricQuotations> list, DateTime now)
        {
            if (quote.Tooltip != null) return;

            HistoricQuotations newQuotation = new HistoricQuotations()
            {
                Tipo = quote.Tipo,
                Subtipo = quote.Subtipo,
                Fecha = now,
                Titulo = quote.Titulo,
                Valor = quote.Valor
            };
            list.Add(newQuotation);
        }
    }
}
