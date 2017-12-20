const tsImportPluginFactory = require('ts-import-plugin')
const {
    getLoader
} = require('react-app-rewired');
const rewireLess = require('react-app-rewire-less');
const Visualizer = require('webpack-visualizer-plugin');

module.exports = function override(config, env) {
    const tsLoader = getLoader(
        config.module.rules,
        rule =>
        rule.loader &&
        typeof rule.loader === 'string' &&
        rule.loader.includes('ts-loader')
    );

    tsLoader.options = {
        getCustomTransformers: () => ({
            before: [tsImportPluginFactory({
                libraryName: 'antd',
                libraryDirectory: 'es',
                style: true,
            })]
        })
    };

    config = rewireLess.withLoaderOptions({
        modifyVars: {
            '@font-family': 'Roboto, Helvetica Neue, Helvetica, Arial, sans-serif'
        },
    })(config, env);

    config.plugins = (config.plugins || []).concat([
        new Visualizer()
    ]);

    return config;
}