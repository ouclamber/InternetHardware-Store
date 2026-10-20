const { override, addBabelPlugin } = require('customize-cra');

module.exports = override(
    addBabelPlugin([
        'transform-remove-console',
        { exclude: ['error', 'warn'] }   // console.error и console.warn остаются
    ])
);