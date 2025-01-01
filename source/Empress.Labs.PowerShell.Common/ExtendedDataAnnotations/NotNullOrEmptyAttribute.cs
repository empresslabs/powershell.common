// Copyright (c) Bruno Sales <me@baliestri.dev>. Licensed under the MIT License.
// See the LICENSE file in the repository root for full license text.

using System.Collections;
using System.ComponentModel.DataAnnotations;

namespace Empress.Labs.PowerShell.Common.ExtendedDataAnnotations;

/// <summary>
///   Represents a validation attribute that ensures the value is not null or empty.
/// </summary>
public sealed class NotNullOrEmptyAttribute : ValidationAttribute {
  /// <inheritdoc />
  public override bool IsValid(object? value)
    => value switch {
      string str => !string.IsNullOrEmpty(str),
      Array array => array.Length > 0,
      ICollection collection => collection.Count > 0,
      var _ => false
    };

  /// <inheritdoc />
  public override string FormatErrorMessage(string name)
    => $"The {name} field must not be null or empty.";
}
