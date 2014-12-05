rm -f ../../BuildLog.txt

echo '' >> ../../BuildLog.txt
echo '' >> ../../BuildLog.txt
echo '' >> ../../BuildLog.txt
echo '********Building Common********' >> ../../BuildLog.txt
xbuild /p:Configuration=Release ../../Common/Common.sln 1>> ../../BuildLog.txt 2>&1

echo '' >> ../../BuildLog.txt
echo '' >> ../../BuildLog.txt
echo '' >> ../../BuildLog.txt
echo '********Building NXBRE********' >> ../../BuildLog.txt
xbuild /p:Configuration=Release ../../XBRLReportBuilder/NxBRE/NxBRE.sln 1>> ../../BuildLog.txt 2>&1

echo '' >> ../../BuildLog.txt
echo '' >> ../../BuildLog.txt
echo '' >> ../../BuildLog.txt
echo '********Building FilingServices********' >> ../../BuildLog.txt
xbuild /p:Configuration=Release ../../XBRLReportBuilder/FilingServices/FilingServices.sln 1>> ../../BuildLog.txt 2>&1

echo '' >> ../../BuildLog.txt
echo '' >> ../../BuildLog.txt
echo '' >> ../../BuildLog.txt
echo '********Building XBRLReportBuilder********' >> ../../BuildLog.txt
xbuild /p:Configuration=Release ../../XBRLReportBuilder/XBRLReportBuilder/XBRLReportBuilder.sln 1>> ../../BuildLog.txt 2>&1

less ../../BuildLog.txt