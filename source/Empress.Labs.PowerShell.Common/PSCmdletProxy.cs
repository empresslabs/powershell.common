// Copyright (c) Bruno Sales <me@baliestri.dev>. Licensed under the MIT License.
// See the LICENSE file in the repository root for full license text.

using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace Empress.Labs.PowerShell.Common;

/// <summary>
///   Base class for proxying existing PowerShell cmdlets.
/// </summary>
[ExcludeFromCodeCoverage]
[SuppressMessage("ReSharper", "InconsistentNaming")]
public abstract class PSCmdletProxy : PSCmdlet, IDynamicParameters, IDisposable {
  /// <summary>
  ///   The target cmdlet to proxy.
  /// </summary>
  public readonly string TargetCmdlet;

  protected PSCmdletProxy() {
    var attribute = GetType().GetCustomAttribute<CmdletProxyAttribute>();

    if (attribute is null) {
      throw new InvalidOperationException("It's necessary to define the target cmdlet with the CmdletProxyAttribute.");
    }

    TargetCmdlet = attribute.TargetCmdlet;
  }

  /// <summary>
  ///   The steppable pipeline of the cmdlet.
  /// </summary>
  protected SteppablePipeline? SteppablePipeline { get; set; }

  /// <inheritdoc />
  public void Dispose() {
    GC.SuppressFinalize(this);
    SteppablePipeline?.Dispose();
  }

  /// <inheritdoc />
  public object GetDynamicParameters() {
    var commandInfo = InvokeCommand.GetCommand(TargetCmdlet, CommandTypes.Cmdlet, MyInvocation.BoundParameters.Values.ToArray());
    var dynamicParameters = commandInfo.Parameters.Where(pair => pair.Value.IsDynamic).ToList();

    if (dynamicParameters.Count == 0) {
      return new object();
    }

    var runtimeDefinedParameterDictionary = new RuntimeDefinedParameterDictionary();

    foreach (var (key, parameterMetadata) in dynamicParameters) {
      if (MyInvocation.MyCommand.Parameters.ContainsKey(key)) {
        continue;
      }

      var runtimeDefinedParameter = new RuntimeDefinedParameter(parameterMetadata.Name, parameterMetadata.ParameterType,
        parameterMetadata.Attributes);
      runtimeDefinedParameterDictionary.Add(parameterMetadata.Name, runtimeDefinedParameter);
    }

    return OnGetDynamicParameters(runtimeDefinedParameterDictionary);
  }

  /// <summary>
  ///   Method to be called when the cmdlet is initialized.
  /// </summary>
  /// <param name="runtimeDefinedParameterDictionary">The runtime defined parameters dictionary.</param>
  /// <returns>The runtime defined parameters dictionary.</returns>
  protected virtual object OnGetDynamicParameters(RuntimeDefinedParameterDictionary runtimeDefinedParameterDictionary)
    => runtimeDefinedParameterDictionary;

  /// <summary>
  ///   The parameters to pass when processing the <see cref="SteppablePipeline" />.
  /// </summary>
  /// <returns>A object with the parameters.</returns>
  protected virtual object PipelineDefinedParameters()
    => GetParameters(this);

  /// <summary>
  ///   Action to be executed together with <see cref="BeginProcessing" /> method.
  /// </summary>
  protected virtual void OnBeginProcessing(Writer writer) { }

  /// <summary>
  ///   Action to be executed together with <see cref="ProcessRecord" /> method.
  /// </summary>
  protected virtual void OnProcessRecord(Writer writer) { }

  /// <summary>
  ///   Action to be executed together with <see cref="EndProcessing" /> method.
  /// </summary>
  protected virtual void OnEndProcessing(Writer writer) { }

  /// <inheritdoc />
  protected sealed override void BeginProcessing() {
    if (MyInvocation.BoundParameters.TryGetValue("OutBuffer", out var _)) {
      MyInvocation.BoundParameters["OutBuffer"] = 1;
    }

    var commandInfo = InvokeCommand.GetCommand(TargetCmdlet, CommandTypes.Cmdlet);
    var scriptBlock = ScriptBlock.Create("& $CommandInfo @BoundParameters @UnboundArguments");
    SessionState.PSVariable.Set("CommandInfo", commandInfo);
    SessionState.PSVariable.Set("UnboundArguments", MyInvocation.UnboundArguments);
    SessionState.PSVariable.Set("BoundParameters", MyInvocation.BoundParameters);

    SteppablePipeline = scriptBlock.GetSteppablePipeline(MyInvocation.CommandOrigin);

    SteppablePipeline.Begin(this);
    OnBeginProcessing(new Writer {
      Object = WriteObject,
      Information = WriteInformation,
      Error = WriteError,
      Verbose = WriteVerbose,
      Warning = WriteWarning,
      Debug = WriteDebug,
      Progress = WriteProgress,
      CommandDetail = WriteCommandDetail
    });
  }

  /// <inheritdoc />
  protected sealed override void ProcessRecord() {
    SteppablePipeline?.Process(PipelineDefinedParameters());
    OnProcessRecord(new Writer {
      Object = WriteObject,
      Information = WriteInformation,
      Error = WriteError,
      Verbose = WriteVerbose,
      Warning = WriteWarning,
      Debug = WriteDebug,
      Progress = WriteProgress,
      CommandDetail = WriteCommandDetail
    });
  }

  /// <inheritdoc />
  protected sealed override void EndProcessing() {
    SteppablePipeline?.End();
    OnEndProcessing(new Writer {
      Object = WriteObject,
      Information = WriteInformation,
      Error = WriteError,
      Verbose = WriteVerbose,
      Warning = WriteWarning,
      Debug = WriteDebug,
      Progress = WriteProgress,
      CommandDetail = WriteCommandDetail
    });
  }

  /// <inheritdoc />
  protected sealed override void StopProcessing()
    => Dispose();

  /// <summary>
  ///   Get the parameters of a cmdlet.
  /// </summary>
  /// <param name="cmdlet">The cmdlet.</param>
  /// <typeparam name="TCmdlet">The type of the cmdlet.</typeparam>
  /// <returns>A <see cref="PSObject" /> with the parameters of the cmdlet.</returns>
  protected static PSObject GetParameters<TCmdlet>(TCmdlet cmdlet) where TCmdlet : PSCmdlet {
    var psobject = new PSObject();
    var properties = typeof(TCmdlet)
      .GetProperties()
      .Where(property => property.GetCustomAttribute<ParameterAttribute>() is not null)
      .Select(property => new PSNoteProperty(property.Name, property.GetValue(cmdlet)))
      .ToArray();

    foreach (var property in properties) {
      psobject.Properties.Add(property);
    }

    return psobject;
  }

  /// <summary>
  ///   Get a parameter from a cmdlet.
  /// </summary>
  /// <param name="cmdlet">The cmdlet.</param>
  /// <param name="parameter">The parameter capturing function.</param>
  /// <typeparam name="TCmdlet">The type of the cmdlet.</typeparam>
  /// <typeparam name="TOut">The type of the parameter.</typeparam>
  /// <returns>The parameter.</returns>
  protected static TOut GetParameter<TCmdlet, TOut>(TCmdlet cmdlet, Func<TCmdlet, TOut> parameter) where TCmdlet : PSCmdlet
    => parameter(cmdlet);

  /// <summary>
  ///   The writer to be used in the cmdlet.
  /// </summary>
  public readonly record struct Writer {
    /// <summary>
    ///   Delegate to write an error record.
    /// </summary>
    public delegate void WriteError(ErrorRecord errorRecord);

    /// <summary>
    ///   Delegate to write an information record.
    /// </summary>
    public delegate void WriteInformation(InformationRecord informationRecord);

    /// <summary>
    ///   Delegate to write an object.
    /// </summary>
    public delegate void WriteObject(object sendToPipeline, bool enumerateCollection = false);

    /// <summary>
    ///   Delegate to write a progress record.
    /// </summary>
    public delegate void WriteProgress(ProgressRecord progressRecord);

    /// <summary>
    ///   Delegate to write a text.
    /// </summary>
    public delegate void WriteText(string text);

    /// <summary>
    ///   Writes an object to the output of the cmdlet.
    /// </summary>
    public WriteObject Object { get; init; }

    /// <summary>
    ///   Writes an information record to the output of the cmdlet.
    /// </summary>
    public WriteInformation Information { get; init; }

    /// <summary>
    ///   Writes an error record to the output of the cmdlet.
    /// </summary>
    public WriteError Error { get; init; }

    /// <summary>
    ///   Writes a verbose text to the output of the cmdlet.
    /// </summary>
    public WriteText Verbose { get; init; }

    /// <summary>
    ///   Writes a warning text to the output of the cmdlet.
    /// </summary>
    public WriteText Warning { get; init; }

    /// <summary>
    ///   Writes a debug text to the output of the cmdlet.
    /// </summary>
    public WriteText Debug { get; init; }

    /// <summary>
    ///   Writes a progress record to the output of the cmdlet.
    /// </summary>
    public WriteProgress Progress { get; init; }

    /// <summary>
    ///   Writes a command detail text to the output of the cmdlet.
    /// </summary>
    public WriteText CommandDetail { get; init; }
  }
}
