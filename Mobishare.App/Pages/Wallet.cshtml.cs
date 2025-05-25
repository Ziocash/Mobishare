using System.Globalization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PayPal.REST.Client;
using PayPal.REST.Models.Orders;

namespace Mobishare.App.Pages
{
    public class WalletModel : PageModel
    {
        private readonly IPayPalClient _payPalClient;

        public WalletModel(IPayPalClient payPalClient)
        {
            _payPalClient = payPalClient ?? throw new ArgumentNullException(nameof(payPalClient));
        }

        public async Task<IActionResult> OnPostDeposit()
        {
            var cost = 20;

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

            return Page();
        }
        public void OnGet()
        {
        }
    }
}
