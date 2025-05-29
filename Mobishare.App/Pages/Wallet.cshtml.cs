using System.Globalization;
using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Mobishare.Core.Requests.Users.BalanceRequest.Commands;
using Mobishare.Core.Requests.Users.BalanceRequest.Queries;
using PayPal.REST.Client;
using PayPal.REST.Models.Orders;
using PayPal.REST.Models.PaymentSources;

namespace Mobishare.App.Pages
{
    public class WalletModel : PageModel
    {
        private readonly IPayPalClient _payPalClient;
        private readonly IMediator _mediator;
        private readonly IMapper _mapper;
        private readonly ILogger<WalletModel> _logger;
        private readonly UserManager<IdentityUser> _userManager;

        public WalletModel(IPayPalClient payPalClient, IMediator mediator, IMapper mapper, ILogger<WalletModel> logger, UserManager<IdentityUser> userManager)
        {
            _payPalClient = payPalClient ?? throw new ArgumentNullException(nameof(payPalClient));
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
        }

        public async Task<IActionResult> OnPostDeposit(float cost)
        {
            cost = 20;
            var url = "https://localhost:7027/Wallet";

            var res = await _payPalClient.CreateOrder(new OrderRequest
            {
                PurchaseUnits =
                [
                    new()
                    {
                        Items =
                        [
                            new()
                            {
                                Name = "Balance payment",
                                Description = $"Payment for balance",
                                Quantity = "1",
                                UnitAmount = new()
                                {
                                    Value = cost.ToString(CultureInfo.InvariantCulture)
                                },
                                Category = ItemCategory.DigitalGoods
                            }
                        ],
                        Amount = new()
                        {
                            Value = cost.ToString(CultureInfo.InvariantCulture),
                            Breakdown = new()
                            {
                                ItemTotal = new()
                                {
                                    Value = cost.ToString(CultureInfo.InvariantCulture)
                                }
                            }
                        }
                    },
                ]
            });

            res = await _payPalClient.ConfirmOrder(
                res.Id,
                new PayPalPaymentSource
                {
                    PayPalData = new()
                    {
                        ExperienceContext = new()
                        {
                            LandingPage = LandingPage.Login,
                            PaymentMethodPreference = PaymentMethodPreference.ImmediatePaymentRequired,
                            UserAction = UserAction.PayNow,
                            BrandName = "MobiShare",
                            ReturnUrl = $"{url}?handler=success",
                            CancelUrl = $"{url}?handler=cancel"
                        }
                    }
                }
            );

            var checkoutUrl = res.Links
                .Where(l => l.Method.ToUpperInvariant() == "GET" && l.Rel.ToLowerInvariant() == "payer-action").First();

            return Redirect(checkoutUrl.Href);
        }

        public async Task<IActionResult> OnGetSuccessAsync(string payerId, string token)
        {
            if (string.IsNullOrEmpty(payerId) || string.IsNullOrEmpty(token))
            {
                return RedirectToPage("Wallet");
            }

            var res = await _payPalClient.CapturePayment(token);

            if (res.Status == OrderStatus.Completed)
            {
                var userId = _userManager.GetUserId(User);
                if (userId == null)
                {
                    _logger.LogWarning("User ID is null. Unable to retrieve balance.");
                    return RedirectToPage("Wallet");
                }

                _logger.LogInformation("Retrieving balance for user {UserId}", userId);

                var balance = await _mediator.Send(new GetBalanceByUserId(userId));

                if (balance == null)
                {
                    _logger.LogWarning("No balance record found for user {UserId}", userId);
                    return RedirectToPage("Wallet");
                }

                // TODO: add payment history

                await _mediator.Send(new UpdateBalance
                {
                    Id = balance.Id,
                    Credit = balance.Credit + double.Parse(res.PurchaseUnits.First().Amount.Value, CultureInfo.InvariantCulture),
                    Points = balance.Points,
                    UserId = userId
                });
            }

            return RedirectToPage("Wallet");
        }

        public void OnGet()
        {
        }
    }
}
