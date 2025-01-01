// Copyright (c) Bruno Sales <me@baliestri.dev>. Licensed under the MIT License.
// See the LICENSE file in the repository root for full license text.

using System.Diagnostics.CodeAnalysis;

namespace Empress.Labs.PowerShell.Common.Extensions;

/// <summary>
///   Extensions for <see cref="Task{TResult}" />.
/// </summary>
[ExcludeFromCodeCoverage]
public static class TaskExtensions {
  /// <summary>
  ///   Cancels a task if a cancellation token is requested.
  /// </summary>
  /// <param name="task">The task.</param>
  /// <param name="cancellationToken">A <see cref="CancellationToken" /> to observe while waiting for the task to complete.</param>
  /// <typeparam name="TResult">The result type of the task.</typeparam>
  /// <returns>A task that represents the asynchronous operation.</returns>
  public static async Task<TResult> WithCancellationAsync<TResult>(this Task<TResult> task, CancellationToken cancellationToken) {
    var cancellationTask = cancellationToken.IsCancellationRequested
      ? Task.FromCanceled<TResult>(cancellationToken)
      : Task.Delay(Timeout.Infinite, cancellationToken);

    var completedTask = await Task.WhenAny(task, cancellationTask);

    if (completedTask == cancellationTask) {
      cancellationToken.ThrowIfCancellationRequested();
    }

    return await task;
  }

  /// <summary>
  ///   Cancels a task if a cancellation token is requested.
  /// </summary>
  /// <param name="task">The task.</param>
  /// <param name="cancellationToken">A <see cref="CancellationToken" /> to observe while waiting for the task to complete.</param>
  public static async Task WithCancellationAsync(this Task task, CancellationToken cancellationToken) {
    var cancellationTask = cancellationToken.IsCancellationRequested
      ? Task.FromCanceled(cancellationToken)
      : Task.Delay(Timeout.Infinite, cancellationToken);

    var completedTask = await Task.WhenAny(task, cancellationTask);

    if (completedTask == cancellationTask) {
      cancellationToken.ThrowIfCancellationRequested();
    }

    await task;
  }
}
