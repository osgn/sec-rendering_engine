This is a port of the SEC Rendering Engine Source for Linux/Unix under Mono,
based on the Windows/.NET SEC Rendering Engine Source for version 2.4.0.8.2014

https://www.sec.gov/spotlight/xbrl/renderingenginelicense.htm


PREREQUISITES

Mono (tested on 3.10.0)


ADVICE

Try using XBRLReportBuilder/Build/XBRLReportBuilder.sh
to build the XBRLReportBuilder solution.

If you need to build the individual solutions, this is the build order:

Common/Common.sln
XBRLReportBuilder/NxBRE/NxBRE.sln
XBRLReportBuilder/FilingServices/FilingServices.sln
XBRLReportBuilder/XBRLReportBuilder/XBRLReportBuilder.sln


After building, the folder 'bin' will contain all build outputs; unlike
renderingeengineconfigurablebinary.zip, the personal renderer, autotester, 
dispatcher, and rendering service are all in the same folder.

Also, all *.config files for each application/service have been placed into the 
XBRLReportBuilder/Build/DefaultConfigs folder. If this is a new installation, 
then copy the *.config files from the XBRLReportBuilder/Build/DefaultConfigs 
folder back to the 'bin' folder. 


KNOWN ISSUES

 - only the personal renderer has been ported/tested


ORIGINAL WINDOWS DOCUMENTATION

In the XBRLReportBuilder/Documentation folder.
