// Copyright (C) 2017 Alaa Masoud
// See the LICENSE file in the project root for more information.

var gulp = require('gulp');
var cssmin = require('gulp-cssmin');
var rename = require('gulp-rename');

gulp.task('default', function () {
    gulp.src('app.css')
        .pipe(cssmin())
        .pipe(rename({
            suffix: '.min'
        }))
        .pipe(gulp.dest('dist'));
});