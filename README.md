# sandra-three
Third incarnation of a chess UI for Windows 7 and higher.

Main design principle is to have a fluid interface, in which everything should 'just work'. This has led to various complex features which should be hardly noticable, but without which the application feels clumsy:

* Windows click or snap together when moved around.
* There are no modal dialogs. For example, choosing a promotion piece is done by hovering over one of the four quadrants of the promotion square.
* Appropriate effects for special types of moves.
* All text is localized. There's a list of available languages under the globe icon. Move notation is localized too. If necessary, there's always the option to switch to PGN.
* The session is auto-saved. When restarting the application, it continues exactly where it left off. Even after a system crash or power failure. *[Work in progress.]*
* User preferences are live: modified preferences are applied immediately, without requiring an application restart.
* JSON editor for user preferences with syntax highlighting and error reporting.
* Different modes of making moves.
* Everything is accessible by keyboard. *[Work in progress.]*
* Pieces can be picked up, and are drawn correctly no matter where the mouse is on the screen.
* Clicking on a move in the moves list navigates to the corresponding position in the game.
* A screenshot of a diagram can be copied to the clipboard with Ctrl+C or by using the corresponding menu item.
* Turning the mouse wheel, or pressing the Ctrl++ and Ctrl+- keyboard shortcuts will zoom in and out.

Version 1.0 will be released when all abovementioned works-in-progress have been completed.

***

Icons used in this application came from:

* https://iconpharm.com/
* https://www.iconfinder.com/icons/186398/globe_online_world_icon

***

Sandra 1 was coded in Delphi in 2004-2005.
Sandra 2 was a dead end research project.

Sandra 3 targets the .NET framework 4.7.2, and is coded in C# 7.

Sandra is that mysterious woman who became a member of chess club SV Drienerlo. Everybody secretly admires her, and everyone wants to play with her. There are rumours about beer tournaments just to decide who gets a game with her in the club competition. Not me, of course.
