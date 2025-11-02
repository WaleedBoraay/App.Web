function emitWarning() {
    if (!emitWarning.warned) {
      emitWarning.warned = true;
      console.log(
        'Deprecation (warning): Using file extension in specifier is deprecated, use "highlight.js/lib/languages/csharp" instead of "highlight.js/lib/languages/csharp.js"'
      );
    }
  }
  emitWarning();
    import lang from '~/Modern/csharp.js';
    export default lang;