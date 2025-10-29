using EduMatch.BusinessLogicLayer.Requests.GoogleCalendar;
using EduMatch.BusinessLogicLayer.Responses.GoogleCalendar;
using EduMatch.DataAccessLayer.Entities;
using Google.Apis.Auth;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EduMatch.BusinessLogicLayer.Interfaces
{
	public interface IGoogleAuthService
	{
		Task<GoogleJsonWebSignature.Payload?> VerifyGoogleTokenAsync(string idToken);
		string GenerateAuthUrlDat();
		Task<GoogleToken> ExchangeCodeForTokenDatAsync(string code);
	}
}
