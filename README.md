## Prerequisites

Make sure you have the following installed:

- **.NET 10 SDK**
- **Docker Desktop** (with Docker Compose)
- **Git**


## Project Structure

```
/Marketplace
/Inventory
/Billing
/Search
compose.yml
```

Each service:

- Has its own Dockerfile
- Connects to its own MariaDB container (defined in compose)
-   **Except Search, it runs with noSQL mongoDB**
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
docker compose up -d
```

This will:

- Build all .NET service images  
- Spin up the matching DB containers
- Check dockerfiles for root access credentials

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
| Marketplace   | http://localhost:8080 |
| Inventory     | http://localhost:8081 |
| Billing       | http://localhost:8082 |
| Search        | http://localhost:8083 |




## 5. Stopping the Environment

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

