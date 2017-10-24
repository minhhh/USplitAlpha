# USplitAlpha

> Split Alpha for Android and iOS

According to a survey ([Unity Texture compression and optimization](http://minhhh.github.io/posts/unity-texture-compression-and-optimization)) about various different texture formats in Unity, `ETC` and Split Alpha channel provides the best build size and memory footprint. As mentioned [here](https://feedback.unity3d.com/suggestions/pvrtc-rgb-4bit-plus-alpha-split-texture-compression-dot-dot-dot), Unity currently does not support SplitAlpha on iOS and there was a bug on UI images in older version.

`USplitAlpha` is a library that helps you apply Split Alpha texture to both Android and iOS easily. On Android it uses `ETC1 RGB4bit` both main texture and alpha texture. On iOS is uses `PVRTC RGB4bit`.

This inspiration for this library is drawn from [https://github.com/keijiro/unity-alphamask](https://github.com/keijiro/unity-alphamask) and [Unity、Mobile端末向けの画一的なテクスチャ圧縮方法](https://web.archive.org/web/20160726041743/http://developers.mobage.jp/blog/texture-compression)

## Usage
To apply USplitAlpha into your project, there are 3 main steps

1. Create Split Alpha textures from the original textures
2. Create Material that uses Split Alpha
3. Apply material to `UI Image`, `UI Raw Image` or `Sprite`.

**Step 1: Create Split Alpha textures from the original textures**

1. Preconditions:
    * The texture that you wish to create alpha channel for must have width and height being power of two and multiple of 4. Note that the texture does NOT have to be squared.
    * The texture must also be `png`. If you have a `psd`, please convert it to `png` first.
    * The texture must not be packed by Unity SpritePacker, i.e. no packing tag
2. In the `Project` view, right-click on the texture. Select `USplitAlpha>Apply`. This will generate an alpha channel texture and put it in a folder called `_alpha` in the same folder as the original texture After that, both the original texture and the alpha texture import settings will be changed to using `ETC1 RGB4bit` and `PVRTC RGB4bit` on Android and iOS, respectively.

![example 1](/imgs/example1.png)

**Step 2: Create Material that uses Split Alpha**

1. Create a new material
2. Set the material's shader to one of the shaders in `Plugins/USplitAlpha/Shaders`. For instance, use `SplitAlpha/Sprites/Default` shader if you want to apply to Sprites.
3. The material's shader will have 2 texture fields: `Sprite Texture` and `External Alpha` (the names might vary slightly according to shaders). Set the original texture to `Sprite Texture` and the alpha mask texture `External Alpha`

![example 2](/imgs/example2.png)

**Step 3: Apply material to UI Image, UI Raw Image or Sprite**

1. Apply the material created in step 2 to the appropriate elements. For `UI Image` and `UI Raw Image`, use `SplitAlpha/UI/Default`. For `Sprites`, use `SplitAlpha/Sprites/Default`. For mobile particles, use `SplitAlpha/Mobile/Particles/Additive` or `SplitAlpha/Mobile/Particles/Multiply` or `SplitAlpha/Mobile/Particles/Alpha Blended`
2. Set the correct sprite. For `RawImage` you don't have to set anything. For `UI Image` and `Sprite`, you have to choose a sprite inside the original texture. Most of the time, you would want to create the material from an Atlas so that you can batch multiple `UI Image` and `Sprites` to save draw call.

![example 3](/imgs/example3.png)

**Bonus 1: Batch create split alpha texture**
You can actually select several textures and/or folders then choose `USplitAlpha>Apply`. This will create split alpha textures for all of the `png` files in all subfolders recursively.

**Bonus 2: Revert split alpha textures**
You can actually select several textures and/or folders then choose `USplitAlpha>Revert` to revert the textures to their original settings without split alpha. All of the split alpha textures will be removed and folders `_alpha` as well if they are empty.

**Bonus 3: Create texture Atlas**
I use the plugin `SimpleSpritePacker` to create texture atlas from NPOT sprites. You can also use other external tools such as TexturePacker to create atlases.

**Bonus 4: Loading from AssetBundle**
You can simply put all the textures and materials in AssetBundle and load them from server. Only thing to note is after you load the material, you will have to re-set the shader of the material like so:

```
    material.shader = Shader.Find (material.shader.name);
```

**Bonus 5: Loading atlas from AssetBundle**
You cannot put Atlas packed by Unity's SpritePacker and load it back, that's why we have to use manual Atlas. Then, loading sprites from an Atlas loaded from AssetBundle is simple. You can use a solution such as [UBootstrap.SpriteCollection](https://github.com/minhhh/UBootstrap.SpriteCollection) to do this


### Examples
You can find all examples in scene `Assets/Tests/Test`

1. `Test A with alpha` uses a material with a single sprite texture
2. `Test B with alpha UI RawImage` uses a material with a raw texture
3. `Test B with alpha UI` uses the same material as `Test B with alpha UI RawImage`
4. `Batchable UI Image 1`, `..2`, etc. uses `ItemAtlasUI` material created from `ItemAtlas`. This allows to use the same material for many UI Images and allow them to batch
5. `BatchableSprite 1`, `..2`, etc. is similar to `Batchable UI Image 1`, except that the shader is replaced by `SplitAlpha/Sprites/Default`

### Support Shaders
Currently, there are 5 Split Alpha shaders corresponding to 6 original shaders:

1. `UI/Default`: `SplitAlpha/UI/Default`
1. `Sprites/Default`: `SplitAlpha/Sprites/Default`
1. `Mobile/Particles/Additive`: `SplitAlpha/Mobile/Particles/Additive`
1. `Mobile/Particles/Multiply`: `SplitAlpha/Mobile/Particles/Multiply`
1. `Mobile/Particles/Alpha Blended`: `SplitAlpha/Mobile/Particles/Alpha Blended`

The code for the ogiginal shaders was taken from Unity then modified to support Split Alpha. Extending SplitAlpha to other shaders is straightforward.

## Future Improvements

Obviously, the workflow so far is a little laborious. You have to create split alpha textures by hand, create materials by hand, apply material to UI elements by hand. There are so many improvements that can be made to automate all of these tasks better. In fact, all of these improvements have been implemented and applied to production but I cannot share the code.

**Improvement 1: Automatic material creation**
This is how you implement:

1. Create a lookup table to replace normal shader by a split alpha one. For instance, `UI/Default` will be replaced by `SplitAlpha/UI/Default`, and so on.
2. Create Helper Component (MonoBehaviour) for different visual elements, such as `UIRawImageSplitAlpha`, `UIImageSplitAlpha`, `SpriteRendererSplitAlpha`, `MeshRendererSplitAlpha` and so on.
3. Helper Components will operate in 2 modes: Editor mode and Play Mode.
4. In Editor Mode, Helper Component will create a dummy material, set the correct shader by searching for matching shader in the lookup table in step 1. Then Helper Component will set the original texture and alpha texture correspondingly. Finally, Helper Component will set the dummy material so that the element is displying correctly in Edit Mode.
5. In Play Mode, Helper Component will create shared static material instead of dummy material. This is done so that multiple component using the same original texture will also share the same Split Alpha material. One thing to note is that sprites that are packed with Unity's SpritePacker using packing tag can also work.

Now you can simply assign sprites to different visual elements, then set all of those sprites to use the same packing tag and voila, all your visual elements can batch and use Split Alpha. However, this approach can NOT work with AssetBundle.

**Improvement 2: Add Helper Component to GameObject automatically**
There is still one manual step that you have to do in **Improvement 1**, that is adding the correct MonoBehaviour to the correct GameObject. We can automate this step like so:

1. Create a tool that allows you to select several objects, prefabs, or scenes, recursively.
2. Make an Apply function that search for certain Components in those GameObjects, Prefabs, or Scenes. The Components can be `UIImage`, `UIRawImage`, `SpriteRenderer`, `MeshRenderer` or other custom renderer.
3. Add the corresponding `UIImageSplitAlpha`, `UIRawImageSplitAlpha`, `SpriteRendererSplitAlpha`, `MeshRendererSplitAlpha` or other custom components.

Also add a Revert function to revert the change easily.

**Improvement 3: Create Split Alpha material automatically**
Create a tool so that users only have to set the main texture, the alpha texture will be filled in automatically by searching in `_alpha` folder.

**Improvement 4: Reduce the size of Alpha mask**
If the alpha mask of a texture is very simple, for instance, it's mostly white pixels or smooth gradation, then it can be downsized to 1/4 or 1/16 of the area of original texture. Then it can be used by expanding it with a bilinear filter.

## Install

To include USplitAlpha into your project, you can copy `Assets/Plugins/USplitAlpha` to your project folder. Alternatively, you can use `npm` method of package management described [here](https://github.com/minhhh/UBootstrap).

## Changelog

**0.0.1**

* Initial commit

<br/>

