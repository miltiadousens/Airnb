using Airnb.Application.Repository.Interfaces;
using Airnb.Application.Security.Interfaces;
using Airnb.Shared.Auth.Requests;
using System;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.Reflection;
using System.Text;

namespace Airnb.Application.Usecases.Users.Logout
{
    public class LogoutUsecase : ILogoutUsecase
    {
        private readonly IRefreshTokenRepository _refreshTokenRepository;
        private readonly ITokenService _tokenService;

        public LogoutUsecase(
            IRefreshTokenRepository refreshTokenRepository,
            ITokenService tokenService)
        {
            _refreshTokenRepository = refreshTokenRepository;
            _tokenService = tokenService;
        }

        public async Task ExecuteAsync(LogoutRequest request, CancellationToken cancellationToken = default)
        {
            // 1. Find tokenet ud fra dets hash (selve tokenet gemmes aldrig i databasen)
            var hash = _tokenService.HashRefreshToken(request.RefreshToken);
            var stored = await _refreshTokenRepository.GetByTokenHashAsync(hash, cancellationToken);

            // 2. Findes det ikke, eller er det allerede brugt/udløbet? Så er der intet at spærre
            if (stored is null || !stored.IsActive)
                return;
            
            /*Den kaster ingen exception, hvis tokenet ikke findes.Det er bevidst. 
            Målet med log ud er, at tokenet ikke virker bagefter.
            Hvis det allerede ikke virker, er målet nået. Det kaldes, 
            at operationen er idempotent: kalder man den to gange, 
            er resultatet det samme som én gang.Det betyder også, 
            at API’et ikke afslører, om et bestemt token findes, 
            hvis nogen prøver at gætte.*/

            // 3. Spær det, så det aldrig kan bruges igen
            stored.Revoke();
            await _refreshTokenRepository.SaveChangesAsync(cancellationToken);

            /*Revoke() er domænets egen metode. Use casen bestemmer ikke selv, 
            hvordan et token spærres(at RevokedAt sættes til nu). 
            Det ved entiteten selv. Use casen siger bare “spær dig”. 
            Det er jeres lagdeling, der virker som den skal.*/
        }
    }
}
