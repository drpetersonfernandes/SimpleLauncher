---
id: radial-tree-chart
title: Radial tree chart
description: Hierarchical layout where the root node is at the center and child nodes radiate outward in concentric rings, space-efficient for large trees.
doc-type: reference
tags:
  - avalonia pro
---

import chartsHierarchicalRadialtree from '/img/controls/charts/charts-hierarchical-radial-tree.png';

:::info
[Charts](/controls/data-display/charts) are available with [Avalonia Pro](https://avaloniaui.net/pricing).
:::

Radial tree charts represent hierarchical data where the root is at the center and child nodes radiate outward in concentric circles. This layout is highly space-efficient for large trees.

<Image light={chartsHierarchicalRadialtree} maxWidth={400} position="center" cornerRadius="true" alt="Radial tree chart with a root node at the center and child nodes radiating outward in concentric rings." />

## When to use
- **Directory visualizers**: Showing many levels of folders in a compact circular form.
- **Genomic maps**: Visualizing relationships between many biological entities.
- **Network topology**: Mapping devices in a network radiating from a central hub.

## Code example

### XAML
```xml
<RadialTreeChart xmlns="https://github.com/avaloniaui" Name="RadialTreeChartSample" Title="Taxonomy" Height="400"
                          ItemsSource="{Binding RadialTreeData}"
                          LabelPath="Name"
                          ChildrenPath="Children" />
```

### Data model (C#)
```csharp
public class TreeNode
{
    public string Name { get; set; } = string.Empty;
    public ObservableCollection<TreeNode> Children { get; set; } = new();
}

public ObservableCollection<TreeNode> RadialTreeData { get; } = new()
{
    new TreeNode { Name = "Animals", Children = {
        new TreeNode { Name = "Mammals", Children = {
            new TreeNode { Name = "Dog" },
            new TreeNode { Name = "Cat" }
        }},
        new TreeNode { Name = "Birds", Children = {
            new TreeNode { Name = "Eagle" }
        }},
        new TreeNode { Name = "Fish", Children = {
            new TreeNode { Name = "Salmon" }
        }}
    }}
};
```

## Common properties

| Property | Description | Default |
| :--- | :--- | :--- |
| `ItemsSource` | The central root nodes. | `null` |
| `ValuePath` | Path to the value associated with each node. | `null` |
| `LabelPath` | Path to the text labels on the nodes. | `null` |
| `ChildrenPath` | Path to the collection of outer nodes. | `null` |
| `NodeSize` | Radius used to draw the node points. | `8.0` |
| `LevelSpacing` | Preferred spacing between radial levels. | `60.0` |
