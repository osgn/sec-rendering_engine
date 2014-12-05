#!/bin/sh

xbuild /p:Configuration=Release Lucene.Net/Lucene.Net-2.1.0-VS2005.csproj
xbuild /p:Configuration=Release ZedGraph/ZedGraph.csproj

xbuild /p:Configuration=Release Resources/Resources.csproj
xbuild /p:Configuration=Release Exceptions/Exceptions.csproj
xbuild /p:Configuration=Release Utilities/Utilities.csproj
xbuild /p:Configuration=Release Data/Data.csproj
xbuild /p:Configuration=Release XBRLParser_Reader/XBRLParser_Reader.csproj
xbuild /p:Configuration=Release ZipCompressDecompress/ZipCompressDecompress.csproj
# xbuild /p:Configuration=Release XBRLParser_Writer/XBRLParser_Writer.csproj
