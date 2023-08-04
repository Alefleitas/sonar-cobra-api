using nordelta.cobra.webapi.Models;
using nordelta.cobra.webapi.Models.ArchivoDeuda;
using nordelta.cobra.webapi.Services.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace nordelta.cobra.webapi.Services.Contracts
{
    public interface INotificationService
    {
        void CheckForNotifications(Type type, DateTime date);
        void CheckForDebinNotifications(List<Debin> debins);
        void NotifyQuotationCancellation(int quotationId);
        bool CreateTemplate(Template template, int notificationTypeId);
        bool UpdateTemplate(Template template);
        string ReplaceTokens(string stringWithTokens, SsoUser ssoUser, DetalleDeuda detalleDeuda, Debin debin, Communication comm, List<TemplateTokenReference> templateReferences, Dictionary<string, string> businessUnitByProductCode, List<DetalleDeuda> detallesDeuda);
        void NotifyAdvanceFeeOrders(EAdvanceFeeStatus status);
        void NotifyTodayQuotations();
        void NotifyRejectedAdvanceFeeOrders(List<dynamic> orders);
        void NotifyDebtFreeUserReport(List<DebtFreeNotificationDto> debtsFreee);
    }
}
