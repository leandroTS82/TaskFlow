#!/bin/bash
set -e

MONGO_AUTH="-u root -p password --authenticationDatabase admin"

echo "Waiting for MongoDB to accept connections..."
until mongosh $MONGO_AUTH --quiet --eval "db.adminCommand('ping').ok" 2>/dev/null | grep -q 1; do
  echo "  ...not ready yet, retrying in 3s"
  sleep 3
done
echo "MongoDB is up."

ALREADY=$(mongosh $MONGO_AUTH --quiet --eval "rs.status().ok" 2>/dev/null || echo "0")

if [ "$ALREADY" = "1" ]; then
  echo "Replica set already initialized, nothing to do."
else
  echo "Initiating replica set rs0..."
  mongosh $MONGO_AUTH --quiet --eval "
    rs.initiate({
      _id: 'rs0',
      members: [{ _id: 0, host: 'localhost:27017' }]
    })
  "
  echo "Replica set rs0 initialized."
fi