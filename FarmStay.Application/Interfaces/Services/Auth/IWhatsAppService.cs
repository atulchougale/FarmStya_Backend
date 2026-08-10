using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FarmStay.Application.Interfaces.Services.Auth
{
    public interface IWhatsAppService
    {
        Task<bool> SendOtpAsync(string mobileNumber, string otp);

        Task<bool> SendMessageAsync(string mobileNumber, string message);
    }
}