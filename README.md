# IcoSphereMeshGenerator

##Introduction

A Unity script for producing custom icosphere meshes. Accepts multiple levels of complexity and sets UVs accordingly.

##Points of interest

* Uses my ThreadRunner multithreading package to push the mathematics over to a new thread.
* Uses an algorithm for generating the point triplets for an icosphere that might be novel.

##Use

1. Include files into project, use provided test scene to configure your desired mesh.
2. Produce a save destination folder at: 'Assets/Meshes/IcoSpheres/'
3. Run the mesh generation with the save bool enabled, and the mesh will be saved into the destination directory.
4. Ensure your material uses the wrap-repeat setting.

##Conclusion

It makes icospheres.
