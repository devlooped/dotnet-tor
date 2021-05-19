@echo off
pushd src\dotnet-tor\bin\Debug\net5.0
tor %*
popd