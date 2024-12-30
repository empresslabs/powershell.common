// Copyright (c) Bruno Sales <me@baliestri.dev>. Licensed under the MIT License.
// See the LICENSE file in the repository root for full license text.

using System.ComponentModel.DataAnnotations;

namespace Empress.Labs.PowerShell.Common.ExtendedDataAnnotations;

/// <summary>
///   Provides a set of utility methods for working with data annotations.
/// </summary>
public static class Prelude {
  /// <summary>
  ///   Validates the instance or throws an exception if the validation fails.
  /// </summary>
  /// <param name="instance">The instance to validate.</param>
  /// <exception cref="AggregateException">Thrown when the validation fails.</exception>
  public static void ValidateOrThrow(object instance) {
    var context = new ValidationContext(instance);
    var validationResult = new List<ValidationResult>();

    if (!Validator.TryValidateObject(instance, context, validationResult, true)) {
      throw new AggregateException(validationResult.Select(result => new ValidationException(result.ErrorMessage)));
    }
  }
}
