// Copyright (c) ktsu.dev
// All rights reserved.
// Licensed under the MIT license.

namespace ktsu.Keybinding.Test.Helpers;

using Microsoft.VisualStudio.TestTools.UnitTesting;
using ktsu.Keybinding.Core.Helpers;

[TestClass]
public class DisposalHelperTests
{
	private readonly object _testInstance = new();

	[TestMethod]
	public void ThrowIfDisposed_WhenNotDisposed_DoesNotThrow()
	{
		// Arrange
		bool disposed = false;

		// Act & Assert
		DisposalHelper.ThrowIfDisposed(disposed, _testInstance);
		// No exception should be thrown
	}

	[TestMethod]
	public void ThrowIfDisposed_WhenDisposed_ThrowsObjectDisposedException()
	{
		// Arrange
		bool disposed = true;

		// Act & Assert
		ObjectDisposedException exception = Assert.ThrowsException<ObjectDisposedException>(
			() => DisposalHelper.ThrowIfDisposed(disposed, _testInstance));

		Assert.IsNotNull(exception);
		Assert.AreEqual(_testInstance.GetType().FullName, exception.ObjectName);
	}

	[TestMethod]
	public void ThrowIfDisposed_WithNullInstance_ThrowsObjectDisposedException()
	{
		// Arrange
		bool disposed = true;
		object? nullInstance = null;

		// Act & Assert
		ObjectDisposedException exception = Assert.ThrowsException<ObjectDisposedException>(
			() => DisposalHelper.ThrowIfDisposed(disposed, nullInstance!));

		Assert.IsNotNull(exception);
	}

	[TestMethod]
	public void ThrowIfDisposed_WithDifferentInstanceTypes_WorksCorrectly()
	{
		// Arrange
		bool disposed = true;
		string stringInstance = "test";
		int intInstance = 42;

		// Act & Assert
		ObjectDisposedException stringException = Assert.ThrowsException<ObjectDisposedException>(
			() => DisposalHelper.ThrowIfDisposed(disposed, stringInstance));
		Assert.AreEqual(typeof(string).FullName, stringException.ObjectName);

		ObjectDisposedException intException = Assert.ThrowsException<ObjectDisposedException>(
			() => DisposalHelper.ThrowIfDisposed(disposed, intInstance));
		Assert.AreEqual(typeof(int).FullName, intException.ObjectName);
	}
}
