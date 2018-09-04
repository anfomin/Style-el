#!/bin/bash

rm -rf artifacts
yarn install
dotnet publish -c Release -o artifacts