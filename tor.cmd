@echo off
pushd src\dotnet-tor\bin\Debug
tor %*
popd