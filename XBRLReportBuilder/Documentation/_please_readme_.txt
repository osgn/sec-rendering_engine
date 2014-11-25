Overview

This is release 2.4.0.8.2014 of the Personal Renderer distribution archive, which contains the Rendering Engine, or simply "the Renderer". To use this release you should be familiar with using Command Prompts and zip files.

The personal renderer must be invoked from a Command Prompt (or a .bat file); it will not operate by simply double clicking on the ReportBuilderRenderer.exe.

The renderer produces HTML and other files identical to the corresponding version of the rendering engine as installed at the SEC's EDGAR web site and Interactive Data previewer (https://datapreview.sec.gov/previewer/). Therefore, you can use it on your own machine to fine-tune your rendered output, without relying on the SEC interactive data previewer.

The files produced include:

	Rn.htm, where n is a sequence number (e.g. R1.htm). These are HTML files for the individual reports. 

	FilingSummary.xml. This contains information about the filing, including a list of report names and the corresponding R-file name. It also includes a section of Log messages (see Examples below). 

	Personal renderers with version numbers 2.4.* do not produce the “menu” of report names and groupings as seen in the Previewer.

Note that the renderer is not a validator and does not test for EDGAR Filer Manual, nor for XBRL 2.1 Validity. Input files that are not valid with respect to the EDGAR Filer Manual may fail or produce poor results. It is highly recommended that before any "live" submission that you validate your XBRL files. You can validate your XBRL files here: (https://www.edgarfiling.sec.gov).


Instructions

This section gives step by step instructions on how to install and run the Renderer. Since it is impossible to encompass all scenarios we have made the following assumptions in our example:

	The folder into which you install the renderer is "C:\Program Files\Renderer" 
	
	The folder containing the XBRL instance and taxonomy files which you wish to renderer is "C:\myXBRL" 

	The instance file which you wish to render is named "my-20111231.xml" 

Download and Install (Do once):

	Right-click on the renderer.zip file from the SEC web site and save a copy to your Desktop or other folder. 

		It is not recommended to simply open the zip file from the web site. 

	Install the renderer by extracting the contents of the zip file to "C:\Program Files". 

		To extract the contents using Windows Explorer, right-click on the zip file, and perform the "Extract All" operation. 

		Make a note of the destination folder name as it will be used in commands in the Command Prompt. 

		It is never sufficient to copy the zip file and simply open it from Windows Explorer; you must extract the contents to the desired location. 

	Verify that the extraction created a subfolder called "c:\Program Files\Renderer" that contains, among other files: 

		A copy of this readme file. 

		One main executable program called ReportBuilderRenderer.exe 

		A folder called "Examples". 


Render an XBRL File:

From the start menu, Run a Command Prompt (Start > Run > cmd.exe).

Execute the following commands:


	c: 

	cd "\Program Files\Renderer" 

	ReportBuilderRenderer /Instance="c:\myXBRL\my-20111231.xml" 

OR

	c: 

	cd \myXBRL 

	"c:\Program Files\Renderer\ReportBuilderRenderer" /Instance="my-20111231.xml" 

Hint: In the command prompt, type the first few characters of a long name followed by the "tab" key to complete the rest.

After running either set of the commands above, a new folder with the results will be created at the following location: "C:\myXBRL\Reports". You may view the files in a browser by the following Command Prompt commands (for example the R1.htm file):

	c: 

	cd \myXBRL\Reports 

	R1.htm 

Alternatively you can launch a browser and open the file, or double click on the filename in Windows Explorer.


Command Line Details

You use the renderer interactively at a command prompt. If you run the program without any arguments:



      ReportBuilderRenderer.exe
      
You will see the following message with details of other ways to use the program:



      The syntax of this command is:

      

      ReportBuilderRenderer.exe /Instance="[drive:][path]instance.xml" [/Instance="[drive:][path]instance.xml"] [...]

      - or -

      ReportBuilderRenderer.exe /Instance="[drive:][path]package.zip" [/Instance="[drive:][path]package.zip"] [...]

      

      Optional parameters:

      

      Set the base output path:

      /ReportsFolder=[drive:][path\to\]folder

      

      Set the report output format(s):

      /ReportFormat=(Xml|Html|HtmlAndXml)

      

      If format contains Html, set the html format:

      /HtmlReportFormat=(Complete|Fragment)

      

      Set the caching policy of remote files:

      Value descriptions can be found at 'http://msdn.microsoft.com/en-us/library/system.net.cache.request

      cachelevel.aspx'

      /RemoteFileCachePolicy=(Default|BypassCache|CacheOnly|CacheIfAvailable|Revalidate|Reload|NoC

      acheNoStore)

      

      Set quiet mode, the application will not interact with the user:

      /Quiet

      

      Set the output format for all filings in this session:

      /SaveAs=(Xml|Zip)Example: assuming that the XBRL instance you want to render is in folder C:\myXBRL and the filename is my-20111231.xml. In the Command Prompt (with the Renderer as the current folder) the command



      ReportBuilderRenderer /Instance="C:\myXBRL\my-20111231.xml"will create a C:\myXBRL\Reports folder with the rendered result.



Examples

The folder "Examples" contains ten examples, each in its own sub folder. It is recommended that you run/view these files before processing your own XBRL files in the renderer in order to become comfortable with the renderer and to view the new features of the rendering engine. Additionally they illustrate some of the new features of this new release including the information, warning and error messages.

There is a ".bat" file for each folder that illustrates how the renderer is invoked from the command line prompt. To view these examples do the following:

	Double-click on each .bat file in Windows Explorer and the results will be generated in a sub-folder called "Reports" (such as edgar2012.bat to create the results in edgar2012\Reports). 

or

	From the Start menu, Run a Command Prompt (cmd.exe). 

	cd to the Renderer\Examples subfolder (such as c: followed by cd "\Program Files\Renderer\Examples"). 

	Then type the name of each .bat file at the command prompt and press Enter. This will run the Renderer and create the results in a subfolder called "Reports". The .bat files also contain a command to view the result. The .bat files are small enough so you can "type" them to see their content. 

Note that if there are already files in the destination "Reports" folder, the renderer will ask you whether you want to overwrite them. Answer "y" to the prompt. To run without such interruptions, delete the "Reports" folder before rendering.

The examples should help you become comfortable with the renderer. For example:

	edgar2012 - a small filing that accesses all the 2012 us-gaap and dei taxonomy files. Output file R1.htm contains all there is in the filing. 

	zip - example using the renderer to process, and produce, a zip file. This filing has a bit more data and comes out as three reports R1.htm, R2.htm and R3.htm. Look at the FilingSummary.xml file to see the correspondence of report names with file names. 

The examples also illustrate new features of the rendering engine: 

flowthru - other information such as removal of columns due to flow-through suppression. This is found toward the bottom of the FilingSummary.xml file, in the section starting with <Logs>. The content of <Logs> is not displayed to end users on the EDGAR Viewer. 

	graphic - how to use graphics and reference them from text blocks 

	horizontal - shows how the order of equity statement columns can be controlled by presentation links. 

	qlabel- Some elements have values that are XML QNames, and those QNames can now have labels. 

	restated - Illustrates the use the 2011 us-gaap taxonomy for adjustments and restated values. 

Finally, the examples illustrate some of the information, warning and error messages that may appear in the <Logs> portion of output file FilingSummary.xml, but not displayed to end users: 

	dropzero - information about how scaling was applied to "drop zeroes" in reports. 

	embedding - warnings about badly formed embedding commands in summary prospectuses. 

	vertical - illustrates that equity statement layout will fail unless there are start and end labels. 


More Details

Instead of passing many arguments on the command line, edit the file

        ReportBuilderRenderer.exe.config
        
to set the argument defaults. Note that the config file MUST be in the same folder as where you extracted the renderer.

The file TaxonomyAddonManager.xml and the folder Taxonomy do not contain a full copy of all EDGAR taxonomies, only their "documentation" and "reference" files; these files are referenced by the pop-ups in the output HTML.

The "Xml" format of the output appears on the EDGAR web site for older filings but has been phased out in favor of the HTML output produced by the renderer.

The source code of the renderer is available from the same web site where this was posted.

For additional information contact EDGAR Filer Support by phone or Contact RiskFin form.


Troubleshooting Hints

The message "Error Parsing the Taxonomy" often means that the renderer is unable to get a file it needs from the Internet. There may be nothing wrong with your filing. Try typing the URL of the file it cannot find into your browser's address bar. If, for example, you cannot reach

	http://www.xbrl.org/dtr/type/numeric-2009-12-16.xsd 

from your browser, then the renderer cannot reach it either.

Many rendering issues can arise because the input files are not compliant with the EDGAR Filer Manual. If you run into rendering problems and have trouble interpreting the Log messages, try validating the input first and correcting any errors before trying again.

The date format of the output and thousands separators are determined by Windows locale settings on the machine where the renderer runs. The SEC previewer and EDGAR Viewer use the US locale. Your machine must be set to the US locale to produce correct results.

Avoid including images among the input having names such as "barchart1.jpg", "BarChart2.jpg" or variations thereof.

If you notice other discrepancies between your renderer and the SEC previewer, be sure to check that the version numbers agree. The version number appears at the top of every FilingSummary.xml file. You can also see the version of your renderer without needing to run a test file, by right-clicking on ReportBuilderRenderer.exe and looking for the details tab. There is no personal renderer for versions prior to 2.3.0.11.
