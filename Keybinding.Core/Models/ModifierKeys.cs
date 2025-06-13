// Copyright (c) ktsu.dev
// All rights reserved.
// Licensed under the MIT license.

namespace ktsu.Keybinding.Core.Models;

/// <summary>
/// Represents modifier keys that can be combined with a primary key
/// </summary>
[Flags]
public enum ModifierKeys
{
	/// <summary>
	/// No modifier keys
	/// </summary>
	None = 0,

	/// <summary>
	/// Control key modifier
	/// </summary>
	Ctrl = 1,

	/// <summary>
	/// Alt key modifier
	/// </summary>
	Alt = 2,

	/// <summary>
	/// Shift key modifier
	/// </summary>
	Shift = 4,

	/// <summary>
	/// Windows/Meta key modifier
	/// </summary>
	Meta = 8
}
