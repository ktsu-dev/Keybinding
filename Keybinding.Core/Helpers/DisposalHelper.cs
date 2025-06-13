// Copyright (c) ktsu.dev
// All rights reserved.
// Licensed under the MIT license.

namespace ktsu.Keybinding.Core.Helpers;

/// <summary>
/// Helper class for common disposal operations to eliminate repeated code
/// </summary>
internal static class DisposalHelper
{
	/// <summary>
	/// Throws an ObjectDisposedException if the object has been disposed
	/// </summary>
	/// <param name="disposed">The disposed flag</param>
	/// <param name="instance">The object instance</param>
	/// <exception cref="ObjectDisposedException">Thrown when the object has been disposed</exception>
	public static void ThrowIfDisposed(bool disposed, object instance) => ObjectDisposedException.ThrowIf(disposed, instance);
}
