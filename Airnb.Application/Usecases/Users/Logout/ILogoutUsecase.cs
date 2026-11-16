using Airnb.Shared.Auth.Requests;
using System;
using System.Collections.Generic;
using System.Text;

namespace Airnb.Application.Usecases.Users.Logout
{
    public interface ILogoutUsecase
    {
        Task ExecuteAsync(LogoutRequest request, CancellationToken cancellationToken = default);
    }
}
