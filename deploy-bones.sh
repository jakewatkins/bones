#!/bin/sh
# Deploy bones to ~/apps/bones

dotnet publish -c Release -o .bin/Release/net9.0/publish
rm -rf ~/apps/bones
cp -r .bin/Release/net9.0/publish/ ~/apps/bones

echo "Deployed bones to ~/apps/bones"
