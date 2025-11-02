var hljs = require('~/Modern/core');

hljs.registerLanguage('xml', require('~/Modern/languages/xml'));
hljs.registerLanguage('bash', require('~/Modern/languages/bash'));
hljs.registerLanguage('c', require('~/Modern/languages/c'));
hljs.registerLanguage('cpp', require('~/Modern/languages/cpp'));
hljs.registerLanguage('csharp', require('~/Modern/languages/csharp'));
hljs.registerLanguage('css', require('~/Modern/languages/css'));
hljs.registerLanguage('markdown', require('~/Modern/languages/markdown'));
hljs.registerLanguage('diff', require('~/Modern/languages/diff'));
hljs.registerLanguage('ruby', require('~/Modern/languages/ruby'));
hljs.registerLanguage('go', require('~/Modern/languages/go'));
hljs.registerLanguage('graphql', require('~/Modern/languages/graphql'));
hljs.registerLanguage('ini', require('~/Modern/languages/ini'));
hljs.registerLanguage('java', require('~/Modern/languages/java'));
hljs.registerLanguage('javascript', require('~/Modern/languages/javascript'));
hljs.registerLanguage('json', require('~/Modern/languages/json'));
hljs.registerLanguage('kotlin', require('~/Modern/languages/kotlin'));
hljs.registerLanguage('less', require('~/Modern/languages/less'));
hljs.registerLanguage('lua', require('~/Modern/languages/lua'));
hljs.registerLanguage('makefile', require('~/Modern/languages/makefile'));
hljs.registerLanguage('perl', require('~/Modern/languages/perl'));
hljs.registerLanguage('objectivec', require('~/Modern/languages/objectivec'));
hljs.registerLanguage('php', require('~/Modern/languages/php'));
hljs.registerLanguage('php-template', require('~/Modern/languages/php-template'));
hljs.registerLanguage('plaintext', require('~/Modern/languages/plaintext'));
hljs.registerLanguage('python', require('~/Modern/languages/python'));
hljs.registerLanguage('python-repl', require('~/Modern/languages/python-repl'));
hljs.registerLanguage('r', require('~/Modern/languages/r'));
hljs.registerLanguage('rust', require('~/Modern/languages/rust'));
hljs.registerLanguage('scss', require('~/Modern/languages/scss'));
hljs.registerLanguage('shell', require('~/Modern/languages/shell'));
hljs.registerLanguage('sql', require('~/Modern/languages/sql'));
hljs.registerLanguage('swift', require('~/Modern/languages/swift'));
hljs.registerLanguage('yaml', require('~/Modern/languages/yaml'));
hljs.registerLanguage('typescript', require('~/Modern/languages/typescript'));
hljs.registerLanguage('vbnet', require('~/Modern/languages/vbnet'));
hljs.registerLanguage('wasm', require('~/Modern/languages/wasm'));

hljs.HighlightJS = hljs
hljs.default = hljs
module.exports = hljs;