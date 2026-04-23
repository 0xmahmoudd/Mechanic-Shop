#!/bin/bash
API_URL="http://localhost:5190/api"

echo "=== 1. Register Customer ==="
CUSTOMER_RES=$(curl -s -X POST "$API_URL/customers/register" -H "Content-Type: application/json" -d '{"firstName":"Alice","lastName":"Smith","username":"alicesmith","email":"alice@example.com","password":"password123","phoneNumber":"+123456"}')
echo $CUSTOMER_RES
CUSTOMER_ID=$(echo $CUSTOMER_RES | grep -o '"id":[0-9]*' | head -1 | cut -d: -f2)
echo "Customer ID: $CUSTOMER_ID"

echo -e "\n=== 2. Get Customer Profile ==="
curl -s -X GET "$API_URL/customers/me" -H "X-Customer-Id: $CUSTOMER_ID"

echo -e "\n\n=== 3. Add Vehicle ==="
VEHICLE_RES=$(curl -s -X POST "$API_URL/customers/me/vehicles" -H "X-Customer-Id: $CUSTOMER_ID" -H "Content-Type: application/json" -d '{"make":"Ford","model":"Mustang","year":2021,"licensePlate":"FAST123","vin":"1FDFP34567890"}')
echo $VEHICLE_RES
VEHICLE_ID=$(echo $VEHICLE_RES | grep -o '"id":[0-9]*' | head -1 | cut -d: -f2)
echo "Vehicle ID: $VEHICLE_ID"

echo -e "\n=== 4. Get Customer Vehicles ==="
curl -s -X GET "$API_URL/customers/me/vehicles" -H "X-Customer-Id: $CUSTOMER_ID"

echo -e "\n\n=== 5. Update Vehicle ==="
curl -s -X PUT "$API_URL/vehicles/$VEHICLE_ID" -H "X-Customer-Id: $CUSTOMER_ID" -H "Content-Type: application/json" -d '{"make":"Ford","model":"Mustang GT","year":2022,"licensePlate":"FASTER1","vin":"1FDFP34567890"}'

echo -e "\n\n=== 6. Employee: Create Employee (No direct endpoint, checking Employee work orders) ==="
# Since no register employee endpoint exists, let's assume Employee 1 exists or test with an arbitrary ID.
# Actually, the DB is empty so Employee 1 might not exist. But we can test the Work Orders endpoint.
echo "Employee ID 1 Work Orders:"
curl -s -X GET "$API_URL/employees/me/work-orders" -H "X-Employee-Id: 1"

echo -e "\n\n=== 7. Delete Vehicle ==="
curl -s -X DELETE "$API_URL/vehicles/$VEHICLE_ID" -H "X-Customer-Id: $CUSTOMER_ID"
echo "Vehicle Deleted."

echo -e "\n=== 8. Get Customer Vehicles (Should be empty) ==="
curl -s -X GET "$API_URL/customers/me/vehicles" -H "X-Customer-Id: $CUSTOMER_ID"
echo ""

