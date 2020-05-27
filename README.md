# sandra-three
Third incarnation of a chess UI for Windows 7 and higher.

![Preview Sandra](https://github.com/PenguinF/sandra-three/tree/master/Doc/preview_sandra_0.9.png)

Version 1.0 is a raw PGN editor, with following features:

* View and edit PGN files.
* Single instance.
* Dragging a PGN file onto the window opens it, or activates it if already open.
* Smart PGN checker with warnings and error messages.
* Auto-saves edits and window state. Reopening the application after closing it restores its exact state.
* Works with large PGN files without problems.
* Automatically reloads files if updated on disk.
* Chessboard windows click or snap together when moved around.
* Appropriate effects for special types of moves.
* Different modes of making moves.
* Pieces can be picked up and moved across the entire screen, not just within their window boundaries.
* Synchronized navigation between PGN editors and chessboards.
* A screenshot of a diagram can be copied to the clipboard with Ctrl+C or by using the corresponding menu item.
* Tab pages can be undocked to become separate windows and vice versa.
* All text is localized. There's a list of available languages under the globe icon.
* Json editor for user preferences with syntax highlighting and error reporting.
* User preferences are live: modified preferences are applied immediately, without requiring an application restart.
* Everything is accessible by keyboard. *[Work in progress.]*
* Turning the mouse wheel, or pressing the Ctrl++ and Ctrl+- keyboard shortcuts will zoom in and out. This applies to chessboards as well as PGN editors.
* Saved screen space by displaying the main menu inside the caption area of the window.
* Responds to Windows display settings such as accent colors, or dark mode.
* Modal dialogs are avoided unless absolutely necessary.
* Developer mode to assist with SandraChess development. It reveals a menu item to edit the localized strings of the current language for example.

Some of these features are still under construction.

***

Icons used in this application came from:

* https://iconpharm.com/
* https://www.iconfinder.com/icons/186398/globe_online_world_icon

***

Sandra 1 was coded in Delphi in 2004-2005.
Sandra 2 was a dead end research project.

Sandra 3 targets the .NET framework 4.7.2, and is coded in C# 7.
