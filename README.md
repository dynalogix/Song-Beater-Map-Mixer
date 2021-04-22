# Song-Beater-Map-Mixer
Mix multiple Song Beater auto-generated maps with various settings into a single composite map

This makes it possible to have some variability within auto-generated songs, e.g. have sections R-L-R-L-R-L and also have other sections where you get R-R-L-L-R-R for example if you vary the "alternating left/right" value. But you can vary any of the other parameters, just make sure to "Generate" with the new settings and then "Save json files".

Pressing the "Merge" button will create a new subfolder called "merged" and place the composite level file (e.g. 4.json) and the updated sb.json file. Notes:

* if you have a single MP4 video in the song folder, it will also be added into the sb.json file
* if you have variants for multiple levels, merge will create all the composite levels (e.g. all 3-&ast;.json files into 3.json, 4-&ast;.json files into 4.json etc)
* merge will not overwrite existing level files in the "merged" subfolder. Delete them if you want to create a new one
* on the device the song.ogg, mp4, cover.jpg and json files all need to be in a single folder
* when updating a song on the device, you can just re-add the json files from the "merged" subfolder

![Screenshot](screenshot.jpg?raw=true "Screenshot") 

How to use:

1. Copy full path of Song Beater song folder (e.g. F:\Oculus\Song Beater\Quest\Star)
2. Keep this app open while working in Song Beater Editor. This app runs in the background and saved levels in that folder will be renamed to level variants (e.g. "4.json" → becomes → "4-225-R2R0L1L1R1R0L2L2R3R0L3L3R3R0L2L2R1R0L0L0.json" (showing the note count and first 20 notes in the file)
3. When you have at least 2 variants of the same level you can merge them into a composite level (where chuncks of orbs are taken randomly from the variants. Chunk size is random between min...max)
4. Note counts in main sb.json file are updated, and video file if found is also added to sb.json

Any questions, ask me on the Song Beater discord
