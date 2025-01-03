// Copyright (c) Bruno Sales <me@baliestri.dev>. Licensed under the MIT License.
// See the LICENSE file in the repository root for full license text.

using Empress.Labs.PowerShell.Common.EndToEndTesting.Cmdlets;
using Empress.Labs.PowerShell.Common.EndToEndTesting.Common;
using Empress.Labs.PowerShell.TestTools;

namespace Empress.Labs.PowerShell.Common.EndToEndTesting;

[TestFixture]
[TestOf(typeof(PSCmdletProxy))]
public sealed class PSCmdletProxyTest {
  [Test]
  public void PSCmdletProxy_ShouldHaveTargetCmdlet() {
    // Act
    var proxy = new SetLocationCmdlet();

    // Assert
    Assert.That(proxy.TargetCmdlet, Is.EqualTo(DefinedProxies.SET_LOCATION));
  }

  [Test]
  public void PSCmdletProxy_ShouldWorkNormally() {
    // Arrange
    var targetLocation = OperatingSystem.IsWindows()
      ? "C:\\"
      : "/tmp";

    // Act
    var result = PSCmdletInvoker.Invoke<SetLocationCmdlet>(prepare => prepare
      .WithParameter("Path", targetLocation));

    // Assert
    Assert.That(result, Is.Not.Null);
    Assert.That(result.BaseObject, Contains.Substring($"and the current location is '{targetLocation}'"));
  }
}
