using FluentValidation;
using Zadanie7.DTOs;

namespace Zadanie7.Validators;

public class ProductWarehouseValidator : AbstractValidator<ProductWarehouseDTO>
{
    public ProductWarehouseValidator()
    {
        RuleFor(x => x.IdProduct).NotEmpty().WithMessage("IdProduct is required");
        RuleFor(x => x.IdWarehouse).NotEmpty().WithMessage("IdWarehouse is required");
        RuleFor(x => x.Amount).NotEmpty().WithMessage("Amount is required")
            .GreaterThan(0).WithMessage("Amount must be greater than 0")
            .WithMessage("Amount must be an integer");
    }
}