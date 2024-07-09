using System.Collections.Generic;
using System;
using System.ComponentModel.DataAnnotations;

namespace DiningHub.Models
{
    public class CheckoutViewModel
    {
        public List<ShoppingCartItem> CartItems { get; set; }
        public decimal TotalAmount { get; set; }

        [Required(ErrorMessage = "Please select a payment method.")]
        public string PaymentMethod { get; set; }

        // Credit/Debit Card
        [RequiredIf("PaymentMethod", "CreditCard", ErrorMessage = "Credit card number is required.")]
        [CreditCard(ErrorMessage = "Invalid credit card number.")]
        public string CreditCardNumber { get; set; }

        [RequiredIf("PaymentMethod", "CreditCard", ErrorMessage = "Expiration date is required.")]
        [RegularExpression(@"\d{2}/\d{2}", ErrorMessage = "Expiration date must be in the format MM/YY.")]
        public string ExpirationDate { get; set; }

        [RequiredIf("PaymentMethod", "CreditCard", ErrorMessage = "CVV is required.")]
        [RegularExpression(@"\d{3,4}", ErrorMessage = "CVV must be a 3 or 4 digit number.")]
        public string CVV { get; set; }

        // Online Banking
        [RequiredIf("PaymentMethod", "OnlineBanking", ErrorMessage = "Bank name is required.")]
        public string BankName { get; set; }

        [RequiredIf("PaymentMethod", "OnlineBanking", ErrorMessage = "Account number is required.")]
        public string AccountNumber { get; set; }
    }

    public class RequiredIfAttribute : ValidationAttribute
    {
        private readonly string _dependentProperty;
        private readonly string _targetValue;

        public RequiredIfAttribute(string dependentProperty, string targetValue)
        {
            _dependentProperty = dependentProperty;
            _targetValue = targetValue;
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var property = validationContext.ObjectType.GetProperty(_dependentProperty);
            if (property == null)
            {
                return new ValidationResult($"Unknown property: {_dependentProperty}");
            }

            var dependentValue = property.GetValue(validationContext.ObjectInstance, null)?.ToString();
            if (dependentValue == _targetValue && string.IsNullOrEmpty(value?.ToString()))
            {
                return new ValidationResult(ErrorMessage);
            }

            return ValidationResult.Success;
        }
    }
}
