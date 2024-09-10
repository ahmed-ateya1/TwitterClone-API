using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SocialMediaApp.Core.Helper
{
    /// <summary>
    /// Helper class for model validation.
    /// </summary>
    public static class ValidationHelper
    {
        /// <summary>
        /// Validates the given model object.
        /// </summary>
        /// <param name="model">The model object to validate.</param>
        /// <exception cref="ArgumentNullException">Thrown when the model object is null.</exception>
        /// <exception cref="ValidationException">Thrown when the model object is not valid.</exception>
        public static void ValidateModel(object? model)
        {
            if (model == null)
            {
                throw new ArgumentNullException(nameof(model));
            }
            var validationContext = new ValidationContext(model);
            var validationResults = new List<ValidationResult>();

            var isValid = Validator.TryValidateObject(model, validationContext, validationResults, true);

            if (!isValid)
                throw new ValidationException("Model is not valid");

        }
    }
}
