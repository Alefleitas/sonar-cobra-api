using nordelta.cobra.webapi.Models;
using System.Collections.Generic;
using nordelta.cobra.webapi.Controllers.ViewModels;
using nordelta.cobra.webapi.Services.DTOs;
using System.Threading.Tasks;

namespace nordelta.cobra.webapi.Services.Contracts
{
    public interface ILoginService
    {
        User GetAuthenticatedUser(string token);
        User GetUserById(string Id);
        ChangePasswordResponse ChangePassword(ChangePasswordRequest changePasswordRequest);
        UpdateUserResponse UpdateUser(UpdateUserRequest updateUserRequest);
        List<UserDataResponse> GetSsoUsers();
        List<EmpresaResponse> GetSsoEmpresas();
        List<LastAccessViewModel> GetAllLastAccess();
        List<ClientDuplicatedEmailsViewModel> GetAllClientDuplicatedMails();
        List<CreatedUsersViewModel> GetAllCreatedUsers();
        void FilterUserPermissions(ref User user);
        Task<List<ForeignCuit>> GetAllForeignCuits();
    }
}
