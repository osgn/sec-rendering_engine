#!/bin/sh

xbuild Lucene.Net/Lucene.Net-2.1.0-VS2005.csproj
xbuild ZedGraph/ZedGraph.csproj

xbuild Resources/Resources.csproj
xbuild Exceptions/Exceptions.csproj
xbuild Utilities/Utilities.csproj
xbuild Data/Data.csproj
xbuild XBRLParser_Reader/XBRLParser_Reader.csproj
xbuild ZipCompressDecompress/ZipCompressDecompress.csproj
# xbuild XBRLParser_Writer/XBRLParser_Writer.csproj
