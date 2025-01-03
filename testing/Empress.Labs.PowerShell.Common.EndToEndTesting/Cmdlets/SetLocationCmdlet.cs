// Copyright (c) Bruno Sales <me@baliestri.dev>. Licensed under the MIT License.
// See the LICENSE file in the repository root for full license text.

using System.Management.Automation;
using Empress.Labs.PowerShell.Common.EndToEndTesting.Common;

namespace Empress.Labs.PowerShell.Common.EndToEndTesting.Cmdlets;

[CmdletProxy(DefinedProxies.SET_LOCATION)]
[Cmdlet(VerbsCommon.Set, "Location", DefaultParameterSetName = PATH_PARAMETER_SET, HelpUri = "https://go.microsoft.com/fwlink/?LinkID=2097049")]
public sealed class SetLocationCmdlet : PSCmdletProxy {
  private const string PATH_PARAMETER_SET = "Path";
  private const string LITERAL_PATH_PARAMETER_SET = "LiteralPath";
  private const string STACK_PARAMETER_SET = "Stack";

  /// <summary>
  ///   Specify the path of a new working location. If no path is provided, Set-Location defaults to the current user's home directory. When wildcards are
  ///   used, the cmdlet chooses the container (directory, registry key, certificate store) that matches the wildcard pattern. If the wildcard pattern
  ///   matches more than one container, the cmdlet returns an error.
  /// </summary>
  /// <remarks>
  ///   PowerShell keeps a history of the last 20 locations you have set. If the Path parameter value is the - character, then the new working location
  ///   will be the previous working location in history (if it exists). Similarly, if the value is the + character, then the new working location will be
  ///   the next working location in history (if it exists). This is similar to using <c>Pop-Location</c> and <c>Push-Location</c> except that the history
  ///   is a list, not a stack, and is implicitly tracked, not manually controlled. There is no way to view the history list.
  /// </remarks>
  [Parameter(Position = 0, ParameterSetName = PATH_PARAMETER_SET, ValueFromPipeline = true, ValueFromPipelineByPropertyName = true)]
  [Alias("PSPath", "LP")]
  [ValidateNotNullOrEmpty]
  public string Path { get; init; } = string.Empty;

  /// <summary>
  ///   Specifies a path of the location. The value of the LiteralPath parameter is used exactly as it is typed. No characters are interpreted as wildcard
  ///   characters. If the path includes escape characters, enclose it in single quotation marks. Single quotation marks tell PowerShell not to interpret
  ///   any characters as escape sequences.
  /// </summary>
  [Parameter(ParameterSetName = LITERAL_PATH_PARAMETER_SET, ValueFromPipelineByPropertyName = true)]
  [ValidateNotNullOrEmpty]
  public string LiteralPath { get; init; } = null!;

  /// <summary>
  ///   Returns a PathInfo object that represents the location. By default, this cmdlet does not generate any output.
  /// </summary>
  [Parameter]
  public SwitchParameter PassThru { get; set; }

  /// <summary>
  ///   Specifies an existing location stack name that this cmdlet makes the current location stack. Enter a location stack name. To indicate the unnamed
  ///   default location stack, type <c>$null</c> or an empty string (<c>""</c>).
  /// </summary>
  /// <remarks>
  ///   Using this parameter does not change the current location. It only changes the stack used by the <c>*-Location</c> cmdlets. The <c>*-Location</c>
  ///   cmdlets act on the current stack unless you use the StackName parameter to specify a different stack. For more information about location stacks,
  ///   see the Notes section.
  /// </remarks>
  /// <seealso href="https://learn.microsoft.com/en-us/powershell/module/microsoft.powershell.management/set-location?view=powershell-7.4#notes">Notes</seealso>
  [Parameter(ParameterSetName = STACK_PARAMETER_SET, ValueFromPipelineByPropertyName = true)]
  public string StackName { get; init; } = null!;

  /// <inheritdoc />
  protected override void OnEndProcessing(Writer writer) {
    var formattedDate = DateTime.Now.ToString("G");
    var pwd = SessionState.Path.CurrentLocation.Path;

    writer.Object($"The current date is {formattedDate} and the current location is '{pwd}'");
  }
}
