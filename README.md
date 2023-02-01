# unity-ros2
This repo contains the Unity source files used to make the simulation binary for the Pioneer robot.
Large files can not be checked into github, hence meshes and textures are store on onedrive [here](https://universitysystemnh-my.sharepoint.com/:f:/g/personal/pac48_usnh_edu/EjzH5h7oiKFFgBeB9zhO8_IB4AWSUQ0zRl6O84Gly7YTww?e=MzlEpp). The Meshes and Textures folder should be downloaded and placed in the Assets folder of the project.


This unity project uses several non-default packages:
1. GLTFUtility version 0.6.0
2. ROS TCP Connector version 0.7.0-preview
3. AI Navigation version 1.1.1
4. Mathematics version 1.2.6
5. Newtonsoft Json version 3.0.2


## Simulator keyboard controls 
`shift+O` : open door

`shift+D` : move to door

`shift+B` : move to bedroom

`shift+C` : move to couch

`shift+K` : toggle move to kitchen position 1/2

`shift+P` : take medicine

`WASD` : nagivate camera

`right-click and drag` : pan camera

`mouse wheel` : zoom in
