'use strict';

var gulp = require('gulp');
var plumber = require('gulp-plumber');
var merge2 = require('merge2');
var concat = require('gulp-concat');
var clean = require('gulp-clean');
var less = require('gulp-less');
var csso = require('gulp-csso');
var uglify = require('gulp-uglify');
var gzip = require('gulp-gzip');

var lightgallery = 'node_modules/lightgallery.js/dist';

gulp.task('clean', function() {
	return gulp.src('wwwroot/**/*.*', { read: false })
		.pipe(clean());
});

gulp.task('resource', function() {
	return merge2(
		gulp.src('Images/**').pipe(gulp.dest('wwwroot/img')),
		gulp.src(`${lightgallery}/img/**`).pipe(gulp.dest('wwwroot/img')),
		gulp.src(`${lightgallery}/fonts/**`).pipe(gulp.dest('wwwroot/fonts'))
	);
});

gulp.task('css', function () {
	var index = gulp.src('Styles/index.less')
		.pipe(plumber())
		.pipe(less());
	var libs = gulp.src(
			`${lightgallery}/css/lightgallery.css`,
			`${lightgallery}/css/lg-transitions.css`
		)
		.pipe(plumber());
	return merge2(index, libs)
		.pipe(concat('index.css'))
		.pipe(csso())
		.pipe(gulp.dest('wwwroot/css'))
		.pipe(gzip())
		.pipe(gulp.dest('wwwroot/css'));
});

gulp.task('js', function() {
	return gulp
		.src([
			`${lightgallery}/js/lightgallery.js`,
			'node_modules/lg-zoom.js/dist/lg-zoom.js',
			'Scripts/index.js'
		])
		.pipe(plumber())
		.pipe(concat('index.js'))
		.pipe(uglify())
		.pipe(gulp.dest('wwwroot'))
		.pipe(gzip())
		.pipe(gulp.dest('wwwroot'));
});

gulp.task('watch', function() {
	gulp.watch('Images/**', gulp.parallel('resource'));
	gulp.watch('Styles/**', gulp.parallel('css'));
	gulp.watch('Scripts/**', gulp.parallel('js'));
});

gulp.task('default', gulp.series('clean', gulp.parallel('resource', 'css', 'js')));