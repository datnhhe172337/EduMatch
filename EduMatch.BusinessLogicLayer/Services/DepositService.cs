using AutoMapper;
using EduMatch.BusinessLogicLayer.DTOs;
using EduMatch.BusinessLogicLayer.Interfaces;
using EduMatch.BusinessLogicLayer.Requests;
using EduMatch.DataAccessLayer.Entities;
using EduMatch.DataAccessLayer.Enum;
using EduMatch.DataAccessLayer.Interfaces;
using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;

// --- THESE ARE THE CORRECT 'using' STATEMENTS ---
using PayOS;

using PayOS.Models.V2.PaymentRequests;
using EduMatch.BusinessLogicLayer.Requests.Wallet; // For CreatePaymentLinkResponse
// ---------------------------------------------

namespace EduMatch.BusinessLogicLayer.Services
{
    public class DepositService : IDepositService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        // This is correct for your project
        private readonly PayOSClient _payosClient;

        private readonly IConfiguration _configuration;

        public DepositService(IUnitOfWork unitOfWork, IMapper mapper, PayOSClient payosClient, IConfiguration configuration)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _payosClient = payosClient;
            _configuration = configuration;
        }

        public async Task<CreateDepositResponseDto> CreateDepositRequestAsync(CreateDepositRequest request, string userEmail)
        {
            // 1. Get or create wallet
            var wallet = await _unitOfWork.Wallets.GetWalletByUserEmailAsync(userEmail);
            if (wallet == null)
            {
                wallet = new Wallet { UserEmail = userEmail };
                await _unitOfWork.Wallets.AddAsync(wallet);
            }

            // 2. Create deposit record
            var newDeposit = new Deposit
            {
                WalletId = wallet.Id,
                Amount = request.Amount,
                Status = TransactionStatus.Pending,
                PaymentGateway = "PayOS",
                CreatedAt = System.DateTime.UtcNow
            };
            await _unitOfWork.Deposits.AddAsync(newDeposit);
            await _unitOfWork.CompleteAsync();

            // 3. Prepare data for PayOS
            var returnUrl = _configuration["AllowedOrigins:0"] + "/user/wallet/deposit-success";
            var cancelUrl = _configuration["AllowedOrigins:0"] + "/user/wallet/deposit-failed";

            // This class is in PayOS.Models
            var paymentRequest = new CreatePaymentLinkRequest
            {
                OrderCode = newDeposit.Id,
                Amount = request.Amount,
                Description = $"Nap tien vao vi {userEmail}",
                ReturnUrl = returnUrl,
                CancelUrl = cancelUrl
            };

            // 4. Create the payment link
            // This method returns 'CreatePaymentLinkResponse'
            CreatePaymentLinkResponse paymentResponse = await _payosClient.PaymentRequests.CreateAsync(paymentRequest);

            // 5. Return the URL
            // The properties are directly on the response object
            // Use PascalCase as is standard in C#
            return new CreateDepositResponseDto
            {
                CheckoutUrl = paymentResponse.CheckoutUrl,
                DepositId = newDeposit.Id
            };
        }
    }
}