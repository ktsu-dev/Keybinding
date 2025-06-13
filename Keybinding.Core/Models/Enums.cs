// Copyright (c) ktsu.dev
// All rights reserved.
// Licensed under the MIT license.

namespace ktsu.Keybinding.Core.Models;

/// <summary>
/// Specifies the case sensitivity for search operations
/// </summary>
public enum CaseSensitivity
{
	/// <summary>
	/// Case insensitive search
	/// </summary>
	CaseInsensitive = 0,

	/// <summary>
	/// Case sensitive search
	/// </summary>
	CaseSensitive = 1
}

/// <summary>
/// Specifies how to handle existing entries when copying
/// </summary>
public enum OverwriteMode
{
	/// <summary>
	/// Skip existing entries without overwriting
	/// </summary>
	SkipExisting = 0,

	/// <summary>
	/// Overwrite existing entries
	/// </summary>
	OverwriteExisting = 1
}

/// <summary>
/// Specifies whether to activate a profile after creation
/// </summary>
public enum ProfileActivation
{
	/// <summary>
	/// Do not activate the profile
	/// </summary>
	DoNotActivate = 0,

	/// <summary>
	/// Activate the profile
	/// </summary>
	Activate = 1
}
