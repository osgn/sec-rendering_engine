rm -f ../../BuildLog.txt

echo '' >> ../../BuildLog.txt
echo '' >> ../../BuildLog.txt
echo '' >> ../../BuildLog.txt
echo '********Building Common********' >> ../../BuildLog.txt
xbuild ../../Common/Common.sln 1>> ../../BuildLog.txt 2>&1

echo '' >> ../../BuildLog.txt
echo '' >> ../../BuildLog.txt
echo '' >> ../../BuildLog.txt
echo '********Building NXBRE********' >> ../../BuildLog.txt
xbuild ../../XBRLReportBuilder/NxBRE/NxBRE.sln 1>> ../../BuildLog.txt 2>&1

echo '' >> ../../BuildLog.txt
echo '' >> ../../BuildLog.txt
echo '' >> ../../BuildLog.txt
echo '********Building FilingServices********' >> ../../BuildLog.txt
xbuild ../../XBRLReportBuilder/FilingServices/FilingServices.sln 1>> ../../BuildLog.txt 2>&1

echo '' >> ../../BuildLog.txt
echo '' >> ../../BuildLog.txt
echo '' >> ../../BuildLog.txt
echo '********Building XBRLReportBuilder********' >> ../../BuildLog.txt
xbuild ../../XBRLReportBuilder/XBRLReportBuilder/XBRLReportBuilder.sln 1>> ../../BuildLog.txt 2>&1

less ../../BuildLog.txt