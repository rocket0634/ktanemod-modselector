# ModSelector - a KTANEModKit mod

This mod provides a holdable user interface while in the setup room, which allows dynamic veto of active mod objects, including regular modules, needy modules, widgets, gameplay rooms, bomb casings, and services.

## v2.0 Changes

The Mod Selector has gone through a few changes as of v2.0, some of which are quite major.

### New UI

As of v2.0, the Mod Selector no longer exists as a 2D screen-space UI; instead, it is now presented as a game-world holdable, much like the bomb binder. As such, it should be interactable by all input schemes already supported by KTaNE itself.

### Dynamic Profile Merging

As of v2.0, you now have the ability to select multiple profiles to run at any given time, thus providing a dynamic profile merging system. Profiles can be merged in two different ways, specified on a per-profile basis:
- Intersect: will intersect with other 'intersect' active profiles, and only disable those objects that are common across all of those 'intersect' profiles. Useful for personal experting profiles.
- Union: will union with all other active profiles, guaranteeing disabling of objects specified in this profile regardless of other profile configurations. Useful for a personal defusal profile, or for different 'toggleable' bomb configuration profiles, such as a 'No Double-Decker' profile, 'No Two-Factor' profile, etc.

### Overview Video

These features are all covered by a short overview video, available on YouTube: https://www.youtube.com/watch?v=Wygs3M4zWbo
