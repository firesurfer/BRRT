
#!/bin/sh
for i in `seq 1 100`;
do
mono BRRT.exe MAP_roundedFreeForRobot.png 2200,600,90 1052,318,180 Result${i}_wide.bmp

done
