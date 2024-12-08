# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/), and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

## [0.2.0] - 2024-12-07

### Changed

- Migrate from self-hosted UI to [StardewUI](https://github.com/focustense/StardewUI/releases) (Framework).

### Fixed

- Settings menu closes properly when opened from Generic Mod Config Menu v1.14.0 and up.
- Settings menu _opens_ properly again from GMCM v1.14.1+.
- Pressing one of the cancel buttons (<kbd>ESC</kbd>, <kbd>E</kbd>, Controller B) should no longer dismiss the entire menu when editing the HUD position or key bindings, regardless of where the menu is opened from.

## [0.1.1] - 2024-09-28

### Fixed

- Fish are no longer removed from fish ponds when predictions are on.

## [0.1.0] - 2024-09-20

### Added

- Initial release.
- Splash predictions - show durations/wait times on splash spots.
- Fish predictions - show next catches, per tile.
- Seeded-random predictions - HUD showing jelly state/remaining catches.
- Accelerate game time and NPC speeds during fishing wait periods.
- API, third-party patching and other compatibility features.

[Unreleased]: https://github.com/focustense/StardewFishingSea/compare/v0.2.0...HEAD
[0.2.0]: https://github.com/focustense/StardewFishingSea/compare/v0.1.1...v0.2.0
[0.1.1]: https://github.com/focustense/StardewFishingSea/compare/v0.1.0...v0.1.1
[0.1.0]: https://github.com/focustense/StardewFishingSea/tree/v0.1.0
