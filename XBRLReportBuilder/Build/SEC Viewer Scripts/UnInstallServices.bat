@ECHO Off

cd PreviewBuilder
C:\WINDOWS\Microsoft.NET\Framework\v4.0.30319\installutil /u Aucent.XBRLReportBuilder.BuilderService.exe

cd ..\SECBuilder
C:\WINDOWS\Microsoft.NET\Framework\v4.0.30319\installutil /u Aucent.XBRLReportBuilder.BuilderService.exe

cd ..\Dispatcher
C:\WINDOWS\Microsoft.NET\Framework\v4.0.30319\installutil /u Aucent.FilingServices.Dispatcher.exe

cd ..