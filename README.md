# BRRT (Beteigeuze RRT)

This is a kind of backward RRT for mobile robots with two seperate controllable steering axes. (see www.kamaro-engineering.de for an example)

# Build it

Open the solution in Monodevelop or Visualstudio an simply build it.
Or run xbuild on a console.

# Run it

You need to pass:

* Input map as an image: `InputFile.png`
* Startpoint as `x`, `y`, `orientation`
* Endpoint as `x`, `y`, `orientation`
* Name of image the path should be drawn in: `Outputfile.png`
* (optional) Path to xml the path should be stored in: `Path.xml`

```
mono BRRT.ext InputFile.png x,y,orientation x,y,orientation Outputfile.png Path.xml
```

# Example

This is an example result of this call run on the `MAP_roundedFreeForRobot.png` in the tests folder.

```
mono ../BRRT/bin/Debug/BRRT.exe MAP_roundedFreeForRobot.png 1000,1247,90 2310,899,280 results/ResultMap$i.png  results/Path.xml
```

![Path planning example](doc/ResultMap1.png)

**Note** : the map been run through an algorithm that inflates the obstacles to consider the robot width. That means if the path cut a small part of an obstacle edge this will not result into a collision.

# Tune the algorithm

There are basically two points where you could tune the result:

The first one is the constructor in the `RRT.cs`. The parameters are explained in the in file documention.

The second one is the constructor in the `PathOptimizer.cs`.
