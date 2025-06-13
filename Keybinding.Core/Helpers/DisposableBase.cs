// Copyright (c) ktsu.dev
// All rights reserved.
// Licensed under the MIT license.

namespace ktsu.Keybinding.Core.Helpers;

/// <summary>
/// Base class for disposable objects that provides common disposal functionality
/// </summary>
public abstract class DisposableBase : IDisposable
{
	/// <summary>
	/// Gets a value indicating whether this instance has been disposed
	/// </summary>
	protected bool IsDisposed { get; private set; }

	/// <summary>
	/// Throws an ObjectDisposedException if this instance has been disposed
	/// </summary>
	/// <exception cref="ObjectDisposedException">Thrown when the object has been disposed</exception>
	protected void ThrowIfDisposed() => ObjectDisposedException.ThrowIf(IsDisposed, this);

	/// <summary>
	/// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources
	/// </summary>
	public void Dispose()
	{
		if (!IsDisposed)
		{
			Dispose(true);
			GC.SuppressFinalize(this);
			IsDisposed = true;
		}
	}

	/// <summary>
	/// Releases managed and unmanaged resources
	/// </summary>
	/// <param name="disposing">True if disposing managed resources, false otherwise</param>
	protected virtual void Dispose(bool disposing)
	{
		// Override in derived classes to add cleanup logic
	}
}
