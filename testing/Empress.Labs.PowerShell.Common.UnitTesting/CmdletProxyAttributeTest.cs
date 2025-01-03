// Copyright (c) Bruno Sales <me@baliestri.dev>. Licensed under the MIT License.
// See the LICENSE file in the repository root for full license text.

namespace Empress.Labs.PowerShell.Common.UnitTesting;

[TestFixture]
[TestOf(typeof(CmdletProxyAttribute))]
public sealed class CmdletProxyAttributeTest {
  [Test]
  public void ProxyCmdletAttribute_ShouldHaveTargetCmdlet() {
    // Arrange
    const string TARGET_CMDLET = "Microsoft.PowerShell.Management\\Set-Location";

    // Act
    var attribute = new CmdletProxyAttribute(TARGET_CMDLET);

    // Assert
    Assert.That(attribute.TargetCmdlet, Is.EqualTo(TARGET_CMDLET));
  }

  [Test]
  public void ProxyCmdletAttribute_ShouldThrowException_WhenTargetCmdletIsNull() {
    // Arrange
    var targetCmdlet = string.Empty;

    // Assert
    Assert.That(Act, Throws.ArgumentNullException);
    return;

    // Act
    void Act() => _ = new CmdletProxyAttribute(targetCmdlet);
  }
}
