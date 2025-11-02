function emitWarning() {
    if (!emitWarning.warned) {
      emitWarning.warned = true;
      console.log(
        'Deprecation (warning): Using file extension in specifier is deprecated, use "highlight.js/lib/languages/clojure-repl" instead of "highlight.js/lib/languages/clojure-repl.js"'
      );
    }
  }
  emitWarning();
    import lang from '~/Modern/clojure-repl.js';
    export default lang;