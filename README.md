# gardaconstruction.com (media content)

This repo contains the graphics, video, pdf, and other content stored in blob storage that has been separated out from the web site deployment.  This was done primarily because of video content because it was causing issues with deployments failing and time it takes to move around all that data.  

## DEPLOYING RELEASES
This repo is part of a larger project for the Garda Construction website.  The repos involved are:
- [Media Content](https://github.com/trevorfritz/garda-construction-graphics) <== this project
- [Website Code](https://github.com/trevorfritz/garda-construction-com)

**To Release** this project create a branch off main named "Release YYYY-MM-DD v1.x.x".  Switch to the branch and use uploader tool to upload data into the environment.


## Media Folder
The .\web\media\ folder contains all the content that is to be uploaded into blob storage

## Tools Folder
The tools folder contains the source code and binary files to upload the blobs into blob storage.  Run the upload.exe command-line tool to see help on how to specify and run an upload session.

## Workspace Folder
The workspace folder contains original images, videos, and other content used to edit and finally produce content to the Media folder.  It may also contain other graphics used for Garda Construction marketing, signs, etc.
