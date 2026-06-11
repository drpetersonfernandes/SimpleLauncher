---
id: keyframe-animations
title: Using keyframe animations
description: Define keyframe animations in XAML to animate control properties over a timeline.
doc-type: how-to
---

import AnimationKeyframeDiagram from '/img/guides/ui-development/graphics/animation-keyframe.png';
import KeyframeFadeScreenshot from '/img/guides/ui-development/graphics/keyframe-fade.gif';
import KeyframeCompositeAnimationScreenshot from '/img/guides/ui-development/graphics/keyframe-composite-animation.gif';
import LinearEasingScreenshot from '/img/guides/ui-development/graphics/linear-easing.gif';
import BounceEaseInScreenshot from '/img/guides/ui-development/graphics/bounce-ease-in.gif';

You can use a keyframe animation to change one or more control properties following a timeline. The keyframes are defined in _Avalonia UI_ styles with **cue** points along the **duration** of the animation, and set the intermediate values of the properties at a point in time.

<Image light={AnimationKeyframeDiagram} alt="Diagram showing keyframe animation timeline with cue points" position="center" maxWidth={400} cornerRadius="true"/>

The property values between keyframes are set following the profile of an **easing function**. The default easing function is a straight-line interpolation.

The animation is triggered to start, and then can run any number of times, in either direction. There are also options to delay the start of the animation, and to repeat it.

:::info
If you are familiar with keyframe animations keyframe work in CSS, you will recognise the similarity with how they are done in in _Avalonia UI_.
:::

## Example

You define a keyframe animation using styles.

:::info
To revise how _Avalonia UI_ uses styles, see the [styles concept](/docs/styling/styles).
:::

Follow this procedure to define a simple color fade animation using XAML:

-  Create a styles collection at your chosen level.
-  Add a style to the collection with a selector that can target the control you want to animate.
-  Add a `Setter` element to define the property that you wan the animation to change. In this example `<Setter Property="Fill" Value="Red"/>`
-  Add a `Style.Animations` element to contain your animation.
-  Add an `Animation` element and set its `Duration` attribute. This is in the format `"Hours:Minutes:Seconds"`.
-  Now define the keyframes for the animation. This example uses cues at 0% and 100%.
-  Add `Setter` elements to each keyframe for value of the fill opacity. This example animates between opacity values of 0.0 and 1.0.

The finished code will look like this: 

```xml
<Window xmlns="https://github.com/avaloniaui">
    <Window.Styles>
        <Style Selector="Rectangle.red">
            <Setter Property="Fill" Value="Red"/>
            <Style.Animations>
                <Animation Duration="0:0:3"> 
                    <KeyFrame Cue="0%">
                        <Setter Property="Opacity" Value="0.0"/>
                    </KeyFrame>
                    <KeyFrame Cue="100%">
                        <Setter Property="Opacity" Value="1.0"/>
                    </KeyFrame>
                </Animation>
            </Style.Animations>
        </Style>
    </Window.Styles>

    <Rectangle Classes="red" Width="100" Height="100"/>
</Window>
```

The resulting animation looks like this:

<Image light={KeyframeFadeScreenshot} alt="Animation showing a red rectangle fading in" position="center" maxWidth={400} cornerRadius="true"/>

The animation runs as soon as the rectangle control is loaded and can be selected by the style. In fact it runs in the preview pane as well!

## Animate two properties

This example shows you how to animate two properties on the same timeline.

```xml
<Window.Styles>
    <Style Selector="Rectangle.red">
      <Setter Property="Fill" Value="Red"/>
      <Style.Animations>
        <Animation Duration="0:0:3" IterationCount="4">
          <KeyFrame Cue="0%">
            <Setter Property="Opacity" Value="0.0"/>
            <Setter Property="RotateTransform.Angle" Value="0.0"/>
          </KeyFrame>
          <KeyFrame Cue="100%"> 
            <Setter Property="Opacity" Value="1.0"/>
            <Setter Property="RotateTransform.Angle" Value="90.0"/>
          </KeyFrame>
        </Animation> 
    </Style.Animations>
    </Style>
  </Window.Styles>
```

The red rectangle is faded-in and rotated at the same time.

<Image light={KeyframeCompositeAnimationScreenshot} alt="Animation showing a red rectangle fading in and rotating simultaneously" position="center" maxWidth={400} cornerRadius="true"/>

## Configuring animation

### Delay

You can add a delay to the start of an animation by setting the delay attribute of the animation element. For example:

```xml
<Animation Duration="0:0:1"
           Delay="0:0:1"> 
    ...
</Animation>
```

### Repeat

You can make an animation repeat for a set number of times, or indefinitely. To repeat for a finite number of iterations, set the `IterationCount` attribute on the animation element:

```xml
<Animation IterationCount="5">
    ...
</Animation>
```

To repeat an animation indefinitely, use the special  `"INFINITE"` value. For example:

```xml
<Animation IterationCount="INFINITE">
    ...
</Animation>
```

### Playback direction

By default an animation plays forward, following the profile of the easing function from left to right. You can alter this behavior by setting the `PlaybackDirection` attribute on the animation element:

```xml
<Animation IterationCount="9" PlaybackDirection="AlternateReverse">
    ...
</Animation>
```

The following table describes the options:

<table><thead><tr><th width="245">Value</th><th>Description</th></tr></thead><tbody><tr><td><code>Normal</code></td><td>(Default) The animation is played forwards.</td></tr><tr><td><code>Reverse</code></td><td>The animation is played in reverse direction.</td></tr><tr><td><code>Alternate</code></td><td>The animation is played forwards first, then backwards.</td></tr><tr><td><code>AlternateReverse</code></td><td>The animation is played backwards first, then forwards.</td></tr></tbody></table>

### Fill mode

The fill mode attribute of an animation defines how the properties being set will persist after it runs, or during any gaps between runs. For example:

```xml
<Animation IterationCount="9" FillMode="Backward">
    ...
</Animation>
```

The following table describes the options:

<table><thead><tr><th width="240">Value</th><th>Description</th></tr></thead><tbody><tr><td><code>None</code></td><td>Value will not persist after animation nor the first value will be applied when the animation is delayed.</td></tr><tr><td><code>Forward</code></td><td>The last interpolated value will be persisted to the target property.</td></tr><tr><td><code>Backward</code></td><td>The first interpolated value will be displayed on animation delay.</td></tr><tr><td><code>Both</code></td><td>Both <code>Forward</code> and <code>Backward</code> behaviors will be applied.</td></tr></tbody></table>

### Easing function

An easing function defines how a property is varied over time during an animation.

<div>

<Image light={LinearEasingScreenshot} alt="Graph showing linear easing function" position="center" maxWidth={400} cornerRadius="true"/>

<Image light={BounceEaseInScreenshot} alt="Graph showing bounce ease-in easing function" position="center" maxWidth={400} cornerRadius="true"/>

</div>

The default easing function is linear (above left), but you use another pattern by setting the name of the desired function in the easing attribute. For example to use the 'bounce ease in' function (above right):

```xml
<Animation Duration="0:0:1"
           Delay="0:0:1"
           Easing="BounceEaseIn"> 
    ...
</Animation>
```

:::info
For a full list of the _Avalonia UI_ easing functions, see the [animation settings reference](/docs/graphics-animation/animation-settings).
:::

You can also add your own custom easing function class like this:

```xml
<Animation Duration="0:0:1"
           Delay="0:0:1">
    <Animation.Easing>
        <local:YourCustomEasingClassHere/>
    </Animation.Easing> 
    ...
</Animation>
```

## Running animation from the code behind

In some situations, developers need more flexibility with animation lifetime, comparing to the XAML style selectors. Easiest would be to define animation in the `Resources` dictionary.

While defining `Animation` this way, it's important to specify both `x:Key` and `x:SetterTargetType`. First one will be used to access animation by the key, and second helps compiler to create strongly typed setters.

```xml
<Window xmlns="https://github.com/avaloniaui">
    <Window.Resources>
        <Animation x:Key="ResourceAnimation"
                   x:SetterTargetType="Rectangle"
                   Duration="0:0:3"> 
            <KeyFrame Cue="0%">
                <Setter Property="Opacity" Value="0.0"/>
            </KeyFrame>
            <KeyFrame Cue="100%">
                <Setter Property="Opacity" Value="1.0"/>
            </KeyFrame>
        </Animation>
    </Window.Resources>

    <Rectangle x:Name="Rect" />
</Window>
```

Now, this animation can be accessed and executed in a custom code behind handler.

```csharp
var animation = (Animation)this.Resources["ResourceAnimation"];
// Running XAML animation on the Rect control. 
await animation.RunAsync(Rect);
```

`RunAsync` returns a task which is completed with the animation. If animation is infinite/repeating, task will never end, unless cancelled externally by passing `CancellationToken` to the RunAsync method.

:::info
While it's easier to define animations in XAML, it's also possible to do completely in C# code. It's possible to create an instance of `Animation` type, and populate key frames collection.
:::

## See also

- [Animation Settings](/docs/graphics-animation/animation-settings): Duration, delay, iteration count, and playback direction.
- [Easing Functions](/docs/graphics-animation/easing-functions): All available easing functions.
- [Control Transitions](/docs/graphics-animation/control-transitions): Animating property changes with transitions.