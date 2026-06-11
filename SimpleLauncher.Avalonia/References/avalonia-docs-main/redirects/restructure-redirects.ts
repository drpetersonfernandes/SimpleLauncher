interface Redirect {
  to: string;
  from: string | string[];
}

function createRedirects(existingPath: string): string[] | undefined {
  if (existingPath.includes('/docs/')) {
    const redirects: string[] = [];

    // Concepts → docs/fundamentals
    const fundamentalsMap: Record<string, string> = {
      '/docs/fundamentals/avalonia-xaml': '/concepts/core-concepts/avalonia-xaml',
      '/docs/fundamentals/code-behind': '/concepts/core-concepts/code-behind',
      '/docs/fundamentals/main-window': '/concepts/core-concepts/main-window',
      '/docs/fundamentals/top-level': '/concepts/core-concepts/top-level',
      '/docs/fundamentals/ui-composition': '/concepts/core-concepts/ui-composition',
      '/docs/fundamentals/cross-platform-architecture': '/concepts/architecture/cross-platform-architecture',
      '/docs/fundamentals/the-mvvm-pattern': '/concepts/architecture/the-mvvm-pattern',
      '/docs/fundamentals/application-lifetimes': '/concepts/platform-concepts/application-lifetimes',
      '/docs/fundamentals/including-assets': '/concepts/ui-concepts/assets',
    };
    for (const [newPath, oldPath] of Object.entries(fundamentalsMap)) {
      if (existingPath === newPath) {
        redirects.push(oldPath);
      }
    }

    // Concepts data-binding → docs/data-binding
    const dataBindingFromConcepts: Record<string, string> = {
      '/docs/data-binding/introduction-to-data-binding': '/concepts/data-concepts/data-binding/introduction-to-data-binding',
      '/docs/data-binding/data-binding-syntax': '/concepts/data-concepts/data-binding/data-binding-syntax',
      '/docs/data-binding/compiled-bindings': '/concepts/data-concepts/data-binding/compiled-bindings',
      '/docs/data-binding/data-context': '/concepts/data-concepts/data-binding/data-context',
      '/docs/data-binding/markup-extensions': '/concepts/data-concepts/markup-extensions',
    };
    for (const [newPath, oldPath] of Object.entries(dataBindingFromConcepts)) {
      if (existingPath === newPath) {
        redirects.push(oldPath);
      }
    }

    // docs/data/* → docs/data-binding/*
    const dataBindingFromDocs: Record<string, string> = {
      '/docs/data-binding/binding-classes': '/docs/data/binding-classes',
      '/docs/data-binding/binding-from-code': '/docs/data/binding-from-code',
      '/docs/data-binding/binding-to-controls': '/docs/data/binding-to-controls',
      '/docs/data-binding/how-to-bind-can-execute': '/docs/data/how-to-bind-can-execute',
      '/docs/data-binding/how-to-bind-image-files': '/docs/data/how-to-bind-image-files',
      '/docs/data-binding/how-to-bind-multiple-properties': '/docs/data/how-to-bind-multiple-properties',
      '/docs/data-binding/how-to-bind-tabs': '/docs/data/how-to-bind-tabs',
      '/docs/data-binding/how-to-bind-to-a-collection': '/docs/data/how-to-bind-to-a-collection',
      '/docs/data-binding/how-to-bind-to-a-task-result': '/docs/data/how-to-bind-to-a-task-result',
      '/docs/data-binding/how-to-bind-to-an-observable': '/docs/data/how-to-bind-to-an-observable',
      '/docs/data-binding/how-to-create-a-custom-data-binding-converter': '/docs/data/how-to-create-a-custom-data-binding-converter',
      '/docs/data-binding/inotifypropertychanged': '/docs/data/inotifypropertychanged',
    };
    for (const [newPath, oldPath] of Object.entries(dataBindingFromDocs)) {
      if (existingPath === newPath) {
        redirects.push(oldPath);
      }
    }

    // reference/data → docs/data-binding
    if (existingPath === '/docs/data-binding/built-in-data-binding-converters') {
      redirects.push('/reference/data/built-in-data-binding-converters');
    }

    // Concepts data-templates → docs/data-templates
    const dataTemplatesFromConcepts: Record<string, string> = {
      '/docs/data-templates/introduction-to-data-templates': '/concepts/data-concepts/data-templates/introduction-to-data-templates',
      '/docs/data-templates/control-content': '/concepts/data-concepts/data-templates/control-content',
      '/docs/data-templates/content-templates': '/concepts/data-concepts/data-templates/content-templates',
      '/docs/data-templates/data-template-collection': '/concepts/data-concepts/data-templates/data-template-collection',
    };
    for (const [newPath, oldPath] of Object.entries(dataTemplatesFromConcepts)) {
      if (existingPath === newPath) {
        redirects.push(oldPath);
      }
    }

    // docs/data/data-templates → docs/data-templates
    const dataTemplatesFromDocs: Record<string, string> = {
      '/docs/data-templates/creating-data-templates-in-code': '/docs/data/data-templates/creating-data-templates-in-code',
      '/docs/data-templates/reusing-data-templates': '/docs/data/data-templates/reusing-data-templates',
      '/docs/data-templates/view-locator': '/docs/data/data-templates/view-locator',
    };
    for (const [newPath, oldPath] of Object.entries(dataTemplatesFromDocs)) {
      if (existingPath === newPath) {
        redirects.push(oldPath);
      }
    }

    // Concepts styling → docs/styling
    const stylingFromConcepts: Record<string, string> = {
      '/docs/styling/styles': '/concepts/ui-concepts/styling/styles',
      '/docs/styling/style-classes': '/concepts/ui-concepts/styling/style-classes',
      '/docs/styling/control-themes': '/concepts/ui-concepts/styling/control-themes',
      '/docs/styling/container-queries': '/concepts/ui-concepts/styling/container-queries',
      '/docs/styling/themes': '/concepts/ui-concepts/styling/themes',
    };
    for (const [newPath, oldPath] of Object.entries(stylingFromConcepts)) {
      if (existingPath === newPath) {
        redirects.push(oldPath);
      }
    }

    // docs/ui-development/styling → docs/styling
    const stylingFromDocs: Record<string, string> = {
      '/docs/styling/sharing-styles': '/docs/ui-development/styling/sharing-styles',
      '/docs/styling/custom-fonts': '/docs/ui-development/styling/custom-fonts',
      '/docs/styling/theme-variants': '/docs/ui-development/styling/theme-variants',
    };
    for (const [newPath, oldPath] of Object.entries(stylingFromDocs)) {
      if (existingPath === newPath) {
        redirects.push(oldPath);
      }
    }

    // reference/styles → docs/styling
    const stylingFromRef: Record<string, string> = {
      '/docs/styling/style-selectors': '/reference/styles/style-selectors',
      '/docs/styling/style-selector-syntax': '/reference/styles/style-selector-syntax',
      '/docs/styling/property-setters': '/reference/styles/property-setters',
      '/docs/styling/pseudoclasses': '/reference/styles/pseudoclasses',
    };
    for (const [newPath, oldPath] of Object.entries(stylingFromRef)) {
      if (existingPath === newPath) {
        redirects.push(oldPath);
      }
    }

    // Layout
    if (existingPath === '/docs/layout/' || existingPath === '/docs/layout/layout') {
      redirects.push('/concepts/ui-concepts/layout');
    }
    if (existingPath === '/docs/layout/positioning-controls') {
      redirects.push('/reference/layout/positioning-controls');
    }

    // Input & interaction from concepts
    const inputFromConcepts: Record<string, string> = {
      '/docs/input-interaction/pointer': '/concepts/ui-concepts/user-input/pointer',
      '/docs/input-interaction/focus': '/concepts/ui-concepts/user-input/focus',
      '/docs/input-interaction/gestures': '/concepts/ui-concepts/user-input/gestures',
      '/docs/input-interaction/keyboard-and-hotkeys': '/concepts/ui-concepts/user-input/keyboard-and-hotkeys',
      '/docs/input-interaction/routed-events': '/concepts/ui-concepts/user-input/routed-events',
    };
    for (const [newPath, oldPath] of Object.entries(inputFromConcepts)) {
      if (existingPath === newPath) {
        redirects.push(oldPath);
      }
    }

    // Input from docs/ui-development
    if (existingPath === '/docs/input-interaction/adding-interactivity') {
      redirects.push('/docs/ui-development/adding-interactivity');
    }
    if (existingPath === '/docs/input-interaction/mouse-and-keyboard-shortcuts') {
      redirects.push('/docs/ui-development/mouse-and-keyboard-shortcuts');
    }

    // Gestures from reference
    const gesturesFromRef: Record<string, string> = {
      '/docs/input-interaction/gestures/pinch-gesture-recognizer': '/reference/gestures/pinch-gesture-recognizer',
      '/docs/input-interaction/gestures/pull-gesture-recognizer': '/reference/gestures/pull-gesture-recognizer',
      '/docs/input-interaction/gestures/scroll-gesture-recognizer': '/reference/gestures/scroll-gesture-recognizer',
    };
    for (const [newPath, oldPath] of Object.entries(gesturesFromRef)) {
      if (existingPath === newPath) {
        redirects.push(oldPath);
      }
    }

    // Graphics & animation from concepts
    if (existingPath === '/docs/graphics-animation/animations') {
      redirects.push('/concepts/ui-concepts/animations');
    }
    if (existingPath === '/docs/graphics-animation/image-interpolation') {
      redirects.push('/concepts/ui-concepts/image-interpolation');
    }

    // Graphics from reference
    if (existingPath === '/docs/graphics-animation/animation-settings') {
      redirects.push('/reference/animations-and-graphics/animation-settings');
    }
    if (existingPath === '/docs/graphics-animation/bitmap-blend-modes') {
      redirects.push('/reference/animations-and-graphics/bitmap-blend-modes');
    }

    // Graphics from docs/ui-development
    const graphicsFromDocs: Record<string, string> = {
      '/docs/graphics-animation/keyframe-animations': '/docs/ui-development/graphics/keyframe-animations',
      '/docs/graphics-animation/control-transitions': '/docs/ui-development/graphics/control-transitions',
      '/docs/graphics-animation/page-transitions': '/docs/ui-development/graphics/page-transitions',
      '/docs/graphics-animation/drawing-graphics': '/docs/ui-development/graphics/drawing-graphics',
      '/docs/graphics-animation/gradients': '/docs/ui-development/graphics/gradients',
      '/docs/graphics-animation/adding-icons': '/docs/ui-development/graphics/adding-icons',
    };
    for (const [newPath, oldPath] of Object.entries(graphicsFromDocs)) {
      if (existingPath === newPath) {
        redirects.push(oldPath);
      }
    }

    // Custom controls from concepts
    if (existingPath === '/docs/custom-controls/control-trees') {
      redirects.push('/concepts/ui-concepts/controls/control-trees');
    }

    // Custom controls from docs/ui-development
    const customControlsFromDocs: Record<string, string> = {
      '/docs/custom-controls/choosing-a-custom-control-type': '/docs/ui-development/custom-controls/choosing-a-custom-control-type',
      '/docs/custom-controls/custom-control-class': '/docs/ui-development/custom-controls/custom-control-class',
      '/docs/custom-controls/templated-controls': '/docs/ui-development/custom-controls/templated-controls',
      '/docs/custom-controls/drawing-custom-controls': '/docs/ui-development/custom-controls/drawing-custom-controls',
      '/docs/custom-controls/defining-properties': '/docs/ui-development/custom-controls/defining-properties',
      '/docs/custom-controls/defining-events': '/docs/ui-development/custom-controls/defining-events',
      '/docs/custom-controls/attached-properties': '/docs/ui-development/custom-controls/attached-properties',
      '/docs/custom-controls/custom-flyout': '/docs/ui-development/custom-controls/custom-flyout',
      '/docs/custom-controls/custom-itemspanel': '/docs/ui-development/custom-controls/custom-itemspanel',
      '/docs/custom-controls/custom-panel': '/docs/ui-development/custom-controls/custom-panel',
      '/docs/custom-controls/custom-control-library': '/docs/ui-development/custom-controls/custom-control-library',
    };
    for (const [newPath, oldPath] of Object.entries(customControlsFromDocs)) {
      if (existingPath === newPath) {
        redirects.push(oldPath);
      }
    }

    // Services from reference
    const servicesFromRef: Record<string, string> = {
      '/docs/services/clipboard': '/reference/services/clipboard',
      '/docs/services/focus-manager': '/reference/services/focus-manager',
      '/docs/services/input-pane': '/reference/services/input-pane',
      '/docs/services/insets-manager': '/reference/services/insets-manager',
      '/docs/services/launcher': '/reference/services/launcher',
      '/docs/services/platform-settings': '/reference/services/platform-settings',
      '/docs/services/activatable-lifetime': '/reference/services/activatable-lifetime',
      '/docs/services/storage/storage-provider': '/reference/services/storage/storage-provider',
      '/docs/services/storage/storage-item': '/reference/services/storage/storage-item',
      '/docs/services/storage/bookmarks': '/reference/services/storage/bookmarks',
      '/docs/services/storage/file-picker-options': '/reference/services/storage/file-picker-options',
    };
    for (const [newPath, oldPath] of Object.entries(servicesFromRef)) {
      if (existingPath === newPath) {
        redirects.push(oldPath);
      }
    }

    // File dialogs from concepts
    if (existingPath === '/docs/services/file-dialogs') {
      redirects.push('/concepts/ui-concepts/file-dialogs');
    }

    // App development moves
    if (existingPath === '/docs/app-development/rendering-markdown') {
      redirects.push('/docs/ui-development/rendering-markdown');
    }
    if (existingPath === '/docs/app-development/resource-dictionary') {
      redirects.push('/docs/ui-development/resource-dictionary');
    }
    if (existingPath === '/docs/app-development/xaml-preview-and-design-settings') {
      redirects.push('/docs/ui-development/xaml-preview-and-design-settings');
    }
    if (existingPath === '/docs/app-development/data-validation') {
      redirects.push('/docs/data/data-validation');
    }

    // Media from reference
    const mediaFromRef: Record<string, string> = {
      '/docs/media/mediaplayer': '/reference/media-player/mediaplayer',
      '/docs/media/mediasource': '/reference/media-player/mediasource',
    };
    for (const [newPath, oldPath] of Object.entries(mediaFromRef)) {
      if (existingPath === newPath) {
        redirects.push(oldPath);
      }
    }

    // Markdown from reference
    const markdownFromRef: Record<string, string> = {
      '/docs/markdown/codehighlighter': '/reference/markdown/codehighlighter',
      '/docs/markdown/imageloader': '/reference/markdown/imageloader',
      '/docs/markdown/markdown-styling': '/reference/markdown/markdown-styling',
    };
    for (const [newPath, oldPath] of Object.entries(markdownFromRef)) {
      if (existingPath === newPath) {
        redirects.push(oldPath);
      }
    }

    // Text from reference
    if (existingPath === '/docs/text/texttrimming') {
      redirects.push('/reference/text/texttrimming');
    }

    // WebView from reference
    const webviewFromRef: Record<string, string> = {
      '/docs/webview/webauthenticationbroker': '/reference/webview/webauthenticationbroker',
      '/docs/webview/webview-environment': '/reference/webview/webview-environment',
    };
    for (const [newPath, oldPath] of Object.entries(webviewFromRef)) {
      if (existingPath === newPath) {
        redirects.push(oldPath);
      }
    }

    // Samples & tutorials
    if (existingPath === '/docs/samples-tutorials') {
      redirects.push('/samples');
    }

    // Multi-touch merged into gestures
    if (existingPath === '/docs/input-interaction/gestures') {
      redirects.push('/docs/platform-specific-guides/multi-touch-events');
    }

    return redirects.length ? redirects : undefined;
  }

  // Tools redirects
  if (existingPath.includes('/tools/')) {
    const redirects: string[] = [];

    // Developer tools from docs → tools
    const devToolsMap: Record<string, string> = {
      '/tools/developer-tools/installation': '/docs/development-optimization/developer-tools/installation',
      '/tools/developer-tools/attaching-applications': '/docs/development-optimization/developer-tools/attaching-applications',
      '/tools/developer-tools/attaching-to-the-previewer': '/docs/development-optimization/developer-tools/attaching-to-the-previewer',
      '/tools/developer-tools/attaching-to-the-remote-tool': '/docs/development-optimization/developer-tools/attaching-to-the-remote-tool',
    };
    for (const [newPath, oldPath] of Object.entries(devToolsMap)) {
      if (existingPath === newPath) {
        redirects.push(oldPath);
      }
    }

    // Parcel from docs → tools
    const parcelMap: Record<string, string> = {
      '/tools/parcel/setup': '/docs/deployment/parcel/setup',
      '/tools/parcel/packaging-for-macos': '/docs/deployment/parcel/packaging-for-macos',
      '/tools/parcel/packaging-for-windows': '/docs/deployment/parcel/packaging-for-windows',
      '/tools/parcel/packaging-for-linux': '/docs/deployment/parcel/packaging-for-linux',
    };
    for (const [newPath, oldPath] of Object.entries(parcelMap)) {
      if (existingPath === newPath) {
        redirects.push(oldPath);
      }
    }

    // Avalonia Pro install
    if (existingPath === '/tools/installing-avalonia-pro') {
      redirects.push('/docs/development-optimization/installing-accelerate');
    }

    // Legacy dev tools
    if (existingPath === '/tools/legacy-developer-tools') {
      redirects.push('/docs/development-optimization/legacy-developer-tools');
    }

    return redirects.length ? redirects : undefined;
  }

  return undefined;
}

const redirects: Redirect[] = [
  // Concepts index → Guides welcome
  { from: '/concepts', to: '/docs/welcome' },
  { from: '/concepts/index', to: '/docs/welcome' },

  // Reference index → Guides welcome
  { from: '/reference', to: '/docs/welcome' },
  { from: '/reference/index', to: '/docs/welcome' },

  // Samples → Samples & Tutorials
  { from: '/samples', to: '/docs/samples-tutorials' },
  { from: '/samples/index', to: '/docs/samples-tutorials' },

  // Android VS Code debug guide → Tools
  { from: '/docs/platform-specific-guides/android/configure-vscode-debug-linux', to: '/tools/visual-studio-code/configure-vscode-debug-linux' },

  // GroupBox troubleshooting → control page (now a built-in control in Avalonia 12)
  { from: '/troubleshooting/controls/groupbox', to: '/controls/layout/containers/groupbox' },

  // WinForms → Windows platform integration
  { from: '/docs/platform-specific-guides/winforms', to: '/docs/platform-specific-guides/windows' },
];

export const restructure_redirects = {
  redirects,
  createRedirects,
};
