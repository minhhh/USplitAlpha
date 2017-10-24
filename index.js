var mkdirp = require('mkdirp');
var path = require('path');
var ncp = require('ncp');

// Don't run this if the original folder where we run `yarn install` is the same as the current folder
var topLvlPackage = path.relative("" + process.env.DIR, __dirname) == ""
if (topLvlPackage) {
    return;
}

var assetDir = 'Assets';
var project = require('./p.json');

for (var i = 0, l = project.assets.length; i < l; i++) {
    var file = project.assets[i];
    var src = path.join(__dirname, assetDir, file);
    var dir = path.join(__dirname, '..', '..', assetDir, file);
    var parentDir = path.dirname(dir);

    // Create folder if missing
    mkdirp(parentDir, function(_src, _dir, err) {
        if (err) {
            console.error(err)
            process.exit(1);
        }

        // Copy files
        ncp(_src, _dir, function(err) {
            if (err) {
                console.error(err);
                process.exit(1);
            }
        });
    }.bind(this, src, dir));
}
