KAMAKURA SHADERS v1.0.7
=======================

Updated 2018/5/17

Kamakura Shaders is a collection of shaders and components focusing on Non-Photorealistic
Rendering for Unity with a bunch of features and adjustable parameters in a user-friendly
interface.


NOTES
-----
- Target Environment: Unity 5.6.x ~ Unity 2017.4, Unity 2018.2.0b3
- It will not compile on Unity 2018.1.0f2. If you are using that version,
  please use Kamakura Shaders v1.0.5 instead
- Graphics API DirectX9: Screen Space Hatch feature is not functioning on this graphics API
- These shaders has not been tested yet on console platforms
  (PS4 / WiiU / Switch / Xbox One)


CHANGELOG
---------

### v1.0.7
- Updated Dreamy Character model

### v1.0.6
- Reenabled point and spot light attenuation as Unity patched the bug in Unity 2018.2.0b3
- Fixes non-uniform outline width bug

### v1.0.5
- Point light and spot light attenuation is temporarily disabled due to a bug in
  Unity 2018.1.0f2. If you are not using Unity 2018.1, please use Kamakura
  Shaders v1.0.4 instead

### v1.0.4
- Fixed normal seams problem
- Improved Outline: scale-independent outline. Back-face outline is now
  calculated in clip-space, so it might be necessary to adjust outline
  thickness parameter on existing materials
- Added Rotate parameter in Kamakura Hair shader for rotating hair specular

### v1.0.3
- Added Blend Mode (Normal, Multiply) for Hatch and Shadow
- Added Normal Intensity property

### v1.0.1
- Fixed weird outline bug when using Metal Graphics API
- Improved Select Mode feature
- Improved property names

### v1.0.0
- 3 Shaders included: Kamakura, Kamakura Hair, Kamakura 2D
- Kamakura (Kamakura-Standard) for general non-photorealistic rendering
- Kamakura Hair (Kamakura-Standard-Hair) for hair specific rendering
- Kamakura 2D (Kamakura-2D) for 2D graphics object
- Local Light System
- Cube Color System
- Ramp Texture Asset features
- Copy and paste feature on material properties


LICENSE
-------

MIT License

Copyright (c) 2017 Kayac Inc.

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.


DREAMY CHARACTER LICENSE
------------------------

CC-BY-SA 4.0

Dreamy character used in Kamakura Shaders' example scenes are created by
Matsumura Masahiro. You are free to:
  - Share: Copy and redistribute the material in any medium or format
  - Adapt: Remix, transform, and build upon the material for any purpose, even
    commercially

As long as you follow the license terms under the following terms:
  - Attribution: You must give appropriate credit, provide a link to the license,
    and indicate if changes were made. You may do so in any reasonable manner,
    but not in any way that suggests the licensor endorses you or your use.
  - ShareAlike: If you remix, transform, or build upon the material, you must
    distribute your contributions under the same license as the original.
  - No Additional Restrictions: You may not apply legal terms or technological
    measures that legally restrict others from doing anything the license
    permits.

Notices:
  You do not have to comply with the license for elements of the material in
  the public domain or where your use is permitted by an applicable exception
  or limitation. No warranties are given. The license may not give you all of
  the permissions necessary for your intended use. For example, other rights
  such as publicity, privacy, or moral rights may limit how you use the material.
