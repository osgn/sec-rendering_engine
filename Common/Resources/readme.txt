Example command-line compilation and file placement for translated .resx file

1. Copy English .resx and rename for translation.
   For example, rename StringResources.resx to
   StringResources.de-DE.resx.

2. Translate the copied .resx, and at the command line,
   run: "resgen.exe StringResources.de-DE.resx"
   to create StringResources.de-DE.resources

3. At the command line, run:
   	"al.exe
		/t:lib
		/embed:StringResources.de-DE.resources,Resources.StringResources.de-DE.resources
		/culture:de-DE
		/keyfile:Q:\AnalyticalStudio\SNK\AucentPrivate.snk
		/v:1.0.1.1
		/out:Resources.resources.dll"

4. Place the file resulting from Step 3,
   Resources.resources.dll,
   into the de-DE subdirectory.