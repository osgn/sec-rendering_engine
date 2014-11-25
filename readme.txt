This is SEC Rendering Engine Source for version 2.4.0.8.2014

PREREQUISITES

Microsoft Visual Studio 2010 OR Visual Studio Express 2010
Nunit-Net (http://www.nunit.org/?p=download)
Familiarity with renderingengineconfigurablebinary.zip installation and use

ADVICE

Bind drive letter S to this folder.
That is, S:\readme.txt should show you this file.

Try using S:\XBRLReportBuilder\Build\XBRLReportBuilder.bat 
to build the XBRLReportBuilder solution.

If you need to build the individual solutions, this is the build order:

S:\Common\Common.sln
S:\XBRLReportBuilder\NxBRE\NxBRE.sln
S:\XBRLReportBuilder\FilingServices\FilingServices.sln
S:\XBRLReportBuilder\XBRLReportBuilder\XBRLReportBuilder.sln


The folder S:\bin contains all build outputs; unlike
renderingeengineconfigurablebinary.zip, the personal
renderer, autotester, dispatcher, and rendering service
are all in the same folder.

Also, to prevent accidental overwrites, all *.config files for each 
application/service have been removed from the S:\bin folder and placed
into the S:\XBRLReportBuilder\Build\DefaultConfigs folder. If this is a new
installation, then copy the *.config files from the 
S:\XBRLReportBuilder\Build\DefaultConfigs folder back to the S:\bin folder. 

DOCUMENTATION

In the S:\XBRLReportBuilder\Documentation folder.
