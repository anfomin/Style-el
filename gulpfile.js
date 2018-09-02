'use strict';

var gulp = require('gulp');
var plumber = require('gulp-plumber');
var less = require('gulp-less');
var csso = require('gulp-csso');
var gzip = require('gulp-gzip');

gulp.task('css', function () {
	return gulp.src('Styles/index.less')
		.pipe(plumber())
		.pipe(less())
		.pipe(csso())
		.pipe(gulp.dest('wwwroot'));
});

gulp.task('gzip', function () {
	return gulp.src('wwwroot/*.css')
		.pipe(gzip())
		.pipe(gulp.dest('wwwroot'));
});

gulp.task('watch', function() {
	return gulp.watch('Styles/**/*.less', gulp.series('css', 'gzip'));
});

gulp.task('default', gulp.series('css', 'gzip'));