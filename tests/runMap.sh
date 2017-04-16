#!/bin/bash
mkdir -p results
for i in `seq 1 1`;
do

mono ../BRRT/bin/Debug/BRRT.exe MAP_roundedFreeForRobot.png 1000,1247,90 2356,420,0 results/ResultMap$i.png 

done
