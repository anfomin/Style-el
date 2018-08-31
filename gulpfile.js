'use strict';

var gulp = require('gulp');
var plumber = require('gulp-plumber');
var less = require('gulp-less');
var csso = require('gulp-csso');

gulp.task('css', function () {
	return gulp.src('Styles/index.less')
		.pipe(plumber())
		.pipe(less())
		.pipe(csso())
		.pipe(gulp.dest('wwwroot'));
});

gulp.task('watch', function() {
	return gulp.watch('Styles/**/*.less', ['css']);
});

gulp.task('default', ['css']);