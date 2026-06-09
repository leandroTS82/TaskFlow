# Docker

## Requirements

- Docker Desktop installed and running

## Start

```bash
docker-compose up -d
```

## Stop and remove

```bash
docker-compose down -v
```

## Known issue — Replica Set not initialized

In some environments the `taskflow-mongo-init` container may fail to initialize
the Replica Set automatically. Symptom: the API returns `MongoNotPrimaryException`.

To fix manually run:

```bash
docker exec -it taskflow-mongo mongosh -u root -p password --authenticationDatabase admin --eval "rs.initiate({ _id: 'rs0', members: [{ _id: 0, host: 'localhost:27017' }] })"
```

To confirm it is working:

```bash
docker exec -it taskflow-mongo mongosh -u root -p password --authenticationDatabase admin --eval "rs.status().myState"
```

Expected return: `1` (Primary).

## Logs

```bash
# MongoDB
docker logs taskflow-mongo --follow

# API
docker logs taskflow-api --follow

# Mongo Init
docker logs taskflow-mongo-init --follow
```