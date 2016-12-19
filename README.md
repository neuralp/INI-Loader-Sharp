# INI-Loader-Sharp
SIMPL# Library for reading and writing to INI syntax style files written by Nicholas Pepper

This module set is BSD licensed (see license.txt) so please attribute me on modifications and even better, make submissions to improve this module set.

This module will load an INI style file and process the input through the other modules. The other modules can hadle digitals, analogs and strings (each has their own module). Section and key are automatically passed to the receiver modules through the common memory in S#.  Changes to the items are passed back to the loader for saving upon request.  The default file name is set to 'system.conf' and is located within the specific running programs individual directory.

This program is meant to have one **loader** module and as many of the digital, analog or serial modules as needed in the individual locations within a program.  In my programs I spread them all over in folders and make it easy for search and replace when duplicating.

### Compilation and Setup

Hopefully you are somewhat proficient with Crestron software and systems because a lot of the steps here are manual and require knowledge of such systems to run properly.

1. Copy all files from the SIMPL+ folder into your user modules directory
2. Open each file and compile, make sure the .clz is in there
3. Open up the test file, compile and transfer to your processor.
 * You may need to replace the DMPS3-300 with whatever you are using.
4. Load the **system.conf** to the directory **&#92;&#92;NVRAM&#92;INILOADER&#92;system.conf**
5. Connect to the console and send `ucmd "start"` to test.

#### Find Me

Find me in the Reddit groups or Facebook group or on Labs if you have questions
