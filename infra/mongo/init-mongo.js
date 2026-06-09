db = db.getSiblingDB('taskflow');

db.createCollection('jobs');
db.createCollection('outbox_messages');

db.jobs.createIndex({ idempotencyKey: 1 }, { unique: true });
db.jobs.createIndex({ status: 1, priority: -1, createdAt: 1 });
db.jobs.createIndex({ scheduledAt: 1, status: 1 });
db.outbox_messages.createIndex({ published: 1, createdAt: 1 });
