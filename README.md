## Developement of a wrapper between ViSP and Unity engine

### Introduction

This project contains:
- a C++ wrapper between ViSP and Unity. This wrapper available in `vispWrapper` folder needs to be linked with [ViSP](https://visp.inria.fr).
- a Unity project available in `unityProject` folder that shows how to use this wrapper to build a demo to illustrate:
  - augmented reality displaying a virtual cube over an AprilTag
  - generic model-based tracking of a cube with an AprilTag glued on one cube face.

This project is compatible with Ubuntu, MacOS and Windows platforms.

### Augmented Reality in Unity using ViSP

* The corresponding Unity scene is available in `unityProject/Assets/Scenes/scene_ar.unity`.
* On each new image AprilTag is detected and localized in 3D.
* A virtual red cube is projected in the scene over the tag thanks to the tag pose estimated with respect to the camera frame.
* Check the video demonstration on YouTube: https://youtu.be/iuD8syhNoGU

### Generic Model-Based Tracking in Unity using ViSP

* The corresponding Unity scene is available in `unityProject/Assets/Scenes/scene_mbt.unity`.
* The tracker initialization is performed using the AprilTag pose. When tracking fails, the tag is again used to initialize the tracker. Thus to start the demo or to recover from a tracking failure, the user has to present the face of the cube that has the tag toward the camera.
* When selecting `Plane > Inspector` the user can modify camera parameters, cube size and tag size to make the demo working with its own material.
* Check the video demonstration on YouTube: https://youtu.be/eLG9B7MHixU

Check [wiki](https://github.com/lagadic/visp_unity/wiki) of repository for more details about this project usage: implementation, tutorial on building using Visual Studio and running demo on Unity.
