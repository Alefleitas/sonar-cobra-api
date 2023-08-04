using Microsoft.EntityFrameworkCore;
using nordelta.cobra.webapi.Models;
using nordelta.cobra.webapi.Repositories.Contexts;
using nordelta.cobra.webapi.Repositories.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using Serilog;

namespace nordelta.cobra.webapi.Repositories
{
    public class HolidaysRepository : IHolidaysRepository
    {
        private readonly InMemoryDbContext _context;

        public HolidaysRepository(InMemoryDbContext context)
        {
            _context = context;
        }

        public bool HasHolidays()
        {
            return _context.HolidayDays.Any();
        }

        public void SaveHolidaysAsync(IEnumerable<HolidayDay> holidays)
        {
            try
            {
                if (_context.HolidayDays.Any())
                {
                    _context.Database.ExecuteSqlRaw("DELETE FROM HolidayDay;");
                }

                var stringValues = holidays.Select(holidayDay => $"({holidayDay.Dia}, {holidayDay.Mes}, {holidayDay.Anio}, '{holidayDay.Motivo}')").ToList();

                _context.Database.ExecuteSqlRaw($"INSERT INTO HolidayDay (Dia, Mes, Anio, Motivo) VALUES {string.Join(',', stringValues)};");
            }
            catch (Exception ex)
            {
                Log.Error("Hubo un error al guardar los feriados: {@ex}", ex);
                throw;
            }
        }

        public IEnumerable<HolidayDay> GetHolidaysByYear(int year)
        {
            return _context.HolidayDays.Where(x => x.Anio.Equals(year));
        }
    }
}
