# Changelog

All notable changes to this project will be documented in this file.



## [1.0.7] - 2026-03-28

### Features

- **Likes Channel**: Added a permanent "Likes" channel for locally storing and rotating favorite wallpapers.
- **Context Menu Interaction**: Integrated "Like/Unlike" actions directly into the wallpaper preview context menu.
- **Dynamic Icons**: Implemented a dedicated heart icon for the new "Likes" channel in the channel list.
- **Local Rotation Engine**: Optimized the wallpaper scheduler to support seamless, offline rotation for the "Likes" collection.

### Bug Fixes

- **Unsplash Attribution**: Hidden external Unsplash links for local-only "Likes" wallpapers to maintain consistency.
- **Rotation Loop**: Fixed an issue where the wallpaper sequence would not loop correctly for the "Likes" channel.
- **Metadata Protection**: Ensured liked wallpapers are preserved during channel cache cleanup operations.


## [1.0.6] - 2026-03-27

### Performance Optimization

- **Async Startup**: Refactored application startup to initialize view models asynchronously, 
significantly reducing main thread blocking and improving launch speed.
- **UI Pre-warming**: Implemented background pre-loading for the "Channels" window and 
"Tray Context Menu" to ensure zero-lag during first interaction.
- **Display Info Caching**: Added caching for screen geometry data, eliminating redundant native 
API calls when opening the tray menu.
- **Image Gallery Tuning**: Optimized thumbnail rendering with `IgnoreColorProfile` and `DecodePixelWidth` 
for smoother scrolling and lower memory footprint.
- **Window Management**: Enhanced `WindowManager` with a `PreloadWindow` capability for seamless 
background window initialization. Fixed "black window" flicker during pre-loading by forcing 
handle creation without visibility.

### Code Cleanup
- **Path Refactoring**: Relocated database and window position files to dedicated subfolders (`db` and `pos`) for better organization of application data.

