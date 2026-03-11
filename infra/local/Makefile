include settings.conf

ENV_FILE := settings.conf
DOCKER_COMPOSE_FILE := docker/docker-compose.yml

.PHONY: up down database database-interface web-server backend

# User commands
up: database database-interface web-server backend
	@echo "The local working stand has been successfully raised"

down:
	docker compose --env-file $(ENV_FILE) -f $(DOCKER_COMPOSE_FILE) down
	@echo "The local work stand has been successfully dismantled"

restart: down | up

# Docker-containers
database:
	docker compose --env-file $(ENV_FILE) -f $(DOCKER_COMPOSE_FILE) up -d database --wait

database-interface: database
	docker compose --env-file $(ENV_FILE) -f $(DOCKER_COMPOSE_FILE) up -d database-interface --wait

web-server:
	docker compose --env-file $(ENV_FILE) -f $(DOCKER_COMPOSE_FILE) up -d web-server --wait

backend: database
	docker compose --env-file $(ENV_FILE) -f $(DOCKER_COMPOSE_FILE) up -d backend --wait