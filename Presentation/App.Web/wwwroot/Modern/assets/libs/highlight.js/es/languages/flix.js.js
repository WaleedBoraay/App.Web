function emitWarning() {
    if (!emitWarning.warned) {
      emitWarning.warned = true;
      console.log(
        'Deprecation (warning): Using file extension in specifier is deprecated, use "highlight.js/lib/languages/flix" instead of "highlight.js/lib/languages/flix.js"'
      );
    }
  }
  emitWarning();
    import lang from '~/Modern/flix.js';
    export default lang;