#!/bin/bash

# Script để chạy migration cho AccountingERP

echo "Running Entity Framework Core migrations..."

cd ../src/AccountingERP.Web

# Development (SQLite)
echo "Creating migration for Development..."
dotnet ef migrations add InitialCreate --context AccountingDbContext --project ../AccountingERP.Infrastructure --startup-project .

echo "Updating database..."
dotnet ef database update --context AccountingDbContext --project ../AccountingERP.Infrastructure --startup-project .

echo "Done!"
