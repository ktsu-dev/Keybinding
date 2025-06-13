// Copyright (c) ktsu.dev
// All rights reserved.
// Licensed under the MIT license.

namespace ktsu.Keybinding.Core;

/// <summary>
/// Contains shared constants used throughout the Keybinding library
/// </summary>
internal static class Constants
{
	/// <summary>
	/// Common error messages
	/// </summary>
	internal static class ErrorMessages
	{
		/// <summary>
		/// Error message for null or whitespace profile ID
		/// </summary>
		internal const string ProfileIdNullOrWhitespace = "Profile ID cannot be null or whitespace";

		/// <summary>
		/// Error message for null or whitespace command ID
		/// </summary>
		internal const string CommandIdNullOrWhitespace = "Command ID cannot be null or whitespace";

		/// <summary>
		/// Error message for null or whitespace data directory
		/// </summary>
		internal const string DataDirectoryNullOrWhitespace = "Data directory cannot be null or whitespace";

		/// <summary>
		/// Error message for null or whitespace search term
		/// </summary>
		internal const string SearchTermNullOrWhitespace = "Search term cannot be null or whitespace";

		/// <summary>
		/// Error message for null or whitespace new name
		/// </summary>
		internal const string NewNameNullOrWhitespace = "New name cannot be null or whitespace";

		/// <summary>
		/// Error message for null or whitespace profile name
		/// </summary>
		internal const string ProfileNameNullOrWhitespace = "New profile name cannot be null or whitespace";

		/// <summary>
		/// Error message for null or whitespace source profile ID
		/// </summary>
		internal const string SourceProfileIdNullOrWhitespace = "Source profile ID cannot be null or whitespace";

		/// <summary>
		/// Error message for null or whitespace target profile ID
		/// </summary>
		internal const string TargetProfileIdNullOrWhitespace = "Target profile ID cannot be null or whitespace";

		/// <summary>
		/// Error message for null or whitespace new profile ID
		/// </summary>
		internal const string NewProfileIdNullOrWhitespace = "New profile ID cannot be null or whitespace";
	}

	/// <summary>
	/// File-related constants
	/// </summary>
	internal static class Files
	{
		/// <summary>
		/// Filename for storing profiles
		/// </summary>
		internal const string ProfilesFileName = "profiles.json";

		/// <summary>
		/// Filename for storing commands
		/// </summary>
		internal const string CommandsFileName = "commands.json";

		/// <summary>
		/// Filename for storing active profile
		/// </summary>
		internal const string ActiveProfileFileName = "active-profile.json";
	}
}
