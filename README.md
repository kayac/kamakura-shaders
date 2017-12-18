Kamakura Shaders
================

**Kamakura Shaders** is a collection of shaders and components focusing on Non-Photorealistic Rendering for Unity with a bunch of features and adjustable parameters in a user-friendly interface.


Features
--------

### Kamakura Shader
Kamakura shader has a set of features that can be used altogether to create styles such as anime, paint, water-color, sketch like, and so on. Here is a brief overview of these features

![image](https://cdn-ak2.f.st-hatena.com/images/fotolife/m/mu4ma6/20170830/20170830111300.png)

- Filter
Adjust the main texture color with hue, saturation, and brightness parameters, analogous to Photoshop's Hue/Saturation tool.

![image](https://cdn-ak.f.st-hatena.com/images/fotolife/m/mu4ma6/20170830/20170830111325.png)

- Specular
Control the shininess of material by adjusting parameters such as power, intensity, and smoothness, along with mask texture and color.

![image](https://cdn-ak.f.st-hatena.com/images/fotolife/m/mu4ma6/20170830/20170830111348.png)

- Light Ramp
Control the lighting and shadowing of the material by using a gradient ramp texture. You can stack multiple gradients vertically into one ramp texture (think of it as multiple presets) and choose which one to use using Light Ramp Preset parameter. An offset parameter to adjust the whole distribution is provided, and it can be adjusted locally per vertex by using the green channel of mesh's vertex color.

![image](https://cdn-ak.f.st-hatena.com/images/fotolife/m/mu4ma6/20170830/20170830111417.png)

- Shadow
Customize material shadow's part by using a shadow texture, along with color and intensity parameters.

![image](https://cdn-ak.f.st-hatena.com/images/fotolife/m/mu4ma6/20170830/20170830111552.png)

- Ambient
Control how material receives ambient light. There are two sources of ambient: Cube Color and Unity's Ambient (such as Skybox or GI, by using Light Probes).

![image](https://cdn-ak.f.st-hatena.com/images/fotolife/m/mu4ma6/20170830/20170830111612.png)

- Outline
Generates outline out of the mesh, with some parameters to control its color and thickness. You can also control outline thickness locally by using the red channel of mesh's vertex color.

![image](https://cdn-ak.f.st-hatena.com/images/fotolife/m/mu4ma6/20170830/20170830111632.png)

- Hatch
Generates a hatch effect by using a hatch texture with each channel representing the intensity of the hatch (red = light hatch, green = medium hatch, blue = strong hatch).

![image](https://cdn-ak.f.st-hatena.com/images/fotolife/m/mu4ma6/20170830/20170830111650.png)

- Rim
Generates rim light at the edge with adjustable parameters such as color, size, intensity and smoothness, along with a texture to adjust the smoothness locally.

![image](https://cdn-ak.f.st-hatena.com/images/fotolife/m/mu4ma6/20170830/20170830111706.png)

- Emission
Generates emission. Control the emissive area by using a texture.

![image](https://cdn-ak.f.st-hatena.com/images/fotolife/m/mu4ma6/20170830/20170830111720.png)

- Cube Color
Set the cube colors (six directions: left, right, front, back, top, down) that can be used for rim and/or ambient light.

![image](https://cdn-ak.f.st-hatena.com/images/fotolife/m/mu4ma6/20170830/20170830112636.png)

- Stencil
Adjust the stencil operations performed when rendering the material.

![image](https://cdn-ak.f.st-hatena.com/images/fotolife/m/mu4ma6/20170830/20170830111755.png)

### Kamakura Hair Shader
Kamakura Hair shaders shared many features with Kamakura shader, but it has some features specialized for hair rendering just as its name.

- Specular
Adjust the waviness of the specular by using Local Shift Texture.

![hair-specular](https://user-images.githubusercontent.com/2058959/34143250-1cc156b0-e4cf-11e7-9128-daac8f96684c.gif)

- Primary and Secondary Specular
Control the color, power, intensity and smoothness of two different speculars to adjust the looks of the hair. There is also Strand Texture parameter in Secondary Specular to give strand details.

![hair-primary-specular](https://user-images.githubusercontent.com/2058959/34143155-a77b09b4-e4ce-11e7-9be6-378437e51483.gif)
![hair-secondary-specular](https://user-images.githubusercontent.com/2058959/34143156-a7991094-e4ce-11e7-9034-a6ee94818753.gif)

### Kamakura 2D Shader
Kamakura 2D is a shader for 2D objects like SpriteRenderer or Unity UI's Graphic component (such as Image). It has two main features: Outline and Filter. Some of the properties can be controlled using vertex color (instead of material properties) by attaching Kamakura2DParams component to a GameObject that contains SpriteRenderer or Graphics component. This allows multiple objects to be rendered with various parameters in one batch by using a single shared material.

- Outline
Oultine uses a SDF texture to display outline effect. It has parameters such as color, thickness, softness and offset.

- Filter
Similar to Kamakura shader's Filter feature, it has hue, saturation, and brightness parameters to adjust the main texture color.

### Local Light System
Local Light System allows a local light to be applied to a GameObject and its children that have MeshRenderer in it.

![local-light](https://user-images.githubusercontent.com/2058959/34143425-fb4c15aa-e4cf-11e7-86b4-954ec2a09fc9.gif)

### Cube Color System
Cube Color is a system that allows color to be assigned on each side of the box: left, right, front, back, top, and bottom. These color on each direction can then be used as the color parameter of the ambient light and/or rim light. This system works on world space by default. To make it works on local space, attach a CubeColorLocalSpaceRoot component to a GameObject.

![cube-color](https://user-images.githubusercontent.com/2058959/34143656-207cdbce-e4d1-11e7-8bfb-807d3eeb59e0.gif)

### Ramp Texture Asset
Create ramp texture asset directly inside Unity that can be used as Light Ramp Texture. Changes made on this ramp texture asset are reflected in realtime which is useful to adjust the shading of a material.

![ramp-asset](https://user-images.githubusercontent.com/2058959/34143917-7eb94f0a-e4d2-11e7-94f4-5df2f868651b.gif)

### Copy Paste Feature
You can select some material properties to be copied and then paste it to other materials by activating Select Mode on top of About box in the material inspector.

Installation
------------

Download the latest unitypackage files from the [Releases] page and import it to your project.

[Releases]: https://github.com/kayac/kamakura-shaders/releases

NOTES
-----
- Target Environment: Unity 5.6.x or newer (Unity 2017.2.0p4 at this moment)
- These shaders has not been tested on console platforms (PS4 / WiiU / Switch / Xbox One)
- Graphics API DirectX9: Screen Space Hatch feature is not functioning on this graphics API

License
-------

[MIT & Creative Common License](LICENSE.md)
