#!/bin/bash
for i in `seq 1 100`;
do

mono BRRT.exe MAP_roundedFreeForRobot.png 939,1003,90 2278,377,0 ResultMap$i.png 

done
