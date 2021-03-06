﻿using Outlook.Models.Data;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace Outlook.Models.Attributes.Validation
{
    public class VolumeNumberUniqueness : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var context = (OutlookContext)validationContext.GetService(typeof(OutlookContext));
            var existingVolumeWithSameName = context.Volume.SingleOrDefault(v => v.Number.ToString() == value.ToString());

            if (existingVolumeWithSameName != null)
            {
                return new ValidationResult(GetErrorMessage(value.ToString()));
            }
            return ValidationResult.Success;
        }

        private string GetErrorMessage(string v) => $"Volume Number {v} already exists.";
    }
}
