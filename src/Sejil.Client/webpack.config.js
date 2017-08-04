// Copyright (C) 2017 Alaa Masoud
// See the LICENSE file in the project root for more information.

const webpack = require('webpack');
const UglifyJsPlugin = require('uglifyjs-webpack-plugin');
const Visualizer = require('webpack-visualizer-plugin');

const env = process.env.BUILD_ENV || 'development';

let config = {
    entry: {
        app: './src/index.tsx',
        vendor: env === 'development' ? './src/vendor.dev.js' : './src/vendor.dist.js'
    },
    output: {
        filename: "[name].js",
        path: __dirname + "/dist"
    },

    resolve: {
        extensions: ['.ts', '.tsx', '.js']
    },

    module: {
        rules: [{
            test: /\.tsx?$/,
            loader: 'awesome-typescript-loader?tsconfig=./tsconfig.json'
        }, {
            enforce: 'pre',
            test: /\.js$/,
            loader: 'source-map-loader'
        }, {
            test: /\.css$/,
            use: ['style-loader', 'css-loader']
        }]
    },

    plugins: [
        new webpack.optimize.CommonsChunkPlugin({
            name: 'vendor',
            minChunks: Infinity
        }),
        new webpack.DefinePlugin({
            'process.env': {
                NODE_ENV: JSON.stringify(env),
            },
        })
    ]
};

if (env === 'development') {
    config.devtool = 'source-map';
    config.plugins.push(new Visualizer());
} else if (env === 'production') {
    config.plugins.push(new UglifyJsPlugin({
        comments: false
    }));
}

module.exports = config;