function emitWarning() {
    if (!emitWarning.warned) {
      emitWarning.warned = true;
      console.log(
        'Deprecation (warning): Using file extension in specifier is deprecated, use "highlight.js/lib/languages/axapta" instead of "highlight.js/lib/languages/axapta.js"'
      );
    }
  }
  emitWarning();
    import lang from '~/Modern/axapta.js';
    export default lang;