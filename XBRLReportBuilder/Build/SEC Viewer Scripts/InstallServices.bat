@ECHO Off

cd Dispatcher
C:\WINDOWS\Microsoft.NET\Framework\v4.0.30319\installutil Aucent.FilingServices.Dispatcher.exe

cd ..\PreviewBuilder
C:\WINDOWS\Microsoft.NET\Framework\v4.0.30319\installutil Aucent.XBRLReportBuilder.BuilderService.exe

cd ..\SECBuilder
C:\WINDOWS\Microsoft.NET\Framework\v4.0.30319\installutil Aucent.XBRLReportBuilder.BuilderService.exe

cd ..