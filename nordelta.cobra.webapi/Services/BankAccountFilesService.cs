using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CsvHelper;
using CsvHelper.Configuration;
using nordelta.cobra.webapi.Models;
using nordelta.cobra.webapi.Repositories.Contracts;
using nordelta.cobra.webapi.Services.Contracts;
using nordelta.cobra.webapi.Services.DTOs;

namespace nordelta.cobra.webapi.Services
{
    public class BankAccountFilesService : IBankAccountFilesService
    {
        private readonly IBankAccountRepository _bankAccountRepository;
        public BankAccountFilesService(IBankAccountRepository bankAccountRepository)
        {
            _bankAccountRepository = bankAccountRepository;
        }
        public void ProcessAllFiles()
        {
            string filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "BankAccountFiles");
            if (!Directory.Exists(filePath))
            {
                Directory.CreateDirectory(filePath);
            }
            string[] files = Directory.GetFiles(filePath);
            List<BankAccount> newBankAccountList = new List<BankAccount>();
            foreach (string file in files)
            {
                List<BankAccount> newFile = ProcessAccountFile(file);
                newBankAccountList.AddRange(newFile);
            }

            IEnumerable<string> dbKeys = _bankAccountRepository.All().Select(e => e.ClientCuit + e.Cbu + e.Currency);
            Dictionary<string, BankAccount> fileDict = newBankAccountList.ToDictionary(e => e.ClientCuit + e.Cbu + e.Currency);

            IEnumerable<string> toInsert = fileDict.Keys.Except(dbKeys);

            List<BankAccount> result = fileDict.Where(e => toInsert.Contains(e.Key)).Select(e => e.Value).ToList();

            if (result.Any())
            {
                _bankAccountRepository.Add(result);
            }
        }

        private static List<BankAccount> ProcessAccountFile(string filePath)
        {
            List<BankAccountFile> accounts = new List<BankAccountFile>();
            if (File.Exists(filePath))
            {
                using (TextReader fileReader = File.OpenText(filePath))
                {
                    var conf = new CsvConfiguration(CultureInfo.InvariantCulture)
                    {
                        HasHeaderRecord = false
                    };
                    using (CsvReader csv = new CsvReader(fileReader, conf))
                    {
                        csv.Read();
                        accounts = csv.GetRecords<BankAccountFile>().ToList();
                    }
                }
            }

            List<BankAccount> bankAccounts = accounts.Select(e => new BankAccount()
            {
                ClientCuit = e.ClientCuit,
                Cbu = e.Cbu,
                Cuit = e.Cuit,
                Currency = (Currency)e.Currency
            }).ToList();

            return bankAccounts;
        }
    }
}