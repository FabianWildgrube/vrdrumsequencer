# PFS-VR Prototype Unity Project
This directory contains the Unity Project implementing a VR-based Drum Sequencer with customizable virtual environments.

## Contents:
* [Setup Instructions](#setup-instructions)
	- [MRTK Setup](#mrtk-setup)
	- [MRTK Configuration](#mrtk-configuration)
		- [Pointer Profile](#pointer-profile)
		- [Teleportation System Adjustments](#teleportation-system-adjustments)
* [Design Overview](#design-overview)
* [Caveats For Further Development](#caveats-for-further-development)
* [General Development Notes](#general-development-notes)
	- [Notes](#notes)
	- [Sample Library](#sample-library)
	- [MRTK scrolling container](#mrtk-scrolling-container)
	- [Environment Configuration GUI](#environment-configuration-gui)
	- [Song Library](#song-library)
	- [Meadow Environment Package Setup](#meadow-environment-package-setup)
	- [Environment Skyboxes](#environment-skyboxes)
	- [Buttons that trigger on press down](#buttons-that-trigger-on-press-down)
	- [Main Menu](#main-menu)
	- [Teleport To Loop](#teleport-to-loop)
	- [Making sure participants complete their tasks](#making-sure-participants-complete-their-tasks)
	- [Loading Loops from Disk](#loading-loops-from-disk)
	- [Environment configuration exporting](#environment-configuration-exporting)

# Setup Instructions
To set up the project on a new machine after having cloned the repo, execute the following steps:
1. Make sure that you cloned the repo into a folder very near the root of your file system. Otherwise, MRTK assets will run into Windows long path name restrictions!
2. Open the project with UnityHub
3. Install MRTK as detailed in [the section below](#installation)
4. Purchase and install all unity asset store packages that were gitignored (due to size and licensing):
	- [Space Sky Boxes](https://assetstore.unity.com/packages/2d/textures-materials/sky/spaceskies-free-80503)
	- [Meadow Environment](https://assetstore.unity.com/packages/3d/vegetation/meadow-environment-dynamic-nature-132195?aid=1011lGkb&utm_source=aff) (has to be purchased)
		* [follow the instructions in the asset to upgrade to URP](#meadow-environment-package-setup)
	- [Concert Hall Environment](https://assetstore.unity.com/packages/3d/environments/historic/theatre-exterior-and-interior-pack-158203) (has to be purchased)
		* upgrade materials to URP
	- [Record Studio Environment](https://assetstore.unity.com/packages/3d/environments/urban/modularit-record-studio-110966) (has to be purchased)
		* upgrade materials to URP
	- [Modular Sci-Fi Space Base](https://assetstore.unity.com/packages/3d/environments/sci-fi/sci-fi-modular-space-base-location-136312) (has to be purchased)
		* upgrade materials to URP
		* the materials using custom shaders must be changed to URP/Lit manually and their main texture dragged into the albedo field of the lit material
		* materials for the floor must be set to "transparent" to avoid weird shadow issues
5. Copy the file `defaultSampleLibrary.json` into the App's Data Directory (printed to the unity console upon first startup of the prototype)

## Project Requirements
* Fixed Unity version 2019.4.20 LTS
* Using the Universal Render Pipeline
* NuGet Packages Used (through the package [NuGetForUnity](https://github.com/GlitchEnzo/NuGetForUnity/releases))
	- `WebSocketSharp-netstandard`
	- `NewtonSoft Json.NET`
* The MRTK (v2.7.2) is used for interaction in VR; however, due to its size, it is gitignored, so it must be installed manually after cloning the project
* __Don't place the project in a deeply nested folder hierarchy__, because MRTK won't work with very long pathnames

## MRTK Setup

### Requirements
make sure the following things are installed (as described in mrtk docs: [here](https://docs.microsoft.com/en-us/windows/mixed-reality/develop/install-the-tools?tabs=unity)):
* Windows 10
* Visual Studio 2019
* Windows 10 SDK
* Unity Hub and Unity 2019.4 LTS

### Installation
* Download the MRTK packages from [their github repo](https://github.com/Microsoft/MixedRealityToolkit-Unity/releases):
	- Microsoft.MixedReality.Toolkit.Foundations.2.7.2
	- Microsoft.MixedReality.Toolkit.Extensions.2.7.2
* Import the packages (drag&drop or Assets > Import Package > Custom Package...)
* Click through the popup asking to change project settings according to MRTK standards and accept the defaults
* Go into Project Settings > Player and under XR Settings make sure "Virtually Reality Supported" is enabled
* Remove Oculus from the "Virtual Reality Supported" settings
* The top menu should have "Mixed Reality" next to "Component"
* Execute "Mixed Reality" > "Toolkit" > "Utilities" > "Upgrade ... for Universal Render Pipeline"

### "MRTK"-ifying a scene: 
* open that scene
* make sure it does not contain a camera
* click Mixed Reality > Toolkit > Add to Scene and Configure ...
* Two GameObjects are added: "MixedRealityToolkit" and "MixedRealityPlayscape"
* Now you can already press "Play" and look at the empty scene with the Vive Headset
* Add a Cube
* Add the components "Object Manipulator (Script)" and "NearInteractionGrabbable(Script)" to the Cube
* Press play again and move the cube with a Vive Wand


## MRTK Configuration
A Profile was created as a clone from the default MRTK Profile. Subprofiles were also cloned and adjusted to e.g. turn off the playspace visualisation, use custom pointer prefabs, and use custom controller input action mappings.

### Pointer Profile
The pointer profile (Input > Pointers) has been altered by supplying a custom pointer prefab that replaces the ShellHandRayPointer. This prefab overrides the global pointer extent to a small number to mimic the grab interactions found in Tvori. The rationale is, that we don't want to allow far interactions because we want the user to move to stuff in the world and extend their arms and so on.

### Teleportation System Adjustments
By default, the MRTK handles teleportation like so:
> On the Vive Pro Controllers, touching the right or left half of the touchpad very lightly triggers an instant rotation of the camera by 90 degrees.

Because this is very sensitive to touching the touchpad, the behaviour of SteamVR Home was replicated by duplicating and adapting the teleportation pointer script (and a number of connected components, all to be found in `Assets/MRTKCustomisation`) and adding an Input Action `Teleport Click` that is triggered by clicking the touch pad (this is configured using a controller mapping profile for generic open vr controller through the MRTK Game Object that holds all mrtk configurations).

The main idea is: Only when click down happens on the touch pad, start a teleportation request and execute that request once the click up happens.

# Design Overview
The project uses a single scene and number of manager singletons.
The most important one is the `ExperimentManager`.
It handles (de-)activating the GameObjects that belong to certain levels (called "stages") of the experience when necessary.

The system is roughly divided into three "modules" of functionality:
* The core sequencer (placing Notes on Tracks, playback, sample editing)
* The environment configuration (jump between different locations, adjust lighting, colors, objects, etc.)
* The experiment specific aspects (enforce a flow through the different stages of the experiment this prototype was built for, log interactions, etc.)

For a more detailed explanation of each of these modules refer to chapter 5 (Implementation) of Fabian Wildgrube's Master Thesis.

# Caveats For Further Development
Due to time constraints and the very specific scope (create a prototype for a user study on an HTC Vive Pro) a number of implementation decisions had to be made that will most likely pose minor problems for adaptation of this project into e.g. an AR context.

* The ExperimentManager controls the visibility of all objects -> to just use the sequencer in a new scene: take a look at the Loop Manager and how it instantiates new loops.
* Loading a loop from disk is integrated into the ExperimentGuide Menu but the list that allows selecting different loop files (found on disc) can easily be extracted as it is a separate GameObject and internally calls the LoopManager's API for instantiating a loop from a json file.

## Known Bugs
* When the sequencer's main panel is tilted in a specific way, newly added note's "Delete" button (visible on hover) will be positioned behind the panel, because the delete button is parented to the note and the notes are placed with their up vector aligned to the world's up instead of the panel's up vector.
* Snapping is not strong enough -> probably a calculation mistake in the track's methods that take care of snapping.
* When an existing note is dragged it "jumps" to the side at first. This is because the note position is calculated as the projection of the controller's position onto the track. This should probably be changed to use only the relative movement of the controller (along the track's direction) and apply it to the selected note.

# General Development Notes
Rationales or explanations of technical necessities that influenced the design or architecture are detailed (in no particular order) in the following sections. If something seems off or is weird in the code, see if there's an explanation here first ;)

## Notes
The main scheduling logic for notes is implemented in an abstract base class that offers a number of overridable functions for subclasses to implement concrete instantiations. Currently there are only the NormalNotes and the SilenNotes (pauses).

Notes are connected to each other through references, effectively building a linked list.
Whenever a note is added or moved this linked list must be updated to reflect the physical order of the notes.
This achieved by raycasting along the track direction (forward and backward) and intersecting on the "Notes" layer to find the two nearest neighbors for the note moved/added.
Each note has a collider on that layer and is thus picked up by the raycasts.

This is not strictly necessary for the grid prototype implemented since the order and timing of notes is a one-dimensional problem along the track's forward axis.
However, if the `Multiple Loops' concept should ever be built this linked list approach is the appropriate data structure.
For that case the raycaster would only need to be adjusted to cast rays in the direction of the previous note.
And the scheduling mechanism would need to be adjusted to traverse the linked list of previous notes to find out at what time from the beginning of the track a note should be played.

### SilentNotes Prefab
Intentionally has a Collider so it can be hit by raycasts (otherwise notes added to a track would not recognize it as a prevNote!). The collider is also intentionally 6cm big, which is smaller than the model in the normal Note prefab, so as to not interfere with a real note when it is placed "over" a silent (invisible) note.

### Track Direction
Tracks have a forward direction. Notes that are spawned by TrackLine are rotated so that their local `right` points in the forward direction of the track they are placed on. This simplifies dragging a note only along that direction for the special case of the "grid" sequencer. If you ever want to build the "free" version, where tracks aren't constrained along a single axis, you will have to change this behaviour.

### Normal Note Collider and component setup (w.r.g. to NoteFinder and context menu)
Normale Note root object has the Note component, its visualisation child has a collider and the NoteInteractionhandler component. That way MRTK pointer events on the visual note are handled there and pointer events on the context menu game objects (currently only delete button) are not registered by NoteInteractionHandler because they are not a child of it. The NoteFinder raycasts will hit the NoteInteractionHandler GO and they search for a Note component in its parent!

## Sample Library
A selection of default samples is loaded at startup from a static json file, located in the AppDataDir (must be placed there manually!). New Samples saved by the user are stored as a copy of this library with the new samples attached in the user's general data directory (see `SampleLibrary.cs`)

The library stores sample definitions. Every component that does NOT change those definitions can use the definitions references from the library. However, as soon as a component can be expected to change the definitions a copy should be handed to that component, so an explicit "update" action on the library must be issued after the copy has been changed to actually persist the changes.

Therefore, currently the Loop copies the selected sample definition when creating a new track (and its associated editor). And the library offers a IReadOnlyCollection of the definitions in the library

## MRTK scrolling container
Turn Collider to manual -> that allows us to position the Collider on the Game Object that hosts the ScrollingCollection component manually
Turn Mask to automatic and adjust the "Boundary" GO's Box Collider's scale (not the overall scale of the GO) if the content you want to clip is not using all the space in the bounding box equally (i.e. is "slanted" to one side)
> __WARNING:__ Do NOT turn on "apply to shared materials" on the clipping box. This will deactivate something within the MRTK Basic Shader itself and all objects that use a material based on that shader will not be drawn anymore (in the entire project), unless they are manually attached to the `Renderer` list of the clipping box. Turning the option off DOES NOT revert this. The only solution to revert the MRTK Standard Shader back to its normal state is to delete the entire MRTK from your project and reimport it!
 
## Environment Configuration GUI
Each section in the configuration GUI has a component that gets all the GUI components (like sliders) set via the editor. These components register themselves with the EnvironmentConfigManager Singleton, which in turn call them whenever a new config is loaded to ensure that the GUIs always display the correct values. By setting the values all the "update" functions are called because setting a value on an MRTK slider triggers its onValueChanged event!

## Song Library
All `LoopableSong` Scriptable Objects placed in the folder `Resources/Songs` will be loaded into the game and selectable as a song for a song track.
Songs can be excluded from the library through the `SongLibrary` singleton's API, which is currently used to exclude each song that was exported in one of the experiments stages.

## Meadow Environment Package Setup
Import, then go into the folder NatureManufacture Assets/Meadow Environment Dynamic Nature 7 HD and URP Support Packs. There import the URP 7.2 Unity 2019.3 Meadow Environment Package to load necessary materials for URP.

## Environment Skyboxes
The Environment Config Manager applies a new skybox for each environment if one is provided in the inspector. If not, that means, that the environment takes care of the skybox itself (e.g. outer space, where colorfulness changes the skybox -> that's also why outer space needs to make sure to activate its skybox during `activate`)

## Buttons that trigger on press down
To allow buttons to trigger their event as soon as the trigger goes down, the `OnClick` event is NOT used, but rather the `InteractableOnPressReceiver` is added to the `Interactable` component on the button prefab and the `OnPress` event is hooked up with the function that should be called. Otherwise buttons can feel "sluggish", because click is only triggered when the Press ends (i.e. in VR: the trigger is released again)

## Main Menu
The `Managers` GameObject has an InputActionHandler Component attached, that listens for the `Menu` Input Action being raised by the MRTK and forwards that to the appropriate gameobject (MainMenu in the current setup.)

The `Menu` Input Action is mapped to be triggered by the vive controller's menu button in the `Generic OpenVR Controller` Definition in the MRTK `InputSystemProfile/ControllerMappingProfile`.

## Teleport To Loop
To jump to the loop the LoopManager looks at the current loop's position and calculates a position that is slightly in front of that loop and on the ground. For the case that there is no loop available, a Transform is needed that moves with the loop space and which's position can then be used whenever the user wants to jump to the Loop.

To ensure that the user can take the "load loop" button for the refinement stages with him, using the "bring instrument here" functionality, the Loop Manager moves the entire Loop stuff section to the Playspace.

## Making sure participants complete their tasks
The ExperimentManager singleton has a dictionary that maps each stage (that needs it) to a predicate function. This function is checked whenever the user wants to exit the current stage.
The predicate functions check whether a loop was loaded/saved/... enough times for each relevant stage. The Experiment Manager is informed of these actions by the components executing them (right alongside those components logging the occurrence of the action).

## Loading Loops from Disk
The mechanism to load a loop from disk is a little convoluted, due to historic growth...
The LoopManager singleton links to and upon the function "startLoopSelectFlow" being called displays the list that loads all saved loops and displays them for selection.
Now comes the part born of necessity: The Experiment Guide includes buttons, which trigger the startLoopSelectFlow on its pages for the perfection stages. Because at the beginning of these stages no loop is visible the only thing we can know is that the user is looking at the Experiment Guide. So a Transform is included as a child of the Guide and this is used by the LoopManager to show the loop selection dialog at that position.

## Environment configuration exporting
Because the environment configuration can be changed after its initial customization in the EnvironmentConfiguration stage, the EnvironmentConfigManager exports the configuration after each change. (I.e. the first export happens when the user actively finishes configuring and when they decide to change after that every one of those changes produces another exported file, which works because of the file saving logic that takes care of duplicates with a counter appended to the name)

To achieve this, the sliders emit an "end" event, so we don't export 60 files / second. This end event is forwarded through the config sub uis to the Environment Config Manager.