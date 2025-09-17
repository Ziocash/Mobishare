using System.ComponentModel.DataAnnotations;
using System.Globalization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Mobishare.Core.Enums.Balance;
using Mobishare.Core.Models.UserRelated;
using Mobishare.Core.Requests.Users.BalanceRequest.Commands;
using Mobishare.Core.Requests.Users.HistoryCreditRequest.Commands;
using Mobishare.Core.Requests.Users.HistoryPointRequest.Commands;
using PayPal.REST.Client;
using PayPal.REST.Models.Orders;
using PayPal.REST.Models.PaymentSources;

namespace Mobishare.App.Pages
{
    public class WalletModel : PageModel
    {
        private readonly IPayPalClient _payPalClient;
        private readonly HttpClient _httpClient;
        private readonly ILogger<WalletModel> _logger;
        private readonly UserManager<IdentityUser> _userManager;
        public Balance UserBalance { get; set; }
        public List<HistoryCredit> HistoryCredit { get; set; }

        public WalletModel(
            IPayPalClient payPalClient,
            IHttpClientFactory httpClientFactory,
            ILogger<WalletModel> logger,
            UserManager<IdentityUser> userManager)
        {
            _payPalClient = payPalClient ?? throw new ArgumentNullException(nameof(payPalClient));
            _httpClient = httpClientFactory.CreateClient("CityApi");
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
        }

        [BindProperty]
        public InputWalletModel Input { get; set; }
        public class InputWalletModel
        {
            [Required(ErrorMessage = "Please enter an an amount.")]
            [Range(5, double.MaxValue, ErrorMessage = "Please enter an amount of at least 5.")]
            public double CreditAmount { get; set; }
        }

        public async Task<IActionResult> OnPostUsePoints()
        {
            var userId = _userManager.GetUserId(User);
            if (userId == null)
            {
                _logger.LogWarning("User ID is null. Unable to retrieve balance.");
                return RedirectToPage("Wallet");
            }

            var balance = await _httpClient.GetFromJsonAsync<Balance>($"api/Balance/{userId}");
            if (balance == null)
            {
                _logger.LogWarning("No balance record found for user {UserId}", userId);
                return RedirectToPage("Wallet");
            }

            if (balance.Points < 5)
            {
                TempData["ErrorMessage"] = "You need at least 5 points to convert.";
                return RedirectToPage();
            }

            var totalPoints = balance.Points;
            var pointsToConvert = (totalPoints / 5) * 5;
            var creditToReceive = pointsToConvert / 5.0;

            var updateBalance = await _httpClient.PutAsJsonAsync("api/Balance",
                new UpdateBalance
                {
                    Id = balance.Id,
                    UserId = userId,
                    Points = balance.Points - pointsToConvert,
                    Credit = balance.Credit + creditToReceive
                }
            );

            if (!updateBalance.IsSuccessStatusCode)
            {
                var errorContent = await updateBalance.Content.ReadAsStringAsync();
                _logger.LogError($"API error: {updateBalance.StatusCode}, Content: {errorContent}");
                TempData["ErrorMessage"] = $"Failed to update balance. Error: {errorContent}";
                await LoadAllData();
                return Page();
            }

            var createHistoryCredit = await _httpClient.PostAsJsonAsync("api/HistoryCredit",
                new CreateHistoryCredit
                {
                    UserId = userId,
                    BalanceId = balance.Id,
                    CreatedAt = DateTime.UtcNow,
                    Credit = creditToReceive,
                    TransactionType = CreditTransactionType.BonusCredit.ToString()
                }
            );

            if (!createHistoryCredit.IsSuccessStatusCode)
            {
                var errorContent = await createHistoryCredit.Content.ReadAsStringAsync();
                _logger.LogError($"API error: {createHistoryCredit.StatusCode}, Content: {errorContent}");
                TempData["ErrorMessage"] = $"Failed to create history credit. Error: {errorContent}";
                await LoadAllData();
                return Page();
            }

            var createHistoryPoint = await _httpClient.PostAsJsonAsync("api/HistoryPoint",
                new CreateHistoryPoint
                {
                    UserId = userId,
                    BalanceId = balance.Id,
                    CreatedAt = DateTime.UtcNow,
                    Point = pointsToConvert,
                    TransactionType = PointTransactionType.Redeemed.ToString()
                }
            );

            if (!createHistoryPoint.IsSuccessStatusCode)
            {
                var errorContent = await createHistoryPoint.Content.ReadAsStringAsync();
                _logger.LogError($"API error: {createHistoryPoint.StatusCode}, Content: {errorContent}");
                TempData["ErrorMessage"] = $"Failed to create history point. Error: {errorContent}";
                await LoadAllData();
                return Page();
            }

            return RedirectToPage("Wallet");
        }

        public async Task<IActionResult> OnPostDeposit()
        {
            if (!ModelState.IsValid)
            {
                await LoadAllData();
                return Page();
            }

            double cost = Input.CreditAmount;
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

                var balance = await _httpClient.GetFromJsonAsync<Balance>($"api/Balance/{userId}");

                if (balance == null)
                {
                    _logger.LogWarning("No balance record found for user {UserId}", userId);
                    return RedirectToPage("Wallet");
                }

                var createResponse = await _httpClient.PostAsJsonAsync("api/HistoryCredit",
                    new CreateHistoryCredit
                    {
                        UserId = userId,
                        Credit = double.Parse(res.PurchaseUnits.First().Amount.Value, CultureInfo.InvariantCulture),
                        TransactionType = CreditTransactionType.Deposit.ToString(),
                        CreatedAt = DateTime.UtcNow,
                        BalanceId = balance.Id
                    }
                );

                if (!createResponse.IsSuccessStatusCode)
                {
                    var errorContent = await createResponse.Content.ReadAsStringAsync();
                    _logger.LogError($"API error: {createResponse.StatusCode}, Content: {errorContent}");
                    TempData["ErrorMessage"] = $"Failed to add histiry credit. Error: {errorContent}";
                }

                var updateResponse = await _httpClient.PutAsJsonAsync("api/Balance",
                    new UpdateBalance
                    {
                        Id = balance.Id,
                        Credit = balance.Credit + double.Parse(res.PurchaseUnits.First().Amount.Value, CultureInfo.InvariantCulture),
                        Points = balance.Points,
                        UserId = userId
                    }
                );

                if (!updateResponse.IsSuccessStatusCode)
                {
                    var errorContent = await updateResponse.Content.ReadAsStringAsync();
                    _logger.LogError($"API error: {updateResponse.StatusCode}, Content: {errorContent}");
                    TempData["ErrorMessage"] = $"Failed to update balance. Error: {errorContent}";
                }
            }

            return RedirectToPage("Wallet");
        }

        public async Task OnGetAsync()
        {
            await LoadAllData();
        }

        private async Task LoadAllData()
        {
            try
            {
                var userId = _userManager.GetUserId(User);

                var userBalanceResponse = await _httpClient.GetFromJsonAsync<Balance>($"api/Balance/{userId}");
                var historyCreditresponse = await _httpClient.GetFromJsonAsync<List<HistoryCredit>>($"api/HistoryCredit/AllHistoryCredits/{userId}");

                UserBalance = userBalanceResponse;
                HistoryCredit = historyCreditresponse ?? [];
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading data");
            }
        }
    }
}
