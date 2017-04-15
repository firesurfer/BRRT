#!/bin/bash
for i in `seq 1 1`;
do

mono BRRT.exe MAP_roundedFreeForRobot.png 1053,747,90 2292,448,0 ResultMap$i.png 

done
