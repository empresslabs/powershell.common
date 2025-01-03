// Copyright (c) Bruno Sales <me@baliestri.dev>. Licensed under the MIT License.
// See the LICENSE file in the repository root for full license text.

namespace Empress.Labs.PowerShell.Common;

/// <summary>
///   Attribute to define the target cmdlet to be proxied.
/// </summary>
/// <param name="targetCmdlet">The target cmdlet to be proxied.</param>
[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public sealed class CmdletProxyAttribute(string targetCmdlet) : Attribute {
  /// <summary>
  ///   The target cmdlet to be proxied.
  /// </summary>
  public string TargetCmdlet { get; } = !string.IsNullOrEmpty(targetCmdlet)
    ? targetCmdlet
    : throw new ArgumentNullException(nameof(targetCmdlet), "The target cmdlet must be defined.");
}
