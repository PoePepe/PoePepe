using System.Text.RegularExpressions;
using Poe.UI.ViewModels;
// using ReactiveValidation;
// using ReactiveValidation.Extensions;

namespace Poe.UI.Validators.Fluent;

public class OrderViewModelValidator 
    // : ValidationRuleBuilder<OrderViewModel>
{
    private const string PoeTradeLinkPattern =
        @"^(https://www\.pathofexile\.com/trade/search/)[a-zA-Z]+\/[a-zA-Z0-9]+$";
    public OrderViewModelValidator()
    {
        // RuleFor(x => x.Name).NotEmpty().MaxLength(30);
        // RuleFor(x => x.QueryLink).NotEmpty().Matches(PoeTradeLinkPattern, RegexOptions.Compiled);
    }
}