//using Kickify.Application.Abstractions.Messaging;
//using Kickify.Application.Abstractions.Repositories;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

//namespace Kickify.Application.Features.Auth.Commands.Login
//{
//    public class LoginWithRefreshToken : ICommandHandler<LoginWithRefreshTokenCommand, LoginWithFirebaseCommandResponse>
//    {
//        private readonly IRefreshTokenRepository refreshTokenRepository;
//        private readonly IJwtProvider jwtProvider;
//    }

//    public class  LoginWithRefreshTokenCommand : ICommand<LoginWithRefreshTokenCommandResponse> 
//    {
//        public string Token { get; set; } = string.Empty;
//    }

//    public class LoginWithRefreshTokenCommandResponse 
//    {
//        public 
//    }
//}
