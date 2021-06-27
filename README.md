# DirLinker 
A tool to create and manage Symbolic Directory Links / Hardlinks on your Computer

![The app with Demonstration entries](https://raw.githubusercontent.com/comroid-git/DirLinker/master/img/DirLinker-1.png)

DirLinker helps you by providing an interface to first set up all hard links you need, and then creates those.

## How to Use
- 1. The `Link Directory` input field expects the path to the directory in which the link should be created
- 2. The `Link Name` input field expectes the desired name of the hard link
- 3. The `Target Directory` input field expects the path to the desired link target
- 4. The `Submit` Button adds the entry to the configuration list.
- 5. The `Config` Button Saves the configuration and opens it using the explorer
- 6. The `Pause Console` Button sets whether the `Apply Configuration` Output should stay open until further input. Useful for Troubleshooting
- 7. Finally, the `Apply Configuration` Button will request administrative privilege in create all desired Hard links
- 8. During this process, if the `HardLinkTool` sees a Directory exist at the Link Directory, **it attempts to Move it to the target directory, and then creates the Hard link**

## Known Problems
- Editing existing entries is currently not implemented
