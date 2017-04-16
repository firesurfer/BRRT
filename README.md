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


