# Integration-Scalability – Environment Setup Guide

This project is a distributed architecture demo consisting of multiple independent .NET services, each with its own MariaDB instance, orchestrated via Docker Compose.

This guide explains how to set up the development environment and run the system locally.

---

## Prerequisites

Make sure you have the following installed:

- **.NET 10 SDK** (or your target version)
- **Docker Desktop** (with Docker Compose v2)
- **Git**

Optional (for DB inspection):

- **DBeaver**, **TablePlus**, or **MySQL CLI**

---

## Project Structure

```
/Marketplace
/Inventory
/Billing
/Search
docker-compose.yml
```

Each service:

- Has its own Dockerfile
- Connects to its own MariaDB container (defined in docker-compose)
- Exposes its API via `http://localhost:<port>`

---

## 1. Clone the Repository

```sh
git clone https://github.com/MortalLizard/Integration-scalability
cd Integration-scalability
```

---

## 2. Build and Start the Environment

From the root folder:

```sh
docker compose up --build
```

This will:

- Build all .NET service images  
- Spin up the matching MariaDB containers  
- Initialize databases with environment variables defined in the compose file

---

## 3. Verify Containers Are Running

```sh
docker ps
```

You should see something like:

- `marketplace`
- `inventory`
- `billing`
- `search`
- `marketplace-db`
- `billing-db`

---

## 4. Accessing the APIs

Each service exposes an HTTP endpoint.  
Typical examples (depending on your configuration):

| Service       | URL                                       |
|---------------|-------------------------------------------|
| Marketplace   | http://localhost:5001 |
| Inventory     | http://localhost:5002 |
| Billing       | http://localhost:5003 |
| Search        | http://localhost:5004 |

You can test them with:

```sh
curl http://localhost:5001/health
```

---

## 5. Connecting to the Databases

Example for Marketplace:

- **Host:** `localhost`
- **Port:** `3307`
- **User:** `root`
- **Password:** `marketplace_root`
- **DB name:** `marketplace`

---

## 6. Stopping the Environment

```sh
docker compose down
```

To also remove database volumes:

```sh
docker compose down -v
```

---

## Next Steps (future additions)

Later the README can be extended with:

- RabbitMQ setup in `docker-compose.yml`
- Messaging between services  
- Instructions for migrations / seeding  
- Load testing setup  
- Horizontal scaling examples  

