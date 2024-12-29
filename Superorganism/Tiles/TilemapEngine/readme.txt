Squared.Tiled  
Copyright (C) 2009 Kevin Gadd  

This software is provided 'as-is,' without any express or implied  
warranty. In no event will the authors be held liable for any damages  
arising from the use of this software.  

Permission is granted to anyone to use this software for any purpose,  
including commercial applications, and to alter it and redistribute it  
freely, subject to the following restrictions:  

1. The origin of this software must not be misrepresented; you must not  
   claim that you wrote the original software. If you use this software  
   in a product, an acknowledgment in the product documentation would be  
   appreciated but is not required.  
2. Altered source versions must be plainly marked as such, and must not be  
   misrepresented as being the original software.  
3. This notice may not be removed or altered from any source distribution.  

Kevin Gadd kevin.gadd@gmail.com http://luminance.org/  

Updates by Stephen Belanger - July 13, 2009  
- Added ProhibitDtd = false, so you don't need to remove the doctype line after each time you edit the map.  
- Changed everything to use SortedLists for easier referencing.  
- Added object groups.  
- Added movable and resizable objects.  
- Added object images.  
- Added meta property support to maps, layers, object groups, and objects.  
- Added non-binary encoded layer data.  
- Added layer and object group transparency.  

TODO: I might add support for .tsx Tileset definitions. Not sure yet how beneficial that would be...  

Modifications by Zach Musgrave - August 2012  
- Fixed errors in TileExample.cs.  
- Added support for rotated and flipped tiles (press Z, X, or Y in Tiled to rotate or flip tiles).  
- Fixed exception when loading an object without a height or width attribute.  
- Fixed property loading bugs (properties now loaded for Layers, Maps, Objects).  
- Added support for margin and spacing in tile sets.  
- CF-compatible System.IO.Compression library available via GitHub release. See releases at https://github.com/zachmu/tiled-xna.  

Zach Musgrave zach.musgrave@gmail.com http://gamedev.sleptlate.org  

Modifications by Nathan Bean - March 2022  
- Changed XmlReader settings to use DtdProcessing instead of now-deprecated ProhibitDtd = false.  
- Added XML-style comments to each class and member.  
- Updated Example to use MonoGame.  

Nathan Bean nhbean@ksu.edu  

Modifications by Synoeca - December 2024  
- Added support for ground-level collision detection in tilemap.  

Synoeca synoeca523@ksu.edu  
