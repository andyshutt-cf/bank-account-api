#!/bin/bash

echo "================================"
echo "Building the project..."
echo "================================"
dotnet build BankAccountSolution.sln

if [ $? -ne 0 ]; then
    echo "Build failed!"
    exit 1
fi

echo ""
echo "================================"
echo "Running all tests..."
echo "================================"
dotnet test BankAccountSolution.sln --verbosity detailed --no-build

if [ $? -eq 0 ]; then
    echo ""
    echo "================================"
    echo "All tests passed!"
    echo "================================"
else
    echo ""
    echo "================================"
    echo "Some tests failed!"
    echo "================================"
    exit 1
fi
